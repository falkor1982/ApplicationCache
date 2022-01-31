using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ApplicationCache.Common
{
    [DataContract]
    public class DTOCachedObjectWrapper<T>
    {
        public DTOCachedObjectWrapper(T cachedObject)
        {
            CachedOject = cachedObject;
            CachedTime = DateTime.Now;
        }

        [DataMember]
        public DateTime CachedTime { get; set; }
        [DataMember]
        public T CachedOject { get; set; }
    }
}
