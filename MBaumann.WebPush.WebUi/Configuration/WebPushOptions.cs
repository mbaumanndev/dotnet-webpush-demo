namespace MBaumann.WebPush.WebUi.Configuration
{
    public sealed class WebPushOptions
    {
        public string GcmAPIKey { get; set; }

        public string VapidSubject { get; set; }

        public string VapidPublicKey { get; set; }

        public string VapidPrivateKey { get; set; }
    }
}
