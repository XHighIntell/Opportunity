namespace Blite.Indicators;

///<summary>
///<para>
/// The relative strength index (RSI) is a momentum indicator used in technical analysis. RSI measures the speed and magnitude of a security's recent price changes to evaluate overvalued or undervalued conditions in the price of that security.
///</para>
/// https://www.investopedia.com/terms/r/rsi.asp
///</summary>
public class RSI {

    ///<summary>Initializes a new instance of the <see cref="RSI"/> class with default length (14).</summary>
    public RSI() : this(14) { }

    ///<summary>Initializes a new instance of the <see cref="RSI"/> class with specified length.</summary>
    public RSI(int period) {
        Period = period;
        this.sources = new(period + 1);
        this.gainAvg = new(period);
        this.lossAvg = new(period);
    }

    readonly List<double> sources;
    readonly SMMA gainAvg;
    readonly SMMA lossAvg;
    
    ///<summary>Gets the length of this indicator.</summary>
    public int Period { get; private set; }


    ///<summary>Gets the current result of the indicator from the value and push the value to the stack.</summary>
    public double Next(double value) {


        var change = value - (sources.Count > 0 ? sources[^1] : double.NaN);
        var gain = double.IsNaN(change) ? double.NaN : change > 0 ? change : 0;
        var loss = double.IsNaN(change) ? double.NaN : change < 0 ? -change : 0;

        var avgGain = this.gainAvg.Next(gain);
        var avgLoss = this.lossAvg.Next(loss);
        var RS = avgGain / avgLoss;
        var nextResult = 100 - 100 / (1 + RS);

        sources.Add(value);
        if (sources.Count > Period) sources.RemoveRange(0, sources.Count - Period);

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
        var change = value - (sources.Count > 0 ? sources[^1] : double.NaN);
        var gain = double.IsNaN(change) ? double.NaN : change > 0 ? change : 0;
        var loss = double.IsNaN(change) ? double.NaN : change < 0 ? -change : 0;

        var avgGain = gainAvg.Moment(gain);
        var avgLoss = lossAvg.Moment(loss);
        var RS = avgGain / avgLoss;
        var nextResult = 100 - 100 / (1 + RS);

        return nextResult;
    }

}

