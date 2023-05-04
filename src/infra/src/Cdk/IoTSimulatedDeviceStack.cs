using System;
using Amazon.CDK;
using Amazon.CDK.AWS.IoT;
using Constructs;


namespace Cdk
{
    public class IoTSimulatedDeviceStack : Stack
    {
        internal IoTSimulatedDeviceStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {  
            var thingName = this.Node.TryGetContext("thingName") as string;
            var thingCertArn = this.Node.TryGetContext("thingCertArn") as string;
            var encodedDataTopic = this.Node.TryGetContext("encodedDataTopic") as string;
            
            // Create an IoT Device to run the simulation.
            var iotDevice = new CfnThing(this, "SimulatedDevice", new CfnThingProps
            {
                ThingName = thingName               
            });

            
            //create a policy and assign to the certificate
            var connectStatement = new System.Collections.Generic.Dictionary<string, object>();
            connectStatement.Add("Effect","Allow");
            connectStatement.Add("Action", new[] { "iot:Connect" });
            connectStatement.Add("Resource", new[] { $"arn:aws:iot:{ props.Env.Region }:{ props.Env.Account }:client/${{iot:Connection.Thing.ThingName}}"});


            var publishStatement = new System.Collections.Generic.Dictionary<string, object>();
            publishStatement.Add("Effect","Allow");
            publishStatement.Add("Action", new[] { "iot:Publish" });
            publishStatement.Add("Resource", new[] { $"arn:aws:iot:{ props.Env.Region }:{ props.Env.Account }:topic/{encodedDataTopic}" });

            var policyDocument = new System.Collections.Generic.Dictionary<string, object>();
            policyDocument.Add("Version", "2012-10-17");
            policyDocument.Add("Statement", new[] {connectStatement, publishStatement });

            var policy = new Amazon.CDK.AWS.IoT.CfnPolicy(this, "SimulatedDevicePolicy", new Amazon.CDK.AWS.IoT.CfnPolicyProps
            {   
                PolicyName = "SimulatedDevicePolicy",
                PolicyDocument = policyDocument                                                                  
            });
            

            var policyPrincipalAttachment = new CfnPolicyPrincipalAttachment(this, "MyPolicyAttachment", new CfnPolicyPrincipalAttachmentProps
            {
                PolicyName = policy.PolicyName,
                Principal = thingCertArn
            });

            policyPrincipalAttachment.AddDependency(policy);
            
            //assign the certificate to the device
            var thingPrincipalAttachment = new CfnThingPrincipalAttachment(this, "MyThingPrincipalAttachment", new CfnThingPrincipalAttachmentProps
            {
                Principal = thingCertArn,
                ThingName = thingName
            });

            thingPrincipalAttachment.AddDependency(iotDevice);
            
        }        
    }
}
