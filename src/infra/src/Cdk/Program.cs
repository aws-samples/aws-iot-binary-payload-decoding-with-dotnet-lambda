using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;
using Cdklabs.CdkNag;


namespace Cdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

         
            //Add cdk-nag
            Aspects.Of(app)
                .Add(
                        new AwsSolutionsChecks( 
                            new Cdklabs.CdkNag.NagPackProps()
                            { 
                                Verbose = true                                
                            })
                    );
                 
            
            //Create the .NET Lambda function to decode the custom payload
            //The Rule in AWS IoT Core that execute the lambda function & re-publish the outcome to another topic
            var ioTCoreInfraStack = new IoTCoreInfraStack(app, "IoTCoreInfraStack", new StackProps
            {
                // If you don't specify 'env', this stack will be environment-agnostic.
                // Account/Region-dependent features and context lookups will not work,
                // but a single synthesized template can be deployed anywhere.

                // Uncomment the next block to specialize this stack for the AWS Account
                // and Region that are implied by the current CLI configuration.
                
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION"),
                }
                
                // Uncomment the next block if you know exactly what Account and Region you
                // want to deploy the stack to.
                /*
                Env = new Amazon.CDK.Environment
                {
                    Account = "123456789012",
                    Region = "us-east-1",
                }
                */

                // For more information, see https://docs.aws.amazon.com/cdk/latest/guide/environments.html
            });

                    
            //create the simulated device and its own certificate
            var ioTSimulatedDeviceStack = new IoTSimulatedDeviceStack(app, "IoTSimulatedDeviceStack", new StackProps
            {
                // If you don't specify 'env', this stack will be environment-agnostic.
                // Account/Region-dependent features and context lookups will not work,
                // but a single synthesized template can be deployed anywhere.

                // Uncomment the next block to specialize this stack for the AWS Account
                // and Region that are implied by the current CLI configuration.
                
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION"),
                }
                
                // Uncomment the next block if you know exactly what Account and Region you
                // want to deploy the stack to.
                /*
                Env = new Amazon.CDK.Environment
                {
                    Account = "123456789012",
                    Region = "us-east-1",
                }
                */

                // For more information, see https://docs.aws.amazon.com/cdk/latest/guide/environments.html
            });



            app.Synth();
        }
    }
}
