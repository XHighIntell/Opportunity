using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Blite.Indicators {
    [TestClass]
    public class RMA_ {

        //private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        //public TestContext TestContext {
        //    get { return testContextInstance; }
        //    set { testContextInstance = value; }
        //}

        [TestMethod("RMA(2)")]
        public void Test1() {
            var rma = new RMA(2);
            var results = rma.Next(double.NaN, 1, 2, 4, 8, 16, 32, 64);

            var alpha = 1d / 2d;
            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual(1.5, results[2]);
            Assert.AreEqual(alpha * 4 + (1 - alpha) * results[2], results[3]);
            Assert.AreEqual(alpha * 8 + (1 - alpha) * results[3], results[4]);
            Assert.AreEqual(alpha * 16 + (1 - alpha) * results[4], results[5]);
            Assert.AreEqual(alpha * 32 + (1 - alpha) * results[5], results[6]);
            Assert.AreEqual(alpha * 64 + (1 - alpha) * results[6], results[7]);
        }

        [TestMethod("RMA(3)")]
        public void Test2() {
            var rma = new RMA(3);
            var results = rma.Next(1, 2, 4, 8, 16, 32, 64);
            var epsilon = 0.000_000_000_000_01d;

            var alpha = 1d / 3d;
            Assert.AreEqual(double.NaN, results[0]);
            Assert.AreEqual(double.NaN, results[1]);
            Assert.AreEqual((1 + 2 + 4) / 3d, results[2], epsilon);
            Assert.AreEqual(alpha * 8 + (1 - alpha) * results[2], results[3]);
            Assert.AreEqual(alpha * 16 + (1 - alpha) * results[3], results[4]);
            Assert.AreEqual(alpha * 32 + (1 - alpha) * results[4], results[5]);
            Assert.AreEqual(alpha * 64 + (1 - alpha) * results[5], results[6]);

            Console.Write("test console");
        }
    }
}
