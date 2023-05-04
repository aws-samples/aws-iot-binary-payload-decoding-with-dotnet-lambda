using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace customSerializationLib
{
    public class MyPayload
    {
        #pragma warning disable CS8618 

        public string EquipmentName {get;set;}

        #pragma warning restore CS8618 

        public DateTime TimeStamp {get;set;}

        public double Temperature {get;set;}
        public double Humidity {get;set;}

        public byte[] Serialize()
        {
            // Serialize EquipmentName as a string length followed by the characters
            var nameBytes = Encoding.UTF8.GetBytes(this.EquipmentName);
            var nameLengthBytes = BitConverter.GetBytes((short)nameBytes.Length);
            var serializedEquipmentName = nameLengthBytes.Concat(nameBytes).ToArray();

            // Serialize TimeStamp as a Unix timestamp with milliseconds
            var unixTimestamp = (long)(this.TimeStamp.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            var serializedTimeStamp = BitConverter.GetBytes(unixTimestamp);

            // Serialize Temperature as a 5-digit truncated double
            var truncatedTemperature = Math.Truncate(this.Temperature * 100000) / 100000;
            var serializedTemperature = BitConverter.GetBytes(truncatedTemperature);

            // Serialize Humidity as a 5-digit truncated double
            var truncatedHumidity = Math.Truncate(this.Humidity * 100000) / 100000;
            var serializedHumidity = BitConverter.GetBytes(truncatedHumidity);

            // Concatenate all serialized fields and return the result
            return serializedEquipmentName.Concat(serializedTimeStamp)
                                        .Concat(serializedTemperature)
                                        .Concat(serializedHumidity)
                                        .ToArray();
        }

        public static MyPayload Deserialize(byte[] bytes)
        {
            // Deserialize EquipmentName string length and characters
            var nameLength = BitConverter.ToInt16(bytes, 0);
            var equipmentName = Encoding.UTF8.GetString(bytes, 2, nameLength);

            // Deserialize TimeStamp as Unix timestamp with milliseconds
            var unixTimestamp = BitConverter.ToInt64(bytes, 2 + nameLength);
            var timeStamp = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(unixTimestamp);

            // Deserialize Temperature as 5-digit truncated double
            var temperature = Math.Round(BitConverter.ToDouble(bytes, 10 + nameLength), 5);

            // Deserialize Humidity as 5-digit truncated double
            var humidity = Math.Round(BitConverter.ToDouble(bytes, 18 + nameLength), 5);

            // Create and return the deserialized MyPayload object
            return new MyPayload { EquipmentName = equipmentName, TimeStamp = timeStamp, Temperature = temperature, Humidity = humidity };
        }
        

        
        public string ToJson()
        {   
            JsonSerializerOptions jsonOption = new JsonSerializerOptions();
            jsonOption.WriteIndented = true;

            string jsonString = JsonSerializer.Serialize(this,jsonOption);

            return jsonString;
        }
    }
}
