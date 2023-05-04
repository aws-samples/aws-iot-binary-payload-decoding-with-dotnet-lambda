using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace deviceSimulator
{
    public class DeviceSimulatorConfig
    {
        #pragma warning disable CS8618 

        public string Endpoint {get;set;}
        public string ClientId {get;set;}

        public string ClientCertificatePath {get;set;}

        public string ClientPrivateKeyPath {get;set;}

        public string CaCertificatePath {get;set;}

        public string EncodedDataTopic {get;set;}

        #pragma warning restore CS8618 

        #pragma warning disable CS8603 

        //load the configuration from a JSON string
        public static DeviceSimulatorConfig FromJson(string json)
        {
            var obj = JsonSerializer.Deserialize<DeviceSimulatorConfig>(json);

            return obj;
        }

        public string ToJson()
        {   
            JsonSerializerOptions jsonOption = new JsonSerializerOptions();
            jsonOption.WriteIndented = true;

            string jsonString = JsonSerializer.Serialize(this,jsonOption);

            return jsonString;
        }

        #pragma warning restore CS8603 
    }
}
