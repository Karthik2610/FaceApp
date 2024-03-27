using FaceApp.Data;
using FaceApp.Models;
using FaceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    public class ReportService : IDisposable
    {
        private readonly FaceAppDBContext _context;
        Authentication authenticate = new Authentication();
        DbAccess access = null;
        CommonService commonService = null;
        public ReportService(FaceAppDBContext context)
        {
            commonService = new CommonService(context);
            access = new DbAccess();
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public List<ReportListViewModel> GetDailyReportData(ReportsViewModel model,int? pageStart, int? pageEnd,string  SortColumn,int? sortorder)
        {
            DataTable dt = new DataTable();
            List<ReportListViewModel> items = new List<ReportListViewModel>();
            try
            {
                DataSet dsMyDataSet = new DataSet();
                SqlParameter[] sqlParameters1 = new SqlParameter[7];

                if (model.ProgramId.HasValue)
                {
                    sqlParameters1[0] = new SqlParameter("@ProgramId", model.ProgramId);
                }
                else
                {
                    sqlParameters1[0] = new SqlParameter("@ProgramId", DBNull.Value);
                }

                if (model.LocationId.HasValue)
                {
                    sqlParameters1[1] = new SqlParameter("@LocationId", model.LocationId);
                }
                else
                {
                    sqlParameters1[1] = new SqlParameter("@LocationId", DBNull.Value);
                }

                if (model.Date.HasValue)
                {
                    sqlParameters1[2] = new SqlParameter("@Date", model.Date);
                }
                else
                {
                    sqlParameters1[2] = new SqlParameter("@Date", DBNull.Value);
                }

                sqlParameters1[3] = new SqlParameter("@SortColumn", SortColumn);
                sqlParameters1[4] = new SqlParameter("@SortOrder", sortorder);
                sqlParameters1[5] = new SqlParameter("@PageSize", pageEnd);
                sqlParameters1[6] = new SqlParameter("@Start", pageStart);


                dsMyDataSet = access.ExecuteSqlQuery("spDailyReportData", sqlParameters1, CommandType.StoredProcedure, true);
                if (dsMyDataSet != null && dsMyDataSet.Tables.Count > 0)
                {
                    dt = dsMyDataSet.Tables[0];
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    items = dt.AsEnumerable().Select(row =>
                    new ReportListViewModel
                    {
                        Name = row.Field<string>("Name") != null ? row.Field<string>("Name").ToString() : "",
                        StatusIn = row.Field<bool?>("StatusIn") != null ? row.Field<bool?>("StatusIn") : false,
                        StatusOut = row.Field<bool?> ("StatusOut") != null ? row.Field<bool?>("StatusOut") : false,
                        TotalCount = row.Field<int?>("TotalCount") != null ? row.Field<int?>("TotalCount") : 0,
                        CheckInCount = row.Field<int?>("CheckInCount") != null ? row.Field<int?>("CheckInCount") : 0,
                        CheckOutCount = row.Field<int?>("CheckOutCount") != null ? row.Field<int?>("CheckOutCount") : 0,
                        PersonID1= row.Field<string>("PersonId1") != null ? row.Field<string>("PersonId1").ToString() :"",
                        LocationName = row.Field<string>("LocationName") != null ? row.Field<string>("LocationName").ToString() : "",
                        ProgramName = row.Field<string>("ProgramName") != null ? row.Field<string>("ProgramName").ToString() : "",
                        //ScheduledCount = row.Field<int?>("ScheduledCount") != null ? row.Field<int?>("ScheduledCount") : 0,
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ////log.Error(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
            return items;
        }

        public DataTable GetDailyReportDataDT(ReportsViewModel model, int? pageStart, int? pageEnd, string SortColumn, int? sortorder)
        {
            DataTable dt = new DataTable();
            List<ReportListViewModel> items = new List<ReportListViewModel>();
            try
            {
                DataSet dsMyDataSet = new DataSet();
                SqlParameter[] sqlParameters1 = new SqlParameter[7];

                if (model.ProgramId.HasValue)
                {
                    sqlParameters1[0] = new SqlParameter("@ProgramId", model.ProgramId);
                }
                else
                {
                    sqlParameters1[0] = new SqlParameter("@ProgramId", DBNull.Value);
                }

                if (model.LocationId.HasValue)
                {
                    sqlParameters1[1] = new SqlParameter("@LocationId", model.LocationId);
                }
                else
                {
                    sqlParameters1[1] = new SqlParameter("@LocationId", DBNull.Value);
                }

                if (model.Date.HasValue)
                {
                    sqlParameters1[2] = new SqlParameter("@Date", model.Date);
                }
                else
                {
                    sqlParameters1[2] = new SqlParameter("@Date", DBNull.Value);
                }

                sqlParameters1[3] = new SqlParameter("@SortColumn", SortColumn);
                sqlParameters1[4] = new SqlParameter("@SortOrder", sortorder);
                sqlParameters1[5] = new SqlParameter("@PageSize", pageEnd);
                sqlParameters1[6] = new SqlParameter("@Start", pageStart);


                dsMyDataSet = access.ExecuteSqlQuery("spDailyReportData", sqlParameters1, CommandType.StoredProcedure, true);
                if (dsMyDataSet != null && dsMyDataSet.Tables.Count > 0)
                {
                    dt = dsMyDataSet.Tables[0];
                }              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ////log.Error(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
            return dt;
        }

        public List<AttendanceGridFieldsListViewModel> GetProgramRowCount(string ReportDate, int? LocationId)
        {
            DataTable Result = new DataTable();
            List<AttendanceGridFieldsListViewModel> fieldList = new List<AttendanceGridFieldsListViewModel>();
            try
            {
                DataSet dsval = null;
                SqlParameter[] sqlparams = new SqlParameter[2];
                if (ReportDate != null)
                {
                    sqlparams[0] = new SqlParameter("@AttendanceDate", Convert.ToDateTime(ReportDate));
                }
                else
                {
                    sqlparams[0] = new SqlParameter("@AttendanceDate", DBNull.Value);
                }
                if (LocationId != null)
                {
                    sqlparams[1] = new SqlParameter("@LocationId", LocationId);
                }
                else
                {
                    sqlparams[1] = new SqlParameter("@LocationId", DBNull.Value);
                }

                //sqlparams[2] = new SqlParameter("@Year", ReportingYear);

                string strQuerry = string.Empty;

                strQuerry = @"SELECT Distinct 'Col'+CAST(A.RankNo as varchar(20))name , ProgramName title
                            FROM(
								  SELECT distinct P.PersonId,L.LocationName,P.LastName +','+P.FirstName PersonName,PR.ProgramName, Rank() OVER (ORDER BY PR.ProgramName) RankNo, 
									  CASE ISNULL(A.AttendanceType,'') WHEN 'CheckIn' THEN 1 WHEN 'CheckOut' THEN 2 ELSE 3 END Attendance,P.PersonId1
									FROM Persons P 
									INNER JOIN PersonsDetails PD ON P.PersonId=PD.PersonId
									INNER JOIN Locations L ON PD.LocationId=L.LocationId
									INNER JOIN Programs PR ON PD.ProgramId=PR.ProgramId
									INNER JOIN CalendarSetting C ON PD.CalendarId=C.CalendarId
									LEFT OUTER JOIN (
											SELECT PersonId,AttendanceDate,AttendanceType,LocationId,ProgramId FROM  
											(
												SELECT PersonId,CONVERT(Varchar,AttendanceDate,101) AttendanceDate,AttendanceType,LocationId,ProgramId, 
												ROW_NUMBER() OVER(PARTITION BY PersonId,CONVERT(DATETIME,CONVERT(Varchar,AttendanceDate,101)),LocationId,ProgramId ORDER BY PersonId,AttendanceDate Desc) RowNo
												from Attendance
											) Rslt WHERE RowNo=1) A ON PD.PersonId=A.PersonId AND PD.LocationId=A.LocationId AND PD.ProgramId=A.ProgramId 
										AND CONVERT(datetime,CONVERT(varchar,A.AttendanceDate,101))=@AttendanceDate 
									WHERE L.LocationId=@LocationId AND CONVERT(datetime,@AttendanceDate) BETWEEN PD.StartDate AND PD.EndDate
                           )A";

                dsval = access.ExecuteSqlQuery(strQuerry, sqlparams, CommandType.Text, false);
                //Result = Convert.ToInt32(dsval.Tables[0].Rows[0][0].ToString());
                Result = dsval.Tables[0];


                if (Result != null && Result.Rows.Count > 0)
                {
                    fieldList = commonService.ConvertToList<AttendanceGridFieldsListViewModel>(Result);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }

            return fieldList;
        }

        public List<ReportListViewModel> GetWeeklyReportData(ReportsViewModel model, int? pageStart, int? pageEnd, string SortColumn, int? sortorder)
        {
            DataTable dt = new DataTable();
            List<ReportListViewModel> items = new List<ReportListViewModel>();
            try
            {
                DataSet dsMyDataSet = new DataSet();
                SqlParameter[] sqlParameters1 = new SqlParameter[8];
                if (model.ProgramId.HasValue) { 
                    sqlParameters1[0] = new SqlParameter("@ProgramId", model.ProgramId);
                }
                else
                {
                    sqlParameters1[0] = new SqlParameter("@ProgramId", DBNull.Value);
                }

                if (model.LocationId.HasValue)
                {
                    sqlParameters1[1] = new SqlParameter("@LocationId", model.LocationId);
                }
                else
                {
                    sqlParameters1[1] = new SqlParameter("@LocationId", DBNull.Value);
                }

                if (model.FromDate.HasValue)
                {
                    sqlParameters1[2] = new SqlParameter("@FromDate", model.FromDate);
                }
                else
                {
                    sqlParameters1[2] = new SqlParameter("@FromDate", DBNull.Value);
                }

                if (model.ToDate.HasValue)
                {
                    sqlParameters1[3] = new SqlParameter("@ToDate", model.ToDate);
                }
                else
                {
                    sqlParameters1[3] = new SqlParameter("@ToDate", DBNull.Value);
                }

                sqlParameters1[4] = new SqlParameter("@SortColumn", SortColumn);
                sqlParameters1[5] = new SqlParameter("@SortOrder", sortorder);
                sqlParameters1[6] = new SqlParameter("@PageSize", pageEnd);
                sqlParameters1[7] = new SqlParameter("@Start", pageStart);

                dsMyDataSet = access.ExecuteSqlQuery("spWeeklyReportData", sqlParameters1, CommandType.StoredProcedure, true);
                if (dsMyDataSet != null && dsMyDataSet.Tables.Count > 0)
                {
                    dt = dsMyDataSet.Tables[0];
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    items = dt.AsEnumerable().Select(row =>
                    new ReportListViewModel
                    {
                        Name = row.Field<string>("Name") != null ? row.Field<string>("Name").ToString() : "",
                        //ProgramName = row.Field<string>("ProgramName") != null ? row.Field<string>("ProgramName") : "",
                        //LocationName = row.Field<string>("LocationName") != null ? row.Field<string>("LocationName").ToString() : "",
                        Date = row.Field<string>("AttendanceDate") != null ? row.Field<string>("AttendanceDate") : "",
                        Duration = row.Field<string>("Duration") != null ? row.Field<string>("Duration").ToString() : "",
                        TotalCount = row.Field<int?>("TotalCount") != null ? row.Field<int?>("TotalCount") : 0,
                        PersonID1 = row.Field<string>("PersonId1") != null ? row.Field<string>("PersonId1").ToString() : "",
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                ////log.Error(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
            return items;
        }

        public DataTable GetWeeklyReportDataDT(ReportsViewModel model, int? pageStart, int? pageEnd, string SortColumn, int? sortorder)
        {
            DataTable dt = new DataTable();
            List<ReportListViewModel> items = new List<ReportListViewModel>();
            try
            {
                DataSet dsMyDataSet = new DataSet();
                SqlParameter[] sqlParameters1 = new SqlParameter[8];
                if (model.ProgramId.HasValue)
                {
                    sqlParameters1[0] = new SqlParameter("@ProgramId", model.ProgramId);
                }
                else
                {
                    sqlParameters1[0] = new SqlParameter("@ProgramId", DBNull.Value);
                }

                if (model.LocationId.HasValue)
                {
                    sqlParameters1[1] = new SqlParameter("@LocationId", model.LocationId);
                }
                else
                {
                    sqlParameters1[1] = new SqlParameter("@LocationId", DBNull.Value);
                }

                if (model.FromDate.HasValue)
                {
                    sqlParameters1[2] = new SqlParameter("@FromDate", model.FromDate);
                }
                else
                {
                    sqlParameters1[2] = new SqlParameter("@FromDate", DBNull.Value);
                }

                if (model.ToDate.HasValue)
                {
                    sqlParameters1[3] = new SqlParameter("@ToDate", model.ToDate);
                }
                else
                {
                    sqlParameters1[3] = new SqlParameter("@ToDate", DBNull.Value);
                }

                sqlParameters1[4] = new SqlParameter("@SortColumn", SortColumn);
                sqlParameters1[5] = new SqlParameter("@SortOrder", sortorder);
                sqlParameters1[6] = new SqlParameter("@PageSize", pageEnd);
                sqlParameters1[7] = new SqlParameter("@Start", pageStart);

                dsMyDataSet = access.ExecuteSqlQuery("spWeeklyReportData", sqlParameters1, CommandType.StoredProcedure, true);
                if (dsMyDataSet != null && dsMyDataSet.Tables.Count > 0)
                {
                    dt = dsMyDataSet.Tables[0];
                }

               
            }
            catch (Exception ex)
            {
                ////log.Error(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
            return dt;
        }

    }
}
