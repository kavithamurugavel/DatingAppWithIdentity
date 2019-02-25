using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        // IOptions is a pattern for custom configurations. We have to do the following steps: 1) Create a class with properties that we want
        // to match with step 2 2) a new custom config section in appSettings.json. The class and appsettings should match 3) Inject it as a service
        // in Startup.cs using services.Configure<Settings>(Configuration.GetSection("Settings")) 4) Inject Ioptions into the constructor of the controller 
        // where we want to use the config settings for eg: the ctor below
        // For more explanation: https://andrewlock.net/how-to-use-the-ioptions-pattern-for-configuration-in-asp-net-core-rc2/
        private readonly IDatingRepository _datingRepo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository datingRepo, IMapper mapper,
        IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _datingRepo = datingRepo;

            // cloudinary config
            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        // created to get the route with a name (used in CreatedAtRoute part in AddPhotoForUser)
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _datingRepo.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDTO>(photoFromRepo);

            return Ok(photo);
        }


        // for explanations of cloudinary snippets below: https://cloudinary.com/documentation/dotnet_integration#overview
        [HttpPost]
        // we give FromForm so that .Net Core a hint that the information is coming the form
        // otherwise we get input not valid error.
        public async Task<IActionResult> AddPhotoForUser(int userID, 
                [FromForm]PhotoForCreationDTO photoForCreationDTO)
        {
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _datingRepo.GetUser(userID, true);

            var file = photoForCreationDTO.File;

            // result we get back from cloudinary
            var uploadResult = new ImageUploadResult();

            if(file.Length > 0)
            {
                using(var stream = file.OpenReadStream()) // Opens the request stream for reading the uploaded file.
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        // this is to transform photo square shape to round for eg
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            // Each media asset uploaded to Cloudinary is assigned an unique Public ID and is available for immediate delivery and transformation. 
            // The upload method returns an instance of ImageUploadResult class that contains a set of properties 
            // of the uploaded image, including the image URL and Public ID, which is what we are using below
            photoForCreationDTO.Url = uploadResult.Uri.ToString();
            photoForCreationDTO.PublicID = uploadResult.PublicId;

            // map the photo to dto
            var photo = _mapper.Map<Photo>(photoForCreationDTO); 
            // alternate to the above is Photo photo = new Photo();
            // _mapper.Map(photoForCreationDTO, photo);

            // if this is the first photo and hence no main photo set for the user
            if(!userFromRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;
            
            userFromRepo.Photos.Add(photo);

            if(await _datingRepo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDTO>(photo);
                // returning location header with the location of the created results
                // CreatedAtRoute adds a Location header to the response. The Location header specifies the URI of the newly created to-do item.
                // for eg: http://localhost:5000/api/users/1/photos/11 after the photo with id 11 is uploaded
                return CreatedAtRoute("GetPhoto", new {id = photo.ID}, photoToReturn);
            }

            return BadRequest("Could not add the photo");
        }

        // this should have been HttpPut or HttpPatch acc. to RESTful standards, but since
        // this is just a simple change from false to true, we'll stick to HttpPost
        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userID, int id)
        {
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _datingRepo.GetUser(userID, true);
            // checking if the user is updating his own photo
            if(!user.Photos.Any(p => p.ID == id))
                return Unauthorized();

            var photoFromRepo = await _datingRepo.GetPhoto(id);

            if(photoFromRepo.IsMain)
                return BadRequest("This is already a main photo");
            
            var currentMainPhoto = await _datingRepo.GetMainPhotoForUser(userID);
            currentMainPhoto.IsMain = false;
            photoFromRepo.IsMain = true;

            if(await _datingRepo.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo to main");
        }

        // deleting a photo entails both deleting from cloudinary and DB
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userID, int id)
        {
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _datingRepo.GetUser(userID, true);
            // checking if the user is updating his own photo
            if(!user.Photos.Any(p => p.ID == id))
                return Unauthorized();

            var photoFromRepo = await _datingRepo.GetPhoto(id);

            if(photoFromRepo.IsMain)
                return BadRequest("You cannot delete your main photo");

            // cloduinary delete
            if(photoFromRepo.PublicID != null)
            {
                // for deleting from cloudinary: https://cloudinary.com/documentation/image_upload_api_reference#destroy_method
                var deleteParams = new DeletionParams(photoFromRepo.PublicID);
                var result = _cloudinary.Destroy(deleteParams);

                // since cloudinary's destroy returns a result of OK if delete was successful
                if(result.Result == "ok") 
                {
                    // delete from repo if cloudinary delete was successful
                    _datingRepo.Delete(photoFromRepo);
                }
            }

            // delete photo not in cloudinary 
            if(photoFromRepo.PublicID == null)
                _datingRepo.Delete(photoFromRepo);

            if(await _datingRepo.SaveAll())
                return Ok();
            
            return BadRequest("Failed to delete the photo");
        }
}
}