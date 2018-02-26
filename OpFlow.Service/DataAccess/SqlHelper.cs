using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using OpFlow.Data;

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

        private static int ExecuteNonQuery(string storedProcedure, SqlParameter[] dsParameters = null)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["OpFlowConnection"].ConnectionString);
            var cmd = new SqlCommand(storedProcedure, conn) { CommandType = CommandType.StoredProcedure };

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

        public static List<Messaging> GetMessaging(int userId, int? surgeryId, int? caseGroupId, int? recipientId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("surgery_id", surgeryId == null ? DBNull.Value : (object)surgeryId),
                new SqlParameter("case_group_id", caseGroupId == null ? DBNull.Value : (object)caseGroupId),
                new SqlParameter("recipient_id", recipientId == null ? DBNull.Value : (object)recipientId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetMessaging", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Messaging>();

            return result;
        }

        public static List<MessagingGroup> GetMessageGroups(int userId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
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
                new SqlParameter("surgery_id", surgeryId == null ? DBNull.Value : (object)surgeryId),
                new SqlParameter("communication_user_id", communicationUserId == null ? DBNull.Value : (object)communicationUserId),
                new SqlParameter("message", message)
            };
            var result = ExecuteNonQuery("SendMessage", parameters);
        }

        public static List<Card> GetCardData(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardData", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

            return result;
        }

        public static List<CardItem> GetCardSurgeryItems(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardSurgeryItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardItem>();

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

        public static List<Card> GetCardList(int userId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardListBySurgeon", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

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

        public static User GetUser(string username)
        {
            var dsParameters = new[]
            {
                new SqlParameter("email", username),
            };
            var dsSchedules = ExecuteCommand("GetUser", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<User>().FirstOrDefault();

            return result;
        }

        public static List<User> SearchUsers(int providerId, int locationId, string searchString)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId),
                new SqlParameter("search", (object)searchString ?? DBNull.Value),
            };
            var dsSchedules = ExecuteCommand("SearchUsers", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<User>();

            return result;
        }

        public static UserSecurity GetSecureUser(string username)
        {
            var dsParameters = new[]
            {
                new SqlParameter("email", username),
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

        public static int AssignUserToCase(User user, int surgeryId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", user.ProviderID),
                new SqlParameter("location_id", user.LocationID),
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("user_id", user.UserID),
                new SqlParameter("role_id", user.RoleID)
            };
            return ExecuteNonQuery("AssignUserToCase", dsParameters);
        }

        public static int AssignCardToCase(Card card, int caseId)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", card.ProviderID),
                new SqlParameter("location_id", card.LocationID),
                new SqlParameter("surgery_id", card.SurgeryID),
                new SqlParameter("case_id", caseId),
                new SqlParameter("card_id", card.CardID)
            };
            return ExecuteNonQuery("AssignCardToCase", dsParameters);
        }

        public static int UpdateCardQuantity(int cardId, CardQuantityEdit quantity)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", quantity.ProviderID),
                new SqlParameter("location_id", quantity.LocationID),
                new SqlParameter("card_id", cardId),
                new SqlParameter("item_id", quantity.ItemID),
                new SqlParameter("open_qty", quantity.OpenQty),
                new SqlParameter("hold_qty", quantity.HoldQty),
                new SqlParameter("item_status", "D")
            };
            return ExecuteNonQuery("UpdateCardItemQty", dsParameters);
        }

        public static int CreateSurgery(SurgeryPost surgery)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", surgery.ProviderID),
                new SqlParameter("location_id", surgery.LocationID),
                new SqlParameter("patient_id", surgery.PatientID),
                new SqlParameter("user_id", surgery.UserID),
                new SqlParameter("specialty_id", surgery.SpecialtyID),
                new SqlParameter("bundle_id", surgery.BundleID),
                new SqlParameter("procedure_id", surgery.ProcedureID),
                new SqlParameter("case_id", surgery.CaseID),
                new SqlParameter("schedule_date", surgery.ScheduleDate)
            };
            return ExecuteNonQuery("NewSurgery", dsParameters);
        }

        public static int CreatePatient(PatientPost patient)
        {
            var dsParameters = new[]
            {
                new SqlParameter("initials", patient.Initials),
                new SqlParameter("birth_date", patient.BirthDate),
                new SqlParameter("gender", patient.Gender)
            };
            return ExecuteNonQuery("NewPatient", dsParameters);
        }

        public static int CreateCase(PatientCase newCase)
        {
            var dsParameters = new[]
            {
                new SqlParameter("provider_id", newCase.ProviderID),
                new SqlParameter("location_id", newCase.LocationID),
                new SqlParameter("patient_id", newCase.PatientID),
                new SqlParameter("user_id", newCase.UserID),
                new SqlParameter("specialty_id", newCase.SpecialtyID)
            };
            return ExecuteNonQuery("NewCase", dsParameters);
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

        public static Surgery GetSurgery(int surgeryId, int providerId, int locationId, string bundleFlag)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };

            var command = (bundleFlag == "Y" ? "GetSurgeryBundle" : "GetSurgeryProcedure");
            var dsSchedules = ExecuteCommand(command, parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgery>().FirstOrDefault();

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

        public static List<Surgery> GetScheduledSurgeries(int? userID, int providerId, int locationId,
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
            var dsSchedules = ExecuteCommand("GetUserScheduledCases", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgery>();

            return result;
        }

        public static List<Surgery> GetOpenSurgeries(int? userID, int providerId, int locationId,
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
            var dsSchedules = ExecuteCommand("GetOpenSurgeriesByUser", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgery>();

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

        public static List<Surgery> GetSurgeryDelays(int surgeryId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("surgery_id", surgeryId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSurgeryDelays", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgery>();

            return result;
        }

        public static List<SurgeryUser> GetSurgeryUsers(int caseId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("case_id", caseId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCheckInCase", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<SurgeryUser>();

            return result;
        }

        public static List<Surgery> GetSurgeryRoomSchedule(int roomId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("room_id", roomId),
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

        public static List<CardFlowRoom> GetBundleDefaultCardFlowRoom(int bundleId)
        {
            var parameters = new[]
            {
                new SqlParameter("bundle_id", bundleId)
            };
            var dsSchedules = ExecuteCommand("GetBundleDefaultCardFlowRoom", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardFlowRoom>();

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

        public static List<CardBundle> GetBundles(int specialtyId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetBundlesBySpecialty", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<CardBundle>();

            return result;
        }

        public static List<Procedure> GetProcedures(int specialtyId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId),
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

        public static List<Surgeon> GetSurgeons(int specialtyId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("specialty_id", specialtyId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetSurgeonBySpecialty", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgeon>();

            return result;
        }

        public static Flow GetFlow(int flowId, int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("card_id", cardId),
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

        public static List<FlowStep> GetFlowTimings(int flowId, int providerId, int locationId, int? surgeryId)
        {
            var command = surgeryId == null ? "GetFlowTimings" : "GetFlowSurgeryTimings";

            var parameters = surgeryId == null
                ? new[]
                {
                    new SqlParameter("flow_id", flowId),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId)
                }
                : new[]
                {
                    new SqlParameter("flow_id", flowId),
                    new SqlParameter("surgery_id", surgeryId),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId)
                };


            var dsSchedules = ExecuteCommand(command, parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowStep>();

            return result;
        }

        public static List<FlowStep> GetFlowInstructions(int flowId, int providerId, int locationId)
        {
            var parameters = new[]
                {
                    new SqlParameter("flow_id", flowId),
                    new SqlParameter("provider_id", providerId),
                    new SqlParameter("location_id", locationId)
                };


            var dsSchedules = ExecuteCommand("GetFlowInstructions", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowStep>();

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

        public static List<FlowNotification> GetFlowNotifications(int flowId, int stepId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("surgery_id", stepId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetFlowStepNotifications", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowNotification>();

            return result;
        }

        public static List<FlowFeedback> GetFlowFeedback(int flowId, int stepId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("flow_id", flowId),
                new SqlParameter("surgery_id", stepId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetFlowFeedback", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<FlowFeedback>();

            return result;
        }
    }
}