using System;

namespace Gemli.Data
{
    /// <summary>
    /// A condition or filter that is used in a <see cref="DataModelQuery&lt;T&gt;"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataModelQueryCondition<TModel> : IDataModelQueryCondition<TModel> //: DataModelQueryCondition 
        where TModel : DataModel
    {

        /// <summary>
        /// Describes how a database field mapping is matched 
        /// by name--by the CLR property/field or by the database
        /// column name.
        /// </summary>
        public FieldMappingKeyType FindFieldMappingBy { get; set; }

        /// <summary>
        /// Used with the <see cref="FieldMap"/> to determine how to 
        /// identify and access the field mapping metadata.
        /// </summary>
        public string EvalSubject { get; set; }

        /// <summary>
        /// Describes the binary comparison type that
        /// is used on behalf of a particular property/field/column.
        /// </summary>
        public Compare CompareOp { get; set; }

        /// <summary>
        /// The value to be compared against the database.
        /// </summary>
        public object CompareValue { get; set; }

        private DataModelQuery<TModel> _returnQuery;

        /// <summary>
        /// Constructs a QueryCondition&lt;T&gt; object using the specified
        /// parameters for describing the nature and for binding to the query.
        /// </summary>
        /// <param name="mapBy"></param>
        /// <param name="query"></param>
// ReSharper disable SuggestBaseTypeForParameter
        public DataModelQueryCondition(FieldMappingKeyType mapBy, DataModelQuery<TModel> query)
// ReSharper restore SuggestBaseTypeForParameter
        {
            _returnQuery = query;
            FindFieldMappingBy = mapBy;
        }

        private DataModelColumnAttribute _fieldMap;
        /// <summary>
        /// Gets or sets the metadata object that describes the 
        /// entity's database field mapping which is used
        /// to construct this condition in the appropriate database
        /// context.
        /// </summary>
        protected internal DataModelColumnAttribute FieldMap
        {
            get
            {
                if (_fieldMap == null)
                {
                    if (this.FindFieldMappingBy == FieldMappingKeyType.ClrMember)
                        _fieldMap = DataModel.GetMapping<TModel>().FieldMappings[this.EvalSubject];
                    else
                    {
                        var map = DataModelMap.GetEntityMapping(typeof (TModel));
                        foreach (var fieldmap in map.FieldMappings)
                        {
                            if (fieldmap.Value.ColumnName == this.EvalSubject)
                            {
                                _fieldMap = fieldmap.Value;
                                break;
                            }
                        }
                    }
                }
                return _fieldMap;
            }
            set
            {
                _fieldMap = value;
            }
        }


        /// <summary>
        /// Selects the field regarding which a condition can be bound.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual DataModelQueryCondition<TModel> this[string key]
        {
            get
            {
                EvalSubject = key;
                return this;
            }
        }

        /// <summary>
        /// Clears out the internal reference to the originating <see cref="DataModelQuery"/>
        /// to remove the circular reference
        /// and then returns that Query to the caller.
        /// </summary>
        /// <returns></returns>
        protected virtual DataModelQuery<TModel> NullifyAndReturnQuery()
        {
            var q = _returnQuery;
            if (q == null) return null;
            _returnQuery = null;
            q.Conditions.Add(this);
            return q;
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual DataModelQuery<TModel> IsEqualTo(object value)
        {
            CheckForNull(value);
            CompareOp = Compare.Equal;
            CompareValue = value;
            return NullifyAndReturnQuery();
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is not equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual DataModelQuery<TModel> IsNotEqualTo(object value)
        {
            CheckForNull(value);
            CompareOp = Compare.NotEqual;
            CompareValue = value;
            return NullifyAndReturnQuery();
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is greater than the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual DataModelQuery<TModel> IsGreaterThan(object value)
        {
            CheckForNull(value);
            CompareOp = Compare.GreaterThan;
            CompareValue = value;
            return NullifyAndReturnQuery();
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is greater than or equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual DataModelQuery<TModel> IsGreaterThanOrEqualTo(object value)
        {
            CheckForNull(value);
            CompareOp = Compare.GreaterThanOrEqual;
            CompareValue = value;
            return NullifyAndReturnQuery();
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is less than the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual DataModelQuery<TModel> IsLessThan(object value)
        {
            CheckForNull(value);
            CompareOp = Compare.LessThan;
            CompareValue = value;
            return NullifyAndReturnQuery();
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is not equal to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual DataModelQuery<TModel> IsLessThanOrEqualTo(object value)
        {
            CheckForNull(value);
            CompareOp = Compare.LessThanOrEqual;
            CompareValue = value;
            return NullifyAndReturnQuery();
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// matches the LIKE comparison to the specified <paramref name="value"/>.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual DataModelQuery<TModel> IsLike(object value)
        {
            CheckForNull(value);
            CompareOp = Compare.Like;
            CompareValue = value;
            return NullifyAndReturnQuery();
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is null.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <returns></returns>
        public virtual DataModelQuery<TModel> IsNull()
        {
            CompareOp = Compare.Null;
            return NullifyAndReturnQuery();
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is not null.
        /// Note: This returns a Query object for syntax chaining purposes.
        /// </summary>
        /// <returns></returns>
        public virtual DataModelQuery<TModel> IsNotNull()
        {
            CompareOp = Compare.NotNull;
            return NullifyAndReturnQuery();
        }

        private void CheckForNull(object value)
        {
            if (value == null)
            {
                throw new ArgumentException(
                    "DataModelQuery condition comparison was set with a null value;"
                    + " use IsNull or IsNotNull instead.");
            }
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is equal to the specified <paramref name="val"/>.
        /// </summary>
        /// <param name="cond"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static DataModelQuery<TModel> operator ==(DataModelQueryCondition<TModel> cond, object val)
        {
            if (val == null || val == DBNull.Value) return cond.IsNull();
            return cond.IsEqualTo(val);
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is not equal to the specified <paramref name="val"/>.
        /// </summary>
        /// <param name="cond"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static DataModelQuery<TModel> operator !=(DataModelQueryCondition<TModel> cond, object val)
        {
            if (val == null || val == DBNull.Value) return cond.IsNotNull();
            return cond.IsNotEqualTo(val);
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is greater than the specified <paramref name="val"/>.
        /// </summary>
        /// <param name="cond"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static DataModelQuery<TModel> operator >(DataModelQueryCondition<TModel> cond, object val)
        {
            return cond.IsGreaterThan(val);
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is less than the specified <paramref name="val"/>.
        /// </summary>
        /// <param name="cond"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static DataModelQuery<TModel> operator <(DataModelQueryCondition<TModel> cond, object val)
        {
            return cond.IsLessThan(val);
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is greater than or equal to the specified <paramref name="val"/>.
        /// </summary>
        /// <param name="cond"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static DataModelQuery<TModel> operator >=(DataModelQueryCondition<TModel> cond, object val)
        {
            return cond.IsGreaterThanOrEqualTo(val);
        }

        /// <summary>
        /// Specifies a condition where a field value 
        /// is less than or equal to the specified <paramref name="val"/>.
        /// </summary>
        /// <param name="cond"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static DataModelQuery<TModel> operator <=(DataModelQueryCondition<TModel> cond, object val)
        {
            return cond.IsLessThanOrEqualTo(val);
        }

        ///<summary>
        ///</summary>
        ///<param name="other"></param>
        ///<returns></returns>
        public bool Equals(DataModelQueryCondition<TModel> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._fieldMap, _fieldMap);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (DataModelQueryCondition<TModel>)) return false;
            return Equals((DataModelQueryCondition<TModel>) obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (_fieldMap != null ? _fieldMap.GetHashCode() : 0);
        }

        #region IDataModelQueryCondition Members


        IDataModelQueryCondition IDataModelQueryCondition.this[string fieldName]
        {
            get { return this[fieldName]; }
        }

        IDataModelQuery IDataModelQueryCondition.IsEqualTo(object value)
        {
            return IsEqualTo(value);
        }

        IDataModelQuery IDataModelQueryCondition.IsNotEqualTo(object value)
        {
            return IsNotEqualTo(value);
        }

        IDataModelQuery IDataModelQueryCondition.IsGreaterThan(object value)
        {
            return IsGreaterThan(value);
        }

        IDataModelQuery IDataModelQueryCondition.IsGreaterThanOrEqualTo(object value)
        {
            return IsGreaterThanOrEqualTo(value);
        }

        IDataModelQuery IDataModelQueryCondition.IsLessThan(object value)
        {
            return IsLessThan(value);
        }

        IDataModelQuery IDataModelQueryCondition.IsLessThanOrEqualTo(object value)
        {
            return IsLessThanOrEqualTo(value);
        }

        IDataModelQuery IDataModelQueryCondition.IsLike(object value)
        {
            return IsLike(value);
        }

        IDataModelQuery IDataModelQueryCondition.IsNull()
        {
            return IsNull();
        }

        IDataModelQuery IDataModelQueryCondition.IsNotNull()
        {
            return IsNotNull();
        }

        #endregion
    }
}
