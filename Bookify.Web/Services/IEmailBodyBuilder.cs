namespace Bookify.Web.Services
{
    public interface IEmailBodyBuilder
    {
        string GenerateEmailBody(string template, Dictionary<string, string> placeholders);
    }
}
