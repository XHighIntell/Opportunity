namespace Blite.Indicators;

///<summary>The largest of values in specified length.</summary>
public class MAX {

    ///<summary>Initializes a new instance of the <see cref="MAX"/> class with specified length.</summary>
    public MAX(int period) {
        if (period < 2) throw new Exception("period must be greater than or equal to 2.");

        sources = new(period + 1);
        results = new(period + 1);
        this.Period = period;
    }

    readonly List<double> sources;
    readonly List<double> results;

    ///<summary>Gets the length of this indicator.</summary>
    public int Period { get; }

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

        sources.Add(value);
        double nextResult;

        if (sources.Count < period) nextResult = double.NaN;
        else {
            nextResult = sources[^period];
            for (var i = sources.Count - period + 1; i < sources.Count; i++) {
                var x = sources[i];
                if (double.IsNaN(x)) {
                    nextResult = double.NaN;
                    continue;
                }
                if (x > nextResult) nextResult = x;
            }
        }

        sources.RemoveAt(sources.Count - 1);
        return nextResult;
    }
}

