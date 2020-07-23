using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// DB接続とSQL実行処理
    /// </summary>
    public class SqlManager:IDisposable
    {
        private SqlConnection con;
        public SqlManager(string connectionString)
        {
            con = new SqlConnection(connectionString);
            con.Open();
        }
        public void Dispose()
        {
            if (con.State != System.Data.ConnectionState.Closed)
                con.Close();
            con.Dispose();
        }


        /// <summary>
        /// </summary>
        public void ExecuteReader(string sql,Action<IEnumerable<IDataRecord>> action) 
        {
            using (var cmd = con.CreateCommand()) 
            {
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader()) 
                {
                    action(reader.EachReader());
                }
            }
        }            
    }
}