using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Configuration
{
    public class MapConfig
    {
        public string MapName { get; private set; }
        public bool Enable { get; set; }
        public List<InteriorConfig> InteriorConfigs { get; set; }

        public MapConfig(string mapName, bool enable)
        {
            MapName = mapName;
            Enable = enable;
            InteriorConfigs = [];
        }

        public override string ToString()
        {
            return $"MapName={MapName}, Enable={Enable}, InteriorConfigs={InteriorConfigs}";
        }
    }
}
