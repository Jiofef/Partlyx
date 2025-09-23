using Microsoft.Data.Sqlite;
using Partlyx.Infrastructure.Data.CommonFileEvents;
using Partlyx.Infrastructure.Data.Interfaces;
using Partlyx.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Partlyx.Infrastructure.Data.Implementations
{
    public record ExportResult(bool Success, string? ErrorMessage = null, string? TempBackupPath = null);


    public class DBSaver : IDBSaver
    {
        private readonly IDBProvider _dbProvider;

        public DBSaver(IDBProvider dbProvider)
        {
            _dbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        }

        public async Task<ExportResult> ExportPartreeAsync(string targetPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(targetPath)) throw new ArgumentException("targetPath required", nameof(targetPath));

            await _dbProvider.DBExportLoadSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            string tempDir = Path.Combine(Path.GetTempPath(), "partree_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            string tempDbPath = Path.Combine(tempDir, "database.db");
            string manifestPath = Path.Combine(tempDir, "manifest.json");

            string targetDir = Path.GetDirectoryName(targetPath) ?? Path.GetTempPath();
            string tempZipPath = Path.Combine(targetDir, Path.GetRandomFileName() + ".partree.tmp");

            try
            {
                var srcConnStr = $"Data Source={_dbProvider.CurrentDbPath}";

                await Task.Run(() =>
                {
                    using var src = new SqliteConnection(srcConnStr);
                    using var dst = new SqliteConnection($"Data Source={tempDbPath};Pooling=False");

                    src.Open();
                    dst.Open();

                    long schemaVersion = 0;
                    using (var cmd = src.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA user_version;";
                        var scalar = cmd.ExecuteScalar();
                        if (scalar != null && long.TryParse(scalar.ToString(), out var v))
                            schemaVersion = v;
                    }

                    src.BackupDatabase(dst);

                    var manifest = new
                    {
                        App = "Partlyx",
                        TimestampUtc = DateTime.UtcNow,
                        SchemaVersion = schemaVersion,
                        SourceFileName = Path.GetFileName(_dbProvider.CurrentDbPath)
                    };

                    var jsonOpt = new JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, jsonOpt));

                }, cancellationToken).ConfigureAwait(false);


                ZipFile.CreateFromDirectory(tempDir, tempZipPath);


                if (File.Exists(targetPath))
                    File.Delete(targetPath);
                File.Move(tempZipPath, targetPath, overwrite: true);
                return new ExportResult(true);
            }
            catch (OperationCanceledException)
            {
                return new ExportResult(false, "Operation cancelled");
            }
            catch (Exception ex)
            {
                string? leftover = File.Exists(tempZipPath) ? tempZipPath : null;
                return new ExportResult(false, ex.Message, leftover);
            }
            finally
            {
                try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { /* ignore */ }
                _dbProvider.DBExportLoadSemaphore.Release();
            }
        }
    }
}
