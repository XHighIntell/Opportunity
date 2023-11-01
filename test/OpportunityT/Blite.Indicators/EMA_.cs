namespace Blite.Indicators {
    [TestClass]
    public class EMA_ {
        [TestMethod("EMA(2)")]
        public void Test1() {
            var ema = new EMA(2);
            var results = ema.Next(double.NaN, 1, 2, 4, 8, 16, 32, 64);

            var alpha = 2d / (2d + 1d);
            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual(1.5, results[2]);
            Assert.AreEqual(alpha * 4 + (1 - alpha) * results[2], results[3]);
            Assert.AreEqual(alpha * 8 + (1 - alpha) * results[3], results[4]);
            Assert.AreEqual(alpha * 16 + (1 - alpha) * results[4], results[5]);
            Assert.AreEqual(alpha * 32 + (1 - alpha) * results[5], results[6]);
            Assert.AreEqual(alpha * 64 + (1 - alpha) * results[6], results[7]);
        }

        [TestMethod("EMA(3)")]
        public void Test2() {
            var ema = new EMA(3);
            var results = ema.Next(1, 2, 4, 8, 16, 32, 64);
            var epsilon = 0.000_000_000_000_01d;

            var alpha = 2d / (3d + 1d);
            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual((1 + 2 + 4) / 3d, results[2], epsilon);
            Assert.AreEqual(alpha * 8 + (1 - alpha) * results[2], results[3]);
            Assert.AreEqual(alpha * 16 + (1 - alpha) * results[3], results[4]);
            Assert.AreEqual(alpha * 32 + (1 - alpha) * results[4], results[5]);
            Assert.AreEqual(alpha * 64 + (1 - alpha) * results[5], results[6]);
        }
    }
}
