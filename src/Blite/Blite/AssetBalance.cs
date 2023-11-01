namespace Blite;

public class AssetBalance {

    public AssetBalance() { }

    decimal _asset = 0;
    decimal _balance = 0;

    #region Properties

    ///<summary>When the asset is positive, the balance will be negative.</summary>
    public decimal Asset {
        get { return _asset; }
        set { _asset = value; } 
    }

    public decimal Balance { 
        get { return _balance; } 
        set { _balance = value; } 
    }
    #endregion

    #region Methods
    ///<summary>If the value of asset is positive, it is bought otherwise sold.</summary>
    public void Insert(decimal asset, decimal price) {
        _asset += asset;
        _balance += -asset * price;
    }

    ///<summary>Get unrealized profit or loss.</summary>
    public decimal GetUnrealizedPNL(decimal bestBidPrice, decimal bestAskPrice) {
        if (_asset > 0)
            return bestBidPrice * _asset + _balance;
        else
            return bestAskPrice * _asset + _balance;
    }
    #endregion
}

