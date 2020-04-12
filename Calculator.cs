using System.Collections.Generic;
using System;

namespace polypolServer{
    public static class Calculator{


        public static List<Location> locations = new List<Location>();
        public static Dictionary<MongoDB.Bson.ObjectId, float> profits = new Dictionary<MongoDB.Bson.ObjectId, float>();
        public static List<Branch> CalculateProfit(List<Branch> branches, string city){
            Location location = locations.Find(x => x.city == city);

            int beds = 0;
            float demand = 0;
            int value = 0;

            if(!float.TryParse(location.visitors, out demand)){
                //Log Error
            }
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

            if(!int.TryParse(location.value, out value)){
                //Log Error
            }

            foreach (var branch in branches)
            {
                int tempStars = 0;
                int tempBeds = 0;
                if(int.TryParse(branch.beds, out tempBeds)){
                    beds += tempBeds;
                }
                else{
                    //Log Error
                }

                if(!int.TryParse(branch.stars, out tempStars)){
                    //Log Error
                }
            }

            Random rnd = new Random();

            foreach (var branch in branches)
            {
                int tempStars = 0;
                int tempBeds = 0;

                if(!int.TryParse(branch.beds, out tempBeds)){
                    //Log Error
                }

                if(!int.TryParse(branch.stars, out tempStars)){
                    //Log Error
                }

                float supplyFine = 1;

                if((float)demand/beds < 1){
                    supplyFine = (float)beds/demand;
                }

                float factor = 0.25f * value + 1.5f * tempStars * value;


                float supply = Math.Clamp((float)demand/beds,0,1);


                float profit = 0.2f * (tempBeds * factor * supply - 0.3f * tempBeds * factor * supplyFine) * (float)(rnd.NextDouble() / 10 + 0.95);

                branch.profit.Add(profit.ToString());
                branch.lables.Add(Data.GetDate());
                profits.Add(branch.id, profit);
            }

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