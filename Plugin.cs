// These are your imports, mostly you'll be needing these 5 for every plugin. Some will need more.

using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using static Obeliskial_Essentials.Essentials;
using static Obeliskial_Essentials.CardDescriptionNew;
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections;
// using static ChaoticCorruptions.EssentialsCompatibility;


// The Plugin csharp file is used to specify some general info about your plugin. and set up things for 


// Make sure all your files have the same namespace and this namespace matches the RootNamespace in the .csproj file
// All files that are in the same namespace are compiled together and can "see" each other more easily.

namespace ChaoticCorruptions
{
    // These are used to create the actual plugin. If you don't need Obeliskial Essentials for your mod, 
    // delete the BepInDependency and the associated code "RegisterMod()" below.

    // If you have other dependencies, such as obeliskial content, make sure to include them here.
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.stiffmeds.obeliskialessentials", BepInDependency.DependencyFlags.SoftDependency)] // this is the name of the .dll in the !libs folder.
    [BepInProcess("AcrossTheObelisk.exe")] //Don't change this

    // If PluginInfo isn't working, you are either:
    // 1. Using BepInEx v6
    // 2. Have an issue with your csproj file (not loading the analyzer or BepInEx appropriately)
    // 3. You have an issue with your solution file (not referencing the correct csproj file)


    public class Plugin : BaseUnityPlugin
    {

        // If desired, you can create configs for users by creating a ConfigEntry object here, 
        // Configs allows users to specify certain things about the mod. 
        // The most common would be a flag to enable/disable portions of the mod or the entire mod.

        // You can use: config = Config.Bind() to set the title, default value, and description of the config.
        // It automatically creates the appropriate configs.


        public static ConfigEntry<bool> EnableMod { get; set; }
        public static ConfigEntry<bool> EnableDebugging { get; set; }
        public static ConfigEntry<int> IncreaseCardCorruptionOdds { get; set; }
        public static ConfigEntry<int> IncreaseItemCorruptionOdds { get; set; }
        public static ConfigEntry<bool> GuaranteeCorruptCards { get; set; }
        public static ConfigEntry<bool> GuaranteeCorruptItems { get; set; }
        public static ConfigEntry<bool> CorruptStartingDecks { get; set; }
        public static ConfigEntry<bool> CorruptStartingItems { get; set; }
        public static ConfigEntry<bool> RandomizeStartingDecks { get; set; }
        public static ConfigEntry<bool> CompletelyRandomizeStartingDecks { get; set; }
        public static ConfigEntry<bool> PandorasBox { get; set; }
        public static ConfigEntry<bool> CraftableCorruptions { get; set; }
        public static ConfigEntry<int> CraftableCorruptionsCost { get; set; }
        public static ConfigEntry<bool> PurchaseableCorruptPets { get; set; }
        public static ConfigEntry<int> PurchaseableCorruptPetsMultiplier { get; set; }
        public static ConfigEntry<bool> OnlyCraftCorrupts { get; set; }
        public static ConfigEntry<bool> DevMode { get; set; }

        public static string PluginName;
        public static string PluginVersion;
        public static string PluginGUID;

        internal static int ModDate = int.Parse(DateTime.Today.ToString("yyyyMMdd"));
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;


        public static string debugBase = $"{PluginInfo.PLUGIN_GUID} ";

        private void Awake()
        {

            // The Logger will allow you to print things to the LogOutput (found in the BepInEx directory)
            Log = Logger;

            // Sets the title, default values, and descriptions
            string modName = "ChaoticCorruptions";
            EnableMod = Config.Bind(new ConfigDefinition(modName, "EnableMod"), true, new ConfigDescription("Enables the mod. If false, the mod will not work then next time you load the game."));
            EnableDebugging = Config.Bind(new ConfigDefinition(modName, "EnableDebugging"), false, new ConfigDescription("Enables the debugging"));
            IncreaseCardCorruptionOdds = Config.Bind(new ConfigDefinition(modName, "IncreaseCardCorruptionOdds"), 0, new ConfigDescription("Adds a second roll to corrupt cards. 100 will make it guaranteed"));
            IncreaseItemCorruptionOdds = Config.Bind(new ConfigDefinition(modName, "IncreaseItemCorruptionOdds"), 0, new ConfigDescription("Adds a second roll to corrupt items. 100 will make it guaranteed"));
            GuaranteeCorruptCards = Config.Bind(new ConfigDefinition(modName, "GuaranteeCorruptCards"), false, new ConfigDescription("Guarantees all card rewards are corrupted."));
            GuaranteeCorruptItems = Config.Bind(new ConfigDefinition(modName, "GuaranteeCorruptItems"), false, new ConfigDescription("Guarantees all item rewardss are corrupted."));
            CorruptStartingDecks = Config.Bind(new ConfigDefinition(modName, "CorruptStartingDecks"), false, new ConfigDescription("Forces all starting cards to be corrupted."));
            CorruptStartingItems = Config.Bind(new ConfigDefinition(modName, "CorruptStartingItems"), false, new ConfigDescription("Forces all starting items to be corrupted."));
            RandomizeStartingDecks = Config.Bind(new ConfigDefinition(modName, "RandomizeStartingDecks"), true, new ConfigDescription("Randomizes starting decks from all craftable cards. If CorruptStartingDecks is active, they are all corrupted"));
            CompletelyRandomizeStartingDecks = Config.Bind(new ConfigDefinition(modName, "CompletelyRandomizeStartingDecks"), true, new ConfigDescription("Randomizes starting decks from all available cards for each hero's class. If CorruptStartingDecks is active, they are all corrupted"));
            PandorasBox = Config.Bind(new ConfigDefinition(modName, "PandorasBox"), false, new ConfigDescription("If true, replaces your starting deck with Chaos (discovers a card). If CorruptStartingDecks is active, replaces your deck with CHAOS (discovers a corrupted card). Overrides all other starting deck options."));
            CraftableCorruptions = Config.Bind(new ConfigDefinition(modName, "CraftableCorruptions"), true, new ConfigDescription("Makes corrupted cards craftable"));
            CraftableCorruptionsCost = Config.Bind(new ConfigDefinition(modName, "CraftableCorruptionsCost"), 800, new ConfigDescription("The cost added to the regular crafting cost that will be added to the card to craft the corrupted version."));
            // PurchaseableCorruptPets = Config.Bind(new ConfigDefinition(modName, "PurchaseableCorruptPets"), false, new ConfigDescription("Makes corrupted cards craftable"));
            // PurchaseableCorruptPetsMultiplier = Config.Bind(new ConfigDefinition(modName, "PurchaseableCorruptPetsMultiplier"), 200, new ConfigDescription("The how much the corrupt pets cost relative to regular pets. So 200 would be twice the price"));
            OnlyCraftCorrupts = Config.Bind(new ConfigDefinition(modName, "OnlyCraftCorrupts"), false, new ConfigDescription("Makes it so that the only cards you can craft are corrupted cards"));
            DevMode = Config.Bind(new ConfigDefinition(modName, "DevMode"), false, new ConfigDescription("Enables all of the things for testing."));

            PluginName = PluginInfo.PLUGIN_NAME;
            PluginVersion = PluginInfo.PLUGIN_VERSION;
            PluginGUID = PluginInfo.PLUGIN_GUID;
            if (EnableMod.Value)
            {
                if (EssentialsCompatibility.Enabled)
                    EssentialsCompatibility.EssentialsRegister();
                else
                    LogInfo($"{PluginGUID} {PluginVersion} has loaded!");
                harmony.PatchAll();
            }
        }


        // These are some functions to make debugging a tiny bit easier.
        internal static void LogDebug(string msg, [CallerMemberName] string caller = "")
        {
            if (EnableDebugging.Value)
            {
                Log.LogDebug($"{debugBase}- {caller} - {msg}");
            }
        }
        internal static void LogInfo(string msg)
        {
            Log.LogInfo(debugBase + msg);
        }
        internal static void LogError(string msg, [CallerMemberName] string caller = "")
        {
            Log.LogError($"{debugBase}- {caller} - {msg}");
        }

        public static IEnumerator RunAfter(IEnumerator original, Action action)
        {
            while (original.MoveNext())
                yield return original.Current;
            // The original enumerator has finished. _NPCsSource is set now.
            action();
        }


    }
}
