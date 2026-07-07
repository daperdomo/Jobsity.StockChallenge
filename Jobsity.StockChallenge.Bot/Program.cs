using Jobsity.StockChallenge.Bot;
using Jobsity.StockChallenge.Bot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddSingleton<IStockService, StockService>();
builder.Services.AddHostedService<StockWorker>();

var host = builder.Build();
host.Run();
