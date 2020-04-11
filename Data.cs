using System.Collections.Generic;

namespace polypolServer{

    public static class Data{

        private static int month = 0;
        private static int year = 1980;
        private static string[] months = {"January","February","March","April","May","June","July", "August","September","October","November","December"};

        public static void NewCalculation(){
            if(month < 12){
                month++;
            }
            else{
                month = 0;
                year++;
            }
        }
        public static string GetDate()
        {
            return $"{months[month]}/{year.ToString()}";
        }


    }
}