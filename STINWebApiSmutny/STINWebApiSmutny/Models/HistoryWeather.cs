namespace STINWebApiSmutny.Models
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class CloudsHistory
    {
        public int all { get; set; }
    }

    public class List
    {
        public int dt { get; set; }
        public MainHistory main { get; set; }
        public WindHistory wind { get; set; }
        public CloudsHistory clouds { get; set; }
        public List<WeatherHistory> weather { get; set; }
    }

    public class MainHistory
    {
        public double temp { get; set; }
        public double feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
    }

    public class HistoryWeather
    {
        public string message { get; set; }
        public string cod { get; set; }
        public int city_id { get; set; }
        public double calctime { get; set; }
        public int cnt { get; set; }
        public DateTime DateTime { get; set; }
        public List<List> list { get; set; }
    }

    public class WeatherHistory
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class WindHistory
    {
        public double speed { get; set; }
        public int deg { get; set; }
        public double gust { get; set; }
    }


}
