using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ApplicationCache.Common;

namespace ApplicationCache.Common.Helpers
{
    public class ExcludeNavigationPropertiesResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> jsonProperties = base.CreateProperties(type, memberSerialization);
            if (type.UnderlyingSystemType.Name == "DTOCachedObjectWrapper`1")
            {
                return jsonProperties.ToList<JsonProperty>();
            }
            List<Type> types = new List<Type>()
            {
                typeof(string),
                typeof(int),
                typeof(int?),
                typeof(DateTime),
                typeof(DateTime?),
                typeof(bool),
                typeof(bool?),
                typeof(Enum)
            };
            List<Type> types1 = types;
            IList<JsonProperty> list = (
                from p in jsonProperties
                where p.Writable
                where types1.Contains(p.PropertyType)
                select p).ToList<JsonProperty>();
            return list;
        }
    }
}