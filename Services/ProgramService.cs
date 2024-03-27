using FaceApp.Data;
using FaceApp.Models;
using FaceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FaceApp.Services
{
    public class ProgramService : IDisposable
    {
        private readonly FaceAppDBContext _context;
        Authentication authenticate = new Authentication();
        DbAccess access = null;
        CommonService commonService = null;
        public ProgramService(FaceAppDBContext context)
        {

            commonService = new CommonService(context);
            access = new DbAccess();
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public int CreateProgram(ProgramViewModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Programs programs = new Programs();
                    programs.ProgramName = model.Name.Trim();
                    programs.IsActive = model.IsActive;
                    programs.ProgramDescription = model.Description != null ? model.Description.Trim() : model.Description;
                    programs.ProgramId1 = model.ProgramID1 != null ? model.ProgramID1.Trim() : model.ProgramID1;
                    programs.ProgramId2 = model.ProgramID2 != null ? model.ProgramID2.Trim() : model.ProgramID2;
                    programs.ProgramId3 = model.ProgramID3 != null ? model.ProgramID3.Trim() : model.ProgramID3;
                    programs.GLAccountCode = model.GLAccountCode != null ? model.GLAccountCode.Trim() : model.GLAccountCode;
                    programs.CreatedDate = DateTime.Now;
                    programs.CreatedBy = model.LoginUserId.Value;
                    _context.Programs.Add(programs);
                    _context.SaveChanges();
                    model.ProgramId = programs.ProgramId;
                    if (model.ProgramId > 0)
                    {
                        if (model.ProgramCalendarList != null && model.ProgramCalendarList.Count > 0)
                        {
                            SaveProgramCalendar(model);
                        }
                        else
                        {
                            RemoveProgramCalendar(model.ProgramId);
                        }
                    }

                    transaction.Commit();

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }
            return model.ProgramId;
        }

        public int UpdateProgram(ProgramViewModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    Programs programs = _context.Programs.Where(u => u.ProgramId == model.ProgramId).FirstOrDefault();
                    programs.ProgramName = model.Name.Trim();
                    programs.IsActive = model.IsActive;
                    programs.ProgramDescription = model.Description != null ? model.Description.Trim() : model.Description;
                    programs.ProgramId1 = model.ProgramID1 != null ? model.ProgramID1.Trim() : model.ProgramID1;
                    programs.ProgramId2 = model.ProgramID2 != null ? model.ProgramID2.Trim() : model.ProgramID2;
                    programs.ProgramId3 = model.ProgramID3 != null ? model.ProgramID3.Trim() : model.ProgramID3;
                    programs.GLAccountCode = model.GLAccountCode != null ? model.GLAccountCode.Trim() : model.GLAccountCode;
                    programs.ModifiedDate = DateTime.Now;
                    programs.ModifiedBy = model.LoginUserId;
                    _context.Entry(programs).State = EntityState.Modified;
                    _context.SaveChanges();

                    if (model.ProgramId > 0)
                    {
                        if (model.ProgramCalendarList != null && model.ProgramCalendarList.Count > 0)
                        {
                            SaveProgramCalendar(model);
                        }
                        else
                        {
                            RemoveProgramCalendar(model.ProgramId);
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.Message);
                    transaction.Rollback();
                }
            }
            return model.ProgramId;
        }

        private bool SaveProgramCalendar(ProgramViewModel model)
        {
            try
            {
                bool retVal = true;
                if (model.ProgramCalendarList != null && model.ProgramCalendarList.Count > 0)
                {
                    ProgramCalendarDetails programCalendarDetails = new ProgramCalendarDetails();
                    List<ProgramCalendarDetails> ListprogramCalendarDetails = new List<ProgramCalendarDetails>();
                    bool IsinsertUpdate = false;
                    int StaffId = 0;
                    List<int> ids = new List<int>();
                    foreach (ProgramCalendarListViewModel CalendarViewModel in model.ProgramCalendarList)
                    {
                        StaffId = CalendarViewModel.CalendarId;
                        IsinsertUpdate = false;
                        programCalendarDetails = new ProgramCalendarDetails();
                        if (CalendarViewModel.ProgramCalendarDetailsId > 0)
                        {
                            programCalendarDetails = _context.ProgramCalendarDetails.FirstOrDefault(x => x.CalendarId == CalendarViewModel.CalendarId && x.ProgramId == model.ProgramId);
                            if (programCalendarDetails != null)
                            {
                                programCalendarDetails.ModifiedBy = model.LoginUserId.Value;
                                programCalendarDetails.ModifiedDate = System.DateTime.Now;
                                IsinsertUpdate = true;
                            }
                        }
                        else
                        {

                                programCalendarDetails = new ProgramCalendarDetails();
                                programCalendarDetails.ProgramId = model.ProgramId;
                                programCalendarDetails.CalendarId = CalendarViewModel.CalendarId;
                                programCalendarDetails.CreatedBy = model.LoginUserId.Value;
                                programCalendarDetails.CreatedDate = DateTime.Now;
                                _context.ProgramCalendarDetails.Add(programCalendarDetails);
                                IsinsertUpdate = true;
                        }
                        if (IsinsertUpdate)
                        {
                            _context.SaveChanges();
                            ids.Add(programCalendarDetails.CalendarId);
                        }
                    }

                    var listToRemove = _context.ProgramCalendarDetails
                                .Where(w => w.ProgramId == model.ProgramId && !ids.Contains(w.CalendarId)).ToList();

                    if (listToRemove != null && listToRemove.Count > 0)
                    {
                        _context.ProgramCalendarDetails.RemoveRange(listToRemove);
                        _context.SaveChanges();
                    }
                }
                return retVal;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void RemoveProgramCalendar(int ProgramId)
        {
            List<ProgramCalendarDetails> programCalendarDetails = _context.ProgramCalendarDetails.Where(w => w.ProgramId == ProgramId).ToList();
            if (programCalendarDetails != null && programCalendarDetails.Count > 0)
            {
                _context.ProgramCalendarDetails.RemoveRange(programCalendarDetails);
                _context.SaveChanges();
            }
        }

        public Programs GetProgramsByName(ProgramViewModel model)
        {
            Programs programs = new Programs();
            if (model.ProgramId > 0)
            {
                programs = _context.Programs.Where(p => p.ProgramName == model.Name.Trim() && p.ProgramId != model.ProgramId).FirstOrDefault();
            }
            else
            {
                programs = _context.Programs.Where(p => p.ProgramName == model.Name.Trim()).FirstOrDefault();
            }
            return programs;
        }

        public List<ProgramList> GetProgramList(ProgramListViewModel model, int start = 1, int pageSize = 10, string sortColumn = "UserName", int sortOrder = 1)
        {
            var viewModel = new List<ProgramList>();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[5];
                sqlParameter[0] = new SqlParameter("@SortColumn", sortColumn);
                sqlParameter[1] = new SqlParameter("@SortOrder", sortOrder);
                sqlParameter[2] = new SqlParameter("@PageSize", pageSize);
                sqlParameter[3] = new SqlParameter("@Start", start);
                if (model.ProgramName != null && !string.IsNullOrEmpty(model.ProgramName))
                {
                    sqlParameter[4] = new SqlParameter("@ProgramName", model.ProgramName.Trim());
                }
                else
                {
                    sqlParameter[4] = new SqlParameter("@ProgramName", DBNull.Value);
                }

                var dt = access.GetData("spProgramsData", CommandType.StoredProcedure, sqlParameter.ToArray());

                if (dt != null && dt.Rows.Count > 0)
                {
                    viewModel = commonService.ConvertToList<ProgramList>(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
            return viewModel;
        }

        public ProgramViewModel GetProgramData(int ProgramId)
        {
            ProgramViewModel model = new ProgramViewModel();
            Programs Programs = _context.Programs.Where(p => p.ProgramId == ProgramId).FirstOrDefault();
            if (Programs != null)
            {
                model.ProgramId = Programs.ProgramId;
                model.Name = Programs.ProgramName != null ? Programs.ProgramName : "";
                model.Description = Programs.ProgramDescription != null ? Programs.ProgramDescription : "";
                model.IsActive = Programs.IsActive;
                model.GLAccountCode = Programs.GLAccountCode != null ? Programs.GLAccountCode : "";
                model.ProgramID1 = Programs.ProgramId1 != null ? Programs.ProgramId1 : "";
                model.ProgramID2 = Programs.ProgramId2 != null ? Programs.ProgramId2 : "";
                model.ProgramID3 = Programs.ProgramId3 != null ? Programs.ProgramId3 : "";
                model.ProgramCalendarList = GetProgramCalendarList(model.ProgramId);
            }
            

            return model;
        }

        public List<ProgramCalendarListViewModel> GetProgramCalendarList(int? ProgramId)
        {
            try
            {

                var programCalendarListViewModel = new List<ProgramCalendarListViewModel>();

                programCalendarListViewModel = (from p in _context.ProgramCalendarDetails
                                                join cs in _context.CalendarSetting on p.CalendarId equals cs.CalendarId
                                                where p.ProgramId == ProgramId.Value && cs.IsActive == true
                                                orderby p.ProgramCalendarDetailsId descending
                                                 select new ProgramCalendarListViewModel
                                                 {
                                                     ProgramCalendarDetailsId = p.ProgramCalendarDetailsId,
                                                     ProgramId = p.ProgramId,
                                                     CalendarId = p.CalendarId,
                                                     CalendarName = cs.CalendarName,
                                                 }).Distinct().ToList();


                return programCalendarListViewModel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        public List<SelectListItem> GetCalendarList()
        {
            try
            {

                var calendarLists = new List<SelectListItem>();

                calendarLists = (from c in _context.CalendarSetting
                                orderby c.CalendarName
                                 where c.IsActive == true
                                 select new SelectListItem
                                {
                                    Value = c.CalendarId.ToString(),
                                    Text = c.CalendarName,
                                }).Distinct().ToList();


                return calendarLists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        public bool GetProgramCalendarSchedule(int? ProgramCalendarDetailsId)
        {
            bool ProgramCalendarScheduled = false;
            try
            {

                var PersonsDetails = new List<PersonsDetails>();

                PersonsDetails = (from c in _context.PersonsDetails
                                 join cs in _context.ProgramCalendarDetails on new { c.ProgramId, c.CalendarId } equals new { cs.ProgramId, cs.CalendarId}
                                 orderby c.PersonId
                                 where cs.ProgramCalendarDetailsId == ProgramCalendarDetailsId select c).Distinct().ToList();


                if(PersonsDetails !=null && PersonsDetails.Count > 0)
                {
                    ProgramCalendarScheduled = true;
                }
                return ProgramCalendarScheduled;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        

    }
}
