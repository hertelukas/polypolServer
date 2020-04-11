using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Linq;

namespace polypolServer
{
    class Program
    {
        static void Main(string[] args)
        {
            MongoClient dbClient = new MongoClient("mongodb+srv://admin:Fz05cKoP4PPx@polypol-i4wle.mongodb.net/test?retryWrites=true&w=majority");
            var database = dbClient.GetDatabase("test");

            UpdateBranches(database);
        }

        private static void UpdateBranches(IMongoDatabase database){

            //Downloading all the data

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


            List<Branch> updatedBranches = new List<Branch>();

            foreach (var item in cityBranches)
            {
                updatedBranches.AddRange(Calculator.CalculateProfit(item.Value, item.Key));
            }

            foreach (var branch in updatedBranches)
            {
                System.Console.WriteLine($"Updating branch in {branch.city}. Made a profit of ${branch.profit[branch.profit.Count - 1]}");
                var update = Builders<BsonDocument>.Update.Set("profit", branch.profit);
                var filter = Builders<BsonDocument>.Filter.Eq("_id", branch.id);
                branchesBson.UpdateOne(filter, update);
            }     
        }
    }
}
