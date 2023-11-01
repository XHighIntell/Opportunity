using Binance.Net.Clients;
using CryptoExchange.Net.Authentication;
using Blite.Spot.Strategy;

namespace Blite.Spot {

    ///<summary>The highest instance of identity of an user.</summary>
    public class Account : IUserDataUnsubscribable {

        public Account(BliteClient client, string key, string secret) : this(client, new ApiCredentials(key, secret)) { }
        public Account(BliteClient client, ApiCredentials apiCredentials) {
            this.Client = client;
            this.ApiCredentials = apiCredentials;
            this.InternalRestClient = new BinanceRestClient(options => {
                options.Environment = client.Environment;
                options.ApiCredentials = apiCredentials;
            });
            Strategies = new();
        }

        Timer? stateTimer;
        DateTime _lastListenKeyAt = DateTime.MinValue;

        #region Properties
        public string? Name { get; set; }
        public BliteClient Client { get; }
        public ApiCredentials ApiCredentials { get; }
        public string? ListenKey { get; private set; }
        public List<StrategyDouble> Strategies { get; }
        public BinanceRestClient InternalRestClient { get; }
        #endregion

        #region Methods

        #endregion

        // ===== Subscribe events =======
        #region Subscribe to user data
        private UserDataSubscriptionGroup? _userDataSubscriptionGroup;
        readonly SemaphoreSlim _subscribeToUserDataAsyncLock = new(1, 1);

        public async Task<UserDataSubscriptionItem> SubscribeToUserDataAsync(TradeOrderUpdateCallback orderUpdateCallback) {

            await _subscribeToUserDataAsyncLock.WaitAsync();
            var group = _userDataSubscriptionGroup;

            try {
                if (group == null) {

                    var startUserStreamResponse = await InternalRestClient.SpotApi.Account.StartUserStreamAsync();
                    if (startUserStreamResponse.Error != null) throw new Exception(startUserStreamResponse.Error.Message);

                    ListenKey = startUserStreamResponse.Data;
                    _lastListenKeyAt = DateTime.UtcNow;

                    var updateSubscription = await Client.RawClient.SpotApi.Account.SubscribeToUserDataUpdatesAsync(ListenKey, e => {
                        if (group == null) return;
                        for (var i = 0; i < group.Items.Count; i++) {
                            var item = group.Items[i];
                            item.OrderUpdated(e.Timestamp, e.Data);
                        }
                    }, null, null, null);

                    if (updateSubscription.Error != null) throw new Exception(updateSubscription.Error.Message);

                    _userDataSubscriptionGroup = group = new UserDataSubscriptionGroup(this, ListenKey, updateSubscription.Data);

                    stateTimer = new Timer(OnTimerTick, null, 60000, 60000);
                }
            }
            finally {
                _subscribeToUserDataAsyncLock.Release();
            }

            return group.AddListener(orderUpdateCallback);
        }
        public void Unsubscribe(UserDataSubscriptionItem subscription) {
            if (_userDataSubscriptionGroup == null) throw new Exception("You trying to unsubscribe wrong place."); // nothing to unsubscribe
            if (_userDataSubscriptionGroup.Items.Contains(subscription) == false) return;

            subscription.Group.Items.Remove(subscription);
        }
        async void OnTimerTick(object? state) {
            if (ListenKey == null) throw new Exception("Why is ListenKey null here?");
            if (DateTime.UtcNow - _lastListenKeyAt > TimeSpan.FromMinutes(26)) {
                var response = await InternalRestClient.SpotApi.Account.KeepAliveUserStreamAsync(ListenKey);
                if (response.Error != null) throw new Exception(response.Error.Message);

                _lastListenKeyAt = DateTime.UtcNow;
            }
        }
        #endregion
    }
}
