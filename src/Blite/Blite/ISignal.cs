using Binance.Net.Enums;

namespace Blite;

public interface ISignal {
    ///<summary>Gets signal for buy or sell. If the signal is unclearly, null is returned.</summary>
    public OrderSide? GetSignal();
}

