using Plugin.Configuration;
using Plugin.Interior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plugin
{
    public class CombinedMap
    {
        private static Dictionary<string, List<InteriorConfig>> combinedConfigs = [];

        public string MapName { get; set; }
        public List<InteriorConfig>? InteriorConfigs { get; set; }
        public List<InteriorWeightData>? InteriorWeightDatas { get; set; }

        public CombinedMap(string name)
        {
            MapName = name;
        }

        public List<InteriorConfig>? GetCached()
        {
            if (combinedConfigs.TryGetValue(MapName, out var value))
            {
                return value;
            }
            return null;
        }

        public List<InteriorConfig>? GetConfigs()
        {
            var cached = GetCached();
            if (cached != null)
            {
                return cached;
            }

            return Merge();
        }

        /// <summary>
        /// 基于随机数，根据概率获取一个对应的Id
        /// </summary>
        /// <returns></returns>
        public int? SampleOneInteriorId(Random? random = null)
        {
            var configs = GetConfigs();
            if (configs == null)
            {
                return null;
            }
            random ??= new Random();

            double sample = random.NextDouble();
            double sum = 0.0;
            foreach (var config in configs)
            {
                sum += config.Rate;
                if (sum >= sample)
                {
                    return config.Id;
                }
            }

            return null;
        }

        private List<InteriorConfig>? Merge()
        {
            if (InteriorConfigs == null || InteriorWeightDatas == null
                || InteriorConfigs.Count == 0 || InteriorWeightDatas.Count == 0)
            {
                // 空配置无意义，应直接采用游戏原始值
                return null;
            }

            var combined = new List<InteriorConfig>();
            // 去除自定义配置包含的不可能生效的结构类型
            var originalMapIds = new HashSet<int>(InteriorWeightDatas.Select(it => it.Id));
            List<InteriorConfig> newInteriorConfigs = InteriorConfigs.Where(it => originalMapIds.Contains(it.Id)).ToList();

            // 未配置的原始值
            double fixedRateSum = newInteriorConfigs.Sum(it => it.Rate);
            var configMapIds = new HashSet<int>(newInteriorConfigs.Select(it => it.Id));
            var diff = new HashSet<int>(originalMapIds);
            diff.ExceptWith(configMapIds);
            List<InteriorWeightData> remainOriginalWeightData = InteriorWeightDatas.Where(it => diff.Contains(it.Id)).ToList();

            // 配置文件满映射情况，直接对配置数据处理
            if (remainOriginalWeightData.Count == 0 && newInteriorConfigs.Count > 0)
            {
                if (newInteriorConfigs.Sum(it => it.Rate) != 1.0)
                {
                    Plugin.Logger.LogWarning($@"Sum of configured interior rates not equal to 1.0, rebalancing...");
                }
                double[] rates = ZoomRatesInto(newInteriorConfigs.Select(it => (double)it.Rate).ToArray(), 1.0);
                for (int i = 0; i < rates.Length; i++)
                {
                    combined.Add(new InteriorConfig
                    {
                        Id = newInteriorConfigs[i].Id,
                        Rate = rates[i],
                    });
                }
                Plugin.Logger.LogInfo($@"Proxied interior rates: [{String.Join(", ", combined)}]");
                combinedConfigs[MapName] = combined;
                return combined;
            }

            // 处理固定概率配置值
            newInteriorConfigs.ForEach(it => combined.Add(it));

            // 原始值补全
            if (remainOriginalWeightData.Count > 0)
            {
                // 权重转绝对概率
                double[] rates = ZoomRatesInto(remainOriginalWeightData.Select(it => (double)it.Weight).ToArray(), 1.0 - fixedRateSum);
                for (int i = 0; i < rates.Length; i++)
                {
                    combined.Add(new InteriorConfig
                    {
                        Id = remainOriginalWeightData[i].Id,
                        Rate = rates[i],
                    });
                }
            }

            Plugin.Logger.LogInfo($@"Proxied interior rates: [{String.Join(", ", combined)}]");
            combinedConfigs[MapName] = combined;
            return combined;
        }

        /// <summary>
        /// 使列表值总和等于target
        /// </summary>
        /// <param name="rates"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static double[] ZoomRatesInto(double[] rates, double target)
        {
            if (rates == null || rates.Length == 0)
            {
                throw new ArgumentException("Rates cannot be null or empty.");
            }

            double sum = rates.Sum();
            double[] normalizedRates = new double[rates.Length];

            if (sum == 0)
            {
                if (target == 0)
                {
                    Array.Fill(normalizedRates, 0);
                }
                else
                {
                    throw new InvalidOperationException("Cannot normalize when the sum of rates is zero and target is non-zero.");
                }
            }
            else
            {
                for (int i = 0; i < rates.Length; i++)
                {
                    normalizedRates[i] = rates[i] * (target / sum);
                }
            }

            return normalizedRates;
        }

        public override string ToString()
        {
            var cached = GetCached();
            string cachedString = "null";
            if (cached != null)
            {
                StringBuilder sb = new();
                sb.Append("[");
                List<string> strs = [];
                foreach (var config in cached)
                {
                    strs.Add($"InteriorConfig {{ Id={config.Id}, Rate={config.Rate} }}");
                }
                sb.Append(String.Join(", ", strs));
                sb.Append("]");
                cachedString = sb.ToString();
            }
            return $"MapName={MapName}, CachedData={cachedString}";
        }
    }
}
