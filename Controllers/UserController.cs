using Microsoft.AspNetCore.Mvc;
using UserRegistrationApp.Data;
using UserRegistrationApp.Data.Models;
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
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;

        // private readonly ApplicationDbContext _context;
        // Add logger
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, IEmailService emailService, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
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

                    // Generate email confirmation token
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Create confirmation link
                    var confirmationLink = Url.Action(
                        "ConfirmEmail",
                        "User",
                        new { userId = user.Id, token = token },
                        Request.Scheme);

                    // Send confirmation email
                    try
                    {
                        await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink!);
                        _logger.LogInformation("Confirmation email sent to {Email}", user.Email);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send confirmation email to {Email}", user.Email);
                        // Don't fail registration if email fails - user can request resend later
                    }

                    return Ok(new RegisterResponse
                    {
                        Message = "Account created successfully! Please check your email to confirm your account.",
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

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            _logger.LogInformation("Email confirmation attempt for UserId: {UserId}", userId);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Email confirmation failed: Missing userId or token");
                return Redirect("/email-confirmed?error=Invalid email confirmation link");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation failed: User not found with ID {UserId}", userId);
                return Redirect("/email-confirmed?error=Invalid email confirmation link");
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Email already confirmed for user {UserId}", userId);
                return Redirect("/email-confirmed?success=true");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email confirmed successfully for user {UserId}", userId);
                return Redirect("/email-confirmed?success=true");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Email confirmation failed for user {UserId}: {Errors}", userId, errors);
                return Redirect("/email-confirmed?error=Email confirmation failed");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            if (request == null)
            {
                _logger.LogWarning("Login request is null.");
                return BadRequest("Request body is null.");
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login failed: Missing email or password.");
                return BadRequest("Email and Password are required.");
            }

            try
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found with email {Email}", request.Email);
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    });
                }

                // Check if email is confirmed
                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Login failed: Email not confirmed for user {Email}", request.Email);
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Please confirm your email address before logging in. Check your inbox for the confirmation email."
                    });
                }

                // Attempt to sign in
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    request.Password,
                    request.RememberMe,
                    lockoutOnFailure: true); // Enable lockout for security

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", request.Email);
                    return Ok(new LoginResponse
                    {
                        Success = true,
                        Message = "Login successful!",
                        Username = user.UserName!,
                        Email = user.Email!,
                        EmailConfirmed = user.EmailConfirmed
                    });
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {Email} account is locked out", request.Email);
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Account is temporarily locked due to multiple failed login attempts. Please try again later."
                    });
                }
                else if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("User {Email} requires two-factor authentication", request.Email);
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Two-factor authentication required."
                    });
                }
                else
                {
                    _logger.LogWarning("Login failed: Invalid password for user {Email}", request.Email);
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in user {Email}", request.Email);
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your request."
                });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User logout requested");

            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out successfully");

                return Ok(new { Success = true, Message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                return StatusCode(500, new { Success = false, Message = "An error occurred during logout." });
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
