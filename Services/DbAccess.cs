using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace FaceApp.Services
{
    public class DbAccess
    {
        private SqlConnection conn;

        private string _strConnection;
        public bool enableTransaction;
        private SqlTransaction dbTransaction;
        private string DefaultConnectionProperty = "FaceAppDatabase";
        private readonly IConfiguration Configuration;

        public DbAccess()
        {
            GetConnectionString(DefaultConnectionProperty);
            conn = new SqlConnection(_strConnection);
            enableTransaction = false;
        }

        #region GetConnectionstring
        public String GetConnectionString()
        {
            return _strConnection;
        }

        public void GetConnectionString(string str)
        {
            _strConnection = string.Empty;
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();
            if (!string.IsNullOrEmpty(str))
            {
                _strConnection = config["ConnectionStrings:FaceAppDatabase"];
            }
            else
            //name not provided, get the 'default' connection
            {
                _strConnection = config["ConnectionStrings:FaceAppDatabase"];
            }
        }
        #endregion


        #region Open-Close Connection,Transaction /Rollback Transaction
        private void OpenConnection()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.ConnectionString = _strConnection;
                conn.Open();
                if (enableTransaction == true)
                {
                    dbTransaction = conn.BeginTransaction();
                }
            }

        }
        public void CommitTransaction()
        {
            if (conn.State == ConnectionState.Open)
            {
                if (enableTransaction == true && dbTransaction != null)
                {
                    dbTransaction.Commit();
                    dbTransaction.Dispose();
                }
                conn.Close();
                conn.Dispose();

                enableTransaction = false;
            }
        }
        public void CloseConnection()
        {
            if (conn.State == ConnectionState.Open && enableTransaction == false)
            {
                conn.Close();
                conn.Dispose();
                enableTransaction = false;
            }
        }
        public void RollBackTransaction()
        {
            if (conn.State == ConnectionState.Open)
            {
                if (enableTransaction == true && dbTransaction != null)
                {
                    dbTransaction.Rollback();
                    dbTransaction.Dispose();
                }
                conn.Close();
                conn.Dispose();

                enableTransaction = false;
            }

        }

        #endregion

        public DataTable GetData(string TableName, CommandType commandType, SqlParameter[] parameters)
        {
            SqlCommand sqlcmd = new SqlCommand();
            DataTable dt = new DataTable();
            StringBuilder errorText = new StringBuilder("");
            try
            {
                OpenConnection();
                sqlcmd.Connection = conn;
                sqlcmd.CommandType = commandType;
                sqlcmd.CommandText = TableName;
                sqlcmd.CommandTimeout = 0;
                if (parameters != null)
                {
                    parameters.ToList().ForEach(f =>
                    {
                        sqlcmd.Parameters.Add(f);
                    });
                }
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = sqlcmd;
                //SqlDataReader dr = sqlcmd.ExecuteReader();
                //dr.Read();
                dataAdapter.Fill(dt);
            }
            catch (Exception ex)
            {
                errorText.Append("In DbAccess::GetData(");
                errorText.Append("Procedure:" + TableName);
                errorText.Append(" with parameters: (");
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i > 0)
                            errorText.Append(",");
                        if (parameters[i].Value == null)
                        {
                            errorText.Append(parameters[i] + " - No Data");
                        }
                        else
                        {
                            errorText.Append(parameters[i].Value.ToString());
                        }
                    }
                }
                //helper.Error(errorText.ToString(), ex);
                //log.Error(errorText.ToString());
                throw ex;
            }
            finally
            {
                sqlcmd.Parameters.Clear();
                CloseConnection();
            }
            return dt;
        }

        public void ExecuteQuery(string TableName, CommandType commandType, SqlParameter[] parameters)
        {
            SqlCommand sqlcmd = new SqlCommand();
            DataTable dt = new DataTable();
            StringBuilder errorText = new StringBuilder("");
            try
            {
                //sqlcon.Open();
                OpenConnection();
                sqlcmd.Connection = conn;
                sqlcmd.CommandType = commandType;
                sqlcmd.CommandText = TableName;
                sqlcmd.CommandTimeout = 0;
                if (parameters != null)
                {
                    parameters.ToList().ForEach(f =>
                    {
                        sqlcmd.Parameters.Add(f);
                    });
                }
                sqlcmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorText.Append("In DbAccess::ExecuteQuery(");
                errorText.Append("Procedure:" + TableName);
                errorText.Append(" with parameters: (");
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i > 0)
                            errorText.Append(",");
                        if (parameters[i].Value == null)
                        {
                            errorText.Append(parameters[i] + " - No Data");
                        }
                        else
                        {
                            errorText.Append(parameters[i].Value.ToString());
                        }
                    }
                }
                //helper.Error(errorText.ToString(), ex);
                //log.Error(errorText.ToString());
                throw ex;
            }
            finally
            {
                sqlcmd.Parameters.Clear();
                CloseConnection();
            }
        }

        public void ExecuteNonQuery(string TableName, CommandType commandType, SqlParameter[] parameters)
        {
            SqlCommand sqlcmd = new SqlCommand();
            DataTable dt = new DataTable();
            StringBuilder errorText = new StringBuilder("");
            try
            {
                OpenConnection();
                sqlcmd.Connection = conn;
                sqlcmd.CommandType = commandType;
                sqlcmd.CommandText = TableName;
                sqlcmd.CommandTimeout = 2400;
                if (parameters != null)
                {
                    parameters.ToList().ForEach(f =>
                    {
                        sqlcmd.Parameters.Add(f);
                    });
                }
                sqlcmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorText.Append("In DbAccess::ExecuteNonQuery(");
                errorText.Append("Procedure:" + TableName);
                errorText.Append(" with parameters: (");
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i > 0)
                            errorText.Append(",");
                        if (parameters[i].Value == null)
                        {
                            errorText.Append(parameters[i] + " - No Data");
                        }
                        else
                        {
                            errorText.Append(parameters[i].Value.ToString());
                        }
                    }
                }
                //helper.Error(errorText.ToString(), ex);
                //log.Error(errorText.ToString());
                throw ex;
            }
            finally
            {
                sqlcmd.Parameters.Clear();
                CloseConnection();
            }
        }

        public DataSet ExecuteSqlQuery(string sqlString, SqlParameter[] spParams, CommandType commandType, bool increaseTimeout = false)
        {
            SqlDataAdapter dataAdapter;
            SqlCommand commandObj = null;
            SqlParameter addedParam;
            DataSet dataSet;
            StringBuilder errorText = new StringBuilder("");
            try
            {
                OpenConnection();
                dataSet = new DataSet();
                commandObj = new SqlCommand(sqlString, conn, dbTransaction);
                if (spParams != null)
                {
                    foreach (SqlParameter paramObj in spParams)
                        addedParam = commandObj.Parameters.Add(paramObj);
                }
                if (increaseTimeout)
                    commandObj.CommandTimeout = 2400;
                commandObj.CommandType = commandType;
                dataAdapter = new SqlDataAdapter(commandObj);
                dataAdapter.Fill(dataSet);
            }
            catch (Exception ex)
            {
                RollBackTransaction();
                dataSet = null;
                errorText.Append("In DataAccess::ExecuteSqlQuery(");
                errorText.Append("Query:" + sqlString);
                errorText.Append(" with parameters: (");
                if (spParams != null)
                {
                    for (int i = 0; i < spParams.Length; i++)
                    {
                        if (i > 0)
                            errorText.Append(",");
                        errorText.Append(spParams[i].Value.ToString());
                    }
                }
                //helper.Error(errorText.ToString() + "Data Set ExecuteSqlQuery failed. Message:" , ex) ;
                //log.Error(errorText.ToString());
                throw ex;
            }
            finally
            {
                commandObj.Parameters.Clear();
                CloseConnection();
            }
            return dataSet;
        }

        public object ExecuteCustomQuery(string spName, SqlParameter[] spParams, SqlParameter outputParam, bool increaseTimeout)
        {
            SqlCommand commandObj = null;
            SqlParameter paramObj;
            object outValue = null;
            string outParamName = "";
            int rowsAffected = 0;
            StringBuilder errorText = new StringBuilder("");
            try
            {
                OpenConnection();
                commandObj = new SqlCommand(spName, conn, dbTransaction);
                commandObj.CommandType = CommandType.StoredProcedure;
                //commandObj.Connection = conn;
                if (increaseTimeout)
                    commandObj.CommandTimeout = 2400;
                if (spParams != null)
                {
                    foreach (SqlParameter param in spParams)
                        paramObj = commandObj.Parameters.Add(param);
                }
                if (outputParam != null)
                {
                    paramObj = commandObj.Parameters.Add(outputParam);
                    paramObj.Direction = ParameterDirection.Output;
                    outParamName = paramObj.ParameterName;
                }
                //if (conn.State == ConnectionState.Closed)
                //    conn.Open();

                rowsAffected = commandObj.ExecuteNonQuery();
                if (outParamName != null && outParamName != "")
                {
                    outValue = commandObj.Parameters[outParamName].Value;
                }
            }
            catch (Exception ex)
            {
                RollBackTransaction();
                errorText.Append("In StaticData::ExecuteCustomQuery(");
                errorText.Append(spName);
                errorText.Append(" with parameters: (");
                if (spParams != null)
                {
                    for (int i = 0; i < spParams.Length; i++)
                    {
                        if (i > 0)
                            errorText.Append(",");
                        errorText.Append(spParams[i].Value.ToString());
                    }
                }

                //helper.Error(errorText.ToString(), ex);
                //log.Error(errorText.ToString());
                throw ex;
            }
            finally
            {
                commandObj.Parameters.Clear();
                CloseConnection();
            }
            return outValue;
        }
    }
}
