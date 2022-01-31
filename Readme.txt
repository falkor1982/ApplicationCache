ApplicationCacheFramework:

-Allows you to cache objects, lists of objects, and optionally to take a cached object within a list (using lambda delegates. See GeneralCacheHelper.EnforcePermanentCache.GetIntelligentCacheableEntityFromList)
-Enables to continuously keep cached data being refreshed from source, so user does not have to wait (see GeneralCacheHelper.EnforcePermanentCache)
-Allows you to maintain cached objects for a specific time, and/or until specific times of the day (for example, cached users might become unvalid every day at 00hs and 9hs).
- Multi-thread safe