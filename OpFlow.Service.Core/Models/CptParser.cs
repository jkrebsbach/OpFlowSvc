using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpFlow.Data.Core;

public static class CptTsvParser
{
    private static readonly string[] ExpectedHeaderColumns =
    {
        "Cpt_Id",
        "CPT Code",
        "Clinician Descriptor Id",
        "Clinician Descriptor"
    };

    public static IEnumerable<(CPTCodeModel? Row, CptUploadErrorDto? Error)> ParseStream(
        Stream stream,
        string fileName,
        DateTime uploadDateUtc,
        DateTime? expiryDateUtc)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

        string? line;
        bool inTable = false;
        int lineNumber = 0;

        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!inTable)
            {
                if (LooksLikeHeaderRow(line))
                    inTable = true;

                continue;
            }

            // Data line
            var cols = line.Split('\t');

            if (cols.Length < 4)
            {
                yield return (null, new CptUploadErrorDto
                {
                    FileName = fileName,
                    LineNumber = lineNumber,
                    Message = $"Invalid TSV row: expected 4 columns, got {cols.Length}.",
                    RawLine = line
                });
                continue;
            }

            if (cols.Length > 4)
            {
                cols = new[]
                {
                    cols[0],
                    cols[1],
                    cols[2],
                    string.Join("\t", cols.Skip(3))
                };
            }

            if (!long.TryParse(cols[0].Trim(), out var Cpt_Id))
            {
                yield return (null, new CptUploadErrorDto
                {
                    FileName = fileName,
                    LineNumber = lineNumber,
                    Message = "Concept Id is not a valid number.",
                    RawLine = line
                });
                continue;
            }

            var cptCode = cols[1].Trim();

            if (!long.TryParse(cols[2].Trim(), out var clinicianDescriptorId))
            {
                yield return (null, new CptUploadErrorDto
                {
                    FileName = fileName,
                    LineNumber = lineNumber,
                    Message = "Clinician Descriptor Id is not a valid number.",
                    RawLine = line
                });
                continue;
            }

            var clinicianDescriptor = cols[3].Trim();

            yield return (new CPTCodeModel
            {
                Cpt_Id = Cpt_Id,
                CptCode = cptCode,
                ClinicianDescriptorId = clinicianDescriptorId,
                ClinicianDescriptor = clinicianDescriptor,
                UploadDateUtc = uploadDateUtc,
                ExpiryDateUtc = expiryDateUtc
            }, null);
        }
    }

    private static bool LooksLikeHeaderRow(string line)
    {
        if (!line.Contains('\t')) return false;

        var cols = line.Split('\t')
            .Select(c => Normalize(c))
            .Where(c => !string.IsNullOrEmpty(c))
            .ToArray();

        if (cols.Length < 4) return false;

        for (int i = 0; i < 4; i++)
        {
            if (!cols[i].Equals(Normalize(ExpectedHeaderColumns[i]), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static string Normalize(string s)
        => string.Join(" ", s.Split(new[] { ' ', '\t', '\u00A0' }, StringSplitOptions.RemoveEmptyEntries)).Trim();
}
