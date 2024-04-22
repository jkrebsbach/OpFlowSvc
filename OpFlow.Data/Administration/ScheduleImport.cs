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
        public string Location { get; set; }
        public string Laterality { get; set; }
        public string ProcedurePreferenceCards { get; set; }
        public string MedicalHistory { get; set; }
        public string RiskFactors { get; set; }
        public string CardDestinguisher { get; set; }

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
