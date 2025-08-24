using Microsoft.AspNetCore.Mvc;
using UserRegistrationApp.Data;
using UserRegistrationApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using UserRegistrationApp.Components.Pages;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace UserRegistrationApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        // Add logger
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/user/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            _logger.LogInformation("Register endpoint called with Username: {Username}, Email: {Email}", request.Username, request.Email);

            if (request == null)
            {
                _logger.LogWarning("Register request is null.");
                return BadRequest("Request body is null.");
            }

            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Registration failed: Missing required fields.");
                return BadRequest("Username, Email, and Password are required.");
            }

            
            try
            {
                // Checking if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    _logger.LogWarning("Registration failed: Email {Email} is already registered.", request.Email);
                    return BadRequest("Email is already registered");
                }
                // Create User entity
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {Username} registered successfully.", user.Username);

                return Ok(new RegisterResponse
                {
                    Message = "Account created successfully!",
                    Username = user.Username
                });

            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while registering user with email: {Email}", request.Email);
                return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user {Username}.", request.Username);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Test endpoint hit.");
            return Ok("Test endpoint reached.");
        }
        

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }

    
}
