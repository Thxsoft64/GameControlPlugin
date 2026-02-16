namespace Loupedeck.GameControlPlugin.Helpers
{
    public static class AdjustmentCache
    {
        private static readonly object Lock = new();
        private static readonly DictionaryNoCase<AdjustmentCacheEntry> Cache = new();

        public static void AddEntry(string pluginName, uint stickId, string axisName, string actionParameter)
        {
            lock (Lock)
            {
                var key = GetKey(stickId, axisName);
                
                if(!Cache.ContainsKey(key))
                    Cache.Add(key, new AdjustmentCacheEntry()
                    {
                        PluginName = pluginName,
                        ActionParameter = actionParameter
                    });
            }
        }
       
        public static AdjustmentCacheEntry Get(uint stickId, string axisName)
        {
            var key = GetKey(stickId, axisName);

            if (Cache.TryGetValue(key, out var entry))
                return entry;

            return null;
        }
        
        private static string GetKey(uint stickId, string axisName)
        {
            return $"{stickId}_{axisName}";
        }
    }

    public class AdjustmentCacheEntry
    {
        public string PluginName { get; set; }
        public string ActionParameter { get; set; }
    }
}