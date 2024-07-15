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
            app.MapPost("/sendSms", async (SmsRequest smsRequest) =>
            {
                if (smsRequest == null || string.IsNullOrEmpty(smsRequest.PhoneNumber))
                {
                    throw new ArgumentNullException(nameof(smsRequest), "SMS request or its properties cannot be null or empty");
                }
                
                // Invoke a binding to an sms service provider (You will need to create your own twilion account) 
                // No body as free tier of twilio offers no message capability
                using var client = new DaprClientBuilder().Build();

                var smsMetadata = new Dictionary<string, string>
                {
                    { "toNumber", smsRequest.PhoneNumber }//,
                    //{ "body", "Hello, this is a test SMS sent via Dapr and Twilio." }
                };

                await client.InvokeBindingAsync("twilio", "create", string.Empty, smsMetadata);

                app.Logger.LogInformation($"SMS sent to: {smsRequest.PhoneNumber}");
                
                return Results.Ok();
                
            });

            app.MapPost("/sendEmail", async (EmailRequest emailRequest) =>
            {
                if (emailRequest == null || string.IsNullOrEmpty(emailRequest.EmailAddress))
                {
                    throw new ArgumentNullException(nameof(emailRequest), "Email request or its properties cannot be null or empty");
                }
                
                // Invoke a binding to an email service provider (you will need to create your own sendgrid account)
                // Used Mailgun
                using var client = new DaprClientBuilder().Build();

                var emailMetadata = new Dictionary<string, string>
                {
                    { "emailTo", emailRequest.EmailAddress },
                    { "subject", "This is a test email" }
                };
                
                var emailBody = "Hello, this is a test email sent via Dapr and SMTP.";
                
                await client.InvokeBindingAsync("smtp", "create", emailBody, emailMetadata);

                app.Logger.LogInformation($"Email sent to: {emailRequest.EmailAddress}");
                
                return Results.Ok();
                
            });

            app.MapPost("/sendAlert", async (AlertRequest alertRequest) =>
            {
                if (alertRequest == null || alertRequest.AlertTypes == null || alertRequest.ClientName == null)
                {
                    throw new ArgumentNullException(nameof(alertRequest), "Alert request or its properties cannot be null");
                }
                
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

                    await client.PublishEventAsync("redis-pubsub", "alerts", pubRequest, pubMetadata);
                    
                    app.Logger.LogInformation($"Published alert: {pubRequest.AlertType} for client: {alertRequest.ClientName} at {pubRequest.PublishRequestTime}");

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                
                return Results.Ok();

            });
        }
    }
}
