using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace OpFlow.Data.Core
{

    public sealed class CptUploadForm
    {
        [Required]
        public List<IFormFile> Files { get; set; } = new();

        public DateTime? ExpiryDateUtc { get; set; }

        /// <summary>
        /// If true, upserting begins on existing rows.
        /// </summary>
        public bool Upsert { get; set; } = true;
    }
    public class CPTCodeModel
    {
        public long Cpt_Id { get; init; }
        public string CptCode { get; init; } = string.Empty;
        public long ClinicianDescriptorId { get; init; }
        public string ClinicianDescriptor { get; init; } = string.Empty;

        public DateTime UploadDateUtc { get; init; }
        public DateTime? ExpiryDateUtc { get; init; }
    }

    public sealed class CptUploadResultDto
    {
        public bool Success { get; set; }
        public int FilesProcessed { get; set; }
        public int RowsParsed { get; set; }
        public int RowsFailed { get; set; }

        // For verification in dev; can be removed later -- I.E.
        public List<CPTCodeModel> SampleRows { get; set; } = new();

        public List<CptUploadFileResultDto> FileResults { get; set; } = new();
        public List<CptUploadErrorDto> Errors { get; set; } = new();
    }

    public sealed class CptUploadFileResultDto
    {
        public string FileName { get; set; } = string.Empty;
        public int RowsParsed { get; set; }
        public int RowsFailed { get; set; }
    }

    public sealed class CptUploadErrorDto
    {
        public string FileName { get; set; } = string.Empty;
        public int? LineNumber { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RawLine { get; set; }
    }
}
