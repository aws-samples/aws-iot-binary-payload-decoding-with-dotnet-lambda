using System;

namespace customSerializationLib
{
    public class IoTCoreRuleSelectLambdaResponse
    {
        public bool isValid {get;set;}
        
        #pragma warning disable CS8618 

        public MyPayload payload {get;set;}

        public string exception {get;set;}

        #pragma warning restore CS8618 
    }
}
