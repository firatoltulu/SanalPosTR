namespace SimplePayTR.UI.Models
{
    public class AppConfig
    {
        public string SuccessRedirectUrl { get; set; }

        public string FailRedirectUrl { get; set; }

        public string RedisConnectionString { get; set; }
        public int? RedisDatabaseId { get; set; }
        public int RedisDefaultExpireTimeOut { get; set; }
        public string RedisPrefix { get; set; }
        public string SuccessEndPoint { get; set; }
        public string FailEndPoint { get; set; }
    }
}