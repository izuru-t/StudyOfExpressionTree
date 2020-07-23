using System;

namespace Entitys
{
    /// <summary>
    /// 速度比較用のサンプルクラス
    /// DB側もid(int),Name(nvarchar50),NameKana(nvarchar50),BirthDay(date)の4フィールド
    /// </summary>
    public class User
    {
        // [PrimaryKey] 計測後に気づいたけどこれつけたら速度に変化でるか？
        public int id { get; set; }
        public string Name { get; set; }
        public string NameKana { get; set; }
        public DateTime BirthDay { get; set; }
    }
}
