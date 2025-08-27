using Microsoft.AspNetCore.Mvc;
using UserRegistrationApp.Data;
using UserRegistrationApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace UserRegistrationApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        
        // private readonly ApplicationDbContext _context;
        // Add logger
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<User> userManager, ILogger<UserController> logger)
        {
            _userManager = userManager;
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
                // if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                // {
                //     _logger.LogWarning("Registration failed: Email {Email} is already registered.", request.Email);
                //     return BadRequest("Email is already registered");
                // }

                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: Email {Email} is already registered.", request.Email);
                    return BadRequest("Email is already registered");
                }

                // Create User entity
                var user = new User
                {
                    UserName = request.Username,
                    Email = request.Email,
                    CreatedAt = DateTime.UtcNow
                };

                // Use UserManager to create user with password
                var result = await _userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Username} registered successfully with ID: {UserId}", user.UserName, user.Id);

                    return Ok(new RegisterResponse
                    {
                        Message = "Account created successfully!",
                        Username = user.UserName
                    });
                }
                else
                {
                    // Log Identity errors
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Registration failed for {Username}: {Errors}", request.Username, errors);
                    
                    return BadRequest(new 
                    { 
                        Message = "Registration failed",
                        Errors = result.Errors.Select(e => e.Description).ToArray()
                    });
                }
            }
            // catch (DbUpdateException ex)
            // {
            //     _logger.LogError(ex, "Database error occurred while registering user with email: {Email}", request.Email);
            //     return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            // }
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
            return BCrypt.Net.BCrypt.HashPassword(password); // Use BCrypt for secure password hashing
        }
    }

    
}
