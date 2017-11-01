using Yc.Sql.Entity.Data.Core.Framework.Access;
using Yc.Sql.Entity.Data.Core.Framework.Model.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yc.Sql.Entity.Data.Core.Framework.Mapper
{
    public delegate object DatabaseMethod<T>(IBaseContext context, List<IDataParameter> parameterCollection);
    public delegate dynamic DeserializeMethod<T>(string jsonContent);

    public interface IDataMapper
    {
        List<T> GetDataItems<T>(IBaseContext context);
        T GetDataItem<T>(IBaseContext context);

        dynamic SubmitData(IBaseContext context);
        void SubmitData(IEnumerable<IBaseContext> entities);

        void SetFunctionSpecificEntityMappings(IBaseContext context);
    }
}
