using Microsoft.Extensions.Hosting;

namespace STINWebApiSmutny.Models
{
    public class User
    {
        public Int32 id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string pass { get; set; }
    }
}
