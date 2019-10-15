using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    public interface IDataModelQueryCondition<TModel> : IDataModelQueryCondition where TModel : DataModel
    {
        /// <summary>
        /// Selects the field regarding which a condition can be bound.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        DataModelQueryCondition<TModel> this[string key] { get; }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DataModelQuery<TModel> IsEqualTo(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is not equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DataModelQuery<TModel> IsNotEqualTo(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is greater than the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DataModelQuery<TModel> IsGreaterThan(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is greater than or equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DataModelQuery<TModel> IsGreaterThanOrEqualTo(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is less than the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DataModelQuery<TModel> IsLessThan(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is not equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DataModelQuery<TModel> IsLessThanOrEqualTo(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// matches the LIKE comparison to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        DataModelQuery<TModel> IsLike(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is null.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <returns></returns>
        DataModelQuery<TModel> IsNull();

        /// <summary>
        /// Specifies a condition where a field value 
        /// is not null.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <returns></returns>
        DataModelQuery<TModel> IsNotNull();

        ///<summary>
        ///</summary>
        ///<param name="other"></param>
        ///<returns></returns>
        bool Equals(DataModelQueryCondition<TModel> other);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Equals(object obj);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetHashCode();
    }
}
