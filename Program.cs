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
            Console.WriteLine("Update rate of the server: ");
            float minutes = 30f;
            while(!float.TryParse(Console.ReadLine(), out minutes)){
                Console.WriteLine("Input invalid.");
            }
            System.Console.WriteLine("Updating every " + minutes + " minutes");

            var timer = new System.Timers.Timer(minutes * 60 * 1000);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            timer.Start();
            OnTimedEvent(null, null);

            Console.ReadLine();
        }

        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e){
            Data.NewCalculation();
            Calculator.profits.Clear();
            MongoClient dbClient = new MongoClient("mongodb+srv://admin:Fz05cKoP4PPx@polypol-i4wle.mongodb.net/test?retryWrites=true&w=majority");
            var database = dbClient.GetDatabase("test");
            UpdateBranches(database);
            UpdateLocations(database);
            UpdateUsers(database);
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
                var updateProfit = Builders<BsonDocument>.Update.Set("profit", branch.profit);
                var filter = Builders<BsonDocument>.Filter.Eq("_id", branch.id);
                branchesBson.UpdateOne(filter, updateProfit);

                var updateLabel = Builders<BsonDocument>.Update.Set("lables", branch.lables);
                branchesBson.UpdateOne(filter, updateLabel);

                var updateStaff = Builders<BsonDocument>.Update.Set("staff", branch.staff);
                branchesBson.UpdateOne(filter, updateStaff);

                var updateInterior = Builders<BsonDocument>.Update.Set("interior", branch.interior);
                branchesBson.UpdateOne(filter, updateInterior);

            }     
        }

        private static void UpdateLocations(IMongoDatabase database){
            var locationsBson = database.GetCollection<BsonDocument>("locations");

            foreach (var location in Calculator.locations)
            {
                var updateBeds = Builders<BsonDocument>.Update.Set("beds", location.beds);
                var filter = Builders<BsonDocument>.Filter.Eq("_id", location.id);
                locationsBson.UpdateOne(filter, updateBeds);
            }
        }

        private static void UpdateUsers(IMongoDatabase database){
            var usersBson = database.GetCollection<BsonDocument>("users");
            var usersBsonList = usersBson.Find(new BsonDocument()).ToList();

            List<User> users = new List<User>();

            foreach (var doc in usersBsonList)
            {
                doc.Remove("mail");
                doc.Remove("username");
                doc.Remove("mailConfirmed");
                doc.Remove("currentCode");
                doc.Remove("salt");
                doc.Remove("hash");
                doc.Remove("__v");
                doc.Remove("chainName");
                doc.Remove("resetPasswordToken");
                doc.Remove("resetPasswordExpires");
                users.Add(BsonSerializer.Deserialize<User>(doc));
            }

            foreach (var user in users)
            {
                double tempProfit = 0;

                foreach (var branch in user.branches)
                {
                    tempProfit += Calculator.profits.GetValueOrDefault(branch);
                }


                user.profit.Add(tempProfit.ToString());
                user.labels.Add(Data.GetDate());
                double cash;

                if(!double.TryParse(user.cash, out cash)){
                    //Log Error
                }
                cash += tempProfit;
                user.cash = cash.ToString();

                var filter = Builders<BsonDocument>.Filter.Eq("_id", user.id);
                var updateCash = Builders<BsonDocument>.Update.Set("cash", user.cash);
                usersBson.UpdateOne(filter, updateCash);

                var updateLabel = Builders<BsonDocument>.Update.Set("labels", user.labels);
                usersBson.UpdateOne(filter, updateLabel);

                var updateProfit = Builders<BsonDocument>.Update.Set("profit", user.profit);
                usersBson.UpdateOne(filter, updateProfit);

            }
        }
    }
}
