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
    public delegate object DatabaseMethod(IBaseContext context, List<IDataParameter> parameterCollection, Type returnEntityType);

    public interface IDataMapper
    {
        IEnumerable<IBaseContext> GetDataItems(IBaseContext context, Type returnEntityType);
        IBaseContext GetDataItem(IBaseContext context, Type returnEntityType);

        IDataReader GetReader(IBaseContext context);

        object SubmitData(IBaseContext context);
        void SubmitData(IEnumerable<IBaseContext> entities);

        void SetFunctionSpecificEntityMappings(IBaseContext context);
    }
}
