using Dapr.Client;
using Common.Models.Requests;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ConsoleApp.Models.Requests;

var httpClient = DaprClient.CreateInvokeHttpClient();

List<string> alerts = new() { "Alert1", "Alert2", "Alert3" };

var alertRequest = new AlertRequest
{
    AlertTypes = alerts,
    ClientName = "Generic Client"
};

// Invoke sendAlert endpoint
try
{
    var sendAlertResponse = await httpClient.PostAsJsonAsync("http://alertsapi/sendAlert", alertRequest);

    if (sendAlertResponse.IsSuccessStatusCode)
    {
        Console.WriteLine("Alerts sent successfully.");
    }
    else
    {
        var result = await sendAlertResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Failed to send alerts. Status Code: {sendAlertResponse.StatusCode}. Message: {result}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception during sendAlert: {ex}");
}

// Invoke sendSms endpoint
try
{
    var smsRequest = new SmsRequest
    {
        PhoneNumber = "+1234567890"
    };
    
    var sendSmsResponse = await httpClient.PostAsJsonAsync("http://alertsapi/sendSms", smsRequest);

    if (sendSmsResponse.IsSuccessStatusCode)
    {
        Console.WriteLine("SMS sent successfully.");
    }
    else
    {
        var result = await sendSmsResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Failed to send SMS. Status Code: {sendSmsResponse.StatusCode}. Message: {result}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception during sendSms: {ex.Message}");
}

// Invoke sendEmail endpoint
try
{
    var emailRequest = new EmailRequest()
    {
        EmailAddress = "example@gmail.com"
    };
    
    var sendEmailResponse = await httpClient.PostAsJsonAsync("http://alertsapi/sendEmail", emailRequest); 

    if (sendEmailResponse.IsSuccessStatusCode)
    {
        Console.WriteLine("Email sent successfully.");
    }
    else
    {
        var result = await sendEmailResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Failed to send email. Status Code: {sendEmailResponse.StatusCode}. Message: {result}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception during sendEmail: {ex.Message}");
}

Console.ReadLine();


/*
 *  dapr run --app-id consoleapp -- dotnet run
 */