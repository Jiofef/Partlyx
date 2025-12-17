using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public class StreamImageProvider
    {
        readonly DbContext _ctx;
        public StreamImageProvider(DbContext ctx) => _ctx = ctx;

        public async Task<Stream> OpenStreamAsync(Guid uid, CancellationToken ct = default)
        {
            var conn = _ctx.Database.GetDbConnection();
            await conn.OpenAsync(ct).ConfigureAwait(false);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Content FROM Images WHERE Uid = @uid";
            var p = cmd.CreateParameter();
            p.ParameterName = "@uid";
            p.Value = uid;
            cmd.Parameters.Add(p);

            try
            {
                var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow, ct).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    await reader.DisposeAsync().ConfigureAwait(false);
                    await conn.DisposeAsync().ConfigureAwait(false);
                    throw new KeyNotFoundException();
                }

                var fieldStream = reader.GetStream(0);

                return new ReaderBackedStream(fieldStream, reader, conn);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Cannot open a stream. Exception: " + ex.Message);
            }

            return null!;
        }

        class ReaderBackedStream : Stream
        {
            readonly Stream _inner;
            readonly DbDataReader _reader;
            readonly DbConnection _conn;
            bool _disposed;
            public ReaderBackedStream(Stream inner, DbDataReader reader, DbConnection conn)
            {
                _inner = inner;
                _reader = reader;
                _conn = conn;
            }
            protected override void Dispose(bool disposing)
            {
                if (_disposed) return;
                if (disposing)
                {
                    _inner.Dispose();
                    _reader.Dispose();
                    _conn.Dispose();
                }
                _disposed = true;
                base.Dispose(disposing);
            }
            public override async ValueTask DisposeAsync()
            {
                await _inner.DisposeAsync().ConfigureAwait(false);
                await _reader.DisposeAsync().ConfigureAwait(false);
                await _conn.DisposeAsync().ConfigureAwait(false);
                await base.DisposeAsync().ConfigureAwait(false);
            }

            public override bool CanRead => _inner.CanRead;
            public override bool CanSeek => _inner.CanSeek;
            public override bool CanWrite => _inner.CanWrite;
            public override long Length => _inner.Length;
            public override long Position { get => _inner.Position; set => _inner.Position = value; }
            public override void Flush() => _inner.Flush();
            public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
            public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
            public override void SetLength(long value) => _inner.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
                await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }
    }
}