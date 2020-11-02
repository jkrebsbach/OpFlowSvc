using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using OpFlow.Data.Administration;

namespace OpFlow.Service.Models
{
    public class CsvParser
    {
        private CsvReader _csv;

        public enum ImportType
        {
            Schedule = 1,
            Item = 2,
            Tray = 3,
            User = 4,
            Card = 5,
            CardlessSchedule = 6,
            SPMSchedule = 7,
            LomaLindaCard = 8
        }

        public CsvParser(string contents)
        {
            var reader = new StringReader(contents);
            _csv = new CsvReader(reader);
        }

        public List<IImportData> ParseCSV(ImportType importType)
        {
            try
            {
                _csv.Read();
                _csv.ReadHeader();

                var result = new List<IImportData>();

                while (_csv.Read())
                {
                    var checkField = _csv.GetField(0);
                    if (checkField == string.Empty)
                        continue;

                    switch (importType)
                    {
                        case ImportType.Schedule:
                            DateTime dob;
                            dob = DateTime.TryParse(_csv.GetField(3), out dob) ? dob : new DateTime(1900, 1, 1);

                            result.Add(new ScheduleImport
                            {
                                MRN = _csv.GetField(0),
                                CaseNbr = _csv.GetField(1),
                                PatientName = _csv.GetField(2),
                                DateOfBirth = dob,
                                Gender = _csv.GetField(4),
                                BMIText = _csv.GetField(5),
                                Medications = _csv.GetField(6),
                                Allergies = _csv.GetField(7),
                                ScheduleDate = DateTime.Parse(_csv.GetField(8)),
                                ScheduleTime = _csv.GetField(9),
                                Location = _csv.GetField(10),
                                Room = _csv.GetField(11),
                                Procedure = _csv.GetField(12),
                                Laterality = _csv.GetField(13),
                                Surgeon = _csv.GetField(14),
                                ProcedurePreferenceCards = _csv.GetField(15),
                                Notes = _csv.GetField(16)
                            });
                            break;
                        case ImportType.Item:
                            result.Add(new ItemImport
                            {
                                Catalog = _csv.GetField(0),
                                EHR_ID = _csv.GetField(1),
                                ItemType = _csv.GetField(2),
                                Category = _csv.GetField(3),
                                Description = _csv.GetField(4),
                                UnitOfMeasure = _csv.GetField(5),
                                Manufacturer = _csv.GetField(6),
                                Cost = _csv.GetField(7)
                            });
                            break;
                        case ImportType.Tray:
                            result.Add(new TrayImport
                            {
                                TrayID = _csv.GetField(0),
                                TrayName = _csv.GetField(1),
                                InstrumentName = _csv.GetField(2),
                                Manufacturer = _csv.GetField(3),
                                InstrumentType = _csv.GetField(4),
                                Quantity = string.IsNullOrEmpty(_csv.GetField(5)) ? 1 : int.Parse(_csv.GetField(5)),
                                Category = _csv.GetField(6),
                                VendorTray = _csv.GetField(7) == "Y"
                            });
                            break;
                        case ImportType.User:
                            result.Add(new UserImport
                            {
                                UserName = _csv.GetField(0),
                                Role = _csv.GetField(1),
                                Specialty = _csv.GetField(2)
                            });
                            break;
                        case ImportType.Card:
                            _csv.TryGetField(typeof(int?), 5, out var qty);

                            result.Add(new CardImport
                            {
                                Surgeon = _csv.GetField(0),
                                PreferenceCardName = _csv.GetField(1),
                                ItemName = _csv.GetField(2),
                                ProductNbr = _csv.GetField(3),
                                ItemType = _csv.GetField(4),
                                Quantity = (int?)qty
                            });
                            break;
                        case ImportType.CardlessSchedule:
                            result.Add(new CardlessScheduleImport
                            {
                                CaseNbr = _csv.GetField(0),
                                Surgeon = _csv.GetField(1),
                                ScheduleDate = DateTime.Parse(_csv.GetField(2)),
                                ScheduleTime = _csv.GetField(3),
                                Room = _csv.GetField(4),
                                TrayList = _csv.GetField(5)
                            });
                            break;
                        default:
                            throw new Exception("Undefined import type");
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}