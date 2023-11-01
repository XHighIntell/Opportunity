using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoExchange.Net.Sockets;

namespace Blite.Spot;

public delegate void KlineUpdateCallback(DateTime timestamp, string symbol, IBinanceStreamKline kline, bool isReplay);
public interface IKlineUnsubscribable {
    public void Unsubscribe(KlineSubscriptionItem subscription);
}

public class KlineSubscriptionGroup {
    public KlineSubscriptionGroup(IKlineUnsubscribable owner, string symbol, KlineInterval interval, UpdateSubscription updateSubscription) {
        this.Owner = owner;
        this.Symbol = symbol;
        this.Interval = interval;
        this.UpdateSubscription = updateSubscription;
        this.Items = new();
    }

    #region Properties
    public IKlineUnsubscribable Owner { get; }
    public string Symbol { get; private set; }
    public KlineInterval Interval { get; private set; }
    public UpdateSubscription UpdateSubscription { get; private set; }
    internal List<KlineSubscriptionItem> Items { get; private set; }
    #endregion

    public KlineSubscriptionItem AddListener(KlineUpdateCallback cb) {
        var item = new KlineSubscriptionItem(this, cb);
        Items.Add(item);

        return item;
    }
}
public class KlineSubscriptionItem: IDisposable {
    public KlineSubscriptionItem(KlineSubscriptionGroup group, KlineUpdateCallback callback) {
        this.Group = group;
        this.Callback = callback;
    }

    bool _disposed = false;

    #region Properties
    public KlineSubscriptionGroup Group { get; }

    public KlineUpdateCallback Callback { get; }

    ///<summary>The Id of the most recent closed candlestick.</summary>
    ///<remarks>The Id is an integral number that represents order of klines since the midnight at the beginning of January 1, 1970, UTC (the epoch).</remarks>
    public long? LastFinalId { get; internal set; }

    public bool IsFetchingMissingCandles { get; internal set; }
    #endregion

    #region Methods
    public void Unsubscribe() { Group.Owner.Unsubscribe(this); }

    public void Dispose() {
        if (_disposed) return;
        _disposed = true;

        Unsubscribe();
        GC.SuppressFinalize(this);
    }
    #endregion

    ~KlineSubscriptionItem() { Dispose(); }
}



