using System.Collections.Concurrent;
using ASISYA.Models.BulkImport;

namespace ASISYA.Services
{
    // Almacena el estado de los jobs en memoria. Suficiente para un solo
    // servidor; si escalas a múltiples instancias, reemplazar por Redis
    // o una tabla en base de datos.
    public interface IBulkImportJobStore
    {
        BulkImportJob Create(string filePath);
        BulkImportJob? Get(Guid jobId);
        void Update(BulkImportJob job);
    }


    public class InMemoryBulkImportJobStore : IBulkImportJobStore
    {
        private readonly ConcurrentDictionary<Guid, BulkImportJob> _jobs = new();

        public BulkImportJob Create(string filePath)
        {
            var job = new BulkImportJob { FilePath = filePath };
            _jobs[job.JobId] = job;
            return job;
        }

        public BulkImportJob? Get(Guid jobId)
        {
            _jobs.TryGetValue(jobId, out var job);
            return job;
        }

        public void Update(BulkImportJob job)
        {
            _jobs[job.JobId] = job;
        }
    }



}
