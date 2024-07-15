using System.Text.Json.Serialization;
using Common.Models.Requests;
using Common.Models.Requests.Publish;
using Dapr;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

if (app.Environment.IsDevelopment()) { app.UseDeveloperExceptionPage(); }

// This endpoint should subscribe to a topic and will then write to the console information about the data received
// Subscription can be via redis or rabbitmq
//app.MapPost("/alert", (PublishAlertRequest pubRequest) => {
//    Console.WriteLine("Subscriber received : " + pubRequest.AlertType + "\nSent at time : " + pubRequest.PublishRequestTime);
//});

app.MapPost("/alert", [Topic("redis-pubsub", "alerts")] (PublishAlertRequest pubRequest) => {
    Console.WriteLine("Subscriber received : " + pubRequest.AlertType + "\nSent at time : " + pubRequest.PublishRequestTime);
    return Results.Ok(pubRequest);
});

await app.RunAsync();

/*
 *  dapr run --app-id subapi --app-port 5148 --resources-path ./Config/Components -- dotnet run
 */
