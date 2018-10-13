using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJIWTF
{
    class JsonConfiguration
    {
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
        [JsonProperty("storage_id")]
        public string StorageId { get; set; }
    }
}
