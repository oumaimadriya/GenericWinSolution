﻿using App.Gwin.Application.Presentation.Messages;
using App.Gwin.Attributes;
using App.Gwin.Entities;
using LinqExtension;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace App.Gwin.Application.BAL
{
    /// <summary>
    /// BaseEntityBAO
    /// this classe is not Abstract bacause it is used in Application Update 
    /// it is inherited by EntityBAO
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseEntityBLO<T> : IBaseBLO where T : BaseEntity
    {
        #region Context
        /// <summary>
        /// Entity Framework context
        /// </summary>
        public virtual DbContext Context { get; set; }
        protected IDbSet<T> DbSet { get; set; }
        #endregion

        #region Entity
        /// <summary>
        /// Entity Type
        /// </summary>
        public Type TypeEntity { get; set; }
        /// <summary>
        /// ConfigEntity Object
        /// </summary>
        public ConfigEntity ConfigEntity { set; get; }
        #endregion


        #region Constructor
        public BaseEntityBLO(DbContext context, Type TypeDBContext)
        {
            // Params
            this.Context = context;
            this.TypeEntity = typeof(T);
            this.ConfigEntity = ConfigEntity.CreateConfigEntity(this.TypeEntity);

            // Create Context instance
            if (TypeDBContext != null)
                this.Context = Activator.CreateInstance(TypeDBContext) as DbContext;
            if (this.Context != null)
                this.DbSet = this.Context.Set<T>();
        }
        public BaseEntityBLO(Type TypeDBContext) : this(null, TypeDBContext) { }
        public BaseEntityBLO(DbContext context) : this(context, null) { }
        #endregion

        #region Business rols
        /// <summary>
        /// This method is overridden to apply the management rules 
        /// Its called after the values of the entity are changed
        /// </summary>
        public virtual void ApplyBusinessRolesAfterValuesChanged(object sender, BaseEntity entity)
        {
            // this member is not abstract because this class is uses and instantiated en Test Project as BAO

            // 
            // Exemple of implémentation : 
            //

            //Role role = entity as Role;
            //string field_name = (string)sender;

            //switch (field_name)
            //{
            //    // Business Role : the role name mut be UperCase
            //    case nameof(role.Name):
            //        {
            //            role.Name = role.Name.ToUpper();
            //        }
            //        break;
            //}


        }
        #endregion

        #region Save
        public virtual int Save(BaseEntity item)
        {
            return this.Save((T)item);
        }
        public virtual int Save(T item)
        {
            // Calculate Order
            CalculateOrder(item);

            // Save
            try
            {
                if (item.Id <= 0) return Insert(item);
                else return Update(item);
            }
            catch (SqlException e)
            {
                this.SQLExceptionTreatment(e);
                return 0;
            }
            catch (DbEntityValidationException e)
            {
                DbEntityValidationExceptionTreatment(e);
                return -1;
            }

        }
        protected virtual int Insert(T item)
        {
            // Business rule: The creation date equals the system date when saving
            item.DateCreation = DateTime.Now;
            // Business rule: Changing the modification date with the current date
            item.DateModification = DateTime.Now;
            this.DbSet.Add(item);
            string state = this.Context.Entry(item).State.ToString();

            return this.Context.SaveChanges();
        }
        protected virtual int Update(T item)
        {
            this.Context.Entry(item).State = EntityState.Modified;
            // Business rule: Changing the modification date with the current date
            item.DateModification = DateTime.Now;
            return this.Context.SaveChanges();
        }
        protected virtual void CalculateOrder(BaseEntity entity)
        {
            if (entity.Ordre == 0)
            {
                int ordre = this.DbSet.Count();
                entity.Ordre = ++ordre;
            }
        }
        #endregion

        #region Delete
        public virtual int Delete(BaseEntity obj)
        {
            return this.Delete(obj.Id);
        }
        public virtual int Delete(Int64 Id)
        {
            var original = DbSet.Find(Id);
            DbSet.Remove(original);
            try
            {
                return Context.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                DbUpdateExceptionTreatment(e);
                return 0;
            }
        }
        public virtual int Count(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = DbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.Count();
        }
        #endregion

        #region Recherche
        /// <summary>
        /// Search, it is not declaring in the interface IBaseRepositoty
        /// </summary>
        /// <param name="startPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="filter"></param>
        /// <param name="order"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual List<T> GetAll(int startPage = 0, int itemsPerPage = 0, Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> order = null, string includeProperties = "")
        {
            IQueryable<T> query = DbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (order != null && (startPage != 0 && itemsPerPage != 0))
            {
                query = order(query).Skip((startPage - 1) * itemsPerPage).Take(itemsPerPage);
            }

            if (order == null && (startPage != 0 && itemsPerPage != 0))
            {
                query = query.OrderByDescending(x => x.Id).Skip((startPage - 1) * itemsPerPage).Take(itemsPerPage);
            }
            return query.ToList<T>();
        }
        public virtual List<object> Recherche(Dictionary<string, object> rechercheInfos, int startPage = 0, int itemsPerPage = 0)
        {
            IQueryable<T> query = DbSet;
            if (rechercheInfos != null)
            {
                foreach (var item in rechercheInfos)
                {
                    var lambda = Extensions.BuildPredicate<T>(item.Key, item.Value);
                    if (lambda != null)
                        query = query.Where(lambda);
                }
            }
            List<object> ls = query.ToList<object>();
            return ls;
        }

        public List<object> GetAll()
        {
            List<T> ls = this.GetAll(0, 0).ToList<T>();
            return ls.ToList<Object>();
        }
        public List<Object> GetAllDetached()
        {

            List<Object> ls = this.DbSet.ToList<Object>();
            foreach (var item in ls)
            {
                this.Context.Entry(item).State = EntityState.Detached;
            }
            return ls;
        }

        public virtual T GetByID(Int64 id)
        {
            return DbSet.Find(id);
        }
        public virtual BaseEntity GetBaseEntityByID(Int64 id)
        {
            return DbSet.Find(id);
        }
        #endregion

        #region  Treatment of EF excretion

        /// <summary>
        /// DbEntityValidationException
        /// </summary>
        /// <param name="ex"></param>
        public void DbEntityValidationExceptionTreatment(DbEntityValidationException ex)
        {
            foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
            {
                // Get entry
                DbEntityEntry entry = item.Entry;

                BaseEntity entity = (BaseEntity)entry.Entity;
                string entityTypeName = entity.ToString();

                // Display or log error messages
                foreach (DbValidationError subItem in item.ValidationErrors)
                {
                    // [fr]
                    // [3-Layer-Violation]
                    string message = string.Format("Erreur : '{0}' \n trouvé dans l'objet : {1}  \n sur la propriété {2}",
                             subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                    MessageToUser.AddMessage(MessageToUser.Category.EntityValidation, message);
                }
            }
        }
        /// <summary>
        /// SQLException
        /// </summary>
        /// <param name="sqlException"></param>
        public void SQLExceptionTreatment(SqlException sqlException)
        {
            if (sqlException != null)
            {
                if (sqlException.Errors.Count > 0)
                {
                    switch (sqlException.Errors[0].Number)
                    {
                        case 547: // Foreign Key violation
                            MessageToUser.AddMessage(MessageToUser.Category.ForeignKeViolation, "");
                            break;
                        default:
                            throw (sqlException);

                    }
                }
            }
            else
            {
                throw (sqlException);
            }
        }
        /// <summary>
        /// DbUpdateException
        /// </summary>
        /// <param name="e"></param>
        public virtual void DbUpdateExceptionTreatment(DbUpdateException e)
        {
            var sqlException = e.GetBaseException() as SqlException;
            if (sqlException != null)
            {
                if (sqlException.Errors.Count > 0)
                {
                    switch (sqlException.Errors[0].Number)
                    {
                        case 547: // Foreign Key violation
                            MessageToUser.AddMessage(MessageToUser.Category.ForeignKeViolation, "");
                            break;
                        default:
                            throw (sqlException);

                    }
                }
            }
            else
            {
                throw (sqlException);
            }
        }
        #endregion

        #region Create Entity Instance
        public virtual object CreateEntityInstance()
        {
            return this.Context.Set<T>().Create();
        }
        #endregion

        #region Create BLO Instance

        /// <summary>
        /// Creating an instance of the Service object from the entity type
        /// </summary>
        /// <param name="TypeEntity">the entity type</param>
        /// <returns></returns>
        public virtual IBaseBLO CreateServiceBLOInstanceByTypeEntity(Type TypeEntity)
        {

            Type TypeEntityService = typeof(BaseEntityBLO<>).MakeGenericType(TypeEntity);
            IBaseBLO EntityService = (IBaseBLO)Activator.CreateInstance(TypeEntityService, this.Context);
            return EntityService;
        }
        /// <summary>
        /// Creating an instance of the Service object from the entity type and Context
        /// </summary>
        /// <param name="TypeEntity">the entity type</param>
        /// <param name="context">the context</param>
        /// <returns></returns>
        public virtual IBaseBLO CreateServiceBLOInstanceByTypeEntityAndContext(Type TypeEntity, DbContext context)
        {

            Type TypeEntityService = typeof(BaseEntityBLO<>).MakeGenericType(TypeEntity);
            IBaseBLO EntityService = (IBaseBLO)Activator.CreateInstance(TypeEntityService, context);
            return EntityService;
        }
        #endregion

        public virtual void Dispose()
        {
            if (this.Context != null)
            {
                this.Context.Dispose();
            }
            if (this.ConfigEntity != null)
                this.ConfigEntity.Dispose();
        }

        #region Static Method
        /// <summary>
        ///  Creating an instance of the Service object from the entity type
        /// </summary>
        /// <param name="TypeEntity">the entity type</param>
        /// <param name="TypeBaseBAO">Type of Base BAL object</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IBaseBLO CreateBLOInstanceByTypeEntity(Type TypeEntity, Type TypeBaseBAO, DbContext context)
        {
            // Test if BLO Exist

            // Create generic Instance if EntiyBLO Not Exist
            Type TypeEntityService = TypeBaseBAO.MakeGenericType(TypeEntity);
            IBaseBLO EntityService = (IBaseBLO)Activator.CreateInstance(TypeEntityService, context);
            return EntityService;
        }

        /// <summary>
        ///  Creating an instance of the BLO
        ///  From EtityBLO id Exist
        ///  if not exist it use generic BaseBLO
        /// </summary>
        /// <param name="TypeEntity">the entity type</param>
        /// <param name="TypeBaseBLO">Type of Base BLO object</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IBaseBLO CreateBLO_Instance(Type TypeEntity, Type TypeBaseBLO)
        {
            // EntityBLO Object
            IBaseBLO EntityBLO = null;

            // Load TypeEntityBLO
            String Name_Entity_BLO = TypeBaseBLO.Namespace + "." + TypeEntity.Name + "BLO";
            Type TypeEntityBLO =  TypeBaseBLO.Assembly.GetType(Name_Entity_BLO);

            // Create EntityBLO Instance if Exist
            if (TypeEntityBLO != null)
            {
                EntityBLO = (IBaseBLO)Activator.CreateInstance(TypeEntityBLO);
            }
            // Create Generic_EntityBLO Instance if EntityBLO not exist
            else
            {
                Type TypeGenericEntityBLO = TypeBaseBLO.MakeGenericType(TypeEntity);
                EntityBLO = (IBaseBLO)Activator.CreateInstance(TypeGenericEntityBLO);
            }

            return EntityBLO;
        }
        #endregion
    }
}
