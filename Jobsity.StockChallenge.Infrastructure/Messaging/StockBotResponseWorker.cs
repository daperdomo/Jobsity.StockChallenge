using System.Text;
using Jobsity.StockChallenge.Application.Chat;
using Jobsity.StockChallenge.Application.Chat.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jobsity.StockChallenge.Infrastructure.Messaging
{
    public class StockBotResponseWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StockBotResponseWorker> _logger;

        public StockBotResponseWorker(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<StockBotResponseWorker> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ListenForResponsesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Stock bot response worker is stopping.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not connect to the stock bot response queue. Retrying soon.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task ListenForResponsesAsync(CancellationToken stoppingToken)
        {
            var responseQueue = _configuration["RabbitMq:responseQueue"] ?? "chat.messages";
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMq:host"] ?? "localhost",
                UserName = _configuration["RabbitMq:username"] ?? "guest",
                Password = _configuration["RabbitMq:password"] ?? "guest"
            };

            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(responseQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, args) => await ProcessResponseAsync(channel, args, stoppingToken);

            await channel.BasicConsumeAsync(responseQueue, autoAck: false, consumer, cancellationToken: stoppingToken);
            _logger.LogInformation("Listening for stock bot responses on queue {ResponseQueue}.", responseQueue);
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }

        private async Task ProcessResponseAsync(IChannel channel, BasicDeliverEventArgs args, CancellationToken stoppingToken)
        {
            try
            {
                var botText = Encoding.UTF8.GetString(args.Body.Span);

                using var scope = _scopeFactory.CreateScope();
                var saveBotMessageHandler = scope.ServiceProvider.GetRequiredService<ISaveBotMessageHandler>();
                var notificationService = scope.ServiceProvider.GetRequiredService<IChatNotificationService>();

                var message = await saveBotMessageHandler.HandleAsync(new SaveBotMessageCommand(botText), stoppingToken);
                await notificationService.NotifyMessageAsync(message, stoppingToken);

                await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not process stock bot response.");
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        }
    }
}
