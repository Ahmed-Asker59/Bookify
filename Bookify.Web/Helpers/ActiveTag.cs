using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Bookify.Web.Helpers
{
    [HtmlTargetElement("a", Attributes = "active-when")]
    public class ActiveTag : TagHelper
    {
        //property that accepts
        public string? ActiveWhen { get; set; }

        //this property is going to be binded to view context
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContextData { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            //if active-when tag is not assigned to a value
            if (string.IsNullOrEmpty(ActiveWhen))
            {
                return;
            }

            var currentController = ViewContextData?.RouteData.Values["controller"]?.ToString();
            if (currentController!.Equals(ActiveWhen))
            {
                if (output.Attributes.ContainsName("class"))
                {
                    output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} active");
                }
                else
                {
                    output.Attributes.SetAttribute("class", "active");
                }
            }
        }
    }
}
