using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SignupController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Signup signup)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ? Check if phone number already exists
            var existingPhone = await _context.Signups.FirstOrDefaultAsync(s => s.Phone_Number == signup.Phone_Number);
            if (existingPhone != null)
                return Conflict(new { field = "phoneNumber", message = "User already registered with this phone number." });

            var existingEmail = await _context.Signups.FirstOrDefaultAsync(s => s.Email == signup.Email);
            if (existingEmail != null)
                return Conflict(new { field = "email", message = "User already registered with this email." });


            // Hash the password before saving
            signup.Password = BCrypt.Net.BCrypt.HashPassword(signup.Password);

            // PostgreSQL requires UTC DateTime
            signup.Dob = DateTime.SpecifyKind(
                signup.Dob,
                DateTimeKind.Utc
            );

            _context.Signups.Add(signup);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Signup successful",
                data = signup
            });
        }

        // GET: api/Signup/names
        [HttpGet("names")]
        public async Task<IActionResult> GetAllUserNames()
        {
            var names = await _context.Signups
                .Select(u => u.Name)
                .ToListAsync();

            return Ok(names);
        }

        [HttpGet("profile/{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            var user = await _context.Signups
                .FirstOrDefaultAsync(u => u.Email == username || u.Phone_Number == username);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                name = user.Name,
                dob = user.Dob,
                gender = user.Gender,
                phone = user.Phone_Number,
                email = user.Email
            });
        }

        //[HttpPost("upload-profile-picture")]
        //public async Task<IActionResult> UploadProfilePicture([FromForm] IFormFile file, [FromQuery] string username)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("No file uploaded.");

        //    var user = await _context.Signups.FirstOrDefaultAsync(u => u.Email == username || u.Phone_Number == username);
        //    if (user == null) return NotFound("User not found.");

        //    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        //    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/profile-images", fileName);

        //    using (var stream = new FileStream(savePath, FileMode.Create))
        //    {
        //        await file.CopyToAsync(stream);
        //    }

        //    user.ProfileImageUrl = $"/profile-images/{fileName}";
        //    _context.Signups.Update(user);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { imageUrl = user.ProfileImageUrl });
        //}




    }
}
