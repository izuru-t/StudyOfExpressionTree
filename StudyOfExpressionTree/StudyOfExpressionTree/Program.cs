using Common;
using Dapper;
using Entitys;
using Ha2ne2.DBSimple;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace StudyOfExpressionTree
{
    class Program
    {
        static void Main(string[] args)
        {
            //deleteTestData();
            //createTestData();
            studyOfET();
            Console.WriteLine("終了");
            Console.ReadLine();
        }
        /// <summary>
        /// ExpressionTreeの勉強。実行速度の比較
        /// </summary>
        private static void studyOfET() 
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString;
            var dbManager = new SqlManager(connectionString);
            var converter = new Converter();
            var connection = new SqlConnection(connectionString);

            // Where句を変えることで対象レコード数を調整
            string query = "SELECT * FROM [User] WHERE id < 1000";
            int loopMax = 10; // 計測回数
            try
            {
                connection.Open();
                #region 準備
                // 全ツールを測定前に1度実行しておく
                List<User> list = new List<User>();
                dbManager.ExecuteReader(query, reader => list.AddRange(reader.Select(dataRecord => dataRecord.ConvertRecord<User>())));
                dbManager.ExecuteReader(query, reader => list.AddRange(reader.Select(dataRecord => converter.ConvertRecordET<User>(dataRecord))));
                var users = DBSimple.ORMap<User>(connectionString, query, preloadDepth: 2);
                var us = connection.Query<User>(query);
                #endregion

                // 計測開始
                var sw = new Stopwatch();
                for (int i = 0; i < loopMax; i++)
                {
                    Console.WriteLine("{0}回目", i+1);

                    clearCache();
                    sw.Start();
                    List<User> userList = new List<User>();
                    dbManager.ExecuteReader(query, reader => userList.AddRange(reader.Select(dataRecord => dataRecord.ConvertRecord<User>())));
                    sw.Stop();
                    Console.WriteLine("　reflection：{0}ms", sw.ElapsedMilliseconds);

                    clearCache();
                    sw.Restart();
                    List<User> userList2 = new List<User>();
                    dbManager.ExecuteReader(query, reader => userList2.AddRange(reader.Select(dataRecord => converter.ConvertRecordET<User>(dataRecord))));
                    sw.Stop();
                    Console.WriteLine("　Expression：{0}ms", sw.ElapsedMilliseconds);

                    clearCache();
                    sw.Restart();
                    var userLists = DBSimple.ORMap<User>(connectionString, query);
                    sw.Stop();
                    Console.WriteLine("　DbSimple  ：{0}ms", sw.ElapsedMilliseconds);

                    clearCache();
                    sw.Restart();
                    var userLists2 = connection.Query<User>(query);
                    sw.Stop();
                    Console.WriteLine("　Dapper    ：{0}ms", sw.ElapsedMilliseconds);
                }
            }
            finally
            {
                connection.Close();
            }
        }
        /// <summary>
        /// DBの実行プランのキャッシュなどをクリアする。
        /// </summary>
        private static void clearCache()
        {
            // 接続文字列の取得
            var connectionString = ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "DBCC DROPCLEANBUFFERS; DBCC FREEPROCCACHE";
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// テストデータ削除
        /// </summary>
        private static void deleteTestData()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                for (int i = 0; i < 10000; i++)
                {
                    command.CommandText = "TRUNCATE TABLE [User]";
                    command.ExecuteNonQuery();
                }

            }
        }
        /// <summary>
        /// テストデータインサート
        /// </summary>
        private static void createTestData()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                for (int i = 0; i < 10000; i++)
                {
                    command.CommandText = string.Format("INSERT INTO [User] (id,Name,NameKana,BirthDay)VALUES({0},{0},{0},'1900/1/1')", i);
                    command.ExecuteNonQuery();
                }

            }
        }
    }
}
