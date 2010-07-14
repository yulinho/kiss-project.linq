﻿using System.Data;

namespace Kiss.Linq.Sql.DataBase
{
    /// <summary>
    /// database related stuffs
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string connstring, CommandType cmdType, string sql);

        /// <summary>
        /// ExecuteReader
        /// </summary>
        IDataReader ExecuteReader(string connstring, CommandType cmdType, string sql);

        /// <summary>
        /// sql format provider
        /// </summary>
        IFormatProvider FormatProvider { get; }
    }
}