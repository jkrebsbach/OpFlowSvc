using System.Data;
using OpFlow.Data.Core;

namespace OpFlow.Service.Core.Builders
{
    public static class CptUploadTableBuilder
    {
        public static DataTable Build(IEnumerable<CPTCodeModel> rows)
        {
            var table = new DataTable();

            table.Columns.Add("cpt_id", typeof(string));
            table.Columns.Add("cpt_code", typeof(string));
            table.Columns.Add("clinician_descriptor_id", typeof(long));
            table.Columns.Add("clinician_descriptor", typeof(string));
            table.Columns.Add("upload_date", typeof(DateTime));
            table.Columns.Add("expiry_date", typeof(DateTime));

            foreach (var r in rows)
            {
                var row = table.NewRow();
                row["cpt_id"] = r.Cpt_Id;
                row["cpt_code"] = r.CptCode;
                row["clinician_descriptor_id"] = r.ClinicianDescriptorId;
                row["clinician_descriptor"] = r.ClinicianDescriptor;
                row["upload_date"] = r.UploadDateUtc;
                row["expiry_date"] = (object?)r.ExpiryDateUtc ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }
    }

}
