using Jobsity.StockChallenge.Bot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Jobsity.StockChallenge.Bot
{
    internal class StockWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StockWorker> _logger;
        private readonly IStockService _stockService;

        public StockWorker(IConfiguration configuration, ILogger<StockWorker> logger, IStockService stockService)
        {
            _configuration = configuration;
            _logger = logger;
            _stockService = stockService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ListenForRequestsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Stock bot is stopping.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not connect to the stock command queue. Retrying soon.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task ListenForRequestsAsync(CancellationToken stoppingToken)
        {
            var requestQueue = _configuration["RabbitMq:requestQueue"] ?? "stock.commands";
            var responseQueue = _configuration["RabbitMq:responseQueue"] ?? "chat.messages";
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMq:host"] ?? "localhost",
                UserName = _configuration["RabbitMq:username"] ?? "guest",
                Password = _configuration["RabbitMq:password"] ?? "guest"
            };

            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(requestQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync(responseQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, args) =>
            {
                var responseTarget = string.IsNullOrWhiteSpace(args.BasicProperties.ReplyTo)
                    ? responseQueue
                    : args.BasicProperties.ReplyTo;

                try
                {
                    var command = GetStockCommand(Encoding.UTF8.GetString(args.Body.Span));
                    var response = await _stockService.GetStockQuoteFromAlphaVantage(command.StockSymbol);
                    var payload = JsonSerializer.Serialize(new
                    {
                        message = response,
                        chatRoom = command.ChatRoom
                    });

                    var properties = new BasicProperties
                    {
                        ContentType = "application/json",
                        DeliveryMode = DeliveryModes.Persistent,
                        CorrelationId = args.BasicProperties.CorrelationId
                    };

                    await channel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: responseTarget,
                        mandatory: false,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes(payload),
                        cancellationToken: stoppingToken);

                    await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not process stock command message.");
                    await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                }
            };

            await channel.BasicConsumeAsync(requestQueue, autoAck: false, consumer, cancellationToken: stoppingToken);
            _logger.LogInformation(
                "Stock bot listening on queue {RequestQueue} and publishing to {ResponseQueue}.",
                requestQueue,
                responseQueue);

            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }

        private static StockCommand GetStockCommand(string body)
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var stockSymbol = root.GetProperty("message").GetString() ?? string.Empty;
            var chatRoom = root.TryGetProperty("chatRoom", out var chatRoomProperty)
                ? chatRoomProperty.GetString() ?? "General"
                : "General";

            return new StockCommand(stockSymbol, chatRoom);
        }

        private sealed record StockCommand(string StockSymbol, string ChatRoom);
    }
}
