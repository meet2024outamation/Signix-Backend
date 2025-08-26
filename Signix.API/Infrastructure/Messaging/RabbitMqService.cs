using RabbitMQ.Client;
using Signix.API.Models.Messages;
using System.Text;
using System.Text.Json;

namespace Signix.API.Infrastructure.Messaging;

public class RabbitMqService : IRabbitMqService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqService> _logger;
    private readonly string _exchangeName;
    private readonly string _documentSignedQueueName;

    public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
    {
        _logger = logger;
        var rabbitMqConfig = configuration.GetSection("RabbitMQ");
        var hostname = rabbitMqConfig["Hostname"] ?? "localhost";
        var port = rabbitMqConfig.GetValue<int>("Port", 5672);
        var username = rabbitMqConfig["Username"] ?? "guest";
        var password = rabbitMqConfig["Password"] ?? "guest";
        var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/";

        _exchangeName = rabbitMqConfig["ExchangeName"] ?? "signix.documents.exchange";
        _documentSignedQueueName = rabbitMqConfig["DocumentSignedQueueName"] ?? "document.signed.queue";

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = hostname,
                Port = port,
                UserName = username,
                Password = password,
                VirtualHost = virtualHost,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            SetupRabbitMqInfrastructure();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection");
            throw;
        }
    }

    private void SetupRabbitMqInfrastructure()
    {
        try
        {
            // Declare exchange
            _channel.ExchangeDeclareAsync(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false).GetAwaiter().GetResult();

            // Declare queue for document signed events
            _channel.QueueDeclareAsync(
                queue: _documentSignedQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false).GetAwaiter().GetResult();

            // Bind queue to exchange
            _channel.QueueBindAsync(
                queue: _documentSignedQueueName,
                exchange: _exchangeName,
                routingKey: "document.signed").GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup RabbitMQ infrastructure");
            throw;
        }
    }

    public async Task PublishDocumentSignedMessageAsync(DocumentSignedMessage message)
    {
        try
        {
            await PublishMessageAsync(message, _documentSignedQueueName, "document.signed");
            _logger.LogInformation("Document signed message published successfully. CorrelationId: {CorrelationId}, SigningRoomId: {SigningRoomId}, DocumentCount: {DocumentCount}",
                message.CorrelationId, message.SigningRoomId, message.SignedDocuments.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish document signed message. SigningRoomId: {SigningRoomId}", message.SigningRoomId);
            throw;
        }
    }

    public async Task PublishMessageAsync<T>(T message, string queueName, string routingKey = "") where T : class
    {
        try
        {
            if (_channel == null || _channel.IsClosed)
            {
                _logger.LogError("RabbitMQ channel is closed. Cannot publish message.");
                throw new InvalidOperationException("RabbitMQ channel is not available");
            }

            var jsonMessage = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var body = Encoding.UTF8.GetBytes(jsonMessage);
            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                ContentEncoding = "UTF-8",
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Headers = new Dictionary<string, object>
                {
                    ["MessageType"] = typeof(T).Name,
                    ["Source"] = "Signix.API",
                    ["Version"] = "1.0"
                }
            };

            // Publish message
            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogDebug("Message published to queue: {QueueName}, RoutingKey: {RoutingKey}, MessageType: {MessageType}",
                queueName, routingKey, typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to queue: {QueueName}", queueName);
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.CloseAsync().GetAwaiter().GetResult();
            _connection?.CloseAsync().GetAwaiter().GetResult();
            _channel?.Dispose();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQ connection disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection");
        }
    }
}