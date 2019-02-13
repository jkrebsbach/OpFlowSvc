using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using OpFlow.Data;
using OpFlow.Data.Administration;
using OpFlow.Data.Analytics;
using OpFlow.Data.Debrief;

namespace OpFlow.Service.DataAccess
{
    public static class SqlHelper 
    {
        private static async Task<DataSet> ExecuteCommandAsync(string storedProcedure, SqlParameter[] dsParameters = null)
        {
            try
            {
                using (var conn =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["OpFlowConnection"].ConnectionString))
                using (var cmd = new SqlCommand(storedProcedure, conn) {CommandType = CommandType.StoredProcedure})
                {
                    cmd.Parameters.AddRange(dsParameters);

                    await conn.OpenAsync();

                    using (var dataAdapter = new SqlDataAdapter(cmd))
                    {
                        var ds = new DataSet();

                        await Task.Run(() => dataAdapter.Fill(ds));

                        cmd.Parameters.Clear();
                        conn.Close();

                        return ds;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static async Task<int> ExecuteNonQueryAsync(string storedProcedure, SqlParameter[] dsParameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["OpFlowConnection"].ConnectionString))
            using (var cmd = new SqlCommand(storedProcedure, conn) { CommandType = commandType })
            { 
                cmd.Parameters.AddRange(dsParameters);

                await conn.OpenAsync();

                var result = await cmd.ExecuteNonQueryAsync();

                cmd.Parameters.Clear();
                conn.Close();

                return result;
            }
        }

        public static async Task<List<TrayRationalization>> GetTrayRationalization(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetTrayRationalization", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<TrayRationalization>();

            return result;
        }

        public static async Task<List<ImportType>> GetImportTypes(int providerId, int locationId)
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

        public static async Task<List<ImportDefinition>> GetImportDefinition(int importTypeId, int providerId, int locationId)
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

        public static async Task<List<ImportMessage>> GetImportMessages(int importLogId, int providerId, int locationId)
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

        public static async Task<OverviewScreen> GetCaseOverview(int providerId, int locationId, DateTime beginDate, DateTime endDate, int? specialtyId, int? bundleId)
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

        public static async Task<List<ImportLog>> GetImportLog(int importTypeId, int providerId, int locationId)
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

        public static async Task<List<Messaging>> GetMessaging(int userId, int? surgeryId, int? caseGroupId, int? recipientId, int providerId, int locationId)
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

        public static async Task<List<MessagingGroup>> GetMessageGroups(int userId, DateTime? startDate, DateTime? endDate, int providerId, int locationId)
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

        public static async Task SendMessage(int userId, int providerId, int locationId,
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

        public static List<string> CalculateSurgeryMessageRecipients(
            int surgeryId, int providerId, int locationId)
        {
            return new List<string>()
            {
                "ben@opflowtech.com"
            };
        }

        public static async Task AcknowledgeMessage(int userId, int providerId, int locationId, int messageId, bool hideMessages)
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

        public static async Task DeletePrivateConversation(int communicationUserId, int userId, int providerId, int locationId)
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

        public static async Task<List<ItemMaster>> GetItems(string itemType, int? trayId, bool? countNeeded, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("item_type", itemType ?? (object)DBNull.Value),
                new SqlParameter("count_needed", countNeeded ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemMaster>();

            return result;
        }

        public static async Task<List<ItemMaster>> GetTrayItems(int trayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("tray_item_id", trayId)
            };
            var dsItems = await ExecuteCommandAsync("GetTrayItems", parameters);

            return dsItems.Tables[0].DataTableToList<ItemMaster>();
        }

        public static async Task<List<CardDetail>> GetCardData(int cardId, int providerId, int locationId)
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

        public static async Task<CardUsageHistory> GetCardUsageHistory(int surgeryId, int providerId, int locationId)
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

        public static async Task<List<CardItem>> GetCardItems(int cardId, int providerId, int locationId)
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

        public static async Task<List<SurgeryCardItem>> GetSurgeryCardItems(int surgeryId, int providerId, int locationId)
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

        public static async Task<List<SurgeryInstrumentCount>> GetCardAdditionalItems(int cardId, int providerId, int locationId)
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

        public static async Task<List<Procedure>> GetCardProcedures(int cardId, int providerId, int locationId)
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

        public static async Task<List<CardItemCount>> GetSurgeryCardItemCounts(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSurgeryCardItemCounts", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardItemCount>();

            return result;
        }

        public static async Task<List<SurgeryTrayOpen>> GetSurgeryTrayOpens(int surgeryId, int providerId, int locationId)
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

        public static async Task<int> UpdateSurgeryTrayOpens(int surgeryId, int providerId, int locationId, int trayId, bool trayOpened)
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

        public static async Task<List<Card>> GetCardSurgeryDelays(int surgeryId, int providerId, int locationId)
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

        public static async Task<List<Card>> GetCardList(int? userId, int? procedureId, int? bundleId, bool defaultCardOnly, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId ?? (object)DBNull.Value),
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

        public static async Task<List<CardItemFeedback>> GetCardFeedback(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardFeedback", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardItemFeedback>();

            return result;
        }

        public static async Task<List<SurgeryCard>> GetSurgeryCardList(int surgeryId, int providerId, int locationId)
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

        public static async Task<List<SurgeryFlow>> GetSurgeryFlowList(int surgeryId, int providerId, int locationId)
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

        public static async Task<List<Card>> GetCardCountAvgClose(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardCountAVGClose", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static async Task<List<Card>> GetCardSurgeryOpens(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardSurgeryCountOpen", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static async Task<List<Card>> GetCardItemPulls(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardItemPulledCount", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static async Task<List<Card>> GetCardSurgeryCloses(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetCardSurgeryCountClose", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static async Task<List<Card>> GetProviderCardChecklist(int providerId, int locationId, int cardId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetProviderCardChecklist", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static async Task<List<SurgeryUser>> GetCardUsers(int cardId, int providerId, int locationId, int? typeId)
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

        public static async Task<User> GetUser(int providerId, int locationId, Guid userAuthId)
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

        public static async Task<User> GetUser(int providerId, int locationId, int userId)
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

        public static async Task<List<User>> SearchUsers(string searchString, int? roleId, int? specialtyId, int providerId, int locationId)
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

        public static async Task<List<Role>> GetRoles(int providerId, int locationId)
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

        public static async Task<UserSecurity> GetSecureUser(Guid? userAuthId, int? userId = null, string email = null)
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

        public static async Task<int> CheckInUser(User user, int surgeryId)
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

        public static async Task<int> CheckOutUser(User user, int surgeryId)
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

        public static async Task<int> WorkupReviewed(User user, int surgeryId)
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

        public static async Task<int> CreateUser(Guid userAuthId, int? roleId, int? specialtyId, string firstName, string lastName,
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

        public static async Task<int> UpdateUser(int userId, int? roleId, int? specialtyId, string firstName, string lastName,
            string email, string cellPhone, string initials, string title, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_id", userId),
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
            return await ExecuteNonQueryAsync("UpdateUser", dsParameters);
        }

        public static async Task<int> DeleteUser(int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("user_id", userId)
            };
            return await ExecuteNonQueryAsync("DeleteUser", dsParameters);
        }

        public static async Task<int> AddSurgerySmartPhrase(int surgeryId, int smartPhraseId, int providerId, int locationId)
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

        public static async Task<int> AddFlowSmartPhrase(int flowId, int smartPhraseId, int? stepId, int providerId, int locationId)
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

        public static async Task<int> NewSmartPhrase(string phrase, int categoryId, int stepId, int roleId, int userId, int providerId, int locationId)
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

        public static async Task<int> EditSmartPhrase(int smartPhraseId, string phrase, 
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

        public static async Task<int> DeleteSmartPhrase(int smartPhraseId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("smart_phrase_id", smartPhraseId)
            };
            return await ExecuteNonQueryAsync("DeleteSmartPhrase", dsParameters);
        }

        public static async Task<int> NewFlowImage(int flowId, int stepId, int roleId, string comment, int providerId, int locationId)
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

        public static async Task<int> UpdateFlowImage(int flowImageId, string comments, int stepId, int roleId, int providerId, int locationId)
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

        public static async Task<int> DeleteFlowImage(int flowImageId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_image_id", flowImageId)
            };
            return await ExecuteNonQueryAsync("DeleteFlowImage", dsParameters);
        }

        public static async Task<int> NewSurgeryImage(int surgeryId, int stepId, int roleId, string comment, int providerId, int locationId)
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

        public static async Task<int> UpdateSurgeryImage(int surgeryImageId, string comments, int stepId, int roleId, int providerId, int locationId)
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

        public static async Task<int> DeleteSurgeryImage(int surgeryImageId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_image_id", surgeryImageId)
            };
            return await ExecuteNonQueryAsync("DeleteSurgeryImage", dsParameters);
        }

        public static async Task<int> AddFlowFeedback(int flowId, string feedback, int userId, int providerId, int locationId)
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

        public static async Task<int> DeleteFlowFeedback(int flowFeedbackId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_feedback_id", flowFeedbackId)
            };
            return await ExecuteNonQueryAsync("DeleteFlowFeedback", dsParameters);
        }

        public static async Task<int> UpdateFlowPhrase(int flowId, int smartPhraseId, string comments, int stepId, int roleId, int providerId, int locationId)
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

        public static async Task<int> DeleteFlowPhrase(int flowId, int smartPhraseId, int providerId, int locationId)
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

        public static async Task<int> UpdateSurgeryPhrase(int surgeryId, int surgeryPhraseId, string comments, int stepId, int roleId, int providerId, int locationId)
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

        public static async Task<int> DeleteSurgeryPhrase(int surgeryId, int smartPhraseId, int providerId, int locationId)
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

        public static async Task<int> NewSurgeonNote(string phrase, int flowId, int stepId, int roleId, int providerId, int locationId)
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

        public static async Task<int> UpdateSurgeonNote(int surgeonNoteId, string comments, int stepId, int roleId, int providerId, int locationId)
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

        public static async Task<int> DeleteSurgeonNote(int surgeonNoteId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgeon_note_id", surgeonNoteId)
            };
            return await ExecuteNonQueryAsync("DeleteSurgeonNote", dsParameters);
        }

        public static async Task<int> SurgeryPhraseDebrief(int surgeryPhraseId, int stepId, int roleId, int providerId, int locationId)
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


        public static async Task<int> SurgeryUpdateCaseNotes(int surgeryId, string caseNotes, int providerId, int locationId)
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
        public static async Task<int> AssignCardToCase(int cardId, int surgeryId, int providerId, int locationId)
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

        public static async Task<int> AssignFlowToCase(int flowId, int surgeryId, int providerId, int locationId)
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

        public static async Task<int> AssignRoomSetupToCase(int roomSetupId, int surgeryId, int providerId, int locationId)
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

        public static async Task<int> AssignFlowToCard(int flowId, int cardId, int providerId, int locationId)
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

        public static async Task<int> AssignRoomSetupToCard(int roomSetupId, int cardId, int providerId, int locationId)
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

        public static async Task<int> AssignUserToCard(int cardId, int userId, int providerId, int locationId)
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

        public static async Task<int> RemoveUserFromCard(int cardId, int userId, int providerId, int locationId)
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

        public static async Task<int> DeleteCardItem(int cardId, int itemId, int providerId, int locationId)
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

        public static async Task<int> UpdateCardItem(int cardId, int itemId, int qtyOpen, int qtyHold, int providerId, int locationId)
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

        public static async Task<int> UpdateCardProcedure(int cardId, int procedureId, int providerId, int locationId)
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

        public static async Task<int> DeleteCardProcedure(int cardId, int procedureId, int providerId, int locationId)
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

        public static async Task<int> InsertCard(string description, int ownerUserId, int? specialtyId, int? procedureId, int? templateFlowId, int? templateRoomId, int? bundleId, string bundleFlag,
            bool? defaultFlag, bool? specialtyDefaultFlag, int providerId, int locationId)
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
                new SqlParameter("bundle_flag", bundleFlag ?? (object)DBNull.Value),
                new SqlParameter("default_flag", defaultFlag ?? (object)DBNull.Value),
                new SqlParameter("specialty_default_flag", specialtyDefaultFlag ?? (object)DBNull.Value)
            };
            var insert = await ExecuteCommandAsync("NewCard", dsParameters);
            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.First().Identifier;
        }

        public static async Task<int> InsertCardItemFromStage(int cardId, int providerId, int locationId, string procedure, string surgeon)
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

        public static async Task<int> UpdateCard(int cardId, string description, int ownerUserId, int? specialtyId, int? procedureId, int? templateFlowId, int? templateRoomId, int? bundleId, string bundleFlag,
            bool? defaultFlag, bool? specialtyDefaultFlag, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("description", description ?? (object)DBNull.Value),
                new SqlParameter("owner_user_id", ownerUserId),
                new SqlParameter("user_id", userId),
                new SqlParameter("procedure_id", procedureId ?? (object)DBNull.Value),
                new SqlParameter("template_flow_id", templateFlowId ?? (object)DBNull.Value),
                new SqlParameter("template_room_setup_id", templateRoomId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("bundle_id", bundleId ?? (object)DBNull.Value),
                new SqlParameter("bundle_flag", bundleFlag ?? (object)DBNull.Value),
                new SqlParameter("default_flag", defaultFlag ?? (object)DBNull.Value),
                new SqlParameter("specialty_default_flag", specialtyDefaultFlag ?? (object)DBNull.Value)
            };
            return await ExecuteNonQueryAsync("UpdateCard", dsParameters);
        }

        public static async Task<int> InitializeCard(int cardId, int providerId, int locationId)
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


        public static async Task<int> DeleteCard(int cardId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId)
            };
            return await ExecuteNonQueryAsync("DeleteCard", dsParameters);
        }

        public static async Task<int> UpdateCardQuantity(int cardId, CardQuantityEdit quantity, int providerId, int locationId)
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

        public static async Task<int> UpdateCardQuantityRequest(int cardId, CardQuantityEdit quantity, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("item_id", quantity.ItemID),
                new SqlParameter("qty_open", quantity.OpenQty),
                new SqlParameter("qty_hold", quantity.HoldQty)
            };
            return await ExecuteNonQueryAsync("UpdateCardItemQtyRequest", dsParameters);
        }

        public static async Task<int> UpdateSurgeryItemQuantity(int surgeryId, CardQuantityEdit quantity, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("item_id", quantity.ItemID),
                new SqlParameter("qty_open", quantity.OpenQty),
                new SqlParameter("qty_hold", quantity.HoldQty)
            };
            return await ExecuteNonQueryAsync("UpdateSurgeryCardItemQty", dsParameters);
        }

        public static async Task<int> CreateSurgery(SurgeryPost surgery, int providerId, int locationId, int patientId, int caseId, 
            int? defaultCardId, int? defaultFlowId, int? defaultRoomId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
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
                new SqlParameter("laterality_id", surgery.LateralityID ?? (object)DBNull.Value)
            };
            var insert = await ExecuteCommandAsync("InsertSurgery", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public static async Task<int> AddCustomSurgeryItem(int surgeryId, int itemId, int quantity, int providerId, int locationId)
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

        public static async Task<int> AddSurgeryUser(int surgeryId, int userId, int providerId, int locationId)
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

        public static async Task<int> DeleteSurgeryUser(int surgeryId, int userId, int providerId, int locationId)
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

        public static async Task<int> AddSurgeryProcedure(int surgeryId, int procedureId, int providerId, int locationId)
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

        public static async Task<int> UpdateSurgeryProcedure(int surgeryId, string cptCode, string performedFlag, int providerId, int locationId)
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

        public static async Task<int> DeleteSurgeryProcedure(int surgeryId, string cptCode, int providerId, int locationId)
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

        public static async Task<int> UpdateSurgeryHeaderCounts(int surgeryId, int sharpCount, int needleCount, int lapCount, int specimenCount, int providerId, int locationId)
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

        public static async Task<int> UpdateStaffChange(int surgeryId, string staffChange, int providerId, int locationId)
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

        public static async Task<int> AddCustomSurgeryTrayItem(int surgeryId, int trayId, int itemId, int quantity, int providerId, int locationId)
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

        public static async Task<int> UpdateSurgeryCount(int surgeryId, List<SurgeryCountItemPost> itemUsage, int providerId, int locationId)
        {
            var usageSummary = GetUsageSummary(itemUsage);
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("usage_summary", usageSummary ?? (object)DBNull.Value)
            };
            var update = await ExecuteNonQueryAsync("UpdateSurgeryCount", dsParameters);

            return update;
        }
        public static async Task<int> UpdateSurgeryInstrumentCount(int surgeryId, List<SurgeryCountItemPost> instrumentUsage, int providerId, int locationId)
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



        private static string GetUsageSummary(List<SurgeryCountItemPost> countData)
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
            }

            return table.OuterXml;
        }

        private static void AddColumn(XmlDocument doc, XmlElement row, object value)
        {
            var col = doc.CreateElement("col");
            col.InnerText = String.Format("{0}", value);
            row.AppendChild(col);
        }

        public static async Task<int> StartSurgery(int surgeryId, int providerId, int locationId, DateTime startTime)
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
        public static async Task<FlowStepResult> SurgeryMoveNextStep(int surgeryId, int providerId, int locationId, DateTime startTime)
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

        public static async Task<int> SurgeryToggleDelay(int surgeryId, int providerId, int locationId, DateTime? startTime, DateTime? endTime, int? delayReasonId)
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

        public static async Task<int> SurgeryToggleDelayCustom(int surgeryId, int providerId, int locationId, DateTime? startTime, DateTime? endTime, string customReason)
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

        public static async Task<int> SurgeryReviewComplete(int surgeryId, DateTime reviewComplete, int userId, int providerId, int locationId)
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

        public static async Task<int> SurgeryEditProperties(int surgeryId, int roomId, DateTime surgeryScheduleDate, int providerId, int locationId)
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

        public static async Task<int> CreateCase(int patientId, int? userId, int? specialtyId, int providerId, int locationId, string caseNbr)
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

        public static async Task<List<Room>> GetRooms(int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetRooms", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<Room>();

            return result;
        }

        public static async Task<List<RoomType>> GetRoomTypes(int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = await ExecuteCommandAsync("GetRoomTypes", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<RoomType>();

            return result;
        }

        public static async Task<List<RoomGroup>> GetRoomGroups(int providerId, int locationId)
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
        public static async Task<RoomSetup> GetRoomSetup(int roomSetupId, int providerId, int locationId)
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

        public static async Task<List<RoomSetup>> GetRoomSetups(int? roomSetupId, int providerId, int locationId)
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

        public static async Task<List<PatientPosition>> GetPatientPositions(int providerId, int locationId)
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

        public static async Task<List<BedOrientation>> GetBedOrientations(int providerId, int locationId)
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

        public static async Task<List<Laterality>> GetLateralities(int providerId, int locationId)
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

        public static async Task<List<SmartPhrase>> GetSmartPhrases(int? categoryId, int? specialtyId, int? userId, int providerId, int locationId)
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
        
        public static async Task<List<SmartPhraseCategory>> GetSmartPhraseCategories(int providerId, int locationId)
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

        public static async Task<List<FlowPhrase>> GetFlowPhrases(int flowId, int providerId, int locationId)
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

        public static async Task<List<SurgeryPhrase>> GetSurgeryPhrases(int surgeryId, int providerId, int locationId)
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

        public static async Task<List<FlowImage>> GetFlowImages(int flowId, int providerId, int locationId)
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

        public static async Task<List<SurgeryImage>> GetSurgeryImages(int surgeryId, int providerId, int locationId)
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

        public static async Task<List<RoomSummary>> GetSurgeryRoomSummary(int surgeryId, int providerId, int locationId)
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

        public static async Task<List<RoomSummary>> GetSurgeryRoomOverview(
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

        public static async Task<List<SurgeryDelay>> GetFlowSurgeryDelays(int flowId, int providerId, int locationId)
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

        public static async Task<List<FlowSurgeonNote>> GetSurgeonNotes(int flowId, int providerId, int locationId)
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

        public static async Task<int> UpdateRoom(int roomId, string description, int typeId, int groupId, int providerId, int locationId)
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

        public static async Task<int> InsertRoom(string description, int typeId, int groupId, int providerId, int locationId)
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

        public static async Task<int> DeleteRoom(int roomId, int providerId, int locationId)
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

        public static async Task<int> UpdateRoomSetup(int roomSetupId, int providerId, int locationId, RoomSetup newRoomSetup)
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

        public static async Task<int> DeleteRoomSetup(int roomSetupId, int providerId, int locationId)
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

        public static async Task<int> CreateRoomSetup(RoomSetup roomSetup, int providerId, int locationId)
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

        private static async Task SyncRoomSetupAttributes(int roomSetupId, int providerId, int locationId, RoomSetup newRoomSetup, RoomSetup currentRoomSetup)
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

        public static async Task<int> NewRoomSetupImage(int roomSetupId, string label, int providerId, int locationId)
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

        public static async Task<int> UpdateRoomSetupImage(int roomSetupImageId, string label, int providerId, int locationId)
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

        public static async Task<int> DeleteRoomSetupImage(int roomSetupImageId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("room_setup_image_id", roomSetupImageId)
            };
            return await ExecuteNonQueryAsync("DeleteRoomSetupImage", dsParameters);
        }

        public static async Task<int> InsertRoomSetupEquipment(int roomSetupId, RoomSetupEquipment roomSetupEquipment, int providerId, int locationId)
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

        public static async Task<int> InsertRoomSetupItem(int roomSetupId, RoomSetupItem roomSetupItem, int providerId, int locationId)
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

        public static async Task<int> InsertRoomSetupStaffPosition(int roomSetupId, RoomSetupStaffPosition roomSetupStaffPosition, int providerId, int locationId)
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
        public static async Task<int> UpdateRoomSetupEquipment(RoomSetupEquipment roomSetupEquipment, int providerId, int locationId)
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

        public static async Task<int> UpdateRoomSetupItem(RoomSetupItem roomSetupItem, int providerId, int locationId)
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

        public static async Task<int> UpdateRoomSetupStaffPosition(RoomSetupStaffPosition roomSetupStaffPosition, int providerId, int locationId)
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
        public static async Task<int> DeleteRoomSetupEquipment(int roomSetupEquipmentId, int providerId, int locationId)
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

        public static async Task<int> DeleteRoomSetupItem(int roomSetupItemId, int providerId, int locationId)
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

        public static async Task<int> DeleteRoomSetupStaffPosition(RoomSetupStaffPosition roomSetupStaffPosition, int providerId, int locationId)
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

        public static async Task<PatientSurgery> GetSurgery(int surgeryId, int providerId, int locationId)
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

        public static async Task<Surgery> GetCase(int caseId, int providerId, int locationId)
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

        public static async Task<List<SurgerySearchResult>> SearchCases(int? userId, int? surgeonUserId, 
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

        public static async Task<List<SurgerySearchResult>> GetCaseNbr(string caseNbr, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("case_nbr", caseNbr),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsSchedules = await ExecuteCommandAsync("SearchCaseNbr", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgerySearchResult>();

            return result;
        }

        public static async Task<List<SurgerySearchResult>> GetSurgeonCases(int userId, DateTime begDate, DateTime endDate, int providerId, int locationId)
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

        public static async Task<List<SurgerySearchResult>> GetRoomCases(int roomId, DateTime begDate, DateTime endDate, int providerId, int locationId)
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

        public static async Task<List<SurgerySchedule>> GetScheduledSurgeries(int? userID, int providerId, int locationId,
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

        public static async Task<List<Surgery>> GetSurgeryAlerts(int surgeryId, int providerId, int locationId)
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

        public static async Task <List<SurgeryDelayReason>> GetSurgeryDelayReasons(int providerId, int locationId)
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

        public static async Task<List<SurgeryUser>> GetSurgeryUsers(int surgeryId, int providerId, int locationId)
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

        public static async Task<List<SurgeryVendorRep>> GetSurgeryVendorReps(int surgeryId, int locationId, int providerId)
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

        public static async Task<CardFlowRoom> GetBundleDefaultCardFlowRoom(int bundleId, int userId, int providerId, int locationId)
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

        public static async Task<List<Surgeon>> GetImportSurgeons(int providerId, int locationId)
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

        public static async Task<List<Procedure>> GetImportProcedures(string importSurgeon, int providerId, int locationId)
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

        public static async Task<List<CardFlowRoom>> GetProcedureDefaultCardFlowRoom(int providerId, int locationId, string cptCode)
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

        public static async Task<CardFlowRoom> GetImportDefaultCardFlowRoom(int providerId, int locationId, int ownerUserId, string procedureCard)
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

        public static async Task<List<CardFlowRoom>> GetSpecialtyProcedureDefaultCardFlowRoom(int providerId, int locationId, string cptCode)
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

        public static async Task<List<CardFlowRoom>> GetMultipleProceduresDefaultCardFlowRoom(int providerId, int locationId, int specialtyId, string cptCodes)
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

        public static async Task<List<CardFlowRoom>> GetSpecialtyMultipleProceduresDefaultCardFlowRoom(int providerId, int locationId, int specialtyId, string cptCodes)
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

        public static async Task<List<CardBundle>> GetBundles(int? specialtyId, int providerId, int locationId)
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

        public static async Task<List<BundleProcedure>> GetBundleProcedures(int bundleId, int providerId, int locationId)
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

        public static async Task<int> NewBundle(string description, int specialtyId, List<int> procedures, int providerId, int locationId)
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

        public static async Task<int> UpdateBundle(int bundleId, string description, int specialtyId, List<int> procedures, int providerId, int locationId)
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

        private static async Task<int> UpdateBundleProcedures(int bundleId, List<int> procedures, int providerId, int locationId)
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

        public static async Task<int> DeleteBundle(int bundleId, int providerId, int locationId)
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

        public static async Task<List<BundleProcedure>> GetSurgeryProcedures(int surgeryId, int providerId, int locationId)
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

        public static async Task<List<Procedure>> GetProcedures(int? specialtyId, int providerId, int locationId)
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

        public static async Task<List<Specialty>> GetSpecialties(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = await ExecuteCommandAsync("GetSpecialtyByLocation", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Specialty>();

            return result;
        }

        public static async Task<int> UpdateSpecialty(int specialtyId, string name, string description, int providerId, int locationId)
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

        public static async Task<int> InsertSpecialty(string name, string description, int providerId, int locationId)
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

        public static async Task<int> DeleteSpecialty(int specialtyId, int providerId, int locationId)
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

        public static async Task<List<Surgeon>> GetSurgeons(int? specialtyId, int providerId, int locationId)
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

        public static async Task<Flow> GetFlow(int flowId, int providerId, int locationId)
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

        public static async Task<List<Flow>> GetCardFlowList(int cardId, int providerId, int locationId)
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

        public static async Task<List<FlowStepTiming>> GetFlowTimings(int flowId, int providerId, int locationId)
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

        public static async Task<List<FlowStepSurgeryTiming>> GetFlowSurgeryTimings(int surgeryId, int providerId, int locationId)
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

        public static async Task<List<FlowStepInstructionResult>> GetFlowInstructions(int flowId, int? surgeryId, int providerId, int locationId)
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

        public static async Task<List<FlowStep>> GetFlowComments(int flowId, int surgeryId, int providerId, int locationId)
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

        public static async Task<List<FlowMessaging>> GetFlowMessaging(int flowId, int providerId, int locationId, int stepId)
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

        public static async Task<List<FlowContent>> GetFlowContent(int flowId, int surgeryId, int providerId, int locationId)
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

        public static async Task<List<FlowNotification>> GetFlowNotifications(int flowId, int? stepId, int providerId, int locationId)
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

        public static async Task<List<Step>> GetSteps(int providerId, int locationId)
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

        public static async Task<List<FlowFeedback>> GetFlowFeedback(int flowId, int providerId, int locationId)
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

        public static async Task<int> DeleteFlowStep(int flowId, int providerId, int locationId)
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

        public static async Task<int> InsertFlowStep(int flowId, int stepId, int stepSequence, decimal duration, int providerId, int locationId)
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

        public static async Task<int> UpdateSurgeryStep(int stepId, string stepDescription, string notificationType, bool stepTiming, int providerId, int locationId)
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

        public static async Task<int> AddSurgeryStep(string stepDescription, string notificationType, bool stepTiming, int providerId, int locationId)
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

        public static async Task<int> DeleteSurgery(int surgeryId, int providerId, int locationId)
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

        public static async Task<int> DeleteSurgeryStep(int stepId, int providerId, int locationId)
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

        public static async Task<int> InsertFlowNotification(int flowId, int stepId, int notificationType, string message, 
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

        public static async Task<int> EditFlowNotification(int flowNotificationId, string message, int stepId,
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

        public static async Task<int> DeleteFlowNotification(int flowNotificationId, int providerId, int locationId)
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

        public static async Task<int> NewFlow(int cardId, int roomSetupId, string description, int userId, bool defaultFlow, int providerId, int locationId)
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

        public static async Task<int> UpdateFlow(int flowId, int cardId, int roomSetupId, string description, int userId, bool defaultFlow, int providerId, int locationId)
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

        public static async Task<int> DeleteFlow(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            return await ExecuteNonQueryAsync("DeleteFlow", parameters);
        }

        public static async Task<ImportResult> InsertStagingData(int providerId, int locationId, int? secureId, IImportData sourceData, FileParserRelations relations)
        {
            var result = new ImportResult();

            if (sourceData is ItemImport item)
            {
                var parameters = new[]
                {
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId),
                    new SqlParameter("location", item.Location),
                    new SqlParameter("location_name", item.LocationName),
                    new SqlParameter("from_loc", item.FromLoc),
                    new SqlParameter("bin_seq", item.BinSeq),
                    new SqlParameter("bin", item.Bin),
                    new SqlParameter("item_nbr", item.ItemNbr),
                    new SqlParameter("description", item.Desc),
                    new SqlParameter("manu_name", item.ManuName),
                    new SqlParameter("mfg_nbr", item.MfgNbr),
                    new SqlParameter("par_level", item.ParLevel),
                    new SqlParameter("uom", item.UOM),
                    new SqlParameter("item_cost", item.ItemCost),
                    new SqlParameter("inventory_value", item.InventoryValue)
                };

                result.Identity = await ExecuteNonQueryAsync(@"INSERT stag_item (provider_id, location_id, location, location_name, from_loc, bin_seq, 
                    bin, item_nbr, description, manu_name, mfg_nbr, par_level, uom, item_cost, inventory_value)
                VALUES (@provider_id, @location_id, @location, @location_name, @from_loc, @bin_seq,
                    @bin, @item_nbr, @description, @manu_name, @mfg_nbr, @par_level, @uom, @item_cost, @inventory_value)", parameters, CommandType.Text);

            }
            else if (sourceData is CardImport card)
            {
                var parameters = new[]
                {
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId),
                    new SqlParameter("location", card.Location),
                    new SqlParameter("surgeon", card.Surgeon),
                    new SqlParameter("preference_card_name", card.PreferenceCardName),
                    new SqlParameter("type", card.Type),
                    new SqlParameter("lawson_id", card.LawsonID),
                    new SqlParameter("catalog_nbr", card.CatalogNbr),
                    new SqlParameter("supply_description", card.SupplyDescription),
                    new SqlParameter("manufacturer", card.Manufacturer),
                    new SqlParameter("open_amt", card.OpenAmt),
                    new SqlParameter("prn_required", card.PrnRequired),
                    new SqlParameter("cost_per_unit_ot", card.CostPerUnitOt),
                    new SqlParameter("dosage", card.Dosage),
                    new SqlParameter("unit", card.Unit)
                };

                result.Identity = await ExecuteNonQueryAsync(@"INSERT stag_card (provider_id, location_id, location, surgeon, preference_card_name, type,
                    lawson_id, catalog_nbr, supply_description, manufacturer, open_amt, prn_required, cost_per_unit_ot, dosage, unit)
                        VALUES (@provider_id, @location_id, @location, @surgeon, @preference_card_name, @type,
                    @lawson_id, @catalog_nbr, @supply_description, @manufacturer, @open_amt, @prn_required, @cost_per_unit_ot, @dosage, @unit)", parameters, CommandType.Text);
            }
            else if (sourceData is TrayImport tray)
            {
                var parameters = new[]
                {
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId),
                    new SqlParameter("customer", tray.Customer),
                    new SqlParameter("tray_id", tray.TrayID),
                    new SqlParameter("tray_name", tray.TrayName),
                    new SqlParameter("instrument_name", tray.InstrumentName),
                    new SqlParameter("quantity", tray.Quantity),
                    new SqlParameter("manufacturer", tray.Manufacturer)
                };

                result.Identity = await ExecuteNonQueryAsync(@"INSERT stag_tray (provider_id, location_id, customer, tray_id, tray_name, instrument_name, quantity, manufacturer)
                VALUES (@provider_id, @location_id, @customer, @tray_id, @tray_name, @instrument_name, @quantity, @manufacturer)", parameters, CommandType.Text);
            }
            else if(sourceData is ScheduleImport schedule)
            {
                var surgery = new SurgeryPost()
                {
                    BundleID = null,
                    CaseNbr = schedule.CaseNbr,
                    CptCode = schedule.CptCode,
                    LateralityID = null,
                    ScheduleDate = schedule.ScheduleDateTime,
                };
                
                var room = relations.Rooms.FirstOrDefault(r => r.RoomDescription == schedule.Room);
                var surgeon = relations.Surgeons.FirstOrDefault(r => r.LastName == schedule.PrimarySurgeon.LastName && r.FirstName == schedule.PrimarySurgeon.FirstName);

                surgery.RoomID = room?.RoomID;
                surgery.SurgeonUserID = surgeon?.UserID;

                if (surgery.RoomID == null)
                {
                    // Only log when the room is non-empty?...
                    if (!string.IsNullOrEmpty(schedule.Room) && schedule.Room != "ORW LITHO")
                        result.Messages.Add("Unable to find room: " + schedule.Room);
                    return result;
                }
                if (surgery.SurgeonUserID == null)
                {
                    result.Messages.Add("Unable to find primary surgeon: " + schedule.Surgeon);
                    return result;
                }

                var procedureCards = await DetermineCards(surgery, schedule, relations, providerId, locationId);
                
                if (!procedureCards.Any())
                {
                    result.Messages.Add("Unable to find card: " + schedule.ProcedurePreferenceCards);
                    return result;
                }

                var cardFlowRoom = await AggregateCards(surgery.SurgeonUserID.Value, procedureCards, providerId, locationId);

                if (cardFlowRoom == null)
                {
                    result.Messages.Add("Unable to find card: " + schedule.ProcedurePreferenceCards);
                    return result;
                }

                var caseId = await CreateCase(secureId ?? -1, surgery.SurgeonUserID, surgery.SpecialtyID, providerId,
                    locationId, surgery.CaseNbr);

                result.Identity = await CreateSurgery(surgery, providerId, locationId, secureId ?? -1, caseId,
                    cardFlowRoom?.CardID, cardFlowRoom?.TemplateFlowID, cardFlowRoom?.TemplateRoomSetupID);

                foreach (var secondarySurgeon in schedule.SecondarySurgeons)
                {
                    surgeon = relations.Surgeons.FirstOrDefault(r => r.LastName == secondarySurgeon.LastName && r.FirstName == secondarySurgeon.FirstName);

                    if (surgeon == null)
                    {
                        result.Messages.Add("Unable to find secondary surgeons: " + schedule.Surgeon);
                    }
                    else
                    {
                        await AddSurgeryUser(result.Identity, surgeon.UserID, providerId, locationId);
                    }
                }
            }

            return result;
        }

        private static async Task<CardFlowRoom> AggregateCards(int ownerUserId, List<CardFlowRoom> procedureCards, int providerId, int locationId)
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

        private static CardFlowRoom AnalyzeSources(List<CardSource> potentialSources, List<CardSource> desiredSources)
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

        private static async Task<List<CardFlowRoom>> DetermineCards(SurgeryPost surgery, ScheduleImport schedule, FileParserRelations relations, int providerId, int locationId)
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

        private static async Task<CardFlowRoom> CheckCard(string cardName, ImportCard procedureCard, SurgeryPost surgery, ScheduleImport schedule, FileParserRelations relations, int providerId, int locationId)
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

        public static async Task<int> InsertImportLog(int providerId, int locationId, int importTypeId, int userId, int recordCount, string filename)
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

        public static async Task InsertImportMessage(int providerId, int locationId, int logId, string logType, string logMessage, string logMrn, DateTime? logServiceDate)
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