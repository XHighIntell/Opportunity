using Binance.Net;
using CryptoExchange.Net.Authentication;
using Newtonsoft.Json;
using System.Threading.Tasks.Sources;

namespace Blite.Spot {
    [TestClass]
    public class AccountT {

        readonly ApiCredentials apiCredentials = new("sxYFKIv3AP2HN5cAASBq0eJZ1MvgCWOoVVUN0iHmQ9fWunEZedlLlJWS8hQ6hRQk", "4TWDkMrigZsZ5782ky0DsmLbNs7s85AflTUmMNYEhDzu8dahHSWqSMHFr9RDF5NO");

        [TestMethod("SubscribeToUserDataAsync")]
        public async Task SubscribeToUserDataAsync() {
            var client = new BliteClient(BinanceEnvironment.Testnet);
            var account = new Account(client, apiCredentials);

            var lastAt = DateTime.MinValue;

            SemaphoreSlim _lock = new(0, 1);

            var o = await account.SubscribeToUserDataAsync((t, order) => {
                Console.WriteLine($"FIRST: {t.ToString("HH:mm:ss")} {JsonConvert.SerializeObject(order)}");
                _lock.Release();
            });

            var placeOrderResponse = await account.InternalRestClient.SpotApi.Trading.PlaceOrderAsync("BTCUSDT", Binance.Net.Enums.OrderSide.Buy, Binance.Net.Enums.SpotOrderType.Limit, 0.001m, null, null, price: 18000m,
                timeInForce: Binance.Net.Enums.TimeInForce.GoodTillCanceled);
            var order = placeOrderResponse.Data;
            var cancelOrderResponse =await  account.InternalRestClient.SpotApi.Trading.CancelOrderAsync("BTCUSDT", order.Id);

            await _lock.WaitAsync();
        }

        public async Task SubscribeToUserDataAsync_LongTest() {
            var liteClient = new BliteClient(BinanceEnvironment.Testnet);
            var account = new Account(liteClient, apiCredentials);

            var o = await account.SubscribeToUserDataAsync((t, order) => {
                Console.WriteLine($"FIRST: {t.ToString("HH:mm:ss")} {JsonConvert.SerializeObject(order)}");
            });
        }

    }
}
