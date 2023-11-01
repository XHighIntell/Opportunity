global using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blite.Indicators;
using Blite.Spot;
using System.Diagnostics;

#pragma warning disable CS1998 
namespace OpportunityT {
    public class Program {

        public static async Task Main(string[] args) {


            //await new BliteClientT().SubscribeToBookTicker2();

            await new Blite.Spot.AccountT().SubscribeToUserDataAsync_LongTest();

            while (true) {
                Console.ReadLine();
                Console.Clear();
            }
            
        }
    }
}

