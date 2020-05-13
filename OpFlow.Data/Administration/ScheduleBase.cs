using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpFlow.Data.Administration
{
    public abstract class ScheduleBase : IImportData
    {
        public string CaseNbr { get; set; }
        public string Surgeon { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string ScheduleTime { get; set; }
        public string Room { get; set; }
        public string Procedure { get; set; }

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
                for (var index = 0; index < surgeons.Length; index++)
                {
                    surgeons[index] = surgeons[index].Trim();
                }

                var distinctSurgeons = surgeons.Distinct().ToList();
                for (var index = 1; index < distinctSurgeons.Count; index++)
                {
                    var surgeon = ImportSurgeon.ParseSurgeon(distinctSurgeons[index].Trim());
                    if (surgeon != null)
                        result.Add(surgeon);
                }

                return result;
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

        public DateTime ScheduleDateTime
        {
            get
            {
                var result = ScheduleDate;

                if (string.IsNullOrEmpty(ScheduleTime))
                    return result;

                try
                {
                    ScheduleTime = ScheduleTime?.Replace(":", "");

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
        public string MInit { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string RawText { get; set; }

        public static ImportSurgeon ParseSurgeon(string surgeonString)
        {
            var result = new ImportSurgeon()
            {
                RawText = surgeonString
            };

            if (string.IsNullOrEmpty(surgeonString))
                return result;

            // Last, First Middle MD
            var match = Regex.Match(surgeonString, @"([A-Za-z\'-\.\s]+), ([A-Za-z\s]+) MD");
            if (match.Success && match.Groups.Count > 2)
            {
                result.LastName = match.Groups[1].Value;
                result.FirstName = match.Groups[2].Value;
                result.Title = "MD";

                return result;
            }

            // Last, First Middle MD, PhD
            match = Regex.Match(surgeonString, @"([A-Za-z\'-\.\s]+), ([A-Za-z\s]+) MD, PhD");
            if (match.Success && match.Groups.Count > 2)
            {
                result.LastName = match.Groups[1].Value;
                result.FirstName = match.Groups[2].Value;
                result.Title = "MD, PhD";

                return result;
            }

            // Last, First Middle
            match = Regex.Match(surgeonString, @"([A-Za-z\'-\.\s]+), ([A-Za-z\s]+)");
            if (match.Success && match.Groups.Count > 2)
            {
                result.LastName = match.Groups[1].Value;
                result.FirstName = match.Groups[2].Value;

                return result;
            }

            // First M Last
            match = Regex.Match(surgeonString, @"([A-Za-z]+) ([A-Za-z]?) ?([A-Za-z\'-\.\s]+)");

            if (match.Success && match.Groups.Count > 3)
            {
                result.FirstName = match.Groups[1].Value;
                result.MInit = match.Groups[2].Value;
                result.LastName = match.Groups[3].Value;

                return result;
            }

            // Assume string is last name
            result.LastName = surgeonString;
            return result;
        }
    }
}
