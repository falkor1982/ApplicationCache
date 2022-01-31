using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCache.Common
{
    public class DTOCacheRetrieveInfo
    {
        public object[] Args { get; set; }

        public Delegate DelegateMethod { get; set; }
    }
}
