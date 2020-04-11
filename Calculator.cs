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

                branch.profit.Add((value * tempStars).ToString());
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