using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJIWTF
{
    public class DroneConfiguration
    {
        [JsonProperty("max_height")]
        public int MaxHeight { get; set; }
        [JsonProperty("speed")]
        public int Speed { get; set; }
        [JsonProperty("path")]
        public List<DronePoint> Path { get; set; }
    }
}
