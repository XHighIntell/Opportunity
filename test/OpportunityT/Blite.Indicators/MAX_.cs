namespace Blite.Indicators {
    [TestClass]
    public class MAX_ {
        [TestMethod("MAX(3)")]
        public void Test1() {
            var max = new MAX(3);
            var results = max.Next(double.NaN, 100, 80, 60, 70, 90);

            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual(double.NaN, results[2]);
            Assert.AreEqual(100, results[3]);
            Assert.AreEqual(80, results[4]);
            Assert.AreEqual(90, results[5]);
        }

    }
}
