using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MBaumann.WebPush.WebUi.Entities;
using MBaumann.WebPush.WebUi.Models;
using WebPush;

namespace MBaumann.WebPush.WebUi.Services
{
    public interface ISubscriptionService
    {
        /// <summary>
        /// Is the vapid public key set ?
        /// </summary>
        /// <returns><c>true</c>, if vapid public key was set, <c>false</c> otherwise.</returns>
        bool HasVapidPublicKey();

        /// <summary>
        /// Gets the vapid public key.
        /// </summary>
        /// <returns>The vapid public key.</returns>
        string GetVapidPublicKey();

        /// <summary>
        /// Saves the push subscription.
        /// </summary>
        /// <returns>The subscribed device.</returns>
        /// <param name="p_pushSubscription">P push subscription.</param>
        Task<Device> SavePushSubscription(PushSubscription p_pushSubscription);

        /// <summary>
        /// Gets the registred devices by predicate.
        /// </summary>
        /// <returns>The devices.</returns>
        /// <param name="p_predicate">Filter predicate.</param>
        IEnumerable<Device> GetDevices(Expression<Func<Device, bool>> p_predicate);

        /// <summary>
        /// Gets all devices.
        /// </summary>
        /// <returns>The devices.</returns>
        IEnumerable<Device> GetAllDevices();

        /// <summary>
        /// Gets a device by predicate.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="p_predicate">Filter predicate.</param>
        Device GetDevice(Expression<Func<Device, bool>> p_predicate);

        /// <summary>
        /// Sends a message to a device.
        /// </summary>
        /// <param name="p_message">Message to send.</param>
        /// <param name="p_targetDevice">Target device.</param>
        Task SendMessage(MessageViewModel p_message, Device p_targetDevice);

        /// <summary>
        /// Sends a message to multiple devices.
        /// </summary>
        /// <param name="p_message">Message to send.</param>
        /// <param name="p_targetDevices">Target devices.</param>
        Task SendMessage(MessageViewModel p_message, IEnumerable<Device> p_targetDevices);
    }
}
