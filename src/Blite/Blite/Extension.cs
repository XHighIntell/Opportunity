using Binance.Net.Enums;
using Binance.Net.Objects.Models.Spot;

namespace Blite;

public static class BinanceSymbolExtension {
    public static decimal NormalizePrice(this BinanceSymbol symbolInfo, decimal price) {
        var priceFilter = symbolInfo.Filters.FirstOrDefault(o => o.FilterType == SymbolFilterType.Price) as BinanceSymbolPriceFilter;
        if (priceFilter == null) throw new Exception("can't find PRICE_FILTER");

        var decimals = (int)-Math.Log10((double)priceFilter.TickSize);
        return Math.Round(price, decimals);
    }

    public static decimal NormalizeQuantity(this BinanceSymbol symbolInfo, decimal quantity) {
        var lotSizeFilter = symbolInfo.Filters.FirstOrDefault(o => o.FilterType == SymbolFilterType.LotSize) as BinanceSymbolLotSizeFilter;
        if (lotSizeFilter == null) throw new Exception("can't find LOT_SIZE");

        var decimals = (int)-Math.Log10((double)lotSizeFilter.StepSize);
        return Math.Round(quantity, decimals);
    }
}

