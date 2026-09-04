using CsvHelper;
using CsvHelper.Configuration;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ASISYA.DTOs.Product;
using ASISYA.Data;
using ASISYA.Models;
using ASISYA.Models.BulkImport;



namespace ASISYA.Services
{

    public interface IBulkProductImportService
    {
        Task ProcessAsync(Guid jobId, CancellationToken cancellationToken);
    }

    public class BulkProductImportService : IBulkProductImportService
    {
        private const int BatchSize = 2000;

        private readonly IDbContextFactory<AppDBContext> _contextFactory;
        private readonly IBulkImportJobStore _jobStore;
        private readonly ILogger<BulkProductImportService> _logger;

        public BulkProductImportService(
            IDbContextFactory<AppDBContext> contextFactory,
            IBulkImportJobStore jobStore,
            ILogger<BulkProductImportService> logger)
        {
            _contextFactory = contextFactory;
            _jobStore = jobStore;
            _logger = logger;
        }

        public async Task ProcessAsync(Guid jobId, CancellationToken cancellationToken)
        {
            var job = _jobStore.Get(jobId);
            if (job == null)
            {
                _logger.LogWarning("Job {JobId} no encontrado en el store.", jobId);
                return;
            }

            job.Status = BulkImportStatus.Running;
            job.StartedAt = DateTime.UtcNow;
            _jobStore.Update(job);

            try
            {
                // Un DbContext exclusivo para este job (no compartido con requests HTTP)
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

                // Conteo rápido de filas (menos el encabezado) para que
                // TotalRows y el progreso reportado sean reales, no 0.
                job.TotalRows = CountDataRows(job.FilePath);
                _jobStore.Update(job);

                // Precarga de IDs válidos en memoria: evita una consulta a la BD
                // por cada una de las 100,000 filas (N+1).
                var categoriasValidas = (await context.Categories
                    .Select(c => c.CategoryID)
                    .ToListAsync(cancellationToken))
                    .ToHashSet();

                var proveedoresValidos = (await context.Suppliers
                    .Select(s => s.SupplierID)
                    .ToListAsync(cancellationToken))
                    .ToHashSet();

                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    HeaderValidated = null
                };

                using var reader = new StreamReader(job.FilePath);
                using var csv = new CsvReader(reader, csvConfig);

                var batch = new List<Product>(BatchSize);
                int rowNumber = 1; // fila 1 = primer registro después del encabezado

                await foreach (var row in csv.GetRecordsAsync<ProductImportRowDto>(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var (esValido, error) = ValidarFila(row, categoriasValidas, proveedoresValidos);

                    if (!esValido)
                    {
                        job.ErrorCount++;
                        job.Errors.Add(new BulkImportRowError { RowNumber = rowNumber, Reason = error! });
                    }
                    else
                    {
                        batch.Add(MapearAProducto(row));
                    }

                    job.ProcessedRows++;
                    rowNumber++;

                    if (batch.Count >= BatchSize)
                    {
                        await InsertarLoteAsync(context, batch, cancellationToken);
                        job.SuccessCount += batch.Count;
                        batch.Clear();

                        // Reporta progreso periódicamente (no en cada fila, sería costoso)
                        _jobStore.Update(job);
                    }
                }

                // Último lote incompleto
                if (batch.Count > 0)
                {
                    await InsertarLoteAsync(context, batch, cancellationToken);
                    job.SuccessCount += batch.Count;
                }

                job.Status = job.ErrorCount > 0
                    ? BulkImportStatus.CompletedWithErrors
                    : BulkImportStatus.Completed;
            }
            catch (OperationCanceledException)
            {
                job.Status = BulkImportStatus.Failed;
                job.GeneralErrorMessage = "El proceso fue cancelado.";
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando el job de importación {JobId}", jobId);
                job.Status = BulkImportStatus.Failed;
                job.GeneralErrorMessage = ex.Message;
            }
            finally
            {
                job.CompletedAt = DateTime.UtcNow;
                _jobStore.Update(job);

                // Limpieza del archivo temporal
                if (File.Exists(job.FilePath))
                {
                    File.Delete(job.FilePath);
                }
            }
        }

        private static int CountDataRows(string filePath)
        {
            using var reader = new StreamReader(filePath);
            int count = 0;
            bool isHeader = true;

            while (reader.ReadLine() != null)
            {
                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }
                count++;
            }

            return count;
        }

        private static (bool esValido, string? error) ValidarFila(
            ProductImportRowDto row,
            HashSet<int> categoriasValidas,
            HashSet<int> proveedoresValidos)
        {
            if (string.IsNullOrWhiteSpace(row.ProductName))
            {
                return (false, "ProductName es requerido.");
            }

            if (!categoriasValidas.Contains(row.CategoryID))
            {
                return (false, $"CategoryID {row.CategoryID} no existe.");
            }

            if (row.SupplierID.HasValue && !proveedoresValidos.Contains(row.SupplierID.Value))
            {
                return (false, $"SupplierID {row.SupplierID} no existe.");
            }

            if (row.UnitPrice.HasValue && row.UnitPrice < 0)
            {
                return (false, "UnitPrice no puede ser negativo.");
            }

            return (true, null);
        }

        private static Product MapearAProducto(ProductImportRowDto row) => new Product
        {
            ProductName = row.ProductName,
            CategoryID = row.CategoryID,
            SupplierID = row.SupplierID,
            QuantityPerUnit = row.QuantityPerUnit,
            UnitPrice = row.UnitPrice,
            UnitsInStock = row.UnitsInStock,
            UnitsOnOrder = row.UnitsOnOrder,
            ReorderLevel = row.ReorderLevel,
            Discontinued = row.Discontinued
        };

        private static async Task InsertarLoteAsync(
            AppDBContext context,
            List<Product> batch,
            CancellationToken cancellationToken)
        {
            // BulkInsertAsync usa inserción masiva nativa (bulk copy),
            // muchísimo más rápido que AddRange + SaveChangesAsync
            // para volúmenes grandes.
            await context.BulkInsertAsync(batch, options => { }, cancellationToken: cancellationToken);
        }
    }


}
