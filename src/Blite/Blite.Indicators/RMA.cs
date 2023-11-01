namespace Blite.Indicators {

    ///<summary>
    /// <para>A Relative Moving Average adds more weight to recent data(and gives less importance to older data). <br></br>
    /// This makes the RMA similar to the Exponential Moving Average, although it’s somewhat slower to respond than an EMA is.</para>
    /// https://www.tradingcode.net/tradingview/relative-moving-average
    ///</summary>
    public class RMA {

        ///<summary>Initializes a new instance of the <see cref="RMA"/> class with specified length.</summary>
        public RMA(int period) {
            if (period < 2) throw new Exception("period must be greater than or equal to 2.");

            sources = new(period + 1);
            results = new(period + 1);
            Period = period;
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

        public IEnumerable<double> INext(params double[] values) { for (var i = 0; i < values.Length; i++) yield return Next(values[i]); }

        ///<summary> Calculating the instantaneous value of the indicator allows you to obtain information about the indicator readings in real time, without affecting future readings.</summary>
        public double Moment(double value) {
            var period = Period;
            double alpha = 1d / period;

            sources.Add(value);


            // rma[1 to period] = (sources[0] + sources[1] + ...) / period
            // rma[i] = alpha * source + (1 - alpha) * ema[i - 1]

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
                else
                    nextResult = alpha * value + (1d - alpha) * previousResult;
            }

            sources.RemoveAt(sources.Count - 1);
            return nextResult;
        }
    }
}
