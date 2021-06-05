using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace CruiseAssistant
{
    public class Config
    {
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("prefixes")]
        public IEnumerable<string> PrefixList { get; set; } = new[]
        {
            "b."
        };

        [JsonProperty("mention_trigger")]
        public bool TriggerOnMention { get; set; } = true;

        [JsonProperty("success_response")]
        public string SuccessResponse { get; set; } = ":thumbsup:";

        [JsonProperty("owner_ids")]
        public List<ulong> OwnerIds { get; set; } = new List<ulong>();

        [JsonProperty("cruise_status")]
        public bool CruiseStatus { get; set; } = false;

        [JsonProperty("cruise_link")]
        public string CruiseTallyLink { get; set; } = "";

        [JsonProperty("update_messages")]
        public Dictionary<ulong, ulong> UpdateMessageIds { get; set; } = new Dictionary<ulong, ulong>();

        public static Config Load()
        {
            if (File.Exists("config.json"))
            {
                var json = File.ReadAllText("config.json");
                return JsonConvert.DeserializeObject<Config>(json);
            }
            var config = new Config();
            config.Save();
            throw new InvalidOperationException("configuration file created; insert token and restart.");
        }

        public void Save()
        {
            //var json = JsonConvert.SerializeObject(this);
            //File.WriteAllText("config.json", json);
            JsonStorage.SerializeObjectToFile(this, "config.json").Wait();
        }
    }
}
