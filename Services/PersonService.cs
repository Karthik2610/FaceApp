using FaceApp.Data;
using FaceApp.Models;
using FaceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
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
using Microsoft.Extensions.Configuration;

namespace FaceApp.Services
{
    public class PersonService : IDisposable
    {
        private readonly FaceAppDBContext _context;
        Authentication authenticate = new Authentication();
        DbAccess access = null;
        CommonService commonService = null;
        public PersonService(FaceAppDBContext context)
        {
            commonService = new CommonService(context);
            access = new DbAccess();
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        public int CreatePerson(PersonsViewModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Persons persons = new Persons();
                    persons.FirstName = model.FirstName.Trim();
                    persons.LastName = model.LastName.Trim();
                    persons.MiddleInitial = model.MiddleInitial != null ? model.MiddleInitial.Trim() : model.MiddleInitial;
                    persons.Address1 = model.Address1 != null ? model.Address1.Trim() : model.Address1;
                    persons.Address2 = model.Address2 != null ? model.Address2.Trim() : model.Address2;
                    persons.City = model.City != null ? model.City.Trim() : model.City;
                    persons.State = model.State != null ? model.State.Trim() : model.State;
                    persons.Zip = model.Zip != null ? model.Zip.Trim() : model.Zip;
                    persons.PersonId1 = model.PersonId1 != null ? model.PersonId1.Trim() : model.PersonId1;
                    persons.PersonId2 = model.PersonId2 != null ? model.PersonId2.Trim() : model.PersonId2;
                    persons.PersonId3 = model.PersonId3 != null ? model.PersonId3.Trim() : model.PersonId3;
                    persons.IsActive = model.IsActive;
                    if (!string.IsNullOrEmpty(model.DOB))
                    {
                        //persons.DOB= Convert.ToDateTime(model.DOB);
                        persons.DOB = DateTime.ParseExact(model.DOB, "MM/dd/yyyy", null);
                    }
                    persons.CreatedDate = DateTime.Now;
                    persons.CreatedBy = model.LoginUserId;
                    _context.Persons.Add(persons);
                    _context.SaveChanges();
                    model.PersonId = persons.PersonId;
                    if (model.PersonId > 0)
                    {
                        if (model.PersonDetailsList != null && model.PersonDetailsList.Count > 0)
                        {
                            SavePersonDetails(model);
                        }
                        else
                        {
                            RemovePersonDetails(model.PersonId);
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }
            return model.PersonId;
        }
        public int UpdatePerson(PersonsViewModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    Persons persons = _context.Persons.Where(u => u.PersonId == model.PersonId).FirstOrDefault();
                    persons.FirstName = model.FirstName.Trim();
                    persons.LastName = model.LastName.Trim();
                    persons.MiddleInitial = model.MiddleInitial !=null? model.MiddleInitial.Trim() : model.MiddleInitial;
                    persons.Address1 = model.Address1 !=null ? model.Address1.Trim(): model.Address1;
                    persons.Address2 = model.Address2 !=null?model.Address2.Trim(): model.Address2;
                    persons.City = model.City != null ? model.City.Trim() : model.City;
                    persons.State = model.State != null ? model.State.Trim() : model.State;
                    persons.Zip = model.Zip != null ? model.Zip.Trim() : model.Zip;
                    persons.PersonId1 = model.PersonId1 != null ? model.PersonId1.Trim() : model.PersonId1;
                    persons.PersonId2 = model.PersonId2 != null ? model.PersonId2.Trim() : model.PersonId2;
                    persons.PersonId3 = model.PersonId3 != null ? model.PersonId3.Trim() : model.PersonId3;
                    persons.IsActive = model.IsActive;
                    if (!string.IsNullOrEmpty(model.DOB))
                    {
                        //persons.DOB = Convert.ToDateTime(model.DOB);
                        persons.DOB = DateTime.ParseExact(model.DOB, "MM/dd/yyyy", null);
                    }
                    persons.ModifiedDate = DateTime.Now;
                    persons.ModifiedBy = model.LoginUserId;
                    _context.Entry(persons).State = EntityState.Modified;
                    _context.SaveChanges();
                    model.PersonId = persons.PersonId;
                    if (model.PersonId > 0)
                    {
                        if (model.PersonDetailsList != null && model.PersonDetailsList.Count > 0)
                        {
                            SavePersonDetails(model);
                        }
                        else
                        {
                            RemovePersonDetails(model.PersonId);
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
            return model.PersonId;
        }

        public void UpdateAttendance(int PersonId,string Modeoftransportation,string AttendanceType, int? LocationId, int? ProgramId,string TransitNumber,int CreatedBy, string CheckOutDate=null)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    Attendance attendance = new Attendance();
                    attendance.PersonId = PersonId;
                    attendance.AttendanceDate = CheckOutDate !=null ? Convert.ToDateTime(CheckOutDate): DateTime.Now;
                    attendance.ModeofTransportation = Modeoftransportation;
                    attendance.AttendanceType = AttendanceType;
                    attendance.TransitNumber = TransitNumber;
                    attendance.LocationId = LocationId;
                    attendance.ProgramId = ProgramId;
                    attendance.CreatedBy = CreatedBy;
                    _context.Attendance.Add(attendance);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.Message);
                    transaction.Rollback();
                }
            }
        }

        private bool SavePersonDetails(PersonsViewModel model)
        {
            try
            {
                bool retVal = true;
                if (model.PersonDetailsList != null && model.PersonDetailsList.Count > 0)
                {
                    PersonsDetails NewpersonsDetails = new PersonsDetails();
                    List<ProgramCalendarDetails> ListprogramCalendarDetails = new List<ProgramCalendarDetails>();
                    bool IsinsertUpdate = false;
                    int? LocationId = 0;
                    List<int> ids = new List<int>();
                    foreach (PersonDetailsListViewModel personsDetail in model.PersonDetailsList)
                    {
                        LocationId = personsDetail.LocationId;
                        IsinsertUpdate = false;
                        NewpersonsDetails = new PersonsDetails();
                        if (personsDetail.PersonsDetailsId > 0)
                        {
                            NewpersonsDetails = _context.PersonsDetails.FirstOrDefault(x => x.PersonsDetailsId == personsDetail.PersonsDetailsId);
                            if (NewpersonsDetails != null)
                            {
                                NewpersonsDetails.ModifiedBy = model.LoginUserId;
                                NewpersonsDetails.ModifiedDate = System.DateTime.Now;
                                IsinsertUpdate = true;
                            }
                        }
                        else
                        {

                            NewpersonsDetails = new PersonsDetails();
                            NewpersonsDetails.PersonId = model.PersonId;
                            NewpersonsDetails.LocationId = personsDetail.LocationId.Value;
                            NewpersonsDetails.ProgramId = personsDetail.ProgramId.Value;
                            NewpersonsDetails.CalendarId = personsDetail.CalendarId.Value;
                            if (!string.IsNullOrEmpty(personsDetail.StartDate))
                            {
                                NewpersonsDetails.StartDate = DateTime.ParseExact(personsDetail.StartDate, "MM/dd/yyyy", null);
                            }
                            if (!string.IsNullOrEmpty(personsDetail.EndDate))
                            {
                                NewpersonsDetails.EndDate = DateTime.ParseExact(personsDetail.EndDate, "MM/dd/yyyy", null);
                            }
                            NewpersonsDetails.CreatedBy = model.LoginUserId;
                            NewpersonsDetails.CreatedDate = DateTime.Now;
                            _context.PersonsDetails.Add(NewpersonsDetails);
                            IsinsertUpdate = true;
                        }
                        if (IsinsertUpdate)
                        {
                            _context.SaveChanges();
                            ids.Add(NewpersonsDetails.PersonsDetailsId);
                        }
                    }

                    var listToRemove = _context.PersonsDetails
                                .Where(w => w.PersonId == model.PersonId && !ids.Contains(w.PersonsDetailsId)).ToList();

                    if (listToRemove != null && listToRemove.Count > 0)
                    {
                        _context.PersonsDetails.RemoveRange(listToRemove);
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

        private void RemovePersonDetails(int PersonId)
        {
            List<PersonsDetails> personsDetails = _context.PersonsDetails.Where(w => w.PersonId == PersonId).ToList();
            if (personsDetails != null && personsDetails.Count > 0)
            {
                _context.PersonsDetails.RemoveRange(personsDetails);
                _context.SaveChanges();
            }
        }
        public Persons GetPersonById(int Id)
        {
            Persons person = _context.Persons.Where(s => s.PersonId == Id).FirstOrDefault();
            return person;
        }

        public bool GetAttendanceCheckOutStatus(int PersonId,Face face)
        {
            bool CheckOutStatus = true;
            Attendance LastCheckIn = GetLastCheckIn(PersonId, face);

            if (LastCheckIn != null && LastCheckIn.AttendanceId > 0)
            {
                Attendance LastCheckOut = GetLastCheckOut(PersonId, face, LastCheckIn);

                if (LastCheckOut == null || LastCheckOut.AttendanceId == 0)
                {
                    CheckOutStatus = false;
                }
            }
            return CheckOutStatus;
        }

        public Attendance GetLastCheckIn(int PersonId,Face face)
        {
            // Attendance LastCheckIn = _context.Attendance.Where(s => s.PersonId == PersonId 
            // && s.AttendanceType == "CheckIn"
            // && s.LocationId == face.LocationId
            // && s.ProgramId == face.ProgramId
            //).OrderByDescending(e => e.AttendanceDate).FirstOrDefault();

            // return LastCheckIn;

            Attendance result = new Attendance();
            DataTable Result = new DataTable();
            try
            {
                DataSet dsval = null;
                SqlParameter[] sqlparams = new SqlParameter[3];
                sqlparams[0] = new SqlParameter("@PersonId", PersonId);
                sqlparams[1] = new SqlParameter("@LocationId", face.LocationId);
                sqlparams[2] = new SqlParameter("@ProgramId", face.ProgramId);
                string strQuerry = string.Empty;

                                strQuerry = @"SELECT TOP 1 * FROM Attendance A
                                              WHERE A.AttendanceType='CheckIn' AND A.PersonId =@PersonId AND A.LocationId =ISNULL(@LocationId,A.LocationId)
                                              ORDER BY AttendanceDate DESC";

                dsval = access.ExecuteSqlQuery(strQuerry, sqlparams, CommandType.Text, false);
                //Result = Convert.ToInt32(dsval.Tables[0].Rows[0][0].ToString());
                Result = dsval.Tables[0];


                if (Result != null && Result.Rows.Count > 0)
                {
                        result = commonService.ConvertToList<Attendance>(Result)[0];
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }

            return result;
        }

        public Attendance GetLastCheckOut(int PersonId, Face face, Attendance attendance)
        {
            Attendance result = new Attendance();
            DataTable Result = new DataTable();
            try
            {
                DataSet dsval = null;
                SqlParameter[] sqlparams = new SqlParameter[4];
                sqlparams[0] = new SqlParameter("@PersonId", PersonId);
                sqlparams[1] = new SqlParameter("@LocationId", face.LocationId);
                sqlparams[2] = new SqlParameter("@ProgramId", face.ProgramId);
                sqlparams[3] = new SqlParameter("@AttendanceId", attendance.AttendanceId);
                string strQuerry = string.Empty;

                strQuerry = @"SELECT TOP 1 * FROM Attendance A
                              WHERE A.AttendanceType='CheckOut' AND A.PersonId =@PersonId AND A.LocationId =ISNULL(@LocationId,A.LocationId) AND A.AttendanceId > @AttendanceId
                              ORDER BY AttendanceDate DESC";

                dsval = access.ExecuteSqlQuery(strQuerry, sqlparams, CommandType.Text, false);
                //Result = Convert.ToInt32(dsval.Tables[0].Rows[0][0].ToString());
                Result = dsval.Tables[0];


                if (Result != null && Result.Rows.Count > 0)
                {
                    result = commonService.ConvertToList<Attendance>(Result)[0];
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }

            return result;
        }

        public bool GetAttendanceCheckInStatus(int PersonId,Face face)
        {
            bool CheckInStatus = false;
            Attendance LastCheckIn = GetLastCheckIn(PersonId, face);

            if (LastCheckIn != null)
            {
                Attendance LastCheckOut = _context.Attendance.Where(s => s.PersonId == PersonId 
                && s.AttendanceType == "CheckOut"
                && s.LocationId == face.LocationId
                && s.ProgramId == face.ProgramId
                && s.AttendanceDate >= LastCheckIn.AttendanceDate
                ).OrderByDescending(e => e.AttendanceDate).FirstOrDefault();
                
                if(LastCheckOut == null)
                {
                    CheckInStatus = true;
                }
            }
            return CheckInStatus;
        }

        //public bool GetTodayAttendaceCheckInStatus(int PersonId, Face face)
        //{
        //    bool CheckInStatus = false;

        //        Attendance LastCheckIn = _context.Attendance.Where(s => s.PersonId == PersonId
        //        && s.AttendanceType == "CheckIn" && s.ProgramId == face.ProgramId && s.LocationId == face.LocationId && s.AttendanceDate.ToShortDateString() == DateTime.Now.ToShortDateString()
        //        ).OrderByDescending(e => e.AttendanceDate).FirstOrDefault();

        //        if (LastCheckIn != null)
        //        {
        //            CheckInStatus = true;
        //        }
        //    return CheckInStatus;
        //}

        public bool GetTodayAttendaceCheckInStatus(int PersonId, Face face, int GraceTime)
        {
            bool result = false;
            DataTable Result = new DataTable();
            try
            {
                DataSet dsval = null;
                SqlParameter[] sqlparams = new SqlParameter[5];
                sqlparams[0] = new SqlParameter("@PersonId", PersonId);
                sqlparams[1] = new SqlParameter("@LocationId", face.LocationId);
                sqlparams[2] = new SqlParameter("@ProgramId", face.ProgramId);
                sqlparams[3] = new SqlParameter("@GraceTime", GraceTime);
                sqlparams[4] = new SqlParameter("@Date", DateTime.Now);
                string strQuerry = string.Empty;

                strQuerry = @" Create Table #CalendarSetting (PersonId Int,LocationId int,ProgramId int,StartTime time,EndTime time);
								INSERT INTO #CalendarSetting
                                SELECT * FROM
                                (
	                                SELECT pd.PersonId,pd.LocationId,pd.ProgramId, CASE 
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Sunday' THEN  CASE WHEN SundayIsActive=1 THEN CAST(SundayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Monday' THEN CASE WHEN MondayIsActive=1 THEN CAST(MondayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Tuesday' THEN CASE WHEN TuesdayIsActive=1 THEN CAST(TuesdayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Wednesday' THEN CASE WHEN WednesdayIsActive=1 THEN CAST(WednesdayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Thursday' THEN CASE WHEN ThursdayIsActive=1 THEN CAST(ThursdayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Friday' THEN CASE WHEN FridayIsActive=1 THEN CAST(FridayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Saturday' THEN CASE WHEN SaturdayIsActive=1 THEN CAST(SaturdayStartTime as time)  ELSE CAST('00:00' as time) END
	                                END StartTime,
	                                CASE 
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Sunday' THEN CASE WHEN SundayIsActive=1 THEN CAST(SundayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Monday' THEN CASE WHEN MondayIsActive=1 THEN CAST(MondayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Tuesday' THEN CASE WHEN TuesdayIsActive=1 THEN CAST(TuesdayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Wednesday' THEN CASE WHEN WednesdayIsActive=1 THEN CAST(WednesdayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Thursday' THEN CASE WHEN ThursdayIsActive=1 THEN CAST(ThursdayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Friday' THEN CASE WHEN FridayIsActive=1 THEN CAST(FridayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Saturday' THEN CASE WHEN SaturdayIsActive=1 THEN CAST(SaturdayEndTime as time)  ELSE CAST('00:00' as time) END
	                                END EndTime
	                                FROM PersonsDetails pd
	                                INNER JOIN CalendarSetting C ON C.CalendarId=pd.CalendarId
	                                WHERE pd.PersonId=@PersonId 
	                                AND pd.LocationId =ISNULL(@LocationId,pd.LocationId)
	                                AND pd.ProgramId =ISNULL(@ProgramId,pd.ProgramId)
	                                AND CONVERT(DATETIME,CONVERT(varchar,@Date,101)) BETWEEN CONVERT(DATETIME,CONVERT(varchar,pd.StartDate,101)) AND CONVERT(DATETIME,CONVERT(varchar,pd.EndDate,101))
                                )x
                                WHERE StartTime IS NOT NULL AND EndTime IS NOT NULL
								AND CAST(@Date as time) < EndTime

                                SELECT * FROM Attendance A
                                INNER JOIN #CalendarSetting C ON C.PersonId=A.PersonId AND C.ProgramId=A.ProgramId AND C.LocationId=A.LocationId
                                WHERE A.AttendanceType='CheckIn' AND A.PersonId =@PersonId AND A.LocationId =ISNULL(@LocationId,A.LocationId)
                                AND A.ProgramId =ISNULL(@ProgramId,A.ProgramId)
                                AND CAST(A.AttendanceDate as time)  BETWEEN CAST(DATEADD(minute, -@GraceTime, CAST(C.StartTime as time)) as time) AND C.EndTime AND CONVERT(varchar,A.AttendanceDate,101) = CONVERT(varchar,@Date,101)
                                DROP TABLE #CalendarSetting;";

                dsval = access.ExecuteSqlQuery(strQuerry, sqlparams, CommandType.Text, false);
                //Result = Convert.ToInt32(dsval.Tables[0].Rows[0][0].ToString());
                Result = dsval.Tables[0];


                if (Result != null && Result.Rows.Count > 0)
                {
                    result = true;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }

            return result;
        }

        public bool GetLastAttendaceCheckInStatus(int PersonId, Face face)
        {
            bool result = false;
            DataTable Result = new DataTable();
            try
            {
                DataSet dsval = null;
                SqlParameter[] sqlparams = new SqlParameter[3];

                sqlparams[0] = new SqlParameter("@PersonId", PersonId);
                sqlparams[1] = new SqlParameter("@LocationId", face.LocationId);
                sqlparams[2] = new SqlParameter("@ProgramId", face.ProgramId);
                string strQuerry = string.Empty;

                strQuerry = @"SELECT TOP 1 * FROM Attendance A
                                WHERE A.AttendanceType='CheckIn' 
                                AND  A.PersonId =@PersonId AND A.LocationId =ISNULL(@LocationId,A.LocationId)
                                AND A.ProgramId =ISNULL(@ProgramId,A.ProgramId)
                                ORDER BY A.AttendanceDate Desc;";

                dsval = access.ExecuteSqlQuery(strQuerry, sqlparams, CommandType.Text, false);
                //Result = Convert.ToInt32(dsval.Tables[0].Rows[0][0].ToString());
                Result = dsval.Tables[0];


                if (Result != null && Result.Rows.Count > 0)
                {
                    result = true;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }

            return result;
        }

        public bool GetPersonLocationProgramDetails(int PersonId, Face face)
        {
            bool result = false;
            DataTable Result = new DataTable();
            try
            {
                DataSet dsval = null;
                SqlParameter[] sqlparams = new SqlParameter[3];

                sqlparams[0] = new SqlParameter("@PersonId", PersonId);
                sqlparams[1] = new SqlParameter("@LocationId", face.LocationId);
                sqlparams[2] = new SqlParameter("@ProgramId", face.ProgramId);
                string strQuerry = string.Empty;

                strQuerry = @"SELECT * FROM PersonsDetails pd
                            WHERE pd.PersonId=@PersonId 
                            AND pd.LocationId =ISNULL(@LocationId,pd.LocationId)
                            AND pd.ProgramId =ISNULL(@ProgramId,pd.ProgramId)";

                dsval = access.ExecuteSqlQuery(strQuerry, sqlparams, CommandType.Text, false);
                //Result = Convert.ToInt32(dsval.Tables[0].Rows[0][0].ToString());
                Result = dsval.Tables[0];


                if (Result != null && Result.Rows.Count > 0)
                {
                    result= true;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }

            return result;
        }

        public bool GetPersonLocationProgramScheduleDetails(int PersonId, Face face,int GraceTime)
        {
            bool result = false;
            DataTable Result = new DataTable();
            try
            {
                DataSet dsval = null;
                SqlParameter[] sqlparams = new SqlParameter[5];

                sqlparams[0] = new SqlParameter("@PersonId", PersonId);
                sqlparams[1] = new SqlParameter("@LocationId", face.LocationId);
                sqlparams[2] = new SqlParameter("@ProgramId", face.ProgramId);
                sqlparams[3] = new SqlParameter("@GraceTime", GraceTime);
                sqlparams[4] = new SqlParameter("@Date", DateTime.Now);
                string strQuerry = string.Empty;

                strQuerry = @"SELECT * FROM
                                (
	                                SELECT pd.PersonId,pd.LocationId,pd.ProgramId, CASE 
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Sunday' THEN  CASE WHEN SundayIsActive=1 THEN CAST(SundayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Monday' THEN CASE WHEN MondayIsActive=1 THEN CAST(MondayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Tuesday' THEN CASE WHEN TuesdayIsActive=1 THEN CAST(TuesdayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Wednesday' THEN CASE WHEN WednesdayIsActive=1 THEN CAST(WednesdayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Thursday' THEN CASE WHEN ThursdayIsActive=1 THEN CAST(ThursdayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Friday' THEN CASE WHEN FridayIsActive=1 THEN CAST(FridayStartTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Saturday' THEN CASE WHEN SaturdayIsActive=1 THEN CAST(SaturdayStartTime as time)  ELSE CAST('00:00' as time) END
	                                END StartTime,
	                                CASE 
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Sunday' THEN CASE WHEN SundayIsActive=1 THEN CAST(SundayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Monday' THEN CASE WHEN MondayIsActive=1 THEN CAST(MondayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Tuesday' THEN CASE WHEN TuesdayIsActive=1 THEN CAST(TuesdayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Wednesday' THEN CASE WHEN WednesdayIsActive=1 THEN CAST(WednesdayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Thursday' THEN CASE WHEN ThursdayIsActive=1 THEN CAST(ThursdayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Friday' THEN CASE WHEN FridayIsActive=1 THEN CAST(FridayEndTime as time)  ELSE CAST('00:00' as time) END
		                                WHEN FORMAT(CAST(@Date AS DATE), 'ddddddddd') = 'Saturday' THEN CASE WHEN SaturdayIsActive=1 THEN CAST(SaturdayEndTime as time)  ELSE CAST('00:00' as time) END
	                                END EndTime
	                                FROM PersonsDetails pd
	                                INNER JOIN CalendarSetting C ON C.CalendarId=pd.CalendarId
	                                WHERE pd.PersonId=@PersonId 
	                                AND pd.LocationId =ISNULL(@LocationId,pd.LocationId)
	                                AND pd.ProgramId =ISNULL(@ProgramId,pd.ProgramId)
                                )x
                                WHERE StartTime IS NOT NULL AND EndTime IS NOT NULL AND CAST(@Date as time) BETWEEN CAST(DATEADD(minute, -@GraceTime, CAST(StartTime as time)) as time) AND CAST(EndTime as time)";

                dsval = access.ExecuteSqlQuery(strQuerry, sqlparams, CommandType.Text, false);
                //Result = Convert.ToInt32(dsval.Tables[0].Rows[0][0].ToString());
                Result = dsval.Tables[0];


                if (Result != null && Result.Rows.Count > 0)
                {
                    result = true;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }

            return result;
        }

        public bool GetTodayAttendaceCheckOutStatus(int PersonId, Face face)
        {
            DataTable Result = new DataTable();
            bool result = false;
            try
            {
                DataSet dsval = null;
                SqlParameter[] sqlparams = new SqlParameter[3];
                sqlparams[0] = new SqlParameter("@PersonId", PersonId);
                sqlparams[1] = new SqlParameter("@LocationId", face.LocationId);
                sqlparams[2] = new SqlParameter("@ProgramId", face.ProgramId);

                string strQuerry = string.Empty;

                strQuerry = @"SELECT * FROM Attendance  A
                            INNER JOIN (
                                            SELECT TOP 1 * FROM Attendance A
                                            WHERE A.AttendanceType='CheckIn' 
                                            AND  A.PersonId =@PersonId AND A.LocationId =ISNULL(@LocationId,A.LocationId)
                                            AND A.ProgramId =ISNULL(@ProgramId,A.ProgramId)
                                            ORDER BY A.AttendanceDate Desc
                                        ) CheckIn ON CheckIn.ProgramId=A.ProgramId AND CheckIn.PersonId=A.PersonId AND CheckIn.LocationId=A.LocationId
                            WHERE A.AttendanceId > CheckIn.AttendanceId";

                dsval = access.ExecuteSqlQuery(strQuerry, sqlparams, CommandType.Text, false);
                //Result = Convert.ToInt32(dsval.Tables[0].Rows[0][0].ToString());
                Result = dsval.Tables[0];


                if (Result != null && Result.Rows.Count > 0)
                {
                    result = true;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }

            return result;
        }

        public List<PersonsList> GetPersonsList(PersonsListViewModel model,int start = 1, int pageSize = 10, string sortColumn = "PersonName", int sortOrder = 1)
        {
            var viewModel = new List<PersonsList>();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[5];
                sqlParameter[0] = new SqlParameter("@SortColumn", sortColumn);
                sqlParameter[1] = new SqlParameter("@SortOrder", sortOrder);
                sqlParameter[2] = new SqlParameter("@PageSize", pageSize);
                sqlParameter[3] = new SqlParameter("@Start", start);
                if(model.PersonName != null && !string.IsNullOrEmpty(model.PersonName))
                {
                    sqlParameter[4] = new SqlParameter("@PersonName", model.PersonName.Trim());
                }
                else
                {
                    sqlParameter[4] = new SqlParameter("@PersonName", DBNull.Value);
                }


                var dt = access.GetData("spPersonsData", CommandType.StoredProcedure, sqlParameter.ToArray());

                if (dt != null && dt.Rows.Count > 0)
                {
                    viewModel = commonService.ConvertToList<PersonsList>(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
            return viewModel;
        }

        public PersonsViewModel GetPersonsData(int PersonId)
        {
            PersonsViewModel model = new PersonsViewModel();
            Persons person = _context.Persons.Where(s => s.PersonId == PersonId).FirstOrDefault();
            if (person != null)
            {
                model.PersonId = person.PersonId;
                model.FirstName = person.FirstName;
                model.LastName = person.LastName;
                model.MiddleInitial = person.MiddleInitial;
                model.City = person.City;
                model.Zip = person.Zip;
                model.State = person.State;
                if (person.DOB != null)
                {
                    model.DOB = person.DOB.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                }
                else
                {
                    model.DOB = "";
                }
                model.Address1 = person.Address1;
                model.Address2 = person.Address2;
                model.IsActive = person.IsActive;
                model.PersonId1 = person.PersonId1;
                model.PersonId2 = person.PersonId2;
                model.PersonId3 = person.PersonId3;
                model.PersonDetailsList = GetPersonProgramCalendarList(model.PersonId);
                PersonTrainDetails personTrain = GetPersonTrainDetailsByPersonID(model.PersonId);
                if (personTrain != null)
                {
                    model.LastTrainedOn = personTrain.ModifiedDate;
                    model.NoOfTrainings = personTrain.NoOfTrainings;
                }
            }

            return model;
        }

        public PersonTrainDetails GetPersonTrainDetailsByPersonID(int PersonId)
        {
            return _context.PersonTrainDetails.Where(s => s.PersonId == PersonId).FirstOrDefault();
        }

        public List<PersonDetailsListViewModel> GetPersonProgramCalendarList(int? PersonId)
        {
            try
            {
                var personDetailsListViewModel = new List<PersonDetailsListViewModel>();

                personDetailsListViewModel = (from pd in _context.PersonsDetails
                                              join l in _context.Locations on pd.LocationId equals l.LocationId
                                              join p in _context.Programs on pd.ProgramId equals p.ProgramId
                                              join cs in _context.CalendarSetting on pd.CalendarId equals cs.CalendarId
                                              where pd.PersonId == PersonId.Value && (l.IsActive == true && p.IsActive ==true && cs.IsActive ==true)
                                              orderby pd.PersonId descending
                                              select new PersonDetailsListViewModel
                                              {
                                                  PersonsDetailsId = pd.PersonsDetailsId,
                                                  PersonId = pd.PersonId,
                                                  ProgramId = p.ProgramId,
                                                  ProgramName = p.ProgramName,
                                                  LocationId = l.LocationId,
                                                  LocationName = l.LocationName,
                                                  CalendarId = cs.CalendarId,
                                                  CalendarName = cs.CalendarName,
                                                  StartDate = pd.StartDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                                                  EndDate = pd.EndDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                                              }).Distinct().ToList();


                return personDetailsListViewModel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        public List<AttendanceList> GetAttendanceList(AttendanceListViewModel model, int? pageStart, int? pageEnd, string SortColumn, int? sortorder)
        {
            DataTable dt = new DataTable();
            List<AttendanceList> items = new List<AttendanceList>();
            try
            {
                DataSet dsMyDataSet = new DataSet();
                SqlParameter[] sqlParameters1 = new SqlParameter[6];


                if (model.LocationId != null)
                {
                    sqlParameters1[0] = new SqlParameter("@LocationId", model.LocationId);
                }
                else
                {
                    sqlParameters1[0] = new SqlParameter("@LocationId", DBNull.Value);
                }

                if (model.ReportDate != null)
                {
                    sqlParameters1[1] = new SqlParameter("@Date", Convert.ToDateTime(model.ReportDate));
                }
                else
                {
                    sqlParameters1[1] = new SqlParameter("@Date", DBNull.Value);
                }

                sqlParameters1[2] = new SqlParameter("@SortColumn", SortColumn);
                sqlParameters1[3] = new SqlParameter("@SortOrder", sortorder);
                sqlParameters1[4] = new SqlParameter("@PageSize", pageEnd);
                sqlParameters1[5] = new SqlParameter("@Start", pageStart);

                if (model.ReportType == "Attendance Detail Report")
                {
                    dsMyDataSet = access.ExecuteSqlQuery("spAttendanceDetailsReportData", sqlParameters1, CommandType.StoredProcedure, true);
                }
                else
                {
                    dsMyDataSet = access.ExecuteSqlQuery("spAttendanceReportData", sqlParameters1, CommandType.StoredProcedure, true);
                }

                if (dsMyDataSet != null && dsMyDataSet.Tables.Count > 0)
                {
                    dt = dsMyDataSet.Tables[0];
                }

                DateTime? dateTime = null;

                if (dt != null && dt.Rows.Count > 0)
                {
                    if (model.ReportType == "Attendance Detail Report")
                    {
                        items = ConvertDatatabletoConsolidateList(dt);
                    }
                    else
                    {
                        items = commonService.ConvertToList<AttendanceList>(dt);
                    }
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

        public DataTable GetAttendanceListDT(AttendanceListViewModel model, int? pageStart, int? pageEnd, string SortColumn, int? sortorder)
        {
            DataTable dt = new DataTable();
            List<AttendanceList> items = new List<AttendanceList>();
            try
            {
                DataSet dsMyDataSet = new DataSet();
                SqlParameter[] sqlParameters1 = new SqlParameter[6];


                if (model.LocationId != null)
                {
                    sqlParameters1[0] = new SqlParameter("@LocationId", model.LocationId);
                }
                else
                {
                    sqlParameters1[0] = new SqlParameter("@LocationId", DBNull.Value);
                }

                if (model.ReportDate != null)
                {
                    sqlParameters1[1] = new SqlParameter("@Date", Convert.ToDateTime(model.ReportDate));
                }
                else
                {
                    sqlParameters1[1] = new SqlParameter("@Date", DBNull.Value);
                }

                sqlParameters1[2] = new SqlParameter("@SortColumn", SortColumn);
                sqlParameters1[3] = new SqlParameter("@SortOrder", sortorder);
                sqlParameters1[4] = new SqlParameter("@PageSize", pageEnd);
                sqlParameters1[5] = new SqlParameter("@Start", pageStart);

                if (model.ReportType == "Attendance Detail Report")
                {
                    dsMyDataSet = access.ExecuteSqlQuery("spAttendanceDetailsReportData", sqlParameters1, CommandType.StoredProcedure, true);
                }
                else
                {
                    dsMyDataSet = access.ExecuteSqlQuery("spAttendanceReportData", sqlParameters1, CommandType.StoredProcedure, true);
                }

                if (dsMyDataSet != null && dsMyDataSet.Tables.Count > 0)
                {
                    dt = dsMyDataSet.Tables[0];
                }                

                //DateTime? dateTime = null;

                //if (dt != null && dt.Rows.Count > 0)
                //{
                //    if (model.ReportType == "Attendance Detail Report")
                //    {
                //        items = ConvertDatatabletoConsolidateList(dt);
                //    }
                //    else
                //    {
                //        items = commonService.ConvertToList<AttendanceList>(dt);
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ////log.Error(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
            return dt;
        }

        public List<AttendanceList> ConvertDatatabletoConsolidateList(DataTable dt)
        {
            List<AttendanceList> result = new List<AttendanceList>();

            for (var i = 4; i < dt.Columns.Count; i++)
            {
                dt.Columns[i].ColumnName = "Col" + dt.Columns[i].ColumnName;
            }

            result = commonService.ConvertToList<AttendanceList>(dt);
            return result;
        }


        public List<SelectListItem> GetLocationsList(int? LocationId,int? LoginUserId = 1,bool IsAdmin =true)
        {
            try
            {

                var locationsLists = new List<SelectListItem>();

                if (IsAdmin)
                {
                    locationsLists = (from l in _context.Locations
                                      orderby l.LocationName
                                      where l.IsActive == true
                                      select new SelectListItem
                                      {
                                          Value = l.LocationId.ToString(),
                                          Text = l.LocationName,
                                      }).Distinct().ToList();
                }
                else
                {
                    locationsLists = (from l in _context.Locations
                                      join ul in _context.UserLocationDetails on l.LocationId equals ul.LocationId
                                      orderby l.LocationName
                                      where l.IsActive == true && ul.UserId == LoginUserId
                                      select new SelectListItem
                                      {
                                          Value = l.LocationId.ToString(),
                                          Text = l.LocationName,
                                      }).Distinct().ToList();
                }

                return locationsLists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        public List<SelectListItem> GetProgramsList(int? LocationId, int? LoginUserId = 1)
        {
            try
            {

                var programLists = new List<SelectListItem>();

                if (LocationId.HasValue)
                {
                    programLists = (from p in _context.Programs
                                    join l in _context.LocationProgramDetails on p.ProgramId equals l.ProgramId
                                    join ul in _context.UserLocationDetails on l.LocationId equals ul.LocationId
                                    orderby p.ProgramName
                                    where l.LocationId == LocationId && ul.UserId ==LoginUserId
                                    where p.IsActive == true
                                    select new SelectListItem
                                    {
                                        Value = p.ProgramId.ToString(),
                                        Text = p.ProgramName,
                                    }).Distinct().ToList();
                }
				else
				{
					programLists = (from p in _context.Programs
									join lp in _context.LocationProgramDetails on p.ProgramId equals lp.ProgramId
									join l in _context.Locations on lp.LocationId equals l.LocationId
                                    join ul in _context.UserLocationDetails on l.LocationId equals ul.LocationId
                                    orderby p.ProgramName
									where l.IsActive == true && ul.UserId == LoginUserId
                                    where p.IsActive == true
									select new SelectListItem
									{
										Value = p.ProgramId.ToString(),
										Text = p.ProgramName,
									}).Distinct().ToList();
				}
				return programLists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        public List<SelectListItem> GetProgramsList(int? LocationId)
        {
            try
            {

                var programLists = new List<SelectListItem>();

                if (LocationId.HasValue)
                {
                    programLists = (from p in _context.Programs
                                    join l in _context.LocationProgramDetails on p.ProgramId equals l.ProgramId
                                    orderby p.ProgramName
                                    where l.LocationId == LocationId
                                    where p.IsActive == true
                                    select new SelectListItem
                                    {
                                        Value = p.ProgramId.ToString(),
                                        Text = p.ProgramName,
                                    }).Distinct().ToList();
                }
                else
                {
                    programLists = (from p in _context.Programs
                                    join lp in _context.LocationProgramDetails on p.ProgramId equals lp.ProgramId
                                    join l in _context.Locations on lp.LocationId equals l.LocationId
                                    orderby p.ProgramName
                                    where l.IsActive == true
                                    where p.IsActive == true
                                    select new SelectListItem
                                    {
                                        Value = p.ProgramId.ToString(),
                                        Text = p.ProgramName,
                                    }).Distinct().ToList();
                }
                return programLists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        public List<SelectListItem> GetCalendarList(int? ProgramId)
        {
            try
            {
                var calendarLists = new List<SelectListItem>();
                if (ProgramId.HasValue)
                {
                    calendarLists = (from cs in _context.CalendarSetting
                                     join pd in _context.ProgramCalendarDetails on cs.CalendarId equals pd.CalendarId
                                     where pd.ProgramId == ProgramId && cs.IsActive == true
                                     orderby cs.CalendarName
                                     select new SelectListItem
                                     {
                                         Value = cs.CalendarId.ToString(),
                                         Text = cs.CalendarName,
                                     }).Distinct().ToList();
                }
                else
                {
                    calendarLists = (from c in _context.CalendarSetting
                                     orderby c.CalendarName
                                     where c.IsActive == true
                                     select new SelectListItem
                                     {
                                         Value = c.CalendarId.ToString(),
                                         Text = c.CalendarName,
                                     }).Distinct().ToList();
                }

                return calendarLists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        public bool GetPersonAttendaceStatus(int? PersonsDetailsId)
        {
            DataTable Result = new DataTable();
            bool result = false;
            try
            {
                DataSet dsval = null;
                SqlParameter[] sqlparams = new SqlParameter[1];
                if (PersonsDetailsId.HasValue)
                {
                    sqlparams[0] = new SqlParameter("@PersonsDetailsId", PersonsDetailsId);
                }
                else
                {
                    sqlparams[0] = new SqlParameter("@PersonsDetailsId", DBNull.Value);
                }

                string strQuerry = string.Empty;

                strQuerry = @"SELECT PersonsDetailsId FROM Attendance a
                            INNER JOIN PersonsDetails pd  ON pd.PersonId = a.PersonId AND pd.ProgramId =a.ProgramId AND pd.LocationId = a.LocationId
                            AND a.AttendanceDate BETWEEN pd.StartDate AND pd.EndDate
                            WHERE pd.PersonsDetailsId = @PersonsDetailsId";

                dsval = access.ExecuteSqlQuery(strQuerry, sqlparams, CommandType.Text, false);
                //Result = Convert.ToInt32(dsval.Tables[0].Rows[0][0].ToString());
                Result = dsval.Tables[0];


                if (Result != null && Result.Rows.Count > 0)
                {
                    result = true;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }

            return result;
        }


        public bool GetCalendarDuplicateStatus(List<CalendarDuplicateCheckModel> checkModel)
        {
            DataTable Result = new DataTable();
            bool result = false;
            try
            {
                if (checkModel.Count > 0)
                {
                    DataTable dt1 = new DataTable();
                    SqlParameter[] sqlParameter = new SqlParameter[4];
                    sqlParameter[0] = new SqlParameter("@CalendarId", checkModel[0].CalendarId);
                    sqlParameter[1] = new SqlParameter("@StartDate", checkModel[0].StartDate);
                    sqlParameter[2] = new SqlParameter("@EndDate", checkModel[0].EndDate);
                    dt1.Columns.Add("RowId");
                    dt1.Columns.Add("CalendarId");
                    dt1.Columns.Add("StartDate");
                    dt1.Columns.Add("EndDate");
                    var count = 1;
                    foreach (CalendarDuplicateCheckModel model in checkModel)
                    {
                        DataRow dr = dt1.NewRow();
                        dr["CalendarId"] = model.RowCalendarId;
                        dr["StartDate"] = model.RowStartDate;
                        dr["EndDate"] = model.RowEndDate;
                        dr["RowId"] = count;
                        dt1.Rows.Add(dr);
                        count++;
                    }

                    sqlParameter[3] = new SqlParameter("@CalendarDuplicateCheck", SqlDbType.Structured)
                    {
                        TypeName = "CalendarDuplicateCheck",
                        Value = dt1
                    };

                    var dt = access.GetData("spGetDuplicateCalendarStatus", CommandType.StoredProcedure, sqlParameter.ToArray());

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        result = true;
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }

            return result;
        }

    }
}
