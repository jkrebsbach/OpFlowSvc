using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using OpFlow.Data;
using OpFlow.Data.Debrief;
using OpFlow.Service.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/room")]
    public class RoomController : OpFlowController
    {
        private BlobStorageHelper _blobStorageHelper;

        public RoomController(BlobStorageHelper blobStorageHelper,
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
            _blobStorageHelper = blobStorageHelper;
        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<Room>> Get(int? roomId = null, int? roomGroupId = null)
        {
            var user = await GetUserSecurity();
            

            var rooms = await _sqlHelper.GetRooms(user.SelectedLocation);
            if (roomId.HasValue)
                rooms = rooms.Where(r => r.RoomID == roomId).ToList();

            if (roomGroupId.HasValue)
                rooms = rooms.Where(r => r.RoomGroupID == roomGroupId).ToList();

            return rooms;
        }

        // GET api/values
        [Route("types")]
        public async Task<IEnumerable<RoomType>> GetTypes()
        {
            var user = await GetUserSecurity();
            

            return await _sqlHelper.GetRoomTypes(user.SelectedLocation);
        }

        // GET api/values
        [Route("groups")]
        public async Task<IEnumerable<RoomGroup>> GetGroups()
        {
            var user = await GetUserSecurity();
            

            return await _sqlHelper.GetRoomGroups(user.SelectedLocation);
        }

        // GET api/values
        [Route("setups")]
        public async Task<IEnumerable<RoomSetup>> GetSetups(int? roomSetupId = null)
        {
            var user = await GetUserSecurity();
            

            var setups = await _sqlHelper.GetRoomSetups(roomSetupId, user.SelectedLocation);

            if (roomSetupId != null)
                setups = setups.Where(s => s.RoomSetupID == roomSetupId).ToList();

            return setups;
        }

        // GET api/values
        [Route("setupDetail")]
        public async Task<RoomSetupDetail> GetSetupDetail(int? roomSetupId = null)
        {
            var user = await GetUserSecurity();
            

            var setups = (await _sqlHelper.GetRoomSetups(roomSetupId, user.SelectedLocation))
                .Where(s => s.RoomSetupID == roomSetupId).ToList();
            
            var roomTypes = await _sqlHelper.GetRoomTypes(user.SelectedLocation);
            var patientPositions = await _sqlHelper.GetPatientPositions(user.SelectedLocation);
            var lateralities = await _sqlHelper.GetLateralities(user.SelectedLocation);
            var bedOrientations = await _sqlHelper.GetBedOrientations(user.SelectedLocation);

            var equipment = await _sqlHelper.GetItems("EQUIPMENT", null, null, user.SelectedLocation);
            var instruments = await _sqlHelper.GetItems("INSTRUMENT", null, null, user.SelectedLocation);

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
        [Route("patientPositions")]
        public async Task<IEnumerable<PatientPosition>> GetPatientPositions()
        {
            var user = await GetUserSecurity();
            

            return await _sqlHelper.GetPatientPositions(user.SelectedLocation);
        }

        // GET api/values
        [Route("lateralities")]
        public async Task<IEnumerable<Laterality>> GetLateralities()
        {
            var user = await GetUserSecurity();
            

            return await _sqlHelper.GetLateralities(user.SelectedLocation);
        }

        // GET api/values
        [Route("bedOrientations")]
        public async Task<IEnumerable<BedOrientation>> GetBedOrientations()
        {
            var user = await GetUserSecurity();
            

            return await _sqlHelper.GetBedOrientations( user.SelectedLocation);
        }

        // PUT api/roomSetup/values/5
        [HttpPut]
        public async Task<ActionResult> PutRoom(int roomId, [FromBody]RoomPost room)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateRoom(roomId, room.Description, room.RoomTypeID, room.RoomGroupID, user.SelectedLocation);

            return Ok(roomId);
        }

        // PUT api/roomSetup/values/5
        [HttpPost]
        public async Task<ActionResult> PostRoom([FromBody]RoomPost room)
        {
            var user = await GetUserSecurity();
            

            var roomId = await _sqlHelper.InsertRoom(room.Description, room.RoomTypeID, room.RoomGroupID, user.SelectedLocation);

            return Ok(roomId);
        }

        // PUT api/roomSetup/values/5
        [HttpDelete]
        public async Task<ActionResult> DeleteRoom(int roomId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteRoom(roomId, user.SelectedLocation);

            return Ok(roomId);
        }

        // POST api/roomGroup/values
        [Route("roomGroup")]
        [HttpPost]
        public async Task<ActionResult> PostRoomGroup([FromBody] RoomGroup roomGroup)
        {
            var user = await GetUserSecurity();
            

            var roomSetupId = await _sqlHelper.CreateRoomGroup(roomGroup, user.SelectedLocation);

            return Ok(roomSetupId);
        }

        // POST api/roomSetup/values
        [Route("roomSetup")]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody]RoomSetup roomSetup)
        {
            var user = await GetUserSecurity();
            

            var roomSetupId = await _sqlHelper.CreateRoomSetup(roomSetup, user.SelectedLocation);

            return Ok(roomSetupId);
        }

        // PUT api/roomSetup/values/5
        [Route("roomSetup/{roomsetupId}")]
        [HttpPut]
        public async Task<ActionResult> PutRoomSetup(int roomsetupId, [FromBody]RoomSetup roomSetup)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateRoomSetup(roomsetupId, user.SelectedLocation, roomSetup);

            return Ok(roomsetupId);
        }

        // PUT api/roomSetup/values/5
        [Route("roomSetup/{roomsetupId}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteSetup(int roomsetupId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteRoomSetup(roomsetupId, user.SelectedLocation);

            return Ok(roomsetupId);
        }

        // POST api/values
        [Route("roomSetupImage", Name = "NewRoomSetupImage")]
        [HttpPut]
        public async Task<ActionResult> NewRoomSetupImage(IFormFile file, int roomSetupId, string label = null)
        {
            var user = await GetUserSecurity();

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }


            // extract file name and file contents
            var fileName = file.FileName;
            var fileExtension = Path.GetExtension(fileName);
            byte[] fileContents;
            using (var stream = file.OpenReadStream())
            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);

                fileContents = memStream.ToArray();
            }

            // Make sure there's actually a valid room setup to assign this image to...
            var roomSetup = await _sqlHelper.GetRoomSetup(roomSetupId, user.SelectedLocation);
            if (roomSetup == null)
                return Ok();

            var roomSetupImageId = await _sqlHelper.NewRoomSetupImage(roomSetupId, label,
                user.SelectedLocation);

            if (fileExtension == ".png" ||
                fileExtension == ".jpg" ||
                fileExtension == ".jpeg")
            {
                fileContents = BlobStorageHelper.CompressImage(fileContents);
            }

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.RoomSetupImages, roomSetupId);


            await _blobStorageHelper.PutBlobBytes(folder, roomSetupImageId.ToString(), fileContents);

            return Ok(roomSetupImageId);
        }

        // POST api/values
        [Route("roomSetupImage", Name = "UpdateRoomSetupImage")]
        [HttpPost]
        public async Task<ActionResult> UpdateRoomSetupImage(int roomSetupImageId, [FromBody]FlowImagePost flowImage)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateRoomSetupImage(roomSetupImageId, flowImage.Comment, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("rotateRoomSetupImage", Name = "RotateRoomSetupImage")]
        [HttpPut]
        public async Task<ActionResult> RotateRoomSetupImage(int roomSetupImageId, int roomSetupId, int direction)
        {
            var user = await GetUserSecurity();

            // Make sure valid rotation direction
            if (direction != 1 && direction != -1)
                return Ok();


            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.RoomSetupImages, roomSetupId);


            await _blobStorageHelper.RotateImage(folder, roomSetupImageId.ToString(), direction);

            return Ok();
        }

        // POST api/values
        [Route("roomSetupImage", Name = "DeleteRoomSetupImage")]
        [HttpDelete]
        public async Task<ActionResult> DeleteRoomSetupImage(int roomSetupImageId, int roomSetupId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteRoomSetupImage(roomSetupImageId, user.SelectedLocation);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.RoomSetupImages, roomSetupId);


            await _blobStorageHelper.DeleteBlob(folder, roomSetupImageId.ToString());

            return Ok();
        }
    }
}