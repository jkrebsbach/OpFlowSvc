using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OpFlow.Data.Core;

public interface ICptUploadService
{
    Task<CptUploadResultDto> ParseFilesAsync(CptUploadForm form, CancellationToken ct);
}

public sealed class CptUploadService : ICptUploadService
{
    public async Task<CptUploadResultDto> ParseFilesAsync(CptUploadForm form, CancellationToken ct)
    {
        var result = new CptUploadResultDto { Success = true };
        var uploadDateUtc = DateTime.UtcNow;

        foreach (var file in form.Files)
        {
            if (file.Length == 0)
                continue;

            var fileResult = new CptUploadFileResultDto { FileName = file.FileName };

            await using var stream = file.OpenReadStream();

            foreach (var (row, err) in CptTsvParser.ParseStream(stream, file.FileName, uploadDateUtc, form.ExpiryDateUtc))
            {
                ct.ThrowIfCancellationRequested();

                if (err != null)
                {
                    fileResult.RowsFailed++;
                    result.RowsFailed++;
                    result.Errors.Add(err);
                    continue;
                }

                fileResult.RowsParsed++;
                result.RowsParsed++;

                // keep a few sample rows for UI verification
                if (result.SampleRows.Count < 10 && row != null)
                    result.SampleRows.Add(row);
            }

            result.FileResults.Add(fileResult);
            result.FilesProcessed++;
        }

        // If literally nothing parsed and errors exist, mark as failure
        if (result.RowsParsed == 0 && result.Errors.Any())
            result.Success = false;

        return result;
    }
}
