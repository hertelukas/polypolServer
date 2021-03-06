using System.Collections.Generic;

namespace polypolServer{
    public class User{
        public MongoDB.Bson.ObjectId id { get; set; }
        public List<MongoDB.Bson.ObjectId> branches { get; set; }
        public List<string> labels { get; set; }

        public List<string> profit { get; set; }
        public List<string> staff { get; set; }
        public List<string> interior { get; set; }
        public double cash { get; set; }
        public string date { get; set; }
        public List<double> netWorth { get; set; }
        public double toPay { get; set; }
        public double loansRemaining { get; set; }
        public bool accountant { get; set; }
        public bool salesperson { get; set; }
        public bool banker { get; set; }
    }
}