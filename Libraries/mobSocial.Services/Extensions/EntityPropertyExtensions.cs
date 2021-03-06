﻿using System;
using System.Collections.Generic;
using System.Linq;
using mobSocial.Core.Data;
using mobSocial.Core.Infrastructure.AppEngine;
using mobSocial.Data.Entity.EntityProperties;
using mobSocial.Data.Extensions;
using mobSocial.Data.Interfaces;
using mobSocial.Services.EntityProperties;
using Newtonsoft.Json;

namespace mobSocial.Services.Extensions
{
    public static class EntityPropertyExtensions
    {
        /// <summary>
        /// Gets the properties of entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IList<EntityProperty> GetProperties<T>(this IHasEntityProperties<T> entity) where T: BaseEntity
        {
            var entityPropertyService = mobSocialEngine.ActiveEngine.Resolve<IEntityPropertyService>();
            var typeName = entity.GetUnproxiedTypeName();
            return entityPropertyService.Get(x => x.EntityName == typeName && x.EntityId == entity.Id, null).ToList();
        }
        /// <summary>
        /// Gets the property with specified name for current entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static EntityProperty GetProperty<T>(this IHasEntityProperties<T> entity, string propertyName) where T : BaseEntity
        {
            var entityPropertyService = mobSocialEngine.ActiveEngine.Resolve<IEntityPropertyService>();
            var typeName = entity.GetUnproxiedTypeName();
            return
                entityPropertyService.Get(
                    x => x.EntityName == typeName && x.EntityId == entity.Id && x.PropertyName == propertyName,
                    null).FirstOrDefault();
        }

        /// <summary>
        /// Gets the property valueas stored
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetPropertyValue<T>(this IHasEntityProperties<T> entity, string propertyName) where T : BaseEntity
        {
            var entityProperty = GetProperty(entity, propertyName);
            return entityProperty?.Value;
        }

        /// <summary>
        /// Gets property value as the target type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetPropertyValueAs<T>(this IHasEntityProperties entity, string propertyName, T defaultValue = default(T))
        {
            var entityPropertyService = mobSocialEngine.ActiveEngine.Resolve<IEntityPropertyService>();
            var typeName = entity.GetUnproxiedTypeName();
            if (typeName == null)
                return defaultValue;
            var entityProperty =  entityPropertyService.Get(
                    x => x.EntityName == typeName && x.EntityId == entity.Id && x.PropertyName == propertyName,
                    null).FirstOrDefault();

            if (entityProperty == null)
                return defaultValue;
            try
            {
                return JsonConvert.DeserializeAnonymousType(entityProperty.Value, defaultValue);
            }
            catch
            {
                return (T) Convert.ChangeType(entityProperty.Value, typeof(T));
            }
        }
        /// <summary>
        /// Sets the property value of the provided entity
        /// </summary>
        public static void SetPropertyValue<T>(this IHasEntityProperties<T> entity, string propertyName, object value)
            where T : BaseEntity
        {
            var typeName = entity.GetUnproxiedTypeName();
            //does this property exist?
            var property = GetProperty(entity, propertyName) ?? new EntityProperty()
            {
                EntityId = entity.Id,
                EntityName = typeName,
                PropertyName = propertyName
            };


            property.Value = JsonConvert.SerializeObject(value);
            var entityPropertyService = mobSocialEngine.ActiveEngine.Resolve<IEntityPropertyService>();
            if (property.Id == 0)
                entityPropertyService.Insert(property);
            else
                entityPropertyService.Update(property);
        }

        public static void DeleteProperty<T>(this IHasEntityProperties<T> entity, string propertyName, string propertyValue = null)
            where T : BaseEntity
        {
            var entityPropertyService = mobSocialEngine.ActiveEngine.Resolve<IEntityPropertyService>();
            var typeName = entity.GetUnproxiedTypeName();
            if (propertyValue == null)
                entityPropertyService.Delete(x => x.EntityName == typeName && x.PropertyName == propertyName && x.EntityId == entity.Id);
            else
                entityPropertyService.Delete(x => x.EntityName == typeName && x.PropertyName == propertyName && x.Value == propertyValue && x.EntityId == entity.Id);
        }
    }
}