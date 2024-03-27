using FaceApp.Data;
using FaceApp.Models;
using FaceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;


namespace FaceApp.Services
{
    public class CommonService : IDisposable
    {
        private readonly FaceAppDBContext _context;
        Authentication authenticate = new Authentication();
        DbAccess access = null;
        public CommonService(FaceAppDBContext context)
        {
            access = new DbAccess();
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            try
            {
                foreach (PropertyDescriptor prop in properties)
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

                foreach (T item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                        /* if (prop.Name.ToLower().Contains("id"))
                         {
                             row[prop.Name] = prop.GetValue(item) ?? 0;
                         }
                         else*/
                        if (prop.Name.ToLower() == "isactive" || prop.Name.ToLower() == "isclosed")
                        {
                            row[prop.Name] = Convert.ToByte(prop.GetValue(item) != null ? prop.GetValue(item) : 0);
                        }
                        else
                        {
                            row[prop.Name] = prop.GetValue(item) != null ? prop.GetValue(item).ToString().Trim() : "";
                        }
                    table.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("ConvertToDataTable Catch Block");
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
            return table;

        }

        public List<T> ConvertToList<T>(DataTable dt)
        {
            var list = new List<T>();
            try
            {
                var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
                var properties = typeof(T).GetProperties();
                list = dt.AsEnumerable().Select(row =>
                {
                    var objT = Activator.CreateInstance<T>();
                    foreach (var pro in properties)
                    {
                        if (columnNames.Contains(pro.Name.ToLower()))
                        {
                            if (pro.Name.ToLower().EndsWith("id") && pro.Name.ToLower().EndsWith("UserEmpId"))
                            {
                                try
                                {
                                    pro.SetValue(objT, row[pro.Name] != DBNull.Value ? Convert.ToInt64(row[pro.Name]) : 0);
                                }
                                catch
                                {
                                    pro.SetValue(objT, row[pro.Name] != DBNull.Value ? Convert.ToInt32(row[pro.Name]) : 0);
                                }
                            }
                            else
                            {
                                pro.SetValue(objT, row[pro.Name] != DBNull.Value ? row[pro.Name] : pro.PropertyType.Name != "Nullable`1" ? "" : null);
                            }
                        }
                    }
                    return objT;
                }).ToList();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("ConvertToList Catch Block");
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw ex;
            }
            return list;
        }

       
    }
}
