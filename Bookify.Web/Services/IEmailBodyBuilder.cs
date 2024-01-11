namespace Bookify.Web.Services
{
    public interface IEmailBodyBuilder
    {
        string GenerateEmailBody(string imageUrl, string header, string body, string url, string linkTitle);
    }
}
