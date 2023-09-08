using Microsoft.AspNetCore.Razor.TagHelpers;

namespace PumpJam.Extensions.TagHelpers
{
    [HtmlTargetElement("*", Attributes = "for-controller")]
    public class CurrentRouteTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentRouteTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HtmlAttributeName("for-controller")]
        public string ControllerName { get; set; }
        [HtmlAttributeName("for-action")]
        public string ActionName { get; set; }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var routeValues = httpContext.GetRouteData().Values;

                if (routeValues.ContainsKey("controller"))
                    if (ControllerName != default && ControllerName != (string)routeValues["controller"])
                        output.SuppressOutput();
                
                if (routeValues.ContainsKey("controller"))
                    if (ActionName != default && ActionName != (string)routeValues["action"])
                        output.SuppressOutput();

            }
        }
    }
}
