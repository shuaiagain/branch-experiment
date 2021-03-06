﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

using WXB.Bussiness.ViewModels;
using WXB.Bussiness.Service;
using WXB.Bussiness.Models;
using WXB.Bussiness.Common;
using System.IO;
using System.Text;
using Wxb2018.Filters;

namespace Wxb2018.Controllers
{
    public class HomeController : BaseController
    {

        #region Index
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        [AuthorizationFilter]
        public ActionResult Index()
        {
            PageVM<NoticeVM> data = new NoticeService().GetNoticeListPage(new NoticeQuery() { PageSize = 6 });
            return View(data);
        }
        #endregion

        #region 获取通知列表(分部视图分页)
        /// <summary>
        /// GetListData
        /// </summary>
        /// <returns></returns>
        public ActionResult GetListDataPage(NoticeQuery query)
        {
            if (query == null) query = new NoticeQuery();

            if (!query.PageIndex.HasValue)
                query.PageIndex = 1;
            if (!query.PageSize.HasValue)
                query.PageSize = 6;

            PageVM<NoticeVM> data = new NoticeService().GetNoticeListPage(query);
            return View("HomeTablePartial", data);
        }
        #endregion

        #region 获取通知列表(分部视图分页)
        /// <summary>
        /// GetListData
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNoticePageData(NoticeQuery query)
        {
            if (query == null) query = new NoticeQuery();

            if (!query.PageIndex.HasValue)
                query.PageIndex = 1;
            if (!query.PageSize.HasValue)
                query.PageSize = 6;

            PageVM<NoticeVM> list = new NoticeService().GetNoticeListPage(query);
            if (list == null || list.Data == null || list.Data.Count == 0)
            {
                return Json(new
                {
                    Code = -200,
                    Msg = "暂无数据"
                });
            }

            return Json(new
            {
                Code = 200,
                Msg = "获取成功",
                Data = list
            });
        }
        #endregion

        #region LoginHeadPartial
        /// <summary>
        /// LoginHeadPartial
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LoginHeadPartial()
        {
            return View();
        }
        #endregion

        #region AccountPartial
        [HttpGet]
        public ActionResult AccountPartial()
        {
            return View();
        }
        #endregion

        #region 添加通知
        /// <summary>
        /// 添加通知
        /// </summary>
        /// <returns></returns>
        [AuthorizationFilter]
        public ActionResult Add()
        {
            return View();
        }
        #endregion

        #region 编辑
        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="noticeId"></param>
        /// <returns></returns>
        [AuthorizationFilter]
        public ActionResult Edit(int noticeId)
        {
            NoticeVM notice = new NoticeService().GetNoticeById(noticeId);

            return View("add", notice);
        }
        #endregion

        #region 保存通知

        /// <summary>
        /// 保存通知
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveNotice(NoticeVM vm)
        {
            ResultEntity<NoticeVM> result = new ResultEntity<NoticeVM>();
            if (vm == null || string.IsNullOrEmpty(vm.Title) || string.IsNullOrEmpty(vm.Content))
            {
                result.Code = -400;
                result.Msg = "通知标题和通知内容不能为空";
                return Json(result);
            }

            try
            {
                if (!vm.ID.HasValue)
                {
                    vm.Inputer = UserData.Name;
                    vm.InputerID = UserData.UserId;
                    vm.InputTime = DateTime.Now;
                }

                vm.UpdateTime = DateTime.Now;

                #region 日志

                var log = new LogVM()
                        {
                            Operator = this.UserData.Name,
                            OperatorID = this.UserData.UserId,
                            RoleType = this.UserData.RoleType,
                            OperateTime = DateTime.Now,
                            OperateType = vm.ID.HasValue ? (int)EnumOperateType.编辑 : (int)EnumOperateType.添加
                        };

                log.OperateDescribe = ((EnumOperateType)log.OperateType).ToString() + "通知：Title = " + vm.Title;
                Logger.AddLog(log);

                #endregion

                vm = new NoticeService().Save(vm);
                result.Data = vm;

            }
            catch (Exception ex)
            {
                throw ex;
            }

            result.Msg = "添加成功";
            result.Code = 200;

            return Json(result);
        }

        #endregion

        #region 获取通知列表
        /// <summary>
        /// 获取通知列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetNoticeList(NoticeQuery query)
        {

            if (query == null) query = new NoticeQuery();

            List<NoticeVM> list = new NoticeService().GetNoticeList(query);
            ResultEntity<List<NoticeVM>> result = new ResultEntity<List<NoticeVM>>();

            if (list == null || list.Count == 0)
            {
                result.Code = -200;
                result.Msg = "暂无数据";

                return Json(result, JsonRequestBehavior.AllowGet);
            }

            result.Data = list;
            result.Msg = "获取成功";
            result.Code = 200;

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除通知
        /// <summary>
        /// 删除通知
        /// </summary>
        /// <param name="noticeId"></param>
        /// <returns></returns>
        public ActionResult Delete(int noticeId)
        {
            bool result = new NoticeService().Delete(noticeId);

            if (!result) return Json(new { Code = -200, Msg = "删除失败" });

            #region 日志

            var log = new LogVM()
                {
                    Operator = this.UserData.Name,
                    OperatorID = this.UserData.UserId,
                    RoleType = this.UserData.RoleType,
                    OperateTime = DateTime.Now,
                    OperateType = (int)EnumOperateType.删除
                };

            log.OperateDescribe = ((EnumOperateType)log.OperateType).ToString() + "通知：ID = " + noticeId;
            Logger.AddLog(log);

            #endregion

            return Json(new { Code = 200, Msg = "删除成功" });
        }
        #endregion

        #region 获取用户列表
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserList()
        {
            List<UserItemVM> userList = new UserService().GetUserList();
            if (userList == null) return Json(new { Code = -200, Msg = "暂无数据" });

            return Json(new { Code = 200, Msg = "获取成功", Data = userList }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 上传文件
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            string filePathName = string.Empty;

            string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Resource/UploadFiles");
            localPath = Path.Combine(Server.MapPath("~/Resource/UploadFiles"));
            if (Request.Files.Count == 0)
            {
                return Json(new { Code = -404, Msg = "没有上传文件" });
            }

            string ex = Path.GetExtension(file.FileName);
            filePathName = Guid.NewGuid().ToString("N") + ex;
            if (!System.IO.Directory.Exists(localPath))
            {
                System.IO.Directory.CreateDirectory(localPath);
            }

            file.SaveAs(Path.Combine(localPath, filePathName));

            return Json(new
            {
                Code = 200,
                Msg = "保存成功",
                Data = new
                {
                    FilePath = Path.Combine(localPath, filePathName),
                    ViewUrl = "~/Resource/UploadFiles/" + filePathName,
                    FilePathName = filePathName
                }
            });
        }
        #endregion

        #region 下载文件
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ActionResult Download(string filePath, string fileName)
        {
            Encoding encoding;
            string outputFileName = null;
            fileName = fileName.Replace("'", "");

            //string browser = Request.UserAgent.ToUpper();
            //if (browser.Contains("MS") == true && browser.Contains("IE") == true)
            //{
            //    outputFileName = HttpUtility.UrlEncode(fileName);
            //    encoding = Encoding.Default;
            //}
            //else if (browser.Contains("FIREFOX") == true)
            //{
            //    outputFileName = fileName;
            //    encoding = Encoding.GetEncoding("GB2312");
            //}
            //else
            //{
            outputFileName = HttpUtility.UrlEncode(fileName, Encoding.UTF8);
            encoding = Encoding.UTF8;
            //}

            FileStream fs = new FileStream(Server.MapPath(filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] bytes = new byte[(int)fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            Response.Charset = "UTF-8";
            Response.ContentType = "application/octet-stream";
            Response.ContentEncoding = encoding;
            Response.AddHeader("Content-Disposition", "attachment; filename=" + outputFileName);
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
            return new EmptyResult();
        }
        #endregion

    }
}