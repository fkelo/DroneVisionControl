using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJIWTF
{
    public class DronePoint
    {
        [JsonProperty("x")]
        public int X { get; set; }
        [JsonProperty("y")]
        public int Y { get; set; }
        [JsonProperty("z")]
        public int Z { get; set; }
        [JsonProperty("rotation")]
        public int Rotation { get; set; }
        [JsonProperty("take_image")]
        public bool TakeImage { get; set; }
        [JsonProperty("order_id")]
        public int OrderId { get; set; }
    }
}
