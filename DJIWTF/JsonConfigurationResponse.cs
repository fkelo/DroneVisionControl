using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJIWTF
{
    class JsonConfigurationResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("data")]
        public DroneConfiguration Data { get; set; }
    }
}
