using System.Collections.Generic;

namespace polypolServer{
    public class Branch{
        public MongoDB.Bson.ObjectId id { get; set; }
        public List<string> lables { get; set; }
        public List<float> profit { get; set; }
        public List<float> staff { get; set; }
        public List<float> interior { get; set; }
        public string city { get; set; }
        public int beds { get; set; }
        public int stars { get; set; }
        public float priceFactor { get; set; }
        public int? renovation { get; set; }
        public int __v { get; set; }
    }
}
