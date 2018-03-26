using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
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
                    switch (importTypeId)
                    {
                        case 1:
                            result.Add(new ScheduleImport
                            {
                                PatientID = _csv.GetField(5),
                                CaseID = _csv.GetField(5),
                                FirstName = _csv.GetField(5),
                                LastName = _csv.GetField(5),
                                DateOfBirth = DateTime.Parse(_csv.GetField(5)),
                                Gender = _csv.GetField(5),
                                BMI = Int32.Parse(_csv.GetField(5)),
                                MedicalHistory = _csv.GetField(5),
                                RiskFactors = _csv.GetField(5),
                                Medications = _csv.GetField(5),
                                Allergies = _csv.GetField(5),
                                Notes = _csv.GetField(5),
                                ScheduleDate = DateTime.Parse(_csv.GetField(5)),
                                ScheduleTime = _csv.GetField(5),
                                Location = _csv.GetField(5),
                                Room = _csv.GetField(5),
                                Procedure = _csv.GetField(5),
                                ProcedureCard = _csv.GetField(5),
                                Surgeon = _csv.GetField(5),
                                Circulator = _csv.GetField(5),
                                Anes = _csv.GetField(5),
                                Tech = _csv.GetField(5)
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