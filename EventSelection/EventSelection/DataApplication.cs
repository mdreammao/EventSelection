using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace EventSelection
{
    class DataApplication
    {
        public string connectionString;
        public string dataBase;

        /// <summary>
        /// 构造函数。获取数据库以及SQL连接字符串。
        /// </summary>
        /// <param name="dataBase">数据库名称</param>
        /// <param name="connectionString">连接字符串</param>
        public DataApplication(string dataBase, string connectionString)
        {
            this.connectionString = connectionString;
            this.dataBase = dataBase;
        }
        /// <summary>
        /// 根据给定的表和日期获取数据内容。
        /// </summary>
        /// <param name="tableName">表的名称</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>DataTable格式的数据</returns>
        public DataTable GetDataTable(string tableName, int startDate = 0, int endDate = 0)
        {
            DataTable myDataTable = new DataTable();
            tableName = "[" + dataBase + "].[dbo].[" + tableName + "]";
            string commandString;
            if (startDate == 0)
            {
                commandString = "select * from " + tableName;
            }
            else
            {
                if (endDate == 0)
                {
                    endDate = startDate;
                }
                commandString = "select * from " + tableName + " where [tdate]>=" + startDate.ToString() + " and [tdate]<=" + endDate.ToString();
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = conn.CreateCommand())
                    {
                        command.CommandText = commandString;
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(myDataTable);
                        }
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            return myDataTable;
        }
    }
}
