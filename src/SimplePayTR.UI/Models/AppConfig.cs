namespace SimplePayTR.UI.Models
{
    public class AppConfig
    {
        public string SuccessRedirectUrl { get; set; }

        public string FailRedirectUrl { get; set; }

        public string RedisConnectionString { get; set; }
        public int? RedisDatabaseId { get; internal set; }
        public int RedisDefaultExpireTimeOut { get; internal set; }
        public string RedisPrefix { get; internal set; }
    }
}