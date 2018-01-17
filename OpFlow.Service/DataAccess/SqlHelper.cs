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

        public static List<Card> GetCardSurgeryItems(int cardId, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("card_id", cardId),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetCardSurgeryItems", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Card>();

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

        public static List<SurgeryUser> GetProviderCardUsers(int cardId, int providerId, int locationId, int? typeId)
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

        public static List<User> GetUsers(string username)
        {
            var dsParameters = new[]
            {
                new SqlParameter("email", (object)username ?? DBNull.Value),
            };
            var dsSchedules = ExecuteCommand("GetUser", dsParameters);

            var result = dsSchedules.Tables[0].DataTableToList<User>();

            return result;
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

        public static List<Surgery> GetScheduledSurgeries(int userID, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userID),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
            };
            var dsSchedules = ExecuteCommand("GetUserScheduledCases", parameters);

            var result = dsSchedules.Tables[0].DataTableToList<Surgery>();

            return result;
        }

        public static List<Surgery> GetOpenSurgeries(int userID, int providerId, int locationId)
        {
            var parameters = new[]
            {
                new SqlParameter("user_id", userID),
                new SqlParameter("provider_id", providerId),
                new SqlParameter("location_id", locationId)
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