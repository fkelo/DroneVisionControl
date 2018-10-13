using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DJIWTF
{
    class MiddlewareConnector
    {

        private IRestClient Client;

        public MiddlewareConnector(string baseUrl)
        {
            Client = new RestClient(baseUrl);
        }

        public string GetDevices()
        {
            IRestRequest req = new RestRequest("/api/v1/device", Method.GET);
            IRestResponse res = Client.Execute(req);
            if (res.IsSuccessful)
            {
                return res.Content;
            }
            throw new Exception("Request Get Devices failed");
        }

        public string GetStorages()
        {
            IRestRequest req = new RestRequest("/api/v1/storage", Method.GET);
            IRestResponse res = Client.Execute(req);
            if (res.IsSuccessful)
            {
                return res.Content;
            }
            throw new Exception("Request Get Storages failed");
        }

        public string GetConfigurations(string deviceId, string storageId)
        {
            IRestRequest req = new RestRequest("/api/v1/storage/{storageId}/configuration", Method.GET);
            req.AddUrlSegment("storageId", storageId);
            req.AddQueryParameter("device_id", deviceId);
            IRestResponse res = Client.Execute(req);
            if (res.IsSuccessful)
            {
                return res.Content;
            }
            throw new Exception("Request Get Configurations failed");
        }

        public string PostRun(string deviceId, string storageId, string configurationId)
        {
            IRestRequest req = new RestRequest("/api/v1/run", Method.POST);
            JsonStartRun json = new JsonStartRun();
            json.ConfigurationId = configurationId;
            json.DeviceId = deviceId;
            json.StorageId = storageId;
            req.AddJsonBody(JsonConvert.SerializeObject(json));
            IRestResponse res = Client.Execute(req);
            if (res.IsSuccessful)
            {
                return res.Content;
            }
            throw new Exception("Request Post Run failed");
        }

        public void PatchRun(string runId)
        {
            IRestRequest req = new RestRequest("/api/v1/run/{runId}", Method.PATCH);
            req.AddUrlSegment("runId", runId);
            req.AddJsonBody("{\"command\":\"stop\"}");
            IRestResponse res = Client.Execute(req);
            if(res.Content != null)
            {
                Console.WriteLine(res.Content);
            }
            if (res.IsSuccessful)
            {
                return;
            }
            throw new Exception("Request Patch Run failed");
        }

        public string GetSequence(string runId)
        {
            IRestRequest req = new RestRequest("/api/v1/run/{runId}/data", Method.GET);
            req.AddUrlSegment("runId", runId);
            IRestResponse res = Client.Execute(req);
            if (res.IsSuccessful)
            {
                return res.Content;
            }
            throw new Exception("Request Get Sequence failed");
        }

        public void PostImage(string runId, string name, string mimetype, string filepath)
        {
            IRestRequest req = new RestRequest("/api/v1/run/{runId}/data", Method.POST);
            req.AddUrlSegment("runId", runId);
            JsonImage json = new JsonImage();
            json.Name = name;
            json.Mimetype = mimetype;
            String b64 = Convert.ToBase64String(File.ReadAllBytes(filepath));
            json.Data = b64;
            req.AddJsonBody(JsonConvert.SerializeObject(json));
            IRestResponse res = Client.Execute(req);
            if (res.IsSuccessful)
            {
                return;
            }
            throw new Exception("Request Post Image failed");
        }
    }
}
