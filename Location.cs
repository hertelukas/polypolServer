using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace polypolServer{
    public class Location{
        public MongoDB.Bson.ObjectId id { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public float visitors { get; set; }
        public int value { get; set; }
        public int beds { get; set; }
        public int __v { get; set; }
    }
}