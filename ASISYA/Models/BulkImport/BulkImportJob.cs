namespace ASISYA.Models.BulkImport
{
    public enum BulkImportStatus
    {
        Pending,
        Running,
        Completed,
        CompletedWithErrors,
        Failed
    }

    public class BulkImportJob
    {
        public Guid JobId { get; set; } = Guid.NewGuid();
        public BulkImportStatus Status { get; set; } = BulkImportStatus.Pending;
        public string FilePath { get; set; } = string.Empty;

        public int TotalRows { get; set; }
        public int ProcessedRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }

        public List<BulkImportRowError> Errors { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? GeneralErrorMessage { get; set; }
    }

    public class BulkImportRowError
    {
        public int RowNumber { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

}
