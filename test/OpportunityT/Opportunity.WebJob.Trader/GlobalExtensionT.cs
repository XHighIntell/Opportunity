using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Clients;
using Binance.Net.Objects.Models.Spot;

using CryptoExchange.Net;
using Blite;

namespace Opportunity.WebJob.Trader {
    [TestClass]
    public class GlobalExtensionT {
        [TestMethod("NormalizePirce")]
        public void Test1() {
            var symbol = new BinanceSymbol {
                Filters = new[] {
                    new BinanceSymbolPriceFilter() { FilterType = SymbolFilterType.Price, TickSize = 0.001m}
                }
            };

            Assert.AreEqual(0.123m, symbol.NormalizePrice(0.12345m));
            Assert.AreEqual(0.1m, symbol.NormalizePrice(0.1m));
        }

        [TestMethod("QuantityPirce")]
        public void Test2() {
            var symbol = new BinanceSymbol {
                Filters = new[] {
                    new BinanceSymbolLotSizeFilter() { FilterType = SymbolFilterType.LotSize, StepSize = 0.001m }
                }
            };

            Assert.AreEqual(0.123m, symbol.NormalizeQuantity(0.12345m));
            Assert.AreEqual(0.1m, symbol.NormalizeQuantity(0.1m));
        }
    }
}