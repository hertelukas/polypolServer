using System.Collections.Generic;
using System;

namespace polypolServer{
    public static class Calculator{

        public static List<Location> locations = new List<Location>();
        public static List<Branch> CalculateProfit(List<Branch> branches, string city){
            Location location = locations.Find(x => x.city == city);

            int beds = 0;
            int demand = 0;
            int value = 0;

            if(!int.TryParse(location.visitors, out demand)){
                //Log Error
            }
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

                float supply = Math.Clamp((float)demand/int.Parse(branch.beds),0,1);
                if(supply > 1){
                    supply *= 1.75f;
                }

                float factor = 0.25f * value + 1.5f * tempStars * value;

                System.Console.WriteLine($"Factor is {factor} and supply is {supply}");

                branch.profit.Add((0.2 * (int.Parse(branch.beds) * factor * supply - 0.3f * int.Parse(branch.beds) * factor)).ToString());
            }

            PlotCity(city, beds, demand);

            return branches;
        }

        private static void PlotCity(string city, int beds, int demand){
            System.Console.WriteLine($"{city}: \nBeds:   {beds} \nDemand: {demand}");    

            if((float)beds/demand > 1){
                Console.ForegroundColor = ConsoleColor.Red;
            }else{
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }

            System.Console.WriteLine($"Supply: {(float)beds/demand * 100}%\n");

            Console.ForegroundColor = ConsoleColor.White;
        }
    }

}