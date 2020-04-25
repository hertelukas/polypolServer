using System.Collections.Generic;
using System;

namespace polypolServer{
    public static class Calculator{


        public static List<Location> locations = new List<Location>();
        public static Dictionary<MongoDB.Bson.ObjectId, double> profits = new Dictionary<MongoDB.Bson.ObjectId, double>();
        public static List<Branch> CalculateProfit(List<Branch> branches, string city){
            Location location = locations.Find(x => x.city == city);

            int beds = 0;
            float demand = location.visitors;
            int value = location.value;

            float monthBonus = 1f;

            switch (Data.GetMonth())
            {
                case 0:
                    monthBonus = 0.9f;
                    break;
                
                case 1:
                    monthBonus = 0.85f;
                    break;
                
                case 4:
                    monthBonus = 1.1f;
                    break;

                case 5: 
                    monthBonus = 1.35f;
                    break;

                case 6:
                    monthBonus = 1.3f;
                    break;

                case 7:
                    monthBonus = 1.25f;
                    break;

                case 8: 
                    monthBonus = 1.25f;
                    break; 

                case 10:
                    monthBonus = 0.95f;
                    break;

                default:
                    monthBonus = 1f;
                    break;
            }
            demand *= monthBonus;

            //TODO write Logging script

            foreach (var branch in branches)
            {
                beds += branch.beds; 

            }

            Random rnd = new Random();

            foreach (var branch in branches)
            {
                float supplyFine = 1;

                if((float)demand/beds < 1){
                    supplyFine = (float)beds/demand;
                }

                float factor = 0.25f * value + 1.5f * branch.stars * value;


                float supply = Math.Clamp((float)demand/beds,0,1);

                if(branch.priceFactor < 0.4f){
                    branch.priceFactor = 1;
                }

                //TODO Change the number of tourists per year based on the year. (Data lookup required)
                //TODO Find a way to dynamically change the number of global tourists (maybe something like a factor)

                double staffExpenses = branch.beds * (Math.Sqrt(branch.stars + 1) * 20);
                double interiorExpenses = branch.beds * (Math.Pow(branch.stars, 0.3) * 3);

                double profit = 0.2f * (branch.beds * factor * supply - Math.Pow(branch.priceFactor, 2) * 0.3f * branch.beds * factor * supplyFine) * 
                (rnd.NextDouble() / 10 + 0.95) * branch.priceFactor - staffExpenses - interiorExpenses;

                branch.renovation -= 1;

                //Punish not renovating
                if(branch.renovation < 0){
                    if(profit > 0){
                        if(branch.renovation < -120){
                            profit = 0;
                        }else{
                            profit /= 2;
                        }
                    }else{
                        profit *= 2;
                    }
                }

                branch.profit.Add((float)profit);
                branch.lables.Add(Data.GetDate());
                branch.staff.Add((float)staffExpenses);
                branch.interior.Add((float)interiorExpenses);
                profits.Add(branch.id, profit);

            }

            location.beds = beds;

            PlotCity(city, beds, demand);
            return branches;
        }

        private static void PlotCity(string city, int beds, float demand){
            System.Console.WriteLine($"{city}: \nBeds:   {beds} \nDemand: {demand}");

            if((float)demand/beds < 1){
                Console.ForegroundColor = ConsoleColor.Red;
            }else{
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }

            System.Console.WriteLine($"Supply: {(float)demand/beds * 100}%\n");

            Console.ForegroundColor = ConsoleColor.White;
        }

    }

}