using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Partlyx.Infrastructure.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public record ImportResult(bool Success, string? ErrorMessage = null, string? DiagnosticFile = null);

    public class DBLoader : IDBLoader
    {
        private readonly IDBProvider _dbProvider;

        public DBLoader(IDBProvider dbProvider)
        {
            _dbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        }


        public async Task<ImportResult> ImportPartreeAsync(string partreePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(partreePath)) throw new ArgumentException("partreePath required", nameof(partreePath));
            if (!File.Exists(partreePath)) return new ImportResult(false, "File not found");

            await _dbProvider.DBExportLoadSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            string tempDir = Path.Combine(Path.GetTempPath(), "partree_import_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            string tempExtractDir = tempDir;
            string sourceDbPath = Path.Combine(tempExtractDir, "database.db");
            string manifestPath = Path.Combine(tempExtractDir, "manifest.json");

            try
            {
                ZipFile.ExtractToDirectory(partreePath, tempExtractDir);

                if (!File.Exists(sourceDbPath))
                    return new ImportResult(false, "Archive doesn't contain database.db", partreePath);

                if (File.Exists(manifestPath))
                {
                    try
                    {
                        var manifestJson = await File.ReadAllTextAsync(manifestPath, cancellationToken).ConfigureAwait(false);
                        using var doc = JsonDocument.Parse(manifestJson);

                        if (!doc.RootElement.TryGetProperty("TimestampUtc", out _)
                            && !doc.RootElement.TryGetProperty("SchemaVersion", out _))
                        {
                            Trace.WriteLine("Invalid .partree manifest at path: " + partreePath);
                        }
                    }
                    catch (JsonException exc) { Trace.WriteLine(exc); }
                }

                var srcConnStr = $"Data Source={sourceDbPath};Pooling=False";
                var dstConnStr = $"Data Source={_dbProvider.CurrentDbPath}";

                await Task.Run(() =>
                {
                    using var src = new SqliteConnection(srcConnStr);
                    using var dst = new SqliteConnection(dstConnStr);

                    src.Open();
                    dst.Open();

                    src.BackupDatabase(dst);
                }, cancellationToken).ConfigureAwait(false);

                try
                {
                    _dbProvider.NotifyDatabaseReplaced();
                }
                catch (Exception exNotify)
                {
                    return new ImportResult(false, $"Succesfull import, but NotifyDatabaseReplaced() thrown an exception: {exNotify.Message}");
                }

                try
                {
                    _dbProvider.NotifyDatabaseClosed();
                }
                catch (Exception exNotify)
                {
                    return new ImportResult(false, $"Succesfull import, but NotifyDatabaseClosed() thrown an exception: {exNotify.Message}");
                }

                return new ImportResult(true);
            }
            catch (OperationCanceledException)
            {
                return new ImportResult(false, "Operation cancelled");
            }
            catch (Exception ex)
            {
                return new ImportResult(false, ex.Message, partreePath);
            }
            finally
            {
                try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { /* ignore */ }
                _dbProvider.DBExportLoadSemaphore.Release();
            }
        }
    }
}
