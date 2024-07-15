using AlertApi.RouteBuilders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.RegisterEndpoints();

app.Run();


/*
 *  dapr run --app-id alertsapi --app-port 5140 --app-protocol http --dapr-http-port 3500 --resources-path ./Config/Components -- dotnet run
 */