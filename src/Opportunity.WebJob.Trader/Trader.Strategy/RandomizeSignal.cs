using Binance.Net.Enums;
using Blite;

namespace Opportunity.WebJob.Trader.Strategy;

public class RandomizeSignal: ISignal, IDisposable {

    public OrderSide? GetSignal() {
        if (Random.Shared.NextDouble() > 0.5d) {
            return OrderSide.Buy;
        }
        else {
            return OrderSide.Sell;
        }
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }
}

