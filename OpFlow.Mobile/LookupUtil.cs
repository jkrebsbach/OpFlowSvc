using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public static class LookupUtil
    {
        public static async Task<List<Specialty>> GetSpecialties()
        {
            var command = string.Format("api/specialty");
            var response = await WebUtility.WebRequest<List<Specialty>>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<List<Gender>> GetGenders()
        {
            var genders = new List<Gender>()
            {
                new Gender() {GenderID = 1, GenderName = "Male"},
                new Gender() {GenderID = 2, GenderName = "Female"}
            };

            return genders;
        }
        public static async Task<List<CardBundle>> GetBundles(int specialtyId)
        {
            var command = string.Format("api/bundle?specialtyId={0}", specialtyId);
            var response = await WebUtility.WebRequest<List<CardBundle>>(command, HttpMethod.Get);

            return response;
        }
        public static async Task<List<Room>> GetRooms()
        {
            var command = string.Format("api/room");

            var response = await WebUtility.WebRequest<List<Room>>(command, HttpMethod.Get);

            return response;
        }
    }
}
