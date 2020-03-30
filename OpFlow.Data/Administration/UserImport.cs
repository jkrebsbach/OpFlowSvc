using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class UserImport : IImportData
    {
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Specialty { get; set; }

        public ImportSurgeon UserEntity
        {
            get
            {
                var result = new ImportSurgeon();

                if (string.IsNullOrEmpty(UserName))
                    return result;

                var surgeons = UserName.Split('\n');
                var surgeon = surgeons[0];

                return ImportSurgeon.ParseSurgeon(surgeon);
            }
        }
    }
}
