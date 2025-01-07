using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using apiDealManagement.Models;
using apiDealManagement.Models.Requests;
using apiDealManagement.Service;

namespace apiDealManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AuthenController : ControllerBase
    {
        private IUserService UserService;
        private readonly IOptions<ConnectionStringConfig> ConnectionString;
        public AuthenController(IOptions<ConnectionStringConfig> ConnectionString, IUserService userService)
        {
            this.ConnectionString = ConnectionString;
            this.UserService = userService;
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login( [FromBody] AuthenticateRequest model)
        {
            try
            {
                var response = UserService.Authenticate(model);

                if (response == null)
                    return BadRequest(new { message = "Username or password is incorrect" });

                return Ok(response);
                //return Ok(new { response = response, data = user });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new { message = ex.Message.ToString() });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("Logout")]
        public IActionResult Logout()
        {
            string token = Request.Headers.FirstOrDefault(x => x.Key == "Authorization").Value.FirstOrDefault()?.Split(" ").Last();
            var response = UserService.Logout(token);
            return Ok(response);
        }

    }
    public class FormUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
