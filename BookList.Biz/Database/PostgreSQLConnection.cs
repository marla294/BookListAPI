﻿using System;
using System.Collections.Generic;
using Npgsql;
using System.Linq;

namespace BookList.Biz.Database
{
    public class ColumnValuePairing
    {
        public string Column { get; set; }
        public object Value { get; set; }

        public ColumnValuePairing(string col, object val)
        {
            Column = col;
            Value = val;
        }
    }

    public class PostgreSQLConnection : IDbConnection
    {
        private string ConnectionString { get; set; }

        private string SQL { get; set; }
        private bool IsQuery { get; set; }
        private Dictionary<int, object> Parameters { get; set; }

        public PostgreSQLConnection()
        {
            ConnectionString = "Host=127.0.0.1;Port=5433;Username=postgres; Password=Password1;Database=booklist";
            ResetFields();
        }

        private void ResetFields()
        {
            SQL = "";
            Parameters = new Dictionary<int, object>();
        }

        // Starting place
        public PostgreSQLConnection Take(string table)
        {
            ResetFields();
            IsQuery = true;

            var sql = $"select * from {table}";

            SQL = sql;

            return this;
        }

        public PostgreSQLConnection Where(params ColumnValuePairing[] whereValues)
        {
            var parameterStart = Parameters.Count;
            var sql = SQL + $" where {whereValues[0].Column} = @parameter{(parameterStart + 1).ToString()}";
            Parameters.Add(parameterStart, whereValues[0].Value);

            for (var i = 1; i < whereValues.Length; i++)
            {
                var whereValue = whereValues[i];
                string snippet = $" and {whereValue.Column} = @parameter{(parameterStart + i + 1).ToString()}";
                sql = sql + snippet;
                Parameters.Add(parameterStart + i, whereValue.Value);
            }

            SQL = sql;

            return this;
        }

        public PostgreSQLConnection OrderBy(string orderBy, string orderByDirection = "desc")
        {
            SQL = SQL + $" order by {orderBy} {orderByDirection}";

            return this;
        }

        public PostgreSQLConnection Limit(int limit)
        {
            SQL = SQL + $" limit {limit}";

            return this;
        }

        // Starting place
        public PostgreSQLConnection Insert(string table, params ColumnValuePairing[] insertValues)
        {
            ResetFields();
            IsQuery = false;

            var columns = $"{insertValues[0].Column}";
            var values = new List<object> { insertValues[0].Value };
            var parameters = "@parameter1";

            Parameters.Add(0, insertValues[0].Value);

            for (var i = 1; i < insertValues.Length; i++)
            {
                var insertValue = insertValues[i];

                columns = columns + $", {insertValue.Column}";
                values.Add(insertValue.Value);
                parameters = parameters + $", @parameter{(i+1).ToString()}";

                Parameters.Add(i, insertValue.Value);
            }

            SQL = $"insert into {table} ({columns}) values ({parameters})";

            return this;
        }

        // Starting place
        public PostgreSQLConnection Update(string table, ColumnValuePairing setValue)
        {
            ResetFields();
            IsQuery = false;

            SQL = $"update {table} set {setValue.Column} = @parameter1";
            Parameters.Add(0, setValue.Value);

            return this;
        }

        // Starting place
        public PostgreSQLConnection Delete(string table)
        {
            ResetFields();
            IsQuery = false;

            SQL = $"delete from {table}";

            return this;
        }

        public List<List<string>> Execute()
        {
            if (IsQuery) {
                return ExecuteQuery(SQL, Parameters.Values.ToArray<object>());
            }
            else {
                ExecuteNonQuery(SQL, Parameters.Values.ToArray<object>());
                return ConnectionUtils.CreateEmptyResultSet(0);
            }
        }

        // pass in sql string with @parameter1 - @parameterN
        private void ExecuteNonQuery(string sql, params object[] parameters) 
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var cmd = AddParameters(new NpgsqlCommand(sql, connection), parameters))
                {
                    cmd.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        // pass in sql string with @parameter1 - @parameterN
        private List<List<string>> ExecuteQuery(string sql, params object[] parameters)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                var results = ConnectionUtils.CreateEmptyResultSet(0);

                using (var cmd = AddParameters(new NpgsqlCommand(sql, connection), parameters))
                {
                    results = ReadDBResults(cmd.ExecuteReader());
                }

                connection.Close();

                return results;
            }
        }

        private NpgsqlCommand AddParameters(NpgsqlCommand cmd, params object[] parameters)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                var param = $"@parameter{(i + 1).ToString()}";

                if (parameters[i] is int)
                {
                    cmd.Parameters.AddWithValue(param, (int)parameters[i]);
                }
                else if (parameters[i] is string)
                {
                    cmd.Parameters.AddWithValue(param, (string)parameters[i]);
                }
                else if (parameters[i] is bool)
                {
                    cmd.Parameters.AddWithValue(param, (bool)parameters[i]);
                }
                else if (parameters[i] is null)
                {
                    cmd.Parameters.AddWithValue(param, DBNull.Value);
                }
            }

            return cmd;
        }

        private List<List<string>> ReadDBResults(NpgsqlDataReader dataReader)
        {
            List<List<string>> results = 
                ConnectionUtils.CreateEmptyResultSet(dataReader.FieldCount);

            while (dataReader.Read())
            {
                for (var col = 0; col < dataReader.FieldCount; col++)
                {
                    results[col].Add(dataReader[col].ToString());
                }
            }

            return results;
        }
    }
}
