using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;
using ApplicationCache.Common;

namespace ApplicationCache.Common.Helpers
{
    public class GeneralCacheHelper
    {
        public static void CleanCachedObject<T>(Delegate getFromSourceDelegate, object[] argsFromSource, Delegate getFromListLambdaDelegate, object[] argsFromListLambda)
        {
            string cacheCategoryKey = CacheApplicationStateManager.GetCacheCategoryKey(typeof(T));
            string cacheFilterKey = CacheApplicationStateManager.GetCacheFilterKey(getFromSourceDelegate.Method.Name, argsFromSource);
            CacheApplicationStateManager.CleanCachedObject(cacheCategoryKey, cacheFilterKey);
        }

        public static void EnforceCacheObject<T>(bool isGeneralCacheEnabled, int? maxMinutesValid, List<TimeSpan> timeOfDayExpirations, int waitForFlagMaxSeconds, Delegate getFromSourceDelegate, object[] argsFromSource)
        {
            string cacheCategoryKey = CacheApplicationStateManager.GetCacheCategoryKey(typeof(T));
            string cacheFilterKey = CacheApplicationStateManager.GetCacheFilterKey(getFromSourceDelegate.Method.Name, argsFromSource);
            if (!CacheApplicationStateManager.HasValidCatchedObject<T>(cacheCategoryKey, cacheFilterKey, maxMinutesValid, timeOfDayExpirations))
            {
                GeneralCacheHelper.LoadObject<T>(waitForFlagMaxSeconds, getFromSourceDelegate, argsFromSource, cacheCategoryKey, cacheFilterKey);
            }
        }

        public static void EnforcePermanentCache(List<DTOCacheRetrieveInfo> permanentRequiredCaches, int restIntervalTimeMinutes)
        {
            /*
             * Usage example:
             * DTOCacheRetrieveInfo dtoCacheRetrieveInfo1 = new DTOCacheRetrieveInfo();
             * Func<List<Person>> personList = PersonManager.GetAll;
             * dtoCacheRetrieveInfo1.DelegateMethod = personList;
             * List<DTOCacheRetrieveInfo> list = new List<DTOCacheRetrieveInfo>() { dtoCacheRetrieveInfo1 };
             * GeneralCacheHelper.EnforcePermanentCache(list, 1);
            */

            HttpContext current = HttpContext.Current;
            (new Thread(() => {
                HttpContext.Current = current;
                while (true)
                {
                    foreach (DTOCacheRetrieveInfo permanentRequiredCach in permanentRequiredCaches)
                    {
                        permanentRequiredCach.DelegateMethod.DynamicInvoke(permanentRequiredCach.Args);
                    }
                    Thread.Sleep(restIntervalTimeMinutes * 60 * 1000);
                }
            })).Start();
        }

        public static T GetCacheableEntity<T>(bool isGeneralCacheEnabled, int? maxMinutesValid, List<TimeSpan> timeOfDayExpirations, int waitForFlagMaxSeconds, Delegate getFromSourceDelegate, object[] argsFromSource, RetrieveMethodBehavior cacheMethodBehavior = 0)
        {
            return GeneralCacheHelper.ObjectCachedOrFromListOperationInternal<T>(isGeneralCacheEnabled, maxMinutesValid, timeOfDayExpirations, waitForFlagMaxSeconds, getFromSourceDelegate, argsFromSource, null, null, cacheMethodBehavior);
        }

        public static IList<T> GetCacheableList<T>(bool isGeneralCacheEnabled, int? maxMinutesValid, List<TimeSpan> timeOfDayExpirations, int waitForFlagMaxSeconds, Delegate getListDelegate, object[] argsFromSource, RetrieveMethodBehavior retrieveMethodBehavior)
        {
            IList<T> ts;
            string cacheCategoryKey = CacheApplicationStateManager.GetCacheCategoryKey(typeof(T));
            string cacheFilterKey = CacheApplicationStateManager.GetCacheFilterKey(getListDelegate.Method.Name, argsFromSource);
            if (retrieveMethodBehavior == RetrieveMethodBehavior.ForceLoad || !isGeneralCacheEnabled || !CacheApplicationStateManager.GetCachedObject<IList<T>>(cacheCategoryKey, cacheFilterKey, maxMinutesValid, timeOfDayExpirations, out ts))
            {
                ts = GeneralCacheHelper.LoadObject<IList<T>>(waitForFlagMaxSeconds, getListDelegate, argsFromSource, cacheCategoryKey, cacheFilterKey);
            }
            return ts;
        }

        public static T GetIntelligentCacheableEntityFromList<T>(bool isGeneralCacheEnabled, int? maxMinutesValid, List<TimeSpan> timeOfDayExpirations, int waitForFlagMaxSeconds, Delegate getFromListLambdaDelegate, object[] argsFromListLambda, Delegate getFromSourceDelegate, object[] argsFromSource, RetrieveMethodBehavior cacheMethodBehavior = 0)
        {
            return GeneralCacheHelper.ObjectCachedOrFromListOperationInternal<T>(isGeneralCacheEnabled, maxMinutesValid, timeOfDayExpirations, waitForFlagMaxSeconds, getFromSourceDelegate, argsFromSource, getFromListLambdaDelegate, argsFromListLambda, cacheMethodBehavior);
        }

        public static IList<T> GetIntelligentCacheableListFromList<T>(bool isGeneralCacheEnabled, int? maxMinutesValid, List<TimeSpan> timeOfDayExpirations, int waitForFlagMaxSeconds, Delegate getFromListLambdaDelegate, object[] argsFromListLambda, Delegate getFromSourceDelegate, object[] argsFromSource, RetrieveMethodBehavior cacheMethodBehavior = 0)
        {
            return GeneralCacheHelper.ListObjectsCachedOrFromListOperationInternal<T>(isGeneralCacheEnabled, maxMinutesValid, timeOfDayExpirations, waitForFlagMaxSeconds, getFromSourceDelegate, argsFromSource, getFromListLambdaDelegate, argsFromListLambda, cacheMethodBehavior);
        }

        private static IList<T> ListObjectsCachedOrFromListOperationInternal<T>(bool isGeneralCacheEnabled, int? maxMinutesValid, List<TimeSpan> timeOfDayExpirations, int waitForFlagMaxSeconds, Delegate getFromSourceDelegate, object[] argsFromSource, Delegate getFromListLambdaDelegate, object[] argsFromListLambda, RetrieveMethodBehavior retrieveMethodBehavior)
        {
            IList<T> ts;
            string cacheCategoryKey = CacheApplicationStateManager.GetCacheCategoryKey(typeof(T));
            string cacheFilterKey = CacheApplicationStateManager.GetCacheFilterKey(getFromListLambdaDelegate.Method.Name, argsFromSource);
            if (retrieveMethodBehavior == RetrieveMethodBehavior.ForceLoad || !isGeneralCacheEnabled)
            {
                return (IList<T>)(object)GeneralCacheHelper.LoadObject<T>(waitForFlagMaxSeconds, getFromSourceDelegate, argsFromSource, cacheCategoryKey, cacheFilterKey);
            }
            if (getFromListLambdaDelegate != null && CacheApplicationStateManager.HasValidCatchedObject<IList<T>>(cacheCategoryKey, null, maxMinutesValid, timeOfDayExpirations))
            {
                DebuggerHelper.SpecificObjectFromListUsedDebug(cacheCategoryKey, cacheFilterKey);
                return (IList<T>)getFromListLambdaDelegate.DynamicInvoke(argsFromListLambda);
            }
            if (!CacheApplicationStateManager.GetCachedObject<IList<T>>(cacheCategoryKey, cacheFilterKey, maxMinutesValid, timeOfDayExpirations, out ts))
            {
                ts = GeneralCacheHelper.LoadObject<IList<T>>(waitForFlagMaxSeconds, getFromSourceDelegate, argsFromSource, cacheCategoryKey, cacheFilterKey);
            }
            return ts;
        }

        private static T LoadObject<T>(int waitForFlagMaxSeconds, Delegate getFromSourceDelegate, object[] argsFromSource, string cacheCategory, string cacheFilter)
        {
            CacheApplicationStateManager.SetLoadingDataFlag(cacheCategory, cacheFilter, waitForFlagMaxSeconds);
            T t = (T)getFromSourceDelegate.DynamicInvoke(argsFromSource);
            CacheApplicationStateManager.SetCachedObject(cacheCategory, cacheFilter, new DTOCachedObjectWrapper<T>(t));
            return t;
        }

        private static T ObjectCachedOrFromListOperationInternal<T>(bool isGeneralCacheEnabled, int? maxMinutesValid, List<TimeSpan> timeOfDayExpirations, int waitForFlagMaxSeconds, Delegate getFromSourceDelegate, object[] argsFromSource, Delegate getFromListLambdaDelegate, object[] argsFromListLambda, RetrieveMethodBehavior retrieveMethodBehavior)
        {
            T t;
            string cacheCategoryKey = CacheApplicationStateManager.GetCacheCategoryKey(typeof(T));
            string cacheFilterKey = CacheApplicationStateManager.GetCacheFilterKey(getFromSourceDelegate.Method.Name, argsFromSource);
            if (retrieveMethodBehavior == RetrieveMethodBehavior.ForceLoad || !isGeneralCacheEnabled)
            {
                return GeneralCacheHelper.LoadObject<T>(waitForFlagMaxSeconds, getFromSourceDelegate, argsFromSource, cacheCategoryKey, cacheFilterKey);
            }
            if (getFromListLambdaDelegate != null && CacheApplicationStateManager.HasValidCatchedObject<IList<T>>(cacheCategoryKey, null, maxMinutesValid, timeOfDayExpirations))
            {
                DebuggerHelper.SpecificObjectFromListUsedDebug(cacheCategoryKey, cacheFilterKey);
                return (T)getFromListLambdaDelegate.DynamicInvoke(argsFromListLambda);
            }
            if (!CacheApplicationStateManager.GetCachedObject<T>(cacheCategoryKey, cacheFilterKey, maxMinutesValid, timeOfDayExpirations, out t))
            {
                t = GeneralCacheHelper.LoadObject<T>(waitForFlagMaxSeconds, getFromSourceDelegate, argsFromSource, cacheCategoryKey, cacheFilterKey);
            }
            return t;
        }
    }
}