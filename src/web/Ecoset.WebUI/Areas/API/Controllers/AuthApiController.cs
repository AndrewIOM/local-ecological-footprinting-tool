using System;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Ecoset.WebUI.Models.AccountViewModels;
using Ecoset.WebUI.Models;
using Microsoft.AspNetCore.Http;
using Ecoset.WebUI.Models.JobViewModels;

namespace Ecoset.WebUI.Areas.API.Controllers {

    [Authorize]
    [Area("Api")]
    [Route("/api/v1/auth")]
    public class AuthApiController : Controller {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthApiController> logger,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Generate a bearer token to use the secured data packages API. You
        /// must add the resultant token into the jwt_auth header of requests
        /// </summary>
        /// <param name="model">Your login details</param>
        /// <returns>A bearer token to be included in request headers</returns>
        /// <response code="200">Returns the bearer token</response>
        /// <response code="400">If the token could not be generated</response>     
        [AllowAnonymous]
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AuthToken), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Authenticate([FromBody] AuthRequest model) {
            if (ModelState.IsValid) {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null) {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                    if (result.Succeeded) {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
                        };
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(_config["Tokens:Issuer"],
                            _config["Tokens:Issuer"],
                            claims,
                            expires: DateTime.Now.AddMinutes(30),
                            signingCredentials: creds);
                        return Ok(new AuthToken() { Token = new JwtSecurityTokenHandler().WriteToken(token) });
                    }
                }
            }
        return BadRequest("Could not create token");
        }

    }
}