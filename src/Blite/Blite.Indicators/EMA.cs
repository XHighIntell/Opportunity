namespace Blite.Indicators;

///<summary>
/// <para>An Exponential Moving Average adds more weight to recent data (which makes older data less important).
/// It uses exponential weighting to favourite recent over older data. This makes the average respond quicker to new prices than, say, a simple moving average.
/// </para>
/// https://www.tradingcode.net/tradingview/exponential-moving-average
///</summary>
public class EMA {
    ///<summary>Initializes a new instance of the <see cref="EMA"/> class with specified period.</summary>
    public EMA(int period) {
        if (period < 2) throw new Exception("period must be greater than or equal to 2.");

        sources = new(period + 1);
        results = new(period + 1);
        alpha = 2d / (period + 1d);
        Period = period;
    }

    readonly List<double> sources;
    readonly List<double> results;
    readonly double alpha;

    ///<summary>Gets the length of this indicator.</summary>
    public int Period { get; }

    #region Methods
    ///<summary>Gets the current result of the indicator from the value and push the value to the stack.</summary>
    public double Next(double value) {
        var nextResult = Moment(value);
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

        sources.Add(value);
        // 1. ema[0 to period - 1] = NaN
        // 2.
        //     ema[i] = (value[i] + value[i - 1] + value[i - 2] ... + value[i - period + 1]) / period
        //     ema[i] = alpha * value + (1 - alpha) * ema[i - 1]

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
    #endregion
}

