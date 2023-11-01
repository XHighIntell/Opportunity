using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blite.Spot {
    public delegate void TradeOrderUpdateCallback(DateTime timestamp, BinanceStreamOrderUpdate order);

    public interface IUserDataUnsubscribable {
        public void Unsubscribe(UserDataSubscriptionItem subscription);
    }

    public class UserDataSubscriptionGroup {
        public UserDataSubscriptionGroup(IUserDataUnsubscribable owner, string listenKey, UpdateSubscription updateSubscription) {
            this.Owner = owner;
            this.ListenKey = listenKey;
            this.UpdateSubscription = updateSubscription;
            this.Items = new();
        }

        #region Properties
        public IUserDataUnsubscribable Owner { get; }
        public string ListenKey { get; }
        public UpdateSubscription UpdateSubscription { get; }
        public List<UserDataSubscriptionItem> Items { get; }
        #endregion

        public UserDataSubscriptionItem AddListener(TradeOrderUpdateCallback orderUpdated) {
            var item = new UserDataSubscriptionItem(this, orderUpdated);
            Items.Add(item);

            return item;
        }
    }

    public class UserDataSubscriptionItem: IDisposable {
        public UserDataSubscriptionItem(UserDataSubscriptionGroup group, TradeOrderUpdateCallback callback) {
            this.Group = group;
            this.OrderUpdated = callback;
        }

        bool _disposed = false;

        public UserDataSubscriptionGroup Group { get; }
        public TradeOrderUpdateCallback OrderUpdated { get; }

        public void Unsubscribe() { Group.Owner.Unsubscribe(this); }

        public void Dispose() {
            if (_disposed) return;
            _disposed = true;

            Unsubscribe();
            GC.SuppressFinalize(this);
        }

        ~UserDataSubscriptionItem() { Dispose(); }
    }


}
