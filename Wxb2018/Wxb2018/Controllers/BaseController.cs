using System.Web.Mvc;

namespace Wxb2018.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!Request.IsAuthenticated || this.UserData == null)
            {
                object[] actionFilter = filterContext.ActionDescriptor.GetCustomAttributes(typeof(AuthorizeAttribute), false);
                object[] controllerFilter = filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(AuthorizeAttribute), false);

                if (controllerFilter.Length > 0 || actionFilter.Length > 0)
                {
                    if (Devx.WebHelper.IsAjaxRequest())
                    {
                        filterContext.Result = Json(new
                        {
                            error = true,
                            message = "请登录！",
                            login = true
                        }, "text/plain", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        filterContext.Result = RedirectToAction("login", "account");
                    }
                }
            }

            base.OnAuthorization(filterContext);
        }

        protected WFFormsAuthentication UserData
        {
            get
            {
                if (Request.IsAuthenticated && this.User != null)
                {
                    var principal = this.User as WFFormPrincipal;
                    //if (principal != null && principal.UserData != null && !string.IsNullOrWhiteSpace(principal.UserData.SessionId))
                    if (principal != null && principal.UserData != null)
                    {
                        return principal.UserData;
                    }
                }

                return null;
            }
        }

    }
}