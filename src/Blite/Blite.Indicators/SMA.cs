using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blite.Indicators {

    ///<summary>
    ///<para>
    /// An Exponential Moving Average adds more weight to recent data (which makes older data less important).
    /// It uses exponential weighting to favourite recent over older data. This makes the average respond quicker to new prices than, say, a simple moving average.
    ///</para>
    ///https://www.tradingcode.net/tradingview/exponential-moving-average
    ///</summary>
    public class SMA {

        ///<summary>Initializes a new instance of the <see cref="SMA"/> class with specified length.</summary>
        public SMA(int period) {
            if (period < 2) throw new Exception("period must be greater than or equal to 2.");

            sources = new(period + 1);
            results = new(period + 1);
            this.Period = period;
        }

        readonly List<double> sources;
        readonly List<double> results;

        ///<summary>Gets the length of this indicator.</summary>
        public int Period { get; private set; }

        ///<summary>Gets the current result of the indicator from the value and push the value to the stack.</summary>
        public double Next(double value) {
            var nextResult = this.Moment(value);
            var period = Period;
            

            sources.Add(value);
            results.Add(nextResult);

            if (sources.Count > period) sources.RemoveRange(0, sources.Count - period);
            if (results.Count > period) results.RemoveRange(0, results.Count - period);

            return nextResult;
        }

        ///<summary>Gets results for a range of values and pushes them to the stack.</summary>
        public double[] Next(params double[] values) {
            var array = new double[values.Length];
            for (var i = 0; i < values.Length; i++) array[i] = Next(values[i]);
            return array;
        }
        public IEnumerable<double> INext(params double[] values) {
            for (var i = 0; i < values.Length; i++) yield return Next(values[i]);
        }

        ///<summary> Calculating the instantaneous value of the indicator allows you to obtain information about the indicator readings in real time, without affecting future readings.</summary>
        public double Moment(double value) {
            var period = Period;


            sources.Add(value);

            // 1. sma[1 to period - 1] = NaN
            // 2. 
            //      a. if sma[i - 1] === NaN
            //          sma[i] = (value[i] + value[i - 1] + value[i - 2] ... + value[i - period + 1]) / period
            //      b. if sma[i - 1] !== NaN
            //          sma[i] = sma[i - 1] + (value[i] - value[i - period]) / period

            double nextResult;

            if (sources.Count < period) nextResult = double.NaN;
            else {
                var previousResult = results[^1];

                if (double.IsNaN(previousResult) == true) {
                    nextResult = 0d;
                    for (var i = sources.Count - period; i < sources.Count; i++) {
                        nextResult += sources[i] / period;

                        if (double.IsNaN(nextResult) == true) break;
                    }
                }
                else {
                    nextResult = previousResult + (value - sources[sources.Count - 1 - period]) / period;
                }
            }

            sources.RemoveAt(sources.Count - 1);
            return nextResult;
        }
    }
}
