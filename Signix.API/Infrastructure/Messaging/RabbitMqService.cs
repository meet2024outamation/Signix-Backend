using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using static Signix.API.Models.Meta;

namespace Signix.API.Infrastructure.Messaging;

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQService> _logger;
    private readonly ConcurrentDictionary<string, string> _consumers = new();
    private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);
    private bool _disposed = false;
    private bool _initialized = false;

    public RabbitMQService(IOptions<RabbitMQSettings> settings, ILogger<RabbitMQService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        await _initializationSemaphore.WaitAsync();
        try
        {
            if (_initialized) return;

            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                VirtualHost = _settings.VirtualHost,
                UserName = _settings.Username,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            _initialized = true;

            _logger.LogInformation("RabbitMQ connection established to {Host}/{VirtualHost}",
                _settings.Host, _settings.VirtualHost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection to {Host}/{VirtualHost}",
                _settings.Host, _settings.VirtualHost);
            throw;
        }
        finally
        {
            _initializationSemaphore.Release();
        }
    }

    public async Task PublishAsync<T>(string queueName, T message)
    {
        await EnsureInitializedAsync();

        try
        {
            await DeclareQueueAsync(queueName);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await _channel!.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);

            _logger.LogInformation("Message published to queue {QueueName}: {MessageType}",
                queueName, typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to queue {QueueName}: {MessageType}",
                queueName, typeof(T).Name);
            throw;
        }
    }

    public void StartConsuming<T>(string queueName, Func<T, Task> onMessage)
    {
        // We need to ensure initialization synchronously for this method
        EnsureInitializedAsync().GetAwaiter().GetResult();

        try
        {
            //if (_consumers.ContainsKey(queueName))
            //{
            //    _logger.LogWarning("Consumer for queue {QueueName} is already running", queueName);
            //    return;
            //}

            DeclareQueueAsync(queueName).GetAwaiter().GetResult();

            var consumer = new AsyncEventingBasicConsumer(_channel!);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var deserializedMessage = JsonSerializer.Deserialize<T>(json);

                    if (deserializedMessage != null)
                    {
                        await onMessage(deserializedMessage);
                        await _channel!.BasicAckAsync(ea.DeliveryTag, false);

                        _logger.LogDebug("Message processed from queue {QueueName}: {MessageType}",
                            queueName, typeof(T).Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue {QueueName}: {MessageType}",
                        queueName, typeof(T).Name);

                    // Reject message and don't requeue on processing errors
                    await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            var consumerTag = _channel!.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer).GetAwaiter().GetResult();
            _consumers.TryAdd(queueName, consumerTag);

            _logger.LogInformation("Started consuming from queue {QueueName} with consumer tag {ConsumerTag}",
                queueName, consumerTag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consuming from queue {QueueName}", queueName);
            throw;
        }
    }

    public void StopConsuming(string queueName)
    {
        try
        {
            if (_consumers.TryRemove(queueName, out var consumerTag))
            {
                _channel?.BasicCancelAsync(consumerTag).GetAwaiter().GetResult();
                _logger.LogInformation("Stopped consuming from queue {QueueName}", queueName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop consuming from queue {QueueName}", queueName);
        }
    }

    public void DeclareQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false)
    {
        DeclareQueueAsync(queueName, durable, exclusive, autoDelete).GetAwaiter().GetResult();
    }

    private async Task DeclareQueueAsync(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false)
    {
        await EnsureInitializedAsync();

        if (_channel == null)
        {
            throw new InvalidOperationException("RabbitMQ channel is not initialized.");
        }
        try
        {
            await _channel!.QueueDeclareAsync(queue: queueName, durable: durable, exclusive: exclusive, autoDelete: autoDelete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to declare queue {QueueName}", queueName);
            throw;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                // Stop all consumers
                foreach (var queueName in _consumers.Keys.ToList())
                {
                    StopConsuming(queueName);
                }

                _channel?.Dispose();
                _connection?.Dispose();
                _initializationSemaphore?.Dispose();

                _logger.LogInformation("RabbitMQ connection disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection");
            }

            _disposed = true;
        }
    }
}