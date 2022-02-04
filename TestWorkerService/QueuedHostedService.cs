namespace TestWorkerService
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly IBackgroundTaskQueue taskQueue;
        private readonly ILogger<QueuedHostedService> logger;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostedService> logger)
        => (this.taskQueue, this.logger) = (taskQueue, logger);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation(
            $"{nameof(QueuedHostedService)} is running.{Environment.NewLine}" +
            $"{Environment.NewLine}Tap W to add a work item to the " +
            $"background queue.{Environment.NewLine}");

            return ProcessTaskQueueAsync(stoppingToken);
        }

        private async Task ProcessTaskQueueAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Func<CancellationToken, ValueTask>? workItem =
                        await taskQueue.DequeueAsync(cancellationToken);

                    await workItem(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Error occurred executing task work item.");
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"{nameof(QueuedHostedService)} is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}