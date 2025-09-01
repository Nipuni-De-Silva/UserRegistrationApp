namespace UserRegistrationApp.Data
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string confirmationLink);
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
