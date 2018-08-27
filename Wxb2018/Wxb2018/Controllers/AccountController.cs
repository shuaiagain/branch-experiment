using System;

using System.Web.Mvc;
using WXB.Bussiness.Service;
using WXB.Bussiness.Common;
using WXB.Bussiness.Models;
using WXB.Bussiness.ViewModels;

namespace Wxb2018.Controllers
{
    public class AccountController : BaseController
    {
        #region Login
        [HttpGet]
        public ActionResult Login()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index","Home");
            }

            return View();
        }
        #endregion

        #region 登录

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="isRememberPwd">是否记住密码</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(string userName, string password, bool isRememberPwd)
        {

            ResultEntity<UserVM> result = new ResultEntity<UserVM>();
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {

                result.Msg = "用户名或密码不能为空";
                result.Code = (int)EnumLoginCode.用户名或密码为空;

                return Json(result);
            }

            UserVM user = new UserService().Login(userName, password);
            if (user == null)
            {

                result.Msg = "用户名或密码错误";
                result.Code = (int)EnumLoginCode.用户不存在;

                return Json(result);
            }

            this.setAuthCookie(userName, user, isRememberPwd);

            result.Code = (int)EnumLoginCode.登录成功;
            result.Msg = "登录成功";
            result.Data = user;

            #region 日志

            var log = new LogVM()
            {
                Operator = user.Name,
                OperatorID = user.ID,
                RoleType = user.RoleType,
                OperateTime = DateTime.Now,
                OperateType = (int)EnumOperateType.登录
            };

            log.OperateDescribe = ((EnumOperateType)log.OperateType).ToString();
            Logger.AddLog(log);

            #endregion

            return Json(result);
        }

        #endregion

        #region setAuthCookie
        /// <summary>
        /// setAuthCookie
        /// </summary>
        /// <param name="name"></param>
        /// <param name="user"></param>
        /// <param name="isPersistent"></param>
        private void setAuthCookie(string name, UserVM user, bool isPersistent)
        {

            WFFormsAuthentication userData = new WFFormsAuthentication()
            {
                UserId = user.ID,
                Name = string.IsNullOrEmpty(user.Name) ? user.Name : user.Alias,
                TrueName = string.IsNullOrEmpty(user.Name) ? user.Name : user.Alias
            };

            if (user.RoleType.HasValue) userData.RoleType = user.RoleType;

            if (user.Flag.HasValue) userData.Flag = user.Flag;

            //把数据保存到cookie中
            WFFormsAuthentication.SetAuthCookie(name, userData, isPersistent);
        }
        #endregion

        #region 退出

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="redirectUrl"></param>
        /// <returns></returns>
        //[Authorize]
        public ActionResult Logout(string redirectUrl)
        {
            #region 日志

            var log = new LogVM()
            {
                Operator = this.UserData.Name,
                OperatorID = this.UserData.UserId,
                RoleType = this.UserData.RoleType,
                OperateTime = DateTime.Now,
                OperateType = (int)EnumOperateType.退出
            };

            log.OperateDescribe = ((EnumOperateType)log.OperateType).ToString();
            Logger.AddLog(log);

            #endregion

            WFFormsAuthentication.SignOut();

            return Redirect(string.IsNullOrEmpty(redirectUrl) ? Url.Action("index", "home") : redirectUrl);
        }

        #endregion
    }
}