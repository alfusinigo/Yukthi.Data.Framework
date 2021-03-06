﻿using System.Collections.Generic;
using System.Data;

namespace Yc.Sql.Entity.Data.Core.Framework.Cache
{
    public interface ICacheRepository
    {
        string this[string commandText, IEnumerable<IDataParameter> parameterCollection] { get; set; }
        string this[string commandText, string dependantTableNamesCsv, IEnumerable<IDataParameter> parameterCollection] { get; set; }

        bool ContainsValue(string commandText, IEnumerable<IDataParameter> parameterCollection);
        bool ContainsValue(string commandText, string dependantTableNamesCsv, IEnumerable<IDataParameter> parameterCollection);
    }
}