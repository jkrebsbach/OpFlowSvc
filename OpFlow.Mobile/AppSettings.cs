using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public static class AppSettings
    {
        public enum FragmentEnum
        {
            Login = 0,
            SearchCases = 1,
            Schedule = 2,
            CheckIn = 3,
            Debrief = 4,
            Review = 5,
            CaseNavigate = 6,
            Patient = 7,
            CardDetail = 8,
            FlowDetail = 9,
            Dashboard = 10,
            Room = 11,
            Communicator = 12,
            CardList = 13,
            CardAssignment = 14,
            FlowAssignment = 15,
            CreateCase = 16,
            CommunicationDetail = 17,
            NewCommunicationSetup = 18,
            ImageSetup = 19
        }

        public static int? CurrentSurgery { get; set; }

        public static byte[] CurrentImage { get; set; }

        public static FragmentEnum CurrentScreen;
        public static FragmentEnum PriorScreen;

        private static readonly Dictionary<int, List<Room>> _roomDictionary = new Dictionary<int, List<Room>>();

        public static string CurrentScreenName
        {
            get
            {
                switch (CurrentScreen)
                {
                    case FragmentEnum.Login:
                        return "";
                    case FragmentEnum.SearchCases:
                        return "Cases";
                    case FragmentEnum.Schedule:
                        return "Schedule";
                    case FragmentEnum.CheckIn:
                        return "Check In";
                    case FragmentEnum.Review:
                        return "Review";
                    case FragmentEnum.Debrief:
                        return "Debrief";
                    case FragmentEnum.Patient:
                        return "Patient";
                    case FragmentEnum.CardDetail:
                        return "Card";
                    case FragmentEnum.FlowDetail:
                        return "Flow";
                    case FragmentEnum.CaseNavigate:
                        return "Surgery";
                    case FragmentEnum.Dashboard:
                        return "Dashboard";
                }

                return "UNDEFINED";
            }

        }

        public static void LoadSurgery(int surgeryId)
        {
            CurrentSurgery = surgeryId;
        }

        public static void LoadSurgeryAttributes(Surgery surgery){
            CurrentPatient = surgery.PatientID;
            CurrentCard = surgery.CardID;
            CurrentBundle = surgery.BundleID;
            CurrentProcedure = surgery.ProcedureID;
            CurrentFlow = surgery.FlowID;
            CurrentRoomSetup = surgery.RoomSetupID;
            CurrentPatient = surgery.PatientID;
        }

        private static AuthToken _authToken;
        public static User CurrentUser { get; private set; }
        public static MessagingGroup CurrentMessagingGroup { get; set; }

        public static int? CurrentCard { get; set; }
        public static int? CurrentBundle { get; set; }
        public static int? CurrentProcedure { get; set; }
        public static int? CurrentFlow { get; set; }
        public static int? CurrentRoomSetup { get; set; }
        public static int? CurrentPatient { get; set; }

        public static string CurrentUserTitle => string.Format("{0} {1}", CurrentUser?.Title, CurrentUser?.LastName);

        public static bool UserAuthenticated => _authToken != null && _authToken.ExpiresDate > DateTime.Now;
        
        public static async Task AuthenticateUser(string username, string password)
        {
            _authToken = await WebUtility.LoginUser(username, password);
            
            if (_authToken != null)
                CurrentUser = await UserUtil.GetUser();
        }

        public static string AuthenticationToken => _authToken?.AccessToken;


        public static void SignOutUser()
        {
            _authToken = null;
            CurrentUser = null;

            CurrentSurgery = null;
            CurrentPatient = null;
        }

        public static async Task<List<Room>> RoomList(int locationId)
        {
            if (!_roomDictionary.ContainsKey(locationId))
            {
                _roomDictionary[locationId] = await LookupUtil.GetRooms();
            }

            return _roomDictionary[locationId];
        }

        public static async Task<Room> GetRoom(int locationId, int roomId)
        {
            var rooms = await RoomList(locationId);

            return rooms.FirstOrDefault(r => r.RoomID == roomId);
        }
    }
}
