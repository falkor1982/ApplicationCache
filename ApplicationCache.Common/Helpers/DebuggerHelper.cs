using System;
using ApplicationCache.Common;
using System.Diagnostics;

namespace ApplicationCache.Common.Helpers
{
    public class DebuggerHelper
    {
        public static void ClearLoadingDataFlagDebug(string cacheCategory, string cacheFilter)
        {
            Debug.WriteLine(string.Format("ClearLoadingDataFlagDebug {0} - {1}", cacheCategory, cacheFilter));
        }

        public static void GetCachedObjectDebug(string cacheCategory, string cacheFilter)
        {
            Debug.WriteLine(string.Format("GetCachedObjectDebug {0} - {1}", cacheCategory, cacheFilter));
        }

        public static void SetCachedObjectDebug(string cacheCategory, string cacheFilter)
        {
            Debug.WriteLine(string.Format("SetCachedObjectDebug {0} - {1}", cacheCategory, cacheFilter));
        }

        public static void SetLoadingDataFlagDebug(string cacheCategory, string cacheFilter, int waitForMeMaxSeconds)
        {
            Debug.WriteLine(string.Format("SetLoadingDataFlagDebug {0} - {1}", cacheCategory, cacheFilter));
        }

        public static void SpecificObjectFromListUsedDebug(string cacheCategory, string cacheFilter)
        {
            Debug.WriteLine(string.Format("SpecificObjectFromListUsedDebug {0} - {1}", cacheCategory, cacheFilter));
        }
    }
}