using System;
using WebPush;

namespace MBaumann.WebPush.WebUi.Entities
{
    public sealed class Device : PushSubscription
    {
        public Device() {}

        public Device(PushSubscription p_subscription) {
            if (p_subscription == null)
                throw new ArgumentNullException(nameof(p_subscription), "Push subscription cannot be null");

            this.Auth = p_subscription.Auth;
            this.Endpoint = p_subscription.Endpoint;
            this.P256DH = p_subscription.P256DH;
        }

        public Guid Id { get; set; }
    }
}
