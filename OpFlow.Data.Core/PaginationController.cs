using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;

namespace OpFlow.Data
{
    /// <summary>
    /// Custom class to expose contract for select2 dropdown pagination
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PaginationController
    {
        public PaginationResult pagination { get; set; }
        public List<KeyPair> results { get; set; }
    }

    public class PaginationResult
    {
        public bool more { get; set; }

        public PaginationResult(int resultCount, int skipped, int totalCount)
        {
            more = (resultCount + skipped) < totalCount;
        }
    }

    public class KeyPair
    {
        public int id { get; set; }
        public string text { get; set; }
    }

    public class RowCountEntity
    {
        public int TotalCount { get; set; }
    }
}
