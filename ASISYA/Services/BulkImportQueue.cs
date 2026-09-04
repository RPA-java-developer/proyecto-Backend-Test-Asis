using System.Threading.Channels;

namespace ASISYA.Services
{
    public interface IBulkImportQueue
    {
        ValueTask EnqueueAsync(Guid jobId);
        IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken);
    }

    public class BulkImportQueue : IBulkImportQueue
    {
        // Canal ilimitado: los jobs son pocos (uno por importación),
        // no miles de mensajes pequeños.
        private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

        public async ValueTask EnqueueAsync(Guid jobId)
        {
            await _channel.Writer.WriteAsync(jobId);
        }

        public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }

}
