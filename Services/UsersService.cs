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
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FaceApp.Services
{
    public class UsersService : IDisposable
    {
        private readonly FaceAppDBContext _context;
        Authentication authenticate = new Authentication();
        DbAccess access = null;
        CommonService commonService = null;
        public UsersService(FaceAppDBContext context)
        {
            commonService = new CommonService(context);
            access = new DbAccess();
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }


        public void CreateUser(UsersViewModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Users users = new Users();
                    users.FirstName = model.FirstName != null ? model.FirstName.Trim() : model.FirstName;
                    users.LastName = model.LastName != null ? model.LastName.Trim() : model.LastName;
                    users.MiddleInitial = model.MiddleInitial != null ? model.MiddleInitial.Trim() : model.MiddleInitial;
                    users.Email = model.Email != null ? model.Email.Trim() : model.Email;
                    users.SecurityGroup = model.SecurityGroup != null ? model.SecurityGroup.Trim() : model.SecurityGroup;
                    users.IsActive = model.IsActive;
                    string salt = authenticate.GenerateSaltValue();
                    string Passhash = authenticate.Hash(model.Password.Trim(), salt);
                    users.Salt = salt;
                    users.Password = Passhash;
                    users.UserEmpId = !string.IsNullOrEmpty(model.UserEmpId)?model.UserEmpId.Trim(): model.UserEmpId;
                    users.CreatedDate = DateTime.Now;
                    users.CreatedBy = model.LoginUserId; // need to modify login Id
                    _context.Users.Add(users);
                    _context.SaveChanges();
                    model.UserId = users.UserId;
                    if (model.UserId > 0)
                    {
                        if (model.UserLocationsList != null && model.UserLocationsList.Count > 0)
                        {
                            SaveUserLocations(model);
                        }
                        else
                        {
                            RemoveUserLocations(model.UserId);
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
        }
        public void UpdateUser(UsersViewModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    Users users = _context.Users.Where(u => u.UserId == model.UserId).FirstOrDefault();
                    users.UserEmpId = model.UserEmpId !=null?model.UserEmpId.Trim(): model.UserEmpId;
                    users.FirstName = model.FirstName.Trim();
                    users.LastName = model.LastName.Trim();
                    users.MiddleInitial = model.MiddleInitial != null ? model.MiddleInitial.Trim() : model.MiddleInitial;
                    users.Email = model.Email != null ? model.Email.Trim() : model.Email;
                    users.SecurityGroup = model.SecurityGroup != null ? model.SecurityGroup.Trim() : model.SecurityGroup;
                    if (model.IsPasswordChanged)
                    {
                        string salt = authenticate.GenerateSaltValue();
                        string Passhash = authenticate.Hash(model.Password.Trim(), salt);
                        users.Salt = salt;
                        users.Password = Passhash;
                    }
                    users.IsActive = model.IsActive;
                    users.ModifiedDate = DateTime.Now;
                    users.ModifiedBy = model.LoginUserId; // need to modify login Id
                    _context.Entry(users).State = EntityState.Modified;
                    _context.SaveChanges();
                    if (model.UserId > 0)
                    {
                        if (model.UserLocationsList != null && model.UserLocationsList.Count > 0)
                        {
                            SaveUserLocations(model);
                        }
                        else
                        {
                            RemoveUserLocations(model.UserId);
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
        }

        //Save tempPassword
        public void UpdateUserPass(ForgotPasswordModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Users users = _context.Users.Where(u => u.UserId == model.UserId).FirstOrDefault();
                    users.Salt = model.Salt;
                    users.Password = model.Password;
                    users.ModifiedDate = DateTime.Now;
                    users.ModifiedBy = model.LoginUserId; // need to modify login Id
                    users.IsForgotPassword = true;
                    _context.Entry(users).State = EntityState.Modified;
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

        //ChangePassword
        public void UpdateUserPass(ChangePasswordModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Users users = _context.Users.Where(u => u.UserId == model.UserId).FirstOrDefault();
                    users.Salt = model.Salt;
                    users.Password = model.Password;
                    users.ModifiedDate = DateTime.Now;
                    users.ModifiedBy = model.LoginUserId; // need to modify login Id
                    users.IsForgotPassword = false;
                    _context.Entry(users).State = EntityState.Modified;
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

        private bool SaveUserLocations(UsersViewModel model)
        {
            try
            {
                bool retVal = true;
                if (model.UserLocationsList != null && model.UserLocationsList.Count > 0)
                {
                    UserLocationDetails userLocationDetails = new UserLocationDetails();
                    List<ProgramCalendarDetails> ListprogramCalendarDetails = new List<ProgramCalendarDetails>();
                    bool IsinsertUpdate = false;
                    int? UserId = 0;
                    List<int> ids = new List<int>();
                    foreach (UserLocationsListViewModel userLocationsListViewModel in model.UserLocationsList)
                    {
                        UserId = userLocationsListViewModel.UserId;
                        IsinsertUpdate = false;
                        userLocationDetails = new UserLocationDetails();
                        if (userLocationsListViewModel.UserLocationsDetailsId > 0)
                        {
                            userLocationDetails = _context.UserLocationDetails.FirstOrDefault(x => x.UserLocationDetailsId == userLocationsListViewModel.UserLocationsDetailsId);
                            if (userLocationDetails != null)
                            {
                                userLocationDetails.ModifiedBy = model.LoginUserId;
                                userLocationDetails.ModifiedDate = System.DateTime.Now;
                                IsinsertUpdate = true;
                            }
                        }
                        else
                        {

                            userLocationDetails = new UserLocationDetails();
                            userLocationDetails.UserId = model.UserId;
                            userLocationDetails.LocationId = userLocationsListViewModel.LocationId;
                            userLocationDetails.CreatedBy = model.LoginUserId;
                            userLocationDetails.CreatedDate = DateTime.Now;
                            _context.UserLocationDetails.Add(userLocationDetails);
                            IsinsertUpdate = true;
                        }
                        if (IsinsertUpdate)
                        {
                            _context.SaveChanges();
                            ids.Add(userLocationDetails.LocationId);
                        }
                    }

                    var listToRemove = _context.UserLocationDetails
                                .Where(w => w.UserId == model.UserId && !ids.Contains(w.LocationId)).ToList();

                    if (listToRemove != null && listToRemove.Count > 0)
                    {
                        _context.UserLocationDetails.RemoveRange(listToRemove);
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

        private void RemoveUserLocations(int UserId)
        {
            List<UserLocationDetails> userLocationDetails = _context.UserLocationDetails.Where(w => w.UserId == UserId).ToList();
            if (userLocationDetails != null && userLocationDetails.Count > 0)
            {
                _context.UserLocationDetails.RemoveRange(userLocationDetails);
                _context.SaveChanges();
            }
        }


        public Users GetUserById(int Id)
        {
            Users user = _context.Users.Where(s => s.UserId == Id).FirstOrDefault();
            return user;
        }
        public Users GetUserByEmail(String Email)
        {
            Users user = _context.Users.Where(s => s.Email == Email && s.IsActive ==true).FirstOrDefault();
            return user;
        }
        public List<UsersList> GetUsersList(UsersListViewModel model,int start = 1, int pageSize = 10, string sortColumn = "UserName", int sortOrder = 1)
        {
            var viewModel = new List<UsersList>();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[6];
                sqlParameter[0] = new SqlParameter("@SortColumn", sortColumn);
                sqlParameter[1] = new SqlParameter("@SortOrder", sortOrder);
                sqlParameter[2] = new SqlParameter("@PageSize", pageSize);
                sqlParameter[3] = new SqlParameter("@Start", start);
                if (model.UserName != null && !string.IsNullOrEmpty(model.UserName))
                {
                    sqlParameter[4] = new SqlParameter("@UserName", model.UserName.Trim());
                }
                else
                {
                    sqlParameter[4] = new SqlParameter("@UserName", DBNull.Value);
                }

                if (model.Email != null && !string.IsNullOrEmpty(model.Email))
                {
                    sqlParameter[5] = new SqlParameter("@Email", model.Email.Trim());
                }
                else
                {
                    sqlParameter[5] = new SqlParameter("@Email", DBNull.Value);
                }

                var dt = access.GetData("spUsersData", CommandType.StoredProcedure, sqlParameter.ToArray());

                if (dt != null && dt.Rows.Count > 0)
                {
                    viewModel = commonService.ConvertToList<UsersList>(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
            return viewModel;
        }

        public UsersViewModel GetUsersData(int UserId)
        {
            UsersViewModel model = new UsersViewModel();
            Users user = _context.Users.Where(s => s.UserId == UserId).FirstOrDefault();
            if (user != null)
            {
                model.UserId = user.UserId;
                model.UserEmpId = user.UserEmpId;
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.MiddleInitial = user.MiddleInitial;
                model.SecurityGroup = user.SecurityGroup;
                model.Email = user.Email;
                model.IsActive = user.IsActive;
                model.UserLocationsList = GetUserLocationList(model.UserId);
            }

            return model;
        }
        public List<UserLocationsListViewModel> GetUserLocationList(int? UserId)
        {
            try
            {

                var userLocationsListViewModel = new List<UserLocationsListViewModel>();

                userLocationsListViewModel = (from ul in _context.UserLocationDetails
                                                join l in _context.Locations on ul.LocationId equals l.LocationId
                                                where ul.UserId == UserId.Value && l.IsActive ==true
                                                orderby ul.UserLocationDetailsId descending
                                                select new UserLocationsListViewModel
                                                {
                                                    UserLocationsDetailsId = ul.UserLocationDetailsId,
                                                    UserId = ul.UserId,
                                                    LocationId = ul.LocationId,
                                                    LocationName = l.LocationName,
                                                }).Distinct().ToList();


                return userLocationsListViewModel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        public bool ValidateEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public bool ValidatePassword(string passWord)
        {
            int validConditions = 0;
            foreach (char c in passWord)
            {
                if (c >= 'a' && c <= 'z')
                {
                    validConditions++;
                    break;
                }
            }
            foreach (char c in passWord)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    validConditions++;
                    break;
                }
            }
            if (validConditions == 0) return false;
            foreach (char c in passWord)
            {
                if (c >= '0' && c <= '9')
                {
                    validConditions++;
                    break;
                }
            }
            if (validConditions == 1) return false;
            if (validConditions == 2)
            {
                char[] special = { '@', '#', '$', '%', '^', '&', '+', '=' }; // or whatever    
                if (passWord.IndexOfAny(special) == -1) return false;
            }
            return true;
        }

        public List<SelectListItem> GetLocationsList()
        {
            try
            {

                var programLists = new List<SelectListItem>();

                programLists = (from l in _context.Locations
                                orderby l.LocationName
                                where l.IsActive == true
                                select new SelectListItem
                                {
                                    Value = l.LocationId.ToString(),
                                    Text = l.LocationName,
                                }).Distinct().ToList();


                return programLists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }


        public bool GetLocationScheduleStatus(int? UserLocationDetailsId)
        {
            bool LocationScheduleStatus = false;
            try
            {

                var PersonsDetails = new List<PersonsDetails>();

                PersonsDetails = (from c in _context.PersonsDetails
                                  join cs in _context.UserLocationDetails on new { c.LocationId} equals new { cs.LocationId}
                                  orderby c.PersonId
                                  where cs.UserLocationDetailsId == UserLocationDetailsId
                                  select c).Distinct().ToList();


                if (PersonsDetails != null && PersonsDetails.Count > 0)
                {
                    LocationScheduleStatus = true;
                }
                return LocationScheduleStatus;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

    }
}
