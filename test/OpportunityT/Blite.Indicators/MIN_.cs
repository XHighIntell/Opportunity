namespace Blite.Indicators {
    [TestClass]
    public class MIN_ {
        [TestMethod("MIN(3)")]
        public void Test1() {
            var min = new MIN(3);
            var results = min.Next(double.NaN, 100, 80, 60, 70, 50);

            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual(double.NaN, results[2]);
            Assert.AreEqual(60, results[3]);
            Assert.AreEqual(60, results[4]);
            Assert.AreEqual(50, results[5]);
        }

    }
}
