using FaceApp.Data;
using FaceApp.Models;
using FaceApp.Models.ViewModels;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
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
    public class SettingsService : IDisposable
    {
        private readonly FaceAppDBContext _context;
        Authentication authenticate = new Authentication();
        DbAccess access = null;
        CommonService commonService = null;
        public SettingsService(FaceAppDBContext context)
        {
            commonService = new CommonService(context);
            access = new DbAccess();
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public int CreateCalendar(SettingsViewModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    DateTime? DateNull = null;
                    CalendarSetting calendar = new CalendarSetting();
                    calendar.CalendarName = model.Name.Trim();
                    calendar.IsActive = model.IsActive;
                    calendar.SundayStartTime = model.SundayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SundayStartTime) : DateNull;
                    calendar.SundayEndTime = model.SundayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SundayEndTime) : DateNull;
                    calendar.SundayIsActive = model.SundayIsActive;
                    calendar.MondayEndTime = model.MondayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.MondayEndTime) : DateNull;
                    calendar.MondayStartTime = model.MondayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.MondayStartTime) : DateNull;
                    calendar.MondayIsActive = model.MondayIsActive;
                    calendar.TuesdayEndTime = model.TuesdayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.TuesdayEndTime) : DateNull;
                    calendar.TuesdayStartTime = model.TuesdayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.TuesdayStartTime) : DateNull;
                    calendar.TuesdayIsActive = model.TuesdayIsActive;
                    calendar.WednesdayEndTime = model.WednesdayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.WednesdayEndTime) : DateNull;
                    calendar.WednesdayStartTime = model.WednesdayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.WednesdayStartTime) : DateNull;
                    calendar.WednesdayIsActive = model.WednesdayIsActive;
                    calendar.ThursdayEndTime = model.ThursdayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.ThursdayEndTime) : DateNull;
                    calendar.ThursdayStartTime = model.ThursdayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.ThursdayStartTime) : DateNull;
                    calendar.ThursdayIsActive = model.ThursdayIsActive;
                    calendar.FridayEndTime = model.FridayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.FridayEndTime) : DateNull;
                    calendar.FridayStartTime = model.FridayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.FridayStartTime) : DateNull;
                    calendar.FridayIsActive = model.FridayIsActive;
                    calendar.SaturdayEndTime = model.SaturdayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SaturdayEndTime) : DateNull;
                    calendar.SaturdayStartTime = model.SaturdayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SaturdayStartTime) : DateNull;
                    calendar.SaturdayIsActive = model.SaturdayIsActive;
                    calendar.CreatedDate = DateTime.Now;
                    calendar.CreatedBy = model.LoginUserId;
                    _context.CalendarSetting.Add(calendar);
                    _context.SaveChanges();
                    transaction.Commit();
                    model.CalendarId = calendar.CalendarId;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }
            return model.CalendarId;
        }

        public int UpdateCalendar(SettingsViewModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    DateTime? DateNull = null;
                    CalendarSetting calendar = _context.CalendarSetting.Where(u => u.CalendarId == model.CalendarId).FirstOrDefault();
                    calendar.CalendarName = model.Name.Trim();
                    calendar.IsActive = model.IsActive;
                    calendar.SundayStartTime = model.SundayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SundayStartTime) : DateNull;
                    calendar.SundayEndTime = model.SundayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SundayEndTime) : DateNull;
                    calendar.SundayIsActive = model.SundayIsActive;
                    calendar.MondayEndTime = model.MondayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.MondayEndTime) : DateNull;
                    calendar.MondayStartTime = model.MondayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.MondayStartTime) : DateNull;
                    calendar.MondayIsActive = model.MondayIsActive;
                    calendar.TuesdayEndTime = model.TuesdayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.TuesdayEndTime) : DateNull;
                    calendar.TuesdayStartTime = model.TuesdayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.TuesdayStartTime) : DateNull;
                    calendar.TuesdayIsActive = model.TuesdayIsActive;
                    calendar.WednesdayEndTime = model.WednesdayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.WednesdayEndTime) : DateNull;
                    calendar.WednesdayStartTime = model.WednesdayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.WednesdayStartTime) : DateNull;
                    calendar.WednesdayIsActive = model.WednesdayIsActive;
                    calendar.ThursdayEndTime = model.ThursdayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.ThursdayEndTime) : DateNull;
                    calendar.ThursdayStartTime = model.ThursdayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.ThursdayStartTime) : DateNull;
                    calendar.ThursdayIsActive = model.ThursdayIsActive;
                    calendar.FridayEndTime = model.FridayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.FridayEndTime) : DateNull;
                    calendar.FridayStartTime = model.FridayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.FridayStartTime) : DateNull;
                    calendar.FridayIsActive = model.FridayIsActive;
                    calendar.SaturdayEndTime = model.SaturdayEndTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SaturdayEndTime) : DateNull;
                    calendar.SaturdayStartTime = model.SaturdayStartTime != null ? Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SaturdayStartTime) : DateNull;
                    calendar.SaturdayIsActive = model.SaturdayIsActive;
                    calendar.ModifiedDate = DateTime.Now;
                    calendar.ModifiedBy = model.LoginUserId;
                    _context.Entry(calendar).State = EntityState.Modified;
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.Message);
                    transaction.Rollback();
                }
            }
            return model.CalendarId;
        }

        public CalendarSetting GetCalendarByName(SettingsViewModel model)
        {
            CalendarSetting calendarSetting = new CalendarSetting();
            if (model.CalendarId > 0)
            {
                calendarSetting = _context.CalendarSetting.Where(l => l.CalendarName == model.Name.Trim() && l.CalendarId != model.CalendarId).FirstOrDefault();
            }
            else
            {
                calendarSetting = _context.CalendarSetting.Where(l => l.CalendarName == model.Name.Trim()).FirstOrDefault();
            }
            return calendarSetting;
        }

        public List<SettingsList> GetSettingsList(SettingsListViewModel model,int start = 1, int pageSize = 10, string sortColumn = "UserName", int sortOrder = 1)
        {
            var viewModel = new List<SettingsList>();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[5];
                sqlParameter[0] = new SqlParameter("@SortColumn", sortColumn);
                sqlParameter[1] = new SqlParameter("@SortOrder", sortOrder);
                sqlParameter[2] = new SqlParameter("@PageSize", pageSize);
                sqlParameter[3] = new SqlParameter("@Start", start);
                if (model.CalendarName != null && !string.IsNullOrEmpty(model.CalendarName))
                {
                    sqlParameter[4] = new SqlParameter("@CalendarName", model.CalendarName.Trim());
                }
                else
                {
                    sqlParameter[4] = new SqlParameter("@CalendarName", DBNull.Value);
                }

                var dt = access.GetData("spCalendarsData", CommandType.StoredProcedure, sqlParameter.ToArray());

                if (dt != null && dt.Rows.Count > 0)
                {
                    viewModel = commonService.ConvertToList<SettingsList>(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
            return viewModel;
        }

        public SettingsViewModel GetCalendarData(int CalendarId)
        {
            SettingsViewModel model = new SettingsViewModel();
            CalendarSetting calendar = _context.CalendarSetting.Where(s => s.CalendarId == CalendarId).FirstOrDefault();
            if (calendar != null)
            {
                model.CalendarId = calendar.CalendarId;
                model.Name = calendar.CalendarName;
                model.IsActive = calendar.IsActive;
                model.SundayStartTime = calendar.SundayStartTime !=null ? calendar.SundayStartTime.Value.ToString("hh:mm tt") : "";
                model.SundayEndTime = calendar.SundayEndTime != null ? calendar.SundayEndTime.Value.ToString("hh:mm tt") : "";
                model.SundayIsActive = calendar.SundayIsActive;
                model.MondayStartTime = calendar.MondayStartTime != null ? calendar.MondayStartTime.Value.ToString("hh:mm tt") : "";
                model.MondayEndTime = calendar.MondayEndTime != null ? calendar.MondayEndTime.Value.ToString("hh:mm tt") : "";
                model.MondayIsActive = calendar.MondayIsActive;
                model.TuesdayStartTime = calendar.TuesdayStartTime != null ? calendar.TuesdayStartTime.Value.ToString("hh:mm tt") : "";
                model.TuesdayEndTime = calendar.TuesdayEndTime != null ? calendar.TuesdayEndTime.Value.ToString("hh:mm tt") : "";
                model.TuesdayIsActive = calendar.TuesdayIsActive;
                model.WednesdayStartTime = calendar.WednesdayStartTime != null ? calendar.WednesdayStartTime.Value.ToString("hh:mm tt") : "";
                model.WednesdayEndTime = calendar.WednesdayEndTime != null ? calendar.WednesdayEndTime.Value.ToString("hh:mm tt") : "";
                model.WednesdayIsActive = calendar.WednesdayIsActive;
                model.ThursdayStartTime = calendar.ThursdayStartTime != null ? calendar.ThursdayStartTime.Value.ToString("hh:mm tt") : "";
                model.ThursdayEndTime = calendar.ThursdayEndTime != null ? calendar.ThursdayEndTime.Value.ToString("hh:mm tt") : "";
                model.ThursdayIsActive = calendar.ThursdayIsActive;
                model.FridayStartTime = calendar.FridayStartTime != null ? calendar.FridayStartTime.Value.ToString("hh:mm tt") : "";
                model.FridayEndTime = calendar.FridayEndTime != null ? calendar.FridayEndTime.Value.ToString("hh:mm tt") : "";
                model.FridayIsActive = calendar.FridayIsActive;
                model.SaturdayStartTime = calendar.SaturdayStartTime != null ? calendar.SaturdayStartTime.Value.ToString("hh:mm tt") : "";
                model.SaturdayEndTime = calendar.SaturdayEndTime != null ? calendar.SaturdayEndTime.Value.ToString("hh:mm tt") : "";
                model.SaturdayIsActive = calendar.SaturdayIsActive;
            }

            return model;
        }
    }
}
