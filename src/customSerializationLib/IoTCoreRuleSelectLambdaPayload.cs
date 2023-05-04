using System;

namespace customSerializationLib
{
    //This is the class reppresenting the payload coming from the AWS IoT Core rule SELECT statement when calling the AWS Lambda
    public class IoTCoreRuleSelectLambdaPayload
    {
         #pragma warning disable CS8618 

        ///base64 encoded binrary data (you can call this property as you like until this matches exactly the AWS IoT Core rule literal)
        public string base64Payload {get;set;}

        #pragma warning restore CS8618 
    }
}
