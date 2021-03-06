using System.Collections.Generic;
using System;
using System.Linq;

namespace polypolServer{
    public static class Calculator{


        public static List<Location> locations = new List<Location>();
        public static Dictionary<MongoDB.Bson.ObjectId, double> profits = new Dictionary<MongoDB.Bson.ObjectId, double>();
        public static Dictionary<MongoDB.Bson.ObjectId, double> taxes = new Dictionary<MongoDB.Bson.ObjectId, double>();
        public static List<Branch> CalculateProfit(List<Branch> branches, string city){
            Location location = locations.Find(x => x.city == city);

            int beds = 0;
            float demand = (float)location.visitors;
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

                float occupancyFactor = 0.3f;
                if(branch.salesperson) occupancyFactor = 0.1f;

                double staffExpenses = branch.beds * Math.Sqrt(branch.stars + 1) * 60 * location.value / 1000;
                double interiorExpenses = branch.beds * Math.Pow(branch.stars, 0.3) * 7 * location.value / 1000;

                double profit = 0.2f * (branch.beds * factor * supply - Math.Pow(branch.priceFactor, 2) * occupancyFactor * branch.beds * factor * supplyFine) * 
                (rnd.NextDouble() / 10 + 0.95) * branch.priceFactor - staffExpenses - interiorExpenses;
                
                if(profit > 0) profit = profit * 0.5;


                double tax = 2 * profit * ((float)location.value / (10000 * ((branch.stars + 1) / 2)));
                if(tax > 0.5 * profit) tax = 0.5 * profit;
                if(profit < 0) tax  = 0;
                profit -= tax;

                if(profit < -1 * (tax + staffExpenses + interiorExpenses) && profit < 0) profit = 0 - tax - staffExpenses - interiorExpenses;

                branch.renovation -= 1;

                if(branch.renovation <= 0 && branch.autorenovate){
                    int tempRenovation = branch.renovation.GetValueOrDefault();
                    branch.renovation = 240;
                    var priceToRenovate = location.value * branch.stars * Math.Sqrt(tempRenovation) * branch.beds / 150 * 1.5;
                    profit -= priceToRenovate;
                }

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
                if(branch.taxes == null) branch.taxes = new List<double>();
                branch.taxes.Add((float)tax);
                branch.interior.Add((float)interiorExpenses);
                profits.Add(branch.id, profit);
                taxes.Add(branch.id, tax);

                //Remove all data older than 24 months
                CutList(branch.profit);
                CutList(branch.lables);
                CutList(branch.staff);
                CutList(branch.interior);
                CutList(branch.taxes);
            }

            location.beds = beds;

            PlotCity(city, beds, demand);
            return branches;
        }

        public static void CutList<T>(List<T> list){
            if(list.Count > 24){
                for (int i = list.Count - 25; i >= 0; i--)
                {
                    list.RemoveAt(i);
                }
            }
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