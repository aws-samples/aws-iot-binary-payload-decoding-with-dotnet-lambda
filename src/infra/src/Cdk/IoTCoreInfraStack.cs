using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.IoT;
using Amazon.CDK.AWS.IAM;
using Cdklabs.CdkNag;

namespace Cdk
{
    public class IoTCoreInfraStack : Stack
    {
        internal IoTCoreInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {       
           var encodedDataTopic = this.Node.TryGetContext("encodedDataTopic") as string;
           var decodedDataTopic = this.Node.TryGetContext("decodedDataTopic") as string;
            
            // Create IAM Role for the for the Lambda
            var lambdaRole = new Role(this, "DecodeCustomPayloadLambdaRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
            });
               
            var lambdaRotationPolicyRole = new Role(this, "LambdaRotationPolicyRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
            });



            //define the build option for the lambda (this will be built together with the CDK deployment)
            var buildOption = new BundlingOptions()
            {
                Image = Runtime.DOTNET_6.BundlingImage,
                User = "root",
                OutputType = BundlingOutput.ARCHIVED,
                Command = new string[]{
               "/bin/sh",
                "-c",
                " dotnet tool install -g Amazon.Lambda.Tools"+
                " && cd lambda/src/decode-custom-payload-dotnet" +
                " && dotnet build"+
                " && dotnet lambda package --output-package /asset-output/function.zip"
                }                
            };

            // Lambda properties
            var lambdaFuncProperties = new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                MemorySize = 128,               
                Handler = "decode-custom-payload-dotnet::decode_custom_payload_dotnet.Function::FunctionHandler",                
                Code = Code.FromAsset("../", new Amazon.CDK.AWS.S3.Assets.AssetOptions
                {
                    Bundling = buildOption
                }),
                //Role = lambdaRole
            };

            // Lambda
            var lambdaFunc = new Function(this, "IoTCorePayloadDecoderFunction", lambdaFuncProperties);
            
            // Add policies to roles
            lambdaRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] { 
                                    "logs:CreateLogGroup",
                                    "logs:CreateLogStream",
                                    "logs:PutLogEvents" },
                Resources = new[] 
                    {                         
                        $"arn:aws:logs:{ props.Env.Region }:{ props.Env.Account }:log-group:/aws/lambda/{lambdaFunc.LogGroup.LogGroupName}"
                    }                
            })); 
         
            lambdaFuncProperties.Role = lambdaRole;

            // Add policies to roles
            lambdaRotationPolicyRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] { 
                                    "logs:CreateLogGroup",
                                    "logs:CreateLogStream",
                                    "logs:PutLogEvents" },
                Resources = new[] 
                    {                         
                        $"arn:aws:logs:{ props.Env.Region }:{ props.Env.Account }:log-group:/aws/lambda/{lambdaFunc.FunctionName}-LogRetention*"
                    }                
            })); 
          
            lambdaFuncProperties.LogRetention = RetentionDays.ONE_DAY;
            lambdaFuncProperties.LogRetentionRole = lambdaRotationPolicyRole;
           
           
           
            // Create the IAM Role for the IoT Core Rule to being able to republish on a target Topic
            var ruleRole = new Role(this, "DecodeCustomPayloadRuleRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("iot.amazonaws.com"),
            });


            // Aggiunta delle autorizzazioni IAM per la Lambda
            ruleRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] { "iot:Publish" },
                Resources = new[] { $"arn:aws:iot:{ props.Env.Region }:{ props.Env.Account }:topic/{decodedDataTopic}" },
            }));
                       

            // Create the IoT Core Rule wich take the binary payload, encode in base64 before sending to the custom decoder function
            var iotRule = new CfnTopicRule(this, "DecodeCustomPayloadRule", new CfnTopicRuleProps
            {
                RuleName = "DecodeCustomPayloadRule",
                TopicRulePayload = new CfnTopicRule.TopicRulePayloadProperty
                {
                    AwsIotSqlVersion ="2016-03-23",

                    //The JSON provided into the Lambda here must match the POCO class used in the .NET Lambda!! (look at the IoTCorePayload class in the Lambda Project)
                    Sql = $"SELECT\n aws_lambda(\"{ lambdaFunc.FunctionArn }\", {{'base64Payload':  encode(*,'base64') }}) as payload\nFROM \"{encodedDataTopic}\"",

                    Actions = new[]
                    {   
                        new CfnTopicRule.ActionProperty
                        {
                            Republish = new CfnTopicRule.RepublishActionProperty
                            {
                                Topic = decodedDataTopic,
                                Qos = 1,
                                RoleArn = ruleRole.RoleArn                            
                            }
                        },                        
                    },
                },
            });
            
            //give the lambda the permission to be executed by the AWS IoT core rule
            lambdaFunc.AddPermission("IoTCorePayloadDecoderFunctionExecPermission", new Permission {                
                SourceAccount = props.Env.Account,
                Principal = new ServicePrincipal("iot.amazonaws.com"),
                SourceArn = iotRule.AttrArn,                
                Action = "lambda:InvokeFunction"
            });    

            NagSuppressions.AddResourceSuppressions( scope , new[] 
                {
                    new Cdklabs.CdkNag.NagPackSuppression()
                    {
                        Id = "AwsSolutions-IAM5",
                        Reason = "Policy is detailed enought even if contains a wildcard (it includes teh lambda name and 'LogRetention' as resource)",                        
                    },
                    new Cdklabs.CdkNag.NagPackSuppression()
                    {
                        Id = "AwsSolutions-IAM4",
                        Reason = "Policy is detailed enought even if contains a wildcard (it includes teh lambda name and 'LogRetention' as resource)",                        
                    }
                }, true);      
        }
    }
}