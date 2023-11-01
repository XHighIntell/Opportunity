using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Blite.Indicators {
    [TestClass]
    public class MACD_ {
        [TestMethod("MACD(2, 3, 4)")]
        public void Test1() {
            var macd = new MACD(2, 3, 4);
            var results = macd.Next(144.61, 146.71, 165.01, 168.23, 173.36, 184.73, 178.52, 185.82, 178.89, 169.03, 173.71, 173.7, 176.07, 182.19, 178.83, 163.46, 159.96, 160.96, 153.61);

            Assert.AreEqual(double.NaN, results[0].Macd); Assert.AreEqual(double.NaN, results[0].Signal); Assert.AreEqual(double.NaN, results[0].Histogram);
            Assert.AreEqual(double.NaN, results[1].Macd); Assert.AreEqual(double.NaN, results[1].Signal); Assert.AreEqual(double.NaN, results[1].Histogram);
            Assert.AreEqual(6.449999999999989, results[2].Macd); Assert.AreEqual(double.NaN, results[2].Signal); Assert.AreEqual(double.NaN, results[2].Histogram);
            Assert.AreEqual(-2.1531977588404345, results[18].Macd); Assert.AreEqual(-1.7790326330670476, results[18].Signal); Assert.AreEqual(-0.3741651257733869, results[18].Histogram);
        }
    }
}
