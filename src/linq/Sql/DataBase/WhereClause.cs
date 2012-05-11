﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using Kiss.Utils;

namespace Kiss.Linq.Sql.DataBase
{
    public class WhereClause<T> : IWhere where T : IQueryObject
    {
        private KeyValuePair<ConnectionStringSettings, ConnectionStringSettings> conn;
        private List<string> where_clauses = new List<string>();
        private List<string> set_clauses = new List<string>();

        private static ILogger _logger;
        private static ILogger logger { get { if (_logger == null) _logger = LogManager.GetLogger(typeof(DatabaseContext)); return _logger; } }

        public WhereClause(KeyValuePair<ConnectionStringSettings, ConnectionStringSettings> conn)
        {
            this.conn = conn;
        }

        public int Count()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("select count(1) from {0}", Kiss.QueryObject<T>.GetTableName());

            if (where_clauses.Count > 0)
            {
                sql.Append(" where ");

                sql.Append(StringUtil.CollectionToDelimitedString(where_clauses, " and ", string.Empty));
            }

            string cacheKey = sql.ToString();

            Kiss.QueryObject.QueryEventArgs e = new Kiss.QueryObject.QueryEventArgs()
            {
                Type = typeof(T),
                Sql = cacheKey
            };
            Kiss.QueryObject.OnPreQuery(e);

            if (e.Result != null)
                return (int)e.Result;

            object ret = new DatabaseContext(conn.Key, typeof(T)).ExecuteScalar(sql.ToString());

            if (ret == null || ret is DBNull) return 0;

            int count = Convert.ToInt32(ret);

            Kiss.QueryObject.OnAfterQuery(new Kiss.QueryObject.QueryEventArgs()
            {
                Type = typeof(T),
                Sql = cacheKey,
                Result = count
            });

            return count;
        }

        public void Delete()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("delete from {0}", Kiss.QueryObject<T>.GetTableName());

            if (where_clauses.Count > 0)
            {
                sql.Append(" where ");

                sql.Append(StringUtil.CollectionToDelimitedString(where_clauses, " and ", string.Empty));
            }

            new DatabaseContext(conn.Value, typeof(T)).ExecuteNonQuery(sql.ToString());

            Kiss.QueryObject.OnBatch(typeof(T));
        }

        public void Update()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("update {0}", Kiss.QueryObject<T>.GetTableName());

            if (set_clauses.Count == 0)
                return;

            sql.Append(" set ");

            sql.Append(StringUtil.CollectionToDelimitedString(set_clauses, ",", string.Empty));

            if (where_clauses.Count > 0)
            {
                sql.Append(" where ");

                sql.Append(StringUtil.CollectionToDelimitedString(where_clauses, " and ", string.Empty));
            }

            new DatabaseContext(conn.Value, typeof(T)).ExecuteNonQuery(sql.ToString());

            Kiss.QueryObject.OnBatch(typeof(T));
        }

        public IWhere Set(string column, string value)
        {
            set_clauses.Add(string.Format("{0} = '{1}'", column, value));

            return this;
        }

        public IWhere Where(string where, params object[] args)
        {
            where_clauses.Add(string.Format(where, args));

            return this;
        }

        public t Select<t>(string field)
        {
            if (string.IsNullOrEmpty(field)) return default(t);

            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("select {0} from {1}", field, Kiss.QueryObject<T>.GetTableName());

            if (where_clauses.Count > 0)
            {
                sql.Append(" where ");

                sql.Append(StringUtil.CollectionToDelimitedString(where_clauses, " and ", string.Empty));
            }

            string cacheKey = sql.ToString();

            Kiss.QueryObject.QueryEventArgs e = new Kiss.QueryObject.QueryEventArgs()
            {
                Type = typeof(T),
                Sql = cacheKey
            };
            Kiss.QueryObject.OnPreQuery(e);

            if (e.Result != null)
                return (t)e.Result;

            t value = TypeConvertUtil.ConvertTo<t>(new DatabaseContext(conn.Key, typeof(T)).ExecuteScalar(sql.ToString()));

            Kiss.QueryObject.OnAfterQuery(new Kiss.QueryObject.QueryEventArgs()
            {
                Type = typeof(T),
                Sql = cacheKey,
                Result = value
            });

            return value;
        }

        public DataTable Select(params string[] fields)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("select {0} from {1}", StringUtil.CollectionToCommaDelimitedString(fields), Kiss.QueryObject<T>.GetTableName());

            if (where_clauses.Count > 0)
            {
                sql.Append(" where ");

                sql.Append(StringUtil.CollectionToDelimitedString(where_clauses, " and ", string.Empty));
            }

            string cacheKey = sql.ToString();

            Kiss.QueryObject.QueryEventArgs e = new Kiss.QueryObject.QueryEventArgs()
            {
                Type = typeof(T),
                Sql = cacheKey
            };
            Kiss.QueryObject.OnPreQuery(e);

            if (e.Result != null)
                return (DataTable)e.Result;

            DataTable dt = new DatabaseContext(conn.Key, typeof(T)).ExecuteDataTable(sql.ToString());

            Kiss.QueryObject.OnAfterQuery(new Kiss.QueryObject.QueryEventArgs()
            {
                Type = typeof(T),
                Sql = cacheKey,
                Result = dt
            });

            return dt;
        }
    }
}
