using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using OpFlow.Data;
using OpFlow.Data.Administration;
using OpFlow.Data.Debrief;

namespace OpFlow.Service.DataAccess
{
    public static class SqlHelper 
    {
        private static DataSet ExecuteCommand(string storedProcedure, SqlParameter[] dsParameters = null)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["OpFlowConnection"].ConnectionString);
            var cmd = new SqlCommand(storedProcedure, conn) {CommandType = CommandType.StoredProcedure};

            cmd.Parameters.AddRange(dsParameters);

            conn.Open();

            using (var dataAdapter = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                dataAdapter.Fill(ds);

                cmd.Parameters.Clear();
                conn.Close();

                return ds;
            }
        }

        private static int ExecuteNonQuery(string storedProcedure, SqlParameter[] dsParameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["OpFlowConnection"].ConnectionString);
            var cmd = new SqlCommand(storedProcedure, conn) { CommandType = commandType };

            cmd.Parameters.AddRange(dsParameters);

            conn.Open();

            var result = -1;

            using (var dataAdapter = new SqlDataAdapter(cmd))
            {
                result = cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();
                conn.Close();

                return result;
            }
        }

        public static List<ImportType> GetImportTypes(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetImportList", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ImportType>();

            return result;
        }

        public static List<ImportDefinition> GetImportDefinition(int importTypeId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("import_id", importTypeId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetImportDefinition", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ImportDefinition>();

            return result;
        }

        public static List<ImportLog> GetImportLog(int importTypeId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("import_id", importTypeId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetImportLog", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ImportLog>();

            return result;
        }

        public static List<Messaging> GetMessaging(int userId, int? surgeryId, int? caseGroupId, int? recipientId, int providerId, int locationId)
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
            var dsSchedules = ExecuteCommand("GetMessaging", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Messaging>();

            return result;
        }

        public static List<MessagingGroup> GetMessageGroups(int userId, DateTime? startDate, DateTime? endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("start_date", startDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetMessagingGroups", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<MessagingGroup>();

            return result;
        }

        public static void SendMessage(int userId, int providerId, int locationId,
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
            var result = ExecuteNonQuery("InsertMessage", parameters);
        }

        public static List<string> CalculateSurgeryMessageRecipients(
            int surgeryId, int providerId, int locationId)
        {
            return new List<string>()
            {
                "ben@opflowtech.com"
            };
        }

        public static void AcknowledgeMessage(int userId, int providerId, int locationId, int messageId, bool hideMessages)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("message_id", messageId),
                new SqlParameter("hide_messages", hideMessages)
            };
            var result = ExecuteNonQuery("InsertMessageAcknowledgement", parameters);
        }

        public static void DeletePrivateConversation(int communicationUserId, int userId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("communication_user_id", communicationUserId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = ExecuteNonQuery("DeletePrivateConversation", parameters);
        }

        public static List<ItemMaster> GetItems(string itemType, int? trayId, bool? countNeeded, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("item_type", itemType ?? (object)DBNull.Value),
                new SqlParameter("count_needed", countNeeded ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<ItemMaster>();

            return result;
        }

        public static List<ItemMaster> GetTrayItems(int trayId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("tray_item_id", trayId)
            };
            var dsItems = ExecuteCommand("GetTrayItems", parameters);

            return dsItems.Tables[0].DataTableToList<ItemMaster>();
        }

        public static List<CardDetail> GetCardData(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardData", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardDetail>();

            return result;
        }

        public static List<CardItem> GetCardItems(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardItem>();

            return result;
        }

        public static List<SurgeryInstrumentCount> GetCardAdditionalItems(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardAdditionalItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryInstrumentCount>();

            return result;
        }

        public static List<Procedure> GetCardProcedures(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardProcedures", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Procedure>();

            return result;
        }

        public static List<CardItemCount> GetSurgeryCardItemCounts(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSurgeryCardItemCounts", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardItemCount>();

            return result;
        }

        public static List<Card> GetCardSurgeryDelays(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardSurgeryDelayItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static List<Card> GetCardList(int? userId, int? procedureId, int? bundleId, bool defaultCardOnly, int providerId, int locationId)
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
            var dsSchedules = ExecuteCommand("GetCardListbyProcedure", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static List<SurgeryCard> GetSurgeryCardList(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSurgeryCardList", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryCard>();

            var currentCard = result.FirstOrDefault(r => r.CurrentCard);
            if (currentCard != null)
            {
                result.ForEach(r => r.CurrentCostDelta = (currentCard.Cost - r.Cost));
            }

            return result;
        }

        public static List<SurgeryFlow> GetSurgeryFlowList(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSurgeryFlowList", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryFlow>();

            var currentCard = result.FirstOrDefault(r => r.CurrentFlow);
            if (currentCard != null)
            {
                result.ForEach(r => r.CurrentCostDelta = (currentCard.TimeCost - r.TimeCost));
            }

            return result;
        }

        public static List<Card> GetCardCountAvgClose(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardCountAVGClose", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static List<Card> GetCardSurgeryOpens(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardSurgeryCountOpen", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static List<Card> GetCardItemPulls(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardItemPulledCount", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static List<Card> GetCardSurgeryCloses(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardSurgeryCountClose", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static List<Card> GetProviderCardChecklist(int providerId, int locationId, int cardId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetProviderCardChecklist", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static List<SurgeryUser> GetCardUsers(int cardId, int providerId, int locationId, int? typeId)
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
            var dsSchedules = ExecuteCommand(command, parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryUser>();

            return result;

        }

        public static User GetUser(int providerId, int locationId, string username, int? userId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("email", username ?? (object)DBNull.Value),
                new SqlParameter("user_id", userId ?? (object)DBNull.Value),
            };
            var dsSchedules = ExecuteCommand("GetUser", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<User>().FirstOrDefault();

            return result;
        }

        public static List<User> SearchUsers(string searchString, int? roleId, int? specialtyId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("role_id", roleId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("search", searchString ?? (object)DBNull.Value),
            };
            var dsSchedules = ExecuteCommand("SearchUsers", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<User>();

            return result;
        }

        public static UserSecurity GetSecureUser(Guid? userAuthId, int? userId = null, string email = null)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_auth_id", userAuthId ?? (object)DBNull.Value),
                new SqlParameter("email", email ?? (object)DBNull.Value),
                new SqlParameter("user_id", userId ?? (object)DBNull.Value)
            };
            var dsSchedules = ExecuteCommand("GetUserSecurity", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<UserSecurity>();

            return result.FirstOrDefault();
        }

        public static int CheckInUser(User user, int surgeryId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_id", user.UserID),
                new SqlParameter("provider_id", user.ProviderID),
                new SqlParameter("location_id", user.LocationID),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("role_id", user.RoleID)
            };
            return ExecuteNonQuery("CheckinUserToCase", dsParameters);
        }

        public static int CheckOutUser(User user, int surgeryId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_id", user.UserID),
                new SqlParameter("provider_id", user.ProviderID),
                new SqlParameter("location_id", user.LocationID),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("role_id", user.RoleID)
            };
            return ExecuteNonQuery("CheckoutOfCase", dsParameters);
        }

        public static int WorkupReviewed(User user, int surgeryId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_id", user.UserID),
                new SqlParameter("provider_id", user.ProviderID),
                new SqlParameter("location_id", user.LocationID),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("role_id", user.RoleID)
            };
            return ExecuteNonQuery("UserSurgeryWorkupReviewed", dsParameters);
        }

        public static int CreateUser(Guid userAuthId, int roleId, int specialtyId, string firstName, string lastName,
            string email, string cellPhone, string initials, string title, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("user_auth_id", userAuthId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("first_name", firstName),
                new SqlParameter("last_name", lastName),
                new SqlParameter("email", email),
                new SqlParameter("cell_phone", cellPhone),
                new SqlParameter("initials", initials),
                new SqlParameter("title", title)
            };
            var dsResult = ExecuteCommand("InsertUser", dsParameters);

            var result = dsResult.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public static int UpdateUser(int userId, int? roleId, int? specialtyId, string firstName, string lastName,
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
            return ExecuteNonQuery("UpdateUser", dsParameters);
        }

        public static int DeleteUser(int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("user_id", userId)
            };
            return ExecuteNonQuery("DeleteUser", dsParameters);
        }

        public static int AddSurgerySmartPhrase(int surgeryId, int smartPhraseId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("smart_phrase_id", smartPhraseId)
            };
            return ExecuteNonQuery("InsertSurgeryPhrase", dsParameters);
        }

        public static int AddFlowSmartPhrase(int flowId, int smartPhraseId, int? stepId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_id", flowId),
                new SqlParameter("step_id", stepId ?? (object)DBNull.Value),
                new SqlParameter("smart_phrase_id", smartPhraseId)
            };
            return ExecuteNonQuery("InsertFlowPhrase", dsParameters);
        }

        public static int NewSmartPhrase(string phrase, int categoryId, int stepId, int roleId, int userId, int providerId, int locationId)
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
            var dsResult = ExecuteCommand("InsertPhrase", dsParameters);

            var result = dsResult.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public static int EditSmartPhrase(int smartPhraseId, string phrase, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("smart_phrase_id", smartPhraseId),
                new SqlParameter("phrase", phrase)
            };
            return ExecuteNonQuery("UpdateSmartPhrase", dsParameters);
        }

        public static int DeleteSmartPhrase(int smartPhraseId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("smart_phrase_id", smartPhraseId)
            };
            return ExecuteNonQuery("DeleteSmartPhrase", dsParameters);
        }

        public static int NewFlowImage(int flowId, int stepId, int roleId, string comment, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("image_comment", comment)
            };
            var insert = ExecuteCommand("InsertFlowImage", dsParameters);
            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.First().Identifier;
        }

        public static int UpdateFlowImage(int flowImageId, string comments, int stepId, int roleId, int providerId, int locationId)
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
            return ExecuteNonQuery("UpdateFlowImage", dsParameters);
        }

        public static int DeleteFlowImage(int flowImageId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_image_id", flowImageId)
            };
            return ExecuteNonQuery("DeleteFlowImage", dsParameters);
        }

        public static int NewSurgeryImage(int surgeryId, int stepId, int roleId, string comment, int providerId, int locationId)
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
            var insert = ExecuteCommand("InsertSurgeryImage", dsParameters);
            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.First().Identifier;
        }

        public static int UpdateSurgeryImage(int surgeryImageId, string comments, int stepId, int roleId, int providerId, int locationId)
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
            return ExecuteNonQuery("UpdateSurgeryImage", dsParameters);
        }

        public static int DeleteSurgeryImage(int surgeryImageId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_image_id", surgeryImageId)
            };
            return ExecuteNonQuery("DeleteSurgeryImage", dsParameters);
        }

        public static int AddFlowFeedback(int flowId, string feedback, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_id", flowId),
                new SqlParameter("user_id", userId),
                new SqlParameter("feedback", feedback)
            };
            return ExecuteNonQuery("InsertFlowFeedback", dsParameters);
        }

        public static int DeleteFlowFeedback(int flowFeedbackId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_feedback_id", flowFeedbackId)
            };
            return ExecuteNonQuery("DeleteFlowFeedback", dsParameters);
        }

        public static int UpdateFlowPhrase(int flowId, int smartPhraseId, string comments, int stepId, int roleId, int providerId, int locationId)
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
            return ExecuteNonQuery("UpdateFlowPhrase", dsParameters);
        }

        public static int DeleteFlowPhrase(int flowId, int smartPhraseId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_id", flowId),
                new SqlParameter("smart_phrase_id", smartPhraseId)
            };
            return ExecuteNonQuery("DeleteFlowPhrase", dsParameters);
        }

        public static int UpdateSurgeryPhrase(int surgeryId, int smartPhraseId, string comments, int stepId, int roleId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("smart_phrase_id", smartPhraseId),
                new SqlParameter("step_id", stepId),
                new SqlParameter("role_id", roleId),
                new SqlParameter("comment", comments ?? (object)DBNull.Value)
            };
            return ExecuteNonQuery("UpdateSurgeryPhrase", dsParameters);
        }

        public static int DeleteSurgeryPhrase(int surgeryId, int smartPhraseId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("smart_phrase_id", smartPhraseId)
            };
            return ExecuteNonQuery("DeleteSurgeryPhrase", dsParameters);
        }

        public static int NewSurgeonNote(string phrase, int flowId, int stepId, int roleId, int providerId, int locationId)
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
            return ExecuteNonQuery("InsertFlowSurgeonNote", dsParameters);
        }

        public static int UpdateSurgeonNote(int surgeonNoteId, string comments, int stepId, int roleId, int providerId, int locationId)
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
            return ExecuteNonQuery("UpdateSurgeonNote", dsParameters);
        }

        public static int DeleteSurgeonNote(int surgeonNoteId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgeon_note_id", surgeonNoteId)
            };
            return ExecuteNonQuery("DeleteSurgeonNote", dsParameters);
        }

        public static int UpdateDebrief(int flowId, List<FlowPhrase> revisedPhrases, int providerId, int locationId)
        {
            throw new NotImplementedException("");
            //var currentPhrases = GetFlowPhrases(flowId, providerId, locationId);

            //foreach (var revisedPhrase in revisedPhrases)
            //{
            //    SqlParameter[] dsParameters;

            //    if (revisedPhrase.PhraseComment == string.Empty)
            //        revisedPhrase.PhraseComment = null;

            //    var currentPhrase = currentPhrases.First(c => c.SmartPhraseID == revisedPhrase.SmartPhraseID);

            //    if (revisedPhrase.PhraseActive)
            //    {
            //        if (revisedPhrase.PhraseActive && !currentPhrase.PhraseActive)
            //        {
            //            // insert phrase
            //            dsParameters = new[]
            //            {
            //                new SqlParameter("provider_id", providerId),
            //                new SqlParameter("location_id", locationId),
            //                new SqlParameter("flow_id", flowId),
            //                new SqlParameter("step_id", revisedPhrase.FlowStepID),
            //                new SqlParameter("role_id", revisedPhrase.RoleID),
            //                new SqlParameter("smart_phrase_id", revisedPhrase.SmartPhraseID),
            //                new SqlParameter("comment", revisedPhrase.PhraseComment ?? (object)DBNull.Value)
            //            };
            //            ExecuteNonQuery("InsertFlowPhrase", dsParameters);
            //        }
            //        else if (revisedPhrase.FlowStepID != currentPhrase.FlowStepID ||
            //            revisedPhrase.RoleID != currentPhrase.RoleID ||
            //            revisedPhrase.PhraseComment != currentPhrase.PhraseComment)
            //        {
            //            // Update step & role if needed
            //            dsParameters = new[]
            //            {
            //                new SqlParameter("provider_id", providerId),
            //                new SqlParameter("location_id", locationId),
            //                new SqlParameter("flow_id", flowId),
            //                new SqlParameter("step_id", revisedPhrase.FlowStepID),
            //                new SqlParameter("role_id", revisedPhrase.RoleID),
            //                new SqlParameter("smart_phrase_id", revisedPhrase.SmartPhraseID),
            //                new SqlParameter("comment", revisedPhrase.PhraseComment ?? (object)DBNull.Value)
            //            };
            //            ExecuteNonQuery("UpdateFlowPhrase", dsParameters);
            //        }
            //    }
            //    else
            //    {
            //        if (currentPhrase.PhraseActive)
            //        {
            //            // delete phrase

            //            dsParameters = new[]
            //            {
            //                new SqlParameter("provider_id", providerId),
            //                new SqlParameter("location_id", locationId),
            //                new SqlParameter("flow_id", flowId),
            //                new SqlParameter("smart_phrase_id", currentPhrase.SmartPhraseID)
            //            };
            //            ExecuteNonQuery("DeleteFlowPhrase", dsParameters);
            //        }
            //    }
                
            //}

            //return 0;
        }

        public static int UpdateCaseNotes(int surgeryId, string caseNotes, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("case_notes", caseNotes)
            };
            return ExecuteNonQuery("UpdateCaseNotes", dsParameters);
        }

        public static int AssignCardToCase(int cardId, int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("card_id", cardId)
            };
            return ExecuteNonQuery("AssignCardToCase", dsParameters);
        }

        public static int AssignFlowToCase(int flowId, int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("flow_id", flowId)
            };
            return ExecuteNonQuery("AssignFlowToCase", dsParameters);
        }

        public static int AssignRoomSetupToCase(int roomSetupId, int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("room_setup_id", roomSetupId)
            };
            return ExecuteNonQuery("AssignRoomSetupToCase", dsParameters);
        }

        public static int AssignFlowToCard(int flowId, int cardId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("flow_id", flowId)
            };
            return ExecuteNonQuery("AssignFlowToCard", dsParameters);
        }

        public static int AssignRoomSetupToCard(int roomSetupId, int cardId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("room_setup_id", roomSetupId)
            };
            return ExecuteNonQuery("AssignRoomSetupToCard", dsParameters);
        }

        public static int AssignUserToCard(int cardId, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("user_id", userId)
            };
            return ExecuteNonQuery("InsertCardUser", dsParameters);
        }

        public static int RemoveUserFromCard(int cardId, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("user_id", userId)
            };
            return ExecuteNonQuery("DeleteCardUser", dsParameters);
        }

        public static int UpdateCardItem(int cardId, int itemId, int qtyOpen, int qtyHold, int providerId, int locationId)
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
            return ExecuteNonQuery("UpdateCardItem", dsParameters);
        }

        public static int? UpdateCardProcedure(int cardId, string cptCode, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("cpt_code", cptCode)
            };
            var updateResult = ExecuteCommand("UpdateCardProcedure", dsParameters);

            if (updateResult.Tables[0].Rows.Count <= 0) return null;

            var result = updateResult.Tables[0].Rows[0];
            return result["ProcedureID"] == DBNull.Value ? (int?)null : (int)result["ProcedureID"];
        }

        public static int? DeleteCardProcedure(int cardId, int procedureId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("card_id", cardId),
                new SqlParameter("procedure_id", procedureId)
            };
            return ExecuteNonQuery("DeleteCardProcedure", dsParameters);
        }

        public static int InsertCard(string description, int ownerUserId, int? specialtyId, int? procedureId, int? templateFlowId, int? templateRoomId, int? bundleId, string bundleFlag,
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
                new SqlParameter("template_room_id", templateRoomId ?? (object)DBNull.Value),
                new SqlParameter("bundle_id", bundleId ?? (object)DBNull.Value),
                new SqlParameter("bundle_flag", bundleFlag ?? (object)DBNull.Value),
                new SqlParameter("default_flag", defaultFlag ?? (object)DBNull.Value),
                new SqlParameter("specialty_default_flag", specialtyDefaultFlag ?? (object)DBNull.Value)
            };
            var insert = ExecuteCommand("NewCard", dsParameters);
            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.First().Identifier;
        }

        public static int InsertCardItemFromStage(int cardId, int providerId, int locationId, string procedure, string surgeon)
        {
            var dsParameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("procedure", procedure),
                new SqlParameter("surgeon", surgeon)
            };

            var insert = ExecuteNonQuery("InsertCardItemFromStage", dsParameters);
            return insert;
        }

        public static int UpdateCard(int cardId, string description, int ownerUserId, int? specialtyId, int? procedureId, int? templateFlowId, int? templateRoomId, int? bundleId, string bundleFlag,
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
                new SqlParameter("template_room_id", templateRoomId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("bundle_id", bundleId ?? (object)DBNull.Value),
                new SqlParameter("bundle_flag", bundleFlag ?? (object)DBNull.Value),
                new SqlParameter("default_flag", defaultFlag ?? (object)DBNull.Value),
                new SqlParameter("specialty_default_flag", specialtyDefaultFlag ?? (object)DBNull.Value)
            };
            return ExecuteNonQuery("UpdateCard", dsParameters);
        }

        public static int DeleteCard(int cardId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("card_id", cardId)
            };
            return ExecuteNonQuery("DeleteCard", dsParameters);
        }

        public static int UpdateCardQuantity(int cardId, CardQuantityEdit quantity, int providerId, int locationId)
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
            return ExecuteNonQuery("UpdateCardItemQty", dsParameters);
        }

        public static int CreateSurgery(SurgeryPost surgery, int providerId, int locationId, int patientId, int caseId, 
            int? procedureId, int? defaultCardId, int? defaultFlowId, int? defaultRoomId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("patient_id", patientId),
                new SqlParameter("user_id", surgery.SurgeonUserID),
                new SqlParameter("specialty_id", surgery.SpecialtyID),
                new SqlParameter("bundle_id", surgery.BundleID ?? (object)DBNull.Value),
                new SqlParameter("procedure_id", procedureId ?? (object)DBNull.Value),
                new SqlParameter("case_id", caseId),
                new SqlParameter("room_id", surgery.RoomID ?? (object)DBNull.Value),
                new SqlParameter("schedule_date", surgery.ScheduleDate),
                new SqlParameter("schedule_time", surgery.ScheduleDate),
                new SqlParameter("default_card_id", defaultCardId ?? (object)DBNull.Value),
                new SqlParameter("default_flow_id", defaultFlowId ?? (object)DBNull.Value),
                new SqlParameter("default_room_id", defaultRoomId ?? (object)DBNull.Value),
                new SqlParameter("cpt_codes", surgery.CptCode ?? (object)DBNull.Value)
            };
            var insert = ExecuteCommand("NewSurgery", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public static int AddCustomSurgeryItem(int surgeryId, int itemId, int quantity, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("item_id", itemId),
                new SqlParameter("quantity", quantity)
            };
            return ExecuteNonQuery("InsertCustomSurgeryItem", dsParameters);
        }

        public static int AddSurgeryUser(int surgeryId, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("user_id", userId)
            };
            return ExecuteNonQuery("InsertSurgeryUser", dsParameters);
        }

        public static int DeleteSurgeryUser(int surgeryId, int userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("user_id", userId)
            };
            return ExecuteNonQuery("DeleteSurgeryUser", dsParameters);
        }

        public static int AddSurgeryProcedure(int surgeryId, string cptCode, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("cpt_code", cptCode)
            };
            return ExecuteNonQuery("InsertSurgeryProcedure", dsParameters);
        }

        public static int UpdateSurgeryProcedure(int surgeryId, string cptCode, string performedFlag, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("cpt_code", cptCode),
                new SqlParameter("performed_flag", performedFlag)
            };
            return ExecuteNonQuery("UpdateSurgeryProcedure", dsParameters);
        }

        public static int DeleteSurgeryProcedure(int surgeryId, string cptCode, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("cpt_code", cptCode)
            };
            return ExecuteNonQuery("DeleteSurgeryProcedure", dsParameters);
        }

        public static int UpdateSurgeryHeaderCounts(int surgeryId, int sharpCount, int needleCount, int lapCount, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("sharp_count", sharpCount),
                new SqlParameter("needle_count", needleCount),
                new SqlParameter("lap_count", lapCount)
            };
            return ExecuteNonQuery("UpdateSurgeryHeaderCounts", dsParameters);
        }

        public static int AddCustomSurgeryTrayItem(int surgeryId, int trayId, int itemId, int quantity, int providerId, int locationId)
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
            return ExecuteNonQuery("InsertCustomSurgeryTrayInstrument", dsParameters);
        }

        public static int UpdateSurgeryCount(int surgeryId, int itemId, bool pass1, bool pass2, bool pass3, int usage, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("item_id", itemId),
                new SqlParameter("pass_1", pass1),
                new SqlParameter("pass_2", pass2),
                new SqlParameter("pass_3", pass3),
                new SqlParameter("usage", usage)
            };
            var update = ExecuteNonQuery("UpdateSurgeryCount", dsParameters);

            return update;
        }
        public static int UpdateSurgeryInstrumentCount(int surgeryId, int instrumentId, bool pass1, bool pass2, bool pass3, int usage, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("instrument_id", instrumentId),
                new SqlParameter("pass_1", pass1),
                new SqlParameter("pass_2", pass2),
                new SqlParameter("pass_3", pass3),
                new SqlParameter("usage", usage)
            };
            var update = ExecuteNonQuery("UpdateSurgeryInstrumentCount", dsParameters);

            return update;
        }

        public static int StartSurgery(int surgeryId, int providerId, int locationId, DateTime startTime)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("start_time", startTime)
            };
            var update = ExecuteNonQuery("StartSurgery", dsParameters);

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
        public static FlowStep SurgeryMoveNextStep(int surgeryId, int providerId, int locationId, DateTime startTime)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("step_time", startTime)
            };

            var update = ExecuteCommand("UpdateSurgeryFlowTimings", dsParameters);
            var flowSteps = update.Tables[0].DataTableToList<FlowStep>();
            var flowStep = flowSteps.FirstOrDefault();

            return flowStep;
        }

        public static int SurgeryToggleDelay(int surgeryId, int providerId, int locationId, DateTime? startTime, DateTime? endTime, int? delayReasonId)
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
            var update = ExecuteNonQuery("UpdateSurgeryDelays", dsParameters);

            return update;
        }

        public static int SurgeryEditProperties(int surgeryId, int roomId, DateTime surgeryScheduleDate, int providerId, int locationId)
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
            var update = ExecuteNonQuery("UpdateSurgeryProperties", dsParameters);

            return update;
        }

        public static int CreateCase(int patientId, int userId, int specialtyId, int providerId, int locationId, string caseNbr)
        {
            var dsParameters = new[]
            {
                new SqlParameter("patient_id", patientId),
                new SqlParameter("user_id", userId),
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("case_nbr", caseNbr)
            };
            var insert = ExecuteCommand("NewCase", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public static List<Room> GetRooms(int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = ExecuteCommand("GetRooms", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<Room>();

            return result;
        }

        public static List<RoomType> GetRoomTypes(int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = ExecuteCommand("GetRoomTypes", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<RoomType>();

            return result;
        }

        public static List<RoomGroup> GetRoomGroups(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = ExecuteCommand("GetRoomGroups", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<RoomGroup>();

            return result;
        }

        public static List<RoomSetup> GetRoomSetups(int? roomSetupId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("room_setup_id", roomSetupId ?? (object)DBNull.Value),
            };
            var dsSchedules = ExecuteCommand("GetRoomSetups", dsParameters);

            var setups = dsSchedules.Tables[0].DataTableToList<RoomSetup>();
            var setupEquipments = dsSchedules.Tables[1].DataTableToList<RoomSetupEquipment>();
            var setupItems = dsSchedules.Tables[2].DataTableToList<RoomSetupItem>();
            var staffPositions = dsSchedules.Tables[3].DataTableToList<RoomSetupStaffPosition>();

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

            return setups;
        }

        public static List<PatientPosition> GetPatientPositions(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = ExecuteCommand("GetPatientPositions", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<PatientPosition>();

            return result;
        }

        public static List<SmartPhrase> GetSmartPhrases(int? categoryId, int? specialtyId, int? userId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("user_id", userId ?? (object)DBNull.Value),
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("category_id", categoryId ?? (object)DBNull.Value)
            };
            var dsSchedules = ExecuteCommand("GetPhrases", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<SmartPhrase>();

            return result;
        }
        
        public static List<SmartPhraseCategory> GetSmartPhraseCategories(int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = ExecuteCommand("GetPhraseCategories", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<SmartPhraseCategory>();

            return result;
        }

        public static List<FlowPhrase> GetFlowPhrases(int flowId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetFlowPhrases", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowPhrase>();

            return result;
        }

        public static List<SurgeryPhrase> GetSurgeryPhrases(int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSurgeryPhrases", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryPhrase>();

            return result;
        }

        public static List<FlowImage> GetFlowImages(int flowId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetFlowImages", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowImage>();

            return result;
        }

        public static List<SurgeryImage> GetSurgeryImages(int surgeryId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSurgeryImages", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryImage>();

            return result;
        }

        public static List<SurgeryDelay> GetFlowSurgeryDelays(int flowId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetFlowSurgeryDelays", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryDelay>();

            return result;
        }

        public static List<FlowSurgeonNote> GetSurgeonNotes(int flowId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("flow_id", flowId)
            };
            var dsSchedules = ExecuteCommand("GetFlowSurgeonNotes", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowSurgeonNote>();

            return result;
        }

        public static int UpdateRoom(int roomId, string description, int typeId, int groupId, int providerId, int locationId)
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
            var result = ExecuteNonQuery("UpdateRoom", dsParameters);

            return result;
        }

        public static int InsertRoom(string description, int typeId, int groupId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("description", description),
                new SqlParameter("room_type_id", typeId),
                new SqlParameter("room_group_id", groupId)
            };
            var dsResult = ExecuteCommand("InsertRoom", dsParameters);

            var result = dsResult.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public static int DeleteRoom(int roomId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("room_id", roomId)
            };
            var result = ExecuteNonQuery("DeleteRoom", dsParameters);

            return result;
        }

        public static int UpdateRoomSetup(int roomSetupId, int providerId, int locationId, RoomSetup roomSetup)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("patient_position_id", roomSetup.PatientPositionID),
                new SqlParameter("patient_extremity_position_id", roomSetup.PatientExtremityPositionID),
                new SqlParameter("room_type_id", roomSetup.RoomTypeID),
                new SqlParameter("bed_orientation", roomSetup.BedOrientation),
            };
            var update = ExecuteNonQuery("UpdateRoomSetup", dsParameters);
            var currentRoomSetup = GetRoomSetups(roomSetupId, providerId, locationId).FirstOrDefault();


            foreach (var roomSetupEquipment in roomSetup.SetupEquipment)
            {
                var currentEquipment = currentRoomSetup?.SetupEquipment?.FirstOrDefault(se =>
                    se.RoomSetupEquipmentID == roomSetupEquipment.RoomSetupEquipmentID);

                if (currentEquipment != null)
                    UpdateRoomSetupEquipment(roomSetupEquipment, providerId, locationId);
                else
                {
                    roomSetupEquipment.RoomSetupEquipmentID =
                        InsertRoomSetupEquipment(roomSetupId, roomSetupEquipment, providerId, locationId);
                }
            }
            foreach (var roomSetupItem in roomSetup.SetupItems)
            {
                var currentItem = currentRoomSetup?.SetupItems?.FirstOrDefault(se =>
                    se.RoomSetupItemID == roomSetupItem.RoomSetupItemID);

                if (currentItem != null)
                    UpdateRoomSetupItem(roomSetupItem, providerId, locationId);
                else
                {
                    roomSetupItem.RoomSetupItemID =
                        InsertRoomSetupItem(roomSetupId, roomSetupItem, providerId, locationId);
                }
            }
            foreach (var roomSetupStaffPosition in roomSetup.StaffPositions)
            {
                var currentStaffPosition = currentRoomSetup?.StaffPositions?.FirstOrDefault(se =>
                    se.StaffPosition == roomSetupStaffPosition.StaffPosition);

                if (currentStaffPosition != null)
                    UpdateRoomSetupStaffPosition(roomSetupStaffPosition, providerId, locationId);
                else
                {
                    InsertRoomSetupStaffPosition(roomSetupId, roomSetupStaffPosition, providerId, locationId);
                }
            }

            if (currentRoomSetup != null)
            {
                foreach (var currentSetupEquipment in currentRoomSetup?.SetupEquipment)
                {
                    var sentEquipment = roomSetup.SetupEquipment.FirstOrDefault(se =>
                        se.RoomSetupEquipmentID == currentSetupEquipment.RoomSetupEquipmentID);

                    if (sentEquipment == null)
                        DeleteRoomSetupEquipment(currentSetupEquipment.RoomSetupEquipmentID, providerId, locationId);
                }

                foreach (var currentSetupEquipment in currentRoomSetup?.SetupItems)
                {
                    var sentItem = roomSetup.SetupItems.FirstOrDefault(se =>
                        se.RoomSetupItemID == currentSetupEquipment.RoomSetupItemID);

                    if (sentItem == null)
                        DeleteRoomSetupItem(currentSetupEquipment.RoomSetupItemID, providerId, locationId);
                }

                foreach (var currentStaffPosition in currentRoomSetup?.StaffPositions)
                {
                    var sentItem = roomSetup.StaffPositions.FirstOrDefault(se =>
                        se.StaffPosition == currentStaffPosition.StaffPosition);

                    if (sentItem == null)
                        DeleteRoomSetupStaffPosition(currentStaffPosition, providerId, locationId);
                }
            }

            return roomSetupId;
        }

        public static int CreateRoomSetup(RoomSetup roomSetup, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("patient_position_id", roomSetup.PatientPositionID),
                new SqlParameter("patient_extremity_position_id", roomSetup.PatientExtremityPositionID),
                new SqlParameter("room_type_id", roomSetup.RoomTypeID),
                new SqlParameter("bed_orientation", roomSetup.BedOrientation)
            };
            var insert = ExecuteCommand("NewRoomSetup", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            var roomSetupId = result.FirstOrDefault()?.Identifier ?? -1;

            foreach (var roomSetupEquipment in roomSetup.SetupEquipment)
            {
                roomSetupEquipment.RoomSetupEquipmentID =
                    InsertRoomSetupEquipment(roomSetupId, roomSetupEquipment, providerId, locationId);
            }
            foreach (var roomSetupItem in roomSetup.SetupItems)
            {
                roomSetupItem.RoomSetupItemID =
                    InsertRoomSetupItem(roomSetupId, roomSetupItem, providerId, locationId);
            }
            foreach (var staffPosition in roomSetup.StaffPositions)
            {
                InsertRoomSetupStaffPosition(roomSetupId, staffPosition, providerId, locationId);
            }

            return roomSetupId;
        }


        public static int InsertRoomSetupEquipment(int roomSetupId, RoomSetupEquipment roomSetupEquipment, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("item_id", roomSetupEquipment.ItemID),
                new SqlParameter("equipment_position", roomSetupEquipment.EquipmentPosition)
            };
            var insert = ExecuteCommand("InsertRoomSetupEquipment", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            var roomSetupEquipmentId = result.FirstOrDefault()?.Identifier ?? -1;

            return roomSetupEquipmentId;
        }

        public static int InsertRoomSetupItem(int roomSetupId, RoomSetupItem roomSetupItem, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("item_id", roomSetupItem.ItemID),
                new SqlParameter("quantity", roomSetupItem.ItemQuantity)
            };
            var insert = ExecuteCommand("InsertRoomSetupItem", dsParameters);

            var result = insert.Tables[0].DataTableToList<InsertionResult>();

            var roomSetupItemId = result.FirstOrDefault()?.Identifier ?? -1;

            return roomSetupItemId;
        }

        public static int InsertRoomSetupStaffPosition(int roomSetupId, RoomSetupStaffPosition roomSetupStaffPosition, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("staff_position", roomSetupStaffPosition.StaffPosition),
                new SqlParameter("staff_role_id", roomSetupStaffPosition.StaffRoleID)
            };
            var result = ExecuteNonQuery("InsertRoomSetupStaffPosition", dsParameters);

            return result;
        }
        public static int UpdateRoomSetupEquipment(RoomSetupEquipment roomSetupEquipment, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_equipment_id", roomSetupEquipment.RoomSetupEquipmentID),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("item_id", roomSetupEquipment.ItemID),
                new SqlParameter("equipment_position", roomSetupEquipment.EquipmentPosition)
            };

            var result = ExecuteNonQuery("UpdateRoomSetupEquipment", dsParameters);

            return result;
        }

        public static int UpdateRoomSetupItem(RoomSetupItem roomSetupItem, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_item_id", roomSetupItem.RoomSetupItemID),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("item_id", roomSetupItem.ItemID),
                new SqlParameter("quantity", roomSetupItem.ItemQuantity)
            };
            var result = ExecuteNonQuery("UpdateRoomSetupItem", dsParameters);

            return result;
        }

        public static int UpdateRoomSetupStaffPosition(RoomSetupStaffPosition roomSetupStaffPosition, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupStaffPosition.RoomSetupID),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("staff_position", roomSetupStaffPosition.StaffPosition),
                new SqlParameter("staff_role_id", roomSetupStaffPosition.StaffRoleID)
            };
            var result = ExecuteNonQuery("UpdateRoomsetupStaffPosition", dsParameters);

            return result;
        }
        public static int DeleteRoomSetupEquipment(int roomSetupEquipmentId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_equipment_id", roomSetupEquipmentId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = ExecuteNonQuery("DeleteRoomSetupEquipment", dsParameters);

            return result;
        }

        public static int DeleteRoomSetupItem(int roomSetupItemId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_item_id", roomSetupItemId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var result = ExecuteNonQuery("DeleteRoomSetupItem", dsParameters);

            return result;
        }

        public static int DeleteRoomSetupStaffPosition(RoomSetupStaffPosition roomSetupStaffPosition, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("room_setup_id", roomSetupStaffPosition.RoomSetupID),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("staff_position", roomSetupStaffPosition.StaffPosition)
            };
            var result = ExecuteNonQuery("DeleteRoomSetupItem", dsParameters);

            return result;
        }

        public static PatientSurgery GetSurgery(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var command = "GetSurgeryProcedure";
            var dsSchedules = ExecuteCommand(command, parameters);

            var result = dsSchedules.Tables[0].DataTableToList<PatientSurgery>().FirstOrDefault();

            return result;
        }

        public static Surgery GetCase(int caseId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("case_id", caseId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsSchedules = ExecuteCommand("GetCase", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgery>().FirstOrDefault();

            return result;
        }

        public static List<SurgerySearchResult> SearchCases(int? surgeonUserId, int? roomId, int? bundleId, int? procedureId, DateTime? begDate, DateTime? endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", surgeonUserId ?? (object)DBNull.Value),
                new SqlParameter("room_id", roomId ?? (object)DBNull.Value),
                new SqlParameter("bundle_id", bundleId ?? (object)DBNull.Value),
                new SqlParameter("procedure_id", procedureId ?? (object)DBNull.Value),
                new SqlParameter("beg_date", begDate ?? (object)DBNull.Value),
                new SqlParameter("end_date", endDate ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsSchedules = ExecuteCommand("SearchCases", parameters);

            var surgeries = dsSchedules.Tables[0].DataTableToList<SurgerySearchResult>();
            var surgeryUsers = dsSchedules.Tables[1].DataTableToList<SurgeryUser>();

            foreach (var surgeryUser in surgeryUsers)
            {
                var surgery = surgeries.FirstOrDefault(s => s.SurgeryID == surgeryUser.SurgeryID);
                surgery?.SurgeryUsers?.Add(surgeryUser);
            }

            return surgeries;
        }

        public static List<SurgerySearchResult> GetCaseNbr(string caseNbr, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("case_nbr", caseNbr),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsSchedules = ExecuteCommand("SearchCaseNbr", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgerySearchResult>();

            return result;
        }

        public static List<SurgerySearchResult> GetSurgeonCases(int userId, DateTime begDate, DateTime endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("beg_date", begDate),
                new SqlParameter("end_date", endDate),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsSchedules = ExecuteCommand("SearchSurgeonCases", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgerySearchResult>();

            return result;
        }

        public static List<SurgerySearchResult> GetRoomCases(int roomId, DateTime begDate, DateTime endDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("room_id", roomId),
                new SqlParameter("beg_date", begDate),
                new SqlParameter("end_date", endDate),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var dsSchedules = ExecuteCommand("SearchRoomCases", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgerySearchResult>();

            return result;
        }

        public static List<SurgerySchedule> GetScheduledSurgeries(int? userID, int providerId, int locationId,
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
            var dsSchedules = ExecuteCommand("GetSurgeriesByUser", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgerySchedule>();

            return result;
        }

        public static List<Surgery> GetSurgeryAlerts(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCaseAlerts", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgery>();

            return result;
        }

        public static List<SurgeryDelayReason> GetSurgeryDelayReasons(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSurgeryDelayReasons", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryDelayReason>();

            return result;
        }

        public static List<SurgeryUser> GetSurgeryUsers(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCheckInCase", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryUser>();

            return result;
        }

        public static List<Surgery> GetSurgeryRoomSchedule(int roomId, DateTime? scheduleDate, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("room_id", roomId),
                new SqlParameter("schedule_date", scheduleDate ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCheckInCasesbyRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgery>();

            return result;
        }

        public static List<SurgeryVendorRep> GetSurgeryVendorReps(int surgeryId, int locationId, int providerId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSurgeryVendorReps", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryVendorRep>();

            return result;
        }

        public static CardFlowRoom GetBundleDefaultCardFlowRoom(int bundleId, int userId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("bundle_id", bundleId),
                new SqlParameter("user_id", userId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = ExecuteCommand("GetBundleDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>();

            return result.FirstOrDefault();
        }

        public static List<Surgeon> GetImportSurgeons(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = ExecuteCommand("GetImportSurgeons", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgeon>();

            return result;
        }

        public static List<Procedure> GetImportProcedures(string importSurgeon, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgeon", importSurgeon),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
            };
            var dsSchedules = ExecuteCommand("GetImportProcedures", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Procedure>();

            return result;
        }

        public static List<CardFlowRoom> GetProcedureDefaultCardFlowRoom(int providerId, int locationId, string cptCode)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("cpt_code", cptCode)
            };
            var dsSchedules = ExecuteCommand("GetProcedureDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>();

            return result;
        }

        public static List<CardFlowRoom> GetSpecialtyProcedureDefaultCardFlowRoom(int providerId, int locationId, string cptCode)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("cpt_code", cptCode)
            };
            var dsSchedules = ExecuteCommand("GetProcedureDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>();

            return result;
        }

        public static List<CardFlowRoom> GetMultipleProceduresDefaultCardFlowRoom(int providerId, int locationId, int specialtyId, string cptCodes)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("cpt_codes", cptCodes)
            };
            var dsSchedules = ExecuteCommand("GetMultipleProceduresDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>();

            return result;
        }

        public static List<CardFlowRoom> GetSpecialtyMultipleProceduresDefaultCardFlowRoom(int providerId, int locationId, int specialtyId, string cptCodes)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("cpt_codes", cptCodes)
            };
            var dsSchedules = ExecuteCommand("GetSpecialtyMultipleProceduresDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>();

            return result;
        }

        public static List<CardBundle> GetBundles(int? specialtyId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetBundlesBySpecialty", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardBundle>();

            return result;
        }

        public static List<BundleProcedure> GetBundleProcedures(int bundleId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("bundle_id", bundleId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetBundleProcedures", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<BundleProcedure>();

            return result;
        }

        public static List<BundleProcedure> GetSurgeryProcedures(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSurgeryProcedures", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<BundleProcedure>();

            return result;
        }

        public static List<Procedure> GetProcedures(int? specialtyId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetProceduresBySpecialty", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Procedure>();

            return result;
        }

        public static List<Specialty> GetSpecialties(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSpecialtyByLocation", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Specialty>();

            return result;
        }

        public static int UpdateSpecialty(int specialtyId, string name, string description, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("name", name),
                new SqlParameter("description", description),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = ExecuteNonQuery("UpdateSpecialty", parameters);

            return result;
        }

        public static int InsertSpecialty(string name, string description, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("name", name),
                new SqlParameter("description", description),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsResult = ExecuteCommand("InsertSpecialty", parameters);

            var result = dsResult.Tables[0].DataTableToList<InsertionResult>();

            return result.FirstOrDefault()?.Identifier ?? -1;
        }

        public static int DeleteSpecialty(int specialtyId, int providerId, int locationId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("specialty_id", specialtyId)
            };
            var result = ExecuteNonQuery("DeleteSpecialty", dsParameters);

            return result;
        }

        public static List<Surgeon> GetSurgeons(int? specialtyId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSurgeonBySpecialty", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgeon>();

            return result;
        }

        public static Flow GetFlow(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardFlowData", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Flow>();

            return result.FirstOrDefault();
        }

        public static List<Flow> GetCardFlowList(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardFlowList", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Flow>();

            return result;
        }

        public static List<FlowStepTiming> GetFlowTimings(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
                {
                    new SqlParameter("flow_id", flowId),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId)
                };


            var dsSchedules = ExecuteCommand("GetFlowTimings", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowStepTiming>();

            return result;
        }

        public static List<FlowStepSurgeryTiming> GetFlowSurgeryTimings(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };


            var dsSchedules = ExecuteCommand("GetFlowSurgeryTimings", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowStepSurgeryTiming>();

            return result;
        }

        public static List<FlowStepInstructionResult> GetFlowInstructions(int flowId, int? surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
                {
                    new SqlParameter("flow_id", flowId),
                    new SqlParameter("surgery_id", surgeryId ?? (object)DBNull.Value),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId)
                };


            var dsSchedules = ExecuteCommand("GetFlowInstructions", parameters);

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

        public static List<FlowStep> GetFlowComments(int flowId, int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetFlowComments", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowStep>();

            return result;
        }

        public static List<FlowMessaging> GetFlowMessaging(int flowId, int providerId, int locationId, int stepId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("step_id", stepId)
            };
            var dsSchedules = ExecuteCommand("GetFlowMessaging", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowMessaging>();

            return result;
        }

        public static List<FlowContent> GetFlowContent(int flowId, int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetFlowContent", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowContent>();

            return result;
        }

        public static List<FlowNotification> GetFlowNotifications(int flowId, int? stepId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("step_id", stepId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetFlowStepNotifications", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowNotification>();

            return result;
        }

        public static List<Step> GetSteps(int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSteps", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Step>();

            return result;
        }

        public static List<FlowFeedback> GetFlowFeedback(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetFlowFeedback", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowFeedback>();

            return result;
        }

        public static int DeleteFlowStep(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = ExecuteNonQuery("DeleteFlowStep", parameters);

            return result;
        }

        public static int InsertFlowStep(int flowId, int stepId, int stepSequence, decimal duration, int providerId, int locationId)
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
            var result = ExecuteNonQuery("NewFlowStep", parameters);

            return result;
        }

        public static int InsertFlowNotification(int flowId, int stepId, int notificationType, string message, 
            string smsNumber, string emailAddress, int? messagingUserId, int providerId, int locationId)
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
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsNotification = ExecuteCommand("InsertFlowNotification", parameters);

            var result = dsNotification.Tables[0].DataTableToList<InsertionResult>().First().Identifier;

            return result;
        }

        public static int EditFlowNotification(int flowNotificationId, string message, int stepId,
            string smsNumber, string emailAddress, int? messagingUserId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_notification_id", flowNotificationId),
                new SqlParameter("message", message),
                new SqlParameter("step_id", stepId),
                new SqlParameter("sms_number", smsNumber ?? (object)DBNull.Value),
                new SqlParameter("email_address", emailAddress ?? (object)DBNull.Value),
                new SqlParameter("messaging_user_id", messagingUserId ?? (object)DBNull.Value),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = ExecuteNonQuery("UpdateFlowNotification", parameters);

            return result;
        }

        public static int DeleteFlowNotification(int flowNotificationId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_notification_id", flowNotificationId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var result = ExecuteNonQuery("DeleteFlowNotification", parameters);

            return result;
        }

        public static int NewFlow(int cardId, int roomSetupId, string description, int userId, bool defaultFlow, int providerId, int locationId)
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
            var dsSchedules = ExecuteCommand("NewFlow", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<InsertionResult>().First().Identifier;

            return result;
        }

        public static int UpdateFlow(int flowId, int cardId, int roomSetupId, string description, int userId, bool defaultFlow, int providerId, int locationId)
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

            return ExecuteNonQuery("UpdateFlow", parameters);
        }

        public static int DeleteFlow(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            return ExecuteNonQuery("DeleteFlow", parameters);
        }

        public static void InsertStagingData(int providerId, int locationId, int? secureId, IImportData sourceData)
        {
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

                var insertion = ExecuteNonQuery(@"INSERT stag_item (provider_id, location_id, location, location_name, from_loc, bin_seq, 
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

                var insertion = ExecuteNonQuery(@"INSERT stag_card (provider_id, location_id, location, surgeon, preference_card_name, type,
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

                var insertion = ExecuteNonQuery(@"INSERT stag_tray (provider_id, location_id, customer, tray_id, tray_name, instrument_name, quantity, manufacturer)
                VALUES (@provider_id, @location_id, @customer, @tray_id, @tray_name, @instrument_name, @quantity, @manufacturer)", parameters, CommandType.Text);
            }
            else if(sourceData is ScheduleImport schedule)
            {
                var parameters = new[]
                {
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId),
                    new SqlParameter("patient_id", secureId),
                    new SqlParameter("case_id", schedule.CaseID),
                    new SqlParameter("[schedule_date]", schedule.ScheduleDate),
                    new SqlParameter("[schedule_time]", schedule.ScheduleTime),
                    new SqlParameter("[location]", schedule.Location),
                    new SqlParameter("[room]", schedule.Room),
                    new SqlParameter("[procedure]", schedule.Procedure),
                    new SqlParameter("[procedure_card]", schedule.ProcedureCard),
                    new SqlParameter("[surgeon]", schedule.Surgeon),
                    new SqlParameter("[circulator]", schedule.Circulator),
                    new SqlParameter("[anes]", schedule.Anes),
                    new SqlParameter("[tech]", schedule.Tech)
                };

                var insertion = ExecuteNonQuery(@"INSERT[dbo].[stag_schedule]([provider_id],[location_id],[patient_id],[case_id],
                        [schedule_date],[schedule_time],[location],[room],[procedure],[procedure_card],[surgeon],[circulator],[anes],[tech]) 
                    VALUES (@provider_id,@location_id,@patient_id,@case_id,
                    @schedule_date,@schedule_time,@location,
                    @room,@procedure,@procedure_card,@surgeon,@circulator,@anes,@tech)", parameters, CommandType.Text);
            }
        }

        public static void InsertImportLog(int providerId, int locationId, int importTypeId, int userId, int recordCount, string filename)
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
            var insertion = ExecuteNonQuery("InsertImportLog", parameters);
        }
    }
}