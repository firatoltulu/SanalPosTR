namespace SanalPosTR.Configuration
{
    public interface IEnvironmentConfiguration
    {
        public string BaseUrl { get; set; }

        /// <summary>
        /// XML veya JSON ile ödeme almak için HTTP Noktası
        /// </summary>
        public string ApiEndPoint { get; }

        /// <summary>
        /// 3D Secure ile ödeme almak için Form Http Noktası
        /// </summary>
        public string SecureEndPointApi { get; }

        /// <summary>
        /// 3D Secure Başarılı işlem sonrası para çekimi için Http noktası
        /// </summary>
        public string SecureReturnEndPoint { get; }

        /// <summary>
        /// 3D Secure iade işlemleri için http noktası
        /// </summary>
        public string RefundEndPoint { get; }
    }
}