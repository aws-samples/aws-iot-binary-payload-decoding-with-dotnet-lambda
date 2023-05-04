using System.Globalization;
using MQTTnet;
using MQTTnet.Client;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace deviceSimulator
{
    class Program
    {
        #pragma warning disable CS8618 

        private static IMqttClient mqttClient;

        private static RootCertificateTrust rootCertificateTrust;

        #pragma warning restore CS8618 

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello IoT Device Simulator!");

            DeviceSimulatorConfig deviceSimulatorConfig  = DeviceSimulatorConfig.FromJson( File.ReadAllText("deviceSimulatorConfig.json")  );

            string clientCertificateStr = File.ReadAllText(deviceSimulatorConfig.ClientCertificatePath);
            string clientPrivateKeyStr = File.ReadAllText(deviceSimulatorConfig.ClientPrivateKeyPath);

            var clientCertWithKey = X509Certificate2.CreateFromPem(clientCertificateStr, clientPrivateKeyStr);

            string caCertificateStr = File.ReadAllText(deviceSimulatorConfig.CaCertificatePath);
            var caCertificate = X509Certificate2.CreateFromPem(caCertificateStr);

            //This is a helper class to allow verifying a root CA separately from the trusted store
            rootCertificateTrust = new RootCertificateTrust();
            rootCertificateTrust.AddCert(caCertificate);

            // Certificate based authentication
            List<X509Certificate> certs = new List<X509Certificate>
            {               
                clientCertWithKey
            };

            MqttClientOptionsBuilderTlsParameters tlsOptions = new MqttClientOptionsBuilderTlsParameters();
            tlsOptions.Certificates = certs;
            tlsOptions.SslProtocol = System.Security.Authentication.SslProtocols.Tls12;
            tlsOptions.UseTls = true;            
            tlsOptions.AllowUntrustedCertificates = false;
            tlsOptions.CertificateValidationHandler += rootCertificateTrust.VerifyServerCertificate;
            
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId(deviceSimulatorConfig.ClientId)
                .WithTcpServer(deviceSimulatorConfig.Endpoint,8883)
                .WithTls(tlsOptions)
                // Disabling packet fragmentation is very important!  
                .WithoutPacketFragmentation()
                .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                .Build();

            var mqttFactory = new MqttFactory();            
      
            mqttClient = mqttFactory.CreateMqttClient();   
            //mqttClient.ConnectingAsync +=          
                   
            var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            //connect!                   
            var connectResult = await mqttClient.ConnectAsync(mqttClientOptions, timeoutToken.Token);

            if ( connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("Connected!");
            }
            else
            {
                Console.WriteLine($"NOT Connected! - {connectResult.ReasonString}");
            }


            //create a payload entity with some random values
            customSerializationLib.MyPayload payload = new customSerializationLib.MyPayload()
            {
                EquipmentName = "example equipment",
                TimeStamp = DateTime.UtcNow,
                Temperature = Random.Shared.NextDouble() * 50.0,
                Humidity = Random.Shared.NextDouble() * 100.0,
            };

            //serialize as bytes
            var payloadBytes = payload.Serialize();                       

            Console.WriteLine($"{ DateTime.UtcNow.ToString() } - Sending message to MQTT topic: { deviceSimulatorConfig.EncodedDataTopic } \n{ payload.ToJson() }\nbase64 encoded byte[]: { Convert.ToBase64String(payloadBytes) }");

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(deviceSimulatorConfig.EncodedDataTopic)
                .WithPayload(payloadBytes)                    
                .Build();


            var publishResult = await mqttClient.PublishAsync(message);

            if (publishResult.IsSuccess)
            {
                Console.WriteLine("Message published!");
            }
            else
            {
                Console.WriteLine($"Message NOT published! - {publishResult.ReasonString}");
            }

            Console.WriteLine(DateTime.UtcNow.ToString() + " Done, closing...");

            var mqttClientDisconnectOptions = mqttFactory.CreateClientDisconnectOptionsBuilder()
            .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
            .Build();

            await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
        }
    }
}