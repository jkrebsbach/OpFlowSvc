using FileHelpers;
using OpFlow.Service.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreTests
{
    public class CptTester
    {
        private SqlHelper _sqlHelper;

        public CptTester()
        {
            _sqlHelper = new SqlHelper(TestBase.GetConfiguration());
        }

        [Fact]
        public async Task UploadCPT()
        {
            var uploadDateUtc = DateTime.UtcNow;

            var filename = @"C:\temp\ConsolidatedCodeList.csv";
            var file = new System.IO.FileInfo(filename);

            if (file.Length == 0)
                return;

            
            var engine = new FileHelpers.FileHelperEngine<CPTCodeModel>();
            var cptData = engine.ReadFile(filename);

            foreach (var cpt in cptData)
            {
                await _sqlHelper.UpdateCptCode(cpt.CptCode, cpt.LongDescriptor, cpt.MediumDescriptor, cpt.ShortDescriptor, cpt.ConsumerDescriptor);
            }

        }

        [DelimitedRecord(",")]
        [IgnoreFirst]
        public class CPTCodeModel
        {
            public long Cpt_Id { get; set; }
            public string CptCode { get; set; }
            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string LongDescriptor { get; set; }
            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string MediumDescriptor { get; set; }
            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string ShortDescriptor { get; set; }
            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string ConsumerDescriptor { get; set; }
            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string SpanishDescriptor { get; set; }
            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string CurrentEffectiveDate { get; set; }
            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string TestName { get; set; }
            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string LabName { get; set; }
            [FieldQuoted('"', QuoteMode.OptionalForBoth)]
            public string Manufacturer { get; set; }

            [FieldOptional]
            public string ExtraData1 { get; set; }

        }
    }
}
