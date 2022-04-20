using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace GenericExcelMapper
{
    class Dal
    {
        const string Conn = @"user id=sa;password=in@123**;server=HANU-192\SQLSERVER;database=CommissionDepartment-ver2; connection timeout=30";

        /// <summary>
        /// Add a Template to database
        /// </summary>
        /// <param name="path"></param>
        /// <param name="payerId"></param>
        /// <param name="xlColumnList"></param>
        /// <param name="sheetName"></param>
        /// <param name="dataPosLeft"></param>
        /// <param name="dataPosTop"></param>
        /// <param name="dataPosBottom"></param>
        /// <param name="dataPosRight"></param>
        /// <returns></returns>
        public int AddTemplate(string payerId, string xlColumnList, string sheetName, int dataPosLeft, int dataPosTop, int dataPosBottom, int dataPosRight)
        {   
          
            var myConnection = new SqlConnection(Conn);            
            myConnection.Open();           
            var cmd = new SqlCommand("AddTemplate", myConnection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@PayerID", payerId));
            cmd.Parameters.Add(new SqlParameter("@ColumnList", xlColumnList));
            cmd.Parameters.Add(new SqlParameter("@SheetName", sheetName));
            cmd.Parameters.Add(new SqlParameter("@DataPosLeft", dataPosLeft));
            cmd.Parameters.Add(new SqlParameter("@DataPosTop", dataPosTop));
            cmd.Parameters.Add(new SqlParameter("@DataPosBottom", dataPosBottom));
            cmd.Parameters.Add(new SqlParameter("@DataPosRight", dataPosRight));
            SqlParameter retval = cmd.Parameters.Add("@retval", SqlDbType.VarChar);
            retval.Direction = ParameterDirection.ReturnValue;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "AddTemplate");
            }
            int retunvalue = (int)cmd.Parameters["@retval"].Value;
            myConnection.Close();
            return retunvalue;
        }

        /// <summary>
        /// Add a Excel and DB Column Mapping to database
        /// </summary>
        /// <param name="m"></param>
        public void AddMapping(Map m)
        {
            var myConnection = new SqlConnection(Conn);           
            myConnection.Open();
            string sql2 = "INSERT INTO Mapping(TemplateID,XLColumn,DBColumn)VALUES(" + m.TemplateId+",'"+ m.XlColumn+"','"+m.DbColumn+"')";
            var myCommand2 = new SqlCommand(sql2, myConnection);
            try
            {
                myCommand2.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, sql2);
            }
            myConnection.Close();         
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public List<Map> GetMapListDb(int templateId)
        {
            var mapList = new List<Map>();
            var sqlconn = new SqlConnection(Conn);
            var dataAdapter = new SqlDataAdapter("SELECT XLColumn , DBColumn FROM Mapping where TemplateID = " + templateId + " ;", sqlconn);
            var dataSet = new DataSet();
            try
            {
                dataAdapter.Fill(dataSet);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "");
            }
            foreach (DataRow dr in dataSet.Tables[0].Rows)
            {
                Map map = new Map();
                map.XlColumn = dr[0].ToString();
                map.DbColumn = dr[1].ToString();
                mapList.Add(map);
            }
            return mapList;
        }

        /// <summary>
        /// Get Schema of EntriesByDEU table 
        /// </summary>
        /// <returns></returns>
        public List<Template> GetTemplates()
        {
            var dbColumnList = new List<Template>();
            var sqlconn = new SqlConnection(Conn);
            var dataAdapter = new SqlDataAdapter("SELECT TemplateID , ExcelColumnList FROM Templates where isActive=1;", sqlconn);
            var dataSet = new DataSet();
            try
            {
                dataAdapter.Fill(dataSet);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "");
            }

            if (dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dataSet.Tables[0].Rows)
                {
                    var template = new Template();
                    template.TemplateId = Convert.ToInt32(dr[0]);
                    template.ColumnList = dr[1].ToString();
                    dbColumnList.Add(template);
                }
            }
            
            return dbColumnList;
        }

        /// <summary>
        /// Get Schema of EntriesByDEU table 
        /// </summary>
        /// <returns></returns>
        public List<Column> GetDbColumns()
        {
            var dbColumnList = new List<Column>();
            var sqlconn = new SqlConnection(Conn);
            var dataAdapter = new SqlDataAdapter("select top 1 * from EntriesByDEU", sqlconn);            
            var dataSet = new DataSet();
            try
            {
                dataAdapter.Fill(dataSet);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "");
            }
            for (int i = 0; i < dataSet.Tables[0].Columns.Count; i++)
            {              
                var cl = new Column();
                cl.CloumnName=dataSet.Tables[0].Columns[i].ToString();
                dbColumnList.Add(cl);
            }
            return dbColumnList;           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Issue> GetIssueList(string payerId)
        {
            var IssueList = new List<Issue>();
            var sqlconn = new SqlConnection(Conn);
            var dataAdapter = new SqlDataAdapter("SELECT [PayerID],[ExcelFilePath] FROM [ExcelIssueList] where [PayerID]='" + payerId + "'", sqlconn);
            var dataSet = new DataSet();
            try
            {
                dataAdapter.Fill(dataSet);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "");
            }
            for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
            {
                var cl = new Issue();
                cl.IssueId = dataSet.Tables[0].Rows[i][0].ToString();
                cl.IssueName = dataSet.Tables[0].Rows[i][1].ToString();
                IssueList.Add(cl);
            }
            return IssueList;
        }

        public void UpdateDatabase(List<ColData> row)
        {
            string columnList = GetColumnList(row);
            string rowData = GetRowData(row);
            var myConnection = new SqlConnection(Conn);
            myConnection.Open();
            string Sql = @"insert into EntriesByDEU (" + columnList + ")values (" + rowData + @")";
            try
            {               
                var myCommand = new SqlCommand(Sql, myConnection);
                myCommand.ExecuteNonQuery();
            }
            catch(Exception ex) 
            {
                WriteErrorLog( ex, Sql);
            }
            myConnection.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public string GetColumnList(List<ColData> row)
        {
            string columnList = string.Empty;            
            foreach (ColData d in row)
            {
                if (columnList == string.Empty)
                {
                    columnList = columnList + d.DbColumn;
                }
                else
                {
                    columnList = columnList + "," + d.DbColumn;
                }
            }
            return columnList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public string GetRowData(List<ColData> row)
        {           
            string rowData = string.Empty;
            foreach (ColData d in row)
            {
                if (!d.IsNumeric)
                {
                    if (rowData == string.Empty)
                    {
                        rowData = rowData + "'"+ d.XlValue+"'";
                    }
                    else
                    {
                        rowData = rowData + "," + "'" + d.XlValue + "'";
                    }
                }
                else
                {
                    if (rowData == string.Empty)
                    {
                        rowData = rowData + d.XlValue;
                    }
                    else
                    {
                        rowData = rowData + "," + d.XlValue;
                    }
                }
            }
            return rowData;
        }

        private static bool WriteErrorLog(Exception objException,string message)
        {
            const string strPathName = @"d:\log.txt";
            bool bReturn;
           
            try
            {
                var sw = new StreamWriter(strPathName,true);
                sw.WriteLine("Source        : " + 
                        objException.Source.ToString().Trim());
                sw.WriteLine("Method        : " + 
                        objException.TargetSite.Name.ToString());
                sw.WriteLine("Date        : " + 
                        DateTime.Now.ToLongTimeString());
                sw.WriteLine("Time        : " + 
                        DateTime.Now.ToShortDateString());     
                sw.WriteLine("Error        : " +  
                        objException.Message.ToString().Trim());
                sw.WriteLine("Error        : " +
                      objException.Message.ToString().Trim());
                sw.WriteLine("Detail    : " +
                        message.Trim());
                sw.WriteLine("^^-------------------------------------------------------------------^^");
                sw.Flush();
                sw.Close();
                bReturn    = true;
            }
            catch(Exception)
            {
                bReturn    = false;
            }
            return bReturn;
        }
    }
}
