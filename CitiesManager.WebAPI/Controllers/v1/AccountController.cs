using CitiesManager.Core.DTO;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace CitiesManager.WebAPI.Controllers.v1
{
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("api/[controller]/{version:ApiVersion?}")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtService _jwtService;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IJwtService jwtService) {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApplicationUser>> PostRegister(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                string errMessages = string.Join(" | ", ModelState.Values.SelectMany<ModelStateEntry, ModelError>(modelStateEntry => modelStateEntry.Errors).Select(err => err.ErrorMessage));
                return Problem(errMessages, statusCode: 400);
            }

            ApplicationUser user = new ApplicationUser()
            {
                UserName = registerDTO.Email,
                PersonName = registerDTO.PersonName,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber,
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);
                user.RefreshToken = authenticationResponse.RefreshToken;
                user.RefreshTokenExpiration = authenticationResponse.RefreshTokenExpiration;
                await _userManager.UpdateAsync(user);
                return Ok(authenticationResponse);
            }
            else
            {
                string errMsgs = string.Join(" | ", result.Errors.Select(err => err.Description));
                return Problem(errMsgs, statusCode: 400);
            }
        }

        [HttpGet("isemailalreadyregistered")]
        public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Ok(false);
            }
            else
            {
                return Ok(true);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                string errMessages = string.Join(" | ", ModelState.Values.SelectMany<ModelStateEntry, ModelError>(modelStateEntry => modelStateEntry.Errors).Select(err => err.ErrorMessage));
                return Problem(errMessages, statusCode: 400);
            }

           Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                ApplicationUser user = await _userManager.FindByEmailAsync(loginDTO.Email);
                AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);
                user.RefreshToken = authenticationResponse.RefreshToken;
                user.RefreshTokenExpiration = authenticationResponse.RefreshTokenExpiration;
                await _userManager.UpdateAsync(user);
                return Ok(authenticationResponse);
            }
            return Problem("Invalid email or password", statusCode: 400);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return NoContent();
        }

        [HttpPost("generate-new-jwt-token")]
        public async Task<IActionResult> GenerateNewAccessToken(TokenModel tokenModel)
        {
            if(tokenModel.Token == null || tokenModel.RefreshToken == null)
            {
                return BadRequest("Invalid client request");
            }

            string jwtToken = tokenModel.Token;
            string refreshToken = tokenModel.RefreshToken;  
            
            try {
                ClaimsPrincipal claimsPrincipal = _jwtService.GetPrincipalFromJwtToken(jwtToken);
                var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiration <= DateTime.Now) {
                    return BadRequest("Invalid client request");
                }
                AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);
                user.RefreshToken = authenticationResponse.RefreshToken;
                user.RefreshTokenExpiration = authenticationResponse.RefreshTokenExpiration;
                await _userManager.UpdateAsync(user);
                return Ok(authenticationResponse);

            }
            catch (SecurityTokenException securityTokenException) {
                return BadRequest(securityTokenException.Message);
            }



        }
    }
}


