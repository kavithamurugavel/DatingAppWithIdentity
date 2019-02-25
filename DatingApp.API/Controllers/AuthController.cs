using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DatingApp.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(IConfiguration configuration, IMapper mapper,
        UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
        }

        [HttpPost("register")] // specifying that this http post is for register
        public async Task<IActionResult> Register(UserForRegisterDTO userForRegisterDTO) // if we don't add ApiController annotation here, then for validating the request, 
        // we have to add [FromBody] before an actions's parameter list, and also add the 
        // if(!ModelState.IsValid) condition first thing in the body of the action method (like in MVC controller course). Section 3 Lecture 31
        {
            var userToCreate = _mapper.Map<User>(userForRegisterDTO);

            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDTO.Password);

            var userToReturn = _mapper.Map<UserForDetailedDTO>(userToCreate);

            if(result.Succeeded)
            {
                // GetUser is from UsersController. Also we are returning userToReturn(i.e. UserForDetailDTO) instead of User
                // because we don't need to be sending the password by mistake
                return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);
            }
            return BadRequest(result.Errors);
        }

        // https://en.wikipedia.org/wiki/JSON_Web_Token - also Section 3 Lecture 32-33
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDTO userForLoginDTO)
        {
            var user = await _userManager.FindByNameAsync(userForLoginDTO.Username);

            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDTO.Password, false);

            if(result.Succeeded)
            {
                var appUser = await _userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(u => u.NormalizedUserName == userForLoginDTO.Username.ToUpper());

                var userToReturn = _mapper.Map<UserForListDTO>(appUser);

                // write the token created as a response we are sending back to client
                return Ok(new
                {
                    // if we don't mention .Result below, then the token comes back as a task object with other properties
                    // the token we need as a string is being stored in the Result property of that object (Sec 20 Lec 208) 
                    token = GenerateJwtToken(appUser).Result,
                    user = userToReturn
                });
            }

            return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            // token to contain two claims
            // we cannot use array of claims here since we are adding roles below (array is fixed length)
            // so we are using list instead
            var claims = new List<Claim>
            {
                // checking the id and the user name
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // making sure that the token that comes back from client is a valid token, we need to sign it
            // key to sign the token, which will be hashed
            // also we have to store this key in appsettings, which is why we need the configuration/appsettings part
            var key = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            // signing credentials with the key created above
            // encrypting the key with SHA512
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // creating a security token descriptor, which contain claims, expiry date of token and signing credentials
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}