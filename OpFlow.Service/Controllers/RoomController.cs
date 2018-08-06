using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Data.Debrief;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class RoomController : ApiController
    {
        // GET api/values
        [SwaggerOperation("GetByLocationId")]
        [Route("api/room")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Room>))]
        [HttpGet]
        public IEnumerable<Room> Get(int? roomId = null, int? roomGroupId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var rooms = DataAccess.SqlHelper.GetRooms(user.LocationID);
            if (roomId.HasValue)
                rooms = rooms.Where(r => r.RoomID == roomId).ToList();

            if (roomGroupId.HasValue)
                rooms = rooms.Where(r => r.RoomGroupID == roomGroupId).ToList();

            return rooms;
        }

        // GET api/values
        [SwaggerOperation("GetTypes")]
        [Route("api/room/types")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RoomType>))]
        public IEnumerable<RoomType> GetTypes()
        {
            var user = CacheUtil.GetUserSecurity();

            return DataAccess.SqlHelper.GetRoomTypes(user.LocationID);
        }

        // GET api/values
        [SwaggerOperation("GetGroups")]
        [Route("api/room/groups")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RoomGroup>))]
        public IEnumerable<RoomGroup> GetGroups()
        {
            var user = CacheUtil.GetUserSecurity();

            return DataAccess.SqlHelper.GetRoomGroups(user.ProviderID, user.LocationID);
        }

        // GET api/values
        [SwaggerOperation("GetSetups")]
        [Route("api/room/setups")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RoomSetup>))]
        public IEnumerable<RoomSetup> GetSetups(int? roomSetupId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var setups = DataAccess.SqlHelper.GetRoomSetups(roomSetupId, user.ProviderID, user.LocationID);

            if (roomSetupId != null)
                setups = setups.Where(s => s.RoomSetupID == roomSetupId).ToList();

            return setups;
        }

        // GET api/values
        [SwaggerOperation("GetSetupDetail")]
        [Route("api/room/setupDetail")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RoomSetupDetail))]
        public RoomSetupDetail GetSetupDetail(int? roomSetupId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var setups = DataAccess.SqlHelper.GetRoomSetups(roomSetupId, user.ProviderID, user.LocationID)
                .Where(s => s.RoomSetupID == roomSetupId).ToList();
            
            var roomTypes = DataAccess.SqlHelper.GetRoomTypes(user.LocationID);
            var patientPositions = DataAccess.SqlHelper.GetPatientPositions(user.ProviderID, user.LocationID);
            var lateralities = DataAccess.SqlHelper.GetLateralities(user.ProviderID, user.LocationID);
            var bedOrientations = DataAccess.SqlHelper.GetBedOrientations(user.ProviderID, user.LocationID);

            var equipment = DataAccess.SqlHelper.GetItems("EQUIPMENT", null, null, user.ProviderID, user.LocationID);
            var instruments = DataAccess.SqlHelper.GetItems("INSTRUMENT", null, null, user.ProviderID, user.LocationID);

            var result = new RoomSetupDetail()
            {
                RoomSetups = setups,
                RoomTypes = roomTypes,
                PatientPositions = patientPositions,
                BedOrientations = bedOrientations,
                Lateralities = lateralities,
                EquipmentItems = equipment,
                InstrumentItems = instruments
            };

            return result;
        }

        // GET api/values
        [SwaggerOperation("GetPatientPositions")]
        [Route("api/room/patientPositions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<PatientPosition>))]
        public IEnumerable<PatientPosition> GetPatientPositions()
        {
            var user = CacheUtil.GetUserSecurity();

            return DataAccess.SqlHelper.GetPatientPositions(user.ProviderID, user.LocationID);
        }

        // GET api/values
        [SwaggerOperation("GetLateralities")]
        [Route("api/room/lateralities")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Laterality>))]
        public IEnumerable<Laterality> GetLateralities()
        {
            var user = CacheUtil.GetUserSecurity();

            return DataAccess.SqlHelper.GetLateralities(user.ProviderID, user.LocationID);
        }

        // GET api/values
        [SwaggerOperation("GetBedOrientations")]
        [Route("api/room/bedOrientations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<BedOrientation>))]
        public IEnumerable<BedOrientation> GetBedOrientations()
        {
            var user = CacheUtil.GetUserSecurity();

            return DataAccess.SqlHelper.GetBedOrientations(user.ProviderID, user.LocationID);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Update")]
        [Route("api/room")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPut]
        public HttpResponseMessage PutRoom(int roomId, [FromBody]RoomPost room)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.UpdateRoom(roomId, room.Description, room.RoomTypeID, room.RoomGroupID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roomId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Create")]
        [Route("api/room")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        public HttpResponseMessage PostRoom([FromBody]RoomPost room)
        {
            var user = CacheUtil.GetUserSecurity();

            var roomId = DataAccess.SqlHelper.InsertRoom(room.Description, room.RoomTypeID, room.RoomGroupID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roomId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Delete")]
        [Route("api/room")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        public HttpResponseMessage DeleteRoom(int roomId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeleteRoom(roomId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roomId);
        }

        // POST api/roomSetup/values
        [SwaggerOperation("CreateSetup")]
        [Route("api/room/roomSetup")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [HttpPost]
        public HttpResponseMessage Post([FromBody]RoomSetup roomSetup)
        {
            var user = CacheUtil.GetUserSecurity();

            var roomSetupId = DataAccess.SqlHelper.CreateRoomSetup(roomSetup, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roomSetupId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("UpdateSetup")]
        [Route("api/room/roomSetup/{roomsetupId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPut]
        public HttpResponseMessage PutRoomSetup(int roomsetupId, [FromBody]RoomSetup roomSetup)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.UpdateRoomSetup(roomsetupId, user.ProviderID, user.LocationID, roomSetup);

            return Request.CreateResponse(HttpStatusCode.OK, roomsetupId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("DeleteSetup")]
        [Route("api/room/roomSetup/{roomsetupId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteSetup(int roomsetupId, [FromBody]RoomSetup roomSetup)
        {
            var user = CacheUtil.GetUserSecurity();

            await DataAccess.SqlHelper.DeleteRoomSetup(roomsetupId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roomsetupId);
        }

        // POST api/values
        [SwaggerOperation("NewRoomSetupImage")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/room/roomSetupImage", Name = "NewRoomSetupImage")]
        [HttpPut]
        public async Task<IHttpActionResult> NewRoomSetupImage(int roomSetupId, string label = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // extract file name and file contents
            var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                .FirstOrDefault(p => p.Name.ToLower() == "filename");
            var fileName = fileNameParam?.Value.Trim('"') ?? "";
            var fileExtension = Path.GetExtension(fileName);
            var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

            var roomSetupImageId = SqlHelper.NewRoomSetupImage(roomSetupId, label,
                user.ProviderID, user.LocationID);

            if (fileExtension == ".png" ||
                fileExtension == ".jpg" ||
                fileExtension == ".jpeg")
            {
                fileContents = BlobStorageHelper.CompressImage(fileContents);
            }

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.RoomSetupImages, roomSetupId);
            await BlobStorageHelper.PutBlobBytes(folder, roomSetupImageId.ToString(), fileContents);

            return Ok(roomSetupImageId);
        }

        // POST api/values
        [SwaggerOperation("UpdateRoomSetupImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/room/roomSetupImage", Name = "UpdateRoomSetupImage")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateRoomSetupImage(int roomSetupImageId, [FromBody]FlowImagePost flowImage)
        {
            var user = CacheUtil.GetUserSecurity();

            await SqlHelper.UpdateRoomSetupImage(roomSetupImageId, flowImage.Comment, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("RotateRoomSetupImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/room/rotateRoomSetupImage", Name = "RotateRoomSetupImage")]
        [HttpPut]
        public async Task<IHttpActionResult> RotateRoomSetupImage(int roomSetupImageId, int roomSetupId, int direction)
        {
            var user = CacheUtil.GetUserSecurity();

            // Make sure valid rotation direction
            if (direction != 1 && direction != -1)
                return Ok();


            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.RoomSetupImages, roomSetupId);
            await BlobStorageHelper.RotateImage(folder, roomSetupImageId.ToString(), direction);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteRoomSetupImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/room/roomSetupImage", Name = "DeleteRoomSetupImage")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteRoomSetupImage(int roomSetupImageId, int roomSetupId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeleteRoomSetupImage(roomSetupImageId, user.ProviderID, user.LocationID);

            var folder = DataAccess.BlobStorageHelper.Folder(BlobStorageHelper.ImageType.RoomSetupImages, roomSetupId);
            await DataAccess.BlobStorageHelper.DeleteBlob(folder, roomSetupImageId.ToString());

            return Ok();
        }
    }
}