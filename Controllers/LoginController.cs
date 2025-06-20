//using EmployeeManagementAPI.Data;
//using EmployeeManagementAPI.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace EmployeeManagementAPI.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class LoginController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public LoginController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpPost]
//        public async Task<IActionResult> Login([FromBody] LoginRequest request)
//        {
//            if (string.IsNullOrWhiteSpace(request.EmailOrPhone) || string.IsNullOrWhiteSpace(request.Password))
//                return BadRequest("Username and password are required.");

//            Signup user = await _context.Signups
//                .FirstOrDefaultAsync(u => u.Email == request.EmailOrPhone || u.Phone_Number == request.EmailOrPhone);

//            if (user == null)
//                return Unauthorized("User not found.");

//            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
//            if (!isPasswordValid)
//                return Unauthorized("Invalid password.");

//            return Ok(new
//            {
//                Message = "Login successful",
//                UserId = user.Id,
//                Name = user.Name
//            });
//        }

//    }
//}

using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeManagementAPI.Helpers;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtHelper _jwtHelper;  // Add this

        public LoginController(ApplicationDbContext context, IConfiguration configuration, JwtHelper jwtHelper)
        {
            _context = context;
            _configuration = configuration;
            _jwtHelper = jwtHelper;  // Assign injected JwtHelper
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmailOrPhone) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            var user = await _context.Signups
                .FirstOrDefaultAsync(u => u.Email == request.EmailOrPhone || u.Phone_Number == request.EmailOrPhone);

            if (user == null)
                return Unauthorized("User not found.");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!isPasswordValid)
                return Unauthorized("Invalid password.");

            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            // Save refresh token and expiry to user in DB
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // For example, refresh token valid for 7 days

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Login successful",
                Token = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
                UserId = user.Id,
                Name = user.Name
            });
        }
        public class RefreshRequest
        {
            public string RefreshToken { get; set; }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest("Refresh token is required");

            var user = await _context.Signups.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null)
                return Unauthorized("Invalid refresh token");

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Unauthorized("Refresh token expired");

            // Generate new tokens
            var newAccessToken = _jwtHelper.GenerateAccessToken(user);
            var newRefreshToken = _jwtHelper.GenerateRefreshToken();

            // Update DB
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
                UserId = user.Id,
                Name = user.Name
            });
        }


    }
}
