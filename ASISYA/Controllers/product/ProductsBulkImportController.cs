using ASISYA.DTOs.Product;
using ASISYA.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASISYA.Controllers.product
{
    [ApiController]
    [Route("api/products/bulk-import")]
    public class ProductsBulkImportController : ControllerBase
    {
        private const long MaxFileSizeBytes = 200 * 1024 * 1024; // 200 MB

        private readonly IBulkImportJobStore _jobStore;
        private readonly IBulkImportQueue _queue;
        private readonly IWebHostEnvironment _env;

        public ProductsBulkImportController(
            IBulkImportJobStore jobStore,
            IBulkImportQueue queue,
            IWebHostEnvironment env)
        {
            _jobStore = jobStore;
            _queue = queue;
            _env = env;
        }

        // POST: api/products/bulk-import
        // Recibe un archivo CSV, lo guarda temporalmente y encola el procesamiento.
        [HttpPost]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Debes adjuntar un archivo CSV.");
            }

            if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("El archivo debe tener extensión .csv");
            }

            // Carpeta temporal para archivos de importación en proceso
            var tempFolder = Path.Combine(_env.ContentRootPath, "temp-imports");
            Directory.CreateDirectory(tempFolder);

            var tempFilePath = Path.Combine(tempFolder, $"{Guid.NewGuid()}.csv");

            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var job = _jobStore.Create(tempFilePath);
            await _queue.EnqueueAsync(job.JobId);

            var statusUrl = Url.Action(nameof(GetStatus), new { jobId = job.JobId });

            // 202 Accepted: el trabajo fue recibido, pero se procesa en background
            return Accepted(statusUrl, new { jobId = job.JobId, statusUrl });
        }

        // GET: api/products/bulk-import/{jobId}/status
        [HttpGet("{jobId}/status")]
        public ActionResult<BulkImportStatusDto> GetStatus(Guid jobId)
        {
            var job = _jobStore.Get(jobId);

            if (job == null)
            {
                return NotFound($"No se encontró un job de importación con ID {jobId}.");
            }

            var dto = new BulkImportStatusDto
            {
                JobId = job.JobId,
                Status = job.Status.ToString(),
                TotalRows = job.TotalRows,
                ProcessedRows = job.ProcessedRows,
                SuccessCount = job.SuccessCount,
                ErrorCount = job.ErrorCount,
                ProgressPercentage = job.ProcessedRows == 0
                    ? 0
                    : Math.Round(job.ProcessedRows / (double)job.TotalRows * 100, 1),
                CreatedAt = job.CreatedAt,
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt,
                GeneralErrorMessage = job.GeneralErrorMessage,
                SampleErrors = job.Errors
                    .Take(50)
                    .Select(e => new BulkImportRowErrorDto { RowNumber = e.RowNumber, Reason = e.Reason })
                    .ToList()
            };

            return dto;
        }
    }
}
