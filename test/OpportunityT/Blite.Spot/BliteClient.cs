using Binance.Net;
using Binance.Net.Enums;

using Blite;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot.Socket;

namespace Blite.Spot {

    [TestClass]
    public class BliteClientT {

        public BliteClientT() { 
        
        }

        readonly BliteClient Client = new(BinanceEnvironment.Live);


        [TestMethod("SubscribeToBookTicker")]
        public async Task SubscribeToBookTicker() {

            BinanceStreamBookPrice? last = null;
            var o = await Client.SubscribeToBookTicker("BTCTUSD", (DateTime timestamp, string symbol, BinanceStreamBookPrice kline) => {
                Console.WriteLine($"{timestamp:HH:mm:ss.ffffff} {symbol}");
                last = kline;
            });
            
            await Task.Delay(2000);
            if (last == null) throw new Exception("We didn't get any updates for book price.");
        }


        [TestMethod("SubscribeToKlinesAsync - 2 times without await")]
        public async Task SubscribeToBookTicker2() {
            long? first_count = null;
            long? second_count = null;

            var first = Client.SubscribeToKlinesAsync("BTCTUSD", KlineInterval.OneMinute, true, (DateTime timestamp, string symbol, IBinanceStreamKline kline, bool replay) => {
                var x = kline.OpenTime.ToJsDateTime() / 1000L / (long)60L;

                if (first_count == null) first_count = x;
                if (x - first_count.Value < 0) throw new Exception("Incorrect order");
                if (x - first_count.Value > 1) throw new Exception("Incorrect order");
                first_count = x;

                Console.WriteLine($"first {x}: {timestamp} " +
                    $"O:{kline.OpenPrice:G29} " +
                    $"L:{kline.LowPrice:G29} " +
                    $"H:{kline.HighPrice:G29} " +
                    $"C:{kline.ClosePrice:G29}" + (kline.Final == true ? "========" : ""));
            });
            var second = Client.SubscribeToKlinesAsync("BTCTUSD", KlineInterval.OneMinute, true, (DateTime timestamp, string symbol, IBinanceStreamKline kline, bool replay) => {
                var x = kline.OpenTime.ToJsDateTime() / 1000L / (long)60L;

                if (second_count == null) second_count = x;
                if (x - second_count.Value < 0) throw new Exception("Incorrect order");
                if (x - second_count.Value > 1) throw new Exception("Incorrect order");
                second_count = x;

                Console.WriteLine($"second {x}: {timestamp} " +
                    $"O:{kline.OpenPrice:G29} " +
                    $"L:{kline.LowPrice:G29} " +
                    $"H:{kline.HighPrice:G29} " +
                    $"C:{kline.ClosePrice:G29}" + (kline.Final == true ? "========" : ""));

            });


            await Task.Delay(20000);

            using (var r1 = await first) {
                var r2 = await second;
            }


            //Client.Unsubscribe(r1);
            var save_second_count = second_count;
            await Task.Delay(5000);

            if (save_second_count != second_count) throw new Exception("Why are we still getting update?");
            Console.WriteLine("Automation testing completed. It does not include disconnect testing.");
        }

        [TestMethod("SubscribeToKlinesAsync - 10 hours testing")]
        public async Task SubscribeToKlinesAsync() {
            int? lastId = null;

            await Client.SubscribeToKlinesAsync("XMRUSDT", KlineInterval.OneMinute, true, (DateTime timestamp, string symbol, IBinanceStreamKline kline, bool isReplay) => {
                if (kline.Final == true) {
                    var currentId = (int)(kline.OpenTime.ToJsDateTime() / 60000);

                    if (lastId == null) lastId ??= currentId;
                    else {
                        var delta = currentId - lastId;

                        if (delta > 1) throw new Exception("");

                        lastId = currentId;
                    }
                }
            });

            await Task.Delay(TimeSpan.FromMinutes(600));
        }
    }


}