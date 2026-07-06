using Jobsity.StockChallenge.Application.Chat;
using Jobsity.StockChallenge.Application.Stocks;
using Jobsity.StockChallenge.Infrastructure.Data;
using Jobsity.StockChallenge.Infrastructure.Messaging;
using Jobsity.StockChallenge.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jobsity.StockChallenge.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
            });

            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddSingleton<IStockQuoteRequestPublisher, StockBotClient>();
            services.AddHostedService<StockBotResponseWorker>();

            return services;
        }
    }
}
