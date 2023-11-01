using Binance.Net;
using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot.Socket;
using Binance.Net.Objects.Models.Spot;

namespace Blite.Spot;

public class BliteClient: IKlineUnsubscribable, IBookPriceUnsubscribable, IUserDataUnsubscribable {
    public BliteClient(BinanceEnvironment environment) {
        RawClient = new BinanceSocketClient(options => {
            options.Environment = environment;
            //options.UsdFuturesOptions = null;
        });
        this.Environment = environment;
    }

    #region Properties
    public BinanceSocketClient RawClient { get; }
    public BinanceEnvironment Environment { get; }
    #endregion

    #region Methods

    #endregion

    #region Subscribe Methods: SubscribeToKlinesAsync, SubscribeToBookTicker, SubscribeToUserDataAsync
    #region Subscribe to Kline
    readonly List<KlineSubscriptionGroup> _klineUpdateSubscriptionGroup = new();
    readonly SemaphoreSlim _subscribeToKlinesAsyncLock = new(1, 1);

    public async Task<KlineSubscriptionItem> SubscribeToKlinesAsync(string symbol, KlineInterval interval, bool requestOldData, KlineUpdateCallback callback) {
        //MarketSubscriptionGroupItem
        await _subscribeToKlinesAsyncLock.WaitAsync();

        var group = _klineUpdateSubscriptionGroup.Find(o =>
            string.Equals(o.Symbol, symbol, StringComparison.OrdinalIgnoreCase) == true && o.Interval == interval
        );

        try {
            if (group == null) {
                // first come,
                var updateSubscription = await RawClient.SpotApi.ExchangeData.SubscribeToKlineUpdatesAsync(symbol, interval, (e) => {
                    if (group == null) return;
                    OnIncomingKline(group, e.Timestamp, e.Data.Data);
                });

                if (updateSubscription.Error != null) throw new Exception(updateSubscription.Error.Message);

                group = new KlineSubscriptionGroup(this, symbol, interval, updateSubscription.Data);
                _klineUpdateSubscriptionGroup.Add(group);
            }
        }
        finally {
            _subscribeToKlinesAsyncLock.Release();
        }


        // already have
        var item = group.AddListener(callback);

        if (requestOldData == true) {
            item.IsFetchingMissingCandles = true;


            var klineResponse = await RawClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, interval, limit: 1000);
            if (klineResponse.Error != null) throw new Exception(klineResponse.Error.Message);

            var klines = klineResponse.Data.Result as List<BinanceSpotKline>;
            if (klines == null) throw new Exception("Why do we get a null?");


            for (var i = 0; i < klines.Count - 1; i++) {
                var kline = klines[i];

                OnIncomingKline(group, kline.CloseTime, new BinanceStreamKline() {
                    OpenPrice = kline.OpenPrice,
                    ClosePrice = kline.ClosePrice,
                    Volume = kline.Volume,
                    CloseTime = kline.CloseTime,
                    HighPrice = kline.HighPrice,
                    LowPrice = kline.LowPrice,
                    OpenTime = kline.OpenTime,
                    QuoteVolume = kline.QuoteVolume,
                    TakerBuyBaseVolume = kline.TakerBuyBaseVolume,
                    TakerBuyQuoteVolume = kline.TakerBuyQuoteVolume,
                    TradeCount = kline.TradeCount,
                    Interval = interval,
                    Symbol = symbol,
                    Final = true,
                });
            }
            item.IsFetchingMissingCandles = false;
        }

        return item;
    }
    public void Unsubscribe(KlineSubscriptionItem subscription) {
        if (_klineUpdateSubscriptionGroup.Contains(subscription.Group) == false) return;
        subscription.Group.Items.Remove(subscription);

        if (subscription.Group.Items.Count == 0) {
            RawClient.UnsubscribeAsync(subscription.Group.UpdateSubscription);
            _klineUpdateSubscriptionGroup.Remove(subscription.Group);
        }
    }

    async void OnIncomingKline(KlineSubscriptionGroup group, DateTime timestamp, IBinanceStreamKline next_kline) {
        var symbol = group.Symbol;
        var interval = group.Interval;

        for (var i = 0; i < group.Items.Count; i++) {
            var item = group.Items[i];

            if (item.LastFinalId == null && next_kline.Final == true) {
                item.LastFinalId = next_kline.OpenTime.ToJsDateTime() / 1000L / (long)interval;
                item.Callback(timestamp, symbol, next_kline, false);
                continue;
            }

            var currentCandleId = next_kline.OpenTime.ToJsDateTime() / 1000L / (long)interval;
            var delta = currentCandleId - item.LastFinalId;

            if (delta < 1) continue; // ignore old data
            else if (delta == 1) {
                if (next_kline.Final == true) item.LastFinalId = currentCandleId;

                item.Callback(timestamp, symbol, next_kline, false);
            }
            else if (delta > 1) {
                // we are missing something n candles, let fetching missing candles
                if (item.IsFetchingMissingCandles == true) continue;

                item.IsFetchingMissingCandles = true;
                var klines = await GetMissingCandle(item, symbol, interval);
                for (var j = 0; j < klines.Count - 1; j++) {
                    var kline = klines[j];

                    var currentId = kline.OpenTime.ToJsDateTime() / 1000L / (long)interval;
                    if (currentId - item.LastFinalId != 1) throw new Exception("Why is the next candle wrong?");

                    item.LastFinalId = currentId;
                    item.Callback(kline.CloseTime, symbol, new BinanceStreamKline() {
                        OpenPrice = kline.OpenPrice,
                        ClosePrice = kline.ClosePrice,
                        Volume = kline.Volume,
                        CloseTime = kline.CloseTime,
                        HighPrice = kline.HighPrice,
                        LowPrice = kline.LowPrice,
                        OpenTime = kline.OpenTime,
                        QuoteVolume = kline.QuoteVolume,
                        TakerBuyBaseVolume = kline.TakerBuyBaseVolume,
                        TakerBuyQuoteVolume = kline.TakerBuyQuoteVolume,
                        TradeCount = kline.TradeCount,
                        Interval = interval,
                        Symbol = symbol,
                        Final = true,
                    }, true);
                }


                item.IsFetchingMissingCandles = false;
            }

        }
    }
    async Task<List<BinanceSpotKline>> GetMissingCandle(KlineSubscriptionItem item, string symbol, KlineInterval interval) {
        if (item.LastFinalId == null) throw new Exception("Why are we getting missing candles?");

        var startime = ((item.LastFinalId.Value + 1) * (long)interval * 1000L).JSToDateTime();
        var klineResponse = await RawClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, interval,
            startTime: startime
        );

        if (klineResponse.Error != null) throw new Exception(klineResponse.Error.Message);
        var klines = klineResponse.Data.Result;
        if (klines == null) throw new Exception("Why is it possible?");

        return klines.ToList();
    }
    #endregion

    #region Subscribe to bookTicker
    readonly List<BookTickerSubscriptionGroup> _bookTickerUpdateSubscriptionGroup = new();
    readonly SemaphoreSlim _subscribeToBookTickerAsyncLock = new(1, 1);
    public async Task<BookTickerSubscriptionItem> SubscribeToBookTicker(string symbol, BookTickerUpdateCallback callback) {
        symbol = symbol.ToUpper();

        await _subscribeToBookTickerAsyncLock.WaitAsync();
        var group = _bookTickerUpdateSubscriptionGroup.Find(o => o.Symbol == symbol);

        try {
            if (group == null) {
                // first come,
                var updateSubscription = await RawClient.SpotApi.ExchangeData.SubscribeToBookTickerUpdatesAsync(symbol, e => {
                    if (group == null) return;
                    for (var i = 0; i < group.Items.Count; i++) {
                        var item = group.Items[i];
                        item.Callback(e.Timestamp, e.Data.Symbol, e.Data);
                    }
                });

                if (updateSubscription.Error != null) throw new Exception(updateSubscription.Error.Message);

                group = new BookTickerSubscriptionGroup(this, symbol, updateSubscription.Data);
                _bookTickerUpdateSubscriptionGroup.Add(group);
            }
        }
        finally {
            _subscribeToBookTickerAsyncLock.Release();
        }

        var item = group.AddListener(callback);
        return item;
    }
    public void Unsubscribe(BookTickerSubscriptionItem subscription) {
        if (_bookTickerUpdateSubscriptionGroup.Contains(subscription.Group) == false) return;
        subscription.Group.Items.Remove(subscription);

        if (subscription.Group.Items.Count == 0) {
            RawClient.UnsubscribeAsync(subscription.Group.UpdateSubscription);
            _bookTickerUpdateSubscriptionGroup.Remove(subscription.Group);
        }
    }
    #endregion

    #region subscribe to user data
    readonly List<UserDataSubscriptionGroup> _userDataSubscriptionGroup = new();
    readonly SemaphoreSlim _subscribeToUserDataAsyncLock = new(1, 1);

    public async Task<UserDataSubscriptionItem> SubscribeToUserDataAsync(string listenKey, TradeOrderUpdateCallback orderUpdateCallback) {
        await _subscribeToUserDataAsyncLock.WaitAsync();
        var group = _userDataSubscriptionGroup.Find(o => o.ListenKey == listenKey);

        try {
            if (group == null) {
                // first come,
                var updateSubscription = await RawClient.SpotApi.Account.SubscribeToUserDataUpdatesAsync(listenKey, e => {
                    if (group == null) return;
                    for (var i = 0; i < group.Items.Count; i++) {
                        var item = group.Items[i];
                        item.OrderUpdated(e.Timestamp, e.Data);
                    }
                }, null, null, null);

                if (updateSubscription.Error != null) throw new Exception(updateSubscription.Error.Message);

                group = new UserDataSubscriptionGroup(this, listenKey, updateSubscription.Data);
                _userDataSubscriptionGroup.Add(group);
            }
        }
        finally {
            _subscribeToUserDataAsyncLock.Release();
        }

        var item = group.AddListener(orderUpdateCallback);
        return item;
    }
    public void Unsubscribe(UserDataSubscriptionItem subscription) {
        if (_userDataSubscriptionGroup.Contains(subscription.Group) == false) return;
        subscription.Group.Items.Remove(subscription);

        if (subscription.Group.Items.Count == 0) {
            RawClient.UnsubscribeAsync(subscription.Group.UpdateSubscription);
            _userDataSubscriptionGroup.Remove(subscription.Group);
        }
    }
    #endregion
    #endregion
}

