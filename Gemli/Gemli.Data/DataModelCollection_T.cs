using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using Gemli.Data.Providers;

namespace Gemli.Data
{
    /// <summary>
    /// A collection class for lists of <see cref="DataModel"/> instances.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class DataModelCollection<TModel> : Collection<TModel>, 
        IDataModelCollection
        where TModel : DataModel
    {
        /// <summary>
        /// Constructs an empty collection.
        /// </summary>
        public DataModelCollection() { }
        /// <summary>
        /// Constructs the collection using the provided
        /// <paramref name="list"/> as its initial collection
        /// data.
        /// </summary>
        /// <param name="list"></param>
        public DataModelCollection(ICollection list)
            : this()
        {
            foreach (DataModel item in list)
            {
                base.Add((TModel)item);
            }
        }

        /// <summary>
        /// Returns an <see cref="IList"/> that contains
        /// the Entity property of the objects
        /// in the collection.
        /// <remarks>
        /// This is useful if the type is not a <see cref="DataModel"/>.
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public IList Unwrap()
        {
            var instanceType = DataModel.GetUnwrappedType(typeof (TModel));
            if (typeof(TModel) == instanceType)
            {
                return this;
            }
            var lstType = typeof(List<>).MakeGenericType(instanceType);
            var lst = (IList)Activator.CreateInstance(lstType);
            foreach (var obj in this)
            {
                lst.Add(obj.Entity);
            }
            return lst;
        }

        /// <summary>
        /// Returns a <see cref="List{TEntity}"/> that contains
        /// the Entity property of the objects
        /// in the collection.
        /// </summary>
        /// <remarks>
        /// This is useful if the type is not a <see cref="DataModel"/>.
        /// </remarks>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public List<TEntity> Unwrap<TEntity>()
        {
            var ret = new List<TEntity>();
            var items = this.Unwrap();
            foreach (var item in items)
            {
                ret.Add((TEntity)item);
            }
            return ret;
        }

        /// <summary>
        /// Iterates through the items in the collection
        /// and saves their changes.
        /// </summary>
        public void Save()
        {
            Save(null);
        }

        /// <summary>
        /// Iterates through the items in the collection
        /// and saves their changes, within the specified
        /// database <paramref name="transactionContext"/>.
        /// </summary>
        /// <param name="transactionContext"></param>
        public void Save(DbTransaction transactionContext)
        {
            Save(false, transactionContext);
        }

        /// <summary>
        /// Iterates through the items in the collection
        /// and saves their changes. If parameter
        /// <paramref name="deep"/> is true, the entire
        /// object graph is saved where the properties
        /// of each item is itself a <see cref="DataModel"/>.
        /// </summary>
        /// <param name="deep"></param>
        public void Save(bool deep)
        {
            Save(deep, null);
        }

        /// <summary>
        /// Iterates through the items in the collection
        /// and saves their changes, the context of the
        /// database <paramref name="transactionContext"/>. 
        /// If parameter <paramref name="deep"/> is true, the entire
        /// object graph is saved where the properties
        /// of each item is itself a <see cref="DataModel"/>.
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="transactionContext"></param>
        public void Save(bool deep, DbTransaction transactionContext)
        {
            if (DataProvider == null)
            {
                throw new InvalidOperationException("Repository has not been assigned.");
            }
            if (deep)
            {
                DataProvider.DeepSaveModels(this, transactionContext);
            }
            else
            {
                var col = new DataModelCollection<TModel>();
                foreach (var e in this)
                {
                    e.SynchronizeFields(SyncTo.FieldMappedData);
                    if (e.IsNew || e.IsDirty || e.MarkDeleted)
                    {
                        col.Add(e);
                    }
                }
                DataProvider.SaveModels(col, transactionContext);
            }
        }

        /// <summary>
        /// Gets or sets the database provider that 
        /// facilitates CRUD functions for the <see cref="DataModel"/>
        /// objects within this collection.
        /// </summary>
        public DataProviderBase DataProvider
        {
            get { return _DataProvider ?? ProviderDefaults.AppProvider; }
            set { _DataProvider = value; }
        }

        private DataProviderBase _DataProvider;

        /// <summary>
        /// Gets the <see cref="DataModel"/> at the specified
        /// index in the collection.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        DataModel IDataModelCollection.GetDataModelAt(int index)
        {
            return this[index];
        }

        /// Sets (replaces) the <see cref="DataModel"/> at the specified
        /// preexisting index in the collection.
        void IDataModelCollection.SetDataModelAt(int index, DataModel value)
        {
            this[index] = (TModel) value;
        }
    }
    
}
