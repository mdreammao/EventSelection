using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace EventSelection
{
    /// <summary>
    /// 记录一些重复使用的sql功能。
    /// </summary>
    class SqlApplication
    {
        /// <summary>
        /// 判断表是否存在。
        /// </summary>
        /// <param name="dataBaseName">数据库名</param>
        /// <param name="tableName">表名</param>
        /// <returns>返回是否存在表</returns>
        public static bool CheckExist(string dataBaseName, string tableName, string connectString = "")
        {
            if (connectString == "")
            {
                connectString = Configuration.connectString;
            }
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select COUNT(*) from [" + dataBaseName + "].sys.sysobjects where name = '" + tableName + "'";
                try
                {

                    int number = (int)cmd.ExecuteScalar();
                    if (number > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// 判断给定数据库是否存在
        /// </summary>
        /// <param name="dataBaseName">数据库名</param>
        /// <param name="connectString">连接字符串</param>
        /// <returns>返回是否存在数据库</returns>
        public static bool CheckDataBaseExist(string dataBaseName, string connectString)
        {
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();//打开数据库  
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select count(*) from sysdatabases where name='" + dataBaseName + "'";
                try
                {

                    int number = (int)cmd.ExecuteScalar();
                    if (number > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception myerror)
                {
                    System.Console.WriteLine(myerror.Message);
                }
            }
            return false;
        }
    }
}
