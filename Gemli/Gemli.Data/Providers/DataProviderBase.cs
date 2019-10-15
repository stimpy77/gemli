using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Gemli.Reflection;

namespace Gemli.Data.Providers
{
    /// <summary>
    /// When implemented, provides basic CRUD services for 
    /// <see cref="DataModel"/> objects.
    /// </summary>
    public abstract class DataProviderBase
    {
        internal const string DeepSaveModelMethodName = "DeepSaveModel";
        internal const string SaveModelMethodName = "SaveModel";

        /// <summary>
        /// Returns true if the data provider supports the 
        /// creation and/or handling of a DbTransaction.
        /// </summary>
        public abstract bool SupportsTransactions { get; }

        /// <summary>
        /// When implemented, loads the first <see cref="DataModel"/> that
        /// the specified <paramref name="query"/> finds,
        /// within the specified database <paramref name="transactionContext"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public abstract TModel LoadModel<TModel>(DataModelQuery<TModel> query, DbTransaction transactionContext)
            where TModel : DataModel;

        /// <summary>
        /// When implemented, loads the first <see cref="DataModel"/> that
        /// the specified <paramref name="query"/> finds.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public TModel LoadModel<TModel>(DataModelQuery<TModel> query) where TModel : DataModel
        {
            return LoadModel(query, null);
        }

        /// <summary>
        /// When implemented, loads a set of <see cref="DataModel"/> objects
        /// using the specified <paramref name="query"/> within the
        /// specified database <paramref name="transactionContext"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public abstract DataModelCollection<TModel> LoadModels<TModel>(
            DataModelQuery<TModel> query, DbTransaction transactionContext)
            where TModel : DataModel;

        protected IDataModelCollection LoadModels(Type modelType, IDataModelQuery query,
                                                  DbTransaction transactionContext)
        {
            MethodInfo mi = GetType().GetMadeGenericMethod(MethodBase.GetCurrentMethod().Name, new[] {modelType},
                                                           new[] {typeof (DataModelQuery<>), typeof (DbTransaction)});
            return (IDataModelCollection) mi.Invoke(this, new object[] {query, transactionContext});
        }

        /// <summary>
        /// When implemented, loads a set of <see cref="DataModel"/> objects
        /// using the specified <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> LoadModels<TModel>(
            DataModelQuery<TModel> query) where TModel : DataModel
        {
            return LoadModels(query, null);
        }

        /// <summary>
        /// When implemented, saves the changes that were made to the
        /// specified <paramref name="dataModel"/>, within the specified
        /// database <paramref name="transactionContext"/>. 
        /// <remarks>
        /// The specified object must have the Deleted property set to true,
        /// IsDirty evaluated as true, or IsNew evaluated as true, or else
        /// the model will not be saved.
        /// </remarks>
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="transactionContext"></param>
        public abstract void SaveModel<TModel>(TModel dataModel, DbTransaction transactionContext)
            where TModel : DataModel;

        /// <summary>
        /// When implemented, saves the changes that were made to the
        /// specified <paramref name="dataModel"/>. 
        /// <remarks>
        /// The specified object must have the Deleted property set to true,
        /// IsDirty evaluated as true, or IsNew evaluated as true, or else
        /// the model will not be saved.
        /// </remarks>
        /// </summary>
        /// <param name="dataModel"></param>
        public void SaveModel<TModel>(TModel dataModel)
            where TModel : DataModel
        {
            SaveModel(dataModel, null);
            dataModel.DataProvider = this;
        }

        /// <summary>
        /// When implemented, saves the changes that were made to each of the
        /// specified <paramref name="dataModels"/>, within the specified
        /// database <paramref name="transactionContext"/>. 
        /// <remarks>
        /// The specified <paramref name="dataModels"/> must have the Deleted 
        /// property set to true, IsDirty evaluated as true, or IsNew 
        /// evaluated as true, or else the model will not be saved.
        /// </remarks>
        /// </summary>
        /// <param name="dataModels"></param>
        /// <param name="transactionContext"></param>
        public abstract void SaveModels<TModel>(DataModelCollection<TModel> dataModels,
                                                DbTransaction transactionContext) where TModel : DataModel;

        /// <summary>
        /// When implemented, saves the changes that were made to each of the
        /// specified <paramref name="dataModels"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="dataModels"></param>
        public void SaveModels<TModel>(DataModelCollection<TModel> dataModels) where TModel : DataModel
        {
            SaveModels(dataModels, null);
        }

        /// <summary>
        /// When implemented, loads the full object graph for the first
        /// <see cref="DataModel"/> that is returned from the specified
        /// <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public TModel DeepLoadModel<TModel>(DataModelQuery<TModel> query) where TModel : DataModel
        {
            return DeepLoadModel(query, (int?) null);
        }

        /// <summary>
        /// When implemented, loads the full object graph for the first
        /// <see cref="DataModel"/> that is returned from the specified
        /// <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public TModel DeepLoadModel<TModel>(DataModelQuery<TModel> query, int? depth) where TModel : DataModel
        {
            return DeepLoadModel(query, depth, null);
        }


        /// <summary>
        /// When implemented, loads the full object graph for the first
        /// <see cref="DataModel"/> that is returned from the specified
        /// <paramref name="query"/>, within the specified database
        /// <paramref name="transactionContext"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public TModel DeepLoadModel<TModel>(
            DataModelQuery<TModel> query, DbTransaction transactionContext)
            where TModel : DataModel
        {
            return DeepLoadModel(query, null, transactionContext);
        }

        /// <summary>
        /// When implemented, loads the full object graph for the first
        /// <see cref="DataModel"/> that is returned from the specified
        /// <paramref name="query"/>, within the specified database
        /// <paramref name="transactionContext"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public TModel DeepLoadModel<TModel>(
            DataModelQuery<TModel> query, int? depth, DbTransaction transactionContext)
            where TModel : DataModel
        {
            DataModel ret = DeepLoadModel(query, depth, transactionContext, null);
            if (ret is TModel) return (TModel) ret;
            if (ret.Entity is TModel) return (TModel) ret.Entity;
            Type t = typeof (DataModel<>).MakeGenericType(ret.GetType());
            ret = (DataModel) Activator.CreateInstance(t, ret);
            return (TModel) ret;
        }

        private void LoadMember(
            DataModel dataModel,
            DataModelColumnAttribute fieldMapping,
            string memberName,
            DbTransaction transactionContext,
            List<DataModel> loadedModels)
        {
            LoadMember(dataModel, null, fieldMapping, memberName,
                       transactionContext, loadedModels);
        }

        private void LoadMember(
            DataModel dataModel,
            int? depth,
            DataModelColumnAttribute fieldMapping,
            string memberName,
            DbTransaction transactionContext,
            List<DataModel> loadedModels)
        {
            DataModelColumnAttribute field = fieldMapping;
            DataModel e = dataModel;
            ForeignKeyAttribute fkmapping = field.ForeignKeyMapping;
            string fMemberName = field.ForeignKeyMapping.AssignToMember;
            FieldInfo memField = e.Entity.GetType().GetField(fMemberName);
            PropertyInfo memProp = e.Entity.GetType().GetProperty(fMemberName);
            Type memberType = memField != null
                                  ? memField.FieldType
                                  : memProp.PropertyType;
            bool useAssignWrapper = false;
            if (!memberType.IsDataModel())
            {
                useAssignWrapper = true;
            }
            Type dataMemberType;
            dataMemberType = useAssignWrapper
                                 ? typeof (DataModel<>).MakeGenericType(memberType)
                                 : memberType;
            Type queryType = typeof (DataModelQuery<>).MakeGenericType(dataMemberType);
            IDataModelQuery subquery = ((IDataModelQuery) Activator.CreateInstance(queryType))
                .WhereProperty[fkmapping.ForeignEntityProperty].IsEqualTo(e[memberName]);
            if (depth != null) depth = depth - 1;
            DataModel subentity = DeepLoadModel(dataMemberType, subquery, depth, transactionContext, loadedModels);

            if (useAssignWrapper)
                memProp.SetValue(
                    e.Entity, subentity.Entity, new object[] {});
            else memProp.SetValue(e, subentity, new object[] {});
        }


        private TModel DeepLoadModel<TModel>(
            DataModelQuery<TModel> query, DbTransaction transactionContext,
            List<DataModel> loadedModels
            ) where TModel : DataModel
        {
            return DeepLoadModel(query, null, transactionContext, loadedModels);
        }

        /// <summary>
        /// When implemented, loads the full object graph for the first
        /// <see cref="DataModel"/> that is returned from the specified
        /// <paramref name="query"/>, within the specified database
        /// <paramref name="transactionContext"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <param name="depth"></param>
        /// <param name="loadedModels">
        /// Used for keeping recursive loading from resulting
        /// in infinite loops. Evaluate each loaded item from 
        /// a database result set against this collection; if 
        /// there is a match, use the collection item, 
        /// otherwise use the database loaded item, deep-load
        /// it, and add it to this collection.
        /// </param>
        /// <returns></returns>
        public virtual TModel DeepLoadModel<TModel>(
            DataModelQuery<TModel> query, int? depth, DbTransaction transactionContext,
            List<DataModel> loadedModels
            ) where TModel : DataModel
        {
            TModel e = LoadModel(query, transactionContext);
            if (e == null) return e;
            if (loadedModels == null) loadedModels = new List<DataModel>();
            if (loadedModels.Contains(e)) return (TModel) loadedModels[loadedModels.IndexOf(e)];
            foreach (DataModel previouslyLoadedModel in loadedModels)
            {
                if (query.GetType().IsGenericType)
                {
                    Type qt = query.GetType().GetGenericArguments()[0];
                    if (previouslyLoadedModel.GetType().IsOrInherits(qt) &&
                        previouslyLoadedModel.Equals(e) &&
                        previouslyLoadedModel is TModel)
                        return (TModel) previouslyLoadedModel;
                }
            }
            if (!loadedModels.Contains(e)) loadedModels.Add(e);
            foreach (var fe_kvp in e.EntityMappings.ForeignModelMappings)
            {
                if (depth == null || depth > 0)
                {
                    ForeignDataModelAttribute fe = fe_kvp.Value;
                    Type targetEntityType = fe.TargetMemberType;
                    while (targetEntityType.IsGenericType &&
                           targetEntityType.IsOrInherits(typeof (IEnumerable)))
                    {
                        targetEntityType = targetEntityType.GetGenericArguments().Last();
                    }
                    if (!targetEntityType.IsDataModel())
                        targetEntityType = typeof (DataModel<>).MakeGenericType(targetEntityType);
                    Type subQueryType = typeof (DataModelQuery<>).MakeGenericType(targetEntityType);

                    Relationship relationship = fe.Relationship;
                    if (relationship == Relationship.ManyToMany &&
                        string.IsNullOrEmpty(fe.MappingTable))
                    {
                        relationship = Relationship.OneToMany;
                    }
                    switch (relationship)
                    {
                        case Relationship.OneToOne:
                        case Relationship.ManyToOne:
                            IDataModelQuery subQuery = ((IDataModelQuery) Activator.CreateInstance(subQueryType))
                                .WhereColumn[fe.RelatedTableColumn].IsEqualTo(
                                e.ColumnMappedValue[fe.LocalColumn]);

                            DataModel e2 = DeepLoadModel(targetEntityType, subQuery,
                                                         depth == null ? null : depth - 1,
                                                         transactionContext, loadedModels);
                            object e2o = e2;
                            if (!fe.TargetMemberType.IsDataModel())
                            {
                                e2o = (e2).Entity;
                            }
                            if (fe.TargetMember.MemberType == MemberTypes.Field)
                            {
                                ((FieldInfo) fe.TargetMember).SetValue(e.Entity, e2o);
                            }
                            else if (fe.TargetMember.MemberType == MemberTypes.Property)
                            {
                                ((PropertyInfo) fe.TargetMember).SetValue(e.Entity, e2o, new object[] {});
                            }
                            break;
                        case Relationship.OneToMany:
                            IDataModelQuery subQuery2 = ((IDataModelQuery) Activator.CreateInstance(subQueryType))
                                .WhereColumn[fe.RelatedTableColumn].IsEqualTo(
                                e.ColumnMappedValue[fe.LocalColumn]);

                            IDataModelCollection e2c = DeepLoadModels(targetEntityType, subQuery2, depth,
                                                                      transactionContext, loadedModels);
                            object e2ct = Activator.CreateInstance(fe.TargetMemberType);
                            if (e2ct is IList)
                            {
                                bool de = fe.TargetMemberType.IsGenericType &&
                                          fe.TargetMemberType.GetGenericArguments().Last().IsDataModel();
                                foreach (object e2cx in e2c)
                                {
                                    if (de)
                                    {
                                        ((IList) e2ct).Add(e2cx);
                                    }
                                    else
                                    {
                                        object e2cx2 = ((DataModel) e2cx).Entity;
                                        ((IList) e2ct).Add(e2cx2);
                                    }
                                }
                            }
                            else e2ct = ((IList) e2c)[0];
                            if (fe.TargetMember.MemberType == MemberTypes.Field)
                            {
                                ((FieldInfo) fe.TargetMember).SetValue(e.Entity, e2ct);
                            }
                            else if (fe.TargetMember.MemberType == MemberTypes.Property)
                            {
                                ((PropertyInfo) fe.TargetMember).SetValue(e.Entity, e2ct, new object[] {});
                            }
                            break;
                        case Relationship.ManyToMany:
                            if (!fe.TargetMemberType.IsOrInherits(typeof (IList)))
                                throw new InvalidCastException(
                                    "Cannot apply ManyToMany binding to a non-IList property.");
                            Type tleft = fe.DeclaringType;
                            Type tleftEntity = tleft;
                            while (tleftEntity.IsDataModelWrapper(true))
                            {
                                if (tleftEntity.BaseType != (typeof (DataModel<>)).BaseType)
                                    tleftEntity = tleftEntity.BaseType;
                                else tleftEntity = tleftEntity.GetGenericArguments()[0];
                            }
                            Type tright = (fe.TargetMemberType.IsGenericType &&
                                           !fe.TargetMemberType.IsDataModel() &&
                                           fe.TargetMemberType.IsOrInherits(typeof (IList)))
                                              ? fe.TargetMemberType.GetGenericArguments()[0]
                                              :
                                                  fe.TargetMemberType;
                            Type trightEntity = tright;
                            if (!tright.IsDataModel())
                            {
                                tright = typeof (DataModel<>).MakeGenericType(tright);
                            }
                            Type mapType = typeof (DataModelMap.RuntimeMappingTable<,>)
                                .MakeGenericType(
                                // left side of mapping table
                                fe.TargetMember.DeclaringType,
                                // right side
                                tright);
                            var mapObj = (DataModel) Activator.CreateInstance(mapType);
                            mapObj.EntityMappings.TableMapping.Schema = fe.MappingTableSchema ??
                                                                        ProviderDefaults.DefaultSchema;
                            mapObj.EntityMappings.TableMapping.Table = fe.MappingTable ??
                                                                       (string.Compare(trightEntity.Name,
                                                                                       tleftEntity.Name) == -1
                                                                            ? trightEntity.Name +
                                                                              tleftEntity.Name
                                                                            : tleftEntity.Name +
                                                                              trightEntity.Name);
                            DataModelColumnAttribute mapLeftCol = mapObj.EntityMappings.FieldMappings["LeftColumn"];
                            mapLeftCol.ColumnName = fe.LocalColumn;
                            mapLeftCol.DbType = e.EntityMappings
                                .GetFieldMappingByDbColumnName(fe.LocalColumn).DbType;
                            mapLeftCol.TargetMemberType = e.EntityMappings
                                .GetFieldMappingByDbColumnName(fe.LocalColumn).TargetMemberType;

                            Type mapQueryType = typeof (DataModelQuery<>).MakeGenericType(new[] {mapType});
                            var mapQuery = (IDataModelQuery) Activator.CreateInstance(mapQueryType);
                            mapQuery.WhereColumn[fe.LocalColumn].IsEqualTo(
                                e.ColumnMappedValue[fe.LocalColumn]);


                            IDataModelCollection mapdes = LoadModels(mapType, mapQuery, transactionContext);

                            var mappedDEs = new DataModelCollection<DataModel>();
                            foreach (DataModel de in mapdes) // de is a MappingTable<L,R>
                            {
                                var mappedDEQuery = (IDataModelQuery)
                                                    Activator.CreateInstance(typeof (DataModelQuery<>)
                                                                                 .MakeGenericType(targetEntityType));
                                mappedDEQuery.WhereColumn[fe.RelatedTableColumn]
                                    .IsEqualTo(de.ColumnMappedValue[fe.RelatedTableColumn]);

                                DataModel mappedDE = DeepLoadModel(targetEntityType, mappedDEQuery,
                                                                   depth == null ? null : depth - 1,
                                                                   transactionContext, loadedModels);

                                if (mappedDE != null) mappedDEs.Add(mappedDE);
                            }

                            Type mmtargtype = fe.TargetMemberType;
                            var mmtargcol = (IList) Activator.CreateInstance(fe.TargetMemberType);
                            Type mapdeType = null;
                            foreach (DataModel mapde in mappedDEs)
                            {
                                if (mapdeType == null) mapdeType = mapde.GetType();
                                object deinst = mapde;
                                if (mmtargtype.IsGenericType &&
                                    !mmtargtype.GetGenericArguments()[0].IsDataModel())
                                {
                                    deinst = mapde.Entity;
                                }
                                mmtargcol.Add(deinst);
                            }
                            if (fe.TargetMember is FieldInfo)
                                ((FieldInfo) fe.TargetMember).SetValue(e, mmtargcol);
                            else if (fe.TargetMember is PropertyInfo)
                                ((PropertyInfo) fe.TargetMember).SetValue(
                                    e.Entity, mmtargcol, new object[] {});
                            break;
                    }
                }
            }
            foreach (var field_kvp in e.EntityMappings.FieldMappings)
            {
                DataModelColumnAttribute field = field_kvp.Value;
                if (field.IsForeignKey &&
                    field.ForeignKeyMapping.AssignToMember != null)
                {
                    LoadMember(e, field, field_kvp.Key, transactionContext, loadedModels);
                }
            }
            return e;
        }

        protected DataModel DeepLoadModel(Type modelType, IDataModelQuery query,
                                          int? depth, DbTransaction transactionContext, List<DataModel> loadedModels)
        {
            MethodInfo mi = GetType().GetMadeGenericMethod(MethodBase.GetCurrentMethod().Name,
                                                           new[] {modelType}, new[]
                                                                                  {
                                                                                      query.GetType(), typeof (int?),
                                                                                      typeof (DbTransaction),
                                                                                      typeof (List<DataModel>)
                                                                                  });
            return (DataModel) mi.Invoke(this, new object[] {query, depth, transactionContext, loadedModels});
        }

        /// <summary>
        /// When implemented, loads the full object graph for the 
        /// <see cref="DataModel"/> objects that are returned from the specified
        /// <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> DeepLoadModels<TModel>(DataModelQuery<TModel> query) where TModel : DataModel
        {
            return DeepLoadModels(query, (int?) null, null);
        }

        /// <summary>
        /// When implemented, loads the full object graph for the 
        /// <see cref="DataModel"/> objects that are returned from the specified
        /// <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> DeepLoadModels<TModel>(DataModelQuery<TModel> query, int? depth)
            where TModel : DataModel
        {
            return DeepLoadModels(query, depth, null);
        }

        /// <summary>
        /// When implemented, loads the full object graph for the 
        /// <see cref="DataModel"/> objects that are returned from the specified
        /// <paramref name="query"/>, within the specified database
        /// <paramref name="transactionContext"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> DeepLoadModels<TModel>(
            DataModelQuery<TModel> query, DbTransaction transactionContext)
            where TModel : DataModel
        {
            return DeepLoadModels(query, null, transactionContext);
        }

        /// <summary>
        /// When implemented, loads the full object graph for the 
        /// <see cref="DataModel"/> objects that are returned from the specified
        /// <paramref name="query"/>, within the specified database
        /// <paramref name="transactionContext"/>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="depth"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public DataModelCollection<TModel> DeepLoadModels<TModel>(
            DataModelQuery<TModel> query, int? depth, DbTransaction transactionContext)
            where TModel : DataModel
        {
            var lst = new List<DataModel>();
            return (DataModelCollection<TModel>) DeepLoadModels(query, depth, transactionContext, lst);
        }

        protected DataModelCollection<TModel> DeepLoadModels<TModel>(
            DataModelQuery<TModel> query, DbTransaction transactionContext,
            List<DataModel> loadedModels
            ) where TModel : DataModel
        {
            return (DataModelCollection<TModel>) DeepLoadModels(query, null, transactionContext, loadedModels);
        }

        /// <summary>
        /// Loads the full object graph for the 
        /// <see cref="DataModel"/> objects that are returned from the specified
        /// <paramref name="query"/>, within the specified database
        /// <paramref name="transactionContext"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <param name="depth"></param>
        /// <param name="loadedModels">
        /// Used for keeping recursive loading from resulting
        /// in infinite loops. Evaluate each loaded item from 
        /// a database result set against this collection; if 
        /// there is a match, use the collection item, 
        /// otherwise use the database loaded item, deep-load
        /// it, and add it to this collection.
        /// </param>
        /// <returns></returns>
        public virtual DataModelCollection<TModel> DeepLoadModels<TModel>(
            DataModelQuery<TModel> query, int? depth, DbTransaction transactionContext,
            List<DataModel> loadedModels) where TModel : DataModel
        {
            DataModelCollection<TModel> entities = LoadModels(query);
            DataModel e;
            bool hasForeignKey = false;
            if (entities.Count > 0)
            {
                e = entities[0];
                foreach (var field_kvp in e.EntityMappings.FieldMappings)
                {
                    DataModelColumnAttribute field = field_kvp.Value;
                    if (field.IsForeignKey)
                    {
                        hasForeignKey = true;
                        break;
                    }
                }
                if (!hasForeignKey &&
                    (e.EntityMappings.ForeignModelMappings == null ||
                     e.EntityMappings.ForeignModelMappings.Count == 0))
                {
                    return entities;
                }
            }
            for (int i = 0; i < entities.Count; i++)
            {
                if (depth == null || depth > 0)
                {
                    TModel entity = entities[i];
                    if (hasForeignKey && !loadedModels.Contains(entity))
                    {
                        foreach (var field_kvp in entity.EntityMappings.FieldMappings)
                        {
                            DataModelColumnAttribute field = field_kvp.Value;
                            if (field.IsForeignKey && field.ForeignKeyMapping.AssignToMember != null)
                            {
                                LoadMember(entity, field, field_kvp.Key,
                                           transactionContext, loadedModels);
                                loadedModels.Add(entity);
                            }
                        }
                    }
                    if (entity.EntityMappings.ForeignModelMappings != null &&
                        entity.EntityMappings.ForeignModelMappings.Count > 0
                        )
                    {
                        DataModelQuery<TModel> query2 = EntityToIdentifyingQuery(entity);
                        entity = (depth == null)
                                     ? DeepLoadModel(query2, transactionContext, loadedModels)
                                     : DeepLoadModel(query2, depth - 1, transactionContext, loadedModels);
                        //loadedModels.Add(entity);
                        entities[i] = entity;
                    }
                }
            }
            return entities;
        }

        protected IDataModelCollection DeepLoadModels(Type modelType, IDataModelQuery query, int? depth,
                                                      DbTransaction transactionContext, List<DataModel> loadedModels)
        {
            MethodInfo mi = GetType().GetMadeGenericMethod(MethodBase.GetCurrentMethod().Name, new[] {modelType},
                                                           new[]
                                                               {
                                                                   typeof (DataModelQuery<>), typeof (int?),
                                                                   typeof (DbTransaction),
                                                                   typeof (List<DataModel>)
                                                               });
            return (IDataModelCollection) mi.Invoke(this, new object[] {query, depth, transactionContext, loadedModels});
        }

        /// <summary>
        /// Returns a query that identifies the entity.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static DataModelQuery<TModel> EntityToIdentifyingQuery<TModel>(TModel model) where TModel : DataModel
        {
            var query = new DataModelQuery<TModel>();
            bool pkfound = false;
            int pkc = 0;
            if (model.EntityMappings.PrimaryKeyColumns.Length > 0)
            {
                foreach (var field_kvp in model.EntityMappings.FieldMappings)
                {
                    DataModelColumnAttribute field = field_kvp.Value;
                    if (field.IsPrimaryKey)
                    {
                        string memName = field_kvp.Key;
                        pkfound = (model[memName] != null);
                        if (pkfound)
                        {
                            query.WhereProperty[memName].IsEqualTo(model[memName]);
                            pkc++;
                        }
                    }
                }
            }
            if (!pkfound || model.EntityMappings.PrimaryKeyColumns.Length != pkc)
            {
                foreach (var field_kvp in model.EntityMappings.FieldMappings)
                {
                    string memName = field_kvp.Key;
                    query.WhereProperty[memName].IsEqualTo(model[memName]);
                }
            }
            return query;
        }

        /// <summary>
        /// Saves the entire object graph of the specified
        /// <paramref name="entity"/>, where any of its properties
        /// are also <see cref="DataModel"/> objects with
        /// IsDirty, Deleted, or IsNew change state.
        /// </summary>
        /// <param name="entity"></param>
        public void DeepSaveModel(DataModel entity)
        {
            DeepSaveModel(entity, null);
        }

        /// <summary>
        /// Saves the entire object graph of the specified
        /// <paramref name="entity"/>, within the specified
        /// database <paramref name="transactionContext"/>,
        /// where any of its properties
        /// are also <see cref="DataModel"/> objects with
        /// IsDirty, Deleted, or IsNew change state.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transactionContext"></param>
        public virtual void DeepSaveModel<TModel>(TModel entity, DbTransaction transactionContext)
            where TModel : DataModel
        {
            var savedModels = new List<DataModel>();
            DeepSaveModel(entity, transactionContext, savedModels);
        }

        /// <summary>
        /// Saves the entire object graph of the specified
        /// <paramref name="entity"/>, within the specified
        /// database <paramref name="transactionContext"/>,
        /// where any of its properties
        /// are also <see cref="DataModel"/> objects with
        /// IsDirty, Deleted, or IsNew change state.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transactionContext"></param>
        /// <param name="savedModels"></param>
        protected virtual void DeepSaveModel<TModel>(TModel entity, DbTransaction transactionContext,
                                                     List<DataModel> savedModels)
            where TModel : DataModel
        {
            if (savedModels.Contains(entity)) return;
            SaveModel(entity, transactionContext);
            Type t = entity.GetType();
            while (t.IsGenericType && t.IsDataModel())
            {
                t = t.GetGenericArguments()[0];
            }
            FieldInfo[] fis = t.GetFields();
            PropertyInfo[] pis = t.GetProperties();
            var mis = new List<MemberInfo>();
            foreach (FieldInfo fi in fis) mis.Add(fi);
            foreach (PropertyInfo pi in pis) mis.Add(pi);
            foreach (MemberInfo mi in mis)
            {
                object[] fks = mi.GetCustomAttributes(typeof (ForeignKeyAttribute), true);
                object[] fes = mi.GetCustomAttributes(typeof (ForeignDataModelAttribute), true);
                if (fks.Length > 0 || fes.Length > 0)
                {
                    object subentity = null;
                    try
                    {
                        if (mi is FieldInfo) subentity = ((FieldInfo) mi).GetValue(entity);
                        if (mi is PropertyInfo)
                            subentity = ((PropertyInfo) mi).GetValue(entity.Entity, new object[] {});
                    }
                    catch
                    {
                    }
                    if (subentity == null) continue;
                    if (subentity is IEnumerable && !subentity.IsDataModel())
                    {
                        foreach (object enmrsubentity in (IEnumerable) subentity)
                        {
                            object subentityitem = enmrsubentity;
                            if (!enmrsubentity.IsDataModel())
                            {
                                // We'll wrap and save anyway because even though
                                // the entity isn't a DataModel, its properties
                                // might be, and we're doing a deep save.
                                Type seit = subentityitem.GetType();
                                Type subentityitemT = typeof (DataModel<>).MakeGenericType(seit);
                                subentityitem = Activator.CreateInstance(subentityitemT, subentityitem);
                            }
                            //SaveModel((DataModel) subentityitem, transactionContext);
                            // need to use reflection to get a strong type reference
                            MethodInfo sm = GetType().GetMethods().Where(
                                m => m.Name == SaveModelMethodName &&
                                     m.IsGenericMethod && m.GetParameters().Length == 2).First();
                            sm = sm.MakeGenericMethod(subentityitem.GetType());
                            sm.Invoke(this, new[] {subentityitem, transactionContext});

                            savedModels.Add((DataModel) subentityitem);
                        }
                    }
                    else
                    {
                        if (!subentity.IsDataModel())
                        {
                            // We'll wrap and save anyway because even though
                            // the entity isn't a DataModel, its properties
                            // might be, and we're doing a deep save.
                            Type seit = subentity.GetType();
                            Type subentityT = typeof (DataModel<>).MakeGenericType(seit);
                            subentity = Activator.CreateInstance(subentityT, subentity);
                        }
                        //SaveModel((DataModel)subentity, transactionContext);
                        // need to use reflection to get a strong type reference
                        MethodInfo sm = GetType().GetMethods().Where(
                            m => m.Name == SaveModelMethodName &&
                                 m.IsGenericMethod && m.GetParameters().Length == 2).First();
                        sm = sm.MakeGenericMethod(subentity.GetType());
                        sm.Invoke(this, new[] {subentity, transactionContext});
                        savedModels.Add((DataModel) subentity);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the entire object graph of the specified
        /// <see cref="DataModel"/> objects,
        /// where any of their properties
        /// are also <see cref="DataModel"/> objects with
        /// IsDirty, Deleted, or IsNew change state.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="col"></param>
        public void DeepSaveModels<TModel>(DataModelCollection<TModel> col) where TModel : DataModel
        {
            DeepSaveModels(col, null);
        }

        /// <summary>
        /// Saves the entire object graph of the specified
        /// <see cref="DataModel"/> objects, within the specified
        /// database <paramref name="transactionContext"/>,
        /// where any of their properties
        /// are also <see cref="DataModel"/> objects with
        /// IsDirty, Deleted, or IsNew change state.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="col"></param>
        /// <param name="transactionContext"></param>
        public void DeepSaveModels<TModel>(
            DataModelCollection<TModel> col,
            DbTransaction transactionContext)
            where TModel : DataModel
        {
            var savedModels = new List<DataModel>();
            DeepSaveModels(col, transactionContext, savedModels);
        }

        /// <summary>
        /// Saves the entire object graph of the specified
        /// <see cref="DataModel"/> objects, within the specified
        /// database <paramref name="transactionContext"/>,
        /// where any of their properties
        /// are also <see cref="DataModel"/> objects with
        /// IsDirty, Deleted, or IsNew change state.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="col"></param>
        /// <param name="transactionContext"></param>
        /// <param name="savedModels"></param>
        public virtual void DeepSaveModels<TModel>(
            DataModelCollection<TModel> col,
            DbTransaction transactionContext,
            List<DataModel> savedModels)
            where TModel : DataModel
        {
            foreach (TModel entity in col)
            {
                if (savedModels.Contains(entity)) continue;
                entity.SynchronizeFields(SyncTo.FieldMappedData);
                DeepSaveModel(entity, transactionContext);
                savedModels.Add(entity);
            }
        }

        /// <summary>
        /// When implemented, starts a database transaction.
        /// </summary>
        /// <returns></returns>
        public abstract DbTransaction BeginTransaction();

        /// <summary>
        /// When implemented, starts a database transaction
        /// with the specified isolation level.
        /// </summary>
        /// <returns></returns>
        public abstract DbTransaction BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// When implemented, invokes a COUNT command to return the record count
        /// on the appropriate table using the specified <paramref name="query"/>'s conditions.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public long GetCount<TModel>(DataModelQuery<TModel> query) where TModel : DataModel
        {
            return GetCount(query, null);
        }

        /// <summary>
        /// When implemented, invokes a COUNT command to return the record count
        /// on the appropriate table using the specified <paramref name="query"/>'s conditions.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        public abstract long GetCount<TModel>(DataModelQuery<TModel> query, DbTransaction transactionContext)
            where TModel : DataModel;
    }
}