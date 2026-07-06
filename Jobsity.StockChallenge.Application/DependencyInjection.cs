using Jobsity.StockChallenge.Application.Chat.Commands;
using Jobsity.StockChallenge.Application.Chat.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Jobsity.StockChallenge.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ISendChatMessageHandler, SendChatMessageHandler>();
            services.AddScoped<ISaveBotMessageHandler, SaveBotMessageHandler>();
            services.AddScoped<IGetRecentChatMessagesHandler, GetRecentChatMessagesHandler>();

            return services;
        }
    }
}
