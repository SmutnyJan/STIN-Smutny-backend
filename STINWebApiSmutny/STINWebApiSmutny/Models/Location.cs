namespace STINWebApiSmutny.Models
{
    public class LocalNames
    {
        public string cs { get; set; }
    }

    public class Location
    {
        public string name { get; set; }
        public LocalNames local_names { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string country { get; set; }
        public string state { get; set; }
    }
}
