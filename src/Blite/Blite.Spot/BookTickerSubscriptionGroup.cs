using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Sockets;

namespace Blite.Spot {
    public delegate void BookTickerUpdateCallback(DateTime timestamp, string symbol, BinanceStreamBookPrice bookPrice);

    public interface IBookPriceUnsubscribable {
        public void Unsubscribe(BookTickerSubscriptionItem subscription);
    }

    public class BookTickerSubscriptionGroup {
        public BookTickerSubscriptionGroup(IBookPriceUnsubscribable owner,  string symbol, UpdateSubscription updateSubscription) {
            this.Owner = owner;
            this.Symbol = symbol;
            this.UpdateSubscription = updateSubscription;
            this.Items = new();
        }

        #region Properties
        public IBookPriceUnsubscribable Owner { get; }
        public string Symbol { get; private set; }
        public UpdateSubscription UpdateSubscription { get; private set; }
        internal List<BookTickerSubscriptionItem> Items { get; private set; }
        #endregion

        public BookTickerSubscriptionItem AddListener(BookTickerUpdateCallback cb) {
            var item = new BookTickerSubscriptionItem(this, cb);
            Items.Add(item);

            return item;
        }
    }
    public class BookTickerSubscriptionItem: IDisposable {
        public BookTickerSubscriptionItem(BookTickerSubscriptionGroup group, BookTickerUpdateCallback callback) {
            this.Group = group;
            this.Callback = callback;
        }

        bool _disposed = false;

        public BookTickerSubscriptionGroup Group { get; }
        public BookTickerUpdateCallback Callback { get; }

        public void Unsubscribe() { Group.Owner.Unsubscribe(this); }
        public void Dispose() {
            if (_disposed) return;
            _disposed = true;

            Unsubscribe();
            GC.SuppressFinalize(this);
        }

        ~BookTickerSubscriptionItem() { Dispose(); }
    }


}
