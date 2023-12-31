﻿using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper;

namespace Rain.Infrastructure
{
    public static class DapperBulk
    {
        public static async Task BulkInsert(this SqlConnection connection, string tableName, Type type, IEnumerable<object> data, SqlTransaction transaction = null, int batchSize = 0, int bulkCopyTimeout = 30)
        {
            var allProperties = PropertiesCache.TypePropertiesCache(type);
            var computedProperties = PropertiesCache.ComputedPropertiesCache(type);
            var columns = PropertiesCache.GetColumnNamesCache(type);

            var insertProperties = allProperties.Except(computedProperties).ToList();

            var insertPropertiesString = GetColumnsStringSqlServer(insertProperties, columns);
            var tempToBeInserted = $"#TempInsert_{tableName}".Replace(".", string.Empty);

            await connection.ExecuteAsync($@"SELECT TOP 0 {insertPropertiesString} INTO {tempToBeInserted} FROM {FormatTableName(tableName)} target WITH(NOLOCK);", null, transaction);

            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                bulkCopy.BatchSize = batchSize;
                bulkCopy.DestinationTableName = tempToBeInserted;
                await bulkCopy.WriteToServerAsync(ToDataTable(data, insertProperties).CreateDataReader());
            }

            await connection.ExecuteAsync($@"
                INSERT INTO {FormatTableName(tableName)}({insertPropertiesString}) 
                SELECT {insertPropertiesString} FROM {tempToBeInserted}
                DROP TABLE {tempToBeInserted};", null, transaction);
        }

        public static async Task BulkUpsert(this SqlConnection connection, string tableName, Type type, IEnumerable<object> data, IEnumerable<string> keys = null, SqlTransaction transaction = null, int batchSize = 0, int bulkCopyTimeout = 30)
        {
            var allProperties = PropertiesCache.TypePropertiesCache(type);
            var computedProperties = PropertiesCache.ComputedPropertiesCache(type);
            var columns = PropertiesCache.GetColumnNamesCache(type);

            var insertProperties = allProperties.Except(computedProperties).ToList();
            var keyProperties = keys != null
                ? allProperties.Where(p => keys.Contains(p.Name)).ToList()
                : PropertiesCache.KeyPropertiesCache(type);

            var insertPropertiesString = GetColumnsStringSqlServer(insertProperties, columns);
            var onPropertiesString = GetOnColumnsStringSqlServer(keyProperties, columns, "[TARGET].", "[SOURCE].");
            var updatePropertiesString = GetUpdateColumnsStringSqlServer(insertProperties.Except(keyProperties).ToList(), columns, "[TARGET].", "[SOURCE].");
            var tempToBeInserted = $"#TempInsert_{tableName}".Replace(".", string.Empty);

            await connection.ExecuteAsync($@"SELECT TOP 0 {insertPropertiesString} INTO {tempToBeInserted} FROM {FormatTableName(tableName)} target WITH(NOLOCK);", null, transaction);

            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                bulkCopy.BatchSize = batchSize;
                bulkCopy.DestinationTableName = tempToBeInserted;
                await bulkCopy.WriteToServerAsync(ToDataTable(data, insertProperties).CreateDataReader());
            }

            var sqlCommand = $@"
                MERGE {FormatTableName(tableName)} AS [TARGET]
                USING (SELECT {insertPropertiesString} FROM {tempToBeInserted}) AS [SOURCE]
                ON {onPropertiesString}
                WHEN MATCHED THEN
                   UPDATE SET {updatePropertiesString}
                WHEN NOT MATCHED THEN
                   INSERT ({insertPropertiesString})
                   VALUES ({insertPropertiesString});
                DROP TABLE {tempToBeInserted};";

            await connection.ExecuteAsync(sqlCommand, null, transaction);
        }

        private static DataTable ToDataTable<T>(IEnumerable<T> data, IList<PropertyInfo> properties)
        {
            var typeCasts = new Type[properties.Count];
            for (var i = 0; i < properties.Count; i++)
            {
                if (properties[i].PropertyType.IsEnum)
                {
                    typeCasts[i] = Enum.GetUnderlyingType(properties[i].PropertyType);
                }
                else
                {
                    typeCasts[i] = null;
                }
            }

            var dataTable = new DataTable();
            for (var i = 0; i < properties.Count; i++)
            {
                // Nullable types are not supported.
                var propertyNonNullType = Nullable.GetUnderlyingType(properties[i].PropertyType) ?? properties[i].PropertyType;
                dataTable.Columns.Add(properties[i].Name, typeCasts[i] == null ? propertyNonNullType : typeCasts[i]);
            }

            foreach (var item in data)
            {
                var values = new object[properties.Count];
                for (var i = 0; i < properties.Count; i++)
                {
                    var value = properties[i].GetValue(item, null);
                    values[i] = typeCasts[i] == null ? value : Convert.ChangeType(value, typeCasts[i]);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        private static string GetColumnsStringSqlServer(IEnumerable<PropertyInfo> properties, IReadOnlyDictionary<string, string> columnNames, string tablePrefix = null)
        {
            if (tablePrefix == "target.")
            {
                return string.Join(", ", properties.Select(property => $"{tablePrefix}[{columnNames[property.Name]}] as [{property.Name}] "));
            }

            return string.Join(", ", properties.Select(property => $"{tablePrefix}[{columnNames[property.Name]}] "));
        }

        private static string GetOnColumnsStringSqlServer(IEnumerable<PropertyInfo> properties, IReadOnlyDictionary<string, string> columnNames, string targetPrefix, string sourcePrefix)
        {
            return string.Join("AND ", properties.Select(property => $"{targetPrefix}[{columnNames[property.Name]}] = {sourcePrefix}[{columnNames[property.Name]}] "));
        }

        private static string GetUpdateColumnsStringSqlServer(IEnumerable<PropertyInfo> properties, IReadOnlyDictionary<string, string> columnNames, string targetPrefix, string sourcePrefix)
        {
            return string.Join(", ", properties.Select(property => $"{targetPrefix}[{columnNames[property.Name]}] = {sourcePrefix}[{columnNames[property.Name]}] "));
        }

        internal static string FormatTableName(string table)
        {
            if (string.IsNullOrEmpty(table))
            {
                return table;
            }

            var parts = table.Split('.');

            if (parts.Length == 1)
            {
                return $"[{table}]";
            }

            var tableName = "";
            for (int i = 0; i < parts.Length; i++)
            {
                tableName += $"[{parts[i]}]";
                if (i + 1 < parts.Length)
                {
                    tableName += ".";
                }
            }

            return tableName;
        }
    }

    internal static class PropertiesCache
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> KeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ComputedProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IReadOnlyDictionary<string, string>> ColumnNames = new ConcurrentDictionary<RuntimeTypeHandle, IReadOnlyDictionary<string, string>>();

        public static List<PropertyInfo> TypePropertiesCache(Type type)
        {
            if (TypeProperties.TryGetValue(type.TypeHandle, out var cachedProps))
            {
                return cachedProps.ToList();
            }

            var properties = type.GetProperties().Where(ValidateProperty).ToList();
            TypeProperties[type.TypeHandle] = properties;
            ColumnNames[type.TypeHandle] = GetColumnNames(properties);

            return properties.ToList();
        }

        public static IReadOnlyDictionary<string, string> GetColumnNamesCache(Type type)
        {
            if (ColumnNames.TryGetValue(type.TypeHandle, out var cachedProps))
            {
                return cachedProps;
            }

            var properties = type.GetProperties().Where(ValidateProperty).ToList();
            TypeProperties[type.TypeHandle] = properties;
            ColumnNames[type.TypeHandle] = GetColumnNames(properties);

            return ColumnNames[type.TypeHandle];
        }

        public static bool ValidateProperty(PropertyInfo prop)
        {
            var result = prop.CanWrite;
            result = result && (prop.GetSetMethod(true)?.IsPublic ?? false);
            result = result && (!prop.PropertyType.IsClass || prop.PropertyType == typeof(string) || prop.PropertyType == typeof(byte[]));
            result = result && prop.GetCustomAttributes(true).All(a => a.GetType().Name != "NotMappedAttribute");

            var writeAttribute = prop.GetCustomAttributes(true).FirstOrDefault(x => x.GetType().Name == "WriteAttribute");
            if (writeAttribute != null)
            {
                var writeProperty = writeAttribute.GetType().GetProperty("Write");
                if (writeProperty != null && writeProperty.PropertyType == typeof(bool))
                {
                    result = result && (bool)writeProperty.GetValue(writeAttribute);
                }
            }

            return result;
        }

        public static List<PropertyInfo> KeyPropertiesCache(Type type)
        {
            if (KeyProperties.TryGetValue(type.TypeHandle, out var cachedProps))
            {
                return cachedProps.ToList();
            }

            var allProperties = TypePropertiesCache(type);
            var keyProperties = allProperties.Where(p => p.GetCustomAttributes(true).Any(a => a.GetType().Name == "KeyAttribute")).ToList();

            if (keyProperties.Count == 0)
            {
                var idProp = allProperties.Find(p => string.Equals(p.Name, "id", StringComparison.CurrentCultureIgnoreCase));
                if (idProp != null)
                {
                    keyProperties.Add(idProp);
                }
            }

            KeyProperties[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        public static List<PropertyInfo> ComputedPropertiesCache(Type type)
        {
            if (ComputedProperties.TryGetValue(type.TypeHandle, out var cachedProps))
            {
                return cachedProps.ToList();
            }

            var computedProperties = TypePropertiesCache(type).Where(p => p.GetCustomAttributes(true).Any(a => a.GetType().Name == "ComputedAttribute")).ToList();
            ComputedProperties[type.TypeHandle] = computedProperties;
            return computedProperties;
        }

        private static IReadOnlyDictionary<string, string> GetColumnNames(IEnumerable<PropertyInfo> props)
        {
            var ret = new Dictionary<string, string>();
            foreach (var prop in props)
            {
                var columnAttr = prop.GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "ColumnAttribute") as dynamic;
                // if the column attribute exists, and specifies a column name, use that, otherwise fall back to the property name as the column name
                ret.Add(prop.Name, columnAttr != null ? (string)columnAttr.Name ?? prop.Name : prop.Name);
            }

            return ret;
        }
    }
}