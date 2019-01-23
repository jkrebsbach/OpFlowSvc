using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public string PatientLastName
        {
            get
            {
                if (string.IsNullOrEmpty(PatientName))
                    return null;

                var lnameRegex = @"([A-Za-z\'-\.\s]+), ([A-Za-z]+)";
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

                var lnameRegex = @"([A-Za-z\'-\.\s]+), ([A-Za-z]+)";
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

                var lnameRegex = @"([A-Za-z\'-\.\s]+), ([A-Za-z]+) ([A-Za-z])";
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

        public List<ImportCard> ProcedureCards
        {
            get
            {
                var result = new List<ImportCard>();

                if (string.IsNullOrEmpty(ProcedurePreferenceCards))
                    return result;

                var cards = ProcedurePreferenceCards.Split('\n');
                for (var index = 0; index < cards.Length; index++)
                {
                    cards[index] = cards[index].Trim();
                }

                var distinctCards = cards.Distinct().ToList();

                result.AddRange(distinctCards.Select(distinctCard => ImportCard.ParseCard(distinctCard.Trim())));

                return result;
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

    public class ImportCard
    {
        public string CardName { get; set; }
        public ImportSurgeon ImportSurgeon { get; set; }
        public string Location { get; set; }

        public static ImportCard ParseCard(string sourceCardString)
        {
            var result = new ImportCard();

            var cardData = sourceCardString.Split(';');
            result.CardName = cardData[0];
            if (cardData.Length > 1)
                result.ImportSurgeon = ImportSurgeon.ParseSurgeon(cardData[1].Trim());
            if (cardData.Length > 2)
                result.Location = cardData[2];

            return result;
        }
    }

    public class ImportSurgeon
    {
        public string FirstName { get; set; }
        public string MInit { get; set; }
        public string LastName { get; set; }
        public string RawText { get; set; }

        public static ImportSurgeon ParseSurgeon(string surgeonString)
        {
            var result = new ImportSurgeon()
            {
                RawText = surgeonString
            };

            // Last, First
            var regex = @"([A-Za-z\'-\.\s]+), ([A-Za-z]+)";
            var match = Regex.Match(surgeonString, regex);

            if (match.Success && match.Groups.Count > 2)
            {
                result.LastName = match.Groups[1].Value;
                result.FirstName = match.Groups[2].Value;

                return result;
            }

            // First M Last
            regex = @"([A-Za-z]+) ([A-Za-z]?) ?([A-Za-z\'-\.\s]+)";
            match = Regex.Match(surgeonString, regex);

            if (match.Success && match.Groups.Count > 3)
            {
                result.FirstName = match.Groups[1].Value;
                result.MInit = match.Groups[2].Value;
                result.LastName = match.Groups[3].Value;

                return result;
            }

            return null;
        }
    }

}
