using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Data
{
    public interface IDataModelQueryCondition
    {
        /// <summary>
        /// Describes how a database field mapping is matched 
        /// by name--by the CLR property/field or by the database
        /// column name.
        /// </summary>
        FieldMappingKeyType FindFieldMappingBy { get; set; }

        /// <summary>
        /// Used with the <see cref="FieldMap"/> to determine how to 
        /// identify and access the field mapping metadata.
        /// </summary>
        string EvalSubject { get; set; }

        /// <summary>
        /// Describes the binary comparison type that
        /// is used on behalf of a particular property/field/column.
        /// </summary>
        Compare CompareOp { get; set; }

        /// <summary>
        /// The value to be compared against the database.
        /// </summary>
        object CompareValue { get; set; }

        IDataModelQueryCondition this[string fieldName] { get; }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IDataModelQuery IsEqualTo(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is not equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IDataModelQuery IsNotEqualTo(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is greater than the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IDataModelQuery IsGreaterThan(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is greater than or equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IDataModelQuery IsGreaterThanOrEqualTo(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is less than the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IDataModelQuery IsLessThan(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is not equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IDataModelQuery IsLessThanOrEqualTo(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// matches the LIKE comparison to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IDataModelQuery IsLike(object value);

        /// <summary>
        /// Specifies a condition where a field value 
        /// is null.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <returns></returns>
        IDataModelQuery IsNull();

        /// <summary>
        /// Specifies a condition where a field value 
        /// is not null.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <returns></returns>
        IDataModelQuery IsNotNull();
    }
}
