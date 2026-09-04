namespace ASISYA.DTOs.Product
{
    public class BulkImportStatusDto
    {
        public Guid JobId { get; set; }
        public string Status { get; set; } = string.Empty;

        public int TotalRows { get; set; }
        public int ProcessedRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public double ProgressPercentage { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? GeneralErrorMessage { get; set; }

        // Solo las primeras N filas con error, para no devolver un JSON gigante
        public List<BulkImportRowErrorDto> SampleErrors { get; set; } = new();
    }

    public class BulkImportRowErrorDto
    {
        public int RowNumber { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

}
