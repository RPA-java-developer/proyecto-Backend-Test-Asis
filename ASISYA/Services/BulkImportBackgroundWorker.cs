namespace ASISYA.Services
{
    public class BulkImportBackgroundWorker : BackgroundService
    {
        private readonly IBulkImportQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BulkImportBackgroundWorker> _logger;

        public BulkImportBackgroundWorker(
            IBulkImportQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<BulkImportBackgroundWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var jobId in _queue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    // Scope propio por job: crea sus propias instancias de
                    // servicios (incluyendo el DbContext), aislado del
                    // request HTTP que ya terminó.
                    using var scope = _scopeFactory.CreateScope();
                    var importService = scope.ServiceProvider
                        .GetRequiredService<IBulkProductImportService>();

                    await importService.ProcessAsync(jobId, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado procesando el job {JobId}", jobId);
                }
            }
        }
    }
}
