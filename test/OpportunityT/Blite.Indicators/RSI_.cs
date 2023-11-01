namespace Blite.Indicators {
    [TestClass]
    public class RSI_ {
        [TestMethod("RSI(2)")]
        public void Test1() {
            var rsi = new RSI(2);
            var results = rsi.Next(145.86, 144.61, 146.71, 165.01, 168.23, 173.36, 184.74);

            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual(62.68656716417904, results[2]);
            Assert.AreEqual(96.87108886107634, results[3]);
            Assert.AreEqual(97.63392012114329, results[4]);
            Assert.AreEqual(98.66837115159262, results[5]);
            Assert.AreEqual(99.54701938757022, results[6]);
        }

        [TestMethod("RSI(3)")]
        public void Test2() {
            var rsi = new RSI(3);
            var results = rsi.Next(145.86, 144.61, 146.71, 165.01, 168.23, 173.36, 184.74);

            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual(double.NaN, results[2]);
            Assert.AreEqual(94.22632794457274, results[3]);
            Assert.AreEqual(95.27945619335347, results[4]);
            Assert.AreEqual(96.71247287790125, results[5]);
            Assert.AreEqual(98.3645165510925, results[6]);
        }

    }
}
