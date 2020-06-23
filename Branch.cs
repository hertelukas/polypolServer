using System.Collections.Generic;

namespace polypolServer{
    public class Branch{
        public MongoDB.Bson.ObjectId id { get; set; }
        public List<string> lables { get; set; }
        public List<double> profit { get; set; }
        public List<double> staff { get; set; }
        public List<double> interior { get; set; }
        public List<double> taxes { get; set; }
        public string city { get; set; }
        public int beds { get; set; }
        public int stars { get; set; }
        public double priceFactor { get; set; }
        public int? renovation { get; set; }
        public int __v { get; set; }
        public bool autorenovate { get; set; }
        public bool salesperson { get; set; }
    }
}
