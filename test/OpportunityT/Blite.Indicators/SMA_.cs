using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Blite.Indicators {
    [TestClass]
    public class SMA_ {
        [TestMethod("SMA(2)")]
        public void Test1() {
            var sma = new SMA(2);
            var results = sma.Next(double.NaN, 1, 2, 4, 8, 16, 32, 64);

            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual(1.5d, results[2]);
            Assert.AreEqual(3d, results[3]);
            Assert.AreEqual(6d, results[4]);
            Assert.AreEqual(12d, results[5]);
            Assert.AreEqual(24d, results[6]);
            Assert.AreEqual(48d, results[7]);
        }

        [TestMethod("SMA(3)")]
        public void Test2() {
            var sma = new SMA(3);
            var results = sma.Next(1, 2, 4, 8, 16, 32, 64);
            var epsilon = 0.000_000_000_000_01d;

            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual((1d + 2d + 4d) / 3d, results[2], epsilon);
            Assert.AreEqual((2d + 4d + 8d) / 3d, results[3], epsilon);
            Assert.AreEqual((4d + 8d + 16d) / 3d, results[4], epsilon);
            Assert.AreEqual((8d + 16d + 32d) / 3d, results[5], epsilon);
            Assert.AreEqual((16d + 32d + 64d) / 3d, results[6], epsilon);
        }
    }
}
