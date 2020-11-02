using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpFlow.Data.Administration
{
    public class ScheduleImport : ScheduleBase
    {
        public string MRN { get; set; }
        public string PatientName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string BMIText { get; set; }
        public string Location { get; set; }
        public string Laterality { get; set; }
        public string ProcedurePreferenceCards { get; set; }
        public string MedicalHistory { get; set; }
        public string RiskFactors { get; set; }
        public string Medications { get; set; }
        public string Allergies { get; set; }
        public string Notes { get; set; }


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

            // sometimes specialty included in tokens
            // card;specialty;surgeon;room
            if (cardData.Length > 3)
            {
                result.ImportSurgeon = ImportSurgeon.ParseSurgeon(cardData[2].Trim());
                result.Location = cardData[3].Trim();
            }
            else
            {
                // New UNC import file should not be parsed(?)
                result.CardName = sourceCardString;
                /*
                if (cardData.Length > 1)
                    result.ImportSurgeon = ImportSurgeon.ParseSurgeon(cardData[1].Trim());
                if (cardData.Length > 2)
                    result.Location = cardData[2];
                */
            }
            return result;
        }
    }

}
