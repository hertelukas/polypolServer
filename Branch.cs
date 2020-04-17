using System.Collections.Generic;

namespace polypolServer{
    public class Branch{
        public MongoDB.Bson.ObjectId id { get; set; }
        public List<string> lables { get; set; }
        public List<string> profit { get; set; }
        public List<string> staff { get; set; }
        public List<string> interior { get; set; }
        public string city { get; set; }
        public string beds { get; set; }
        public string stars { get; set; }
        public string priceFactor { get; set; }
        public int __v { get; set; }
    }
}
