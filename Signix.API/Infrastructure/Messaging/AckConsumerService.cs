using Microsoft.EntityFrameworkCore;
using Signix.API.Models;
using Signix.API.Models.Messages;
using Signix.Entities.Context;

namespace Signix.API.Infrastructure.Messaging
{
    public class AckConsumerService : BackgroundService
    {
        private readonly ILogger<AckConsumerService> _logger;
        private readonly IRabbitMQService _rabbitMQ;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly string _queueName = Meta.RabbitMQ.AckQueueName;
        public AckConsumerService(
            ILogger<AckConsumerService> logger,
            IRabbitMQService rabbitMQ,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _rabbitMQ = rabbitMQ;
            _serviceScopeFactory = serviceScopeFactory;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting DocumentAckConsumer...");

            _rabbitMQ.StartConsuming<AcknowledgmentMessage>(_queueName, async (message) =>
            {
                await ProcessMessageAsync(message);
            });

            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(AcknowledgmentMessage ackMessage)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<SignixDbContext>();
                _logger.LogInformation("Processing acknowledgment for SigningRoomId: {SigningRoomId}, Status: {Status}, ProcessedDocuments: {Count}",
                    ackMessage.SigningRoomId, ackMessage.Status, ackMessage.ProcessedDocuments.Count);

                var signingRoom = await dbContext.SigningRooms
                    .Include(sr => sr.Documents)
                        .ThenInclude(d => d.DocumentStatus)
                    .FirstOrDefaultAsync(sr => sr.Id == ackMessage.SigningRoomId);

                if (signingRoom == null)
                {
                    _logger.LogWarning("Signing room not found for acknowledgment. SigningRoomId: {SigningRoomId}",
                        ackMessage.SigningRoomId);
                    return;
                }

                string targetStatusName = ackMessage.Status.ToLower() switch
                {
                    "completed" => Meta.DocumentStatus.Signed,
                    "failed" => Meta.DocumentStatus.Failed,
                    _ => Meta.DocumentStatus.Pending
                };

                var targetStatus = await dbContext.DocumentStatuses
                    .FirstOrDefaultAsync(ds => ds.Name == targetStatusName);

                if (targetStatus == null)
                {
                    _logger.LogError("Document status '{StatusName}' not found in database", targetStatusName);
                    return;
                }
                var updatedDocumentsCount = 0;
                foreach (var processedDoc in ackMessage.ProcessedDocuments)
                {
                    var document = signingRoom.Documents
                        .FirstOrDefault(d => d.Name == processedDoc.Name);

                    if (document != null)
                    {
                        document.DocumentStatusId = targetStatus.Id;
                        updatedDocumentsCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Document '{DocumentName}' not found in signing room {SigningRoomId}",
                            processedDoc.Name, ackMessage.SigningRoomId);
                    }
                }
                if (ackMessage.Status.ToLower() == "completed")
                {
                    signingRoom.CompletedAt = ackMessage.Timestamp;

                    //var firstSignedDoc = ackMessage.ProcessedDocuments.FirstOrDefault(pd => !string.IsNullOrEmpty(pd.SignedPath));
                    //if (firstSignedDoc != null)
                    //{
                    //    var directory = Path.GetDirectoryName(firstSignedDoc.SignedPath);
                    //    if (!string.IsNullOrEmpty(directory))
                    //    {
                    //        signingRoom.SignedPath = directory;
                    //    }
                    //}
                }
                else if (ackMessage.Status.ToLower() == "failed")
                {
                    _logger.LogWarning("Signing room {SigningRoomId} processing failed", signingRoom.Id);
                }

                await dbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully processed acknowledgment for SigningRoomId: {SigningRoomId}. " +
                    "Updated {DocumentCount} documents to '{Status}' status",
                    ackMessage.SigningRoomId, updatedDocumentsCount, targetStatusName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing acknowledgment message for SigningRoomId: {SigningRoomId}",
                    ackMessage.SigningRoomId);
                throw;
            }
        }
    }
}
