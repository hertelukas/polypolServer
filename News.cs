using System.Collections.Generic;

namespace polypolServer{
    public class News{
        public List<string> region { get; set; }
        public string city { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public string date { get; set; }
        public double change { get; set; }
    }
}