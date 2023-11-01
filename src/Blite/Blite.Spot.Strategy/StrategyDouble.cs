using Binance.Net.Enums;
using Binance.Net.Objects.Models.Spot;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.CommonObjects;
using System.Diagnostics;

namespace Blite.Spot.Strategy;

public class StrategyDouble : IAsyncDisposable {

    public const int LoopDelay = 2000;

    public StrategyDouble(Account account, StrategyDoubleParameters parameters, ISignal? signal = null) {
        _client = account.Client;

        Account = account;
        Parameters = parameters;
        Status = StrategyStatus.None;
        Signal = signal;
        ChainSteps = new(10);
        Statistics = new();
    }

    readonly BliteClient _client;

    CancellationTokenSource? _cancellationTokenSource;
    int _maxChainStep = 1000;
    BinanceSymbol? _symbolInfo;
    readonly AssetBalance _assetBalance = new();
    BinanceStreamBookPrice? lastBookTicker;
    DateTime lastBookTickerAt = DateTime.MinValue;

    #region Properties
    public Account Account { get; private set; }
    public StrategyDoubleParameters Parameters { get; private set; }
    public StrategyStatus Status { get; private set; }
    public ISignal? Signal { get; set; }
    public List<ChainStep> ChainSteps { get; private set; }
    public int Step { get; private set; } = 0;
    public StrategyStatistics Statistics { get; private set; }
    #endregion

    #region Events
    public event Action<ChainStep, ChainUpdateReason>? ChainUpdated;
    public event Action<ChainStep, decimal>? Profit;
    #endregion

    #region Methods
    public async void Run() {
        if (Signal == null) throw new Exception("Signal can't be null.");
        if (Status != StrategyStatus.None) throw new Exception("Strategy is already running.");
        Status = StrategyStatus.Running;

        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        Statistics.StartedAt = DateTime.UtcNow;

        /* asdasd
            ┌──────────────────┐
            │ buy              │ try = 1
            │ completed: false │ asset = 1
            └──────────────────┘
                    ↓  lose
            ┌──────────────────┐
            │ buy              │ try = 2
            │ completed: false │ asset = 2
            └──────────────────┘
                    ↓ win
            ┌──────────────────┐
            │ buy              │ try = 1
            │ completed: false │ asset = 1
            └──────────────────┘
        */

        /* A. init setup
              1. get information of symbol 
              2. subscribe events - bookTicker and userData
        */
        /* B. while loop
            1. loop handle
                a. break if cancellation requested
                b. prevent running if lastBookTicker older than 60s
            C. STEPS
                0. Create new chain with signal
                    a. Create new chain not include signal, try, asset
                    b. Does signal reach its lifespan? YES, clear it
                    c. Ask new signal
                    => At the end of step, we have a current chain with signal
                1. calculate try, asset for current chain for later steps can place order
                    a. if price changes lesser than Parameters.MIN_PRICE_CHANGE_PERCENT, sleep 60s
                    b. if unrealizedPNL > 0, reset nextTry to 1.
                    c. Asset = ((decimal)Math.Pow(2, nextTry - 1)) * Parameters.QUANTITY * assetFactor;
                    => agree with current price
                2. place order
                3.
                    => agree order FILLED
                4. Insert coint into _assetBalance
                5. calculate profit, 
        */

        // A. init setup
        #region 1. get information of symbol
        var exchangeInfoResponse = await _client.RawClient.SpotApi.ExchangeData.GetExchangeInfoAsync(new string[] { Parameters.SYMBOL });
        _symbolInfo = exchangeInfoResponse.Data.Result.Symbols.FirstOrDefault(o => o.Name == Parameters.SYMBOL);
        if (_symbolInfo == null) throw new Exception("Can't find information about '" + Parameters.SYMBOL + "'' trading pair.");
        exchangeInfoResponse = null;
        GC.Collect();

        #endregion
        #region 2. subscribe events
        using var bookTickeSubscriptor = await _client.SubscribeToBookTicker(Parameters.SYMBOL, OnBookTicker);
        using var userDataSubscriptor = await Account.SubscribeToUserDataAsync(OnTradeOrderUpdate);
        #endregion
        
        // B. while loop
        while (true) {
            #region 1. loop handle
            // --1a--
            if (cancellationToken.IsCancellationRequested) break;
            await Task.Delay(LoopDelay);

            // --1b--
            if (DateTime.UtcNow - lastBookTickerAt > TimeSpan.FromSeconds(60)) continue;
            #endregion

            var currentChain = GetCurrentChain();

            #region 0. Create new chain with signal

            // 0a. Create new chain not include try, asset
            if (currentChain == null) {
                var next_signal = this.GetSignalConditions();
                if (next_signal == null) continue;

                var newChain = new ChainStep { CreationTime = DateTime.UtcNow, Signal = next_signal, SignalTime = DateTime.UtcNow };
                ChainSteps.Add(newChain);
                currentChain = newChain;

                // to prevent memory leak, let chop our array
                if (ChainSteps.Count > _maxChainStep) ChainSteps.RemoveRange(0, ChainSteps.Count - _maxChainStep);

                // trigger onChainCreate
                OnChainUpdated(newChain, ChainUpdateReason.New);
                Step = 1;
                continue;
            }

            // 0b. Does signal reach its lifespan? 
            if (currentChain.Signal != null && currentChain.Order == null) {
                var timeSpanSinceLastSignal = DateTime.UtcNow - currentChain.SignalTime;
                if (timeSpanSinceLastSignal > Parameters.SIGNAL_LIFESPAN) {
                    currentChain.Signal = null;
                    currentChain.SignalTime = null;
                    Console.WriteLine("Signal reached EOF");
                }
            }

            // 0c. Ask new signal
            if (currentChain.Signal == null) {
                var next_signal = this.GetSignalConditions();
                if (next_signal == null) continue;

                currentChain.Signal = next_signal;
                currentChain.SignalTime = DateTime.UtcNow;

                // trigger onChainCreate
                OnChainUpdated(currentChain, ChainUpdateReason.Modified);
                Step = 1;
            }

            #endregion

            if (currentChain == null || currentChain.Signal == null) throw new Exception("Your code suck.");

            #region 1.calculate try, asset for current chain for later steps can place order
            /*  a. if price changes lesser than Parameters.MIN_PRICE_CHANGE_PERCENT, sleep 60s
                b. if unrealizedPNL > 0, reset nextTry to 1.
                c.Asset = ((decimal)Math.Pow(2, nextTry - 1)) * Parameters.QUANTITY * assetFactor;
                    => agree with current price */
            if (Step == 1) {
                // a. a new chainStep is created that does not have a Try, Asset
                // b. or this step is for when we canceled previous order, need to recalculate Try, Asset
                var lastCompletedChain = GetLastCompletedChain();
                var assetFactor = currentChain.Signal.Value == OrderSide.Buy ? 1m : -1m;
                int nextTry;
                decimal? entryPrice;

                if (lastCompletedChain == null) {
                    nextTry = 1;
                    entryPrice = GetEntryPrice(currentChain.Signal.Value, 1);
                }
                else {
                    var askPrice = GetEntryPrice(OrderSide.Sell, 1);
                    var bidPrice = GetEntryPrice(OrderSide.Buy, 1);

                    var unrealizedPNL = _assetBalance.GetUnrealizedPNL(askPrice, bidPrice);
                    if (unrealizedPNL == 0) continue;

                    if (lastCompletedChain.Order == null) throw new Exception("Please check your code.");
                    if (lastCompletedChain.Try == null) throw new Exception("Please check your code.");

                    if (unrealizedPNL > 0) {
                        // --a--
                        entryPrice = GetEntryPrice(currentChain.Signal.Value, 1);
                        var changePercent = Math.Abs(entryPrice.Value - lastCompletedChain.Order.Price) / lastCompletedChain.Order.Price * 100;
                        if (changePercent < GetMinPriceChangePercent(lastCompletedChain.Try.Value)) { await Task.Delay(60000); continue; }

                        //currentChain.Price.Value = currentChain.Price; //_assetBalance.Asset

                        // --b--
                        nextTry = 1;
                        if (nextTry > Parameters.MAX_TRY) { await Task.Delay(120000); continue; }
                    }
                    else {
                        // --a--
                        entryPrice = GetEntryPrice(currentChain.Signal.Value, lastCompletedChain.Try.Value + 1);
                        var changePercent = Math.Abs(entryPrice.Value - lastCompletedChain.Order.Price) / lastCompletedChain.Order.Price * 100;
                        if (changePercent < GetMinPriceChangePercent(lastCompletedChain.Try.Value + 1)) { await Task.Delay(60000); continue; }

                        // --b--
                        nextTry = lastCompletedChain.Try.Value + 1;
                        if (nextTry > Parameters.MAX_TRY) { await Task.Delay(120000); continue; }
                    }
                        
                }

                // --c--
                currentChain.Try = nextTry;
                currentChain.Asset = ((decimal)Math.Pow(2, nextTry - 1)) * Parameters.QUANTITY * assetFactor;
                currentChain.Price = entryPrice.Value;
                OnChainUpdated(currentChain, ChainUpdateReason.Modified);
                Step = 2;
            }
            #endregion

            // place order
            if (Step == 2) {

                if (currentChain.Asset == null || currentChain.Try == null || currentChain.Price == null) throw new Exception("Please check your previous steps.");

                // --0a-
                // x is number of asset need to buy
                // storage.asset + x = currentChain.asset
                // ==> x = currentChain.asset - storage.asset
                var quantity = currentChain.Asset.Value - _assetBalance.Asset; // x
                var side = quantity > 0 ? OrderSide.Buy : OrderSide.Sell;
                var price = currentChain.Price.Value; // GetEntryPrice(side, currentChain.Try.Value);

                quantity = _symbolInfo.NormalizeQuantity(Math.Abs(quantity));
                price = _symbolInfo.NormalizePrice(price);

                if (quantity == 0) {
                    // --0b-
                    currentChain.Order = new() {
                        Status = OrderStatus.Filled, Side = OrderSide.Buy, 
                        Price = price, QuantityFilled = 0,
                    };
                }
                else {
                    // --4c--
                    var placeOrderResponse = await Account.InternalRestClient.SpotApi.Trading.PlaceOrderAsync(Parameters.SYMBOL,
                        side: side, type: SpotOrderType.LimitMaker,
                        quantity: quantity, price: price,
                        orderResponseType: OrderResponseType.Full
                    );

                    if (placeOrderResponse.Error != null) {
                        var error = placeOrderResponse.Error;
                        Console.WriteLine("[error]" + error.Code + " " + error.Message);

                        switch (error.Code) {
                            case -1001: // Internal error; unable to process your request. Please try again.
                                continue;
                            case -1021: // INVALID_TIMESTAMP
                                // Timestamp for this request is outside of the recvWindow.
                                // Timestamp for this request was 1000ms ahead of the server's time.
                                continue; // safe
                            case -2010: // "Order would immediately match and take."
                                if (error.Message == "Order would immediately match and take.") {
                                    Step = 1; // Let's go back to step 1 to recalculate the price
                                    currentChain.Price = null;
                                    continue; // safe to ignore
                                }
                                throw new Exception(error.Message);
                            case -5022: // "Due to the order could not be executed as maker, the Post Only order will be rejected.The order will not be recorded in the order history"
                                continue;
                            case -5028: // "Timestamp for this request is outside of the ME recvWindow."
                                continue; // safe to ignore
                            default:
                                throw new Exception(error.Message);
                        }
                    }

                    currentChain.Order = placeOrderResponse.Data;
                }

                // new order created
                OnChainUpdated(currentChain, ChainUpdateReason.OrderCreated);
                Step = 3;

            }

            // order reach end of life or filled
            if (Step == 3) {
                var order = currentChain.Order;
                if (order == null) throw new Exception("Please check your previous steps.");

                if (order.Status == OrderStatus.New && DateTime.UtcNow - order.CreateTime > Parameters.WAIT_TIME_FOR_ORDER_TO_FILL_BEFORE_CANCEL) {
                    // NO
                    var canelResponse = await Account.InternalRestClient.SpotApi.Trading.CancelOrderAsync(Parameters.SYMBOL, orderId: order.Id, cancelRestriction: CancelRestriction.OnlyNew);
                    var error = canelResponse.Error;
                    
                    if (error != null) {
                        Console.WriteLine("[error]" + error.Code + " " + error.Message);

                        switch (error.Code) {
                            case -1021: // INVALID_TIMESTAMP
                                // Timestamp for this request is outside of the recvWindow.
                                // Timestamp for this request was 1000ms ahead of the server's time.
                                continue; // safe
                            case -2011: // Unknown order sent.
                                if (error.Message == "Unknown order sent.") {
                                    var response = await Account.InternalRestClient.SpotApi.Trading.GetOrderAsync(Parameters.SYMBOL, orderId: order.Id);
                                    order.ClientOrderId = response.Data.ClientOrderId;
                                    order.CreateTime = response.Data.CreateTime;
                                    order.IcebergQuantity = response.Data.IcebergQuantity;
                                    order.OrderListId = response.Data.OrderListId;
                                    //order.OriginalClientOrderId = response.Data.OriginalClientOrderId;
                                    order.Price = response.Data.Price;
                                    order.Quantity = response.Data.Quantity;
                                    order.QuantityFilled = response.Data.QuantityFilled;
                                    //order.QuantityRemaining = response.Data.QuantityRemaining;
                                    order.QuoteQuantity = response.Data.QuoteQuantity;
                                    order.QuoteQuantityFilled = response.Data.QuoteQuantityFilled;
                                    order.Side = response.Data.Side;
                                    order.Status = response.Data.Status;
                                    order.StopPrice = response.Data.StopPrice;
                                    order.Symbol = response.Data.Symbol;
                                    order.TimeInForce = response.Data.TimeInForce;
                                    order.Type = response.Data.Type;
                                    order.UpdateTime = response.Data.UpdateTime;

                                    Console.WriteLine("All Good");
                                    continue;
                                }
                                else if (error.Message == "Order was not canceled due to cancel restrictions.") continue;
                                throw new Exception(error.Message);
                            default:
                                throw new Exception(error.Message);
                        }
                    }

                    currentChain.Order = null;
                    currentChain.Try = null;
                    currentChain.Asset = null;

                    Step = 0;
                     
                    OnChainUpdated(currentChain, ChainUpdateReason.OrderCanceled); // order canceled
                }
                // --1b--
                else if (order.Status == OrderStatus.Filled) {
                    // YES
                    OnChainUpdated(currentChain, ChainUpdateReason.OrderFilled);
                    Step = 4;
                }
            }

            if (Step == 4) {
                var order = currentChain.Order;

                if (order == null) throw new Exception("Order can't be null.");

                if (order.Side == OrderSide.Buy) _assetBalance.Insert(order.QuantityFilled, order.Price);
                else if (order.Side == OrderSide.Sell) _assetBalance.Insert(-order.QuantityFilled, order.Price);
                else throw new Exception("????");

                Console.WriteLine("asset = " +  _assetBalance.Asset + " ; balance=" + _assetBalance.Balance, order.Price);
                Step = 5;
            }

            if (Step == 5) {
                var lastCompletedChain = GetLastCompletedChain();
                var order = currentChain.Order;
                if (order == null) throw new Exception("Order can't be null.");

                // --3a--
                if (lastCompletedChain == null) {
                    // this means we are on the first chain
                    currentChain.IsCompleted = true;
                    Step = 0;
                }
                // --3b--
                else {
                    var profit = _assetBalance.GetUnrealizedPNL(order.Price, order.Price);
                    if (profit > 0) {
                        // raise event profit
                        lastCompletedChain.Profit = profit;
                        OnProfit(lastCompletedChain, profit);

                        _assetBalance.Balance = -_assetBalance.Asset * order.Price; // reset unrealized profit & loss
                        currentChain.IsCompleted = true;
                        Step = 0;
                        Console.WriteLine("Reset storage asset=" + _assetBalance.Asset + "; balance=" + _assetBalance.Balance);
                    }
                    else {
                        // raise event profit
                        lastCompletedChain.Profit = profit;
                        OnProfit(lastCompletedChain, profit);

                        _assetBalance.Balance = -_assetBalance.Asset * order.Price; // reset unrealized profit & loss
                        currentChain.IsCompleted = true;
                        Step = 0;
                        Console.WriteLine("Reset storage asset=" + _assetBalance.Asset + "; balance=" + _assetBalance.Balance);
                    }
                }
            }
        }

        Status = StrategyStatus.Aborted;

    }
    public void Stop() {
        if (Status != StrategyStatus.Running) throw new Exception("Strategy is not running.");


        _cancellationTokenSource!.Cancel();
        _cancellationTokenSource.Dispose();
    }
    public ValueTask DisposeAsync() {
        if (Status == StrategyStatus.Running) Stop();

        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
    #endregion

    #region Private Methods - Steps & Event handlers
    decimal GetEntryPrice(OrderSide side, int currentTry) {
        if (currentTry <= 0) throw new Exception("The currentTry can't lesser than 0.");

        var entry_diff_percent = GetEntryDiffPercent(currentTry);

        if (lastBookTicker == null) throw new Exception("lastBookTicker can't be null.");
        if (_symbolInfo == null) throw new Exception("symbolInfo can't be null.");

        if (side == OrderSide.Buy) {
            var entryPrice = lastBookTicker.BestBidPrice * (1 - entry_diff_percent / 100);
            entryPrice = _symbolInfo.NormalizePrice(entryPrice);

            return entryPrice;
        }
        else if (side == OrderSide.Sell) {
            var entryPrice = lastBookTicker.BestAskPrice * (1 + entry_diff_percent / 100);
            entryPrice = _symbolInfo.NormalizePrice(entryPrice);
                
            return entryPrice;
        }
        else throw new Exception("must be 'BUY' or 'SELL'");
    }

    decimal GetEntryDiffPercent(int currentTry) {
        if (currentTry <= 0) throw new Exception("The currentTry can't lesser than 0.");

        if (currentTry == 1) return Parameters.ENTRY_DIFF_PERCENT;
        else 
            return Parameters.ENTRY_DIFF_PERCENT * (decimal)Math.Pow((double)Parameters.LOSS_SERIES_ENTRY_DIFF_MULTIPLIER, currentTry - 1);
        
        
    }
    decimal GetMinPriceChangePercent(int currentTry) {
        if (currentTry <= 0) throw new Exception("The currentTry can't lesser than 0.");

        if (currentTry == 1) return Parameters.MIN_PRICE_CHANGE_PERCENT;
        else 
            return Parameters.MIN_PRICE_CHANGE_PERCENT * (decimal)Math.Pow((double)Parameters.LOSS_SERIES_MIN_PRICE_CHANGE_MULTIPLIER, currentTry - 1);
    }

    ///<summary>Gets the last chain that is not completed.</summary>
    ChainStep? GetCurrentChain() {
        for (var i = ChainSteps.Count - 1; i >= 0; i--) {
            var chain = ChainSteps[i];

            if (chain.IsCompleted == false) return chain;
        }

        return null;
    }
    ChainStep? GetLastCompletedChain() {
        for (var i = ChainSteps.Count - 1; i >= 0; i--) {
            var chain = ChainSteps[i];

            if (chain.IsCompleted == true) return chain;
        }
        return null;
    }

    ///<summary>Satisfied conditions: MIN_TIME_BETWEEN_SIGNAL.</summary>
    OrderSide? GetSignalConditions() {
        var lastCompletedChain = GetLastCompletedChain();
        var lastSignalTime = lastCompletedChain?.SignalTime;

        // do not ask signal if time not greater than MIN_TIME_BETWEEN_SIGNAL
        if (lastSignalTime != null && DateTime.UtcNow - lastSignalTime < Parameters.MIN_TIME_BETWEEN_SIGNAL) return null;

        return Signal!.GetSignal();
    }
    #endregion

    #region Private Event handlers
    void OnBookTicker(DateTime timestamp, string symbol, BinanceStreamBookPrice bookPrice) {
        lastBookTicker = bookPrice;
        lastBookTickerAt = DateTime.UtcNow;
    }
    void OnTradeOrderUpdate(DateTime timestamp, BinanceStreamOrderUpdate order) {
        for (var i = 0; i < ChainSteps.Count; i++) {
            var chain = ChainSteps[i];

            // PARTIALLY_FILLED -> FILLED at the same updateTime

            if (chain.Order?.Id == order.Id && (order.UpdateTime >= chain.Order?.UpdateTime || chain.Order?.UpdateTime == null)
                ) {
                if (chain.Order == null) throw new Exception("Never happend. Stupid warning.");

                chain.Order.ClientOrderId = order.ClientOrderId;
                chain.Order.CreateTime = order.CreateTime;
                chain.Order.IcebergQuantity = order.IcebergQuantity;
                chain.Order.OrderListId = order.OrderListId;
                //chain.Order.OriginalClientOrderId = order.OriginalClientOrderId;
                chain.Order.Price = order.Price;
                chain.Order.Quantity = order.Quantity;
                chain.Order.QuantityFilled = order.QuantityFilled;
                //chain.Order.QuantityRemaining = order.QuantityRemaining;
                chain.Order.QuoteQuantity = order.QuoteQuantity;
                chain.Order.QuoteQuantityFilled = order.QuoteQuantityFilled;
                chain.Order.Side = order.Side;
                chain.Order.Status = order.Status;
                chain.Order.StopPrice = order.StopPrice;
                chain.Order.Symbol = order.Symbol;
                chain.Order.TimeInForce = order.TimeInForce;
                chain.Order.Type = order.Type;
                chain.Order.UpdateTime = order.UpdateTime;
            }
        }
    }
    void OnChainUpdated(ChainStep chain, ChainUpdateReason reason) {
        ChainUpdated?.Invoke(chain, reason);
    }
    void OnProfit(ChainStep chain, decimal profit) {
        if (profit > 0) Statistics.WinningSignal++;
        else Statistics.LossingSignal++;

        Statistics.TotalProfit += Math.Max(profit, 0);
        Statistics.TotalLoss += Math.Abs(Math.Min(profit, 0));


        Profit?.Invoke(chain, profit);
    }
    #endregion

}




