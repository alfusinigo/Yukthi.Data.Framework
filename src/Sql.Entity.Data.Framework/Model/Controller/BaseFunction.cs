using System;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Controller
{
    /// <summary>
    /// Can be derived to add more custom functions
    /// </summary>
    [Serializable]
    public class BaseFunction
    {
        //General Database BaseFunction
        public const string Create = "CREATE";
        public const string Update = "UPDATE";
        public const string DeleteById = "DELETE_BY_ID";
        public const string GetById = "GET_BY_ID";
        public const string GetAll = "GET_ALL";
    }
}
