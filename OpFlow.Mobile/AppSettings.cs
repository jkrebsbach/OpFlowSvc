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
            FutureCases = 1,
            Schedule = 2,
            CheckIn = 3,
            CaseDetail = 4,
            CaseNavigate = 5,
            Flow = 6,
            Room = 7
        }

        public static int? CurrentSurgery { get; private set; }
        public static int? CurrentPatient { get; private set; }

        public static FragmentEnum CurrentScreen;

        private static readonly Dictionary<int, List<Room>> _roomDictionary = new Dictionary<int, List<Room>>();

        public static string CurrentScreenName
        {
            get
            {
                switch (CurrentScreen)
                {
                    case FragmentEnum.Login:
                        return "Login";
                    case FragmentEnum.FutureCases:
                        return "Cases";
                    case FragmentEnum.Schedule:
                        return "Schedule";
                    case FragmentEnum.CheckIn:
                        return "Check In";
                    case FragmentEnum.CaseDetail:
                        return "Case Detail";
                    case FragmentEnum.CaseNavigate:
                        return "Surgery";
                }

                return "UNDEFINED";
            }

        }

        public static async Task LoadSurgery(Surgery surgery, Patient patient)
        {
            CurrentSurgery = surgery.SurgeryID;
            CurrentPatient = patient.PatientID;
        }

        private static AuthToken _authToken;
        public static User CurrentUser { get; private set; }

        public static bool UserAuthenticated => _authToken?.ExpiresDate != null && _authToken.ExpiresDate > DateTime.Now;

        public static async Task AuthenticateUser(string username, string password)
        {
            _authToken = await WebUtility.LoginUser(username, password);

            if (_authToken != null)
                CurrentUser = await UserUtil.GetUser(username);
        }

        public static async Task<List<Room>> RoomList(int locationId)
        {
            if (!_roomDictionary.ContainsKey(locationId))
            {
                _roomDictionary[locationId] = await RoomUtil.GetRooms(locationId);
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
