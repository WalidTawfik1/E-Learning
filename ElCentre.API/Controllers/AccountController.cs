﻿using AutoMapper;
using ElCentre.API.Helper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElCentre.API.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IGenerateToken _generateToken;
        private readonly ElCentreDbContext _context;
        public AccountController(IUnitofWork work, IMapper mapper, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, IGenerateToken generateToken, ElCentreDbContext context) : base(work, mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _generateToken = generateToken;
            _context = context;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="registerDTO">The registration details.</param>
        /// <returns>A response indicating the result of the registration.</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            var result = await work.Authentication.RegisterAsync(registerDTO);
            if (result != "User Created Successfully")
            {
                return BadRequest(new APIResponse(400, result));
            }
            return Ok(new APIResponse(200, result));
        }

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="loginDTO">The login details.</param>
        /// <returns>A response indicating the result of the login.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var result = await work.Authentication.LoginAsync(loginDTO);
            if (result.StartsWith("Please") || result.StartsWith("Invalid"))
            {
                return BadRequest(new APIResponse(400, result));
            }
            Response.Cookies.Append("token", result, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true,
                IsEssential = true,
                Expires = DateTime.Now.AddDays(7)
            });
            return Ok(new APIResponse(200, result));
        }

        /// <summary>
        /// Activates a user account.
        /// </summary>
        /// <param name="activeAccountDTO">The activation details.</param>
        /// <returns>A response indicating the result of the activation.</returns>
        [AllowAnonymous]
        [HttpPost("active-account")]
        public async Task<IActionResult> ActiveAccount(ActiveAccountDTO activeAccountDTO)
        {
            var result = await work.Authentication.ActiveAccount(activeAccountDTO);
            return result ? Ok(new APIResponse(200, "Account Activated Successfully")) :
                BadRequest(new APIResponse(200, "Please activate your account"));
        }

        /// <summary>
        /// Sends an email for password reset.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns>A response indicating the result of the email sending.</returns>
        [AllowAnonymous]
        [HttpGet("send-email-forget-password")]
        public async Task<IActionResult> SendEmailForgetPassword(string email)
        {
            var result = await work.Authentication.SendEmailForgetPassword(email);
            return result ? Ok(new APIResponse(200, "Email Sent Successfully")) : 
                BadRequest(new APIResponse(200, "Email Not Sent"));
        }

        /// <summary>
        /// Resets the user password.
        /// </summary>
        /// <param name="resetPasswordDTO">The password reset details.</param>
        /// <returns>A response indicating the result of the password reset.</returns>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var result = await work.Authentication.ResetPassword(resetPasswordDTO);
            if (result != "Password Reset Successfully")
            {
                return BadRequest(new APIResponse(400, result));
            }
            return Ok(new APIResponse(200, result));
        }

        /// <summary>
        /// Logs out a user.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("token");
            return Ok(new APIResponse(200, "Logged out Successfully"));
        }
    }
}
