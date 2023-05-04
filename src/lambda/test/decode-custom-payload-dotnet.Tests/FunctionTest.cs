using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using customSerializationLib;

namespace decode_custom_payload_dotnet.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestToUpperFunction()
    {
        var payload = new MyPayload
        {
            EquipmentName = "Equipment 1",
            TimeStamp = DateTime.UtcNow,
            Temperature = 25.6789,
            Humidity = 60.1234
        };


        // Invoke the lambda function and confirm the custom serialization works.
        var function = new Function();
        var context = new TestLambdaContext();

        IoTCoreRuleSelectLambdaPayload input = new IoTCoreRuleSelectLambdaPayload(){ base64Payload = Convert.ToBase64String( payload.Serialize() ) };
                
        var decodedPayload = await function.FunctionHandler(input, context);

        Assert.True(decodedPayload.isValid);

        Assert.Equal(payload.EquipmentName, decodedPayload.payload.EquipmentName);
        Assert.Equal(payload.TimeStamp, decodedPayload.payload.TimeStamp);
        Assert.Equal(payload.Temperature, decodedPayload.payload.Temperature);
        Assert.Equal(payload.Humidity, decodedPayload.payload.Humidity);
    }
}
