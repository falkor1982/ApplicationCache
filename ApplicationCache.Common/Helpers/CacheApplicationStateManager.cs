using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;
using ApplicationCache.Common;

namespace ApplicationCache.Common.Helpers
{
    public class CacheApplicationStateManager
    {
        public const string FULLDATEFORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";

        public const string CACHEKEYPREFIX = "CacheFrmwk";

        private const char KEYSEPARATOR = '|';

        private const int THREADLOOPWAITMS = 300;

        public static List<string> AllCachedKeys
        {
            get
            {
                return CacheApplicationStateManager.ApplicationState.AllKeys.ToList<string>();
            }
        }

        public static HttpApplicationState ApplicationState
        {
            get
            {
                return HttpContext.Current.Application;
            }
        }

        public CacheApplicationStateManager()
        {
        }

        public static void CleanCachedObject(string cacheCategory = null, string cacheFilter = null)
        {
            foreach (string allCachedKey in CacheApplicationStateManager.AllCachedKeys)
            {
                if (!allCachedKey.StartsWith(CACHEKEYPREFIX))
                    continue;
                string[] strArrays = allCachedKey.Split(new char[] { KEYSEPARATOR });
                if (cacheCategory != null && !(strArrays[0].Trim() == string.Concat(CACHEKEYPREFIX, cacheCategory)) || cacheFilter != null && !(strArrays[1].Trim() == cacheFilter))
                    continue;
                CacheApplicationStateManager.ApplicationState.Remove(allCachedKey);
            }
        }

        public static void ClearLoadingDataFlag(string cacheCategory, string cacheFilter)
        {
            CacheApplicationStateManager.ApplicationState[string.Concat(CACHEKEYPREFIX, CacheApplicationStateManager.GetLoadingDataFlagKey(cacheCategory, cacheFilter))] = null;
            DebuggerHelper.ClearLoadingDataFlagDebug(cacheCategory, cacheFilter);
        }

        public static Dictionary<string, string> FindCachedObjects(string cacheCategory = null, string cacheFilter = null)
        {
            JsonSerializerSettings jsonSerializerSetting = new JsonSerializerSettings();
            jsonSerializerSetting.ContractResolver = new ExcludeNavigationPropertiesResolver();
            JsonSerializerSettings jsonSerializerSetting1 = jsonSerializerSetting;
            Dictionary<string, string> strs = new Dictionary<string, string>();
            foreach (string allCachedKey in CacheApplicationStateManager.AllCachedKeys)
            {
                if (!allCachedKey.StartsWith(CACHEKEYPREFIX))
                {
                    continue;
                }
                string[] strArrays = allCachedKey.Split(new char[] { KEYSEPARATOR });
                if (cacheCategory != null && !(strArrays[0].Trim() == string.Concat(CACHEKEYPREFIX, cacheCategory)) || cacheFilter != null && !(strArrays[1].Trim() == cacheFilter))
                    continue;
                strs.Add(allCachedKey, JsonConvert.SerializeObject(CacheApplicationStateManager.ApplicationState[allCachedKey], jsonSerializerSetting1));
            }
            return strs;
        }

        private static string GetApplicationStateKeyName(string cacheCategory, string cacheFilter)
        {
            if (string.IsNullOrEmpty(cacheCategory))
                throw new Exception("CacheCategory no puede ser null");
            return string.Concat(cacheCategory, KEYSEPARATOR.ToString(), cacheFilter) ?? string.Empty;
        }

        public static string GetCacheCategoryKey(Type entityType)
        {
            return entityType.Name;
        }

        public static bool GetCachedObject<T>(string cacheCategory, string cacheFilter, int? maxMinutesValid, List<TimeSpan> timeOfDayExpirations, out T retVal)
        {
            DTOCachedObjectWrapper<T> item = (DTOCachedObjectWrapper<T>)CacheApplicationStateManager.ApplicationState[string.Concat(CACHEKEYPREFIX, CacheApplicationStateManager.GetApplicationStateKeyName(cacheCategory, cacheFilter))];
            if (item != null)
            {
                if (CacheApplicationStateManager.IsCachedTimeStillValid(item.CachedTime, maxMinutesValid, timeOfDayExpirations))
                {
                    retVal = item.CachedOject;
                    DebuggerHelper.GetCachedObjectDebug(cacheCategory, cacheFilter);
                    return true;
                }
                CacheApplicationStateManager.ApplicationState[string.Concat(CACHEKEYPREFIX, CacheApplicationStateManager.GetApplicationStateKeyName(cacheCategory, cacheFilter))] = null;
            }
            string str = (string)CacheApplicationStateManager.ApplicationState[string.Concat(CACHEKEYPREFIX, CacheApplicationStateManager.GetLoadingDataFlagKey(cacheCategory, cacheFilter))];
            if (str != null)
            {
                string[] strArrays = str.Split(new char[] { KEYSEPARATOR });
                DateTime dateTime = DateTime.ParseExact(strArrays[0], "yyyy'-'MM'-'dd'T'HH':'mm':'ss", CultureInfo.InvariantCulture);
                int num = int.Parse(strArrays[1]);
                do
                {
                    if (DateTime.Now < dateTime.AddSeconds((double)num))
                        Thread.Sleep(THREADLOOPWAITMS);
                    else
                    {
                        CacheApplicationStateManager.ClearLoadingDataFlag(cacheCategory, cacheFilter);
                        retVal = default(T);
                        return false;
                    }
                }
                while (CacheApplicationStateManager.ApplicationState[string.Concat(CACHEKEYPREFIX, CacheApplicationStateManager.GetLoadingDataFlagKey(cacheCategory, cacheFilter))] != null);
                return CacheApplicationStateManager.GetCachedObject<T>(cacheCategory, cacheFilter, maxMinutesValid, timeOfDayExpirations, out retVal);
            }
            retVal = default(T);
            return false;
        }

        public static DateTime? GetCachedObjectDateTime<T>(string cacheCategory, string cacheFilter)
        {
            DTOCachedObjectWrapper<T> item = (DTOCachedObjectWrapper<T>)CacheApplicationStateManager.ApplicationState[string.Concat(CACHEKEYPREFIX, CacheApplicationStateManager.GetApplicationStateKeyName(cacheCategory, cacheFilter))];
            if (item != null)
            {
                return new DateTime?(item.CachedTime);
            }
            return null;
        }

        public static int? GetCachedObjectElapsedMinutes<T>(string cacheCategory, string cacheFilter)
        {
            DTOCachedObjectWrapper<T> item = (DTOCachedObjectWrapper<T>)CacheApplicationStateManager.ApplicationState[string.Concat(CACHEKEYPREFIX, CacheApplicationStateManager.GetApplicationStateKeyName(cacheCategory, cacheFilter))];
            if (item == null)
                return null;
            TimeSpan now = DateTime.Now - item.CachedTime;
            return new int?((int)now.TotalMinutes);
        }

        public static string GetCacheFilterKey(string delegateName, object[] argsFromSource)
        {
            if (argsFromSource == null)
                return string.Empty;
            return string.Format("{0}" + KEYSEPARATOR.ToString() + "{ 1}", delegateName, string.Join(",", argsFromSource));
        }

        private static string GetLoadingDataFlagKey(string cacheCategory, string cacheFilter)
        {
            return string.Concat("LoadingDataFlags", KEYSEPARATOR, CacheApplicationStateManager.GetApplicationStateKeyName(cacheCategory, cacheFilter));
        }

        private static bool HasLoadingDataFlagKey(string cacheCategory, string cacheFilter)
        {
            return CacheApplicationStateManager.ApplicationState[string.Concat(CACHEKEYPREFIX, CacheApplicationStateManager.GetLoadingDataFlagKey(cacheCategory, cacheFilter))] != null;
        }

        public static bool HasValidCatchedObject<T>(string cacheCategory, string cacheFilter, int? maxMinutesValid, List<TimeSpan> timeOfDayExpirations)
        {
            DTOCachedObjectWrapper<T> item = (DTOCachedObjectWrapper<T>)CacheApplicationStateManager.ApplicationState[string.Concat(CACHEKEYPREFIX, CacheApplicationStateManager.GetApplicationStateKeyName(cacheCategory, cacheFilter))];
            if (item == null)
            {
                return false;
            }
            return CacheApplicationStateManager.IsCachedTimeStillValid(item.CachedTime, maxMinutesValid, timeOfDayExpirations);
        }

        private static bool IsCachedTimeStillValid(DateTime objectCachedTime, int? maxMinutesValid, List<TimeSpan> timeOfDayExpirations)
        {
            double totalMinutes = (DateTime.Now - objectCachedTime).TotalMinutes;
            if (maxMinutesValid.HasValue)
            {
                double num = totalMinutes;
                int? nullable = maxMinutesValid;
                if ((num >= (double)nullable.GetValueOrDefault() ? true : !nullable.HasValue))
                    return false;
            }
            if (timeOfDayExpirations == null)
            {
                return true;
            }
            return !timeOfDayExpirations.Where<TimeSpan>((TimeSpan tde) => {
                if (tde > DateTime.Now.TimeOfDay)
                    return false;
                return tde > objectCachedTime.TimeOfDay;
            }).Any<TimeSpan>();
        }

        public static void SetCachedObject(string cacheCategory, string cacheFilter, object objectToCach)
        {
            CacheApplicationStateManager.ApplicationState[string.Concat(CACHEKEYPREFIX, CacheApplicationStateManager.GetApplicationStateKeyName(cacheCategory, cacheFilter))] = objectToCach;
            if (CacheApplicationStateManager.HasLoadingDataFlagKey(cacheCategory, cacheFilter))
                CacheApplicationStateManager.ClearLoadingDataFlag(cacheCategory, cacheFilter);
            DebuggerHelper.SetCachedObjectDebug(cacheCategory, cacheFilter);
        }

        public static void SetLoadingDataFlag(string cacheCategory, string cacheFilter, int waitForFlagMaxSeconds)
        {
            HttpApplicationState applicationState = CacheApplicationStateManager.ApplicationState;
            string str = string.Concat(CACHEKEYPREFIX, CacheApplicationStateManager.GetLoadingDataFlagKey(cacheCategory, cacheFilter));
            DateTime now = DateTime.Now;
            applicationState[str] = string.Concat(now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"), KEYSEPARATOR.ToString(), waitForFlagMaxSeconds);
            DebuggerHelper.SetLoadingDataFlagDebug(cacheCategory, cacheFilter, waitForFlagMaxSeconds);
        }
    }
}