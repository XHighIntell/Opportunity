using Binance.Net.Objects.Models.Spot;

namespace Blite.Indicators {
    [TestClass]
    public class Stoch_ {
        [TestMethod("Stoch(3)")]
        public void Test1() {
            var stoch = new Stoch(3);
            var results = stoch.Next(
                Stoch.StochInput.From(close: 146.73, high: 146.91, low: 140.01),
                Stoch.StochInput.From(close: 164.96, high: 166.25, low: 146.38),
                Stoch.StochInput.From(close: 168.25, high: 170.02, low: 161.49),
                Stoch.StochInput.From(close: 173.35, high: 174.35, low: 166.12),
                Stoch.StochInput.From(close: 184.7, high: 185.18, low: 170.58),
                Stoch.StochInput.From(close: 178.46, high: 187.77, low: 174.33)
            );

            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual(94.10196601132952, results[2]);
            Assert.AreEqual(96.42474079370754, results[3]);
            Assert.AreEqual(97.97382861967067, results[4]);
            Assert.AreEqual(56.997690531177824, results[5]);
        }

    }
}
