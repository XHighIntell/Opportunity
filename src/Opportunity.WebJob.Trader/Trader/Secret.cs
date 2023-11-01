using CryptoExchange.Net.Authentication;

namespace Opportunity.WebJob.Trader;

public static class Secret {
    public static class Spot {
        ///<summary>Testnet</summary>
        public static ApiCredentials Testnet { get; } = new("sxYFKIv3AP2HN5cAASBq0eJZ1MvgCWOoVVUN0iHmQ9fWunEZedlLlJWS8hQ6hRQk", "4TWDkMrigZsZ5782ky0DsmLbNs7s85AflTUmMNYEhDzu8dahHSWqSMHFr9RDF5NO");


        /////<summary>Azure Traders</summary>
        //public static ApiCredentials Live { get; } new("", "");

    }
}

