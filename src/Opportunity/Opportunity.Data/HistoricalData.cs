using System.Data.Common;
using System.Text;
using Microsoft.Data.SqlClient;
using Intell.Data;
using Intell.Data.SqlClient.Extension;
using Intell.Data.SqlClient;

namespace Opportunity.Data;

public class HistoricalData: DataBase {

    public HistoricalData() { }
    public HistoricalData(DbDataReader reader) : base(reader) { }

    #region Columns: symbol, time, funding_rate
    public string? Symbol {
        get { return this["symbol"] as string; }
        set { this["symbol"] = value; }
    }
    public long? Time {
        get { return (long?)this["time"]; }
        set { this["time"] = value; }
    }
    public decimal? FundingRate {
        get { return (decimal)this["funding_rate"]; }
        set { this["funding_rate"] = value; }
    }
    #endregion


    public static async Task<int> InsertAsync(SqlConnection connection, HistoricalData[] data) {
        var command = CommandGenerator.GenerateInsertCommand(data, "[MARKET].Historical", new[] { "symbol", "time", "funding_rate" });
        return await connection.ExecuteNonQueryAsync(command);
    }

    public static async Task<HistoricalData[]> FromCommandAllAsync(SqlConnection connection, string commandText) {
        var list = new List<HistoricalData>();
        var reader = await connection.ExecuteReaderAsync(commandText);
            
        while (await reader.ReadAsync() == true) 
            list.Add(new HistoricalData(reader));
            
        return list.ToArray();
    }
    public static async Task<HistoricalData[]> GetAsync(SqlConnection connection, string? symbol) {
        var sb = new StringBuilder();
        sb.Append("SELECT * FROM [MARKET].Historical");
        if (symbol != null) sb.Append(" WHERE symbol='" + symbol + "'");

        return await FromCommandAllAsync(connection, sb.ToString());
    }

}

