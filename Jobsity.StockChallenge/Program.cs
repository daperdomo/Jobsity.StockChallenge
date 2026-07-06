using Jobsity.StockChallenge.Application;
using Jobsity.StockChallenge.Application.Chat;
using Jobsity.StockChallenge.Hubs;
using Jobsity.StockChallenge.Infrastructure;
using Jobsity.StockChallenge.Infrastructure.Migrations;
using Jobsity.StockChallenge.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddScoped<IChatNotificationService, SignalRChatNotificationService>();

var app = builder.Build();

await app.Services.MigrateDatabaseAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapHub<ChatHub>("/chatHub");

app.Run();
