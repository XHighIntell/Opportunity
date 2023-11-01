using Binance.Net;
using Binance.Net.Clients;
using Binance.Net.Objects.Models.Spot;

namespace Blite.Indicators {
    [TestClass]
    public class StochRSI_ {
        [TestMethod("StochRSI(4, 4, 3, 3)")]
        public void Test1() {
            var stoch = new StochRSI(4, 4, 3, 3);
            var sources = new double[] {
                146.73, 164.96, 168.25, 173.35, 184.7, 178.46, 185.82, 178.91, 169.06, 173.73, 173.71, 176.08, 182.18, 178.86, 163.47, 159.96, 160.9, 153.57, 147.62, 150.66, 148.62, 146.9
            };
            var results = stoch.Next(sources[..]);


            Assert.AreEqual(double.NaN, results[0].K); Assert.AreEqual(double.NaN, results[0].D);
            Assert.AreEqual(double.NaN, results[1].K); Assert.AreEqual(double.NaN, results[1].D);
            Assert.AreEqual(double.NaN, results[2].K); Assert.AreEqual(double.NaN, results[2].D);
            Assert.AreEqual(double.NaN, results[3].K); Assert.AreEqual(double.NaN, results[3].D);
            Assert.AreEqual(double.NaN, results[4].K); Assert.AreEqual(double.NaN, results[4].D);
            Assert.AreEqual(double.NaN, results[5].K); Assert.AreEqual(double.NaN, results[5].D);
            Assert.AreEqual(double.NaN, results[6].K); Assert.AreEqual(double.NaN, results[6].D);
            Assert.AreEqual(double.NaN, results[7].K); Assert.AreEqual(double.NaN, results[7].D);
            Assert.AreEqual(double.NaN, results[8].K); Assert.AreEqual(double.NaN, results[8].D);
            Assert.AreEqual(7.08317319396149, results[9].K); Assert.AreEqual(double.NaN, results[9].D);
            Assert.AreEqual(20.790980901971988, results[10].K); Assert.AreEqual(double.NaN, results[10].D);
            Assert.AreEqual(54.12431423530532, results[11].K); Assert.AreEqual(27.3328227770796, results[11].D);
            Assert.AreEqual(80.37447437467716, results[12].K); Assert.AreEqual(51.76325650398482, results[12].D);
            Assert.AreEqual(74.60660148327572, results[13].K); Assert.AreEqual(69.70179669775273, results[13].D);
            Assert.AreEqual(41.27326814994239, results[14].K); Assert.AreEqual(65.41811466929842, results[14].D);
            Assert.AreEqual(7.939934816609053, results[15].K); Assert.AreEqual(41.27326814994238, results[15].D);
        }

        [TestMethod("StochRSI(14, 14, 3, 3) - Manual With XMRUSDT")]
        public async Task Test2() {
            var client = new BinanceRestClient(options => {
                options.Environment = BinanceEnvironment.Live;
            });
            var klineResponse = await client.UsdFuturesApi.ExchangeData.GetKlinesAsync("XMRUSDT", Binance.Net.Enums.KlineInterval.OneDay);
            var sources = klineResponse.Data.Select(o => (double)o.ClosePrice).ToArray();

            var stoch = new StochRSI(14, 14, 3, 3);
            var results = stoch.Next(sources[..]);

            for (var i = 0; i < results.Length; i++) {
                var item = results[i];
                Console.WriteLine("[" + i + "] K=" + item.K + ", D=" + item.D);
            }
            
        }
    }
}
