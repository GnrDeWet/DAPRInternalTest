using System.Globalization;
using Common.Interfaces.Requests;
using Common.Models.Requests;
using Common.Models.Requests.Publish;
using Common.Models.Responses;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using AlertApi.Models.Requests;
using WebApi.Models.Requests;

namespace AlertApi.RouteBuilders
{
    public static class EndpointFactory
    {
        public static void RegisterEndpoints(this WebApplication app)
        {
            app.MapPost("/sendSms", async ([FromBody] SmsRequest request) =>
            {
                // Invoke a binding to an sms service provider (You will need to create your own twilion account) 
                // No body as free tier of twilio offers no message capability
                using var client = new DaprClientBuilder().Build();

                var smsMetadata = new Dictionary<string, string>
                {
                    { "toNumber", request.PhoneNumber }//,
                    //{ "body", "Hello, this is a test SMS sent via Dapr and Twilio." }
                };

                await client.InvokeBindingAsync("twilio", "create", string.Empty, smsMetadata);

                app.Logger.LogInformation("SMS sent successfully");
            });

            app.MapPost("/sendEmail", async ([FromBody] EmailRequest request) =>
            {
                // Invoke a binding to an email service provider (you will need to create your own sendgrid account)
                // Used Mailgun
                using var client = new DaprClientBuilder().Build();

                var emailMetadata = new Dictionary<string, string>
                {
                    { "emailTo", request.EmailAddress },
                    { "subject", "This is a test email" }
                };
                
                var emailBody = "Hello, this is a test email sent via Dapr and SMTP.";
                
                await client.InvokeBindingAsync("smtp", "create", emailBody, emailMetadata);


                app.Logger.LogInformation("Email sent successfully");
            });

            app.MapPost("/sendAlert", async (ApiAlertRequest alertRequest) =>
            {
                // Publish multiple events via rabbitmq (could also use redis but only in self-hosted kubernetes mode) 
                using var client = new DaprClientBuilder().Build();

                for (int i = 0; i < alertRequest.AlertTypes.Count(); i++)
                {
                    app.Logger.LogInformation("Publishing alert: " + alertRequest.AlertTypes[i] + "\n      For client: " + alertRequest.ClientName);
                    DateTime now = DateTime.Now;

                    PublishAlertRequest pubRequest = new PublishAlertRequest()
                    {
                        AlertType = alertRequest.AlertTypes[i],
                        PublishRequestTime = now
                    };

                    var pubMetadata = new Dictionary<string, string>
                    {
                        { "clientName", alertRequest.ClientName },
                        { "alertType", pubRequest.AlertType },
                        { "publishTime", pubRequest.PublishRequestTime.ToString(CultureInfo.CurrentCulture) }
                    };

                    await client.PublishEventAsync("redis-pubsub", "alerts", pubRequest); //, pubMetadata);
                    
                    //await client.PublishEventAsync("orderpubsub", "orders", order);
                    
                    app.Logger.LogInformation("Published data at: " + pubRequest.PublishRequestTime);    

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

            });
        }
    }
}
