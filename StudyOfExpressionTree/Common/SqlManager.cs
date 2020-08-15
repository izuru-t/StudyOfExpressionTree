using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;

namespace Common
{
    /// <summary>
    /// DB接続とSQL実行処理
    /// </summary>
    public class SqlManager : IDisposable
    {
        /// <summary>DB接続文字列</summary>
        private string connectionString;
        /// <summary>トランザクション制御用オブジェクト</summary>
        private TransactionScope transactionScope;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="connectionString">DB接続文字列</param>
        public SqlManager(string connectionString)
        {
            this.connectionString = connectionString;
        }
        /// <summary>
        /// パラメータクエリを使用したSQL文の実行
        /// 内部でトランザクションを生成しているので呼び出し側でトランザクションを実装すると入れ子になることに注意してください。
        /// </summary>
        /// <param name="sql">SQLの平文</param>
        /// <param name="action">SQL実行結果をどう使用するかを定義するAction</param>
        /// <param name="sqlPaamList">SQLパラメータのリスト（パラメータ無しの場合、指定しない）</param>
        public void ExecuteReader(string sql, Action<IEnumerable<IDataRecord>> action, List<IDataParameter> sqlPaamList = null)
        {
            if (!canExecuteSQL(sql))
                throw new ArgumentException("SQL文が正しくありません。");
            if(sqlPaamList != null && !sqlPaamList.TrueForAll(s => canExecuteSQL(s.Value.ToString()))) 
                throw new ArgumentException("パラメータが正しくありません。");

            using (var tx = new TransactionScope(TransactionScopeOption.Required))
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        if(sqlPaamList != null)
                            cmd.Parameters.AddRange(sqlPaamList.ToArray());
                        using (var reader = cmd.ExecuteReader())
                        {
                            action(reader.EachReader());
                        }
                    }
                }
                tx.Complete();
            }
        }
        /// <summary>
        /// パラメータクエリを使用したSQL文の実行
        /// トランザクションを生成しないので必要に応じて呼び出し側でトランザクションを実装してください。
        /// </summary>
        /// <param name="sql">SQLの平文</param>
        /// <param name="action">SQL実行結果をどう使用するかを定義するAction</param>
        /// <param name="sqlPaamList">SQLパラメータのリスト（パラメータ無しの場合、指定しない）</param>
        public void NoTransactionExecuteReader(string sql, Action<IEnumerable<IDataRecord>> action, List<IDataParameter> sqlPaamList=null)
        {
            if (!canExecuteSQL(sql))
                throw new ArgumentException("SQL文が正しくありません。");
            if (sqlPaamList != null && !sqlPaamList.TrueForAll(s => canExecuteSQL(s.Value.ToString())))
                throw new ArgumentException("パラメータが正しくありません。");

            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if(sqlPaamList != null)
                        cmd.Parameters.AddRange(sqlPaamList.ToArray());
                    using (var reader = cmd.ExecuteReader())
                    {
                        action(reader.EachReader());
                    }
                }
            }
        }

        /// <summary>
        /// トランザクションの開始
        /// </summary>
        /// <param name="option">トランザクションスコープのための追加オプション</param>
        /// <param name="timeout">トランザクションのタイムアウト値</param>
        public void BeginTransaction(TransactionScopeOption option,TimeSpan timeout) 
        {
            transactionScope = new TransactionScope(option, timeout);
        }
        /// <summary>
        /// トランザクションの終了
        /// </summary>
        /// <param name="isComplete">コミットして良いか</param>
        public void EndTransaction(bool isComplete) 
        {
            if(isComplete)
                transactionScope.Complete();
            transactionScope.Dispose();
        }

        /// <summary>
        /// SQLインジェクションチェック
        /// ここではサンプルとして「;」と「'」のみチェックしています
        /// </summary>
        /// <param name="target">チェック対象の文字列</param>
        /// <returns>
        /// true：問題なし
        /// false：問題あり
        /// </returns>
        private bool canExecuteSQL(string target)
        {
            if (target.Contains(";")) return false;
            if (target.Contains("'")) return false;

            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    transactionScope.Dispose();
                }
                disposedValue = true;
            }
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        void IDisposable.Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}