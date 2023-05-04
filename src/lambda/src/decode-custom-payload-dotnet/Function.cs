using Amazon.Lambda.Core;

using Amazon.XRay.Recorder.Handlers.AwsSdk;

using customSerializationLib;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace decode_custom_payload_dotnet;

public class Function
{
    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        AWSSDKHandler.RegisterXRayForAllServices();
    }

    #pragma warning disable CS1998 

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <remarks>
    /// Data from AWS IoT Core Rule came as JSON: System.Text.Json.JsonElement or can be directly mapped into a POCO object.
    /// https://docs.aws.amazon.com/iot/latest/developerguide/iot-sql-functions.html#iot-func-aws-lambda
    /// </remarks>
    public async Task<IoTCoreRuleSelectLambdaResponse> FunctionHandler(IoTCoreRuleSelectLambdaPayload input, ILambdaContext context)
    {
        IoTCoreRuleSelectLambdaResponse response = new IoTCoreRuleSelectLambdaResponse();

        LambdaLogger.Log($"Got this raw payload: {input.base64Payload}");

        try
        {
            byte[] serializedPayload = Convert.FromBase64String(input.base64Payload);

            var deserializedPayload = MyPayload.Deserialize(serializedPayload);

            response.isValid = true;
            response.payload = deserializedPayload;
        }
        catch (Exception ex)
        {
            response.isValid = false;
            response.exception = ex.ToString();
        }

        return response;
    }

    #pragma warning restore CS1998 
}
