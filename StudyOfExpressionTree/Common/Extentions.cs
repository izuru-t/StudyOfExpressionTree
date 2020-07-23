using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// reflectionによる簡易O/RMapperの実装
    /// </summary>
    public static class Extentions
    {
        public static IEnumerable<IDataRecord> EachReader(this DbDataReader reader)
        {
            do
            {
                while (!reader.IsClosed && reader.Read())
                    yield return reader;
            }
            while (reader.NextResult());
        }
        public static T ConvertRecord<T>(this IDataRecord record) where T : new()
        {
            var container = new T();
            foreach (var property in container.GetType().GetProperties())
            {
                switch (property.PropertyType.Name)
                {
                    case "Int32":
                        property.SetValue(container, record.GetInt32(record.GetOrdinal(property.Name)));
                        break;
                    case "Decimal":
                        property.SetValue(container, record.GetDecimal(record.GetOrdinal(property.Name)));
                        break;
                    case "DateTime":
                        property.SetValue(container, record.GetDateTime(record.GetOrdinal(property.Name)));
                        break;
                    case "String":
                        property.SetValue(container, record.GetString(record.GetOrdinal(property.Name)));
                        break;
                }
            }
            return container;
        }
    }
}
