using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace OpFlow.Data.Administration
{
    public class ScheduleImport : IImportData
    {
        public string MRN { get; set; }
        public string CaseNbr { get; set; }
        public string PatientName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string BMIText { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string ScheduleTime { get; set; }
        public string Location { get; set; }
        public string Room { get; set; }
        public string Procedure { get; set; }
        public string Laterality { get; set; }
        public string Surgeon { get; set; }
        public string ProcedurePreferenceCards { get; set; }
        public string MedicalHistory { get; set; }
        public string RiskFactors { get; set; }
        public string Medications { get; set; }
        public string Allergies { get; set; }
        public string Notes { get; set; }

        public ImportSurgeon PrimarySurgeon
        {
            get
            {
                var result = new ImportSurgeon();

                if (string.IsNullOrEmpty(Surgeon))
                    return result;

                var surgeons = Surgeon.Split('\n');
                var surgeon = surgeons[0];

                return ImportSurgeon.ParseSurgeon(surgeon);
            }
        }

        public List<ImportSurgeon> SecondarySurgeons
        {
            get
            {
                var result = new List<ImportSurgeon>();

                if (string.IsNullOrEmpty(Surgeon))
                    return result;

                var surgeons = Surgeon.Split('\n');
                for (var index = 1; index < surgeons.Length; index++)
                {
                    result.Add(ImportSurgeon.ParseSurgeon(surgeons[index]));
                }

                return result;
            }
        }
        public string PatientLastName
        {
            get
            {
                if (string.IsNullOrEmpty(PatientName))
                    return null;

                var lnameRegex = "([A-Za-z]+), ([A-Za-z]+)";
                var match = Regex.Match(PatientName, lnameRegex);

                if (match.Groups.Count > 2)
                    return match.Groups[1].Value;

                return null;
            }
        }

        public string PatientFirstName
        {
            get
            {
                if (string.IsNullOrEmpty(PatientName))
                    return null;

                var lnameRegex = "([A-Za-z]+), ([A-Za-z]+)";
                var match = Regex.Match(PatientName, lnameRegex);

                if (match.Groups.Count > 1)
                    return match.Groups[2].Value;

                return null;
            }
        }

        public string PatientMInit
        {
            get
            {
                if (string.IsNullOrEmpty(PatientName))
                    return null;

                var lnameRegex = "([A-Za-z]+), ([A-Za-z]+) ([A-Za-z])";
                var match = Regex.Match(PatientName, lnameRegex);

                if (match.Groups.Count > 3)
                    return match.Groups[3].Value;

                return null;
            }
        }
        public decimal? BMI
        {
            get
            {
                if (string.IsNullOrEmpty(BMIText))
                    return null;

                var bmiRegex = @"([0-9\.]+)";
                var match = Regex.Match(BMIText, bmiRegex);

                if (match.Groups.Count > 1)
                {
                    var strBmi = match.Groups[1].Value;

                    decimal.TryParse(strBmi, out var bmi);
                    return bmi;
                }

                return null;
            }
        }

        public string CptCode
        {
            get
            {
                if (string.IsNullOrEmpty(Procedure))
                    return null;

                var cptRegex = @"\[([0-9]+)";
                var match = Regex.Match(Procedure, cptRegex);

                if (match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }

                return null;
            }
        }

        public string ProcedureCard
        {
            get
            {
                if (string.IsNullOrEmpty(ProcedurePreferenceCards))
                    return null;

                var results = ProcedurePreferenceCards.Split(';');
                return results[0];
            }
            
        }


        public DateTime ScheduleDateTime
        {
            get
            {
                var result = ScheduleDate;

                if (string.IsNullOrEmpty(ScheduleTime))
                    return result;

                try
                {
                    if (ScheduleTime.Length == 3)
                        ScheduleTime = "0" + ScheduleTime;

                    var ts = DateTime.ParseExact(ScheduleTime, "HHmm", CultureInfo.InvariantCulture).TimeOfDay;
                    result = ScheduleDate.Add(ts);
                }
                catch (Exception)
                {
                    // Just ignore
                }

                return result;
            }
        }
    }

    public class ImportSurgeon
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public static ImportSurgeon ParseSurgeon(string surgeonString)
        {
            var result = new ImportSurgeon();

            var regex = "([A-Za-z]+), ([A-Za-z]+)";
            var match = Regex.Match(surgeonString, regex);

            if (match.Success && match.Groups.Count > 2)
            {
                result.LastName = match.Groups[1].Value;
                result.FirstName = match.Groups[2].Value;
            }

            return result;
        }
    }

}
