using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    public interface IDataModelQuery
    {
        IDataModelQueryCondition WhereProperty { get; }
        IDataModelQueryCondition WhereColumn { get; }
    }
}
