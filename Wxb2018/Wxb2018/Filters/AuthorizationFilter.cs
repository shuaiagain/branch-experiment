using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using Wxb2018.Controllers;

namespace Wxb2018.Filters
{

    public class AuthorizationFilter : ActionFilterAttribute
    {

        #region 登录判断
        /// <summary>
        /// 登录判断
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            base.OnActionExecuting(filterContext);

            if (!filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/account/login");
            }
        } 
        #endregion

    }
}