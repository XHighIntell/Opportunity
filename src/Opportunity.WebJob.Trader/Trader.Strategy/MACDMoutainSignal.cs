using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot;
using Blite;
using Blite.Indicators;
using Blite.Spot;
using System.Collections.Generic;
using static Blite.Indicators.MACD;
using static Blite.Indicators.StochRSI;

namespace Opportunity.WebJob.Trader.Strategy;

public class MACDMoutainSignal: ISignal, IDisposable {

    public MACDMoutainSignal() {
        IsTestMode = true;
    }
    public MACDMoutainSignal(BliteClient client, string symbol, KlineInterval interval) {
        klineSubscriptionItem = client.SubscribeToKlinesAsync(symbol.ToUpper(), interval, true, OnKlineUpdate).Result;
    }

    readonly KlineSubscriptionItem? klineSubscriptionItem;
    readonly MACD macd = new(); 
    readonly StochRSI stochRSI = new();
    readonly List< MACDResult> macd_results = new();
    readonly List<StochRSIResult> stochRSI_results = new();

    public bool IsTestMode { get; }

    

    #region Methods
    public OrderSide? GetSignal() {
        if (macd_results.Count < 4) return null;

        // 0 1 2 3

        if (macd_results[2].Macd > 1) {
            if (macd_results[2].Macd > macd_results[0].Macd) {
                if (macd_results[2].Macd > macd_results[1].Macd) {
                    if (macd_results[2].Macd > macd_results[3].Macd) {
                        return OrderSide.Sell;
                    }
                }
            }
        }

        if (macd_results[2].Macd < -1) {
            if (macd_results[2].Macd < macd_results[0].Macd) {
                if (macd_results[2].Macd < macd_results[1].Macd) {
                    if (macd_results[2].Macd < macd_results[3].Macd) {
                        return OrderSide.Buy;
                    }
                }
            }
        }

        return null;
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }
    public void NextKlineUpdate(IBinanceKline kline) {
        macd_results.Add(macd.Next((double)kline.ClosePrice));
        stochRSI_results.Add(stochRSI.Next((double)kline.ClosePrice));

        if (macd_results.Count > 4) macd_results.RemoveRange(0, macd_results.Count - 4);
        if (stochRSI_results.Count > 4) stochRSI_results.RemoveRange(0, stochRSI_results.Count - 4);
    }

    #endregion




    void OnKlineUpdate(DateTime timestamp, string symbol, IBinanceStreamKline kline, bool isReplay) {
     
    }
}

