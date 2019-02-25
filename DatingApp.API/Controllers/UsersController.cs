using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    // https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-2.2#dependency-injection
    // https://www.devtrends.co.uk/blog/dependency-injection-in-action-filters-in-asp.net-core
    // The ServiceFilter attribute allows us to specify the type of our action filter.
    // We are writing this in controller level so that the user's lastActive is updated whenever any of the below actions are touched by the user
    [ServiceFilter(typeof(LogUserActivity))]     
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _datingRepo;
        private readonly IMapper _mapper;
        public UsersController(IDatingRepository datingRepo, IMapper mapper)
        {
            _mapper = mapper;
            _datingRepo = datingRepo;
        }

        [HttpGet]
        // we are just giving a nudge to the action by giving FromQuery, so that the action/postman knows
        // that the information is coming from the query string (since this is a get request, we cannot test this in
        // postman by giving information in the Body part)
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams) // for pagination
        {
            var currentUserID = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);            
            var userFromRepo = await _datingRepo.GetUser(currentUserID, true);
            userParams.UserID = currentUserID;

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                // setting the user params gender to the opposite so that we show male users to female and vice versa
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }
            
            // this would now contain a paged list of users and pagination information(instead of sending all the users unnecessarily)
            var users = await _datingRepo.GetUsers(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDTO>>(users);

            // APIController has access to HttpResponse (represents the outgoing side of an individual HTTP request) via variable Response
            Response.AddPagination(users.CurrentPage, users.PageSize, 
                        users.TotalCount, users.TotalPages);

            return Ok(usersToReturn); // now the response headers will have the pagination information
            // from the AddPagination method call in the above step
        }

        [HttpGet("{id}", Name="GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var isCurrentUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) == id;
            var user = await _datingRepo.GetUser(id, isCurrentUser);
            var userToReturn = _mapper.Map<UserForDetailedDTO>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDTO userForUpdateDTO)
        {
            // if the current user that is passed to our server matches the id
            // FindFirst - Retrieves the first claim with the specified claim type.
            // the ClaimType is connected to the ones declared in AuthController
            // https://docs.microsoft.com/en-us/dotnet/api/system.security.claims.claimsidentity.findfirst?view=netframework-4.7.2#System_Security_Claims_ClaimsIdentity_FindFirst_System_String_
            if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _datingRepo.GetUser(id, true);

            _mapper.Map(userForUpdateDTO, userFromRepo);

            // if save is successful then return no content
            if(await _datingRepo.SaveAll())
                return NoContent();
            
            // $ - string interpolation - i.e. instead of the usual '{0}...id we do in string formatting,
            // we can directly interpolate id as below
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated
            throw new Exception($"Updating user {id} failed on save");
        }

        // id is of the currently logged in user and recipient id is the id that the user likes
        // this would be called in user.service in the SPA
        [HttpPost("{id}/like/{recipientID}")]
        public async Task<IActionResult> LikeUser(int id, int recipientID)
        {
            if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var like = await _datingRepo.GetLike(id, recipientID);

            // if a like is already in place for the user
            if(like != null)
                return BadRequest("You already liked the user");

            if(await _datingRepo.GetUser(recipientID, false) == null)
                return NotFound();

            like = new Like
            {
                LikerID = id,
                LikeeID = recipientID
            };

            _datingRepo.Add<Like>(like);

            if(await _datingRepo.SaveAll())
                return Ok();

            return BadRequest("Failed to like user");
        }
    }
}