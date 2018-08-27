using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace Wxb2018.Filters
{
    public class BaseFilter : ActionFilterAttribute
    {
        protected WFFormsAuthentication UserData { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/account/login");
            }

            //1.登录状态获取用户信息（自定义保存的用户）
            var cookie = filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];

            //2.使用 FormsAuthentication 解密用户凭据
            var ticket = FormsAuthentication.Decrypt(cookie.Value);

            //3. 直接解析到用户模型里去
            this.UserData = new JavaScriptSerializer().Deserialize<WFFormsAuthentication>(ticket.UserData);
        }

    }
}