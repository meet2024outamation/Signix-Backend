using Signix.API.Models.Messages;

namespace Signix.API.Infrastructure.Messaging;

public interface IRabbitMqService
{
    Task PublishDocumentSignedMessageAsync(DocumentSignedMessage message);
    Task PublishMessageAsync<T>(T message, string queueName, string routingKey = "") where T : class;
}