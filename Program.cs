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
        static private int? lastHour;
        static void Main(string[] args)
        {
            System.Console.WriteLine("Input the beginning year: ");
            int year;
            while(!int.TryParse(Console.ReadLine(), out year)){
                Console.WriteLine("Input invalid.");
            }
            Data.SetYear(year);

            System.Console.WriteLine("Do you want to restart the server? [yes/no]");
            bool? update;
            while(!Data.CheckIfYesNo(Console.ReadLine(), out update)){
                Console.WriteLine("Input invalid.");
            }

            if(update.GetValueOrDefault()){
                int month;
                System.Console.WriteLine("Input the month to start at: ");
                while(!int.TryParse(Console.ReadLine(), out month)){
                    Console.WriteLine("Input invalid.");
                }
                Data.SetMonth(month - 2);
            }

            Console.WriteLine("Update rate of the server: (To run the server in production mode type 0)");
            float minutes = 30f;
            while(!float.TryParse(Console.ReadLine(), out minutes)){
                Console.WriteLine("Input invalid.");
            }
            if(minutes == 0){
                System.Console.WriteLine("Updating the server every full hour.");
                var timer = new System.Timers.Timer(5000);
                lastHour = DateTime.Now.Hour;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
                timer.Start();

            }
            else if(minutes > 0){
                System.Console.WriteLine("Updating every " + minutes + " minutes");
                var timer = new System.Timers.Timer(minutes * 60 * 1000);
                timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
                timer.Start();
                OnTimedEvent(null, null);
            }
            Console.ReadLine();
        }

        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e){
            if(!lastHour.HasValue || (lastHour < DateTime.Now.Hour || lastHour == 23 && DateTime.Now.Hour == 0)){
                if(lastHour.HasValue) lastHour = DateTime.Now.Hour;
                Data.NewCalculation();
                Calculator.profits.Clear();
                MongoClient dbClient = new MongoClient("mongodb+srv://admin:Fz05cKoP4PPx@polypol-i4wle.mongodb.net/test?retryWrites=true&w=majority");
                // MongoClient dbClient = new MongoClient("mongodb://localhost:27017/zivi?readPreference=primary&appname=MongoDB%20Compass&ssl=false");
                var database = dbClient.GetDatabase("test");
                // var database = dbClient.GetDatabase("game");
                UpdateBranches(database);
                UpdateLocations(database);
                UpdateUsers(database);
                CleanUp();
            }

        }

        private static void UpdateBranches(IMongoDatabase database){

            //Downloading all the data
            var branchesBson = database.GetCollection<BsonDocument>("branches");
            var branchesBsonList = branchesBson.Find(new BsonDocument()).ToList();

            List<Branch> branches = new List<Branch>();
            Dictionary<string, List<Branch>> cityBranches = new Dictionary<string, List<Branch>>();

            foreach (var doc in branchesBsonList)
            {
                doc.Remove("onSale");
                doc.Remove("salePrice");
                branches.Add(BsonSerializer.Deserialize<Branch>(doc));
            }

            foreach (var branch in branches)
            {
                if(!branch.renovation.HasValue){
                    branch.renovation = 240;
                }
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
                location.Remove("latitude");
                location.Remove("longitude");
                location.Remove("region");
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

                var updateTaxes = Builders<BsonDocument>.Update.Set("taxes", branch.taxes);
                branchesBson.UpdateOne(filter, updateTaxes);

                var updateRenovation = Builders<BsonDocument>.Update.Set("renovation", branch.renovation);
                branchesBson.UpdateOne(filter, updateRenovation);
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
            var branchesBson = database.GetCollection<BsonDocument>("branches");
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
                doc.Remove("dark");
                doc.Remove("invitesRemaining");
                doc.Remove("invitedby");
                users.Add(BsonSerializer.Deserialize<User>(doc));
            }

            foreach (var user in users)
            {
                double tempProfit = 0;
                double taxes = 0;

                foreach (var branch in user.branches)
                {
                    tempProfit += Calculator.profits.GetValueOrDefault(branch);
                    taxes += Calculator.taxes.GetValueOrDefault(branch);
                }

                user.profit.Add(tempProfit.ToString());
                user.labels.Add(Data.GetDate());

                user.cash += tempProfit;
                user.netWorth.Add(user.netWorth[user.netWorth.Count - 1] += tempProfit);
                if(user.netWorth[user.netWorth.Count - 1] > 10000000){
                    user.cash -= taxes;
                    user.netWorth[user.netWorth.Count - 1] -= taxes;
                }
                else{
                    foreach (var branch in user.branches)
                    {
                        var branchesFilter = Builders<BsonDocument>.Filter.Eq("_id", branch);
                        var updateTaxes = Builders<BsonDocument>.Update.Set("taxes", new List<double>());
                        branchesBson.UpdateOne(branchesFilter, updateTaxes);
                    }
                }

                Calculator.CutList(user.netWorth);
                Calculator.CutList(user.labels);
                Calculator.CutList(user.profit);

                var filter = Builders<BsonDocument>.Filter.Eq("_id", user.id);
                var updateCash = Builders<BsonDocument>.Update.Set("cash", user.cash);
                usersBson.UpdateOne(filter, updateCash);

                var updateLabel = Builders<BsonDocument>.Update.Set("labels", user.labels);
                usersBson.UpdateOne(filter, updateLabel);

                var updateProfit = Builders<BsonDocument>.Update.Set("profit", user.profit);
                usersBson.UpdateOne(filter, updateProfit);

                var updateNet = Builders<BsonDocument>.Update.Set("netWorth", user.netWorth);
                usersBson.UpdateOne(filter, updateNet);

            }
        }

        private static void CleanUp(){
            Calculator.locations.Clear();
            Calculator.profits.Clear();
            System.Console.WriteLine("Updated everything.");
        }
    }
}
