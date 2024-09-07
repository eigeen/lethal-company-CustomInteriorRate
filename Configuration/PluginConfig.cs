using BepInEx;
using BepInEx.Configuration;
using Plugin.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Plugin.Configuration
{
    public class PluginConfig
    {
        private ConfigFile _configFile;
        private Dictionary<string, MapConfig> _mapConfigs;

        public ConfigEntry<bool> GeneralEnable { get; private set; } = null!;

        public PluginConfig()
        {
            _configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.cfg"), true);

            BindAll();

            foreach (var pair in _configFile)
            {
                Plugin.Logger.LogDebug($"Section={pair.Key.Section}, Key={pair.Key.Key}, Value={pair.Value.BoxedValue}");
            }

            _mapConfigs = [];

            var configKeys = _configFile.Keys;
            foreach (var configDef in configKeys)
            {
                // key: Map.Adamance.Enable
                string[] sections = configDef.Section.Split('.');
                if (sections.Length == 2 && sections[0] == "Map")
                {
                    string mapName = sections[1];
                    MapConfig mapConfig;
                    if (!_mapConfigs.ContainsKey(mapName))
                    {
                        mapConfig = new MapConfig(mapName, false);
                    }
                    else
                    {
                        mapConfig = _mapConfigs[mapName];
                    }

                    var configEntry = _configFile[configDef];
                    if (configDef.Key == "Enable")
                    {
                        mapConfig.Enable = (bool)configEntry.BoxedValue;
                        _mapConfigs[mapName] = mapConfig;
                        continue;
                    }
                    if (configDef.Key.EndsWith("Rate"))
                    {
                        string interiorName = StringUtils.TrimEnd(configDef.Key, "Rate");
                        mapConfig.InteriorConfigs.Add(new InteriorConfig
                        {
                            Name = interiorName,
                            Rate = (double)configEntry.BoxedValue
                        });
                        _mapConfigs[mapName] = mapConfig;
                        continue;
                    }
                }
            }

            // validation
            foreach (var mapConfigPair in _mapConfigs)
            {
                var mapName = mapConfigPair.Key;
                var mapConfig = mapConfigPair.Value;
                if (mapConfig == null)
                {
                    _mapConfigs.Remove(mapName);
                    continue;
                }
                // remove -1 values
                _mapConfigs[mapName].InteriorConfigs = mapConfig.InteriorConfigs.Where(item => item.Rate > 0).ToList();
                // total rate 0.0 - 1.0
                var totalRate = mapConfig.InteriorConfigs.Select(item => item.Rate).Sum();
                if (totalRate > 1.0 || totalRate < 0.0)
                {
                    Plugin.Logger.LogError($"Total rate of map {mapName} is invalid: expect [0.0, 1.0], found {totalRate}");
                    _mapConfigs.Remove(mapName);
                }
            }
            // debug output
            LogMapConfigs(_mapConfigs);
        }

        private static void LogMapConfigs(Dictionary<string, MapConfig> mapConfigs)
        {
            if (mapConfigs == null || mapConfigs.Count == 0)
            {
                Plugin.Logger.LogDebug($"mapConfigs is empty!");
                return;
            }
            foreach (var mapConfigPair in mapConfigs)
            {
                Plugin.Logger.LogDebug($"Map: {mapConfigPair.Key}, Config: {mapConfigPair.Value.ToString()}");
            }
        }

        public MapConfig? GetMapConfigByLevelName(string levelName)
        {
            if (_mapConfigs.TryGetValue(GetMapNameByLevelName(levelName), out var value))
            {
                return value;
            }
            return null;
        }

        private static string GetMapNameByLevelName(string levelName)
        {
            return StringUtils.TrimEnd(levelName, "Level");
        }

        private void BindAll()
        {
            GeneralEnable = _configFile.Bind("General", "Enable", true, "Enable plugin features.");

            _configFile.Bind("Map.Adamance", "Enable", false, "Enable the modification on map Vow.");
            _configFile.Bind("Map.Adamance", "FactoryRate", -1.0, "Override the rate of Factory interior. If set to -1, keep the original rate.");
            _configFile.Bind("Map.Adamance", "ManorRate", -1.0, "Override the rate of Manor interior. If set to -1, keep the original rate.");
            _configFile.Bind("Map.Adamance", "MineshaftRate", -1.0, "Override the rate of Mineshaft interior. If set to -1, keep the original rate.");

            _configFile.Bind("Map.Artifice", "Enable", false);
            _configFile.Bind("Map.Artifice", "FactoryRate", -1.0);
            _configFile.Bind("Map.Artifice", "ManorRate", -1.0);
            _configFile.Bind("Map.Artifice", "MineshaftRate", -1.0);

            _configFile.Bind("Map.Assurance", "Enable", false);
            _configFile.Bind("Map.Assurance", "FactoryRate", -1.0);
            _configFile.Bind("Map.Assurance", "ManorRate", -1.0);
            _configFile.Bind("Map.Assurance", "MineshaftRate", -1.0);

            _configFile.Bind("Map.Dine", "Enable", false);
            _configFile.Bind("Map.Dine", "FactoryRate", -1.0);
            _configFile.Bind("Map.Dine", "ManorRate", -1.0);
            _configFile.Bind("Map.Dine", "MineshaftRate", -1.0);

            _configFile.Bind("Map.Embrion", "Enable", false);
            _configFile.Bind("Map.Embrion", "FactoryRate", -1.0);
            _configFile.Bind("Map.Embrion", "ManorRate", -1.0);
            _configFile.Bind("Map.Embrion", "MineshaftRate", -1.0);

            _configFile.Bind("Map.Experimentation", "Enable", false);
            _configFile.Bind("Map.Experimentation", "FactoryRate", -1.0);
            _configFile.Bind("Map.Experimentation", "ManorRate", -1.0);
            _configFile.Bind("Map.Experimentation", "MineshaftRate", -1.0);

            _configFile.Bind("Map.Liquidation", "Enable", false);
            _configFile.Bind("Map.Liquidation", "FactoryRate", -1.0);
            _configFile.Bind("Map.Liquidation", "ManorRate", -1.0);
            _configFile.Bind("Map.Liquidation", "MineshaftRate", -1.0);

            _configFile.Bind("Map.March", "Enable", false);
            _configFile.Bind("Map.March", "FactoryRate", -1.0);
            _configFile.Bind("Map.March", "ManorRate", -1.0);
            _configFile.Bind("Map.March", "MineshaftRate", -1.0);

            _configFile.Bind("Map.Offense", "Enable", false);
            _configFile.Bind("Map.Offense", "FactoryRate", -1.0);
            _configFile.Bind("Map.Offense", "ManorRate", -1.0);
            _configFile.Bind("Map.Offense", "MineshaftRate", -1.0);

            _configFile.Bind("Map.Rend", "Enable", false);
            _configFile.Bind("Map.Rend", "FactoryRate", -1.0);
            _configFile.Bind("Map.Rend", "ManorRate", -1.0);
            _configFile.Bind("Map.Rend", "MineshaftRate", -1.0);

            _configFile.Bind("Map.Titan", "Enable", false);
            _configFile.Bind("Map.Titan", "FactoryRate", -1.0);
            _configFile.Bind("Map.Titan", "ManorRate", -1.0);
            _configFile.Bind("Map.Titan", "MineshaftRate", -1.0);

            _configFile.Bind("Map.Vow", "Enable", false);
            _configFile.Bind("Map.Vow", "FactoryRate", -1.0);
            _configFile.Bind("Map.Vow", "ManorRate", -1.0);
            _configFile.Bind("Map.Vow", "MineshaftRate", -1.0);

        }
    }
}
