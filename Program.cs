using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Linq;

namespace polypolServer
{
    class Program
    {
        static void Main(string[] args)
        {
#region Getting all data
            MongoClient dbClient = new MongoClient("mongodb+srv://admin:Fz05cKoP4PPx@polypol-i4wle.mongodb.net/test?retryWrites=true&w=majority");

            var dbList = dbClient.ListDatabaseNames().ToList();

            var database = dbClient.GetDatabase("test");
            var branchesBson = database.GetCollection<BsonDocument>("branches");
            var branchesBsonList = branchesBson.Find(new BsonDocument()).ToList();

            List<Branch> branches = new List<Branch>();
            Dictionary<string, List<Branch>> cityBranches = new Dictionary<string, List<Branch>>();

            foreach (var doc in branchesBsonList)
            {
                branches.Add(BsonSerializer.Deserialize<Branch>(doc));
            }

            foreach (var branch in branches)
            {
                if(!cityBranches.ContainsKey(branch.city)){
                    cityBranches.Add(branch.city, new List<Branch>());
                }
                cityBranches[branch.city].Add(branch);
            }

            var locationsBson = database.GetCollection<BsonDocument>("locations");
            var locationsBsonList = locationsBson.Find(new BsonDocument()).ToList();
            
            foreach (var location in locationsBsonList)
            {
                location.Remove("branches");
                Calculator.locations.Add(BsonSerializer.Deserialize<Location>(location));
            }
#endregion


            var tempCityBranches = cityBranches.ToDictionary(entry => entry.Key, entry => entry.Value);

            foreach (var item in cityBranches)
            {
                tempCityBranches[item.Key] = Calculator.CalculateProfit(item.Value, item.Key);
            }

            cityBranches = tempCityBranches;
        }
    }
}
