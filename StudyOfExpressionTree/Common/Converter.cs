using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;

namespace Common
{
    /// <summary>
    /// 自分なりに考えたExpressionTree版の簡易O/RMapper
    /// 本当は複数のクラスオブジェクトに対応するために
    /// ConcurrentDictionaryの構造はもっとまじめに作る必要がある
    /// （ConcurrentDictionary<string, ConcurrentDictionary<string, Action<object, object>>>にするとか）
    /// </summary>
    public class Converter
    {
        /// <summary>式木を保持しておくためのDictionary</summary>
        private ConcurrentDictionary<string, Action<object, object>> dic;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Converter()
        {
            dic = new ConcurrentDictionary<string, Action<object, object>>();
        }
        /// <summary>
        /// DBから取得した値をオブジェクトに変換する
        /// </summary>
        /// <typeparam name="T">変換するクラスオブジェクト</typeparam>
        /// <param name="record">DBから取得した1行</param>
        /// <returns></returns>
        public T ConvertRecordET<T>(IDataRecord record) where T : new()
        {
            var container = new T();
            createDelegate(typeof(T));
            foreach (var property in container.GetType().GetProperties())
            {
                dic[property.Name].Invoke(container, record.GetValue(record.GetOrdinal(property.Name)));
            }
            return container;
        }
        /// <summary>
        /// 式木をプロパティの数だけ生成する
        /// </summary>
        /// <param name="type">クラスオブジェクトの型</param>
        private void createDelegate(Type type)
        {
            // プロパティの数だけ繰り替えす
            foreach (var prop in type.GetProperties())
            {
                // キーがDictionaryになければ追加
                if (!dic.ContainsKey(prop.Name))
                {
                    // Actionの第一引数：クラスオブジェクトのインスタンス
                    var target = Expression.Parameter(typeof(object), "target");
                    // Actionの第二引数：プロパティに設定する値（DBから取得した値）
                    var value = Expression.Parameter(typeof(object), "value");
                    // クラスオブジェクトから取得したプロパティをExpressionの型として生成
                    var left = Expression.PropertyOrField(
                        Expression.Convert(target, type)
                        , prop.Name
                        );
                    // プロパティに設定する値をプロパティの型に変換
                    var right = Expression.Convert(value, left.Type);
                    // 式の構築　プロパティに値を代入しコンパイル
                    var lambda = Expression.Lambda<Action<object, object>>(
                        Expression.Assign(left, right),
                        target, value).Compile();
                    // 式をDictionaryに保持
                    dic.TryAdd(prop.Name, lambda);
                }
            }
        }
    }
}
