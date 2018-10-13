using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJIWTF
{
    class JsonImage
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mimetype")]
        public string Mimetype { get; set; }
        [JsonProperty("data")]
        public string Data { get; set; }
    }
}
