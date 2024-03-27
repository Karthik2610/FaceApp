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
    public class LocationService : IDisposable
    {
        private readonly FaceAppDBContext _context;
        Authentication authenticate = new Authentication();
        DbAccess access = null;
        CommonService commonService = null;
        public LocationService(FaceAppDBContext context)
        {
            commonService = new CommonService(context);
            access = new DbAccess();
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public int CreateLocation(LocationViewModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Locations locations = new Locations();
                    locations.LocationName = model.Name.Trim();
                    locations.IsActive = model.IsActive;
                    locations.Address1 = model.Address1 != null ? model.Address1.Trim() : model.Address1;
                    locations.Address2 = model.Address2 != null ? model.Address2.Trim() : model.Address2;
                    locations.City = model.City != null ? model.City.Trim() : model.City;
                    locations.State = model.State != null ? model.State.Trim() : model.State;
                    locations.Zip = model.Zip != null ? model.Zip.Trim() : model.Zip;
                    locations.AccountingCode = model.AccountingCode != null ? model.AccountingCode.Trim() : model.AccountingCode;
                    locations.LocationId1 = model.LocationId1 != null ? model.LocationId1.Trim() : model.LocationId1;
                    locations.LocationId2 = model.LocationId2 != null ? model.LocationId2.Trim() : model.LocationId2;
                    locations.LocationId3 = model.LocationId3 != null ? model.LocationId3.Trim() : model.LocationId3;
                    locations.Latitude = model.Latitude != null ? model.Latitude.Trim() : model.Latitude;
                    locations.Longitude = model.Longitude != null ? model.Longitude.Trim() : model.Longitude;
                    locations.CreatedDate = DateTime.Now;
                    locations.CreatedBy = model.LoginUserId.Value;
                    _context.Locations.Add(locations);
                    _context.SaveChanges();
                    model.LocationId = locations.LocationId;
                    if (model.LocationId > 0)
                    {
                        if (model.LocationProgramList != null && model.LocationProgramList.Count > 0)
                        {
                            SaveLocationPrograms(model);
                        }
                        else
                        {
                            RemoveLocationPrograms(model.LocationId);
                        }
                    }

                    transaction.Commit();

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                }
            }
            return model.LocationId;
        }

        public int UpdateLocation(LocationViewModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    Locations locations = _context.Locations.Where(u => u.LocationId == model.LocationId).FirstOrDefault();
                    locations.LocationName = model.Name.Trim();
                    locations.IsActive = model.IsActive;
                    locations.Address1 = model.Address1 != null ? model.Address1.Trim() : model.Address1;
                    locations.Address2 = model.Address2 != null ? model.Address2.Trim() : model.Address2;
                    locations.City = model.City != null ? model.City.Trim() : model.City;
                    locations.State = model.State != null ? model.State.Trim() : model.State;
                    locations.Zip = model.Zip != null ? model.Zip.Trim() : model.Zip;
                    locations.AccountingCode = model.AccountingCode != null ? model.AccountingCode.Trim() : model.AccountingCode;
                    locations.LocationId1 = model.LocationId1 != null ? model.LocationId1.Trim() : model.LocationId1;
                    locations.LocationId2 = model.LocationId2 != null ? model.LocationId2.Trim() : model.LocationId2;
                    locations.LocationId3 = model.LocationId3 != null ? model.LocationId3.Trim() : model.LocationId3;
                    locations.Latitude = model.Latitude != null ? model.Latitude.Trim() : model.Latitude;
                    locations.Longitude = model.Longitude != null ? model.Longitude.Trim() : model.Longitude;
                    locations.ModifiedDate = DateTime.Now;
                    locations.ModifiedBy = model.LoginUserId;
                    _context.Entry(locations).State = EntityState.Modified;
                    _context.SaveChanges();

                    if (model.LocationId > 0)
                    {
                        if (model.LocationProgramList != null && model.LocationProgramList.Count > 0)
                        {
                            SaveLocationPrograms(model);
                        }
                        else
                        {
                            RemoveLocationPrograms(model.LocationId);
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
            return model.LocationId;
        }

        private bool SaveLocationPrograms(LocationViewModel model)
        {
            try
            {
                bool retVal = true;
                if (model.LocationProgramList != null && model.LocationProgramList.Count > 0)
                {
                    LocationProgramDetails locationProgramDetails = new LocationProgramDetails();
                    List<ProgramCalendarDetails> ListprogramCalendarDetails = new List<ProgramCalendarDetails>();
                    bool IsinsertUpdate = false;
                    int? LocationId = 0;
                    List<int> ids = new List<int>();
                    foreach (LocationProgramListViewModel locationProgramListViewModel in model.LocationProgramList)
                    {
                        LocationId = locationProgramListViewModel.LocationId;
                        IsinsertUpdate = false;
                        locationProgramDetails = new LocationProgramDetails();
                        if (locationProgramListViewModel.LocationProgramDetailsId > 0)
                        {
                            locationProgramDetails = _context.LocationProgramDetails.FirstOrDefault(x => x.LocationProgramDetailsId == locationProgramListViewModel.LocationProgramDetailsId);
                            if (locationProgramDetails != null)
                            {
                                locationProgramDetails.ModifiedBy = model.LoginUserId.Value;
                                locationProgramDetails.ModifiedDate = System.DateTime.Now;
                                IsinsertUpdate = true;
                            }
                        }
                        else
                        {

                                locationProgramDetails = new LocationProgramDetails();
                                locationProgramDetails.LocationId = model.LocationId;
                                locationProgramDetails.ProgramId = locationProgramListViewModel.ProgramId;
                                locationProgramDetails.CreatedBy = model.LoginUserId.Value;
                                locationProgramDetails.CreatedDate = DateTime.Now;
                                _context.LocationProgramDetails.Add(locationProgramDetails);
                                IsinsertUpdate = true;
                        }
                        if (IsinsertUpdate)
                        {
                            _context.SaveChanges();
                            ids.Add(locationProgramDetails.ProgramId);
                        }
                    }

                    var listToRemove = _context.LocationProgramDetails
                                .Where(w => w.LocationId == model.LocationId && !ids.Contains(w.ProgramId)).ToList();

                    if (listToRemove != null && listToRemove.Count > 0)
                    {
                        _context.LocationProgramDetails.RemoveRange(listToRemove);
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

        private void RemoveLocationPrograms(int LocationId)
        {
            List<LocationProgramDetails> locationProgramDetails = _context.LocationProgramDetails.Where(w => w.LocationId == LocationId).ToList();
            if (locationProgramDetails != null && locationProgramDetails.Count > 0)
            {
                _context.LocationProgramDetails.RemoveRange(locationProgramDetails);
                _context.SaveChanges();
            }
        }

        public Locations GetLocationByName(LocationViewModel model)
        {
            Locations locations = new Locations();
            if (model.LocationId > 0)
            {
                locations = _context.Locations.Where(l => l.LocationName == model.Name.Trim() && l.LocationId != model.LocationId).FirstOrDefault();
            }
            else
            {
                locations = _context.Locations.Where(l => l.LocationName == model.Name.Trim()).FirstOrDefault();
            }
            return locations;
        }

        public List<LocationList> GetLocationList(LocationListViewModel model, int start = 1, int pageSize = 10, string sortColumn = "LocationName", int sortOrder = 1)
        {
            var viewModel = new List<LocationList>();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[5];
                sqlParameter[0] = new SqlParameter("@SortColumn", sortColumn);
                sqlParameter[1] = new SqlParameter("@SortOrder", sortOrder);
                sqlParameter[2] = new SqlParameter("@PageSize", pageSize);
                sqlParameter[3] = new SqlParameter("@Start", start);
                if (model.LocationName != null && !string.IsNullOrEmpty(model.LocationName))
                {
                    sqlParameter[4] = new SqlParameter("@LocationName", model.LocationName.Trim());
                }
                else
                {
                    sqlParameter[4] = new SqlParameter("@LocationName", DBNull.Value);
                }

                var dt = access.GetData("spLocationsData", CommandType.StoredProcedure, sqlParameter.ToArray());

                if (dt != null && dt.Rows.Count > 0)
                {
                    viewModel = commonService.ConvertToList<LocationList>(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
            return viewModel;
        }

        public LocationViewModel GetLocationData(int LocationId)
        {
            LocationViewModel model = new LocationViewModel();
            Locations locations = _context.Locations.Where(p => p.LocationId == LocationId).FirstOrDefault();
            if (locations != null)
            {
                model.LocationId = locations.LocationId;
                model.Name = locations.LocationName != null ? locations.LocationName : "";
                model.Address1 = locations.Address1 != null ? locations.Address1  : "";
                model.Address2 = locations.Address2 != null ? locations.Address2 : "";
                model.IsActive = locations.IsActive;
                model.City = locations.City != null ? locations.City : "";
                model.State = locations.State != null ? locations.State : "";
                model.Zip = locations.Zip != null ? locations.Zip : "";
                model.AccountingCode = locations.AccountingCode != null ? locations.AccountingCode : "";
                model.LocationId1 = locations.LocationId1 != null ? locations.LocationId1 : "";
                model.LocationId2 = locations.LocationId2 != null ? locations.LocationId2 : "";
                model.LocationId3 = locations.LocationId3 != null ? locations.LocationId3 : "";
                model.Latitude = locations.Latitude != null ? locations.Latitude : "";
                model.Longitude = locations.Longitude != null ? locations.Longitude : "";
                model.LocationProgramList = GetLocationProgramList(model.LocationId);
            }
            

            return model;
        }

        public List<LocationProgramListViewModel> GetLocationProgramList(int? LocationId)
        {
            try
            {

                var locationProgramListViewModel = new List<LocationProgramListViewModel>();

                locationProgramListViewModel = (from lp in _context.LocationProgramDetails
                                                join p in _context.Programs on lp.ProgramId equals p.ProgramId
                                                where lp.LocationId == LocationId && p.IsActive == true
                                                orderby lp.LocationProgramDetailsId descending
                                                 select new LocationProgramListViewModel
                                                 {
                                                     LocationProgramDetailsId = lp.LocationProgramDetailsId,
                                                     ProgramId = lp.ProgramId,
                                                     LocationId = lp.LocationId,
                                                     ProgramName = p.ProgramName,
                                                 }).Distinct().ToList();


                return locationProgramListViewModel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

        public List<SelectListItem> GetProgramList()
        {
            try
            {

                var programLists = new List<SelectListItem>();

                programLists = (from p in _context.Programs
                                orderby p.ProgramName
                                where p.IsActive == true
                                select new SelectListItem
                                {
                                    Value = p.ProgramId.ToString(),
                                    Text = p.ProgramName,
                                }).Distinct().ToList();


                return programLists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }
        
        public bool GetLocationProgramScheduleStatus(int? LocationProgramDetailsId)
        {
            bool LocationProgramScheduled = false;
            try
            {

                var PersonsDetails = new List<PersonsDetails>();

                PersonsDetails = (from c in _context.PersonsDetails
                                  join cs in _context.LocationProgramDetails on new { c.LocationId, c.ProgramId} equals new { cs.LocationId, cs.ProgramId }
                                  join p in _context.Persons on new { c.PersonId} equals new { p.PersonId}
                                  orderby c.PersonId
                                  where cs.LocationProgramDetailsId == LocationProgramDetailsId && p.IsActive == true
                                  select c).Distinct().ToList();


                if (PersonsDetails != null && PersonsDetails.Count > 0)
                {
                    LocationProgramScheduled = true;
                }
                return LocationProgramScheduled;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
        }

    }
}
