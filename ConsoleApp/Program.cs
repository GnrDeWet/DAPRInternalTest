using Dapr.Client;
using Common.Models.Requests;
using System.Net.Http.Json;

var client = new DaprClientBuilder().Build();

List<string> alerts = new() { "Alert1", "Alert2", "Alert3" };

var alertRequest = new AlertRequest
{
    AlertTypes = alerts,
    ClientName = "Generic Client"
};

// Invoke sendAlert endpoint
try
{
    var sendAlertResponse = await client.InvokeMethodAsync<AlertRequest, HttpResponseMessage>(
        HttpMethod.Post,
        "alertsapi",
        "sendAlert",
        alertRequest);

    if (sendAlertResponse.IsSuccessStatusCode)
    {
        Console.WriteLine("Alerts sent successfully.");
    }
    else
    {
        Console.WriteLine($"Failed to send alerts. Status Code: {sendAlertResponse.StatusCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception during sendAlert: {ex.ToString()}");
}

// Invoke sendSms endpoint
try
{
    var smsRequest = new
    {
        PhoneNumber = "+1234567890"
    };

    var sendSmsResponse = await client.InvokeMethodAsync<object, HttpResponseMessage>(
        HttpMethod.Post,
        "alertsapi",
        "sendSms",
        smsRequest);

    if (sendSmsResponse.IsSuccessStatusCode)
    {
        Console.WriteLine("SMS sent successfully.");
    }
    else
    {
        Console.WriteLine($"Failed to send SMS. Status Code: {sendSmsResponse.StatusCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception during sendSms: {ex.Message}");
}

// Invoke sendEmail endpoint
try
{
    var emailRequest = new
    {
        EmailAddress = "gnrdewet@gmail.com"
    };

    var sendEmailResponse = await client.InvokeMethodAsync<object, HttpResponseMessage>(
        HttpMethod.Post,
        "alertsapi",
        "sendEmail",
        emailRequest);

    if (sendEmailResponse.IsSuccessStatusCode)
    {
        Console.WriteLine("Email sent successfully.");
    }
    else
    {
        Console.WriteLine($"Failed to send email. Status Code: {sendEmailResponse.StatusCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception during sendEmail: {ex.Message}");
}

Console.ReadLine();
