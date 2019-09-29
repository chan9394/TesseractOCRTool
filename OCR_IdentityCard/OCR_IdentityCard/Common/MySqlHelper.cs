using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace OCR_IdentityCard.Common
{
    public class MySqlHelper
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["MyContext"].ToString();

       
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static String ExecuteSqlList(List<String> list)
        {
            string str = "";
            string strError = "";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    foreach (string strSql in list)
                    {
                        using (SqlCommand cmd = new SqlCommand(strSql, connection, transaction))
                        {
                            strError = strSql;
                            int rows = cmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    transaction.Rollback();
                    str = e.Message + " ------- " + strError;
                }
                finally
                {
                    connection.Close();
                }
            }

            return str;
        }

        //查询
        public static DataTable SelectData(string strSql)
        {
            DataTable dt = new DataTable();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (MySqlDataAdapter sda = new MySqlDataAdapter(strSql, connection))
                {
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    dt = ds.Tables[0];
                }

            }

            return dt;
        }
        /// <summary> 
        /// 执行SQL语句并返回数据表 
        /// </summary> 
        /// <param name="Sqlstr">SQL语句</param> 
        /// <returns></returns> 
        public static DataTable ExecuteQuery(String Sqlstr)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                MySqlDataAdapter da = new MySqlDataAdapter(Sqlstr, conn);
                DataTable dt = new DataTable();
                conn.Open();
                da.Fill(dt);
                conn.Close();
                return dt;
            }
        }
        /// <summary> 
        /// 执行SQL语句并返回数据表 
        /// </summary> 
        /// <param name="Sqlstr">SQL语句</param> 
        /// <returns></returns> 
        //public static DataSet ExecuteQuery(String Sqlstr, int index, int count, out int pages, out int totalcount)
        //{
        //    using (MySqlConnection conn = new MySqlConnection(connectionString))
        //    {
        //        MySqlDataAdapter da = new MySqlDataAdapter(Sqlstr, conn);
        //        DataSet dt = new DataSet();
        //        conn.Open();
        //        da.Fill(dt, index, count, "table");
        //        conn.Close();
        //        return dt;
        //    }
        //}

        /// <summary>
        /// 执行查询语句返回DataTable
        /// </summary>
        /// <param name="strSql">查询语句</param>
        /// <returns>DataTable</returns>
        public static DataTable ExecuteQuery(string strSql, SqlParameter[] pares, CommandType ty)
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(strSql, conn);
                cmd.CommandTimeout = 180;
                cmd.CommandType = ty;
                cmd.Parameters.AddRange(pares);
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// 执行查询语句返回DataSet
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="pares"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public static DataSet ExecuteSql(string strSql, SqlParameter[] pares, CommandType ty)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(strSql, conn);
                cmd.CommandTimeout = 180;
                cmd.CommandType = ty;
                cmd.Parameters.AddRange(pares);
                SqlDataAdapter dr = new SqlDataAdapter(cmd);
                dr.Fill(ds);

            }
            return ds;
        }

        /// <summary>
        /// 执行查询语句返回DataSet
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="pares"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public static DataTable ExecuteToDataTable(string strMySql, Dictionary<string, object> parameters)
        {
            DataTable table = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(strMySql, conn);
                    foreach (string key in parameters.Keys)
                    {
                        cmd.Parameters.Add(new MySqlParameter(key, parameters[key]));
                    }
                    MySqlDataAdapter dr = new MySqlDataAdapter(cmd);
                    dr.Fill(table);
                }
                catch (Exception ex)
                {
                    throw new Exception("执行语句失败:\n" + ex.Message);
                }
            }
            return table;
        }


        /// <summary>
        /// 执行查询语句返回DataSet
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="pares"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public static DataSet ExecuteSql(string strSql)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(strSql, conn);
                cmd.CommandTimeout = 180;
                //cmd.CommandType = ty;
                SqlDataAdapter dr = new SqlDataAdapter(cmd);
                dr.Fill(ds);

            }
            return ds;
        }


        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="strSql">sql语句</param>
        /// <returns></returns>
        public static object ExecuteQueryScalar(string strSql)
        {

            object obj = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(strSql, conn);
                cmd.CommandTimeout = 180;
                cmd.CommandType = CommandType.Text;
                obj = cmd.ExecuteScalar();
                conn.Close();
                return obj;
            }
        }

        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="strSql">sql语句</param>
        /// <returns></returns>
        public static object ExecuteQueryScalar(string strSql, SqlParameter[] paras, CommandType ct)
        {

            object obj = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(strSql, conn);
                cmd.CommandType = ct;
                cmd.Parameters.AddRange(paras);
                obj = cmd.ExecuteScalar();
                conn.Close();
                return obj;
            }
        }

        /// <summary>
        ///  执行带参数的增删改SQL语句或存储过程
        /// </summary>
        /// <param name="cmdText">增删改SQL语句或存储过程</param>
        /// <param name="ct">命令类型</param>
        /// <returns></returns>
        public static bool ExecuteNonQuery(string cmdText, MySqlParameter[] paras, CommandType ct)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(cmdText, conn);
                cmd.CommandType = ct;
                cmd.Parameters.AddRange(paras);
                int temp = cmd.ExecuteNonQuery();
                if (temp > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 以非查询（添加，删除，更新等）的方式执行数据库命令
        /// </summary>
        /// <param name="command">数据库命令</param>
        /// <returns></returns>
        public static int ExecuteNoQuery(string sql, Dictionary<string, object> parameters)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand command = new MySqlCommand(sql, conn);
                    foreach (string key in parameters.Keys)
                    {
                        command.Parameters.Add(new MySqlParameter(key, parameters[key]));
                    }
                    return command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("执行语句失败:\n" + ex.Message);
                }
            }
        }
        /// <summary>
        /// 以非查询（添加，删除，更新等）的方式执行数据库命令
        /// </summary>
        /// <param name="command">数据库命令</param>
        /// <returns></returns>
        public static int ExecuteNoQuery(string sql, Dictionary<string, object> parameters, out long id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {

                try
                {
                    conn.Open();
                    MySqlCommand command = new MySqlCommand(sql, conn);
                    foreach (string key in parameters.Keys)
                    {
                        command.Parameters.Add(new MySqlParameter(key, parameters[key]));
                    }
                    int resultcount = command.ExecuteNonQuery();
                    id = command.LastInsertedId;
                    return resultcount;
                }
                catch (Exception ex)
                {
                    throw new Exception("执行语句失败:\n" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="strMySql">MySql语句</param>
        /// <returns></returns>
        public static object ExecuteQueryScalarWithId(string strMySql, MySqlParameter[] paras, CommandType ct)
        {

            object obj = null;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(strMySql, conn);
                cmd.CommandType = ct;
                cmd.Parameters.AddRange(paras);
                obj = cmd.ExecuteScalar();
                conn.Close();
                return obj;
            }
        }

        /// <summary>
        ///  执行带参数的增删改SQL语句或存储过程
        /// </summary>
        /// <param name="cmdText">增删改SQL语句或存储过程</param>
        /// <param name="ct">命令类型</param>
        /// <returns></returns>
        public static bool ExecuteNonQuery(SqlConnection conn, string cmdText, SqlParameter[] paras, CommandType ct)
        {

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.CommandType = ct;
                cmd.Parameters.AddRange(paras);
                cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }


        }


        /// <summary>
        ///  执行不带参数的增删改SQL语句或存储过程
        /// </summary>
        /// <param name="cmdText">增删改SQL语句或存储过程</param>
        /// <param name="ct">命令类型</param>
        /// <returns></returns>
        public static bool ExecuteNonQuery(string cmdText, CommandType ct, out long reusltid)
        {
            bool res = false;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(cmdText, conn);
                cmd.CommandType = ct;
                int temp = cmd.ExecuteNonQuery();
                reusltid = cmd.LastInsertedId;
                if (temp > 0)
                {
                    res = true;

                }
                else
                {
                    res = false;
                }
            }
            return res;
        }

        /// <summary>
        /// 实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public static bool ExecuteSqlTran(List<string> SQLStringList)
        {
            bool success = true;
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();
            try
            {
                for (var i = 0; i < SQLStringList.Count; i++)
                {
                    SqlCommand cmd = new SqlCommand(SQLStringList[i], conn);
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();
                }
                trans.Commit();
            }
            catch (Exception ex)
            {
                success = false;
                trans.Rollback();
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return success;

        }


        /// <summary>
        /// 执行多条他带参数的sql语句
        /// </summary>
        /// <param name="SqlStrings"></param>
        /// <param name="prams"></param>
        /// <returns></returns>
        public static bool ExecuteSqlTran(List<string> SqlStrings, List<SqlParameter[]> prams)
        {
            bool success = true;
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();
            try
            {
                for (var i = 0; i < SqlStrings.Count; i++)
                {
                    SqlCommand cmd = new SqlCommand(SqlStrings[i], conn);
                    cmd.Transaction = trans;
                    if (prams.Count > 0)
                    {
                        foreach (SqlParameter p in prams[i])
                        {
                            cmd.Parameters.Add(p);
                        }
                    }
                    else
                    {
                        return false;
                    }
                    cmd.ExecuteNonQuery();
                }
                trans.Commit();

            }
            catch (SqlException ex)
            {
                success = false;
                trans.Rollback();
                throw new Exception(ex.Message);

            }
            finally
            {
                conn.Close();
            }
            return success;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.Fill(dataSet, tableName);
                return dataSet;
            }
        }

        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand</returns>
        private static MySqlCommand BuildQueryCommand(MySqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            MySqlCommand command = new MySqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                foreach (MySqlParameter parameter in parameters)
                {
                    if (parameter != null)
                    {
                        // 检查未分配值的输出参数,将其分配以DBNull.Value.
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                            (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(parameter);
                    }
                }
            }

            return command;
        }

    }
}
