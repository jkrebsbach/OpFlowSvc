using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using OpFlow.Data;
using OpFlow.Data.Administration;
using OpFlow.Data.Analytics;
using OpFlow.Data.Debrief;

namespace OpFlow.Service.DataAccess
{
    public class SqlHelper : SqlBase
    {
        public SqlHelper() : base("CommonConnection")
        {
        }

        public async Task<int> CreateLocation(int? providerId, string providerName, string locationName)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId ?? (object)DBNull.Value),
                new SqlParameter("provider_name", providerName),
                new SqlParameter("location_name", locationName)
            };
            var dsSchedules = await ExecuteCommandAsync("InsertLocation", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<InsertionResult>().First().Identifier;

            return result;
        }

        public async Task<int> UpdateLocation(int locationId, int? trayHigh, int? trayMed, int? trayLow,
            int? surgeonHigh, int? surgeonMed, int? surgeonLow, int? auditHigh, int? auditMed, int? auditLow, int? dailyTarget)
        {
            var parameters = new[]
            {
                new SqlParameter("location_id", locationId),
                new SqlParameter("tray_high", trayHigh ?? (object)DBNull.Value),
                new SqlParameter("tray_med", trayMed ?? (object)DBNull.Value),
                new SqlParameter("tray_low", trayLow ?? (object)DBNull.Value),
                new SqlParameter("surgeon_high", surgeonHigh ?? (object)DBNull.Value),
                new SqlParameter("surgeon_med", surgeonMed ?? (object)DBNull.Value),
                new SqlParameter("surgeon_low", surgeonLow ?? (object)DBNull.Value),
                new SqlParameter("audit_high", auditHigh ?? (object)DBNull.Value),
                new SqlParameter("audit_med", auditMed ?? (object)DBNull.Value),
                new SqlParameter("audit_low", auditLow ?? (object)DBNull.Value),
                new SqlParameter("daily_target", dailyTarget ?? (object)DBNull.Value)
            };
            var result = await ExecuteNonQueryAsync("UpdateLocation", parameters);

            return result;
        }

        public async Task<List<TraySurgeryAudit>> GetDisposableAudits(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetDisposableAudits", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TraySurgeryAudit>();
            var cptCodes = dsSchedules.Tables[1].DataTableToList<SurgeryCPTCode>();

            foreach (var cptCode in cptCodes)
            {
                result.FirstOrDefault(r => r.SurgeryID == cptCode.SurgeryID)?.CptCodes.Add(cptCode);
            }

            return result;
        }

        public async Task<List<TraySurgeryAudit>> GetDisposableCounts(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetDisposableCounts", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TraySurgeryAudit>();
            var cptCodes = dsSchedules.Tables[1].DataTableToList<SurgeryCPTCode>();

            foreach (var cptCode in cptCodes)
            {
                result.FirstOrDefault(r => r.SurgeryID == cptCode.SurgeryID)?.CptCodes.Add(cptCode);
            }

            return result;
        }

        public async Task<List<ItemRationalization>> GetItemRationalization(int? specialtyId, int? surgeonId,
            decimal? minCost, decimal? maxCost, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonId ?? (object)DBNull.Value),
                new SqlParameter("min_cost", minCost ?? (object)DBNull.Value),
                new SqlParameter("max_cost", maxCost ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetItemRationalization", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemRationalization>();
            var cards = dsSchedules.Tables[1].DataTableToList<ItemCard>();

            foreach (var card in cards)
            {
                var item = result.FirstOrDefault(i => i.ItemID == card.ItemID);
                item?.Cards.Add(card);
            }

            return result;
        }

        public async Task<List<ItemRationalization>> SearchItems(string itemName,
            decimal? minCost, decimal? maxCost, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("item_name", itemName ?? (object)DBNull.Value),
                new SqlParameter("min_cost", minCost ?? (object)DBNull.Value),
                new SqlParameter("max_cost", maxCost ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("SearchItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemRationalization>();
            
            return result;
        }

        public async Task<List<ItemRationalizationCase>> GetItemAuditCases(int? specialtyId, string itemName,
            int? surgeonId, int? cardId,
            DateTime beginDate, DateTime endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("item_name", itemName ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonId ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardId ?? (object)DBNull.Value),
                new SqlParameter("begin_date", beginDate),
                new SqlParameter("end_date", endDate),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetItemAuditCases", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemRationalizationCase>();
            var items = dsSchedules.Tables[1].DataTableToList<ItemRationalizationCaseItem>();

            foreach (var item in items)
            {
                result.FirstOrDefault(r => r.SurgeryID == item.SurgeryID)?.Items.Add(item);
            }

            return result;
        }

        public async Task<int> UpdateDisposableAuditComplete(int surgeryId, string target, int userId,
            int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("target", target),
                new SqlParameter("user_id", userId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateDisposableAuditComplete", parameters);

            return result;
        }
        

        public async Task<int> InsertDisposableAudit(List<int> surgeryId, string target, int providerId, int locationId)
        {
            var surgeryXml = GetIdentitySummary(surgeryId);

            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryXml),
                new SqlParameter("target", target),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("InsertDisposableAudit", parameters);

            return result;
        }

        public async Task<int> UpdateDisposableAudit(List<ItemAuditUpdate> audits, string target, int providerId, int locationId)
        {
            var auditXml = GetAuditUpdateSummary(audits);

            var parameters = new[]
            {
                new SqlParameter("audits", auditXml),
                new SqlParameter("target", target),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateDisposableAudit", parameters);

            return result;
        }

        public async Task<int> DeleteDisposableAudit(int surgeryId, string target, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("target", target),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteDisposableAudit", parameters);

            return result;
        }

        private string GetAuditUpdateSummary(List<ItemAuditUpdate> surgeries)
        {
            if (surgeries == null || !surgeries.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var surgery in surgeries)
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, surgery.SurgeryID);
                AddColumn(doc, row, surgery.Comment);
            }

            return table.OuterXml;
        }

        public async Task<int> UpdateItemCountNeeded(int itemId, bool countNeeded, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("item_id", itemId),
                new SqlParameter("count_needed", countNeeded),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateItemCountNeeded", parameters);

            return result;
        }

        public async Task<List<TrayRationalizationItem>> GetTrayRationalization(int? trayProposalId,
            List<int> specialties, List<int> trays, List<int> surgeons, List<int> cards, string cptCode, List<TrayQuestion> questions,
            int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialties);
            var trayXml = GetIdentitySummary(trays);
            var surgeonXml = GetIdentitySummary(surgeons);
            var cardXml = GetIdentitySummary(cards);
            var questionXml = GetQuestionSummary(questions);

            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("specialties", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("trays", trayXml ?? (object)DBNull.Value),
                new SqlParameter("surgeons", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("cards", cardXml ?? (object)DBNull.Value),
                new SqlParameter("cpt_code", cptCode ?? (object)DBNull.Value),
                new SqlParameter("questions", questionXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetTrayRationalization", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayRationalizationItem>();

            return result;
        }

        public async Task<TrayRationalizationCompareResult> GetTrayRationalizationCompare(
            int customerId, int baselineId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("customer_id", customerId),
                new SqlParameter("baseline_id", baselineId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetTrayRationalizationCompare", parameters);

            var instruments = dsSchedules.Tables[0].DataTableToList<TrayRationalizationCompare>();
            var categories = dsSchedules.Tables[1].DataTableToList<TrayRationalizationCompareCategory>();

            var result = new TrayRationalizationCompareResult()
            {
                Instruments = instruments,
                CustomerCategories = categories.Where(c => c.TrayProposalID == customerId).ToList(),
                BaselineCategories = categories.Where(c => c.TrayProposalID == baselineId).ToList()
            };

            return result;
        }


        public async Task<TrayRationalizationDetailResult> GetTrayRationalizationDetail(int trayProposalId,
            string type, int? itemId,
            int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("type", type ?? (object)DBNull.Value),
                new SqlParameter("item_id", itemId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetTrayRationalizationDetail", parameters);

            var result = new TrayRationalizationDetailResult();
            var trayNameTable = dsSchedules.Tables[0].DataTableToList<TrayRationalizationDetail>();

            result.TrayName = trayNameTable.First().TrayName;
            result.Quantity = trayNameTable.First().QtyOpen;
            result.Instruments = dsSchedules.Tables[1].DataTableToList<TrayRationalizationDetail>();

            return result;
        }

        private string GetIdentitySummary(List<int> identities)
        {
            if (identities == null || !identities.Any() || (identities.Count == 1 && identities[0] == 0))
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var identity in identities.Distinct())
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, identity);
            }

            return table.OuterXml;
        }

        public async Task<DataSet> GetAnalyticsSalesToolSummary(string systemName, string hospitalName, string city, string state,
            string contactName, string salesperson, int? caseCount, int? spdLaborRate, int? contractDuration, int? annualMaintenance,
            int? depreciation, int? trayCount, int? instrumentAvg,
            int? trayYear, int? vendorYear, int? cardYear, int? improveYear)
        {
            var parameters = new[]
            {
                new SqlParameter("system_name", systemName ?? (object)DBNull.Value),
                new SqlParameter("hospital_name", hospitalName ?? (object)DBNull.Value),
                new SqlParameter("city", city ?? (object)DBNull.Value),
                new SqlParameter("state", state ?? (object)DBNull.Value),
                new SqlParameter("contact_name", contactName ?? (object)DBNull.Value),
                new SqlParameter("salesperson", salesperson ?? (object)DBNull.Value),
                new SqlParameter("case_count", caseCount ?? (object)DBNull.Value),
                new SqlParameter("spd_labor_rate", spdLaborRate ?? (object)DBNull.Value),
                new SqlParameter("contract_duration", contractDuration ?? (object)DBNull.Value),
                new SqlParameter("annual_maintenance", annualMaintenance ?? (object)DBNull.Value),
                new SqlParameter("depreciation", depreciation ?? (object)DBNull.Value),
                new SqlParameter("tray_count", trayCount ?? (object)DBNull.Value),
                new SqlParameter("instrument_avg", instrumentAvg ?? (object)DBNull.Value),
                new SqlParameter("tray_year", trayYear ?? (object)DBNull.Value),
                new SqlParameter("vendor_year", vendorYear ?? (object)DBNull.Value),
                new SqlParameter("card_year", cardYear ?? (object)DBNull.Value),
                new SqlParameter("improve_year", improveYear ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetAnalyticsSalesToolSummary", parameters);

            return dsSchedules;
        }

        public async Task<List<TrayConsolidationResult>> GetAnalyticsTrayConsolidationData(List<int> specialtyId, List<int> trayId, int? reallocationId,
            int? maxSize, int? minCards, int? minConsolidationInstances, int? minTargetInstances, int? minOverlap, int? maxEffect, List<int> procedureGroup, string group, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var trayXml = GetIdentitySummary(trayId);
            var procedureXml = GetIdentitySummary(procedureGroup);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("tray_item_id", trayXml ?? (object)DBNull.Value),
                new SqlParameter("reallocation_id", reallocationId ?? (object)DBNull.Value),
                new SqlParameter("max_size", maxSize ?? (object)DBNull.Value),
                new SqlParameter("min_cards", minCards ?? (object)DBNull.Value),
                new SqlParameter("min_consolidation_instances", minConsolidationInstances ?? (object)DBNull.Value),
                new SqlParameter("min_target_instances", minTargetInstances ?? (object)DBNull.Value),
                new SqlParameter("min_overlap", minOverlap ?? (object)DBNull.Value),
                new SqlParameter("card_category", procedureXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            DataSet dsSchedules;
            if (group == "S")
                dsSchedules = await ExecuteCommandAsync("GetAnalyticsTrayConsolidation", parameters);
            else
                dsSchedules = await ExecuteCommandAsync("GetAnalyticsTrayConsolidationInstrument", parameters);

            var consolidations = dsSchedules.Tables[0].DataTableToList<TrayConsolidation>();

            var result = TrayConsolidationResult.Summarize(consolidations, group, maxEffect);

            if (reallocationId.HasValue)
            {
                foreach (var specialty in result)
                {
                    foreach (var tray in specialty.Consolidations)
                    {
                        var child = tray.Children?.FirstOrDefault(c => c.TrayItemID == reallocationId);
                        if (child == null)
                            continue;

                        var filterSpecialtyId = specialty.SpecialtyID;
                        var filterInstrumentId = specialty.InstrumentID;

                        if (group == "S")
                            filterInstrumentId = null;
                        if (group == "I" && specialtyId == null)
                            filterSpecialtyId = null;

                        parameters = new[]
                        {
                            new SqlParameter("specialty_id", filterSpecialtyId ?? (object)DBNull.Value),
                            new SqlParameter("instrument_id", filterInstrumentId ?? (object)DBNull.Value),
                            new SqlParameter("tray_item_id", tray.TrayItemID),
                            new SqlParameter("reallocation_id", reallocationId ?? (object)DBNull.Value),
                            new SqlParameter("provider_id", providerId),
                            new SqlParameter("location_id", locationId)
                        };

                        var dsValidation = await ExecuteCommandAsync("GetAnalyticsTrayConsolidationValidation", parameters);

                        consolidations = dsValidation.Tables[0].DataTableToList<TrayConsolidation>();
                        TrayConsolidationResult.SummarizeChildren(tray, child, consolidations);
                    }
                }
            }
            
            return result;
        }

        public async Task<DataSet> GetAnalyticsTrayRationalizationData(List<int> specialtyId, List<int> surgeonId, List<int> trayId, List<int> cardCategoryId,
            int? minSize, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var trayXml = GetIdentitySummary(trayId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("tray_item_id", trayXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("min_size", minSize ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetAnalyticsTrayRationalization", parameters);

            return dsSchedules;
        }

        public async Task<DataSet> GetAnalyticsVendorTrayRationalizationData(List<int> specialtyId, List<int> surgeonId, List<int> trayId, List<int> cardCategoryId,
            int? minSize, DateTime? startDate, DateTime? endDate, int? caseProfileId, List<int> questionId, List<int> answerId, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var trayXml = GetIdentitySummary(trayId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);
            var questionXml = GetIdentitySummary(questionId);
            var answerXml = GetIdentitySummary(answerId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("tray_item_id", trayXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("min_size", minSize ?? (object)DBNull.Value),
                new SqlParameter("start_date", startDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("case_profile_id", caseProfileId ?? (object)DBNull.Value),
                new SqlParameter("question_id", questionXml ?? (object)DBNull.Value),
                new SqlParameter("answer_id", answerXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetAnalyticsVendorTrayRationalization", parameters);

            return dsSchedules;
        }

        public async Task<DataSet> GetAnalyticsTrayScopeData(List<int> specialtyId, List<int> trayId, string group, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var trayXml = GetIdentitySummary(trayId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("tray_item_id", trayXml ?? (object)DBNull.Value),
                new SqlParameter("group", group ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetAnalyticsTrayScope", parameters);

            return dsSchedules;
        }

        public async Task<DataSet> GetInstrumentUsageReportData(List<int> specialtyId, List<int> surgeonId, List<int> categoryId,
            List<int> procedureId, List<string> cptList, List<int> trayId, List<int> instrumentId, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var categoryXml = GetIdentitySummary(categoryId);
            var procedureXml = GetIdentitySummary(procedureId);
            var trayXml = GetIdentitySummary(trayId);
            var instrumentXml = GetIdentitySummary(instrumentId);
            var cptXml = GetStringSummary(cptList);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("category_id", categoryXml ?? (object)DBNull.Value),
                new SqlParameter("procedure_id", procedureXml ?? (object)DBNull.Value),
                new SqlParameter("cpts", cptXml ?? (object)DBNull.Value),
                new SqlParameter("tray_item_id", trayXml ?? (object)DBNull.Value),
                new SqlParameter("instrument_id", instrumentXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsInstrumentUsage", parameters);

            return result;
        }

        public async Task<DataSet> GetDisposableUsageReportData(List<int> specialtyId, List<int> surgeonId, List<int> cardId,
            List<int> cardCategoryId, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var cardXml = GetIdentitySummary(cardId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsDisposableUsage", parameters);

            return result;
        }

        public async Task<DataSet> GetConcordanceReportData(List<int> specialtyId, List<int> surgeonId,
            List<int> procedureId, List<int> trayId, List<int> cardCategoryId, List<int> cardId, 
            string instruments, string label, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var procedureXml = GetIdentitySummary(procedureId);
            var trayXml = GetIdentitySummary(trayId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);
            var cardXml = GetIdentitySummary(cardId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("procedure_id", procedureXml ?? (object)DBNull.Value),
                new SqlParameter("tray_item_id", trayXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardXml ?? (object)DBNull.Value),
                new SqlParameter("instruments", instruments),
                new SqlParameter("label", label),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsConcordanceReport", parameters);

            return result;
        }

        public async Task<DataSet> GetSupplyConcordanceReportData(List<int> specialtyId, List<int> surgeonId,
            List<int> procedureId, List<int> cardCategoryId, List<int> cardId, string instruments, string label, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var procedureXml = GetIdentitySummary(procedureId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);
            var cardXml = GetIdentitySummary(cardId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("procedure_id", procedureXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardXml ?? (object)DBNull.Value),
                new SqlParameter("instruments", instruments),
                new SqlParameter("label", label),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsSupplyConcordanceReport", parameters);

            return result;
        }

        public async Task<DataSet> GetVendorTrayConcordanceReportData(List<int> specialtyId, List<int> surgeonId,
            List<int> procedureId, List<int> trayId, List<int> cardCategoryId, List<int> cardId, string instruments, 
            int? caseProfileId, List<int> questionId, List<int> answerId,
            int providerId, int locationId, string group)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var procedureXml = GetIdentitySummary(procedureId);
            var trayXml = GetIdentitySummary(trayId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);
            var cardXml = GetIdentitySummary(cardId);
            var questionXml = GetIdentitySummary(questionId);
            var answerXml = GetIdentitySummary(answerId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("procedure_id", procedureXml ?? (object)DBNull.Value),
                new SqlParameter("tray_item_id", trayXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardXml ?? (object)DBNull.Value),
                new SqlParameter("instruments", instruments ?? (object)DBNull.Value),
                new SqlParameter("case_profile_id", caseProfileId ?? (object)DBNull.Value),
                new SqlParameter("question_id", questionXml ?? (object)DBNull.Value),
                new SqlParameter("answer_id", answerXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("group", group ?? (object)DBNull.Value)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsVendorTrayConcordanceReport", parameters);

            return result;
        }

        public async Task<DataSet> GetServiceLineReviewReportData(List<int> specialtyId, List<int> surgeonId,
            List<int> cardId, List<int> itemId, List<int> cardCategoryId, int? minCost, decimal? minOpen, decimal? minHold,
            bool fieldAll, bool fieldWaste, bool fieldOver, bool fieldUnder,
            DateTime? startDate, DateTime? endDate,
            string groupBy, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var cardXml = GetIdentitySummary(cardId);
            var itemXml = GetIdentitySummary(itemId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardXml ?? (object)DBNull.Value),
                new SqlParameter("item_id", itemXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("min_cost", minCost ?? (object)DBNull.Value),
                new SqlParameter("min_open", minOpen ?? (object)DBNull.Value),
                new SqlParameter("min_hold", minHold ?? (object)DBNull.Value),
                new SqlParameter("start_date", startDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("field_all", fieldAll),
                new SqlParameter("field_waste", fieldWaste),
                new SqlParameter("field_over", fieldOver),
                new SqlParameter("field_under", fieldUnder),
                new SqlParameter("group_by", groupBy),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsServiceLineReview", parameters);

            return result;
        }

        public async Task<DataSet> GetSupplyWasteReportData(List<int> specialtyId, List<int> surgeonId,
            List<int> cardId, List<int>itemId, List<int> cardCategoryId, int? minCost, decimal? minOpen, decimal? minHold, 
            DateTime? startDate, DateTime? endDate, 
            bool fieldAll, bool fieldWaste, bool fieldOver, bool fieldUnder, string groupBy, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var cardXml = GetIdentitySummary(cardId);
            var itemXml = GetIdentitySummary(itemId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardXml ?? (object)DBNull.Value),
                new SqlParameter("item_id", itemXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("min_cost", minCost ?? (object)DBNull.Value),
                new SqlParameter("min_open", minOpen ?? (object)DBNull.Value),
                new SqlParameter("min_hold", minHold ?? (object)DBNull.Value),
                new SqlParameter("start_date", startDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("field_all", fieldAll),
                new SqlParameter("field_waste", fieldWaste),
                new SqlParameter("field_over", fieldOver),
                new SqlParameter("field_under", fieldUnder),
                new SqlParameter("group_by", groupBy),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsSupplyWaste", parameters);

            return result;
        }

        public async Task<DataSet> GetCardRedundancyReport(List<int> specialtyId, List<int> surgeonId, List<int> cardId, int? minQty, int? redundancy, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var cardXml = GetIdentitySummary(cardId);
        
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardXml ?? (object)DBNull.Value),
                new SqlParameter("min_quantity", minQty ?? (object)DBNull.Value),
                new SqlParameter("redundancy", redundancy ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsCardRedundancy", parameters);

            return result;
        }

        public async Task<DataSet> GetProcedureProfileReport(int procedureProfileId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsProcedureProfile", parameters);

            return result;
        }

        public async Task<ProcedureProfileDashboardComparisonQuery> GetProcedureProfileDashboardComparison(int procedureProfileId, 
            List<int> locationId, List<int> specialtyId, List<int> cardCategoryId, List<int> surgeonId, List<int> proposalId)
        {
            var locationXml = GetIdentitySummary(locationId);
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);
            var proposalXml = GetIdentitySummary(proposalId);

            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("location_id", locationXml ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("tray_proposal_id", proposalXml ?? (object)DBNull.Value)
            };
            var response = await ExecuteCommandAsync("GetProcedureProfileDashboardComparison", parameters);

            var result = new ProcedureProfileDashboardComparisonQuery()
            {
                Comparisons = response.Tables[0].DataTableToList<ProcedureProfileDashboardComparison>()
            };

            return result;
        }        

        public async Task<DataSet> GetProcedureProfileTrayAlignment(int procedureProfileId, List<int> cardId, List<int> trayId, int providerId, int locationId)
        {
            var cardXml = GetIdentitySummary(cardId);
            var trayXml = GetIdentitySummary(trayId);

            var parameters = new[]
            {
                new SqlParameter("tray_id", trayXml ?? (object)DBNull.Value),
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsProcedureProfileTrayAlignment", parameters);

            return result;
        }

        public async Task<DataSet> GetProcedureProfileSupplyAlignment(int procedureProfileId, List<int> cardId, List<int> trayId, int providerId, int locationId)
        {
            var cardXml = GetIdentitySummary(cardId);
            var trayXml = GetIdentitySummary(trayId);

            var parameters = new[]
            {
                new SqlParameter("card_id", cardXml ?? (object)DBNull.Value),
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsProcedureProfileSupplyAlignment", parameters);

            return result;
        }

        public async Task<DataSet> GetExcessInventoryReport(List<int> specialtyId, List<int> proposedTrayId, List<string> trayStatus, List<int> trayPhaseId,
            string group, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var proposedXml = GetIdentitySummary(proposedTrayId);
            var statusXml = GetStringSummary(trayStatus);
            var phaseXml = GetIdentitySummary(trayPhaseId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("proposed_tray_id", proposedXml ?? (object)DBNull.Value),
                new SqlParameter("tray_status", statusXml ?? (object)DBNull.Value),
                new SqlParameter("tray_phase_id", phaseXml ?? (object)DBNull.Value),
                new SqlParameter("group_by", group),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsExcessInventory", parameters);

            return result;
        }

        public async Task<DataSet> GetSupplyCardCostReportData(List<int> specialtyId, List<int> surgeonId, List<int> cardCategoryId,
            DateTime? startDate, DateTime? endDate, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("start_date", startDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsSupplyCardCost", parameters);

            return result;
        }

        public async Task<DataSet> GetSupplySavingsEstimatorReportData(List<int> specialtyId, List<int> surgeonId, List<int> cardCategoryId,
            List<int> itemId, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);
            var itemXml = GetIdentitySummary(itemId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("item_id", itemXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("GetAnalyticsSupplySavings", parameters);

            return result;
        }

        private string GetStringSummary(List<string> cptList)
        {
            if (cptList == null || !cptList.Any() || (cptList.Count == 1 && string.IsNullOrEmpty(cptList[0])))
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var cpt in cptList)
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, cpt);
            }

            return table.OuterXml;
        }

        public async Task<DataSet> GetAnalyticsSupplyCountData(List<int> specialtyId, List<int> surgeonId, List<int> cardId, List<int> cardCategoryId,
            string group, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var cardXml = GetIdentitySummary(cardId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("group", group ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetAnalyticsSupplyCount", parameters);

            return dsSchedules;
        }

        public async Task<DataSet> GetAnalyticsCountDistributionData(List<int> specialtyId, List<int> surgeonId, List<int> cardCategoryId, 
            List<int> itemCategoryId, List<int> itemId, int? minCost, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);
            var itemCategoryXml = GetIdentitySummary(itemCategoryId);
            var itemXml = GetIdentitySummary(itemId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("item_category_id", itemCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("item_id", itemXml ?? (object)DBNull.Value),
                new SqlParameter("min_cost", minCost ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetAnalyticsCountDistribution", parameters);

            return dsSchedules;
        }

        public async Task<DataSet> GetAnalyticsCountSummaryData(List<int> specialtyId, List<int> surgeonId, List<int> cardId, List<int> cardCategoryId,
            List<int> roomGroupId, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var cardXml = GetIdentitySummary(cardId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);
            var roomGroupXml = GetIdentitySummary(roomGroupId);

            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("room_group_id", roomGroupXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetAnalyticsCountSummary", parameters);

            return dsSchedules;
        }
        
        public async Task<DataSet> GetAnalyticsCountSampleDispersion(string countType, List<int> specialtyId, List<int> surgeonId, List<int> trayId, List<int> itemId,
            List<int> cardCategoryId, DateTime? startDate, DateTime? endDate, string group, int providerId, int locationId)
        {
            var specialtyXml = GetIdentitySummary(specialtyId);
            var surgeonXml = GetIdentitySummary(surgeonId);
            var trayXml = GetIdentitySummary(trayId);
            var itemXml = GetIdentitySummary(itemId);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);

            countType = (countType == string.Empty ? null : countType);

            var parameters = new[]
            {
                new SqlParameter("count_type", countType ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonXml ?? (object)DBNull.Value),
                new SqlParameter("tray_id", trayXml ?? (object)DBNull.Value),
                new SqlParameter("item_id", itemXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("start_date", startDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("group", group),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetAnalyticsCountSampleDispersion", parameters);

            return dsSchedules;
        }

        public async Task<int> InsertProposedTray(int? proposedTrayId, int? specialtyId, bool customized,
            string trayName, List<ProposedTrayInstrumentPost> instruments, int providerId, int locationId)
        {
            var instrumentXml = GetInstrumentSummary(instruments);
            
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId ?? (object)DBNull.Value),
                new SqlParameter("tray_name", trayName ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("customized", customized),
                new SqlParameter("instruments", instrumentXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("InsertProposedTrayInstruments", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<InsertionResult>();

            return result.First().Identifier;
        }

        public async Task<int> UpdateProposedTrayInstruments(int proposedTrayId, List<UpdateTrayInstrumentPost> instruments, int providerId, int locationId)
        {
            var instrumentXml = GetInstrumentUpdateSummary(instruments);

            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("instruments", instrumentXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayInstruments", parameters);

            return result;
        }

        public async Task<int> UpdateProposedTrayQuantities(int proposedTrayId, List<UpdateTrayInstrumentPost> instruments, int providerId, int locationId)
        {
            var instrumentXml = GetInstrumentUpdateSummary(instruments);

            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("instruments", instrumentXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayQuantities", parameters);

            return result;
        }

        public async Task<int> DeleteProposedTrayInstrument(int proposedTrayId, int instrumentId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("instrument_id", instrumentId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteProposedTrayInstrument", parameters);

            return result;
        }

        public async Task<int> InsertProposedTrayInstrumentLog(int proposedTrayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("InsertProposedTrayInstrumentLog", parameters);

            return result;
        }

        public async Task<int> UpdateProposedTrayAuditComments(int proposedTrayId, int surgeryId, string comments, string target,
            int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("comments", comments ?? (object)DBNull.Value),
                new SqlParameter("target", target),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayAuditComments", parameters);

            return result;
        }

        public async Task<int> InsertTrayGroup(string trayGroup, int providerId, int locationId)
        {
            if (!int.TryParse(trayGroup, out var trayGroupId))
            {
                var parameters = new[]
                {
                    new SqlParameter("tray_group", trayGroup),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId)
                };
                    
                var dsResult = await ExecuteCommandAsync("InsertTrayGroup", parameters);
                trayGroupId = dsResult.Tables[0].DataTableToList<InsertionResult>().First().Identifier;
            }

            return trayGroupId;
        }
        public async Task<int> UpdateTrayGroup(int? trayGroupId, string groupName, List<TrayGroupTray> trays, int providerId, int locationId)
        {
            var result = trayGroupId ?? -1;

            if (!trayGroupId.HasValue)
            {
                result = await InsertTrayGroup(groupName, providerId, locationId);
            }

            var trayXml = GetIdentitySummary(trays?.Select(t => t.TrayItemID)?.ToList());

            var parameters = new[]
            {
                new SqlParameter("tray_group_id", result),
                new SqlParameter("tray_group", groupName),
                new SqlParameter("trays", trayXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            await ExecuteNonQueryAsync("UpdateTrayGroup", parameters);
            
            return result;
        }

        public async Task<int> UpdateProposedTray(int proposedTrayId,
            string trayName, string status, int statusUserId, int? vendorId, int? specialtyId, int? phaseId, bool customized,
            List<int> cardCategories, List<int> trayGroups, int providerId, int locationId)
        {
            var cardCategoryXml = GetIdentitySummary(cardCategories);
            var trayGroupXml = GetIdentitySummary(trayGroups);

            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("tray_name", trayName),
                new SqlParameter("status", status),
                new SqlParameter("status_user_id", statusUserId),
                new SqlParameter("vendor_id", vendorId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("phase_id", phaseId ?? (object)DBNull.Value),
                new SqlParameter("customized", customized),
                new SqlParameter("card_categories", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("tray_groups", trayGroupXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTray", parameters);

            return result;
        }

        public async Task<int> UpdateProposedTrayDashboard(int proposedTrayId, int instances, string deploymentStatus,
            DateTime? countComplete, DateTime? auditComplete, DateTime? trayChanges, string comments, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("tray_instances", instances),
                new SqlParameter("deployment_status", deploymentStatus ?? (object)DBNull.Value),
                new SqlParameter("count_complete", countComplete ?? (object)DBNull.Value),
                new SqlParameter("audit_complete", auditComplete ?? (object)DBNull.Value),
                new SqlParameter("tray_changes", trayChanges ?? (object)DBNull.Value),
                new SqlParameter("comments", comments ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayDashboard", parameters);

            return result;
        }

        public async Task<int> DeleteProposedTray(int proposedTrayId, int statusUserId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("status_user_id", statusUserId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteProposedTray", parameters);

            return result;
        }

        private string GetInstrumentSummary(List<ProposedTrayInstrumentPost> instruments)
        {
            if (instruments == null || !instruments.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var instrument in instruments)
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, instrument.InstrumentID);
                AddColumn(doc, row, instrument.TrayItemID?.ToString() ?? "");
                AddColumn(doc, row, instrument.Quantity);
            }

            return table.OuterXml;
        }

        private string GetInstrumentUpdateSummary(List<UpdateTrayInstrumentPost> instruments)
        {
            if (instruments == null || !instruments.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var instrument in instruments)
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, instrument.InstrumentID);
                AddColumn(doc, row, instrument.Quantity);
                AddColumn(doc, row, instrument.Reason);
                AddColumn(doc, row, instrument.CategoryID);
                AddColumn(doc, row, instrument.EponymID);
                AddColumn(doc, row, instrument.TypeID);
                AddColumn(doc, row, instrument.Description);
                AddColumn(doc, row, instrument.Size);
                AddColumn(doc, row, instrument.Comments);
                AddColumn(doc, row, instrument.Sequence);
                AddColumn(doc, row, instrument.Notes);
            }

            return table.OuterXml;
        }

        public async Task<List<TrayRationalization>> GetProposedTrays(int? proposedTrayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrays", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayRationalization>();
            var userAssignments = dsSchedules.Tables[1].DataTableToList<TrayProposalUserAssignment>();

            foreach (var tray in result)
            {
                tray.UserAssignments = userAssignments.Where(t => t.TrayProposalID == tray.TrayProposalID).ToList();
            }

            return result;
        }

        public async Task<List<Card>> GetCardsInternal(List<int> locationId)
        {
            var locationXml = GetIdentitySummary(locationId);

            var parameters = new[]
            {
                new SqlParameter("location_id", locationXml ?? (object)DBNull.Value),
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardsInternal", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public async Task<List<ItemMaster>> GetTraysInternal(List<int> locationId)
        {
            var locationXml = GetIdentitySummary(locationId);

            var parameters = new[]
            {
                new SqlParameter("location_id", locationXml ?? (object)DBNull.Value),
            };
            var dsSchedules = await ExecuteCommandAsync("GetTraysInternal", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemMaster>();

            return result;
        }

        public async Task<List<ItemMaster>> GetItemsInternal(List<int> locationId)
        {
            var locationXml = GetIdentitySummary(locationId);

            var parameters = new[]
            {
                new SqlParameter("location_id", locationXml ?? (object)DBNull.Value),
            };
            var dsSchedules = await ExecuteCommandAsync("GetItemsInternal", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemMaster>();

            return result;
        }

        public async Task<List<CardItem>> GetCardItemsInternal(List<int> cardId)
        {
            var cardXml = GetIdentitySummary(cardId);

            var parameters = new[]
            {
                new SqlParameter("card_id", cardXml)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardItemsInternal", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardItem>();

            return result;
        }

        public async Task<List<ItemTray>> GetTrayItemsInternal(List<int> trayId)
        {
            var trayXml = GetIdentitySummary(trayId);

            var parameters = new[]
            {
                new SqlParameter("tray_item_id", trayXml ?? (object)DBNull.Value),
            };
            var dsSchedules = await ExecuteCommandAsync("GetTrayItemsInternal", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemTray>();

            return result;
        }

        public async Task<List<TrayRationalization>> GetProposedTraysInternal(List<int> locationId)
        {
            var locationXml = GetIdentitySummary(locationId);

            var parameters = new[]
            {
                new SqlParameter("location_id", locationXml ?? (object)DBNull.Value),
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTraysInternal", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayRationalization>();
            
            return result;
        }

        public async Task<List<TrayProposalLog>> GetProposedTrayLog(int proposedTrayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayLog", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayProposalLog>();
            var instrumentLog = dsSchedules.Tables[1].DataTableToList<TrayProposalInstrumentLog>();

            foreach (var log in result)
            {
                log.Instruments = instrumentLog.Where(t => t.TrayProposalLogID == log.TrayProposalLogID).ToList();
            }

            return result;
        }

        public async Task<List<TrayRationalization>> GetBaselineTrays(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetBaselineTrays", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayRationalization>();

            return result;
        }

        public async Task<List<TrayRationalizationStatusLog>> GetProposedTrayStatusLog(int proposedTrayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayStatusLog", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayRationalizationStatusLog>();

            return result;
        }

        public async Task<List<TrayProposalHistory>> GetProposedTrayCommunicationHistory(List<int> trayProposalIds, int? phaseId, int? userId,
            int providerId, int locationId)
        {
            var trayProposals = GetIdentitySummary(trayProposalIds);

            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposals ?? (object)DBNull.Value),
                new SqlParameter("phase_id", phaseId ?? (object)DBNull.Value),
                new SqlParameter("user_id", userId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayCommunicationHistory", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayProposalHistory>();

            return result;
        }

        public async Task<int> InsertProposedTrayCommunicationHistory(int phaseId, string activity, int trayProposalId, int audienceUserId,
            int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("phase_id", phaseId),
                new SqlParameter("activity", activity ?? (object)DBNull.Value),
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("audience_user_id", audienceUserId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("InsertProposedTrayCommunicationHistory", parameters);

            return result;
        }

        public async Task<int> UpdateProposedTrayCommunicationHistory(int historyId, string comments,
            int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("history_id", historyId),
                new SqlParameter("comments", comments ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayCommunicationHistory", parameters);

            return result;
        }

        public async Task<List<TrayRationalizationItem>> GetProposedTrayInstruments(int trayProposalId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("proposed_tray_id", trayProposalId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayInstruments", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayRationalizationItem>();
            var comparables = dsSchedules.Tables[1].DataTableToList<ComparableInstrument>();

            var sequence = 0;
            foreach (var item in result.Where(r => r.HistoryType == null))
            {
                if (item.Sequence == null)
                    item.Sequence = (++sequence);

                sequence = item.Sequence ?? 0;

                item.ComparableInstruments = comparables.Where(c => c.InstrumentID == item.InstrumentID).ToList();
            }

            return result;
        }

        public async Task<List<TrayRationalizationItem>> GetProposedTrayInstrumentCategories(int trayProposalId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("proposed_tray_id", trayProposalId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayInstrumentCategories", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayRationalizationItem>();

            return result;
        }

        public async Task<ProposedTrayExport> GetProposedTrayInstrumentExport(int proposedTrayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("proposed_tray_id", proposedTrayId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayInstrumentExport", parameters);

            var result = new ProposedTrayExport()
            {
                ProposedInstruments = dsSchedules.Tables[0].DataTableToList<TrayRationalizationExport>(),
                SourceInstruments = dsSchedules.Tables[1].DataTableToList<TrayRationalizationExport>()
            };

            return result;
        }

        public async Task<int> PutProposedTrayCards(int proposedTrayId, List<CardListTrayPost> trays, int providerId, int locationId)
        {
            var trayXml = GetCardTraySummary(trays);

            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("trays", trayXml),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("PutProposedTrayCards", parameters);

            return result;
        }

        public async Task<List<TrayRationalizationCard>> GetProposedTrayCards(int proposedTrayId, List<CardListTrayPost> trays, int providerId, int locationId)
        {
            var trayXml = GetCardTraySummary(trays);

            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", proposedTrayId),
                new SqlParameter("trays", trayXml),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayCardInstruments", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayRationalizationCard>();

            return result;
        }

        private string GetCardTraySummary(List<CardListTrayPost> trays)
        {
            if (trays == null || !trays.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var tray in trays)
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, tray.CardID);
                AddColumn(doc, row, tray.TrayID);
            }

            return table.OuterXml;
        }

        public async Task<List<TrayCardOverlap>> GetTrayCards(int trayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_item_id", trayId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetTrayCards", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayCardOverlap>();

            return result;
        }

        public async Task<List<TrayApproval>> GetProposedTrayApprovalDocuments(int trayProposalId, int providerId,
            int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayApprovalDocuments", parameters);

            var documents = dsSchedules.Tables[0].DataTableToList<TrayApproval>();

            return documents;
        }

        public async Task<List<TrayCardOverlap>> GetProposedTrayCardOverlap(int trayProposalId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("proposed_tray_id", trayProposalId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayCardOverlap", parameters);

            var instruments = dsSchedules.Tables[0].DataTableToList<TrayCardOverlap>();

            var result = new List<TrayCardOverlap>();
            foreach (var grp in instruments.GroupBy(i => new { i.CardID, i.TrayID }))
            {
                var overlap = new TrayCardOverlap()
                {
                    CardID = grp.Key.CardID,
                    TrayID = grp.Key.TrayID,
                    CardDescription = grp.First().CardDescription,
                    TrayName = grp.First().TrayName,
                    ReplaceCard = grp.First().ReplaceCard,
                    SpecialtyName = grp.First().SpecialtyName,
                    SurgeonName = grp.First().SurgeonName,
                    TimesUsed = grp.First().TimesUsed,
                    MissingInstruments = grp.Sum(g => g.UsedInstruments - g.CurrentTrayItems),
                    CommonInstruments = grp.Sum(g => g.CommonInstruments),
                    UsedInstruments = grp.Sum(g => g.UsedInstruments),
                    CurrentTrayItems = grp.Sum(g => g.CurrentTrayItems)
                };

                result.Add(overlap);
            }

            return result;
        }

        public async Task<InstrumentLookup> GetTrayInstrumentLookups(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetInstrumentLookups", parameters);

            var result = new InstrumentLookup()
            {
                Categories = dsSchedules.Tables[0].DataTableToList<TrayInstrumentCategory>(),
                Eponyms = dsSchedules.Tables[1].DataTableToList<TrayInstrumentEponym>(),
                Types = dsSchedules.Tables[2].DataTableToList<TrayInstrumentType>()
            };

            return result;
        }

        public async Task<List<TraySurgeryAudit>> GetProposedTrayAudits(int? trayProposalId, DateTime? startDate, DateTime? endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("proposed_tray_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("start_date", startDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayAudits", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TraySurgeryAudit>();
            var scrubTechs = dsSchedules.Tables[1].DataTableToList<SurgeryUser>();
            var cptCodes = dsSchedules.Tables[2].DataTableToList<SurgeryCPTCode>();

            foreach (var audit in result)
            {
                audit.SurgeonAuditCount = result.Count(r => r.SurgeonName == audit.SurgeonName);
            }

            foreach (var scrubTech in scrubTechs)
            {
                var surgery = result.FirstOrDefault(r => r.SurgeryID == scrubTech.SurgeryID);
                surgery?.ScrubTechs.Add(scrubTech);
            }

            foreach (var cptCode in cptCodes)
            {
                var surgery = result.FirstOrDefault(r => r.SurgeryID == cptCode.SurgeryID);
                surgery?.CptCodes.Add(cptCode);
            }

            return result;
        }

        public async Task<List<TraySurgeryAudit>> GetProposedTrayCounts(int? trayProposalId, DateTime? startDate, DateTime? endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("proposed_tray_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("start_date", startDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayCounts", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TraySurgeryAudit>();
            var scrubTechs = dsSchedules.Tables[1].DataTableToList<SurgeryUser>();
            var cptCodes = dsSchedules.Tables[2].DataTableToList<SurgeryCPTCode>();

            foreach (var audit in result)
            {
                audit.SurgeonAuditCount = result.Count(r => r.SurgeonName == audit.SurgeonName);
            }

            foreach (var scrubTech in scrubTechs)
            {
                var surgery = result.FirstOrDefault(r => r.SurgeryID == scrubTech.SurgeryID);
                surgery?.ScrubTechs.Add(scrubTech);
            }

            foreach (var cptCode in cptCodes)
            {
                var surgery = result.FirstOrDefault(r => r.SurgeryID == cptCode.SurgeryID);
                surgery?.CptCodes.Add(cptCode);
            }

            return result;
        }        

        public async Task<List<TrayProposalSchedule>> GetProposedTraySchedule(int? trayProposalId, int? vendorId, int userId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("vendor_id", vendorId ?? (object)DBNull.Value),
                new SqlParameter("user_id", userId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTraySchedule", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayProposalSchedule>();
            var surgeryUsers = dsSchedules.Tables[1].DataTableToList<SurgeryUser>();
            var cardCategories = dsSchedules.Tables[2].DataTableToList<ProposalCardCategory>();

            foreach (var surgery in result)
            {
                surgery.SurgeryUsers = surgeryUsers.Where(su => su.SurgeryID == surgery.SurgeryID).ToList();
                surgery.CardCategories = cardCategories.Where(cc => cc.TrayProposalID == surgery.TrayProposalID).ToList();
            }

            return result;
        }

        public async Task<List<TrayProposalDashboard>> GetProposedTrayDashboard(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayDashboard", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayProposalDashboard>();
            
            return result;
        }

        public async Task<TrayProposalScheduleRule> GetProposedTrayScheduleRules(int userId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayScheduleRules", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayProposalScheduleRule>().FirstOrDefault() ?? new TrayProposalScheduleRule();

            return result;
        }

        public async Task<List<AdminTrayProposal>> GetProposedTrayAlert()
        {
            var parameters = new SqlParameter[0];
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayAlert", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<AdminTrayProposal>();
            var reps = dsSchedules.Tables[1].DataTableToList<TrayProposalRep>();

            foreach (var proposal in result)
            {
                proposal.ProposalUsers = reps.Where(r => r.TrayProposalID == proposal.TrayProposalID).ToList();
            }

            return result;
        }

        public async Task<List<TrayCountSummary>> GetTrayCountSummary(int trayProposalId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("proposed_tray_id", trayProposalId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetTrayCountSummary", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayCountSummary>();
            
            return result;
        }

        public async Task<List<SourceTraySummary>> GetSourceTraySummary(int trayProposalId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTraySummary", parameters);
            var result = dsSchedules.Tables[0].DataTableToList<SourceTraySummary>();
            var instruments = dsSchedules.Tables[1].DataTableToList<ItemTrayOverlap>();

            foreach (var tray in result)
            {
                tray.Instruments = instruments.Where(i => i.TrayItemID == tray.TrayItemID).ToList();
            }

            return result;
        }

        public async Task<List<TrayRationalizationCardCategory>> GetProposedTrayCardCategories(int trayProposalId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayCardCategories", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayRationalizationCardCategory>();

            return result;
        }

        public async Task<List<TraySurgeryAudit>> GetProposedTrayAuditSummary(DateTime startDate, DateTime endDate,
            int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("start_date", startDate),
                new SqlParameter("end_date", endDate),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayAuditSummary", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TraySurgeryAudit>();
            var trays = dsSchedules.Tables[1].DataTableToList<SurgeryAuditSourceTray>();

            foreach (var sourceTray in trays)
            {
                var audit = result.FirstOrDefault(r => r.TrayProposalID == sourceTray.TrayProposalID && r.SurgeryID == sourceTray.SurgeryID);
                audit?.SourceTrays.Add(sourceTray);
            }

            return result;
        }
        public async Task<List<TrayProposalSchedule>> GetProposedTrayScheduleHistory(int? caseProfileId, int? vendorId, int? surgeonId, int? trayProposalId, int? categoryId, int? questionId,
            int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("case_profile_id", caseProfileId ?? (object)DBNull.Value),
                new SqlParameter("vendor_id", vendorId ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonId ?? (object)DBNull.Value),
                new SqlParameter("tray_proposal_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("category_id", categoryId ?? (object)DBNull.Value),
                new SqlParameter("question_id", questionId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsResult = await ExecuteCommandAsync("GetProposedTrayScheduleHistory", parameters);

            var surgeries = dsResult.Tables[0].DataTableToList<TrayProposalSchedule>();

            return surgeries;
        }

        public async Task<List<TrayProposalSchedule>> SearchCaseTraySchedule(int? surgeonId, int? proposedTrayId, DateTime startDate, DateTime endDate,
            int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgeon_user_id", surgeonId ?? (object)DBNull.Value),
                new SqlParameter("tray_proposal_id", proposedTrayId ?? (object)DBNull.Value),
                new SqlParameter("beg_date", startDate),
                new SqlParameter("end_date", endDate),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("SearchCaseTraySchedule", parameters);

            var surgeries = dsSchedules.Tables[0].DataTableToList<TrayProposalSchedule>();
            var surgeryUsers = dsSchedules.Tables[1].DataTableToList<SurgeryUser>();

            foreach (var surgery in surgeries)
            {
                surgery.SurgeryUsers = surgeryUsers.Where(su => su.SurgeryID == surgery.SurgeryID).ToList();
            }

            return surgeries;
        }

        public async Task<CaseProfile> GetCaseProfile(int caseProfileId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("case_profile_id", caseProfileId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCaseProfile", parameters);

            var caseProfile = dsSchedules.Tables[0].DataTableToList<CaseProfile>().First();
            var questions = dsSchedules.Tables[1].DataTableToList<CaseProfileQuestionResult>();

            caseProfile.ParseResults(questions, null);

            return caseProfile;
        }

        public async Task<List<CaseProfile>> GetCaseProfiles(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("case_profile_id", DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCaseProfile", parameters);

            var caseProfiles = dsSchedules.Tables[0].DataTableToList<CaseProfile>();
            var questions = dsSchedules.Tables[1].DataTableToList<CaseProfileQuestionResult>();

            foreach (var caseProfile in caseProfiles)
            {
                caseProfile.ParseResults(questions.Where(q => q.CaseProfileID == caseProfile.CaseProfileID), null);
            }

            return caseProfiles;
        }

        public async Task<List<SurgeonPreference>> GetSurgeonPreferences(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeonPreferences", parameters);

            var surgeonPreferences = dsSchedules.Tables[0].DataTableToList<SurgeonPreference>();
            foreach (var surgeonPreference in surgeonPreferences.Where(sp => sp.TrayGroup != null))
            {
                surgeonPreference.TrayGroupID = JsonConvert.DeserializeObject<List<int>>(surgeonPreference.TrayGroup);
            }

            return surgeonPreferences;
        }

        public async Task<CaseProfile> GetSurgeryCaseProfile(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryCaseProfile", parameters);

            var caseProfiles = dsSchedules.Tables[0].DataTableToList<CaseProfile>();
            var validAnswers = dsSchedules.Tables[1].DataTableToList<CaseProfileQuestionResult>();
            var questionAnswers = dsSchedules.Tables[2].DataTableToList<CaseProfileQuestionResult>();

            foreach (var caseProfile in caseProfiles)
            {
                caseProfile.ParseResults(validAnswers, questionAnswers);
            }

            return caseProfiles.FirstOrDefault();
        }

        public async Task<int> UpdateCaseProfile(int? caseProfileId, string profileName, string profileType, List<CaseProfileQuestion> questions,
            int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("case_profile_id", caseProfileId ?? (object)DBNull.Value),
                new SqlParameter("profile_name", profileName ?? (object)DBNull.Value),
                new SqlParameter("profile_type", profileType ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsResult = await ExecuteCommandAsync("UpdateCaseProfile", parameters);

            var identity = dsResult.Tables[0].DataTableToList<InsertionResult>().First().Identifier;
            
            foreach (var question in questions ?? new List<CaseProfileQuestion>())
            {
                var answerXml = GetProfileAnswerSummary(question.ValidAnswers);

                parameters = new[]
                {
                    new SqlParameter("case_profile_id", identity),
                    new SqlParameter("question_id", question.QuestionID ?? (object)DBNull.Value),
                    new SqlParameter("question", question.Question ?? (object)DBNull.Value),
                    new SqlParameter("answers", answerXml ?? (object)DBNull.Value),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId)
                };

                var q = await ExecuteNonQueryAsync("UpdateCaseProfileQuestion", parameters);
            }

            return identity;
        }
        public async Task<int> UpdateScheduleRules(int userId, string surgeon, string category, DateTime? startDate, DateTime? endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("surgeon", surgeon ?? (object)DBNull.Value),
                new SqlParameter("category", category ?? (object)DBNull.Value),
                new SqlParameter("start_date", startDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateScheduleRules", parameters);

            return result;
        }

        public async Task<int> UpdateProposedTrayRep(int trayProposalId, int userId, bool ignore, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("user_id", userId),
                new SqlParameter("ignore_flag", ignore),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayRep", parameters);

            return result;
        }

        public async Task<int> UpdateProposedTrayLog(int trayProposalLogId, 
            string requestor, string audience, string changeType, string changeDescription, string affectedItems, DateTime changeDate,
            string comments, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_log_id", trayProposalLogId),
                new SqlParameter("requestor", requestor),
                new SqlParameter("audience", audience),
                new SqlParameter("change_type", changeType),
                new SqlParameter("change_description", changeDescription),
                new SqlParameter("affected_items", affectedItems),
                new SqlParameter("change_date", changeDate),
                new SqlParameter("comments", comments),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsResult = await ExecuteCommandAsync("UpdateProposedTrayLog", parameters);

            var result = dsResult.Tables[0].DataTableToList<InsertionResult>().First();

            return result.Identifier;
        }
        public async Task<int> UpdateProposedTrayImageFilename(int trayProposalId, string imageFilename, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("image_filename", imageFilename ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayImageFilename", parameters);

            return result;
        }
        private string GetProfileAnswerSummary(List<CaseProfileAnswer> answers)
        {
            if (!answers.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var answer in answers)
            {
                // prevent adding invalid data
                if (string.IsNullOrEmpty(answer.Answer))
                    continue;

                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, answer.AnswerID);
                AddColumn(doc, row, answer.Answer);
            }

            return table.OuterXml;
        }

        public async Task<List<TrayProposalSchedule>> GetSurgeryTraySchedule(int surgeryId, int? vendorId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("vendor_id", vendorId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryTraySchedule", parameters);

            var trayProposals = dsSchedules.Tables[0].DataTableToList<TrayProposalSchedule>();
            
            return trayProposals;
        }

        public async Task<int> UpdateProposedTrayApproval(int trayProposalId, string filename, int typeId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("filename", filename),
                new SqlParameter("type_id", typeId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayApproval", parameters);

            return result;
        }
        public async Task<List<ProposedTrayOrgChart>> GetOrgChartAttachments(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsAttachments = await ExecuteCommandAsync("GetProposedTrayOrgChartAttachments", parameters);

            var result = dsAttachments.Tables[0].DataTableToList<ProposedTrayOrgChart>();

            return result;
        }
        public async Task<int> DeleteProposedTrayOrgChart(int? orgChartId, int? trayProposalId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("org_chart_id", orgChartId ?? (object)DBNull.Value),
                new SqlParameter("tray_proposal_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteProposedTrayOrgChart", parameters);

            return result;
        }
        public async Task<int> UpdateOrgChartAttachment(string type, int? trayProposalId, string filename, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("type", type),
                new SqlParameter("filename", filename),
                new SqlParameter("tray_proposal_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsAttachments = await ExecuteCommandAsync("UpdateProposedTrayOrgChartAttachment", parameters);

            var result = dsAttachments.Tables[0].DataTableToList<InsertionResult>().First();

            return result.Identifier;
        }

        public async Task<List<ImplementationAttachment>> GetImplementationAttachments(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetImplementationAttachments", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ImplementationAttachment>();

            return result;
        }

        public async Task<int> DeleteImplementationAttachment(int attachmentId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("attachment_id", attachmentId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteImplementationAttachment", parameters);

            return result;
        }

        public async Task<int> UpdateImplementationAttachment(int target, string filename, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("target", target),
                new SqlParameter("filename", filename),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsAttachment = await ExecuteCommandAsync("UpdateImplementationAttachment", parameters);

            var result = dsAttachment.Tables[0].DataTableToList<InsertionResult>().First();

            return result.Identifier;
        }

        public async Task<int> UpdateConsolidationPlan(List<TrayRationalization> consolidations, int providerId, int locationId)
        {
            var consolidationPlanXml = SummarizeConsolidations(consolidations);

            var parameters = new[]
            {
                new SqlParameter("consolidations", consolidationPlanXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayConsolidationPlan", parameters);

            return result;
        }

        public async Task<int> UpdateSurgeryCaseProfile(int surgeryId, int? caseProfileId,
            List<CaseProfileQuestionPost> questions, int providerId, int locationId)
        {
            var questionXml = SummarizeQuestions(questions);

            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("case_profile_id", caseProfileId),
                new SqlParameter("questions", questionXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateSurgeryCaseProfile", parameters);

            return result;
        }

        public async Task<int> UpdateSurgeryPerioperativeCaseProfile(int surgeryId, 
            List<CaseProfileQuestionPost> questions, int providerId, int locationId)
        {
            var questionXml = SummarizeQuestions(questions);

            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("questions", questionXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateSurgeryPerioperativeCaseProfile", parameters);

            return result;
        }

        public async Task<int> UpdateProposedTraySchedule(int surgeryId, int? trayProposalId, int? trayGroupId,
            int caseProfileId,
            List<CaseProfileQuestionPost> questions, int providerId, int locationId)
        {
            var questionXml = SummarizeQuestions(questions);

            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("case_profile_id", caseProfileId),
                new SqlParameter("tray_proposal_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("tray_group_id", trayGroupId ?? (object)DBNull.Value),
                new SqlParameter("questions", questionXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTraySchedule", parameters);

            return result;
        }

        public async Task<int> UpdateSurgeonPreference(int? preferenceId, string preferenceName,
            int? surgeonId, int? caseProfileId, string trayGroup, string comments, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgeon_preference_id", preferenceId ?? (object)DBNull.Value),
                new SqlParameter("preference_name", preferenceName ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonId ?? (object)DBNull.Value),
                new SqlParameter("case_profile_id", caseProfileId ?? (object)DBNull.Value),
                new SqlParameter("tray_group_id", trayGroup ?? (object)DBNull.Value),
                new SqlParameter("comments", comments ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateSurgeonPreference", parameters);

            return result;
        }

        public async Task<int> UpdateProposedTrayScheduleDetails(int scheduleId, string supplies, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("schedule_id", scheduleId),
                new SqlParameter("supplies", supplies ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayScheduleDetails", parameters);

            return result;
        }
        public async Task<int> UpdateProposedTrayRoles(int trayProposalId, int? ownerUserId, List<int> approvers,
            List<int> users, int providerId, int locationId)
        {
            var approverXml = GetIdentitySummary(approvers);
            var userXml = GetIdentitySummary(users);

            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("owner_user_id", ownerUserId ?? (object)DBNull.Value),
                new SqlParameter("approvers", approverXml ?? (object)DBNull.Value),
                new SqlParameter("users", userXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayRoles", parameters);

            return result;
        }
        private string SummarizeConsolidations(List<TrayRationalization> consolidations)
        {
            if (consolidations == null || !consolidations.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var question in consolidations)
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, question.TrayProposalID);
                AddColumn(doc, row, question.Level1Target);
                AddColumn(doc, row, question.Level1Instances);
                AddColumn(doc, row, question.Level2Target);
                AddColumn(doc, row, question.Level2Instances);
                AddColumn(doc, row, question.Level3Target);
                AddColumn(doc, row, question.Level3Instances);
            }

            return table.OuterXml;
        }
        private string SummarizeQuestions(List<CaseProfileQuestionPost> questions)
        {
            if (questions == null || !questions.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var question in questions ?? new List<CaseProfileQuestionPost>())
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, question.QuestionID);

                if (question.AnswerID == null || !question.AnswerID.Any())
                {
                    AddColumn(doc, row, "0");
                    continue;
                }

                foreach (var answer in question.AnswerID ?? new List<int>())
                {
                    AddColumn(doc, row, answer);
                }
            }
            
            return table.OuterXml;
        }

        public async Task<List<User>> GetProposedTrayCommunicationTeam(int trayProposalId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsResult = await ExecuteCommandAsync("GetProposedTrayCommunicationTeam", parameters);

            var users = dsResult.Tables[0].DataTableToList<User>();

            return users;
        }

        public async Task<List<Messaging>> GetProposedTrayCommunication(int userId, int trayProposalId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsResult = await ExecuteCommandAsync("GetProposedTrayCommunication", parameters);

            var history = dsResult.Tables[0].DataTableToList<Messaging>();

            return history;
        }

        public async Task<int> InsertProposedTrayCommunication(int? senderId, int trayProposalId, string message, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", senderId ?? (object)DBNull.Value),
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("message", message),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("InsertProposedTrayCommunication", parameters);

            return result;
        }

        public async Task<int> UpdateProposedTrayCommunicationStatus(int trayProposalId, string status, int userId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("communication_status", status),
                new SqlParameter("user_id", userId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayCommunicationStatus", parameters);

            return result;
        }

        public async Task<int> UpdateProposedTrayAudit(int? trayProposalId, int surgeryId, int? scrubTechUserId, int? auditUserId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("scrub_tech_user_id", scrubTechUserId ?? (object)DBNull.Value),
                new SqlParameter("audit_user_id", auditUserId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayAudit", parameters);

            return result;
        }

        public async Task<int> UpdateProposedTrayCount(int? trayProposalId, int surgeryId, int? scrubTechUserId, int? countUserId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("scrub_tech_user_id", scrubTechUserId ?? (object)DBNull.Value),
                new SqlParameter("count_user_id", countUserId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayCount", parameters);

            return result;
        }

        public async Task<int> DeleteProposedTrayAudit(int trayProposalId, int surgeryId, string target, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("target", target),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteProposedTrayAudit", parameters);

            return result;
        }

        public async Task<int> InsertItemMaster(string itemType, string itemName, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("item_type", itemType),
                new SqlParameter("item_name", itemName),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteCommandAsync("InsertItemMaster", parameters);

            return result.Tables[0].DataTableToList<InsertionResult>().First().Identifier;
        }

        public async Task<int> InsertComparableItem(int itemId, int relatedItemId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("item_id", itemId),
                new SqlParameter("related_item_id", relatedItemId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("InsertComparableItem", parameters);

            return result;
        }

        public async Task<int> InsertComparableInstrument(int instrumentId, int trayItemId, int relatedInstrumentId, int relatedTrayItemId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("instrument_id", instrumentId),
                new SqlParameter("tray_item_id", trayItemId),
                new SqlParameter("related_instrument_id", relatedInstrumentId),
                new SqlParameter("related_tray_item_id", relatedTrayItemId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("InsertComparableInstrument", parameters);

            return result;
        }

        public async Task<int> DeleteComparableInstrument(int? trayProposalId, int instrumentId, int comparableInstrumentId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("instrument_id", instrumentId),
                new SqlParameter("comparable_instrument_id", comparableInstrumentId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteComparableInstrument", parameters);

            return result;
        }

        public async Task<int> DeleteComparableItem(int comparableItemId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("comparable_item_id", comparableItemId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteComparableItem", parameters);

            return result;
        }

        public async Task<int> DeleteComparableInstrument(int comparableInstrumentId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("comparable_instrument_id", comparableInstrumentId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteComparableInstrument", parameters);

            return result;
        }

        public async Task<TrayCardOverlapSummary> GetTrayOverlapSummary(int trayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_id", trayId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetTrayOverlapSummary", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayCardOverlapSummary>().FirstOrDefault();

            return result;
        }

        public async Task<List<ImportType>> GetImportTypes(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetImportList", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ImportType>();

            return result;
        }

        public async Task<List<ImportDefinition>> GetImportDefinition(int importTypeId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("import_id", importTypeId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetImportDefinition", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ImportDefinition>();

            return result;
        }

        public async Task<List<ImportMessage>> GetImportMessages(int importLogId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("import_log_id", importLogId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetImportMessages", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ImportMessage>();

            return result;
        }

        public async Task<OverviewScreen> GetCaseOverview(int providerId, int locationId, DateTime beginDate, DateTime endDate, int? specialtyId, int? bundleId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("begin_date", beginDate),
                new SqlParameter("end_date", endDate),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("bundle_id", bundleId ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCaseOverview", parameters);

            var result = new OverviewScreen();

            result.CaseOverview = dsSchedules.Tables[0].DataTableToList<CaseOverview>().FirstOrDefault();
            result.CaseSurgeonOverview = dsSchedules.Tables[1].DataTableToList<CaseSurgeonOverview>();
            result.CaseBundleOverview = dsSchedules.Tables[2].DataTableToList<CaseBundleOverview>();

            return result;
        }

        public async Task<List<Patient>> GetCleanupPatients(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsPatient = await ExecuteCommandAsync("GetCleanupPatientList", parameters);

            var result = dsPatient.Tables[0].DataTableToList<Patient>();

            return result;
        }

        public async Task<List<ImportLog>> GetImportLog(int importTypeId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("import_id", importTypeId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetImportLog", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ImportLog>();

            return result;
        }

        public async Task<List<Messaging>> GetMessaging(int userId, int? surgeryId, int? caseGroupId, int? recipientId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("surgery_id", surgeryId ?? (object)DBNull.Value),
                new SqlParameter("case_group_id", caseGroupId ?? (object)DBNull.Value),
                new SqlParameter("recipient_id", recipientId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetMessaging", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Messaging>();

            return result;
        }

        public async Task<List<MessagingGroup>> GetMessageGroups(int userId, DateTime? startDate, DateTime? endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("start_date", startDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetMessagingGroups", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<MessagingGroup>();

            return result;
        }

        public async Task SendMessage(int userId, int providerId, int locationId,
            int? surgeryId, int? communicationUserId, string message)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId ?? (object)DBNull.Value),
                new SqlParameter("communication_user_id", communicationUserId ?? (object)DBNull.Value),
                new SqlParameter("message", message)
            };
            var result = await ExecuteNonQueryAsync("InsertMessage", parameters);
        }

        public async Task AcknowledgeMessage(int userId, int providerId, int locationId, int messageId, bool hideMessages)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("message_id", messageId),
                new SqlParameter("hide_messages", hideMessages)
            };
            var result = await ExecuteNonQueryAsync("InsertMessageAcknowledgement", parameters);
        }

        public async Task DeletePrivateConversation(int communicationUserId, int userId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("communication_user_id", communicationUserId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeletePrivateConversation", parameters);
        }

        public async Task<List<Card>> GetCards(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardList", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public async Task<List<ItemMaster>> GetItems(string itemType, int? trayId, bool? countNeeded, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("item_type", itemType ?? (object)DBNull.Value),
                new SqlParameter("tray_id", trayId ?? (object)DBNull.Value),
                new SqlParameter("count_needed", countNeeded ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemMaster>();

            return result;
        }

        public async Task<List<ItemMasterCategory>> GetItemCategories(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetItemCategories", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemMasterCategory>();

            return result;
        }

        public async Task<List<ItemMasterCategory>> GetInstrumentCategories(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetInstrumentCategories", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemMasterCategory>();

            return result;
        }

        public async Task<List<ItemMaster>> GetItemSutures(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetItemSutures", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemMaster>();

            return result;
        }

        public async Task<List<ItemMaster>> GetInstruments(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetInstruments", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemMaster>();

            return result;
        }

        public async Task<PaginationController> GetInstrumentsPaged(string searchTerm, int page, int providerId, int locationId)
        {
            var pageSize = 50;
            var parameters = new[]
            {
                new SqlParameter("search_term", searchTerm ?? (object)DBNull.Value),
                new SqlParameter("page", page),
                new SqlParameter("page_size", pageSize),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetInstrumentsPaged", parameters);

            var instruments = dsSchedules.Tables[0].DataTableToList<ItemMaster>();
            var totalCount = dsSchedules.Tables[1].DataTableToList<RowCountEntity>().First().TotalCount;

            var results = instruments.Select(i => new KeyPair() {id = i.ItemID, text = i.ItemDescription}).ToList();
            var skipped = (page - 1) * pageSize;

            return new PaginationController()
            {
                pagination = new PaginationResult(instruments.Count, skipped, totalCount),
                results = results
            };
        }

        public async Task<List<ItemTray>> GetTrayItems(int trayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("tray_item_id", trayId)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayItems", parameters);

            return dsItems.Tables[0].DataTableToList<ItemTray>();
        }

        public async Task<List<ItemTray>> GetTrayItemCategories(int trayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("tray_item_id", trayId)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayItemCategories", parameters);

            return dsItems.Tables[0].DataTableToList<ItemTray>();
        }

        public async Task<List<ItemTrayOverlap>> GetTrayItemOverlaps(int trayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("tray_item_id", trayId)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayItems", parameters);

            return dsItems.Tables[0].DataTableToList<ItemTrayOverlap>();
        }

        public async Task<List<ItemTrayOverlap>> GetProposedTrayItemOverlaps(int trayProposalId, int? trayProposalLogId, int providerId, int locationId)
        {
            DataSet dsItems;

            if (trayProposalLogId.HasValue)
            {
                var parameters = new[]
                {
                    new SqlParameter("tray_proposal_id", trayProposalId),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId)
                };
                var dsSchedules = await ExecuteCommandAsync("GetProposedTrayLog", parameters);

                var instrumentLog = dsSchedules.Tables[1].DataTableToList<ItemTrayOverlap>();

                return instrumentLog.Where(i => i.TrayProposalLogID == trayProposalLogId).ToList();
            }
            else
            {

                var parameters = new[]
                {
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId),
                    new SqlParameter("proposed_tray_id", trayProposalId)
                };
                dsItems = await ExecuteCommandAsync("GetProposedTrayInstruments", parameters);

                return dsItems.Tables[0].DataTableToList<ItemTrayOverlap>();
            }
        }

        public async Task<List<TrayQuestionSummary>> GetTrayQuestions(int? itemId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("item_id", itemId ?? (object)DBNull.Value)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayQuestions", parameters);
            var answers = dsItems.Tables[0].DataTableToList<TrayQuestion>();

            var summary = answers.GroupBy(r => r.QuestionID);

            var questions = summary.Select(questionAnswers => new TrayQuestionSummary
                {
                    QuestionID = questionAnswers.Key,
                    Question = questionAnswers.First().Question,
                    Answers = questionAnswers.ToList()
                })
                .ToList();

            return questions;
        }

        public async Task<List<TrayProposalPhase>> GetTrayProposalPhases(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayProposalPhases", parameters);
            var phases = dsItems.Tables[0].DataTableToList<TrayProposalPhase>();

            return phases;
        }

        public async Task<List<TrayRationalizationReduction>> GetTrayRationalizationReduction(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayRationalizationReduction", parameters);
            var phases = dsItems.Tables[0].DataTableToList<TrayRationalizationReduction>();

            return phases;
        }

        public async Task<List<TrayRationalizationUsage>> GetTrayRationalizationUsage(int? trayPlanId, int? specialtyId, 
            int? instrumentCategoryId, int? trayItemId, int? instrumentId, int? cardCategoryId,
            int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_plan_id", trayPlanId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("instrument_category_id", instrumentCategoryId ?? (object)DBNull.Value),
                new SqlParameter("tray_item_id", trayItemId ?? (object)DBNull.Value),
                new SqlParameter("instrument_id", instrumentId ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayRationalizationUsage", parameters);
            var result = dsItems.Tables[0].DataTableToList<TrayRationalizationUsage>();
            var details = dsItems.Tables[1].DataTableToList<TrayRationalizationUsageDetail>();

            foreach (var detail in details.GroupBy(d => d.InstrumentID))
            {
                var instrument = result.FirstOrDefault(r => r.InstrumentID == detail.Key);
                if (instrument == null)
                    continue;

                instrument.Details = detail.ToList();
            }

            return result;
        }

        public async Task<List<TrayPlan>> GetTrayPlans(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayPlans", parameters);
            var result = dsItems.Tables[0].DataTableToList<TrayPlan>();
            
            return result;
        }

        public async Task<TrayPlanDetail> GetTrayPlanDetail(int trayPlanId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_plan_id", trayPlanId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayPlanDetail", parameters);
            var result = dsItems.Tables[0].DataTableToList<TrayPlanDetail>();

            return result.FirstOrDefault();
        }

        public async Task<List<TrayPlanInstrument>> GetTrayPlanInstruments(int trayPlanId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_plan_id", trayPlanId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayPlanInstruments", parameters);
            var result = dsItems.Tables[0].DataTableToList<TrayPlanInstrument>();

            return result;
        }

        public async Task<List<TrayPlanExcessTray>> GetTrayPlanExcessTrays(int trayPlanId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_plan_id", trayPlanId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayPlanExcessTrays", parameters);
            var result = dsItems.Tables[0].DataTableToList<TrayPlanExcessTray>();

            return result;
        }

        public async Task<int> UpdateTrayPlan(int? trayPlanId, string planName, int? specialtyId, List<TrayPlanInstrumentUsage> instruments,
            List<int> excessTrays,
            int providerId, int locationId)
        {
            var instrumentXml = SummarizePlanInstruments(instruments);
            var excessTrayXml = GetIdentitySummary(excessTrays);

            var parameters = new[]
            {
                new SqlParameter("tray_plan_id", trayPlanId ?? (object)DBNull.Value),
                new SqlParameter("plan_name", planName ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("instruments", instrumentXml ?? (object)DBNull.Value),
                new SqlParameter("excess_trays", excessTrayXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsPlan = await ExecuteCommandAsync("UpdateTrayPlan", parameters);

            var result = dsPlan.Tables[0].DataTableToList<InsertionResult>().First();

            return result.Identifier;
        }
        
        public async Task<int> UpdateTrayPlanDetail(int trayPlanId, string planType, List<TrayPlanInstrumentDetail> instruments,
            int providerId, int locationId)
        {
            var instrumentXml = SummarizePlanDetails(instruments);

            var parameters = new[]
            {
                new SqlParameter("tray_plan_id", trayPlanId),
                new SqlParameter("plan_type", planType),
                new SqlParameter("instruments", instrumentXml),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var result = await ExecuteNonQueryAsync("UpdateTrayPlanDetail", parameters);

            return result;
        }

        private string SummarizePlanInstruments(List<TrayPlanInstrumentUsage> instruments)
        {
            if (instruments == null || !instruments.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var instrument in instruments)
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, instrument.InstrumentID);
                AddColumn(doc, row, instrument.TrayID);
                AddColumn(doc, row, instrument.Main);
                AddColumn(doc, row, instrument.Add);
                AddColumn(doc, row, instrument.Single);
                AddColumn(doc, row, instrument.Peel);
            }

            return table.OuterXml;
        }

        private string SummarizePlanDetails(List<TrayPlanInstrumentDetail> instruments)
        {
            if (instruments == null || !instruments.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var instrument in instruments)
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, instrument.InstrumentID);
                AddColumn(doc, row, instrument.TrayID);
                AddColumn(doc, row, instrument.TargetTray);
                AddColumn(doc, row, instrument.Quantity);
            }

            return table.OuterXml;
        }

        public async Task<int> InsertTrayInstrument(string instrumentName, string instrumentNbr, 
            int trayId, int trayQuantity, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("instrument_name", instrumentName),
                new SqlParameter("instrument_nbr", instrumentNbr ?? (object)DBNull.Value),
                new SqlParameter("tray_id", trayId),
                new SqlParameter("tray_quantity", trayQuantity)
            };
            var dsItems = await ExecuteCommandAsync("InsertTrayInstrument", parameters);

            var result = dsItems.Tables[0].DataTableToList<InsertionResult>().First();

            return result.Identifier;
        }

        public async Task<List<ItemMaster>> GetCollectionTrays(int itemId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("item_id", itemId)
            };
            var dsItems = await ExecuteCommandAsync("GetCollectionTrays", parameters);

            return dsItems.Tables[0].DataTableToList<ItemMaster>();
        }

        public async Task<List<TrayHistory>> GetTrayHistory(int? specialtyId, int? userId, int? cardId, DateTime? beginDate, DateTime? endDate, int? itemId, 
            List<TrayQuestion> questions, int providerId, int locationId)
        {
            var filters = GetQuestionSummary(questions);

            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("user_id", userId ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardId ?? (object)DBNull.Value),
                new SqlParameter("begin_date", beginDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("item_id", itemId ?? (object)DBNull.Value),
                new SqlParameter("filters", filters ?? (object)DBNull.Value)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayHistory", parameters);

            return dsItems.Tables[0].DataTableToList<TrayHistory>();
        }

        private string GetQuestionSummary(List<TrayQuestion> questions)
        {
            if (questions == null || !questions.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var question in questions)
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, question.QuestionID);
                AddColumn(doc, row, question.Answer);
            }

            return table.OuterXml;
        }

        public async Task<List<CardDetail>> GetCardData(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardData", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardDetail>();

            return result;
        }

        public async Task<CardUsageHistory> GetCardUsageHistory(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardUsageHistory", parameters);

            var result = new CardUsageHistory()
            {
                Summary = dsSchedules.Tables[0].DataTableToList<CardSummary>().FirstOrDefault(),
                TrayItems = dsSchedules.Tables[1].DataTableToList<ItemUsageHistory>(),
                Supplies = dsSchedules.Tables[2].DataTableToList<ItemUsageHistory>(),
            };
            
            return result;
        }

        public async Task<List<CardItem>> GetCardItems(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardItem>();

            return result;
        }

        public async Task<List<SurgeryCardItem>> GetSurgeryCardItems(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryCardItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryCardItem>();

            return result;
        }

        public async Task<List<SurgeryInstrumentCount>> GetCardAdditionalItems(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardAdditionalItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryInstrumentCount>();

            return result;
        }

        public async Task<List<Procedure>> GetCardProcedures(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardProcedures", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Procedure>();

            return result;
        }

        public async Task<CardItemCountQueryResult> GetSurgeryCardItemCounts(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryCardItemCounts", parameters);

            var result = new CardItemCountQueryResult
            {
                CardItemCounts = dsSchedules.Tables[0].DataTableToList<CardItemCount>(),
                TrayCollectionCounts = dsSchedules.Tables[1].DataTableToList<CollectionItemCount>(),
                TrayQuestions = dsSchedules.Tables[2].DataTableToList<TrayQuestion>()
            };

            return result;
        }
        
        public async Task<List<SurgerySutureCount>> GetSurgerySutureCounts(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgerySutureCounts", parameters);

            return dsSchedules.Tables[0].DataTableToList<SurgerySutureCount>();
        }

        public async Task<List<SurgeryTrayOpen>> GetSurgeryTrayOpens(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryTrayOpens", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryTrayOpen>();

            return result;
        }

        public async Task<int> UpdateSurgeryTrayOpens(int surgeryId, int providerId, int locationId, int trayId, bool trayOpened)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("tray_id", trayId),
                new SqlParameter("tray_opened", trayOpened)
            };
            var result = await ExecuteNonQueryAsync("UpdateSurgeryTrayOpens", parameters);

            return result;
        }

        public async Task<int> UpdateSurgeryTrayFeedback(int surgeryId, int trayId, string feedback, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("tray_id", trayId),
                new SqlParameter("feedback", feedback ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateSurgeryTrayFeedback", parameters);

            return result;
        }

        public async Task<int> UpdateSurgeryEstCompTime(int surgeryId, DateTime estCompTime, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("est_comp_time", estCompTime),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateSurgeryEstCompTime", parameters);

            return result;
        }

        public async Task<List<Card>> GetCardSurgeryDelays(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardSurgeryDelayItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public async Task<List<Card>> GetCardList(int? userId, int? specialtyId, int? procedureId, int? bundleId, bool defaultCardOnly, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("procedure_id", procedureId ?? (object)DBNull.Value),
                new SqlParameter("bundle_id", bundleId ?? (object)DBNull.Value),
                new SqlParameter("default_flag", defaultCardOnly),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardListbyProcedure", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();
            var cardSources = dsSchedules.Tables[1].DataTableToList<CardSource>()
                .GroupBy(cs => cs.CardID);

            foreach (var cardSource in cardSources)
            {
                var card = result.FirstOrDefault(c => c.CardID == cardSource.Key);

                if (card != null)
                    card.Sources = cardSource.ToList();
            }

            return result;
        }

        public async Task<List<CardCategoryXRef>> GetCardCategoryXRef(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardCategoryXRefDetail", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardCategoryXRef>();
            
            return result;
        }

        public async Task<List<CardWithCategory>> GetCardCategoryXRef(int? userId, int? specialtyId, string cardName, string hierarchyLevel, int? cardCategoryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("card_name", cardName ?? (object)DBNull.Value),
                new SqlParameter("hierarchy_level", hierarchyLevel ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardCategoryXRef", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardWithCategory>();
            var categories = dsSchedules.Tables[1].DataTableToList<CardCategoryXRef>();

            foreach (var cardCategory in categories.GroupBy(c => c.CardID))
            {
                var card = result.FirstOrDefault(c => c.CardID == cardCategory.Key);

                if (card != null)
                    card.CardCategories = cardCategory.ToList();
            }

            return result;
        }

        public async Task<List<Card>> GetUsedCardList(List<int> userIds, List<int> trayIds, List<int> categoryIds, int providerId, int locationId)
        {
            var userXml = GetIdentitySummary(userIds);
            var trayXml = GetIdentitySummary(trayIds);
            var categoryXml = GetIdentitySummary(categoryIds);

            var parameters = new[]
            {
                new SqlParameter("user_id", userXml ?? (object)DBNull.Value),
                new SqlParameter("tray_id", trayXml ?? (object)DBNull.Value),
                new SqlParameter("category_id", categoryXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetUsedCardList", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();
            
            return result;
        }

        public async Task<List<CardItemFeedback>> GetCardFeedback(int? specialtyId, int? userId, int? cardId, DateTime? beginDate, DateTime? endDate,
            int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("user_id", userId ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardId ?? (object)DBNull.Value),
                new SqlParameter("begin_date", beginDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardFeedback", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardItemFeedback>();

            return result;
        }

        public async Task<List<CardCategory>> GetCardCategories()
        {
            var parameters = new SqlParameter[0];
            var dsSchedules = await ExecuteCommandAsync("GetCardCategories", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardCategory>();

            return result;
        }

        public async Task<int> InsertProcedureProfileCpt(int procedureProfileId, string cptCode, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("cpt_id", cptCode),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("InsertProcedureProfileCpt", parameters);

            return result;
        }

        public async Task<int> InsertProcedureProfileTrayInstrument(int procedureProfileId, int trayItemId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("tray_item_id", trayItemId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("InsertProcedureProfileTrayInstrument", parameters);

            return result;
        }

        public async Task<int> UpdateProcedureProfileItem(int procedureProfileId, int? itemId, int? instrumentId, string category, int quantity, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("item_id", itemId ?? (object)DBNull.Value),
                new SqlParameter("instrument_id", instrumentId ?? (object)DBNull.Value),
                new SqlParameter("category", category ?? (object)DBNull.Value),
                new SqlParameter("quantity", quantity),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("UpdateProcedureProfileItem", parameters);

            return result;
        }

        public async Task<int> UpdateProcedureProfileCard(int procedureProfileId, int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("UpdateProcedureProfileCard", parameters);

            return result;
        }

        public async Task<int> UpdateProcedureProfileTrayInstrument(int procedureProfileId, int instrumentId, int trayItemId, 
            int? categoryId, int quantity, string reason, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("tray_item_id", trayItemId),
                new SqlParameter("instrument_id", instrumentId),
                new SqlParameter("category_id", categoryId ?? (object)DBNull.Value),
                new SqlParameter("reason", reason ?? (object)DBNull.Value),
                new SqlParameter("quantity", quantity),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("UpdateProcedureProfileTrayInstrument", parameters);

            return result;
        }

        public async Task<int> DeleteProcedureProfileCpt(int procedureProfileId, string cptCode, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("cpt_id", cptCode),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("DeleteProcedureProfileCpt", parameters);

            return result;
        }

        public async Task<int> DeleteProcedureProfileItem(int procedureProfileId, int? itemId, int? instrumentId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("item_id", itemId ?? (object)DBNull.Value),
                new SqlParameter("instrument_id", instrumentId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("DeleteProcedureProfileItem", parameters);
                
            return result;
        }

        public async Task<int> DeleteProcedureProfileTrayInstrument(int procedureProfileId, int itemId, int trayItemId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("item_id", itemId),
                new SqlParameter("tray_item_id", trayItemId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("DeleteProcedureProfileTrayInstrument", parameters);
                
            return result;
        }

        public async Task<List<ProcedureProfile>> GetProcedureProfiles()
        {
            var parameters = new SqlParameter[]
            {
            };
            var dsSchedules = await ExecuteCommandAsync("GetProcedureProfiles", parameters);

            var results = dsSchedules.Tables[0].DataTableToList<ProcedureProfile>();
            var procedures = dsSchedules.Tables[1].DataTableToList<ProfileProcedure>();
            var specialties = dsSchedules.Tables[2].DataTableToList<ProfileSpecialty>();
            var cardCategories = dsSchedules.Tables[3].DataTableToList<ProfileCardCategory>();

            foreach (var result in results)
            {
                result.Procedures = procedures.Where(p => p.ProcedureProfileID == result.ProcedureProfileID).ToList();
                result.Specialties = specialties.Where(p => p.ProcedureProfileID == result.ProcedureProfileID).ToList();
                result.CardCategories = cardCategories.Where(p => p.ProcedureProfileID == result.ProcedureProfileID).ToList();
            }

            return results;
        }

        public async Task<ProcedureProfile> GetProcedureProfile(int procedureProfileId, List<int> locationId)
        {
            var locationXml = GetIdentitySummary(locationId);

            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("location_id", locationXml ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProcedureProfile", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ProcedureProfile>().First();
            
            result.Procedures = dsSchedules.Tables[1].DataTableToList<ProfileProcedure>();
            result.Items = dsSchedules.Tables[2].DataTableToList<ProfileItem>();
            result.Specialties = dsSchedules.Tables[3].DataTableToList<ProfileSpecialty>();
            result.TrayItems = dsSchedules.Tables[4].DataTableToList<ProfileItem>();
            result.Cards = dsSchedules.Tables[5].DataTableToList<ProfileCard>();
            result.Trays = dsSchedules.Tables[6].DataTableToList<ProfileTray>();
            result.CardCategories = dsSchedules.Tables[7].DataTableToList<ProfileCardCategory>();

            var comparableItems = dsSchedules.Tables[8].DataTableToList<ComparableItem>();
            var comparableInstruments = dsSchedules.Tables[9].DataTableToList<ComparableInstrument>();

            result.TrayUsage = dsSchedules.Tables[10].DataTableToList<ProcedureProfileTrayUsage>();
            result.AssociatedTrays = dsSchedules.Tables[11].DataTableToList<ProcedureProfileAssociatedTray>();

            foreach (var item in result.Items)
            {
                item.ComparableItems = comparableItems.Where(c => c.ItemID == item.ItemID).ToList();
            }
            foreach (var item in result.TrayItems)
            {
                item.ComparableInstruments = comparableInstruments.Where(c => c.InstrumentID == item.ItemID && c.TrayItemID == item.TrayID).ToList();
            }
            
            result.Cards.ForEach(c => c.ProfileCount = result.NbrInstruments);
            result.Trays.ForEach(t => t.ProfileCount = result.NbrInstruments);

            return result;
        }

        public async Task<int> UpdateProcedureProfile(int? procedureProfileId, string profileName, List<int> locationFilter, List<int> cardCategoryId, List<int> specialtyId,
            int providerId, int locationId)
        {
            var locationXml = GetIdentitySummary(locationFilter);
            var cardCategoryXml = GetIdentitySummary(cardCategoryId);
            var specialtyXml = GetIdentitySummary(specialtyId);
            
            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId ?? (object)DBNull.Value),
                new SqlParameter("profile_name", profileName),
                new SqlParameter("location_filter_id", locationXml ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryXml ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("UpdateProcedureProfile", parameters);

            var results = dsSchedules.Tables[0].DataTableToList<InsertionResult>();
            
            return results.First().Identifier;
        }

        public async Task<int> UpdateProcedureProfileDashboard(int procedureProfileId, List<int> locationFilter, List<int> cards, List<int> trays, List<int> proposed,
            int providerId, int locationId)
        {
            var locationXml = GetIdentitySummary(locationFilter);
            var cardXml = GetIdentitySummary(cards);
            var trayXml = GetIdentitySummary(trays);
            var proposedXml = GetIdentitySummary(proposed);

            var parameters = new[]
            {
                new SqlParameter("procedure_profile_id", procedureProfileId),
                new SqlParameter("location_filter_id", locationXml ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardXml ?? (object)DBNull.Value),
                new SqlParameter("tray_item_id", trayXml ?? (object)DBNull.Value),
                new SqlParameter("tray_proposal_id", proposedXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("UpdateProcedureProfileDashboard", parameters);

            return result;
        }

        public async Task<List<TrayGroup>> GetTrayGroups(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetTrayGroups", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayGroup>();
            var trays = dsSchedules.Tables[1].DataTableToList<TrayGroupTray>();

            foreach (var trayGroup in result)
            {
                trayGroup.Trays = trays.Where(t => t.TrayGroupID == trayGroup.TrayGroupID).ToList();
            }

            return result;
        }

        public async Task<List<TrayRationalizationCardCategory>> GetProposedTrayCardCategories(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayCardCategories", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayRationalizationCardCategory>();
            var surgeons = dsSchedules.Tables[1].DataTableToList<SurgeonCardCategory>();

            foreach (var trayGroup in result)
            {
                trayGroup.Surgeons = surgeons.Where(t => t.CardCategoryID == trayGroup.CardCategoryID).ToList();
            }

            return result;
        }

        public async Task<int> UpdateCardCategories(string hierarchyLevel, int cardCategoryId, List<int> cards, int providerId, int locationId)
        {
            var cardData = GetIdentitySummary(cards);

            var parameters = new[]
            {
                new SqlParameter("hierarchy_level", hierarchyLevel),
                new SqlParameter("card_category_id", cardCategoryId),
                new SqlParameter("cards", cardData),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("UpdateCardCategories", parameters);

            return result;
        }

        public async Task<int> DeleteCardCategoryXRef(int cardCategoryXRefId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_category_xref_id", cardCategoryXRefId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("DeleteCardCategoryXRef", parameters);

            return result;
        }

        public async Task<int> UpdateCardCategoryXRef(int cardId, List<CardCategoryXRef> categories, int providerId, int locationId)
        {
            var categoryData = SummarizeCategories(categories);

            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("card_category_id", categoryData),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("UpdateCardCategoryXRef", parameters);

            return result;
        }

        private string SummarizeCategories(List<CardCategoryXRef> categories)
        {
            if (categories == null || !categories.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var category in categories)
            {
                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, category.HierarchyLevel);
                AddColumn(doc, row, category.CardCategoryID);
            }

            return table.OuterXml;
        }

        public async Task<int> UpdateCardFeedback(int feedbackId, bool response, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("feedback_id", feedbackId),
                new SqlParameter("response", response),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateCardFeedback", parameters);

            return result;
        }

        public async Task<List<SurgeryCard>> GetSurgeryCardList(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryCardList", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryCard>();

            var currentCard = result.FirstOrDefault(r => r.CurrentCard);
            if (currentCard != null)
            {
                result.ForEach(r => r.CurrentCostDelta = (currentCard.Cost - r.Cost));
            }

            return result;
        }

        public async Task<List<SurgeryFlow>> GetSurgeryFlowList(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryFlowList", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryFlow>();

            var currentCard = result.FirstOrDefault(r => r.CurrentFlow);
            if (currentCard != null)
            {
                result.ForEach(r => r.CurrentCostDelta = (currentCard.TimeCost - r.TimeCost));
            }

            return result;
        }

        public async Task<List<SurgeryUser>> GetCardUsers(int cardId, int providerId, int locationId, int? typeId)
        {
            var command = typeId == null ? "GetCardUsers" : "GetCardUsersType";

            var parameters = typeId == null ?
            new[] {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            } : 
            new[] {
                    new SqlParameter("card_id", cardId),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId),
                    new SqlParameter("type_id", typeId)
                };
            var dsSchedules = await ExecuteCommandAsync(command, parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryUser>();

            return result;

        }

        public async Task<User> GetUser(int providerId, int locationId, Guid userAuthId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("user_auth_id", userAuthId),
                new SqlParameter("user_id", DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetUser", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<User>().FirstOrDefault();

            return result;
        }

        public async Task<User> GetUser(int providerId, int locationId, int userId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("user_auth_id", DBNull.Value),
                new SqlParameter("user_id", userId),
            };
            var dsUsers = await ExecuteCommandAsync("GetUser", dsParameters);

            var result = dsUsers.Tables[0].DataTableToList<User>().FirstOrDefault();

            return result;
        }

        public async Task<List<User>> SearchUsers(string searchString, int? roleId, int? specialtyId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("role_id", roleId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("search", searchString ?? (object)DBNull.Value),
            };
            var dsSchedules = await ExecuteCommandAsync("SearchUsers", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<User>();

            return result;
        }

        public async Task<List<User>> GetSurgeryUsers(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryUsers", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<User>();

            return result;
        }

        public async Task<List<InternalUser>> SearchInternalUsers(string searchString, int? roleId, int? specialtyId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("role_id", roleId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("search", searchString ?? (object)DBNull.Value),
            };
            var dsSchedules = await ExecuteCommandAsync("SearchInternalUsers", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<InternalUser>();

            return result;
        }

        public async Task<List<Role>> GetRoles(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetRoles", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<Role>();

            return result;
        }

        public async Task<List<Role>> GetInternalRoles(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetInternalRoles", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<Role>();

            return result;
        }
        public async Task<List<Vendor>> GetVendors(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetVendors", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<Vendor>();

            return result;
        }
        public async Task<int> InitializeLocation(int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("InitializeLocation", dsParameters);

            return result;
        }

        public async Task<UserSecurity> GetSecureUser(Guid? userAuthId, int? userId = null, string email = null)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_auth_id", userAuthId ?? (object)DBNull.Value),
                new SqlParameter("email", email ?? (object)DBNull.Value),
                new SqlParameter("user_id", userId ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetUserSecurity", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<UserSecurity>();

            return result.FirstOrDefault();
        }

        public async Task<List<OpFlowProvider>> GetOpFlowSetup()
        {
            var dsParameters = new SqlParameter[0];
            var dsSchedules = await ExecuteCommandAsync("GetOpFlowSetup", dsParameters);


            var providers = dsSchedules.Tables[0].DataTableToList<OpFlowProvider>();
            var locations = dsSchedules.Tables[1].DataTableToList<OpFlowLocation>();

            foreach (var location in locations)
            {
                var provider = providers.FirstOrDefault(p => p.ProviderID == location.ProviderID);
                provider?.Locations.Add(location);
            }

            return providers;
        }

        public async Task<int> UpdateUserLocation(Guid userAuthId, int locationId)
        {
            var dsParameters = new []
            {
                new SqlParameter("user_auth_id", userAuthId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateUserLocation", dsParameters);

            return result;
        }

        public async Task<int> CheckInUser(User user, int surgeryId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_id", user.UserID),
                new SqlParameter("provider_id", user.ProviderID),
                new SqlParameter("location_id", user.LocationID),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("role_id", user.RoleID)
            };
            return await ExecuteNonQueryAsync("CheckinUserToCase", dsParameters);
        }

        public async Task<int> CheckOutUser(User user, int surgeryId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_id", user.UserID),
                new SqlParameter("provider_id", user.ProviderID),
                new SqlParameter("location_id", user.LocationID),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("role_id", user.RoleID)
            };
            return await ExecuteNonQueryAsync("CheckoutOfCase", dsParameters);
        }

        public async Task<int> WorkupReviewed(User user, int surgeryId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_id", user.UserID),
                new SqlParameter("provider_id", user.ProviderID),
                new SqlParameter("location_id", user.LocationID),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("role_id", user.RoleID)
            };
            return await ExecuteNonQueryAsync("UserSurgeryWorkupReviewed", dsParameters);
        }

        public async Task<int> CreateUser(Guid userAuthId, int? roleId, int? specialtyId, string firstName, string lastName,
            string email, string cellPhone, string initials, string title, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_auth_id", userAuthId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("role_id", roleId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("first_name", firstName ?? (object)DBNull.Value),
                new SqlParameter("last_name", lastName ?? (object)DBNull.Value),
                new SqlParameter("email", email ?? (object)DBNull.Value),
                new SqlParameter("cell_phone", cellPhone ?? (object)DBNull.Value),
                new SqlParameter("initials", initials ?? (object)DBNull.Value),
                new SqlParameter("title", title ?? (object)DBNull.Value)
            };
            var dsResult = await ExecuteCommandAsync("InsertUser", dsParameters);

            var result = dsResult.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public async Task<int> UpdateUser(int userId, int? roleId, int? vendorId, int? specialtyId, string firstName, string lastName,
            string email, string cellPhone, string initials, string title, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("role_id", roleId ?? (object)DBNull.Value),
                new SqlParameter("vendor_id", vendorId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("first_name", firstName ?? (object)DBNull.Value),
                new SqlParameter("last_name", lastName ?? (object)DBNull.Value),
                new SqlParameter("email", email ?? (object)DBNull.Value),
                new SqlParameter("cell_phone", cellPhone ?? (object)DBNull.Value),
                new SqlParameter("initials", initials ?? (object)DBNull.Value),
                new SqlParameter("title", title ?? (object)DBNull.Value)
            };
            return await ExecuteNonQueryAsync("UpdateUser", dsParameters);
        }

        public async Task<int> DeleteUser(int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("user_id", userId)
            };
            return await ExecuteNonQueryAsync("DeleteUser", dsParameters);
        }

        public async Task<int> AddSurgerySmartPhrase(int surgeryId, int smartPhraseId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("smart_phrase_id", smartPhraseId)
            };
            return await ExecuteNonQueryAsync("InsertSurgeryPhrase", dsParameters);
        }

        public async Task<int> AddFlowSmartPhrase(int flowId, int smartPhraseId, int? stepId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_id", flowId),
                new SqlParameter("step_id", stepId ?? (object)DBNull.Value),
                new SqlParameter("smart_phrase_id", smartPhraseId)
            };
            return await ExecuteNonQueryAsync("InsertFlowPhrase", dsParameters);
        }

        public async Task<int> NewSmartPhrase(string phrase, int categoryId, int stepId, int roleId, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("category_id", categoryId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("user_id", userId),
                new SqlParameter("phrase", phrase)
            };
            var dsResult = await ExecuteCommandAsync("InsertPhrase", dsParameters);

            var result = dsResult.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public async Task<int> EditSmartPhrase(int smartPhraseId, string phrase, 
            int categoryId, int userId, int stepId, int roleId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("smart_phrase_id", smartPhraseId),
                new SqlParameter("category_id", categoryId),
                new SqlParameter("user_id", userId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("phrase", phrase)
            };
            return await ExecuteNonQueryAsync("UpdateSmartPhrase", dsParameters);
        }

        public async Task<int> DeleteSmartPhrase(int smartPhraseId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("smart_phrase_id", smartPhraseId)
            };
            return await ExecuteNonQueryAsync("DeleteSmartPhrase", dsParameters);
        }

        public async Task<int> NewFlowImage(int flowId, int stepId, int roleId, string comment, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("image_comment", comment ?? (object)DBNull.Value)
            };
            var insert = await ExecuteCommandAsync("InsertFlowImage", dsParameters);
            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.First().Identifier;
        }

        public async Task<int> UpdateFlowImage(int flowImageId, string comments, int stepId, int roleId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_image_id", flowImageId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("comment", comments ?? (object)DBNull.Value)
            };
            return await ExecuteNonQueryAsync("UpdateFlowImage", dsParameters);
        }

        public async Task<int> DeleteFlowImage(int flowImageId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_image_id", flowImageId)
            };
            return await ExecuteNonQueryAsync("DeleteFlowImage", dsParameters);
        }

        public async Task<int> NewSurgeryImage(int surgeryId, int stepId, int roleId, string comment, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("image_comment", comment)
            };
            var insert = await ExecuteCommandAsync("InsertSurgeryImage", dsParameters);
            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.First().Identifier;
        }

        public async Task<int> UpdateSurgeryImage(int surgeryImageId, string comments, int stepId, int roleId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_image_id", surgeryImageId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("comment", comments ?? (object)DBNull.Value)
            };
            return await ExecuteNonQueryAsync("UpdateSurgeryImage", dsParameters);
        }

        public async Task<int> DeleteSurgeryImage(int surgeryImageId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_image_id", surgeryImageId)
            };
            return await ExecuteNonQueryAsync("DeleteSurgeryImage", dsParameters);
        }

        public async Task<int> AddFlowFeedback(int flowId, string feedback, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_id", flowId),
                new SqlParameter("user_id", userId),
                new SqlParameter("feedback", feedback)
            };
            return await ExecuteNonQueryAsync("InsertFlowFeedback", dsParameters);
        }

        public async Task<int> DeleteFlowFeedback(int flowFeedbackId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_feedback_id", flowFeedbackId)
            };
            return await ExecuteNonQueryAsync("DeleteFlowFeedback", dsParameters);
        }

        public async Task<int> UpdateFlowPhrase(int flowId, int smartPhraseId, string comments, int stepId, int roleId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_id", flowId),
                new SqlParameter("smart_phrase_id", smartPhraseId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("comment", comments ?? (object)DBNull.Value)
            };
            return await ExecuteNonQueryAsync("UpdateFlowPhrase", dsParameters);
        }

        public async Task<int> DeleteFlowPhrase(int flowId, int smartPhraseId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_id", flowId),
                new SqlParameter("smart_phrase_id", smartPhraseId)
            };
            return await ExecuteNonQueryAsync("DeleteFlowPhrase", dsParameters);
        }

        public async Task<int> UpdateSurgeryPhrase(int surgeryId, int surgeryPhraseId, string comments, int stepId, int roleId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("surgery_phrase_id", surgeryPhraseId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("comment", comments ?? (object)DBNull.Value)
            };
            return await ExecuteNonQueryAsync("UpdateSurgeryPhrase", dsParameters);
        }

        public async Task<int> DeleteSurgeryPhrase(int surgeryId, int smartPhraseId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("smart_phrase_id", smartPhraseId)
            };
            return await ExecuteNonQueryAsync("DeleteSurgeryPhrase", dsParameters);
        }

        public async Task<int> NewSurgeonNote(string phrase, int flowId, int stepId, int roleId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_id", flowId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("phrase", phrase)
            };
            return await ExecuteNonQueryAsync("InsertFlowSurgeonNote", dsParameters);
        }

        public async Task<int> UpdateSurgeonNote(int surgeonNoteId, string comments, int stepId, int roleId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgeon_note_id", surgeonNoteId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("comment", comments ?? (object)DBNull.Value)
            };
            return await ExecuteNonQueryAsync("UpdateSurgeonNote", dsParameters);
        }

        public async Task<int> DeleteSurgeonNote(int surgeonNoteId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgeon_note_id", surgeonNoteId)
            };
            return await ExecuteNonQueryAsync("DeleteSurgeonNote", dsParameters);
        }

        public async Task<int> SurgeryPhraseDebrief(int surgeryPhraseId, int stepId, int roleId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_phrase_id", surgeryPhraseId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId)
            };
            return await ExecuteNonQueryAsync("SurgeryPhraseDebrief", dsParameters);
        }


        public async Task<int> SurgeryUpdateCaseNotes(int surgeryId, string caseNotes, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("case_notes", caseNotes)
            };
            return await ExecuteNonQueryAsync("UpdateSurgeryCaseNotes", dsParameters);
        }
        public async Task<int> AssignCardToCase(int cardId, int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("card_id", cardId)
            };
            return await ExecuteNonQueryAsync("AssignCardToCase", dsParameters);
        }

        public async Task<int> AssignFlowToCase(int flowId, int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("flow_id", flowId)
            };
            return await ExecuteNonQueryAsync("AssignFlowToCase", dsParameters);
        }

        public async Task<int> AssignRoomSetupToCase(int roomSetupId, int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("room_setup_id", roomSetupId)
            };
            return await ExecuteNonQueryAsync("AssignRoomSetupToCase", dsParameters);
        }

        public async Task<int> AssignFlowToCard(int flowId, int cardId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("flow_id", flowId)
            };
            return await ExecuteNonQueryAsync("AssignFlowToCard", dsParameters);
        }

        public async Task<int> AssignRoomSetupToCard(int roomSetupId, int cardId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("room_setup_id", roomSetupId)
            };
            return await ExecuteNonQueryAsync("AssignRoomSetupToCard", dsParameters);
        }

        public async Task<int> AssignUserToCard(int cardId, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("user_id", userId)
            };
            return await ExecuteNonQueryAsync("InsertCardUser", dsParameters);
        }

        public async Task<int> RemoveUserFromCard(int cardId, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("user_id", userId)
            };
            return await ExecuteNonQueryAsync("DeleteCardUser", dsParameters);
        }

        public async Task<int> DeleteCardItem(int cardId, int itemId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("item_id", itemId)
            };
            return await ExecuteNonQueryAsync("DeleteCardItem", dsParameters);
        }

        public async Task<int> UpdateCardItem(int cardId, int itemId, int qtyOpen, int qtyHold, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("item_id", itemId),
                new SqlParameter("qty_open", qtyOpen),
                new SqlParameter("qty_hold", qtyHold)
            };
            return await ExecuteNonQueryAsync("UpdateCardItem", dsParameters);
        }

        public async Task<int> UpdateCardProcedure(int cardId, int procedureId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("procedure_id", procedureId)
            };
            return await ExecuteNonQueryAsync("UpdateCardProcedure", dsParameters);
        }

        public async Task<int> DeleteCardProcedure(int cardId, int procedureId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("procedure_id", procedureId)
            };
            return await ExecuteNonQueryAsync("DeleteCardProcedure", dsParameters);
        }

        public async Task<int> InsertCard(string description, int ownerUserId, int? specialtyId, int? procedureId, int? templateFlowId, int? templateRoomId, int? bundleId, string bundleFlag,
            int? cardCategoryId, bool? defaultFlag, bool? specialtyDefaultFlag, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("description", description ?? (object)DBNull.Value),
                new SqlParameter("owner_user_id", ownerUserId),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("procedure_id", procedureId ?? (object)DBNull.Value),
                new SqlParameter("template_flow_id", templateFlowId ?? (object)DBNull.Value),
                new SqlParameter("template_room_setup_id", templateRoomId ?? (object)DBNull.Value),
                new SqlParameter("bundle_id", bundleId ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryId ?? (object)DBNull.Value),
                new SqlParameter("bundle_flag", bundleFlag ?? (object)DBNull.Value),
                new SqlParameter("default_flag", defaultFlag ?? (object)DBNull.Value),
                new SqlParameter("specialty_default_flag", specialtyDefaultFlag ?? (object)DBNull.Value)
            };
            var insert = await ExecuteCommandAsync("NewCard", dsParameters);
            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.First().Identifier;
        }

        public async Task<int?> ParseCardCategory(string cardCategory, int providerId, int locationId)
        {
            if (string.IsNullOrEmpty(cardCategory) || cardCategory == "undefined")
                return null;

            if (int.TryParse(cardCategory, out var cardCategoryId))
                return cardCategoryId;

            var dsParameters = new[]
            {
                new SqlParameter("card_category", cardCategory),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var insert = await ExecuteCommandAsync("InsertCardCategory", dsParameters);
            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.First().Identifier;
        }

        public async Task<int> InsertCardItemFromStage(int cardId, int providerId, int locationId, string procedure, string surgeon)
        {
            var dsParameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("procedure", procedure),
                new SqlParameter("surgeon", surgeon)
            };

            var insert = await ExecuteNonQueryAsync("InsertCardItemFromStage", dsParameters);
            return insert;
        }

        public async Task<int> UpdateCard(int cardId, string description, int? specialtyId, int? procedureId, int? templateFlowId, int? templateRoomId, int? bundleId, string bundleFlag,
            int? cardCategoryId, bool? defaultFlag, bool? specialtyDefaultFlag, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("description", description ?? (object)DBNull.Value),
                new SqlParameter("procedure_id", procedureId ?? (object)DBNull.Value),
                new SqlParameter("template_flow_id", templateFlowId ?? (object)DBNull.Value),
                new SqlParameter("template_room_setup_id", templateRoomId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("bundle_id", bundleId ?? (object)DBNull.Value),
                new SqlParameter("card_category_id", cardCategoryId ?? (object)DBNull.Value),
                new SqlParameter("bundle_flag", bundleFlag ?? (object)DBNull.Value),
                new SqlParameter("default_flag", defaultFlag ?? (object)DBNull.Value),
                new SqlParameter("specialty_default_flag", specialtyDefaultFlag ?? (object)DBNull.Value)
            };
            return await ExecuteNonQueryAsync("UpdateCard", dsParameters);
        }

        public async Task<int> InitializeCard(int cardId, int providerId, int locationId)
        {

            var dsParameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var insert = await ExecuteNonQueryAsync("InitializeCard", dsParameters);
            return insert;
        }


        public async Task<int> DeleteCard(int cardId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId)
            };
            return await ExecuteNonQueryAsync("DeleteCard", dsParameters);
        }

        public async Task<int> UpdateCardQuantity(int cardId, CardQuantityEdit quantity, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("item_id", quantity.ItemID),
                new SqlParameter("open_qty", quantity.OpenQty),
                new SqlParameter("hold_qty", quantity.HoldQty),
                new SqlParameter("item_status", "D")
            };
            return await ExecuteNonQueryAsync("UpdateCardItemQty", dsParameters);
        }

        public async Task<int> UpdateCardQuantityRequest(int cardId, CardQuantityEdit quantity, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("item_id", quantity.ItemID),
                new SqlParameter("qty_open", quantity.OpenQty ?? (object)DBNull.Value),
                new SqlParameter("qty_hold", quantity.HoldQty ?? (object)DBNull.Value),
                new SqlParameter("delete_item", quantity.DeleteItem)
            };
            return await ExecuteNonQueryAsync("UpdateCardItemQtyRequest", dsParameters);
        }

        public async Task<int> UpdateSurgeryItemQuantity(int surgeryId, CardQuantityEdit quantity, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("item_id", quantity.ItemID),
                new SqlParameter("qty_open", quantity.OpenQty ?? (object)DBNull.Value),
                new SqlParameter("qty_hold", quantity.HoldQty ?? (object)DBNull.Value),
                new SqlParameter("delete_item", quantity.DeleteItem)
            };
            return await ExecuteNonQueryAsync("UpdateSurgeryCardItemQty", dsParameters);
        }

        public async Task<int> CreateSurgery(SurgeryPost surgery, int patientId, int caseId, 
            int? defaultCardId, int? defaultFlowId, int? defaultRoomId,
            int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("vendor_location_id", surgery.VendorLocationID ?? (object)DBNull.Value),
                new SqlParameter("patient_id", patientId),
                new SqlParameter("user_id", surgery.SurgeonUserID ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", surgery.SpecialtyID ?? (object)DBNull.Value),
                new SqlParameter("bundle_id", surgery.BundleID ?? (object)DBNull.Value),
                new SqlParameter("case_id", caseId),
                new SqlParameter("room_id", surgery.RoomID ?? (object)DBNull.Value),
                new SqlParameter("schedule_date", surgery.ScheduleDate),
                new SqlParameter("schedule_time", surgery.ScheduleDate),
                new SqlParameter("default_card_id", defaultCardId ?? (object)DBNull.Value),
                new SqlParameter("default_flow_id", defaultFlowId ?? (object)DBNull.Value),
                new SqlParameter("default_room_id", defaultRoomId ?? (object)DBNull.Value),
                new SqlParameter("cpt_codes", surgery.CptCode ?? (object)DBNull.Value),
                new SqlParameter("laterality_id", surgery.LateralityID ?? (object)DBNull.Value),
                new SqlParameter("case_profile_id", surgery.CaseProfileID ?? (object)DBNull.Value)
            };
            var insert = await ExecuteCommandAsync("InsertSurgery", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public async Task<int> AddCustomSurgeryItem(int surgeryId, int itemId, int quantity, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("item_id", itemId),
                new SqlParameter("quantity", quantity)
            };
            return await ExecuteNonQueryAsync("InsertCustomSurgeryItem", dsParameters);
        }
        public async Task<int> AddSurgeryProposedTray(int surgeryId, int trayProposalId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("tray_proposal_id", trayProposalId)
            };
            return await ExecuteNonQueryAsync("InsertSurgeryProposedTray", dsParameters);
        }
        public async Task<int> AddSurgeryProposedTrayGroup(int surgeryId, int trayGroupId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("tray_group_id", trayGroupId)
            };
            return await ExecuteNonQueryAsync("InsertSurgeryProposedTrayGroup", dsParameters);
        }

        public async Task<int> AddSurgeryUser(int surgeryId, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("user_id", userId)
            };
            return await ExecuteNonQueryAsync("InsertSurgeryUser", dsParameters);
        }

        public async Task<int> DeleteSurgeryUser(int surgeryId, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("user_id", userId)
            };
            return await ExecuteNonQueryAsync("DeleteSurgeryUser", dsParameters);
        }

        public async Task<int> AddSurgeryProcedure(int surgeryId, int procedureId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("procedure_id", procedureId)
            };
            return await ExecuteNonQueryAsync("InsertSurgeryProcedure", dsParameters);
        }

        public async Task<int> UpdateSurgeryProcedure(int surgeryId, string cptCode, string performedFlag, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("cpt_code", cptCode),
                new SqlParameter("performed_flag", performedFlag)
            };
            return await ExecuteNonQueryAsync("UpdateSurgeryProcedure", dsParameters);
        }

        public async Task<int> DeleteSurgeryProcedure(int surgeryId, string cptCode, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("cpt_code", cptCode)
            };
            return await ExecuteNonQueryAsync("DeleteSurgeryProcedure", dsParameters);
        }

        public async Task<int> UpdateSurgeryHeaderCounts(int surgeryId, int sharpCount, int needleCount, int lapCount, int specimenCount, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("sharp_count", sharpCount),
                new SqlParameter("needle_count", needleCount),
                new SqlParameter("lap_count", lapCount),
                new SqlParameter("specimen_count", specimenCount)
            };
            return await ExecuteNonQueryAsync("UpdateSurgeryHeaderCounts", dsParameters);
        }

        public async Task<int> UpdateStaffChange(int surgeryId, string staffChange, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("staff_change", staffChange)
            };
            return await ExecuteNonQueryAsync("UpdateSurgeryStaffChange", dsParameters);
        }

        public async Task<int> AddCustomSurgeryTrayItem(int surgeryId, int trayId, int itemId, int quantity, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("tray_id", trayId),
                new SqlParameter("instrument_id", itemId),
                new SqlParameter("quantity", quantity)
            };
            return await ExecuteNonQueryAsync("InsertCustomSurgeryTrayInstrument", dsParameters);
        }

        public async Task<int> AddCustomSurgeryTray(int surgeryId, int itemId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("tray_collection_id", itemId)
            };
            return await ExecuteNonQueryAsync("InsertCustomSurgeryTray", dsParameters);
        }
        

        public async Task<int> UpdateSurgeryCount(int surgeryId, List<SurgeryCountItemPost> itemUsage, string countComments, string surgeryType,
            int providerId, int locationId)
        {
            var usageSummary = GetUsageSummary(itemUsage);
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("count_comments", countComments ?? (object)DBNull.Value),
                new SqlParameter("surgery_type", surgeryType ?? (object)DBNull.Value),
                new SqlParameter("usage_summary", usageSummary ?? (object)DBNull.Value)
            };
            var update = await ExecuteNonQueryAsync("UpdateSurgeryCount", dsParameters);

            return update;
        }
        public async Task<int> UpdateSurgeryInstrumentCount(int surgeryId, List<SurgeryCountItemPost> instrumentUsage, int providerId, int locationId)
        {
            var usageSummary = GetUsageSummary(instrumentUsage);
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("usage_summary", usageSummary ?? (object)DBNull.Value)
            };
            var update = await ExecuteNonQueryAsync("UpdateSurgeryInstrumentCount", dsParameters);

            return update;
        }
        public async Task<int> UpdateSurgeryProposedCount(int surgeryId, List<SurgeryCountItemPost> proposedUsage, int providerId, int locationId)
        {
            var usageSummary = GetUsageSummary(proposedUsage);
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("usage_summary", usageSummary ?? (object)DBNull.Value)
            };
            var update = await ExecuteNonQueryAsync("UpdateSurgeryProposedCount", dsParameters);

            return update;
        }
        public async Task<int> UpdateSurgerySutureCount(int surgeryId, List<SurgeryCountSuturePost> sutureUsage, List<int> deletedSutures, int providerId, int locationId)
        {
            var usageSummary = GetSutureSummary(sutureUsage);
            var deletedSutureXml = GetIdentitySummary(deletedSutures);

            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("usage_summary", usageSummary ?? (object)DBNull.Value),
                new SqlParameter("deleted_sutures", deletedSutureXml ?? (object)DBNull.Value)
            };
            var update = await ExecuteNonQueryAsync("UpdateSurgerySutureCount", dsParameters);

            return update;
        }

        private string GetUsageSummary(List<SurgeryCountItemPost> countData)
        {
            if (!countData.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var customItem in countData)
            {
                // prevent adding invalid data
                if (customItem.ItemID <= 0)
                    continue;

                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, customItem.ItemID);
                AddColumn(doc, row, customItem.TrayID);
                AddColumn(doc, row, "U");
                AddColumn(doc, row, customItem.Usage);
                AddColumn(doc, row, customItem.UsageType);
                AddColumn(doc, row, customItem.RoleID);
                AddColumn(doc, row, customItem.Setup);
                AddColumn(doc, row, customItem.SetupAdded);
                AddColumn(doc, row, customItem.Notes);
            }

            return table.OuterXml;
        }

        private string GetSutureSummary(List<SurgeryCountSuturePost> countData)
        {
            if (!countData.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var customItem in countData)
            {
                // prevent adding invalid data
                if (customItem.ItemID <= 0)
                    continue;

                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, customItem.SutureID);
                AddColumn(doc, row, customItem.ItemID);
                AddColumn(doc, row, customItem.Manufacturer);
                AddColumn(doc, row, customItem.Size);
                AddColumn(doc, row, customItem.PackSize);
                AddColumn(doc, row, customItem.SetupAdded);
                AddColumn(doc, row, customItem.Usage);
                AddColumn(doc, row, customItem.Notes);
            }

            return table.OuterXml;
        }

        public async Task<int> UpdateSurgeryQuestionAnswers(int surgeryId, List<TrayQuestion> answers, int providerId, int locationId)
        {
            var answerSummary = GetAnswerSummary(answers);
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("answer_summary", answerSummary ?? (object)DBNull.Value)
            };
            var update = await ExecuteNonQueryAsync("UpdateSurgeryQuestionAnswer", dsParameters);

            return update;
        }

        public async Task<int> UpdateSurgeryCPTs(int surgeryId, List<string> surgeryCpts, int providerId, int locationId)
        {
            var codes = await GetSurgeryCPTCodes(surgeryId, providerId, locationId);

            foreach (var surgeryCpt in surgeryCpts)
            {
                var existing = codes.FirstOrDefault(c => c.CptCode == surgeryCpt);
                if (existing == null && !string.IsNullOrEmpty(surgeryCpt))
                    await InsertSurgeryCPTCode(surgeryId, surgeryCpt, providerId, locationId);
            }

            foreach (var code in codes)
            {
                var desired = surgeryCpts.FirstOrDefault(c => code.CptCode == c);
                if (desired == null)
                    await DeleteSurgeryCPTCode(surgeryId, code.CptCode, providerId, locationId);
            }

            return surgeryCpts.Count;
        }

        public async Task<List<SurgeryCPTCode>> GetKnownCPTCodes(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsResults = await ExecuteCommandAsync("GetKnownCPTCodes", dsParameters);
            var codes = dsResults.Tables[0].DataTableToList<SurgeryCPTCode>();

            return codes;
        }

        public async Task<List<SurgeryCPTCode>> GetSurgeryCPTCodes(int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsResults = await ExecuteCommandAsync("GetSurgeryCPTCodes", dsParameters);
            var codes = dsResults.Tables[0].DataTableToList<SurgeryCPTCode>();
            
            return codes;
        }

        public async Task<int> InsertSurgeryCPTCode(int surgeryId, string cptCode, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("cpt_code", cptCode),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("AssignCPTCodestoSurgery", dsParameters);
            
            return result;
        }

        public async Task<int> DeleteSurgeryCPTCode(int surgeryId, string cptCode, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("cpt_code", cptCode)
            };
            var result = await ExecuteNonQueryAsync("DeleteSurgeryProcedure", dsParameters);
            
            return result;
        }

        private string GetAnswerSummary(List<TrayQuestion> answers)
        {
            if (!answers.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var answer in answers)
            {
                // prevent adding invalid data
                if (answer.QuestionID <= 0)
                    continue;

                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, answer.QuestionID);
                AddColumn(doc, row, answer.Answer);
            }

            return table.OuterXml;
        }

        public async Task<int> StartSurgery(int surgeryId, int providerId, int locationId, DateTime startTime)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("start_time", startTime)
            };
            var update = await ExecuteNonQueryAsync("StartSurgery", dsParameters);

            return update;
        }

        /// <summary>
        /// Advance surgery to next step
        /// </summary>
        /// <param name="surgeryId"></param>
        /// <param name="providerId"></param>
        /// <param name="locationId"></param>
        /// <param name="startTime"></param>
        /// <returns>Current flow step after advancing</returns>
        public async Task<FlowStepResult> SurgeryMoveNextStep(int surgeryId, int providerId, int locationId, DateTime startTime)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("step_time", startTime)
            };

            var update = await ExecuteCommandAsync("UpdateSurgeryFlowTimings", dsParameters);
            var flowSteps = update.Tables[0].DataTableToList<FlowStepResult>();
            var flowStep = flowSteps.FirstOrDefault();

            return flowStep;
        }

        public async Task<int> SurgeryToggleDelay(int surgeryId, int providerId, int locationId, DateTime? startTime, DateTime? endTime, int? delayReasonId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("delay_start_time", startTime ?? (object)DBNull.Value),
                new SqlParameter("delay_end_time", endTime?? (object)DBNull.Value),
                new SqlParameter("delay_reason_id", delayReasonId ?? (object)DBNull.Value)
            };
            var update = await ExecuteNonQueryAsync("UpdateSurgeryDelays", dsParameters);

            return update;
        }

        public async Task<int> SurgeryToggleDelayCustom(int surgeryId, int providerId, int locationId, DateTime? startTime, DateTime? endTime, string customReason)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("delay_start_time", startTime ?? (object)DBNull.Value),
                new SqlParameter("delay_end_time", endTime?? (object)DBNull.Value),
                new SqlParameter("custom_reason", customReason ?? (object)DBNull.Value)
            };
            var dataSet = await ExecuteCommandAsync("UpdateSurgeryDelayCustom", dsParameters);

            var result = dataSet.Tables[0].DataTableToList<InsertionResult>().First().Identifier;

            return result;
        }

        public async Task<int> SurgeryReviewComplete(int surgeryId, DateTime reviewComplete, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("surgery_review", reviewComplete),
                new SqlParameter("user_id", userId)
            };
            var update = await ExecuteNonQueryAsync("UpdateSurgeryUserReview", dsParameters);

            return update;
        }

        public async Task<int> SurgeryEditProperties(int surgeryId, int roomId, DateTime surgeryScheduleDate, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("room_id", roomId),
                new SqlParameter("schedule_date", surgeryScheduleDate.Date),
                new SqlParameter("schedule_time", surgeryScheduleDate.TimeOfDay)
            };
            var update = await ExecuteNonQueryAsync("UpdateSurgeryProperties", dsParameters);

            return update;
        }

        public async Task<int> CreateCase(int patientId, int? userId, int? specialtyId, int providerId, int locationId, string caseNbr)
        {
            var dsParameters = new[]
            {
                new SqlParameter("patient_id", patientId),
                new SqlParameter("user_id", userId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("case_nbr", caseNbr)
            };
            var insert = await ExecuteCommandAsync("NewCase", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public async Task<List<Room>> GetRooms(int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetRooms", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<Room>();

            return result;
        }

        public async Task<List<RoomType>> GetRoomTypes(int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetRoomTypes", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<RoomType>();

            return result;
        }

        public async Task<List<RoomGroup>> GetRoomGroups(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetRoomGroups", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<RoomGroup>();

            return result;
        }
        public async Task<RoomSetup> GetRoomSetup(int roomSetupId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("room_setup_id", roomSetupId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetRoomSetups", dsParameters);

            var setups = dsSchedules.Tables[0].DataTableToList<RoomSetup>();
            
            return setups.FirstOrDefault(s => s.RoomSetupID == roomSetupId);
        }

        public async Task<List<RoomSetup>> GetRoomSetups(int? roomSetupId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("room_setup_id", roomSetupId ?? (object)DBNull.Value),
            };
            var dsSchedules = await ExecuteCommandAsync("GetRoomSetups", dsParameters);

            var setups = dsSchedules.Tables[0].DataTableToList<RoomSetup>();
            var setupEquipments = dsSchedules.Tables[1].DataTableToList<RoomSetupEquipment>();
            var setupItems = dsSchedules.Tables[2].DataTableToList<RoomSetupItem>();
            var staffPositions = dsSchedules.Tables[3].DataTableToList<RoomSetupStaffPosition>();
            var roomSetupImages = dsSchedules.Tables[4].DataTableToList<RoomSetupImage>();

            foreach (var setupItem in setupEquipments)
            {
                var setup = setups.FirstOrDefault(s => s.RoomSetupID == setupItem.RoomSetupID);
                setup?.SetupEquipment.Add(setupItem);
            }

            foreach (var setupItem in setupItems)
            {
                var setup = setups.FirstOrDefault(s => s.RoomSetupID == setupItem.RoomSetupID);
                setup?.SetupItems.Add(setupItem);
            }

            foreach (var staffPosition in staffPositions)
            {
                var setup = setups.FirstOrDefault(s => s.RoomSetupID == staffPosition.RoomSetupID);
                setup?.StaffPositions.Add(staffPosition);
            }

            foreach (var setupImage in roomSetupImages)
            {
                var setup = setups.FirstOrDefault(s => s.RoomSetupID == setupImage.RoomSetupID);
                setup?.SetupImages.Add(setupImage);
            }

            return setups;
        }

        public async Task<List<PatientPosition>> GetPatientPositions(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetPatientPositions", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<PatientPosition>();

            return result;
        }

        public async Task<List<BedOrientation>> GetBedOrientations(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetBedOrientations", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<BedOrientation>();

            return result;
        }

        public async Task<List<Laterality>> GetLateralities(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetLateralities", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<Laterality>();

            return result;
        }

        public async Task<List<SmartPhrase>> GetSmartPhrases(int? categoryId, int? specialtyId, int? userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("user_id", userId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("category_id", categoryId ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetPhrases", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<SmartPhrase>();

            return result;
        }
        
        public async Task<List<SmartPhraseCategory>> GetSmartPhraseCategories(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetPhraseCategories", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<SmartPhraseCategory>();

            return result;
        }

        public async Task<List<FlowPhrase>> GetFlowPhrases(int flowId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetFlowPhrases", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowPhrase>();

            return result;
        }

        public async Task<List<SurgeryPhrase>> GetSurgeryPhrases(int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryPhrases", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryPhrase>();

            return result;
        }

        public async Task<List<FlowImage>> GetFlowImages(int flowId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetFlowImages", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowImage>();

            return result;
        }

        public async Task<FlowImage> GetFlowImage(int flowImageId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("flow_image_id", flowImageId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetFlowImage", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowImage>();

            return result.FirstOrDefault();
        }

        public async Task<List<SurgeryImage>> GetSurgeryImages(int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryImages", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryImage>();

            return result;
        }

        public async Task<SurgeryAudits> GetSurgeryProposedTrays(int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryProposedTrays", dsParameters);

            var audits = dsSchedules.Tables[0].DataTableToList<SurgeryTrayAudit>();
            var items = dsSchedules.Tables[1].DataTableToList<TrayRationalizationItem>();
            var scrubTechs = dsSchedules.Tables[2].DataTableToList<User>();

            foreach (var proposalItem in items.GroupBy(i => i.TrayProposalID))
            {
                var sequence = 0;
                foreach (var item in proposalItem.Where(r => r.HistoryType == null))
                {
                    if (item.Sequence == null)
                        item.Sequence = (++sequence);

                    sequence = item.Sequence ?? 0;
                }

                audits.First(a => a.TrayProposalID == proposalItem.Key).Instruments = proposalItem.ToList();
            }

            var result = new SurgeryAudits()
            {
                Audits = audits,
                ScrubTechs = scrubTechs
            };

            return result;
        }

        public async Task<List<RoomSummary>> GetSurgeryRoomSummary(int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryRoomSummary", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<RoomSummary>();

            return result;
        }

        public async Task<List<RoomSummary>> GetSurgeryRoomOverview(
            int? specialtyId, int? roomGroupId, int? roomId, int? surgeonId, 
            DateTime surgeryDate, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("room_group_id", roomGroupId ?? (object)DBNull.Value),
                new SqlParameter("room_id", roomId ?? (object)DBNull.Value),
                new SqlParameter("surgeon_id", surgeonId ?? (object)DBNull.Value),
                new SqlParameter("surgery_date", surgeryDate),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryRoomOverview", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<RoomSummary>();

            return result;
        }

        public async Task<List<SurgeryDelay>> GetFlowSurgeryDelays(int flowId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetFlowSurgeryDelays", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryDelay>();

            return result;
        }

        public async Task<List<FlowSurgeonNote>> GetSurgeonNotes(int flowId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_id", flowId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetFlowSurgeonNotes", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowSurgeonNote>();

            return result;
        }

        public async Task<int> UpdateRoom(int roomId, string description, int typeId, int groupId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("room_id", roomId),
                new SqlParameter("description", description),
                new SqlParameter("room_type_id", typeId),
                new SqlParameter("room_group_id", groupId)
            };
            var result = await ExecuteNonQueryAsync("UpdateRoom", dsParameters);

            return result;
        }

        public async Task<int> InsertRoom(string description, int typeId, int groupId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("description", description),
                new SqlParameter("room_type_id", typeId),
                new SqlParameter("room_group_id", groupId)
            };
            var dsResult = await ExecuteCommandAsync("InsertRoom", dsParameters);

            var result = dsResult.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public async Task<int> DeleteRoom(int roomId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("room_id", roomId)
            };
            var result = await ExecuteNonQueryAsync("DeleteRoom", dsParameters);

            return result;
        }

        public async Task<int> UpdateRoomSetup(int roomSetupId, int providerId, int locationId, RoomSetup newRoomSetup)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("setup_name", newRoomSetup.SetupName),
                new SqlParameter("laterality_id", newRoomSetup.LateralityID),
                new SqlParameter("patient_extremity_position_id", newRoomSetup.PatientExtremityPositionID),
                new SqlParameter("room_type_id", newRoomSetup.RoomTypeID),
                new SqlParameter("bed_orientation_id", newRoomSetup.BedOrientationID),
            };
            var update = await ExecuteNonQueryAsync("UpdateRoomSetup", dsParameters);
            var currentRoomSetup = (await GetRoomSetups(roomSetupId, providerId, locationId)).FirstOrDefault();

            await SyncRoomSetupAttributes(roomSetupId, providerId, locationId, newRoomSetup, currentRoomSetup);

            return roomSetupId;
        }

        public async Task<int> DeleteRoomSetup(int roomSetupId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var insert = await ExecuteNonQueryAsync("DeleteRoomSetup", dsParameters);

            return insert;
        }

        public async Task<int> CreateRoomSetup(RoomSetup roomSetup, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("setup_name", roomSetup.SetupName),
                new SqlParameter("laterality_id", roomSetup.LateralityID),
                new SqlParameter("patient_extremity_position_id", roomSetup.PatientExtremityPositionID),
                new SqlParameter("room_type_id", roomSetup.RoomTypeID),
                new SqlParameter("bed_orientation_id", roomSetup.BedOrientationID)
            };
            var insert = await ExecuteCommandAsync("InsertRoomSetup", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            var roomSetupId = result.FirstOrDefault()?.Identifier ?? -1;

            await SyncRoomSetupAttributes(roomSetupId, providerId, locationId, roomSetup, null);

            return roomSetupId;
        }

        private async Task SyncRoomSetupAttributes(int roomSetupId, int providerId, int locationId, RoomSetup newRoomSetup, RoomSetup currentRoomSetup)
        {
            foreach (var roomSetupEquipment in newRoomSetup.SetupEquipment)
            {
                var currentEquipment = currentRoomSetup?.SetupEquipment?.FirstOrDefault(se =>
                    se.RoomSetupEquipmentID == roomSetupEquipment.RoomSetupEquipmentID);

                if (currentEquipment != null)
                    await UpdateRoomSetupEquipment(roomSetupEquipment, providerId, locationId);
                else
                {
                    roomSetupEquipment.RoomSetupEquipmentID =
                        await InsertRoomSetupEquipment(roomSetupId, roomSetupEquipment, providerId, locationId);
                }
            }
            foreach (var roomSetupItem in newRoomSetup.SetupItems)
            {
                var currentItem = currentRoomSetup?.SetupItems?.FirstOrDefault(se =>
                    se.RoomSetupItemID == roomSetupItem.RoomSetupItemID);

                if (currentItem != null)
                    await UpdateRoomSetupItem(roomSetupItem, providerId, locationId);
                else
                {
                    roomSetupItem.RoomSetupItemID =
                        await InsertRoomSetupItem(roomSetupId, roomSetupItem, providerId, locationId);
                }
            }
            foreach (var roomSetupStaffPosition in newRoomSetup.StaffPositions)
            {
                var currentStaffPosition = currentRoomSetup?.StaffPositions?.FirstOrDefault(se =>
                    se.StaffPosition == roomSetupStaffPosition.StaffPosition);

                if (currentStaffPosition != null)
                    await UpdateRoomSetupStaffPosition(roomSetupStaffPosition, providerId, locationId);
                else
                {
                    await InsertRoomSetupStaffPosition(roomSetupId, roomSetupStaffPosition, providerId, locationId);
                }
            }
            foreach (var setupImage in newRoomSetup.SetupImages)
            {
                var currentImage = currentRoomSetup?.SetupImages?.FirstOrDefault(img =>
                    img.RoomSetupImageID == setupImage.RoomSetupImageID);

                // can't create inline - must upload out of band
                if (currentImage != null)
                    await UpdateRoomSetupImage(currentImage.RoomSetupImageID, setupImage.Label, providerId, locationId);
            }

            if (currentRoomSetup != null)
            {
                foreach (var currentSetupEquipment in currentRoomSetup.SetupEquipment ?? new List<RoomSetupEquipment>())
                {
                    var sentEquipment = newRoomSetup.SetupEquipment.FirstOrDefault(se =>
                        se.RoomSetupEquipmentID == currentSetupEquipment.RoomSetupEquipmentID);

                    if (sentEquipment == null)
                        await DeleteRoomSetupEquipment(currentSetupEquipment.RoomSetupEquipmentID, providerId, locationId);
                }

                foreach (var currentSetupEquipment in currentRoomSetup.SetupItems ?? new List<RoomSetupItem>())
                {
                    var sentItem = newRoomSetup.SetupItems.FirstOrDefault(se =>
                        se.RoomSetupItemID == currentSetupEquipment.RoomSetupItemID);

                    if (sentItem == null)
                        await DeleteRoomSetupItem(currentSetupEquipment.RoomSetupItemID, providerId, locationId);
                }

                foreach (var currentStaffPosition in currentRoomSetup.StaffPositions ?? new List<RoomSetupStaffPosition>())
                {
                    var sentItem = newRoomSetup.StaffPositions.FirstOrDefault(se =>
                        se.StaffPosition == currentStaffPosition.StaffPosition);

                    if (sentItem == null)
                        await DeleteRoomSetupStaffPosition(currentStaffPosition, providerId, locationId);
                }

                foreach (var currentImage in currentRoomSetup.SetupImages ?? new List<RoomSetupImage>())
                {
                    var sentImage = newRoomSetup.SetupImages.FirstOrDefault(img =>
                        img.RoomSetupImageID == currentImage.RoomSetupImageID);

                    if (sentImage == null)
                        await DeleteRoomSetupImage(currentImage.RoomSetupImageID, providerId, locationId);
                }
            }
        }

        public async Task<int> NewRoomSetupImage(int roomSetupId, string label, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("label", label ?? (object)DBNull.Value)
            };
            var insert = await ExecuteCommandAsync("InsertRoomSetupImage", dsParameters);
            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.First().Identifier;
        }

        public async Task<int> UpdateRoomSetupImage(int roomSetupImageId, string label, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("room_setup_image_id", roomSetupImageId),
                new SqlParameter("label", label ?? (object)DBNull.Value)
            };
            return await ExecuteNonQueryAsync("UpdateRoomSetupImage", dsParameters);
        }

        public async Task<int> DeleteRoomSetupImage(int roomSetupImageId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("room_setup_image_id", roomSetupImageId)
            };
            return await ExecuteNonQueryAsync("DeleteRoomSetupImage", dsParameters);
        }

        public async Task<int> InsertRoomSetupEquipment(int roomSetupId, RoomSetupEquipment roomSetupEquipment, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("item_id", roomSetupEquipment.ItemID),
                new SqlParameter("equipment_position", roomSetupEquipment.EquipmentPosition)
            };
            var insert = await ExecuteCommandAsync("InsertRoomSetupEquipment", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            var roomSetupEquipmentId = result.FirstOrDefault()?.Identifier ?? -1;

            return roomSetupEquipmentId;
        }

        public async Task<int> InsertRoomSetupItem(int roomSetupId, RoomSetupItem roomSetupItem, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("item_id", roomSetupItem.ItemID),
                new SqlParameter("quantity", roomSetupItem.ItemQuantity)
            };
            var insert = await ExecuteCommandAsync("InsertRoomSetupItem", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            var roomSetupItemId = result.FirstOrDefault()?.Identifier ?? -1;

            return roomSetupItemId;
        }

        public async Task<int> InsertRoomSetupStaffPosition(int roomSetupId, RoomSetupStaffPosition roomSetupStaffPosition, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("staff_position", roomSetupStaffPosition.StaffPosition),
                new SqlParameter("staff_role_id", roomSetupStaffPosition.StaffRoleID)
            };
            var result = await ExecuteNonQueryAsync("InsertRoomSetupStaffPosition", dsParameters);

            return result;
        }
        public async Task<int> UpdateRoomSetupEquipment(RoomSetupEquipment roomSetupEquipment, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_equipment_id", roomSetupEquipment.RoomSetupEquipmentID),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("item_id", roomSetupEquipment.ItemID),
                new SqlParameter("equipment_position", roomSetupEquipment.EquipmentPosition)
            };

            var result = await ExecuteNonQueryAsync("UpdateRoomSetupEquipment", dsParameters);

            return result;
        }

        public async Task<int> UpdateRoomSetupItem(RoomSetupItem roomSetupItem, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_item_id", roomSetupItem.RoomSetupItemID),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("item_id", roomSetupItem.ItemID),
                new SqlParameter("quantity", roomSetupItem.ItemQuantity)
            };
            var result = await ExecuteNonQueryAsync("UpdateRoomSetupItem", dsParameters);

            return result;
        }

        public async Task<int> UpdateRoomSetupStaffPosition(RoomSetupStaffPosition roomSetupStaffPosition, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupStaffPosition.RoomSetupID),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("staff_position", roomSetupStaffPosition.StaffPosition),
                new SqlParameter("staff_role_id", roomSetupStaffPosition.StaffRoleID)
            };
            var result = await ExecuteNonQueryAsync("UpdateRoomsetupStaffPosition", dsParameters);

            return result;
        }
        public async Task<int> DeleteRoomSetupEquipment(int roomSetupEquipmentId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_equipment_id", roomSetupEquipmentId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("DeleteRoomSetupEquipment", dsParameters);

            return result;
        }

        public async Task<int> DeleteRoomSetupItem(int roomSetupItemId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_item_id", roomSetupItemId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = await ExecuteNonQueryAsync("DeleteRoomSetupItem", dsParameters);

            return result;
        }

        public async Task<int> DeleteRoomSetupStaffPosition(RoomSetupStaffPosition roomSetupStaffPosition, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupStaffPosition.RoomSetupID),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("staff_position", roomSetupStaffPosition.StaffPosition)
            };
            var result = await ExecuteNonQueryAsync("DeleteRoomSetupStaffPosition", dsParameters);

            return result;
        }

        public async Task<PatientSurgery> GetSurgery(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var command = "GetSurgeryProcedure";
            var dsSchedules = await ExecuteCommandAsync(command, parameters);

            var result = dsSchedules.Tables[0].DataTableToList<PatientSurgery>().FirstOrDefault();

            return result;
        }

        public async Task<Surgery> GetCase(int caseId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("case_id", caseId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsSchedules = await ExecuteCommandAsync("GetCase", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgery>().FirstOrDefault();

            return result;
        }

        public async Task<List<SurgerySearchResult>> SearchCases(int? userId, int? surgeonUserId, 
            int? roomGroupId, int? roomId, int? bundleId, int? procedureId, int? specialtyId, 
            DateTime? begDate, DateTime? endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId ?? (object)DBNull.Value),
                new SqlParameter("surgeon_user_id", surgeonUserId ?? (object)DBNull.Value),
                new SqlParameter("room_group_id", roomGroupId ?? (object)DBNull.Value),
                new SqlParameter("room_id", roomId ?? (object)DBNull.Value),
                new SqlParameter("bundle_id", bundleId ?? (object)DBNull.Value),
                new SqlParameter("procedure_id", procedureId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("beg_date", begDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsSchedules = await ExecuteCommandAsync("SearchCases", parameters);

            var surgeries = dsSchedules.Tables[0].DataTableToList<SurgerySearchResult>();
            var surgeryUsers = dsSchedules.Tables[1].DataTableToList<SurgeryUser>();

            foreach (var surgeryUser in surgeryUsers)
            {
                var surgery = surgeries.FirstOrDefault(s => s.SurgeryID == surgeryUser.SurgeryID);
                surgery?.SurgeryUsers?.Add(surgeryUser);
            }

            return surgeries;
        }

        public async Task<List<SurgerySearchResult>> LoadProposalCounts(List<SurgerySearchResult> schedule, int providerId, int locationId)
        {
            var surgeryXml = GetIdentitySummary(schedule.Select(s => s.SurgeryID).ToList());

            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryXml ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("SearchCaseProposals", parameters);
            var results = dsSchedules.Tables[0].DataTableToList<CountPriority>();

            foreach (var surgery in schedule)
            {
                surgery.ProposalCounts = results.Where(r => r.SurgeryID == surgery.SurgeryID).ToList();
            }

            return schedule;
        }

        public async Task<int> UpdateProposedTrayComments(int trayProposalId, string comments, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("tray_proposal_id", trayProposalId),
                new SqlParameter("comments", comments ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateProposedTrayComments", parameters);
            
            return result;
        }

        public async Task<List<SurgeryAuditSearchResult>> GetProposedTrayAuditSearch(int? trayProposalId,
            int? surgeonUserId,
            int? specialtyId, int? trayId, int? cardId,
            DateTime? begDate, DateTime? endDate, string target, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("target", target),
                new SqlParameter("tray_proposal_id", trayProposalId ?? (object)DBNull.Value),
                new SqlParameter("surgeon_user_id", surgeonUserId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("tray_id", trayId ?? (object)DBNull.Value),
                new SqlParameter("card_id", cardId ?? (object)DBNull.Value),
                new SqlParameter("beg_date", begDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsSchedules = await ExecuteCommandAsync("GetProposedTrayAuditSearch", parameters);

            var surgeries = dsSchedules.Tables[0].DataTableToList<SurgeryAuditSearchResult>();
            var auditTrays = dsSchedules.Tables[1].DataTableToList<SurgeryAuditTray>();

            foreach (var auditTray in auditTrays)
            {
                var surgery = surgeries.FirstOrDefault(s => s.SurgeryID == auditTray.SurgeryID);
                surgery?.Trays?.Add(auditTray);
            }

            return surgeries;
        }

        public async Task<List<SurgerySearchResult>> GetSurgeonCases(int userId, DateTime begDate, DateTime endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("beg_date", begDate),
                new SqlParameter("end_date", endDate),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsSchedules = await ExecuteCommandAsync("SearchSurgeonCases", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgerySearchResult>();

            return result;
        }

        public async Task<List<SurgerySearchResult>> GetRoomCases(int roomId, DateTime begDate, DateTime endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("room_id", roomId),
                new SqlParameter("beg_date", begDate),
                new SqlParameter("end_date", endDate),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsSchedules = await ExecuteCommandAsync("SearchRoomCases", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgerySearchResult>();

            return result;
        }

        public async Task<List<SurgerySchedule>> GetScheduledSurgeries(int? userID, int providerId, int locationId,
            DateTime? scheduleDate, int? roomId)
        {
            if (roomId.HasValue)
                userID = null;

            var parameters = new[]
            {
                new SqlParameter("user_id", userID ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("SurgeryScheduleDate", scheduleDate ?? (object)DBNull.Value),
                new SqlParameter("room_id", roomId ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeriesByUser", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgerySchedule>();

            return result;
        }

        public async Task<List<Surgery>> GetSurgeryAlerts(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCaseAlerts", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgery>();

            return result;
        }

        public async Task <List<SurgeryDelayReason>> GetSurgeryDelayReasons(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryDelayReasons", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryDelayReason>();

            return result;
        }

        public async Task<List<SurgeryUser>> GetSurgeryUsers(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCheckInCase", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryUser>();

            return result;
        }

        public async Task<List<SurgeryVendorRep>> GetSurgeryVendorReps(int surgeryId, int locationId, int providerId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryVendorReps", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryVendorRep>();

            return result;
        }

        public async Task<CardFlowRoom> GetBundleDefaultCardFlowRoom(int bundleId, int userId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("bundle_id", bundleId),
                new SqlParameter("user_id", userId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetBundleDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>();

            return result.FirstOrDefault();
        }

        public async Task<List<Surgeon>> GetImportSurgeons(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetImportSurgeons", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgeon>();

            return result;
        }

        public async Task<List<Procedure>> GetImportProcedures(string importSurgeon, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgeon", importSurgeon ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetImportProcedures", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Procedure>();

            return result;
        }

        public async Task<List<CardFlowRoom>> GetProcedureDefaultCardFlowRoom(int providerId, int locationId, string cptCode)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("cpt_code", cptCode ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProcedureDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>();

            return result;
        }

        public async Task<CardFlowRoom> GetImportDefaultCardFlowRoom(int providerId, int locationId, int ownerUserId, string procedureCard)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("owner_user_id", ownerUserId),
                new SqlParameter("procedure_card", procedureCard ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetImportDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>().FirstOrDefault();

            return result;
        }

        public async Task<List<CardFlowRoom>> GetSpecialtyProcedureDefaultCardFlowRoom(int providerId, int locationId, string cptCode)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("cpt_code", cptCode ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProcedureDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>();

            return result;
        }

        public async Task<List<CardFlowRoom>> GetMultipleProceduresDefaultCardFlowRoom(int providerId, int locationId, int specialtyId, string cptCodes)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("cpt_codes", cptCodes ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetMultipleProceduresDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>();

            return result;
        }

        public async Task<List<CardFlowRoom>> GetSpecialtyMultipleProceduresDefaultCardFlowRoom(int providerId, int locationId, int specialtyId, string cptCodes)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("cpt_codes", cptCodes ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSpecialtyMultipleProceduresDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>();

            return result;
        }

        public async Task<List<CardBundle>> GetBundles(int? specialtyId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetBundlesBySpecialty", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardBundle>();
            var procedures = dsSchedules.Tables[1].DataTableToList<BundleProcedure>();

            foreach (var procedure in procedures)
            {
                var bundle = result.FirstOrDefault(b => b.BundleID == procedure.BundleID);
                bundle?.Procedures.Add(procedure);
            }

            return result;
        }

        public async Task<List<BundleProcedure>> GetBundleProcedures(int bundleId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("bundle_id", bundleId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetBundleProcedures", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<BundleProcedure>();

            return result;
        }

        public async Task<int> NewBundle(string description, int specialtyId, List<int> procedures, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("description", description),
                new SqlParameter("specialty_id", specialtyId)
            };
            var dsSchedules = await ExecuteCommandAsync("InsertBundle", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<InsertionResult>();

            var bundleId = result.FirstOrDefault()?.Identifier ?? 0;

            await UpdateBundleProcedures(bundleId, procedures, providerId, locationId);

            return bundleId;
        }

        public async Task<int> UpdateBundle(int bundleId, string description, int specialtyId, List<int> procedures, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("bundle_id", bundleId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("description", description),
                new SqlParameter("specialty_id", specialtyId)
            };
            var result = await ExecuteNonQueryAsync("UpdateBundle", parameters);

            await UpdateBundleProcedures(bundleId, procedures, providerId, locationId);

            return result;
        }

        private async Task<int> UpdateBundleProcedures(int bundleId, List<int> procedures, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("bundle_id", bundleId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteBundleProcedures", parameters);

            foreach (var procedureId in procedures)
            {
                parameters = new[]
                {
                    new SqlParameter("bundle_id", bundleId),
                    new SqlParameter("procedure_id", procedureId),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId)
                };

                result = await ExecuteNonQueryAsync("InsertBundleProcedure", parameters);
            }

            return result;
        }

        public async Task<int> DeleteBundle(int bundleId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("bundle_id", bundleId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteBundle", parameters);

            return result;
        }

        public async Task<List<BundleProcedure>> GetSurgeryProcedures(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryProcedures", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<BundleProcedure>();

            return result;
        }

        public async Task<List<Procedure>> GetProcedures(int? specialtyId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProceduresBySpecialty", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Procedure>();

            return result;
        }

        public async Task<List<Procedure>> GetCptCodes(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCptCodes", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Procedure>();

            return result;
        }

        public async Task<List<Specialty>> GetSpecialties(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSpecialties", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Specialty>();

            return result;
        }

        public async Task<List<Specialty>> GetSpecialtiesInternal()
        {
            var parameters = new SqlParameter[]
            {
            };
            var dsSchedules = await ExecuteCommandAsync("GetSpecialtiesInternal", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Specialty>();

            return result;
        }

        public async Task<int> UpdateSpecialty(int specialtyId, string name, string description, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("name", name),
                new SqlParameter("description", description),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateSpecialty", parameters);

            return result;
        }

        public async Task<int> InsertSpecialty(string name, string description, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("name", name),
                new SqlParameter("description", description),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsResult = await ExecuteCommandAsync("InsertSpecialty", parameters);

            var result = dsResult.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public async Task<int> DeleteSpecialty(int specialtyId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("specialty_id", specialtyId)
            };
            var result = await ExecuteNonQueryAsync("DeleteSpecialty", dsParameters);

            return result;
        }

        public async Task<List<SpecialtyCardCategory>> GetSpecialtyProcedureGroup(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSpecialtyProcedureGroup", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SpecialtyCardCategory>();

            return result;
        }

        public async Task<int> InsertSpecialtyProcedureGroup(int specialtyId, int procedureGroupId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("procedure_group_id", procedureGroupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("InsertSpecialtyProcedureGroup", parameters);

            return result;
        }

        public async Task<int> DeleteSpecialtyProcedureGroup(int specialtyId, int procedureGroupId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("procedure_group_id", procedureGroupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            
            var result = await ExecuteNonQueryAsync("DeleteSpecialtyProcedureGroup", parameters);

            return result;
        }

        public async Task<List<Surgeon>> GetSurgeons(int? specialtyId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeonBySpecialty", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgeon>();

            return result;
        }

        public async Task<Flow> GetFlow(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardFlowData", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Flow>();

            return result.FirstOrDefault();
        }

        public async Task<List<Flow>> GetCardFlowList(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardFlowList", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Flow>();

            return result;
        }

        public async Task<List<FlowStepTiming>> GetFlowTimings(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
                {
                    new SqlParameter("flow_id", flowId),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId)
                };


            var dsSchedules = await ExecuteCommandAsync("GetFlowTimings", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowStepTiming>();

            return result;
        }

        public async Task<List<FlowStepSurgeryTiming>> GetFlowSurgeryTimings(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };


            var dsSchedules = await ExecuteCommandAsync("GetFlowSurgeryTimings", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowStepSurgeryTiming>();

            FlowStepSurgeryTiming.CalculateEstimatedTimes(result);

            return result;
        }

        public async Task<List<FlowStepInstructionResult>> GetFlowInstructions(int flowId, int? surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
                {
                    new SqlParameter("flow_id", flowId),
                    new SqlParameter("surgery_id", surgeryId ?? (object)DBNull.Value),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId)
                };


            var dsSchedules = await ExecuteCommandAsync("GetFlowInstructions", parameters);

            var instructions = dsSchedules.Tables[0].DataTableToList<FlowInstruction>();
            var result = new List<FlowStepInstructionResult>();

            foreach (var stepInstruction in instructions.GroupBy(i => i.StepID))
            {
                var flowStepInstruction = new FlowStepInstructionResult()
                {
                    StepID = stepInstruction.Key,
                    StepName = stepInstruction.First().StepDescription
                };

                foreach (var roleInstruction in stepInstruction.GroupBy(i => i.RoleID))
                {
                    var flowRoleInstruction = new FlowRoleInstruction()
                    {
                        RoleID = roleInstruction.Key,
                        RoleName = stepInstruction.First().RoleDescription
                    };

                    flowRoleInstruction.FlowInstructions.AddRange(roleInstruction.ToList());
                    flowStepInstruction.FlowRoleInstructions.Add(flowRoleInstruction);
                }

                result.Add(flowStepInstruction);
            }

            return result;
        }

        public async Task<List<FlowStep>> GetFlowComments(int flowId, int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetFlowComments", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowStep>();

            return result;
        }

        public async Task<List<FlowMessaging>> GetFlowMessaging(int flowId, int providerId, int locationId, int stepId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("step_id", stepId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetFlowMessaging", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowMessaging>();

            return result;
        }

        public async Task<List<FlowContent>> GetFlowContent(int flowId, int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetFlowContent", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowContent>();

            return result;
        }

        public async Task<List<FlowNotification>> GetFlowNotifications(int flowId, int? stepId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("step_id", stepId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetFlowStepNotifications", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowNotification>();

            return result;
        }

        public async Task<List<Step>> GetSteps(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSteps", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Step>();

            return result;
        }

        public async Task<List<FlowFeedback>> GetFlowFeedback(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetFlowFeedback", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowFeedback>();

            return result;
        }

        public async Task<int> DeleteFlowStep(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteFlowStep", parameters);

            return result;
        }

        public async Task<int> InsertFlowStep(int flowId, int stepId, int stepSequence, decimal duration, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("duration", duration),
                new SqlParameter("step_sequence", stepSequence),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("NewFlowStep", parameters);

            return result;
        }

        public async Task<int> UpdateSurgeryStep(int stepId, string stepDescription, string notificationType, bool stepTiming, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("step_id", stepId),
                new SqlParameter("step_description", stepDescription),
                new SqlParameter("notification_type", notificationType),
                new SqlParameter("dashboard_time", stepTiming),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateStep", parameters);

            return result;
        }

        public async Task<int> AddSurgeryStep(string stepDescription, string notificationType, bool stepTiming, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("step_description", stepDescription),
                new SqlParameter("notification_type", notificationType),
                new SqlParameter("dashboard_time", stepTiming),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsNotification = await ExecuteCommandAsync("InsertStep", parameters);

            var result = dsNotification.Tables[0].DataTableToList<InsertionResult>().First().Identifier;

            return result;
        }

        public async Task<int> DeleteSurgery(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteSurgery", parameters);

            return result;
        }

        public async Task<int> DeleteSurgeryStep(int stepId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("step_id", stepId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteStep", parameters);

            return result;
        }

        public async Task<int> InsertFlowNotification(int flowId, int stepId, int notificationType, string message, 
            string smsNumber, string emailAddress, int? messagingUserId, int? messagingRoleId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("notification_type", notificationType),
                new SqlParameter("message", message),
                new SqlParameter("sms_number", smsNumber ?? (object)DBNull.Value),
                new SqlParameter("email_address", emailAddress ?? (object)DBNull.Value),
                new SqlParameter("messaging_user_id", messagingUserId ?? (object)DBNull.Value),
                new SqlParameter("messaging_role_id", messagingRoleId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsNotification = await ExecuteCommandAsync("InsertFlowNotification", parameters);

            var result = dsNotification.Tables[0].DataTableToList<InsertionResult>().First().Identifier;

            return result;
        }

        public async Task<int> EditFlowNotification(int flowNotificationId, string message, int stepId,
            string smsNumber, string emailAddress, int? messagingUserId, int? messagingRoleId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_notification_id", flowNotificationId),
                new SqlParameter("message", message),
                new SqlParameter("step_id", stepId),
                new SqlParameter("sms_number", smsNumber ?? (object)DBNull.Value),
                new SqlParameter("email_address", emailAddress ?? (object)DBNull.Value),
                new SqlParameter("messaging_user_id", messagingUserId ?? (object)DBNull.Value),
                new SqlParameter("messaging_role_id", messagingRoleId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("UpdateFlowNotification", parameters);

            return result;
        }

        public async Task<int> DeleteFlowNotification(int flowNotificationId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_notification_id", flowNotificationId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = await ExecuteNonQueryAsync("DeleteFlowNotification", parameters);

            return result;
        }

        public async Task<int> NewFlow(int cardId, int roomSetupId, string description, int userId, bool defaultFlow, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("user_id", userId),
                new SqlParameter("default_flow", defaultFlow),
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("description", description ?? (object)DBNull.Value)
            };
            var dsSchedules = await ExecuteCommandAsync("NewFlow", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<InsertionResult>().First().Identifier;

            return result;
        }

        public async Task<int> UpdateFlow(int flowId, int cardId, int roomSetupId, string description, int userId, bool defaultFlow, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("user_id", userId),
                new SqlParameter("default_flow", defaultFlow),
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("description", description ?? (object)DBNull.Value)
            };

            return await ExecuteNonQueryAsync("UpdateFlow", parameters);
        }

        public async Task<int> DeleteFlow(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            return await ExecuteNonQueryAsync("DeleteFlow", parameters);
        }

        public async Task<ImportResult> UpdateCardItemImport(List<CardImport> records, FileParserRelations relations, int providerId, int locationId)
        {
            var result = new ImportResult();

            foreach (var record in records)
            {
                if (record.Quantity == null) record.Quantity = 1; // Provide default value

                // Sometimes product nbr is in item name
                record.ItemName = record.ItemName.Replace($" - {record.ProductNbr}", "");
            }

            foreach (var surgeonCard in records.GroupBy(r => new { r.Surgeon, r.PreferenceCardName }))
            {
                var card = surgeonCard.First();

                var surgeon = relations.Surgeons.FirstOrDefault(r => r.LastName == card.PrimarySurgeon.LastName
                        && r.FirstName == card.PrimarySurgeon.FirstName);

                if (surgeon == null)
                {
                    result.Messages.Add("Unable to find surgeon: " + card.Surgeon);
                    continue;
                }


                var doc = new XmlDocument();
                var table = doc.CreateElement("table");

                foreach (var cardItem in surgeonCard)
                {
                    var row = doc.CreateElement("row");
                    table.AppendChild(row);

                    if (cardItem.ItemName?.Length > 100)
                        cardItem.ItemName = cardItem.ItemName.Substring(0, 100);
                    if (cardItem.ItemType?.Length > 30)
                        cardItem.ItemType = cardItem.ItemType.Substring(0, 30);
                    if (cardItem.ProductNbr?.Length > 30)
                        cardItem.ProductNbr = cardItem.ProductNbr.Substring(0, 30);
                    
                    AddColumn(doc, row, cardItem.ItemName ?? "");
                    AddColumn(doc, row, cardItem.ItemType ?? "");
                    AddColumn(doc, row, cardItem.ProductNbr ?? "");
                    AddColumn(doc, row, cardItem.Quantity);
                }

                var itemData = table.OuterXml;

                var parameters = new[]
                {
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId),
                    new SqlParameter("owner_user_id", surgeon.UserID),
                    new SqlParameter("preference_card_name", card.PreferenceCardName),
                    new SqlParameter("item_data", itemData)
                };

                result.Identity = await ExecuteNonQueryAsync(@"UpdateCardItemImport", parameters);
            }

            return result;
        }

        public async Task<ImportResult> InsertStagingData(int providerId, int locationId, int? secureId, IImportData sourceData, FileParserRelations relations)
        {
            var result = new ImportResult();

            if (sourceData is ItemImport item)
            {
                if (string.IsNullOrEmpty(item.Description))
                    return result;

                var parameters = new[]
                {
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId),
                    new SqlParameter("catalog_id", item.Catalog),
                    new SqlParameter("ehr_id", item.EHR_ID),
                    new SqlParameter("item_type", item.ItemType),
                    new SqlParameter("category", item.Category),
                    new SqlParameter("item_description", item.Description),
                    new SqlParameter("unit_of_measure", item.UnitOfMeasure),
                    new SqlParameter("manufacturer", item.Manufacturer),
                    new SqlParameter("unit_cost", item.Cost)
                };

                result.Identity = await ExecuteNonQueryAsync(@"UpdateItemImport", parameters);

            }
            else if (sourceData is CardImport card)
            {
                var tmpInt = 0;
            }
            else if (sourceData is TrayImport tray)
            {
                if (string.IsNullOrEmpty(tray.TrayName) ||
                    string.IsNullOrEmpty(tray.InstrumentName))
                    return result;

                var parameters = new[]
                {
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId),
                    new SqlParameter("tray_id", tray.TrayID),
                    new SqlParameter("tray_name", tray.TrayName),
                    new SqlParameter("instrument_name", tray.InstrumentName),
                    new SqlParameter("manufacturer", tray.Manufacturer),
                    new SqlParameter("quantity", tray.Quantity),
                    new SqlParameter("category", tray.Category)
                };

                result.Identity = await ExecuteNonQueryAsync(@"UpdateTrayImport", parameters);
            }
            else if (sourceData is UserImport user)
            {
                Specialty specialty = null;

                var role = relations.Roles.FirstOrDefault(r => r.RoleDescription == user.Role);
                if (role == null)
                {
                    result.Messages.Add("Unable to find role: " + user.Role);
                    return result;
                }

                if (user.Specialty != null)
                {
                    specialty = relations.Specialties.FirstOrDefault(r => r.SpecialtyDescription == user.Specialty);

                    if (specialty == null)
                    {
                        result.Messages.Add("Unable to find specialty: " + user.Specialty);
                        return result;
                    }
                }

                var parameters = new[]
                {
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId),
                    new SqlParameter("last_name", user.UserEntity.LastName),
                    new SqlParameter("first_name", user.UserEntity.FirstName),
                    new SqlParameter("role_id", role.RoleID),
                    new SqlParameter("specialty_id", specialty?.SpecialtyID ?? (object)DBNull.Value)
                };

                result.Identity = await ExecuteNonQueryAsync(@"UpdateUserImport", parameters);
            }
            else if(sourceData is ScheduleBase scheduleBase)
            {
                var surgery = new SurgeryPost()
                {
                    BundleID = null,
                    CaseNbr = scheduleBase.CaseNbr,
                    CptCode = scheduleBase.CptCode,
                    LateralityID = null,
                    ScheduleDate = scheduleBase.ScheduleDateTime,
                };
                
                var room = relations.Rooms.FirstOrDefault(r => r.RoomDescription == scheduleBase.Room);
                var surgeon = relations.Surgeons.FirstOrDefault(r => r.LastName == scheduleBase.PrimarySurgeon.LastName 
                    && r.FirstName == scheduleBase.PrimarySurgeon.FirstName);

                surgery.RoomID = room?.RoomID;
                surgery.SurgeonUserID = surgeon?.UserID;

                if (surgery.RoomID == null)
                {
                    // Only log when the room is non-empty?...
                    if (!string.IsNullOrEmpty(scheduleBase.Room) && scheduleBase.Room != "ORW LITHO")
                        result.Messages.Add("Unable to find room: " + scheduleBase.Room);
                    return result;
                }
                if (surgery.SurgeonUserID == null)
                {
                    result.Messages.Add("Unable to find primary surgeon: " + scheduleBase.Surgeon);
                    return result;
                }

                CardFlowRoom cardFlowRoom = null;
                if (scheduleBase is CardlessScheduleImport cardlessSchedule)
                {
                    cardFlowRoom = await GetCardFromTrays(surgery.SurgeonUserID.Value, cardlessSchedule.Trays, providerId, locationId);

                    if (cardFlowRoom == null)
                    {
                        result.Messages.Add("Unable to find card with trays: " + cardlessSchedule.TrayList);
                    }
                } 
                else if (scheduleBase is ScheduleImport schedule)
                {
                    var procedureCards = await DetermineCards(surgery, schedule, relations, providerId, locationId);

                    if (procedureCards.Any())
                    {
                        cardFlowRoom = await AggregateCards(surgery.SurgeonUserID.Value, procedureCards, providerId, locationId);

                        if (cardFlowRoom == null)
                        {
                            result.Messages.Add("Unable to find card: " + schedule.ProcedurePreferenceCards);
                        }
                    }
                    else
                    {
                        result.Messages.Add("Unable to find card: " + schedule.ProcedurePreferenceCards);
                    }
                }

                var caseId = await CreateCase(secureId ?? -1, surgery.SurgeonUserID, surgery.SpecialtyID, providerId,
                    locationId, surgery.CaseNbr);

                result.Identity = await CreateSurgery(surgery, secureId ?? -1, caseId,
                    cardFlowRoom?.CardID, cardFlowRoom?.TemplateFlowID, cardFlowRoom?.TemplateRoomSetupID,
                    providerId, locationId);

                foreach (var secondarySurgeon in scheduleBase.SecondarySurgeons)
                {
                    surgeon = relations.Surgeons.FirstOrDefault(r => r.LastName == secondarySurgeon.LastName && r.FirstName == secondarySurgeon.FirstName);

                    if (surgeon == null)
                    {
                        result.Messages.Add("Unable to find secondary surgeons: " + scheduleBase.Surgeon);
                    }
                    else
                    {
                        await AddSurgeryUser(result.Identity, surgeon.UserID, providerId, locationId);
                    }
                }
            }

            return result;
        }

        private async Task<CardFlowRoom> GetCardFromTrays(int ownerUserId, List<string> trayList, int providerId, int locationId)
        {
            if (trayList == null || !trayList.Any())
                return null;

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var tray in trayList)
            {
                var trimTray = tray.Trim();

                // prevent adding invalid data
                if (string.IsNullOrEmpty(trimTray))
                    continue;

                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, trimTray);
            }

            var cardData = table.OuterXml;

            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("owner_user_id", ownerUserId),
                new SqlParameter("tray_data", cardData)
            };

            var dsCardMatch = await ExecuteCommandAsync("GetCardFromTrays", parameters);

            var matches = dsCardMatch.Tables[0].DataTableToList<CardFlowRoom>();
            
            return matches.FirstOrDefault();
        }

        private async Task<CardFlowRoom> AggregateCards(int ownerUserId, List<CardFlowRoom> procedureCards, int providerId, int locationId)
        {
            if (procedureCards == null || !procedureCards.Any())
                return null;

            if (procedureCards.Count == 1)
                return procedureCards[0];

            var doc = new XmlDocument();
            var table = doc.CreateElement("table");

            foreach (var customItem in procedureCards)
            {
                // prevent adding invalid data
                if (customItem.CardID <= 0)
                    continue;

                var row = doc.CreateElement("row");
                table.AppendChild(row);

                AddColumn(doc, row, customItem.CardID);
            }

            var cardData = table.OuterXml;

            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_data", cardData)
            };

            var dsAnalyze = await ExecuteCommandAsync("GetCompositeCards", parameters);

            var potentialSources = dsAnalyze.Tables[0].DataTableToList<CardSource>();
            var desiredSources = dsAnalyze.Tables[1].DataTableToList<CardSource>();
            
            var match = AnalyzeSources(potentialSources, desiredSources);

            if (match != null) return match;

            var cardName = "COMPOSITE CARD: " + string.Join("|", desiredSources.Select(s => s.PreferenceCardName));
            if (cardName.Length > 250)
                cardName = cardName.Substring(0, 250);

            parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_data", cardData),
                new SqlParameter("card_name", cardName),
                new SqlParameter("owner_user_id", ownerUserId),
            };

            // we haven't hit one quite like this yet - make one now
            var dsImport = await ExecuteCommandAsync("InsertCompositeCard", parameters);
            match = dsImport.Tables[0].DataTableToList<CardFlowRoom>().FirstOrDefault();

            return match;
        }

        private CardFlowRoom AnalyzeSources(List<CardSource> potentialSources, List<CardSource> desiredSources)
        {
            foreach (var source in potentialSources.GroupBy(s => s.CardID))
            {
                if (source.Count() != desiredSources.Count) continue;

                var sameSources = true;
                foreach (var desiredSource in desiredSources)
                {
                    var matching = source.FirstOrDefault(s =>
                        s.PreferenceCardName == desiredSource.PreferenceCardName &&
                        s.SurgeonName == desiredSource.SurgeonName);

                    if (matching != null) continue;

                    sameSources = false;
                    break;
                }

                // the list of desired sources matches the card we found
                if (sameSources)
                {
                    return new CardFlowRoom()
                    {
                        CardID = source.Key,
                        TemplateFlowID = source.First().TemplateFlowID
                    };
                }
            }

            return null;
        }

        private async Task<List<CardFlowRoom>> DetermineCards(SurgeryPost surgery, ScheduleImport schedule, FileParserRelations relations, int providerId, int locationId)
        {
            var result = new List<CardFlowRoom>();
            foreach (var procedureCard in schedule.ProcedureCards)
            {
                var cardFlowRoom = await CheckCard(procedureCard.CardName, procedureCard, surgery, schedule, relations, providerId, locationId);

                if (cardFlowRoom != null)
                {
                    result.Add(cardFlowRoom);
                    continue;
                }

                // Sometimes card name appears where the surgeon should be
                cardFlowRoom = await CheckCard(procedureCard.ImportSurgeon?.RawText, procedureCard, surgery, schedule, relations, providerId, locationId);

                if (cardFlowRoom != null)
                {
                    result.Add(cardFlowRoom);
                }
            }

            return result;
        }

        private async Task<CardFlowRoom> CheckCard(string cardName, ImportCard procedureCard, SurgeryPost surgery, ScheduleImport schedule, FileParserRelations relations, int providerId, int locationId)
        {
            if (string.IsNullOrEmpty(cardName))
                return null;

            var cardSurgeon = relations.Surgeons.FirstOrDefault(r => r.LastName == procedureCard.ImportSurgeon?.LastName && r.FirstName == procedureCard.ImportSurgeon?.FirstName)?.UserID ?? surgery.SurgeonUserID;

            CardFlowRoom cardFlowRoom = null;
            if (cardSurgeon.HasValue)
                cardFlowRoom = await GetImportDefaultCardFlowRoom(providerId, locationId, cardSurgeon.Value, cardName);

            if (cardFlowRoom != null)
            {
                return cardFlowRoom;
            }

            // attempt to find a card for using any additional surgeons before failing
            foreach (var secondarySurgeon in schedule.SecondarySurgeons)
            {
                var surgeon = relations.Surgeons.FirstOrDefault(r => r.LastName == secondarySurgeon.LastName && r.FirstName == secondarySurgeon.FirstName);

                if (surgeon == null) continue;
                if (cardFlowRoom == null)
                {
                    cardFlowRoom = await GetImportDefaultCardFlowRoom(providerId, locationId,
                        surgeon.UserID, cardName);
                }
            }

            return cardFlowRoom;
        }

        public async Task<int> InsertImportLog(int providerId, int locationId, int importTypeId, int userId, int recordCount, string filename)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("import_id", importTypeId),
                new SqlParameter("user_id", userId),
                new SqlParameter("load_date", DateTime.Now),
                new SqlParameter("record_count", recordCount),
                new SqlParameter("file_name", filename)
            };
            var dsImport = await ExecuteCommandAsync("InsertImportLog", parameters);

            var result = dsImport.Tables[0].DataTableToList<InsertionResult>().First().Identifier;

            return result;
        }

        public async Task InsertImportMessage(int providerId, int locationId, int logId, string logType, string logMessage, string logMrn, DateTime? logServiceDate)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("import_log_id", logId),
                new SqlParameter("error_type", logType),
                new SqlParameter("error_message", logMessage),
                new SqlParameter("error_mrn", logMrn ?? (object)DBNull.Value),
                new SqlParameter("error_service_date", logServiceDate ?? (object)DBNull.Value)
            };

            await ExecuteNonQueryAsync("InsertImportMessage", parameters);
        }
    }
}