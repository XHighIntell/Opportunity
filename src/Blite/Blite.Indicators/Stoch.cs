namespace Blite.Indicators;

///<summary>
///<para>
/// A stochastic oscillator is a momentum indicator comparing a particular closing price of a security to a range of its prices over a certain period of time. The sensitivity of the oscillator to market movements is reducible by adjusting that time period or by taking a moving average of the result. It is used to generate overbought and oversold trading signals, utilizing a 0–100 bounded range of values.
///</para>
///https://www.investopedia.com/terms/s/stochasticoscillator.asp
///</summary>
public class Stoch {

    ///<summary>Initializes a new instance of the <see cref="Stoch"/> class with default length (14).</summary>
    public Stoch() : this(14) { }

    ///<summary>Initializes a new instance of the <see cref="Stoch"/> class with specified length.</summary>
    public Stoch(int period) {
        Period = period;
        this.min = new(period);
        this.max = new(period);
    }

    readonly MIN min;
    readonly MAX max;

    ///<summary>Gets the length of this indicator.</summary>
    public int Period { get; private set; }


    ///<summary>Gets the current result of the indicator from the value and push the value to the stack.</summary>
    public double Next(double close, double high, double low) {
        var min = this.min.Next(low);
        var max = this.max.Next(high);

        return (close - min) / (max - min) * 100;
    }

    ///<summary>Gets the current result of the indicator from the value and push the value to the stack.</summary>
    public double Next(StochInput value) {
        return Next(value.Close, value.High, value.Low);
    }

    ///<summary>Gets results for a range of values and pushes them to the stack.</summary>
    public double[] Next(params StochInput[] values) {
        var array = new double[values.Length];
        for (var i = 0; i < values.Length; i++) array[i] = Next(values[i]);
        return array;
    }

    //public IEnumerable<double> INext(params double[] values) { for (var i = 0; i < values.Length; i++) yield return Next(values[i]); }

    ///<summary> Calculating the instantaneous value of the indicator allows you to obtain information about the indicator readings in real time, without affecting future readings.</summary>
    public double Moment(double close, double high, double low) {
        var min = this.min.Moment(low);
        var max = this.max.Moment(high);

        return (close - min) / (max - min) * 100;
    }


    public struct StochInput {
        public StochInput() { Close = 0; High = 0; Low = 0; }
        public StochInput(double close, double high, double low) {
            Close = close;
            High = high;
            Low = low;
        }

        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }

        public static StochInput From(double close, double high, double low) {
            return new StochInput(close, high, low);
        }
    }

}

