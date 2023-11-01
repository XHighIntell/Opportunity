using Binance.Net.Enums;
using Binance.Net.Interfaces;

using Blite;
using Blite.Spot;
using Blite.Indicators;

namespace Opportunity.WebJob.Trader.Strategy;

public class MACDStochRSISignal: ISignal, IDisposable {

    public MACDStochRSISignal(BliteClient client, string symbol, KlineInterval interval) {
        klineSubscriptionItem = client.SubscribeToKlinesAsync(symbol.ToUpper(), interval, true, OnKlineUpdate).Result;
    }

    bool _disposed = false;
    readonly MACD macd = new(); MACD.MACDResult? macd_result;
    readonly StochRSI stochRSI = new(); StochRSI.StochRSIResult? stochRSI_result;
    DateTime lastResultAt = DateTime.MinValue;
    readonly KlineSubscriptionItem klineSubscriptionItem;

    #region Properties
    public MACD.MACDResult? LastMACDResult => macd_result;
    public StochRSI.StochRSIResult? LastStochRSIResult => stochRSI_result;
    public int UpdateCount { get; private set; }
    #endregion

    #region Methods
    public OrderSide? GetSignal() {
        if (DateTime.UtcNow - lastResultAt > TimeSpan.FromSeconds(30)) return null;

        var macd = macd_result;
        var stochRSI = stochRSI_result;

        if (macd == null) return null;
        if (stochRSI == null) return null;


        //Console.WriteLine("(macd.Macd + macd.Signal) / 2 = " + (macd.Macd + macd.Signal) / 2 + "; (stochRSI.K + stochRSI.K) / 2 = " + (stochRSI.K + stochRSI.K) / 2);

        var macdAverage = (macd.Macd + macd.Signal) / 2d;
        var stochRSIAverage = (stochRSI.K + stochRSI.D) / 2d;

        if (macdAverage > 4d && stochRSIAverage > 80d) return OrderSide.Sell;
        if (macdAverage < -4d && stochRSIAverage < 20d) return OrderSide.Buy;

        return null;
    }
    public void Dispose() {
        if (_disposed == true) return;
        _disposed = true;

        klineSubscriptionItem.Unsubscribe();
        GC.SuppressFinalize(this);
    }
    #endregion

    void OnKlineUpdate(DateTime timestamp, string symbol, IBinanceStreamKline kline, bool isReplay) {
        UpdateCount++;

        if (kline.Final == false) {
            macd_result = macd.Moment((double)kline.ClosePrice);
            stochRSI_result = stochRSI.Moment((double)kline.ClosePrice);
        }
        else {
            macd_result = macd.Next((double)kline.ClosePrice);
            stochRSI_result = stochRSI.Next((double)kline.ClosePrice);
        }

        lastResultAt = timestamp;
    }

    ~MACDStochRSISignal() { Dispose(); }
}

