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

        public string GenerateEmailBody(string template, Dictionary<string,string> placeholders)
        {
            //READ the HTML content of the template
            var templatePath = $"{_WebHostEnvironment.WebRootPath}/templates/{template}.html";
            StreamReader str = new StreamReader(templatePath);
            var templateContent = str.ReadToEnd();
            str.Close();

            foreach (var placeholder in placeholders)
                templateContent = templateContent.Replace($"[{placeholder.Key}]", placeholder.Value);

            return templateContent;
        }
    }
}
