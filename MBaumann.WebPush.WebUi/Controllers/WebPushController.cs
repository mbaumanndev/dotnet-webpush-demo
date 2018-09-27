using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MBaumann.WebPush.WebUi.Entities;
using MBaumann.WebPush.WebUi.Models;
using MBaumann.WebPush.WebUi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebPush;

namespace MBaumann.WebPush.WebUi.Controllers
{
    public sealed class WebPushController : Controller
    {
        ISubscriptionService Service { get; }
        ILogger Logger { get; }

        public WebPushController(ISubscriptionService p_service, ILogger<WebPushController> p_logger)
        {
            Service = p_service;
            Logger = p_logger;
        }

        [HttpGet("/api/get_vapid_public_key")]
        public string GetVapidPublicKey() 
        {
            return Service.HasVapidPublicKey() ? Service.GetVapidPublicKey() : String.Empty;
        }

        [HttpPost("/api/save_user_endpoint")]
        public async Task<IActionResult> SaveUserEndpoint([FromBody]PushSubscription p_pushSubscription)
        {
            Device v_device = await Service.SavePushSubscription(p_pushSubscription);

            return Ok(v_device);
        }

        [HttpGet("/api/endpoints")]
        public IEnumerable<Device> Endpoints()
        {
            return Service.GetAllDevices();
        }

        [HttpPost("/api/trigger_push_message")]
        public async Task<IActionResult> TriggerPushMessage([FromBody]MessageViewModel message)
        {
            try
            {
                await Service.SendMessage(message, Service.GetAllDevices());

                return Ok();
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "Failed to send messages.");
                return StatusCode(500);
            }
        }
    }
}
