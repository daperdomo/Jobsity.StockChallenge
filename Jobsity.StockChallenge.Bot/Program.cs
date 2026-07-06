using Jobsity.StockChallenge.Bot;
using Jobsity.StockChallenge.Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IStockService, StockService>();
builder.Services.AddHostedService<StockWorker>();
var host = builder.Build();
host.Run();