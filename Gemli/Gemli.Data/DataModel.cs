using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Xml.Serialization;
using Gemli.Collections;
using Gemli.Data.Providers;

namespace Gemli.Data
{
    /// <summary>
    /// Represents a database table bound entity.
    /// </summary>
    [Serializable]
    public partial class DataModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        ///// <summary>
        ///// Raised after a Collection instance of <see cref="DataModel"/>
        ///// has been created and initialized.
        ///// </summary>
        //public static event EventHandler CollectionCreated;

        private bool _1stInit = true;
        [NonSerialized] private ColumnMappedValueProperty _ColumnMappedValue;
        [NonSerialized] private DataModelConverter _Converter;
        [NonSerialized] private DataProviderBase _DataProvider;
        private CaseInsensitiveDictionary<object> _InnerData;
        private bool _IsDirty;
        private bool _IsNew = true;
        [NonSerialized] private CaseInsensitiveStringList _ModifiedProperties;
        [NonSerialized] private CaseInsensitiveDictionary<object> _OriginalData;
        private bool DataChangeDirtyHandlerInit;
        private bool Loading;

        /// <summary>
        /// Constructs and initializes an empty DataModel object.
        /// </summary>
        public DataModel()
        {
            Initialize(this);
        }

        /// <summary>
        /// Constructs and initializes a typed DataModel object.
        /// </summary>
        /// <param name="type"></param>
        protected DataModel(Type type)
        {
            Initialize(Entity, type);
        }

        /// <summary>
        /// Constructs and initializes a typed DataModel object
        /// and populates the inner data dictionary with the members
        /// of the specified <paramref name="modelInstance"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="modelInstance"></param>
        protected DataModel(Type type, object modelInstance)
        {
            Initialize(modelInstance, type);
        }

        /// <summary>
        /// Returns a list of database-bound CLR properties/fields 
        /// that have been modified since the last time this object
        /// was loaded or saved.
        /// </summary>
        public CaseInsensitiveStringList ModifiedProperties
        {
            get { return _ModifiedProperties; }
            set { _ModifiedProperties = value; }
        }

        /// <summary>
        /// Gets the unwrapped CLR object that this DataModel object
        /// represents to the database.
        /// </summary>
        [XmlIgnore]
        public object Entity
        {
            get { return _Entity ?? this; }
            protected set { _Entity = value; }
        }

        private object _Entity { get; set; }

        /// <summary>
        /// Gets or sets the value identified by the specified 
        /// <paramref name="key"/>, which corresponds to the 
        /// CLR property/field. To perform the same indexing
        /// on the mapped database column name instead, use 
        /// the <see cref="ColumnMappedValue"/> property.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected internal object this[string key]
        {
            get
            {
                if (InnerData.ContainsKey(key))
                {
                    return DbNullToNull(InnerData[key]);
                }
                if (key.ToLower().StartsWith("field:") && key.Length > 6)
                {
                    string prop = key.Substring(6);
                    foreach (var fmap in EntityMappings.FieldMappings)
                    {
                        if (fmap.Value.ColumnName.ToLower() == prop.ToLower())
                        {
                            key = fmap.Key;
                            if (InnerData.ContainsKey(key))
                            {
                                return DbNullToNull(InnerData[key]);
                            }
                            break;
                        }
                    }
                }
                return InnerData[key] = DBNull.Value;
            }
            set
            {
                DataModelColumnAttribute fieldmapping;
                if (EntityMappings.FieldMappings.ContainsKey(key))
                {
                    fieldmapping = EntityMappings.FieldMappings[key];
                } else
                {
                    fieldmapping = EntityMappings.FieldMappings.ToList().Find(f => f.Value.TargetMember.Name == key).Value;
                }
                if ((value == null || value == DBNull.Value) &&
                    !fieldmapping.IsNullable &&
                    !(fieldmapping.IsIdentity && (IsNew || Loading)))
                {
                    throw new ArgumentException("Field \"" + key + "\" is mapped as IsNullable=False, "
                                                + "but a null value was specified.");
                }
                if (DataChanging != null && !Loading)
                {
                    DataChanging(this, new PropertyChangingEventArgs(key));
                }
                InnerData[key] = NullToDbNull(value);
                if (DataChanged != null && !Loading)
                {
                    DataChanged(this, new PropertyChangedEventArgs(key));
                }
            }
        }

        /// <summary>
        /// Returns true if this entity was not loaded from a database record.
        /// </summary>
        public virtual bool IsNew
        {
            get { return _IsNew; }
            set { _IsNew = value; }
        }

        /// <summary>
        /// Returns true if this entity's inner data dictionary has been changed
        /// since it was last saved or loaded.
        /// </summary>
        [XmlIgnore]
        public virtual bool IsDirty
        {
            get { return _IsDirty; }
            set
            {
                bool oldValue = _IsDirty;
                _IsDirty = value;
                if (value != oldValue)
                {
                    if (DirtyStateChanged != null && !Loading)
                        DirtyStateChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to delete this entity the next time it is saved.
        /// </summary>
        public bool MarkDeleted { get; set; }

        /// <summary>
        /// The inner data dictionary for the record.
        /// </summary>
        protected CaseInsensitiveDictionary<object> InnerData
        {
            get { return _InnerData; }
            private set { _InnerData = value; }
        }

        /// <summary>
        /// Gets or sets the underlying model data dictionary.
        /// </summary>
        /// <remarks>
        /// This property is not exposed to Intellisense when
        /// if the binary assembly is referenced.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CaseInsensitiveDictionary<object> ModelData
        {
            get { return InnerData; }
            set { InnerData = value; }
        }

        /// <summary>
        /// Gets or sets the data that was loaded before changes were made.
        /// </summary>
        [XmlIgnore]
        public CaseInsensitiveDictionary<object> OriginalData
        {
            get { return _OriginalData; }
            set { _OriginalData = value; }
        }

        /// <summary>
        /// Returns the database mapping metadata associated with this entity.
        /// </summary>
        [XmlIgnore]
        public DataModelMap EntityMappings { get; protected set; }

        /// <summary>
        /// Returns a utlity for the conversion and migration 
        /// of data between CLR and database on behalf of this entity.
        /// </summary>
        [XmlIgnore]
        public DataModelConverter Convert
        {
            get { return _Converter; }
            private set { _Converter = value; }
        }

        /// <summary>
        /// Gets or sets the value associated with this record based
        /// on the mapped field name rather than the CLR property
        /// name which is the default for default indexer this[].
        /// <example><code>int customerId = myDataModel.ColumnMappedValue["customer_id"];</code></example>
        /// </summary>
        protected internal ColumnMappedValueProperty ColumnMappedValue
        {
            get { return _ColumnMappedValue; }
            set { _ColumnMappedValue = value; }
        }

        /// <summary>
        /// Gets or sets the database provider that is used to manage
        /// this entity's database storage.
        /// </summary>
        [XmlIgnore]
        public DataProviderBase DataProvider
        {
            get { return _DataProvider ?? ProviderDefaults.AppProvider; }
            set { _DataProvider = value; }
        }

        #region INotifyPropertyChanged Members

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { DataChanged += value; }
            remove { DataChanged -= value; }
        }

        #endregion

        #region INotifyPropertyChanging Members

        event PropertyChangingEventHandler INotifyPropertyChanging.PropertyChanging
        {
            add { DataChanging += value; }
            remove { DataChanging -= value; }
        }

        #endregion

        /// <summary>
        /// Raised when one of the inner data dictionary values has been modified,
        /// or else when the object's dirty state has been reset.
        /// </summary>
        public event EventHandler DirtyStateChanged;

        /// <summary>
        /// Raised when the inner data dictionary has changed.
        /// </summary>
        protected event PropertyChangedEventHandler DataChanged;

        /// <summary>
        /// Raised just before the inner data dictionary changes.
        /// </summary>
        protected event PropertyChangingEventHandler DataChanging;

        /// <summary>
        /// Raised just before the data gets loaded from the <see cref="DataProvider"/>.
        /// </summary>
        protected event EventHandler DataLoading;

        /// <summary>
        /// Raised when the data has been loaded from the <see cref="DataProvider"/>.
        /// </summary>
        protected event EventHandler DataLoaded;

        /// <summary>
        /// Raised after an instance of a <see cref="DataModel"/>
        /// has been created and initialized.
        /// </summary>
        public static event EventHandler Created;

        /// <summary>
        /// Loads the metadata (mappings, types, etc) for this DataModel.
        /// </summary>
        /// <param name="type"></param>
        private void PreInitialize(Type type)
        {
            InnerData = new CaseInsensitiveDictionary<object>();
            EntityMappings = GetMapping(type);
            ColumnMappedValue = new ColumnMappedValueProperty(this, EntityMappings);
            Convert = new DataModelConverter(this);

            DataChanging += DataModel_DataChanging;
            DataChanged += DataModel_DataChanged;
            ModifiedProperties = new CaseInsensitiveStringList();
        }

        private void DataModel_DataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!ModifiedProperties.Contains(e.PropertyName))
                ModifiedProperties.Add(e.PropertyName);
        }

        private void DataModel_DataChanging(object sender, PropertyChangingEventArgs e)
        {
            if (!OriginalData.ContainsKey(e.PropertyName))
                OriginalData[e.PropertyName] = this[e.PropertyName];
        }

        /// <summary>
        /// Returns the database mapping metadata for type 
        /// <typeparamref name="T">T</typeparamref>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static DataModelMap GetMapping<T>()
        {
            return GetMapping(typeof (T));
        }

        /// <summary>
        /// Returns the unwrapped type for the specified DataModel type. 
        /// For example, if DataModel&lt;Customer&gt; 
        /// is supplied in the parameter, returns Customer.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Type GetUnwrappedType(Type t)
        {
            if (!t.IsDataModel()) return t;
            if (t.IsGenericType)
            {
                Type[] typeArgs = t.GetGenericArguments();
                if (typeArgs.Length == 1)
                {
                    Type g = typeof (DataModel<>).MakeGenericType(typeArgs);
                    if (g == t)
                    {
                        t = typeArgs[0];
                    }
                }
            }
            return t;
        }

        private static DataModelMap GetMapping(Type t)
        {
            return DataModelMap.GetEntityMapping(t);
        }

        private void DataModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsDirty = true;
        }

        /// <summary>
        /// Initializes the DataModel with the specified <paramref name="modelInstance"/>
        /// as its initial data load.
        /// </summary>
        /// <param name="modelInstance"></param>
        protected void Initialize(object modelInstance)
        {
            Initialize(modelInstance, modelInstance.GetType());
        }

        /// <summary>
        /// Initializes the DataModel with the specified <paramref name="modelInstance"/>
        /// as its initial data load. The <paramref name="type"/> parameter should match
        /// modelInstance.GetType().
        /// </summary>
        /// <param name="modelInstance"></param>
        /// <param name="type"></param>
        private void Initialize(object modelInstance, Type type)
        {
            Entity = (modelInstance != null &&
                      modelInstance is DataModel &&
                      ((DataModel) modelInstance).Entity != null)
                         ? ((DataModel) modelInstance).Entity
                         : modelInstance;
            PreInitialize(type);

            Loading = true;
            foreach (var fieldmap in EntityMappings.FieldMappings)
            {
                if (!fieldmap.Value.DefaultValueDefined &&
                    fieldmap.Value.IsIdentity && !fieldmap.Value.IncludeOnInsert)
                {
                    InnerData[fieldmap.Key] = DBNull.Value;
                }
                else InnerData[fieldmap.Key] = NullToDbNull(fieldmap.Value.DefaultValue);
            }
            SynchronizeFields(SyncTo.FieldMappedData);
            if (OriginalData == null)
            {
                OriginalData = new CaseInsensitiveDictionary<object>(InnerData);
            }
            Loading = false;

            if (!DataChangeDirtyHandlerInit)
            {
                DataChanged += DataModel_PropertyChanged;
                DataChangeDirtyHandlerInit = true;
            }
            if (_1stInit && Created != null)
            {
                Created(this, new EventArgs());
            }
            _1stInit = false;
        }

        /// <summary>
        /// Performs a special reset on this DataModel object
        /// and resets the OriginalData, IsDirty, MarkDeleted,
        /// and IsNew properties.
        /// </summary>
        /// <seealso cref="ResetMode"/>
        /// <param name="resetMode"></param>
        public void Reset(ResetMode resetMode)
        {
            if (resetMode != ResetMode.RetainNotDirty)
            {
                if (resetMode == ResetMode.RevertNotDirty)
                {
                    InnerData = OriginalData;
                    SynchronizeFields(SyncTo.ClrMembers);
                }
                else if (resetMode == ResetMode.ClearAndNew)
                {
                    Initialize(Entity);
                }
            }
            _IsDirty = false;
            MarkDeleted = false;
            _IsNew = _IsNew || resetMode == ResetMode.ClearAndNew;

            OriginalData = new CaseInsensitiveDictionary<object>(InnerData); // hrm I don't remember why this is being done??
        }

        private static object NullToDbNull(object obj)
        {
            if (null == obj) return DBNull.Value;
            return obj;
        }

        private static object DbNullToNull(object obj)
        {
            if (obj == DBNull.Value) return null;
            return obj;
        }

        /// <summary>
        /// Loads data from an <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="dr"></param>
        public void Load(IDataReader dr)
        {
            BeginLoadingData();
            Convert.FromDataReader(dr);
            EndLoadingData();
        }

        /// <summary>
        /// Cache DataTable; use with collection
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="dt"></param>
        internal void Load(IDataReader dr, DataTable dt)
        {
            BeginLoadingData();
            Convert.FromDataReader(dr, dt);
            EndLoadingData();
        }

        /// <summary>
        /// Loads data from a <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr"></param>
        public void Load(DataRow dr)
        {
            BeginLoadingData();
            Convert.FromDataRow(dr);
            EndLoadingData();
        }

        /// <summary>
        /// When overridden with <see cref="DataModel&lt;T&gt;"/>, prepares the 
        /// data to propagate to or from the <see cref="Entity"/> before 
        /// the Entity is retrieved or before data changes are made and/or saved.
        /// </summary>
        /// <param name="syncTo"></param>
        public virtual void SynchronizeFields(SyncTo syncTo)
        {
        }

        private void BeginLoadingData()
        {
            Loading = true;
            if (DataLoading != null)
                DataLoading(this, new EventArgs());
        }

        private void EndLoadingData()
        {
            SynchronizeFields(SyncTo.ClrMembers);
            IsNew = false;
            IsDirty = false;
            Loading = false;
            if (DirtyStateChanged != null)
                DirtyStateChanged(this, new EventArgs());
            OriginalData = new CaseInsensitiveDictionary<object>(InnerData);
            if (DataLoaded != null)
                DataLoaded(this, new EventArgs());
        }


        /// <summary>
        /// Loads an instance of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static TModel Load<TModel>(DataModelQuery<TModel> query) where TModel : DataModel
        {
            return Load(query, false, null, null);
        }

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static TModel Load<TModel>(DataModelQuery<TModel> query, DataProviderBase provider)
            where TModel : DataModel
        {
            return Load(query, false, provider, null);
        }

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static TModel Load<TModel>(DataModelQuery<TModel> query, bool deep, DataProviderBase provider)
            where TModel : DataModel
        {
            return Load(query, deep, provider, null);
        }

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static TModel Load<TModel>(DataModelQuery<TModel> query, DataProviderBase provider,
                                             DbTransaction transactionContext) where TModel : DataModel
        {
            return Load(query, false, provider, transactionContext);
        }

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static TModel Load<TModel>(DataModelQuery<TModel> query, bool deep, DataProviderBase provider,
                                             DbTransaction transactionContext) where TModel : DataModel
        {
            if (provider == null) provider = ProviderDefaults.AppProvider;
            if (deep) return Load(query, null, provider, transactionContext);
            TModel ret = provider.LoadModel(query, transactionContext);
            return ret;
        }

        /// <summary>
        /// Loads an instance of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="depth"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static TModel Load<TModel>(DataModelQuery<TModel> query, int? depth, DataProviderBase provider,
                                             DbTransaction transactionContext) where TModel : DataModel
        {
            TModel ret = depth.HasValue
                             ? provider.DeepLoadModel(query, depth, transactionContext)
                             : provider.DeepLoadModel(query, transactionContext);
            return ret;
        }

        /// <summary>
        /// Loads all instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadAll<TModel>() where TModel : DataModel
        {
            var query = new DataModelQuery<TModel>();
            return query.SelectMany();
        }

        /// <summary>
        /// Loads all instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadAll<TModel>(DbDataProvider provider) where TModel : DataModel
        {
            var query = new DataModelQuery<TModel>();
            return query.SelectMany(provider);
        }

        /// <summary>
        /// Loads all instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadAll<TModel>(DbDataProvider provider, DbTransaction transactionContext) where TModel : DataModel
        {
            var query = new DataModelQuery<TModel>();
            return query.SelectMany(provider, transactionContext);
        }

        /// <summary>
        /// Loads all instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadAll<TModel>(DbTransaction transactionContext) where TModel : DataModel
        {
            var query = new DataModelQuery<TModel>();
            return query.SelectMany(transactionContext);
        }

        /// <summary>
        /// Loads all instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="deep"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadAll<TModel>(bool deep) where TModel : DataModel
        {
            var query = new DataModelQuery<TModel>();
            return query.SelectMany(deep);
        }

        /// <summary>
        /// Loads all instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadAll<TModel>(bool deep, DbDataProvider provider) where TModel : DataModel
        {
            var query = new DataModelQuery<TModel>();
            return query.SelectMany(deep, provider);
        }

        /// <summary>
        /// Loads all instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadAll<TModel>(bool deep, DbDataProvider provider, DbTransaction transactionContext) where TModel : DataModel
        {
            var query = new DataModelQuery<TModel>();
            return query.SelectMany(deep, provider, transactionContext);
        }

        /// <summary>
        /// Loads all instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="deep"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadAll<TModel>(bool deep, DbTransaction transactionContext) where TModel : DataModel
        {
            var query = new DataModelQuery<TModel>();
            return query.SelectMany(deep, transactionContext);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadMany<TModel>(DataModelQuery<TModel> query)
            where TModel : DataModel
        {
            return LoadMany(query, false, null, null);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadMany<TModel>(DataModelQuery<TModel> query,
                                                                   DataProviderBase provider) where TModel : DataModel
        {
            return LoadMany(query, false, provider, null);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadMany<TModel>(DataModelQuery<TModel> query, bool deep,
                                                                   DataProviderBase provider) where TModel : DataModel
        {
            return LoadMany(query, deep, provider, null);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadMany<TModel>(DataModelQuery<TModel> query,
                                                                   DataProviderBase provider,
                                                                   DbTransaction transactionContext)
            where TModel : DataModel
        {
            return LoadMany(query, false, provider, transactionContext);
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="deep"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadMany<TModel>(DataModelQuery<TModel> query, bool deep,
                                                                   DataProviderBase provider,
                                                                   DbTransaction transactionContext)
            where TModel : DataModel
        {
            if (provider == null) provider = ProviderDefaults.AppProvider;
            if (deep) return LoadMany(query, null, provider, transactionContext);
            DataModelCollection<TModel> ret = provider.LoadModels(query, transactionContext);
            return ret;
        }

        /// <summary>
        /// Loads instances of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="depth"></param>
        /// <param name="provider"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public static DataModelCollection<TModel> LoadMany<TModel>(DataModelQuery<TModel> query, int? depth,
                                                                   DataProviderBase provider,
                                                                   DbTransaction transactionContext)
            where TModel : DataModel
        {
            DataModelCollection<TModel> ret = depth.HasValue 
                ? provider.DeepLoadModels(query, depth, transactionContext) 
                : provider.DeepLoadModels(query, transactionContext);
            return ret;
        }

        /// <summary>
        /// Pushes all CLR changes in this entity to the mapped database.
        /// </summary>
        public void Save()
        {
            Save(false, null);
        }

        /// <summary>
        /// Pushes all CLR changes in this entity to the mapped database 
        /// using the specified <paramref name="transactionContext"/>.
        /// </summary>
        /// <param name="transactionContext"></param>
        public void Save(DbTransaction transactionContext)
        {
            Save(false, transactionContext);
        }

        /// <summary>
        /// Pushes all CLR changes in this entity to the mapped database.
        /// If <paramref name="deep"/> is true, invokes
        /// the same Save(true) method on all of this entity's
        /// Entity members that are also DataModel types.
        /// </summary>
        /// <param name="deep"></param>
        public void Save(bool deep)
        {
            Save(deep, null);
        }

        /// <summary>
        /// Pushes all CLR changes in this entity to the mapped database.
        /// If <paramref name="deep"/> is true, invokes
        /// the same Save(true, transactionContext) method on all of this entity's
        /// Entity members that are also DataModel types.
        /// All of this is done within the specified <paramref name="transactionContext"/>.
        /// </summary>
        /// <param name="deep"></param>
        /// <param name="transactionContext"></param>
        public virtual void Save(bool deep, DbTransaction transactionContext)
        {
            SynchronizeFields(SyncTo.FieldMappedData);
            if (IsNew || MarkDeleted || IsDirty)
            {
                if (DataProvider == null)
                {
                    throw new InvalidOperationException("The repository has not been assigned.");
                }
                if (deep)
                {
                    //DataProvider.DeepSaveModel(this, transactionContext);

                    // Have to use reflection to use a strong type reference to the inheriting class.
                    var mi =
                        DataProvider.GetType().GetMethods().Where(
                            x => x.IsGenericMethod && x.Name == DataProviderBase.DeepSaveModelMethodName && x.GetParameters().Length == 2).First();
                    mi = mi.MakeGenericMethod(this.GetType());
                    mi.Invoke(DataProvider, new object[] {this, transactionContext});
                }
                else
                {
                    //DataProvider.SaveModel(this, transactionContext);

                    // Have to use reflection to use a strong type reference to the inheriting class.
                    var mi = DataProvider.GetType().GetMethods().Where(
                        x => x.IsGenericMethod && x.Name == DataProviderBase.SaveModelMethodName && x.GetParameters().Length == 2).First();
                    mi = mi.MakeGenericMethod(this.GetType());
                    mi.Invoke(DataProvider, new object[] { this, transactionContext });
                }
            }
            SynchronizeFields(SyncTo.ClrMembers);
        }

        /// <summary>
        /// Creates a new query.
        /// </summary>
        /// <returns></returns>
        public static DataModelQuery<DataModel> NewQuery()
        {
            return new DataModelQuery<DataModel>();
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (base.Equals(obj)) return true;
            if (!(obj is DataModel)) return false;
            var e = obj as DataModel;

            //this.SynchronizeFields(SyncTo.FieldMappedData);
            //e.SynchronizeFields(SyncTo.FieldMappedData);

            if (IsNew != e.IsNew) return false;
            if (MarkDeleted != e.MarkDeleted) return false;

            if (EntityMappings.TableMapping.Schema != e.EntityMappings.TableMapping.Schema ||
                EntityMappings.TableMapping.Table != e.EntityMappings.TableMapping.Table)
            {
                return false;
            }
            var checkedprops = new CaseInsensitiveStringList();
            foreach (var field_kvp in EntityMappings.FieldMappings)
            {
                string propname = null;
                foreach (var ef_kvp in e.EntityMappings.FieldMappings)
                {
                    if (field_kvp.Value.ColumnName.ToLower() == ef_kvp.Value.ColumnName.ToLower())
                    {
                        propname = field_kvp.Key;
                        checkedprops.Add(propname);
                        break;
                    }
                }
                if (propname == null) return false;
                if ((InnerData == null) != (e.InnerData == null)) return false;
                if (InnerData != null && e.InnerData != null &&
                    InnerData.ContainsKey(propname) != e.InnerData.ContainsKey(propname))
                    return false;
                if (InnerData != null)
                {
                    if (InnerData.ContainsKey(propname))
                    {
                        object thisProp = this[propname];
                        object eProp = e[propname];
                        if ((thisProp == null) != (eProp == null))
                            return false;
                        if (thisProp != null && !(thisProp.Equals(eProp)))
                            return false;
                    }
                }
            }
            foreach (var field_kvp in e.EntityMappings.FieldMappings)
            {
                if (checkedprops.Contains(field_kvp.Key)) continue;
                string propname = null;
                foreach (var ef_kvp in EntityMappings.FieldMappings)
                {
                    if (field_kvp.Value.ColumnName.ToLower() == ef_kvp.Value.ColumnName.ToLower())
                    {
                        propname = field_kvp.Key;
                        break;
                    }
                }
                if (propname == null) return false;
                if ((InnerData == null) != (e.InnerData == null)) return false;
                if (InnerData != null && e.InnerData != null)
                {
                    if (InnerData.ContainsKey(propname) != e.InnerData.ContainsKey(propname))
                        return false;
                    if (InnerData.ContainsKey(propname))
                    {
                        object thisProp = this[propname];
                        object eProp = e[propname];
                        if ((thisProp == null) != (eProp == null))
                            return false;
                        if (thisProp != null && !(thisProp.Equals(eProp)))
                            return false;
                    }
                }
            }
            return true;
        }

        #region Nested type: ColumnMappedValueProperty

        /// <summary>
        /// Provides an indexer for <see cref="ColumnMappedValue"/>.
        /// </summary>
        protected internal class ColumnMappedValueProperty
        {
            private readonly DataModel DataModel;
            private readonly DataModelMap Mapping;

            internal ColumnMappedValueProperty(DataModel dataModel, DataModelMap mapping)
            {
                DataModel = dataModel;
                Mapping = mapping;
            }

            /// <summary>
            /// Gets or sets the value identified by the specified 
            /// <paramref name="key"/>, which corresponds to the 
            /// mapped database column name.
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public object this[string key]
            {
                get
                {
                    foreach (var field_kvp in Mapping.FieldMappings)
                    {
                        DataModelColumnAttribute field = field_kvp.Value;
                        if (field.ColumnName == null)
                        {
                            field.ColumnName = field_kvp.Key;
                        }
                        if (field.ColumnName.ToLower() == key.ToLower())
                        {
                            return DataModel[field_kvp.Key];
                        }
                    }
                    if (DataModel["field:" + key] != null)
                        return DataModel["field:" + key];
                    throw new ArgumentException("Field mapping not found");
                }
                set
                {
                    foreach (var field_kvp in Mapping.FieldMappings)
                    {
                        DataModelColumnAttribute field = field_kvp.Value;
                        if (field.ColumnName.ToLower() == key.ToLower())
                        {
                            DataModel[field_kvp.Key] = value;
                            return;
                        }
                    }
                    var attr = new DataModelColumnAttribute(key)
                                   {
                                       DbType = DbTypeConverter.ToDbType(value.GetType())
                                   };
                    Mapping.FieldMappings.Add("field:" + key, attr);
                    DataModel["field:" + key] = value;
                }
            }
        }

        #endregion
    }
}