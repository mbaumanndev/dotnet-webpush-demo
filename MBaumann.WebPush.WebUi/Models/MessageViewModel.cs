using System.Collections.Generic;
using Newtonsoft.Json;

namespace MBaumann.WebPush.WebUi.Models
{
    public sealed class MessageViewModel
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("options")]
        public Dictionary<string, object> Options { get; set; }
    }
}
