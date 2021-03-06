﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using UDS.Models;

namespace UDS.Controllers
{
    public class DetailController : Controller
    {
        //
        // GET: /Detail/

        public ActionResult DetailContainer()
        {
            int id = Convert.ToInt32(Request["id"]);
            int show = int.Parse(Request["show"]);
            int canSign = Convert.ToInt32(Request["canSign"]);
            int backid = Convert.ToInt32(Request["backid"]);
            DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", id));
            int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
            string pagename = dtPreMain.Rows[0]["pagename"].ToString();
            ViewBag.pageName = pagename;
            ViewBag.innerId = innerid;
            ViewBag.show = show;
            DataTable dtBaseInfo = SQLHelper.ProcDataTable("usp_BaseInfo", new SqlParameter("@id", id));
            BaseInfo baseinfo = new BaseInfo();
            ViewBag.BaseInfo = baseinfo.DbToObject(dtBaseInfo);
            DataTable dtSignInfo = SQLHelper.ProcDataTable("usp_ExamInfo", new SqlParameter("@id", id));
            ViewBag.SignInfoList = SignInfo.DBToObject(dtSignInfo);
            ViewBag.Id = id;
            ViewBag.Old = Request["isOld"];
            ViewBag.canSign = canSign;
            switch (backid)
            {
                case 1:
                    ViewBag.ActionName = "NextSignList";
                    ViewBag.ControllerName = "List";
                    ViewData["paras"] = Request["paras"];
                    break;
                case 2:
                    ViewBag.ActionName = "OwnApplyList";
                    ViewBag.ControllerName = "List";
                    ViewData["paras"] = Request["paras"];
                    break;
                case 3:
                    ViewBag.ActionName = "RecordSignList";
                    ViewBag.ControllerName = "List";
                    ViewData["paras"] = Request["paras"];
                    break;
                case 4:
                    ViewBag.ActionName = "AgentSign";
                    ViewBag.ControllerName = "Agent";
                    ViewData["paras"] = Request["paras"];
                    break;
                case 5:
                    ViewBag.ActionName = "FormQuery";
                    ViewBag.ControllerName = "Administrator";
                    ViewData["paras"] = Request["paras"];
                    break;
                case 6:
                    ViewBag.ActionName = "SubsistenceList";
                    ViewBag.ControllerName = "List";
                    ViewData["paras"] = Request["paras"];
                    break;
                default:
                    ViewBag.ActionName = "OwnApplyList";
                    ViewBag.ControllerName = "List";
                    ViewData["paras"] = Request["paras"];
                    break;
            }
            ViewBag.BackId = backid;
            return PartialView();
        }

        public ActionResult WriteContainer(int flowid)
        {
            DataTable dt = SQLHelper.ProcDataTable("usp_WriteDetail", new SqlParameter("@id", flowid));
            string pagename = dt.Rows[0]["pagename"].ToString();
            string formname = dt.Rows[0]["formname"].ToString();
            ViewBag.pageName = pagename;
            ViewBag.formname = formname;
            ViewBag.Id = flowid;
            return PartialView();
        }

        public ActionResult DraftContainer(int show, int isOld, int formflowid)
        {
            DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", formflowid));
            int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
            string pagename = dtPreMain.Rows[0]["pagename"].ToString();
            string formname = dtPreMain.Rows[0]["formname"].ToString();
            ViewBag.PageName = pagename;
            ViewBag.formname = formname;
            ViewBag.InnerId = innerid;
            ViewBag.Show = show;
            ViewBag.isOld = isOld;
            ViewBag.Id = formflowid;
            ViewData["paras"] = Request["paras"];
            return PartialView();
        }

        public ActionResult SignDeal()
        {
            int formflowid = Convert.ToInt32(Request["id"]);
            int backid = Convert.ToInt32(Request["backid"]);
            User user = Session["user"] as User;
            if (user != null)
            {
                int eid = user.Eid;
                string reason = Request["reason"];
                if (Request["agree"] != null && Request["disagree"] == null)
                {
                    DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                    string nextid = preSign.Rows[0]["nextstep"].ToString();
                    List<string> signlist = new List<string>(preSign.Rows[0]["signposlist"].ToString().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                    int index = signlist.IndexOf(nextid) + 1;
                    nextid = signlist.ElementAt(index);
                    string lastid = signlist.ElementAt(signlist.Count - 1);
                    if (nextid.Equals(lastid))
                    {
                        SQLHelper.ProcNoQuery("usp_SignSuccess", new SqlParameter("@nextstep", nextid), new SqlParameter("@id", formflowid));
                    }
                    else
                    {
                        SQLHelper.ProcNoQuery("usp_SignNext", new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                    }
                    SQLHelper.ProcNoQuery("usp_SignAgreeInfo", new SqlParameter("formflowid", formflowid), new SqlParameter("@eid", eid), new SqlParameter("time", DateTime.Now), new SqlParameter("reason", reason));
                }
                else if (Request["agree"] == null && Request["disagree"] != null)
                {
                    SQLHelper.ProcNoQuery("usp_SignFail", new SqlParameter("id", formflowid));
                    SQLHelper.ProcNoQuery("usp_SignRefuseInfo", new SqlParameter("formflowid", formflowid), new SqlParameter("@eid", eid), new SqlParameter("time", DateTime.Now), new SqlParameter("reason", reason));
                }
            }
            if (backid == 4)
            {
                return RedirectToAction("AgentSign", "Agent", new { pageindex = 1 });
            }
            else
            {
                return RedirectToAction("NextSignList", "List", new { pageindex = 1 });
            }
        }

        private static int AddForm(string formtitle, int innerid, int formid, int eid, DateTime datetime, string signlist)
        {
            SqlParameter[] parameters = { new SqlParameter("@innerid", SqlDbType.Int), new SqlParameter("@formid", SqlDbType.Int), new SqlParameter("@eid", SqlDbType.Int), new SqlParameter("@writetime", SqlDbType.DateTime), new SqlParameter("@signlist", SqlDbType.VarChar), new SqlParameter("@id", SqlDbType.Int), new SqlParameter("@formtitle", SqlDbType.NVarChar) };
            parameters[0].Value = innerid;
            parameters[1].Value = formid;
            parameters[2].Value = eid;
            parameters[3].Value = datetime;
            parameters[4].Value = signlist;
            parameters[5].Direction = ParameterDirection.Output;
            parameters[6].Value = formtitle;
            SQLHelper.ProcNoQuery("usp_WriteSave", parameters);
            return Convert.ToInt32(parameters[5].Value);
        }

        private static int UpdateFormTitle(int formflowid, string title)
        {
            return SQLHelper.ProcNoQuery("usp_WriteModify", new SqlParameter("@id", formflowid), new SqlParameter("time", DateTime.Now), new SqlParameter("@formtitle", title));
        }

        private static int UpdateFormSignList(int formflowid, string signlist, string title)
        {
            return SQLHelper.ProcNoQuery("usp_WriteModifySignList", new SqlParameter("@id", formflowid), new SqlParameter("@time", DateTime.Now), new SqlParameter("@signlist", signlist), new SqlParameter("@formtitle", title));
        }

        private static List<string> CalcSignList(int flowid, int eid)
        {
            List<string> signlist = new List<string>();
            DataTable dt = SQLHelper.ProcDataTable("usp_WriteDetail", new SqlParameter("@id", flowid));
            string flow = dt.Rows[0]["flow"].ToString();
            string[] flowlist = flow.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            dt = SQLHelper.ProcDataTable("usp_PosId", new SqlParameter("@id", eid));
            string posid = dt.Rows[0]["positionid"].ToString();
            for (int i = 0; i < flowlist.Length - 1; i++)
            {
                int temp;
                if (int.TryParse(flowlist[i], out temp))
                {
                    if (!signlist.Contains(flowlist[i]))
                    {
                        signlist.Add(flowlist[i]);
                    }
                    continue;
                }
                if (flowlist[i].Equals("U"))
                {
                    dt = SQLHelper.ProcDataTable("usp_SuperiorPosId", new SqlParameter("@id", eid));
                    signlist.Add(dt.Rows[0]["superiorposid"].ToString());
                    continue;
                }

                if (flowlist[i].Equals("T"))
                {
                    signlist.Add(posid);
                    continue;
                }
                if (flowlist[i].Equals("D"))
                {
                    dt = SQLHelper.ProcDataTable("usp_DepPosId", new SqlParameter("@id", eid));
                    string depposid = dt.Rows[0]["directorposid"].ToString();
                    if (posid.Equals("1") || posid.Equals("2") || posid.Equals(depposid))
                    {
                        continue;
                    }
                    else
                    {
                        var tempSuper = new List<string>();
                        int supereid = eid;
                        string superposid;
                        do
                        {
                            dt = SQLHelper.ProcDataTable("usp_SuperiorPosId", new SqlParameter("@id", supereid));
                            superposid = dt.Rows[0]["superiorposid"].ToString();
                            if (!superposid.Equals(depposid))
                            {
                                tempSuper.Add(superposid);
                            }
                            dt = SQLHelper.ProcDataTable("usp_SuperEid", new SqlParameter("@id", supereid));
                            supereid = Convert.ToInt32(dt.Rows[0]["employeeid"]);
                        }
                        while (!superposid.Equals(depposid));
                        foreach (string item in tempSuper)
                        {
                            if (!signlist.Contains(item))
                            {
                                signlist.Add(item);
                            }
                        }
                        if (!signlist.Contains(depposid))
                        {
                            signlist.Add(depposid);
                        }
                        continue;
                    }
                }
            }
            signlist.Add(flowlist[flowlist.Length - 1]);
            return signlist;
        }

        public ActionResult JbInfo(Dictionary<string, int> pars, JBInfo jbinfo)
        {
            //初始化下拉列表的数据信息            
            ViewData["typelist"] = JBInfo.GetTypeList();
            //开始日期的选择范围限制
            ViewBag.Before = Convert.ToInt32(FlowParameter.GetParaValueFromDb("daysbefore", 2));
            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                string isOld = Request["isOld"];
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    JBInfo.UpdateInfo(jbinfo, innerid);
                    string formtitle = jbinfo.Reason.Substring(0,
                            jbinfo.Reason.Length < 50 ? jbinfo.Reason.Length : 50);
                    UpdateFormTitle(ffid, formtitle);
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
                else if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    int innerid = JBInfo.AddInfo(jbinfo);
                    int flowid = int.Parse(Request["id"]);
                    var user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = jbinfo.Reason.Substring(0,
                            jbinfo.Reason.Length < 50 ? jbinfo.Reason.Length : 50);
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);
                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras = "pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = JBInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }

        public ActionResult QjInfo(Dictionary<string, int> pars, QJInfo qjinfo)
        {
            //初始化下拉列表的数据信息            
            ViewData["typelist"] = QJInfo.GetTypeList();
            //开始日期的选择范围限制，该表单的参数
            ViewBag.Before = Convert.ToInt32(FlowParameter.GetParaValueFromDb("daysbefore", 3));
            ViewBag.HourPreDay = Convert.ToInt32(FlowParameter.GetParaValueFromDb("hourspreday", 3));
            ViewBag.EndWorkHour = Convert.ToInt32(FlowParameter.GetParaValueFromDb("endworkhour", 3));
            ViewBag.EndWorkMin = Convert.ToInt32(FlowParameter.GetParaValueFromDb("endworkmin", 3));
            ViewBag.BeginWorkHour = Convert.ToInt32(FlowParameter.GetParaValueFromDb("beginworkhour", 3));
            ViewBag.BeginWorkMin = Convert.ToInt32(FlowParameter.GetParaValueFromDb("beginworkmin", 3));

            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                string endPosId;
                int hoursCount = Convert.ToInt32(FlowParameter.GetParaValueFromDb("hourslimit", 3));
                string isOld = Request["isOld"];
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    QJInfo.UpdateInfo(qjinfo, innerid);
                    //修改总表单中的签核步奏
                    int flowid = Convert.ToInt32(dtPreMain.Rows[0]["formid"]);
                    var user = Session["user"] as User;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        if (qjinfo.TotalTime <= hoursCount)
                        {
                            endPosId = FlowParameter.GetParaValueFromDb("endposid", 3).ToString();
                            if (signposlist.Contains(endPosId))
                            {
                                int startIndex = signposlist.IndexOf(endPosId);
                                int endIndex = signposlist.Count - 1;
                                int count = endIndex - startIndex;
                                signposlist.RemoveRange(startIndex, count);
                            }
                        }
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = qjinfo.Reason.Substring(0,
                            qjinfo.Reason.Length < 50 ? qjinfo.Reason.Length : 50);
                        UpdateFormSignList(ffid, signlist, formtitle);
                    }
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
                if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    int innerid = QJInfo.AddInfo(qjinfo);
                    int flowid = int.Parse(Request["id"]);
                    var user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        if (qjinfo.TotalTime <= hoursCount)
                        {
                            endPosId = FlowParameter.GetParaValueFromDb("endposid", 3).ToString();
                            if (signposlist.Contains(endPosId))
                            {
                                int startIndex = signposlist.IndexOf(endPosId);
                                int endIndex = signposlist.Count - 1;
                                int count = endIndex - startIndex;
                                signposlist.RemoveRange(startIndex, count);
                            }
                        }
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = qjinfo.Reason.Substring(0,
                            qjinfo.Reason.Length < 50 ? qjinfo.Reason.Length : 50);
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);
                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras = "pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = QJInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }

        public ActionResult GcInfo(Dictionary<string, int> pars, GCInfo gcinfo)
        {
            //开始日期的选择范围限制
            ViewBag.Before = Convert.ToInt32(FlowParameter.GetParaValueFromDb("daysbefore", 4));
            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                string isOld = Request["isOld"];
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    GCInfo.UpdateInfo(gcinfo, innerid);
                    string formtitle = gcinfo.Reason.Substring(0,
                            gcinfo.Reason.Length < 50 ? gcinfo.Reason.Length : 50);
                    UpdateFormTitle(ffid, formtitle);
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
                if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    int innerid = GCInfo.AddInfo(gcinfo);
                    int flowid = int.Parse(Request["id"]);
                    var user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = gcinfo.Reason.Substring(0,
                            gcinfo.Reason.Length < 50 ? gcinfo.Reason.Length : 50);
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);
                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras = "pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = GCInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }

        [ValidateInput(false)]
        public ActionResult FybxInfo(Dictionary<string, int> pars, FYBXInfo fybxinfo)
        {
            //初始化暂支信息
            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                string isOld = Request["isOld"];
                //该表单的参数，根据金额大小调整签核步骤
                decimal moneyLimit = Convert.ToInt32(FlowParameter.GetParaValueFromDb("moneylimit", 5));
                string endPosId;
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    FYBXInfo.UpdateInfo(fybxinfo, innerid);
                    //修改总表单中的签核步奏
                    int flowid = Convert.ToInt32(dtPreMain.Rows[0]["formid"]);
                    var user = Session["user"] as User;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        if (fybxinfo.Money <= moneyLimit)
                        {
                            endPosId = FlowParameter.GetParaValueFromDb("endposid", 5).ToString();
                            if (signposlist.Contains(endPosId))
                            {
                                int startIndex = signposlist.IndexOf(endPosId);
                                int endIndex = signposlist.Count - 1;
                                int count = endIndex - startIndex;
                                signposlist.RemoveRange(startIndex, count);
                            }
                        }
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = fybxinfo.Usage.Substring(0, fybxinfo.Usage.Length < 50 ? fybxinfo.Usage.Length : 50);
                        UpdateFormSignList(ffid, signlist, formtitle);
                    }
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
                if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    fybxinfo.AttachContent = fybxinfo.AttachContent ?? string.Empty;
                    int innerid = FYBXInfo.AddInfo(fybxinfo);
                    int flowid = int.Parse(Request["id"]);
                    var user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        if (fybxinfo.Money <= moneyLimit)
                        {
                            endPosId = FlowParameter.GetParaValueFromDb("endposid", 5).ToString();
                            if (signposlist.Contains(endPosId))
                            {
                                int startIndex = signposlist.IndexOf(endPosId);
                                int endIndex = signposlist.Count - 1;
                                int count = endIndex - startIndex;
                                signposlist.RemoveRange(startIndex, count);
                            }
                        }
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = fybxinfo.Usage.Substring(0, fybxinfo.Usage.Length < 50 ? fybxinfo.Usage.Length : 50);
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);
                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras = "pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = FYBXInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }

        [ValidateInput(false)]
        public ActionResult SyqmkhInfo(Dictionary<string, int> pars, SYQMKHInfo syqmkhinfo)
        {
            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                string isOld = Request["isOld"];
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    SYQMKHInfo.UpdateInfo(syqmkhinfo, innerid);
                    string formtitle = string.Format("试用期满考核{0}", DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                    UpdateFormTitle(ffid, formtitle);
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
                else if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    int innerid = SYQMKHInfo.AddInfo(syqmkhinfo);
                    int flowid = int.Parse(Request["id"]);
                    var user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = string.Format("试用期满考核{0}", DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);
                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras = "pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = SYQMKHInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }

        [ValidateInput(false)]
        public ActionResult HYJLInfo(Dictionary<string, int> pars, HYJLInfo hyjlinfo)
        {
            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                string isOld = Request["isOld"];
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    Models.HYJLInfo.UpdateInfo(hyjlinfo, innerid);
                    string formtitle = hyjlinfo.Topic.Substring(0,
                            hyjlinfo.Topic.Length < 50 ? hyjlinfo.Topic.Length : 50);
                    UpdateFormTitle(ffid, formtitle);
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
                else if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    hyjlinfo.AttachContent = hyjlinfo.AttachContent ?? string.Empty;
                    int innerid = Models.HYJLInfo.AddInfo(hyjlinfo);
                    int flowid = int.Parse(Request["id"]);
                    var user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = hyjlinfo.Topic.Substring(0, hyjlinfo.Topic.Length < 50 ? hyjlinfo.Topic.Length : 50);
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);
                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras = "pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = Models.HYJLInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }

        [ValidateInput(false)]
        public ActionResult CommonModelInfo(Dictionary<string, int> pars, CommonModelInfo commonmodelinfo)
        {
            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                string isOld = Request["isOld"];
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    Models.CommonModelInfo.UpdateInfo(commonmodelinfo, innerid);
                    string formtitle = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
                    UpdateFormTitle(ffid, formtitle);
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
                else if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    int innerid = Models.CommonModelInfo.AddInfo(commonmodelinfo);
                    int flowid = int.Parse(Request["id"]);
                    var user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);
                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras = "pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = Models.CommonModelInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }

        [ValidateInput(false)]
        public ActionResult QTJLInfo(Dictionary<string, int> pars, QTJLInfo qtjlinfo)
        {
            //初始化下拉列表的数据信息
            ViewData["typelist"] = Models.QTJLInfo.GetTypeList();
            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                string isOld = Request["isOld"];
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    Models.QTJLInfo.UpdateInfo(qtjlinfo, innerid);
                    string formtitle = string.Format("洽谈记录{0}", DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                    UpdateFormTitle(ffid, formtitle);
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid });
                }
                else if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    int innerid = Models.QTJLInfo.AddInfo(qtjlinfo);
                    int flowid = int.Parse(Request["id"]);
                    var user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = string.Format("洽谈记录{0}", DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);
                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras = "pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = Models.QTJLInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }

        [ValidateInput(false)]
        public ActionResult XZJXInfo(Dictionary<string, int> pars, XZJXInfo xzjxinfo)
        {
            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                string isOld = Request["isOld"];
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    Models.XZJXInfo.UpdateInfo(xzjxinfo, innerid);
                    string formtitle = string.Format("薪资绩效{0}", DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                    UpdateFormTitle(ffid, formtitle);
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
                else if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    int innerid = Models.XZJXInfo.AddInfo(xzjxinfo);
                    int flowid = int.Parse(Request["id"]);
                    var user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = string.Format("薪资绩效{0}", DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);
                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras = "pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = Models.XZJXInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }

        [ValidateInput(false)]
        public ActionResult PXInfo(Dictionary<string, int> pars, PXInfo pxinfo)
        {
            //初始化下拉列表的数据信息            
            ViewData["typelist"] = Models.PXInfo.GetTypeList();
            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                string isOld = Request["isOld"];
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    Models.PXInfo.UpdateInfo(pxinfo, innerid);
                    string formtitle = pxinfo.Title.Substring(0, pxinfo.Title.Length < 50 ? pxinfo.Title.Length : 50);
                    UpdateFormTitle(ffid, formtitle);
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
                else if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    int innerid = Models.PXInfo.AddInfo(pxinfo);
                    int flowid = int.Parse(Request["id"]);
                    var user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = pxinfo.Title.Substring(0, pxinfo.Title.Length < 50 ? pxinfo.Title.Length : 50);
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);
                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras = "pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = Models.PXInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }

        public ActionResult GDZCInfo(Dictionary<string, int> pars, GDZCInfo gdzcinfo)
        {
            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                decimal moneyLimit = Convert.ToInt32(FlowParameter.GetParaValueFromDb("moneylimit", 11));
                string endPosId;
                string isOld = Request["isOld"];
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    Models.GDZCInfo.UpdateInfo(gdzcinfo, innerid);
                    //修改总表单中的签核步奏
                    int flowid = Convert.ToInt32(dtPreMain.Rows[0]["formid"]);
                    var user = Session["user"] as User;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        if (gdzcinfo.TotalPrice <= moneyLimit)
                        {
                            endPosId = FlowParameter.GetParaValueFromDb("endposid", 11).ToString();
                            if (signposlist.Contains(endPosId))
                            {
                                int startIndex = signposlist.IndexOf(endPosId);
                                int endIndex = signposlist.Count - 1;
                                int count = endIndex - startIndex;
                                signposlist.RemoveRange(startIndex, count);
                            }
                        }
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = gdzcinfo.DeviceName.Substring(0, gdzcinfo.DeviceName.Length < 50 ? gdzcinfo.DeviceName.Length : 50);
                        UpdateFormSignList(ffid, signlist, formtitle);
                    }
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
                if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    int innerid = Models.GDZCInfo.AddInfo(gdzcinfo);
                    int flowid = int.Parse(Request["id"]);
                    var user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        if (gdzcinfo.TotalPrice <= moneyLimit)
                        {
                            endPosId = FlowParameter.GetParaValueFromDb("endposid", 11).ToString();
                            if (signposlist.Contains(endPosId))
                            {
                                int startIndex = signposlist.IndexOf(endPosId);
                                int endIndex = signposlist.Count - 1;
                                int count = endIndex - startIndex;
                                signposlist.RemoveRange(startIndex, count);
                            }
                        }
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = gdzcinfo.DeviceName.Substring(0, gdzcinfo.DeviceName.Length < 50 ? gdzcinfo.DeviceName.Length : 50);
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);
                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras = "pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = Models.GDZCInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }

        public ActionResult ZZInfo(Dictionary<string, int> pars, ZZInfo zzinfo)
        {
            if (pars.ContainsKey("isNew"))
            {//新建表单时的空页面显示
                ViewBag.Display = 1;
                ViewBag.Old = 0;
                ViewBag.Id = pars["id"];
            }
            else if (Request["save"] != null)
            {
                string isOld = Request["isOld"];
                decimal moneyLimit = Convert.ToInt32(FlowParameter.GetParaValueFromDb("moneylimit", 12));
                string endPosId;
                if (isOld.Equals("1"))
                {//保存修改的表单的处理逻辑
                    int ffid = Convert.ToInt32(Request["id"]);
                    DataTable dtPreMain = SQLHelper.ProcDataTable("usp_PreMainInfo", new SqlParameter("@id", ffid));
                    int innerid = Convert.ToInt32(dtPreMain.Rows[0]["forminnerid"]);
                    Models.ZZInfo.UpdateInfo(zzinfo, innerid);
                    //修改总表单中的签核步奏
                    int flowid = Convert.ToInt32(dtPreMain.Rows[0]["formid"]);
                    var user = Session["user"] as User;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        if (zzinfo.Money <= moneyLimit)
                        {
                            endPosId = FlowParameter.GetParaValueFromDb("endposid", 12).ToString();
                            if (signposlist.Contains(endPosId))
                            {
                                int startIndex = signposlist.IndexOf(endPosId);
                                int endIndex = signposlist.Count - 1;
                                int count = endIndex - startIndex;
                                signposlist.RemoveRange(startIndex, count);
                            }
                        }
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = zzinfo.Reason.Substring(0, zzinfo.Reason.Length < 50 ? zzinfo.Reason.Length : 50);
                        UpdateFormSignList(ffid, signlist, formtitle);
                    }
                    ViewBag.Old = 1;
                    ViewBag.Id = ffid;
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras="pageindex=1" });
                }
                if (isOld.Equals("0"))
                {//新建表单时的保存处理逻辑
                    int innerid = Models.ZZInfo.AddInfo(zzinfo);
                    int flowid = int.Parse(Request["id"]);
                    User user = Session["user"] as User;
                    int ffid = 0;
                    if (user != null)
                    {
                        int eid = user.Eid;
                        List<string> signposlist = CalcSignList(flowid, eid);
                        signposlist.RemoveAt(0);
                        if (zzinfo.Money <= moneyLimit)
                        {
                            endPosId = FlowParameter.GetParaValueFromDb("endposid", 12).ToString();
                            if (signposlist.Contains(endPosId))
                            {
                                int startIndex = signposlist.IndexOf(endPosId);
                                int endIndex = signposlist.Count - 1;
                                int count = endIndex - startIndex;
                                signposlist.RemoveRange(startIndex, count);
                            }
                        }
                        string signlist = string.Join("|", signposlist.ToArray());
                        string formtitle = zzinfo.Reason.Substring(0, zzinfo.Reason.Length < 50 ? zzinfo.Reason.Length : 50);
                        ffid = AddForm(formtitle, innerid, flowid, eid, DateTime.Now, signlist);

                        ViewBag.Old = 1;
                        ViewBag.Id = ffid;
                    }
                    ViewBag.Display = 1;
                    return RedirectToAction("DraftContainer", "Detail", new { show = 1, isOld = 1, formflowid = ffid, paras = "pageindex=1" });
                }
            }
            else if (Request["send"] != null)
            {//发送表单的处理逻辑
                int formflowid = Convert.ToInt32(Request["id"]);
                DataTable preSign = SQLHelper.ProcDataTable("usp_PreSign", new SqlParameter("@id", formflowid));
                string signlist = preSign.Rows[0]["signposlist"].ToString();
                int nextid = int.Parse(signlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                SQLHelper.ProcNoQuery("usp_Send", new SqlParameter("@sendtime", DateTime.Now), new SqlParameter("@nextid", nextid), new SqlParameter("@id", formflowid));
                ViewBag.Display = 1;
                return RedirectToAction("OwnApplyList", "List", new { pageindex = 1, paras="pageindex=1" });
            }
            else if (Request["save"] == null && Request["send"] == null)
            {//用于显示表单详细信息的处理逻辑
                int innerid = pars["innerid"];
                int show = pars["show"];
                ViewData.Model = Models.ZZInfo.GetInfoById(innerid);
                ViewBag.Display = show;
                return PartialView();
            }
            return PartialView();
        }



    }
}
