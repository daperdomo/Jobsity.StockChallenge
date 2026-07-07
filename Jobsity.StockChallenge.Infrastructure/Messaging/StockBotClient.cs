using System.Text;
using System.Text.Json;
using Jobsity.StockChallenge.Application.Stocks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Jobsity.StockChallenge.Infrastructure.Messaging
{
    public class StockBotClient : IStockQuoteRequestPublisher
    {
        private readonly IConfiguration _configuration;

        public StockBotClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task RequestStockQuoteAsync(string stockSymbol, string chatRoom, CancellationToken cancellationToken = default)
        {
            var queueName = _configuration["RabbitMq:requestQueue"] ?? "stock.commands";
            var factory = CreateConnectionFactory();

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

            var payload = JsonSerializer.Serialize(new { message = stockSymbol, chatRoom });
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: Encoding.UTF8.GetBytes(payload),
                cancellationToken: cancellationToken);
        }

        private ConnectionFactory CreateConnectionFactory()
        {
            return new ConnectionFactory
            {
                HostName = _configuration["RabbitMq:host"] ?? "localhost",
                UserName = _configuration["RabbitMq:username"] ?? "guest",
                Password = _configuration["RabbitMq:password"] ?? "guest"
            };
        }
    }
}
