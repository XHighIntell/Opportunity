namespace Blite.Indicators;
///<summary>
///<para>Smoothed Moving Average (SMMA) is another popular and widely used moving average indicator.</para>
///<para> As all the other moving average indicators, to achieve the goals, the indicator filters
///  out the market fluctuations(noises) by averaging the price values of the periods, over which it is calculated.</para>
///</summary>
public class SMMA {

    ///<summary>Initializes a new instance of the <see cref="SMMA"/> class with specified length.</summary>
    public SMMA(int period) {
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

        var capacity = period - 1;
        if (sources.Count > capacity) sources.RemoveRange(0, sources.Count - capacity);
        if (results.Count > capacity) results.RemoveRange(0, results.Count - capacity);

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


        // 1. smma[1 to period - 1] = NaN
        // 2.
        //      a. if smma[i - 1] === NaN
        //          smma[i] = (value[i] + value[i - 1] + value[i - 2] ... + value[i - period + 1]) / period
        //      b. if smma[i - 1] !== NaN
        //          smma[i] = (smma[i - 1] * (period -1) + value) / period


        sources.Add(value);

        double nextResult;
        if (sources.Count < period) nextResult = double.NaN;
        else {
            if (sources.Count != period) throw new Exception("number of sources is not equal period");

            var previousResult = results[^1];

            if (double.IsNaN(previousResult) == true) {
                nextResult = 0d;

                for (var i = sources.Count - period; i < sources.Count; i++) {
                    nextResult += sources[i] / period;
                    if (double.IsNaN(nextResult) == true) break;
                }
            }
            else nextResult = (previousResult * (period - 1) + value) / period;   
        }

        sources.RemoveAt(sources.Count - 1);
        return nextResult;
    }
}

