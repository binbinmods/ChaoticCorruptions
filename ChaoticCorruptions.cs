using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
// using static Obeliskial_Essentials.Essentials;
using System;
using static ChaoticCorruptions.Plugin;
using static ChaoticCorruptions.CustomFunctions;
using System.Collections.Generic;

// Make sure your namespace is the same everywhere
namespace ChaoticCorruptions
{

    [HarmonyPatch] // DO NOT REMOVE/CHANGE - This tells your plugin that this is part of the mod

    public class NameYourPluginClass
    {
        // The base game is organized into a large number of classes
        // AtOManager is the class that handles the majority of the game logic.
        // MatchManager handles things to do with combats
        // MapManager handles things to do with the map/node selection
        // Character handles things to do which your characters traits, stats, and stuff like that

        // I will show some of the most commonly patched methods below and how to patch them

        // Patches must start with either a [HarmonyPrefix] or [HarmonyPostfix] tag
        // Followed by [HarmonyPatch(typeof(<class>),nameof(<class>.<method>))]
        // This tells your plugin which base game method to patch and whether it will be a prefix or a postfix

        // Prefixes are executed before the original code, postfixes are executed after

        public static int i = 0;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Functions), "GetCardByRarity")]
        public static void OverwriteCardPool(ref string __result, CardData _cardData)
        {
            if(GuaranteeCorruptCards.Value)
            {
                __result = _cardData?.UpgradesToRare?.Id ?? __result;

            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(RewardsManager), "ShowRewards")]
        public static void ShowRewardsPrefix(RewardsManager __instance, Dictionary<int, string[]> ___cardsByOrder)
        {
            // BeginAdventure
            LogDebug("ShowRewardsPrefix");
            int increasedCorruptionChance = GuaranteeCorruptCards.Value ? 100 : IncreaseCorruptionOdds.Value;
            if (increasedCorruptionChance == 0 || true)
            {
                return;
            }
            else
            {
                int randInt = Functions.Random(0, 100, PluginInfo.PLUGIN_GUID + i);
                i++;
                foreach (KeyValuePair<int, string[]> kvp in ___cardsByOrder)
                {
                    // Globals.Instance.Cardlist
                }
            }

            if(RandomizeStartingDecks.Value)
            {

            }




            return;

        }

       

    }
}