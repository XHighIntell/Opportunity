namespace Blite.Indicators {
    [TestClass]
    public class SMMA_ {
        [TestMethod("SMMA(3)")]
        public void Test1() {
            var smma = new SMMA(3);
            var results = smma.Next(double.NaN, 100, 80, 60, 70, 50, 30);

            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual(double.NaN, results[2]);
            Assert.AreEqual(80d, results[3]);
            Assert.AreEqual(76.666666666666671, results[4]);
            Assert.AreEqual(67.777777777777786, results[5]);
            Assert.AreEqual(55.18518518518519, results[6]);
        }

    }
}
