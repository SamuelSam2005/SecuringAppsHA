using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SecureDocumentExchange.Web.Filters
{
    public class VerifyAccessFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var email = context.HttpContext.Request.Query["email"].ToString();
            var code = context.HttpContext.Request.Query["code"].ToString();
            var file = context.HttpContext.Request.Query["file"].ToString();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(file))
            {
                context.Result = new BadRequestResult();
                return;
            }

            var env = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
            var secureFolder = Path.Combine(env.ContentRootPath, "SecureFiles");
            var metaPath = Path.Combine(secureFolder, $"{file}.meta.txt");

            if (!System.IO.File.Exists(metaPath))
            {
                context.Result = new NotFoundResult();
                return;
            }

            var metaLines = System.IO.File.ReadAllLines(metaPath);
            var metaCode = metaLines.FirstOrDefault(l => l.StartsWith("AccessCode:"))?.Split(":")[1].Trim();
            var metaEmail = metaLines.FirstOrDefault(l => l.StartsWith("LawyerEmail:"))?.Split(":")[1].Trim();

            if (metaCode != code || metaEmail?.ToLower() != email.ToLower())
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
