using GameNetcodeStuff;
using HarmonyLib;
using Plugin.Configuration;
using Plugin.Interior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Plugin.Patches
{
    [HarmonyPatch(typeof(StartOfRound), "ChooseNewRandomMapSeed")]
    public class ChooseNewRandomMapSeedPatch
    {
        private static void Postfix(StartOfRound __instance)
        {
            var config = Plugin.Config;
            var levelName = __instance.currentLevel.name;
            var mapConfig = config.GetMapConfigByLevelName(levelName);
            if (mapConfig == null || !mapConfig.Enable)
            {
                Plugin.Logger.LogInfo($"Level {levelName} config set to disable. Ignore.");
                return;
            }

            int mapSeed = __instance.randomMapSeed;
            var roundManager = RoundManager.Instance;

            var combinedMap = new CombinedMap(mapConfig.MapName)
            {
                InteriorConfigs = mapConfig.InteriorConfigs,
                InteriorWeightDatas = TransWeightData(__instance.currentLevel)
            };

            // 计算需要生成的目标结构
            var targetId = combinedMap.SampleOneInteriorId();
            if (targetId == null)
            {
                Plugin.Logger.LogWarning("Got null target interior id, ignored. Maybe set empty configuration values?");
                return;
            }
            var interiorName = Interior.Interior.ParseName(targetId.Value);
            Plugin.Logger.LogInfo($"Target interior: {interiorName}");

            Plugin.Logger.LogDebug($"CombindMap: {combinedMap}");

            // 预计算当前种子生成的结构
            var interiorId = CalcInteriorId(mapSeed, roundManager);
            if (interiorId == null)
            {
                Plugin.Logger.LogError("Unexpected calc interior id: null");
                return;
            }
            if (interiorId == targetId)
            {
                Plugin.Logger.LogInfo($"Detected the current map seed ({mapSeed}) will generate {interiorName} interior, pass.");
                return;
            }

            // 生成目标不匹配，开始尝试重生成
            Plugin.Logger.LogInfo($"Unmatching interior type. Try to regenerate.");
            // 可能没有作用，保险起见
            roundManager.hasInitializedLevelRandomSeed = false;
            roundManager.InitializeRandomNumberGenerators();

            for (int i = 0; i < 2000; i++)
            {
                mapSeed = NewMapSeed();
                interiorId = CalcInteriorId(mapSeed, roundManager);
                if (interiorId == null)
                {
                    Plugin.Logger.LogWarning($"Detected unknown interior type. Ignore.");
                    return;
                }
                interiorName = Interior.Interior.ParseName(interiorId.Value);
                Plugin.Logger.LogDebug($"Trying No.{i + 1} (Seed {mapSeed}) Interior: {interiorName}");

                if (interiorId == targetId)
                {
                    __instance.randomMapSeed = mapSeed;
                    Plugin.Logger.LogInfo($"Generated new map seed ({mapSeed}) after {i + 1} retries.");
                    return;
                }
            }
            Plugin.Logger.LogWarning($"Regenerate failed after 2000 retries.");
        }

        private static List<InteriorWeightData>? TransWeightData(SelectableLevel level)
        {
            var duns = level.dungeonFlowTypes;
            if (duns == null)
            {
                return null;
            }

            var result = new List<InteriorWeightData>();
            foreach (var dun in duns)
            {
                if (dun.rarity == 0)
                {
                    continue;
                }
                result.Add(new InteriorWeightData
                {
                    Id = dun.id,
                    Weight = dun.rarity,
                });
            }

            return result;
        }

        private static int? CalcInteriorId(int mapSeed, RoundManager roundManager)
        {
            var levelRandom = new System.Random(mapSeed);

            if (roundManager.currentLevel.dungeonFlowTypes == null || roundManager.currentLevel.dungeonFlowTypes.Length == 0)
            {
                return null;
            }

            List<int> list = roundManager.currentLevel.dungeonFlowTypes
                .Select(flowType => flowType.rarity)
                .ToList();
            Plugin.Logger.LogDebug($"List: {String.Join(", ", list)}");
            int randomWeightedIndex = roundManager.GetRandomWeightedIndex(list.ToArray(), levelRandom);
            Plugin.Logger.LogDebug($"randomWeightedIndex: {randomWeightedIndex}");
            int interiorId = roundManager.currentLevel.dungeonFlowTypes[randomWeightedIndex].id;
            Plugin.Logger.LogDebug($"interiorId: {interiorId}");

            return interiorId;
        }

        private static Interior.Interior.InteriorEnum? CalcInteriorType(int mapSeed, RoundManager roundManager)
        {
            var id = CalcInteriorId(mapSeed, roundManager);
            if (id == null)
            {
                return null;
            }

            var ty = Interior.Interior.ParseEnumFromId(id.Value);
            if (ty == null)
            {
                return null;
            }

            return ty;
        }

        private static int NewMapSeed()
        {
            return UnityEngine.Random.Range(1, 100000000);
        }
    }
}
