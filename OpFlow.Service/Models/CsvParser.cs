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

        public CsvParser(string contents)
        {
            var reader = new StringReader(contents);
            _csv = new CsvReader(reader);
        }

        public List<IImportData> ParseCSV(int importTypeId)
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

                    switch (importTypeId)
                    {
                        case 1:
                            result.Add(new ScheduleImport
                            {
                                MRN = _csv.GetField(0),
                                CaseNbr = _csv.GetField(1),
                                PatientName = _csv.GetField(2),
                                DateOfBirth = DateTime.Parse(_csv.GetField(3)),
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
                        case 2:
                            result.Add(new ItemImport
                            {
                                Location = _csv.GetField(0),
                                LocationName = _csv.GetField(1),
                                FromLoc = _csv.GetField(2),
                                BinSeq = _csv.GetField(3),
                                Bin = _csv.GetField(4),
                                ItemNbr = _csv.GetField(5),
                                Desc = _csv.GetField(6),
                                ManuName = _csv.GetField(7),
                                MfgNbr = _csv.GetField(8),
                                ParLevel = _csv.GetField(9),
                                UOM = _csv.GetField(10),
                                ItemCost = _csv.GetField(11),
                                InventoryValue = _csv.GetField(12)
                            });
                            break;
                        case 3:
                            result.Add(new UserImport
                            {
                                UserName = _csv.GetField(0)
                            });
                            break;
                        case 4:
                            result.Add(new TrayImport
                            {
                                Customer = _csv.GetField(0),
                                TrayID = _csv.GetField(1),
                                TrayName = _csv.GetField(2),
                                InstrumentName = _csv.GetField(3),
                                Quantity = int.Parse(_csv.GetField(4)),
                                Manufacturer = _csv.GetField(5)
                            });
                            break;
                        case 5:
                            result.Add(new CardImport
                            {
                                Location = _csv.GetField(0),
                                Surgeon = _csv.GetField(1),
                                PreferenceCardName = _csv.GetField(2),
                                Type = _csv.GetField(3),
                                LawsonID = _csv.GetField(4),
                                CatalogNbr = _csv.GetField(5),
                                SupplyDescription = _csv.GetField(6),
                                Manufacturer = _csv.GetField(7),
                                OpenAmt = _csv.GetField(8),
                                PrnRequired = _csv.GetField(9),
                                CostPerUnitOt = _csv.GetField(10),
                                Dosage = _csv.GetField(11),
                                Unit = _csv.GetField(12)
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