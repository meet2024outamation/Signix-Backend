namespace Signix.API.Infrastructure.Messaging;

public interface IRabbitMQService
{
    /// <summary>
    /// Publishes a message to the specified queue
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name</param>
    /// <param name="message">Message to publish</param>
    Task PublishAsync<T>(string queueName, T message);

    /// <summary>
    /// Starts consuming messages from the specified queue
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="queueName">Queue name</param>
    /// <param name="onMessage">Callback to handle received messages</param>
    void StartConsuming<T>(string queueName, Func<T, Task> onMessage);

    /// <summary>
    /// Stops consuming messages from the specified queue
    /// </summary>
    /// <param name="queueName">Queue name</param>
    void StopConsuming(string queueName);

    /// <summary>
    /// Declares a queue with the specified configuration
    /// </summary>
    /// <param name="queueName">Queue name</param>
    /// <param name="durable">Whether the queue should survive broker restarts</param>
    /// <param name="exclusive">Whether the queue is used by only one connection</param>
    /// <param name="autoDelete">Whether to delete the queue when it's no longer in use</param>
    void DeclareQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false);
}