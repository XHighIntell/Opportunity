using Blite.Spot;
using Opportunity.WebJob.Trader.Text.Json.Serialization;
using System.Text.Json.Serialization;

namespace Opportunity.WebJob.Trader.Api;

public static class AccountApi {
    public static Account GetAccount(string name) {
        var account = Application.Accounts.FirstOrDefault(a => a.Name == name);
        if (account == null) throw new Exception("Account not found.");

        return account;
    }

}

public static class AccountController {
    public static object GetAccounts() => Application.Accounts.Select(a => a.ToJsonObject()).ToArray();
    public static object GetAccount(string name) {
        
        return AccountApi.GetAccount(name).ToJsonObject();

        //return new {
        //    name = account.Name,
        //    strategies = account.Strategies.Select(s => new {
        //        name = s.Parameters.SYMBOL + nameof(StrategyDouble),
        //        //parameters = s.Parameters.ToJsonObject(),
        //    })
        //};

        //var jaccount = account.ToJsonObject();
        //return jaccount.ToJsonString();
    }
}

