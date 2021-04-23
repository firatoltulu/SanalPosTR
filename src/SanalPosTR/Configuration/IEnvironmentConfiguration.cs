namespace SanalPosTR.Configuration
{
    public interface IEnvironmentConfiguration
    {
        string BaseUrl { get; set; }

        /// <summary>
        /// XML veya JSON ile ödeme almak için HTTP Noktası
        /// </summary>
        string ApiEndPoint { get; }

        /// <summary>
        /// 3D Secure ile ödeme almak için Form Http Noktası
        /// </summary>
        string SecureEndPointApi { get; }

        /// <summary>
        /// 3D Secure Başarılı işlem sonrası para çekimi için Http noktası
        /// </summary>
        string SecureReturnEndPoint { get; }

        /// <summary>
        /// 3D Secure iade işlemleri için http noktası
        /// </summary>
        string RefundEndPoint { get; }
    }
}