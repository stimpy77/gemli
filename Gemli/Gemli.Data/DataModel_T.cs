using System;
using System.Data.Common;
using System.Reflection;
using Gemli.Collections;
using Gemli.Data.Providers;

namespace Gemli.Data
{
    /// <summary>
    /// Wraps a CLR object to make it a database-mapped
    /// <see cref="DataModel"/>.
    /// </summary>
    /// <typeparam name="TEntity">
    /// Any business object with public properties or fields 
    /// that this object is wrapping as a <see cref="DataModel"/>.
    /// </typeparam>
    [Serializable]
    public partial class DataModel<TEntity> : DataModel
    {
        [NonSerialized] private Type _Type = typeof (TEntity);

        /// <summary>
        /// Constructs a <see cref="DataModel{TEntity}"/> wrapper object,
        /// initializing it with the specified 
        /// <typeparamref name="TEntity">type</typeparamref>.
        /// </summary>
        public DataModel() : base(typeof (TEntity), Activator.CreateInstance(typeof (TEntity)))
        {
        }

        /// <summary>
        /// Creates a <see cref="DataModel"/> wrapper for 
        /// the specified <paramref name="modelInstance"/>.
        /// </summary>
        /// <param name="modelInstance"></param>
        public DataModel(TEntity modelInstance)
            : base(typeof (TEntity), modelInstance)
        {
        }

        /// <summary>
        /// Exposes inner data dictionary to public scope.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new object this[string key]
        {
            get { return base[key]; }
            set { base[key] = value; }
        }

        /// <summary>
        /// Gets the unwrapped CLR object that this <see cref="DataModel"/> wrapper
        /// represents to the database.
        /// </summary>
        public new TEntity Entity
        {
            get { return (TEntity) base.Entity; }
            set { base.Entity = value; }
        }

        private Type Type
        {
            get { return _Type ?? (_Type = typeof (TEntity)); }
        }

        private new CaseInsensitiveDictionary<object> InnerData
        {
            get { return base.InnerData; }
        }

        /// <summary>
        /// Prepares the 
        /// data to propagate to or from the <see cref="Entity"/> before 
        /// the Entity is retrieved or before data changes are made and/or saved.
        /// </summary>
        /// <param name="syncTo"></param>
        public override void SynchronizeFields(SyncTo syncTo)
        {
            if (Type.IsDataModel() &&
                Type == GetUnwrappedType(Type)) return;
            switch (syncTo)
            {
                case SyncTo.FieldMappedData:
                    foreach (var field_kvp in EntityMappings.FieldMappings)
                    {
                        DataModelColumnAttribute field = field_kvp.Value;

                        // todo: optimize with cached Action<T>
                        // see: http://msmvps.com/blogs/jon_skeet/archive/2008/08/09/making-reflection-fly-and-exploring-delegates.aspx

                        if (field.TargetMember is FieldInfo)
                        {
                            object val = ((FieldInfo) field.TargetMember).GetValue(Entity);
                            object dval = this[field.TargetMember.Name];
                            if (((dval == null) != (val == null)) ||
                                (dval != null && !dval.Equals(val)))
                            {
                                this[field.TargetMember.Name] = val;
                            }
                        }
                        else if (field.TargetMember is PropertyInfo)
                        {
                            object val = ((PropertyInfo) field.TargetMember).GetValue(Entity, new object[] {});
                            if (val != null && !DataModelMap.TypeIsFieldMappable(val.GetType()))
                            {
                                if (DataModelMap.MapItems.ContainsKey(val.GetType()) &&
                                    DataModelMap.GetEntityMapping(val.GetType()).PrimaryKeyColumns.Length==1)
                                {
                                    var map = DataModelMap.GetEntityMapping(val.GetType());
                                    if (val is DataModel)
                                        val =
                                            ((DataModel)val).ColumnMappedValue[
                                                ((DataModel)val).EntityMappings.PrimaryKeyColumns[0]];
                                    else val = val.GetType().GetProperty(
                                        map.PrimaryKeyColumns[0]).GetValue(val, new object[] { });
                                }
                            }
                            object dval = InnerData.ContainsKey(field.TargetMember.Name) 
                                ? this[field.TargetMember.Name]
                                : null;
                            if (((dval == null) != (val == null)) ||
                                (dval != null && !dval.Equals(val)))
                            {
                                if (this.ModelData.ContainsKey(field.TargetMember.Name))
                                {
                                    this[field.TargetMember.Name] = val;
                                }
                                else
                                {
                                    var mapping = EntityMappings[field.TargetMember.Name];
                                    if (mapping != null)
                                    {
                                        var name = mapping.TargetMember.Name;
                                        if (this.ModelData.ContainsKey(name))
                                        {
                                            this[name] = val;
                                        }
                                        else if (this.ModelData.ContainsKey("field:" + mapping.ColumnName))
                                        {
                                            this["field:" + mapping.ColumnName] = val;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case SyncTo.ClrMembers:
                    foreach (var field_kvp in EntityMappings.FieldMappings)
                    {
                        DataModelColumnAttribute field = field_kvp.Value;

                        // todo: optimize with cached Action<T>
                        // see: http://msmvps.com/blogs/jon_skeet/archive/2008/08/09/making-reflection-fly-and-exploring-delegates.aspx

                        if (field.TargetMember is FieldInfo)
                        {
                            object clrval = ((FieldInfo) field.TargetMember).GetValue(Entity);
                            object dicval = this[field.TargetMember.Name];
                            if (((clrval == null) != (dicval == null)) ||
                                (dicval != null && !dicval.Equals(clrval)))
                            {
                                ((FieldInfo) field.TargetMember).SetValue(
                                    Entity, this[field.TargetMember.Name]);
                            }
                        }
                        else if (field.TargetMember is PropertyInfo)
                        {
                            if (DataModelMap.TypeIsFieldMappable(field.TargetMemberType))
                            {
                                object propval = ((PropertyInfo) field.TargetMember).GetValue(Entity, new object[] {});
                                object dicval = this[field.TargetMember.Name];
                                if (dicval == DBNull.Value) dicval = null;
                                if (((dicval == null) != (propval == null)) ||
                                    (dicval != null && !dicval.Equals(propval)))
                                {
                                    ((PropertyInfo) field.TargetMember).SetValue(
                                        Entity, dicval, new object[] {});
                                }
                            } else
                            {
                                // todo maybe: assign loaded entity?
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Saves the specified entity using the application default provider.
        /// </summary>
        /// <param name="entity"></param>
        public static DataModel<TEntity> Save(TEntity entity)
        {
            var model = new DataModel<TEntity>(entity);
            model.Save();
            return model;
        }

        /// <summary>
        /// Saves the specified entity using the application default provider.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="deep"></param>
        /// <returns></returns>
        public static DataModel<TEntity> Save(TEntity entity, bool deep)
        {
            var model = new DataModel<TEntity>(entity);
            model.Save(deep);
            return model;
        }

        /// <summary>
        /// Saves the specified entity using the application default provider.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="deep"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModel<TEntity> Save(TEntity entity, bool deep, DbTransaction transactionContext)
        {
            var model = new DataModel<TEntity>(entity);
            model.Save(deep, transactionContext);
            return model;
        }

        /// <summary>
        /// Saves the specified entity using the application default provider.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModel<TEntity> Save(TEntity entity, DbTransaction transactionContext)
        {
            var model = new DataModel<TEntity>(entity);
            model.Save(transactionContext);
            return model;
        }

        /// <summary>
        /// Saves the specified entity using the application default provider.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="provider"></param>
        public static DataModel<TEntity> Save(TEntity entity, DataProviderBase provider)
        {
            var model = new DataModel<TEntity>(entity);
            provider.SaveModel(model);
            return model;
        }

        /// <summary>
        /// Saves the specified entity using the specified data provider.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="provider"></param>
        /// <param name="deep"></param>
        /// <returns></returns>
        public static DataModel<TEntity> Save(TEntity entity, DataProviderBase provider, bool deep)
        {
            var model = new DataModel<TEntity>(entity);
            if (deep) provider.DeepSaveModel(model);
            else provider.SaveModel(model);
            return model;
        }

        /// <summary>
        /// Saves the specified entity using the specified data provider.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="provider"></param>
        /// <param name="deep"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModel<TEntity> Save(TEntity entity, DataProviderBase provider, bool deep, DbTransaction transactionContext)
        {
            var model = new DataModel<TEntity>(entity);
            if (deep) provider.DeepSaveModel(model, transactionContext);
            else provider.SaveModel(model, transactionContext);
            return model;
        }

        /// <summary>
        /// Saves the specified entity using the specified data provider.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModel<TEntity> Save(TEntity entity, DataProviderBase provider, DbTransaction transactionContext)
        {
            var model = new DataModel<TEntity>(entity);
            provider.SaveModel(model, transactionContext);
            return model;
        }

        /// <summary>
        /// Creates and returns a new <see cref="DataModelQuery&lt;T&gt;"/> object
        /// that is typed for this wrapper.
        /// </summary>
        /// <returns></returns>
        public new static DataModelQuery<DataModel<TEntity>> NewQuery()
        {
            return new DataModelQuery<DataModel<TEntity>>();
        }


        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public new static DataModel<TEntity> Load<TModel>(DataModelQuery<TModel> query)
            where TModel : DataModel<TEntity>
        {
            return Load(query, false, null, null);
        }

        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public new static DataModel<TEntity> Load<TModel>(DataModelQuery<TModel> query, DataProviderBase provider)
            where TModel : DataModel<TEntity>
        {
            return Load(query, false, provider, null);
        }

        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public new static DataModel<TEntity> Load<TModel>(DataModelQuery<TModel> query, bool deep,
                                                          DataProviderBase provider) where TModel : DataModel<TEntity>
        {
            return Load(query, deep, provider, null);
        }

        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public new static DataModel<TEntity> Load<TModel>(DataModelQuery<TModel> query, DataProviderBase provider,
                                                          DbTransaction transactionContext)
            where TModel : DataModel<TEntity>
        {
            return Load(query, false, provider, transactionContext);
        }

        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public new static DataModel<TEntity> Load<TModel>(DataModelQuery<TModel> query, bool deep,
                                                          DataProviderBase provider, DbTransaction transactionContext)
            where TModel : DataModel<TEntity>
        {
            if (provider == null) provider = ProviderDefaults.AppProvider;
            if (deep) return Load(query, null, provider, transactionContext);
            TModel ret = provider.LoadModel(query, transactionContext);
            return ret;
        }

        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="depth"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public new static DataModel<TEntity> Load<TModel>(DataModelQuery<TModel> query, int? depth,
                                                          DataProviderBase provider, DbTransaction transactionContext)
            where TModel : DataModel<TEntity>
        {
            TModel ret;
            if (depth.HasValue) ret = provider.DeepLoadModel(query, depth, transactionContext);
            else ret = provider.DeepLoadModel(query, transactionContext);
            return ret;
        }

        /// <summary>
        /// Loads all instances of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadAll()
        {
            var query = new DataModelQuery<DataModel<TEntity>>();
            return query.SelectMany();
        }

        /// <summary>
        /// Loads all instances of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadAll(DbDataProvider provider)
        {
            var query = new DataModelQuery<DataModel<TEntity>>();
            return query.SelectMany(provider);
        }

        /// <summary>
        /// Loads all instances of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadAll(DbDataProvider provider, DbTransaction transactionContext)
        {
            var query = new DataModelQuery<DataModel<TEntity>>();
            return query.SelectMany(provider, transactionContext);
        }

        /// <summary>
        /// Loads all instances of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadAll(DbTransaction transactionContext)
        {
            var query = new DataModelQuery<DataModel<TEntity>>();
            return query.SelectMany(transactionContext);
        }

        /// <summary>
        /// Loads all instances of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="deep"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadAll(bool deep)
        {
            var query = new DataModelQuery<DataModel<TEntity>>();
            return query.SelectMany(deep);
        }

        /// <summary>
        /// Loads all instances of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadAll(bool deep, DbDataProvider provider)
        {
            var query = new DataModelQuery<DataModel<TEntity>>();
            return query.SelectMany(deep, provider);
        }

        /// <summary>
        /// Loads all instances of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadAll(bool deep, DbDataProvider provider, DbTransaction transactionContext)
        {
            var query = new DataModelQuery<DataModel<TEntity>>();
            return query.SelectMany(deep, provider, transactionContext);
        }

        /// <summary>
        /// Loads all instances of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadAll(bool deep, DbTransaction transactionContext)
        {
            var query = new DataModelQuery<DataModel<TEntity>>();
            return query.SelectMany(deep, transactionContext);
        }
        
        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadMany(DataModelQuery<DataModel<TEntity>> query)
        {
            return LoadMany(query, false, null, null);
        }

        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadMany(DataModelQuery<DataModel<TEntity>> query, DataProviderBase provider)
        {
            return LoadMany(query, false, provider, null);
        }

        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadMany(DataModelQuery<DataModel<TEntity>> query, bool deep,
                                                          DataProviderBase provider)
        {
            return LoadMany(query, deep, provider, null);
        }

        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadMany(DataModelQuery<DataModel<TEntity>> query, DataProviderBase provider,
                                                          DbTransaction transactionContext)
        {
            return LoadMany(query, false, provider, transactionContext);
        }

        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadMany(DataModelQuery<DataModel<TEntity>> query, bool deep,
                                                          DataProviderBase provider, DbTransaction transactionContext)
        {
            if (provider == null) provider = ProviderDefaults.AppProvider;
            if (deep) return LoadMany(query, null, provider, transactionContext);
            var col = provider.LoadModels(query, transactionContext);
            var ret = new DataModelCollection<DataModel<TEntity>>(col);
            return ret;
        }

        /// <summary>
        /// Loads an instance of <see cref="DataModel{TEntity}"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="depth"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<DataModel<TEntity>> LoadMany(DataModelQuery<DataModel<TEntity>> query, int? depth,
                                                          DataProviderBase provider, DbTransaction transactionContext)
        {
            DataModelCollection<DataModel<TEntity>> col;
            if (depth.HasValue) col = provider.DeepLoadModels(query, depth, transactionContext);
            else col = provider.DeepLoadModels(query, transactionContext);
            var ret = new DataModelCollection<DataModel<TEntity>>(col);
            return ret;
        }
    }
}