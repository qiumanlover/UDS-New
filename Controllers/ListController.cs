﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using UDS.Models;

namespace UDS.Controllers
{
    [LoginActionFilter]
    public class ListController : Controller
    {
        //
        // GET: /List/

        public ActionResult NextSignList()
        {
            var user = Session["user"] as User;
            if (user != null)
            {
                int eid = user.Eid;
                int pagecount;
                int pageindex = int.Parse(Request["pageindex"]);
                List<InfoList> data = GetInfoListFromDb("usp_NextSign", eid, pageindex, 20, out pagecount);
                ViewData.Model = data;
                ViewBag.pageCount = pagecount;
                ViewBag.pageIndex = pagecount == 0 ? 0 : pageindex;
                ViewBag.frtIndex = pagecount == 0 ? 0 : 1;
                ViewBag.preIndex = pagecount == 0 ? 0 : (pageindex == 1 ? 1 : pageindex - 1);
                ViewBag.nxtIndex = pagecount == 0 ? 0 : (pageindex == pagecount ? pageindex : pageindex + 1);
                ViewData["paras"] = string.Format("pageindex={0}", pageindex);
            }
            return PartialView();
        }

        public ActionResult RecordSignList()
        {
            var user = Session["user"] as User;
            if (user != null)
            {
                int eid = user.Eid;
                int pagecount;
                int pageindex = int.Parse(Request["pageindex"]);
                List<InfoList> data = GetInfoListFromDb("usp_RecordSign", eid, pageindex, 20, out pagecount);
                ViewData.Model = data;
                ViewBag.pageCount = pagecount;
                ViewBag.pageIndex = pagecount == 0 ? 0 : pageindex;
                ViewBag.frtIndex = pagecount == 0 ? 0 : 1;
                ViewBag.preIndex = pagecount == 0 ? 0 : (pageindex == 1 ? 1 : pageindex - 1);
                ViewBag.nxtIndex = pagecount == 0 ? 0 : (pageindex == pagecount ? pageindex : pageindex + 1);
                ViewData["paras"] = string.Format("pageindex={0}", pageindex);
            }
            return PartialView();
        }

        public ActionResult SubsistenceList()
        {
            var user = Session["user"] as User;
            if (user != null)
            {
                int eid = user.Eid;
                int pagecount;
                int pageindex = int.Parse(Request["pageindex"]);
                List<InfoList> data = GetInfoListFromDb("usp_Subsistence", eid, pageindex, 20, out pagecount);
                ViewData.Model = data;
                ViewBag.pageCount = pagecount;
                ViewBag.pageIndex = pagecount == 0 ? 0 : pageindex;
                ViewBag.frtIndex = pagecount == 0 ? 0 : 1;
                ViewBag.preIndex = pagecount == 0 ? 0 : (pageindex == 1 ? 1 : pageindex - 1);
                ViewBag.nxtIndex = pagecount == 0 ? 0 : (pageindex == pagecount ? pageindex : pageindex + 1);
                ViewData["paras"] = string.Format("pageindex={0}", pageindex);
            }
            return PartialView();
        }

        public ActionResult OwnApplyList()
        {
            var user = Session["user"] as User;
            if (user != null)
            {
                int eid = user.Eid;
                int pagecount;
                int pageindex = int.Parse(Request["pageindex"]);
                List<InfoList> data = GetInfoListFromDb("usp_OwnApply", eid, pageindex, 20, out pagecount);
                ViewData.Model = data;
                ViewBag.pageCount = pagecount;
                ViewBag.pageIndex = pagecount == 0 ? 0 : pageindex;
                ViewBag.frtIndex = pagecount == 0 ? 0 : 1;
                ViewBag.preIndex = pagecount == 0 ? 0 : (pageindex == 1 ? 1 : pageindex - 1);
                ViewBag.nxtIndex = pagecount == 0 ? 0 : (pageindex == pagecount ? pageindex : pageindex + 1);
                ViewData["paras"] = string.Format("pageindex={0}", pageindex);
            }
            return PartialView();
        }

        public ActionResult OwnDraftList()
        {
            var user = Session["user"] as User;
            if (user != null)
            {
                int eid = user.Eid;
                int pagecount;
                int pageindex = int.Parse(Request["pageindex"]);
                List<InfoList> data = GetInfoListFromDb("usp_OwnDraft", eid, pageindex, 20, out pagecount);
                ViewData.Model = data;
                ViewBag.pageCount = pagecount;
                ViewBag.pageIndex = pagecount == 0 ? 0 : pageindex;
                ViewBag.frtIndex = pagecount == 0 ? 0 : 1;
                ViewBag.preIndex = pagecount == 0 ? 0 : (pageindex == 1 ? 1 : pageindex - 1);
                ViewBag.nxtIndex = pagecount == 0 ? 0 : (pageindex == pagecount ? pageindex : pageindex + 1);
                ViewData["paras"] = string.Format("pageindex={0}", pageindex);
            }
            return PartialView();
        }

        public ActionResult WriteList()
        {
            DataTable dt = SQLHelper.ProcDataTable("usp_WriteList", null);
            Dictionary<int, string> flowList = new Dictionary<int, string>();
            foreach (DataRow row in dt.Rows)
            {
                flowList.Add(Convert.ToInt32(row["id"]), row["formname"].ToString());
            }
            ViewBag.flowList = flowList;
            return PartialView();
        }

        private static List<InfoList> GetInfoListFromDb(string procname, int eid, int pageindex, int pagesize, out int pagecount)
        {
            SqlParameter[] parameters = { new SqlParameter("@pageIndex", SqlDbType.Int), new SqlParameter("@pageSize", SqlDbType.Int), new SqlParameter("@pageCount", SqlDbType.Int), new SqlParameter("@eid", SqlDbType.Int) };
            parameters[0].Value = pageindex;
            parameters[1].Value = pagesize;
            parameters[2].Direction = ParameterDirection.Output;
            parameters[3].Value = eid;
            DataTable dt = SQLHelper.ProcDataTable(procname, parameters);
            pagecount = Convert.ToInt32(parameters[2].Value);
            List<InfoList> data = new List<InfoList>();
            DataColumnCollection columns = dt.Columns;
            foreach (DataRow row in dt.Rows)
            {
                InfoList info = new InfoList();
                info = info.DBDataToInfo(row, columns);
                data.Add(info);
            }
            return data;
        }
    }
}
