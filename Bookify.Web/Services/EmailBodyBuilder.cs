using Microsoft.AspNetCore.Hosting;
using System.Text.Encodings.Web;

namespace Bookify.Web.Services
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _WebHostEnvironment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
        {
            _WebHostEnvironment = webHostEnvironment;
        }

        public string GenerateEmailBody(string imageUrl, string header, string body, string url, string linkTitle)
        {
            //READ the HTML content of the template
            var templatePath = $"{_WebHostEnvironment.WebRootPath}/templates/email.html";
            StreamReader str = new StreamReader(templatePath);
            var template = str.ReadToEnd();
            str.Close();

            template = template.Replace("[imageUrl]", imageUrl)
                .Replace("[header]", header)
                .Replace("[body]", body)
                .Replace("[url]", url)
                .Replace("[linkTitle]", linkTitle);

            return template;
        }
    }
}
