using System.Collections.Generic;

namespace polypolServer{

    public static class Data{

        private static int Month = -1;
        private static int Year = 1980;
        private static string[] months = {"January","February","March","April","May","June","July", "August","September","October","November","December"};

        public static void NewCalculation(){
            if(Month < 12){
                Month++;
            }
            else{
                Month = 0;
                Year++;
            }
            System.Console.WriteLine($"===========================\n{months[Month]}/{Year.ToString()}\n===========================");
        }
        public static string GetDate()
        {
            return $"{months[Month]}/{Year.ToString()}";
        }
        
        public static int GetMonth(){
            return Month;
        }

        public static void SetYear(int year){
            Year = year;
        }

        public static void SetMonth(int month){
            if(month > months.Length - 1){
                throw new System.IndexOutOfRangeException();
            }else{
                Month = month;
            }
        }

        public static bool CheckIfYesNo(string input, out bool? result){
            input = input.ToLower();
            if(input == "yes" ||  input == "y"){
                result = true;
                return true;

            }
            else if(input == "no" || input == "n"){
                result = false;
                return true;
            }
            result = null;
            return false;
        }

    }
}