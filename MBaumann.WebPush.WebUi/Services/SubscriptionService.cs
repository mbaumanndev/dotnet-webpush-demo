using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MBaumann.WebPush.WebUi.Configuration;
using MBaumann.WebPush.WebUi.Data;
using MBaumann.WebPush.WebUi.Entities;
using MBaumann.WebPush.WebUi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebPush;

namespace MBaumann.WebPush.WebUi.Services
{
    public sealed class SubscriptionService : ISubscriptionService
    {
        private WebPushDbContext DbContext { get; }
        private ILogger Logger { get; }

        private static readonly object m_lock = new object();

        private static VapidDetails VapidDetails { get; set; }
        private static string GcmAPIKey { get; set; }
        private static bool m_initilized;

        #region Constructor and init

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MBaumann.WebPush.WebUi.Services.SubscriptionService"/> class.
        /// </summary>
        /// <param name="p_options">Web push options.</param>
        /// <param name="p_dbContext">Database context.</param>
        /// <param name="p_logger">Logger.</param>
        public SubscriptionService(
            IOptions<WebPushOptions> p_options,
            WebPushDbContext p_dbContext,
            ILogger<SubscriptionService> p_logger)
        {
            if (p_options.Value == null)
                throw new ArgumentNullException(nameof(p_options), "Web Push options cannot be null");

            if (String.IsNullOrWhiteSpace(p_options.Value.VapidSubject) && String.IsNullOrWhiteSpace(p_options.Value.GcmAPIKey))
                throw new ApplicationException("You must set either the Vapid Subject or the GCM API Key");

            DbContext = p_dbContext;
            Logger = p_logger;

            if (!m_initilized)
            {
                lock(m_lock) 
                {
                    if (!m_initilized) {
                        GcmAPIKey = p_options.Value.GcmAPIKey;
                        SetVapidDetails(p_options).Wait();
                        m_initilized = true;
                    }
                }
            }

        }

        /// <summary>
        /// Sets the vapid details.
        /// </summary>
        /// <param name="p_options">Web push options.</param>
        private async Task SetVapidDetails(IOptions<WebPushOptions> p_options)
        {
            bool v_validSubject = true;
            string v_vapidSubject;
            string v_vapidPublicKey;
            string v_vapidPrivateKey;

            try
            {
                VapidHelper.ValidateSubject(p_options.Value.VapidSubject);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Vapid subject is not valid");
                v_validSubject = false;
            }

            if (!v_validSubject && String.IsNullOrWhiteSpace(GcmAPIKey))
                throw new ApplicationException("You must set a valid vapid subject or GCM API Key");

            if (v_validSubject)
            {
                v_vapidSubject = p_options.Value.VapidSubject;

                try
                {
                    VapidHelper.ValidatePublicKey(p_options.Value.VapidPublicKey);
                    VapidHelper.ValidatePrivateKey(p_options.Value.VapidPrivateKey);

                    v_vapidPublicKey = p_options.Value.VapidPublicKey;
                    v_vapidPrivateKey = p_options.Value.VapidPrivateKey;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Your Vapid keys are not valid. Using auto generated keys.");

                    var v_tempVapid = VapidHelper.GenerateVapidKeys();
                    v_vapidPublicKey = v_tempVapid.PublicKey;
                    v_vapidPrivateKey = v_tempVapid.PrivateKey;

                    Logger.LogInformation("Flushing existing devices");
                    IEnumerable<Device> v_devices = this.DbContext.Devices;

                    this.DbContext.RemoveRange(v_devices);
                    await this.DbContext.SaveChangesAsync();
                }

                VapidDetails = new VapidDetails(v_vapidSubject, v_vapidPublicKey, v_vapidPrivateKey);
            }
        }

        #endregion

        /// <summary>
        /// Is the vapid public key set ?
        /// </summary>
        /// <returns><c>true</c>, if vapid public key was set, <c>false</c> otherwise.</returns>
        public bool HasVapidPublicKey()
        {
            return VapidDetails != null;
        }

        /// <summary>
        /// Gets the vapid public key.
        /// </summary>
        /// <returns>The vapid public key.</returns>
        public string GetVapidPublicKey()
        {
            if (VapidDetails == null)
                throw new ApplicationException("The Vapid Keys are not set, please set a Vapid Sender to generate the keys automatically");

            return VapidDetails.PublicKey;
        }

        /// <summary>
        /// Saves the push subscription.
        /// </summary>
        /// <returns>The subscribed device.</returns>
        /// <param name="p_pushSubscription">P push subscription.</param>
        public async Task<Device> SavePushSubscription(PushSubscription p_pushSubscription) 
        {
            if (p_pushSubscription == null)
                throw new ArgumentNullException(nameof(p_pushSubscription), "Push subscription cannot be null");

            Device v_device = new Device(p_pushSubscription);

            if (this.DbContext.Devices.Any(s => s.Auth == v_device.Auth && s.Endpoint == v_device.Endpoint && s.P256DH == v_device.P256DH))
            {
                v_device = this.DbContext.Devices.First(s => s.Auth == v_device.Auth && s.Endpoint == v_device.Endpoint && s.P256DH == v_device.P256DH);
            }
            else
            {
                await this.DbContext.Devices.AddAsync(v_device);
                await this.DbContext.SaveChangesAsync();
            }

            return v_device;
        }

        /// <summary>
        /// Gets the registred devices by predicate.
        /// </summary>
        /// <returns>The devices.</returns>
        /// <param name="p_predicate">Filter predicate.</param>
        public IEnumerable<Device> GetDevices(Expression<Func<Device, bool>> p_predicate)
        {
            return this.DbContext.Devices.Where(p_predicate);
        }

        /// <summary>
        /// Gets all devices.
        /// </summary>
        /// <returns>The devices.</returns>
        public IEnumerable<Device> GetAllDevices()
        {
            return this.DbContext.Devices;
        }

        /// <summary>
        /// Gets a device by predicate.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="p_predicate">Filter predicate.</param>
        public Device GetDevice(Expression<Func<Device, bool>> p_predicate)
        {
            return this.DbContext.Devices.FirstOrDefault(p_predicate);
        }

        /// <summary>
        /// Sends a message to a device.
        /// </summary>
        /// <param name="p_message">Message to send.</param>
        /// <param name="p_targetDevice">Target device.</param>
        public async Task SendMessage(MessageViewModel p_message, Device p_targetDevice)
        {
            if (p_message == null)
                throw new ArgumentNullException(nameof(p_message), "Message cannot be null");

            if (p_targetDevice == null)
                throw new ArgumentNullException(nameof(p_targetDevice), "Target device cannot be null");

            await this.SendMessage(p_message, new List<Device> { p_targetDevice });
        }

        /// <summary>
        /// Sends a message to multiple devices.
        /// </summary>
        /// <param name="p_message">Message to send.</param>
        /// <param name="p_targetDevices">Target devices.</param>
        public Task SendMessage(MessageViewModel p_message, IEnumerable<Device> p_targetDevices)
        {
            WebPushClient v_webPushClient = new WebPushClient();

            if (this.HasVapidPublicKey())
                v_webPushClient.SetVapidDetails(VapidDetails);

            if (!String.IsNullOrWhiteSpace(GcmAPIKey))
                v_webPushClient.SetGcmApiKey(GcmAPIKey);


            List<Task> v_pending = new List<Task>();

            foreach (Device v_targetDevice in p_targetDevices)
            {
                string v_message = JsonConvert.SerializeObject(p_message);
                v_pending.Add(v_webPushClient.SendNotificationAsync(v_targetDevice, v_message));
            }

            return Task.WhenAll(v_pending);
        }
    }
}
