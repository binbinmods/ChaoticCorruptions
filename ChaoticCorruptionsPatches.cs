using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
// using static Obeliskial_Essentials.Essentials;
using System;
using System.Runtime.CompilerServices;
using static ChaoticCorruptions.Plugin;
using static ChaoticCorruptions.ChaoticCorruptionsFunctions;
using System.Collections.Generic;
using System.Collections;
using Cards;
using Cards.Data;

// Make sure your namespace is the same everywhere
namespace ChaoticCorruptions
{

    [HarmonyPatch] // DO NOT REMOVE/CHANGE - This tells your plugin that this is part of the mod

    public class ChaoticCorruptionsPatches
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
        public static bool devMode = DevMode.Value;

        public static int i = 0;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Functions), "GetCardByRarity")]
        public static void GetCardByRarityPostfix(ref string __result, int rarity, CardRealtimeData _cardData, bool isChallenge = false)
        {

            LogDebug("GetCardByRarityPostfix");
            if (GuaranteeCorruptCards.Value || devMode || UnityEngine.Random.Range(0, 100) <= IncreaseCardCorruptionOdds.Value)
            {
                __result = _cardData?.UpgradesToRare?.Id ?? __result;

            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Loot), "GetLootItems")]//, [typeof(string), typeof(string)])]
        public static void GetLootItemsPostfix(ref List<string> __result, string _itemListId, string _idAux = "")
        {
            LogDebug("GetLootItemsPostfix");
            if (!GuaranteeCorruptItems.Value && IncreaseItemCorruptionOdds.Value <= 0 && !devMode)
            {
                return;
            }
            for (int i = 0; i < __result.Count; i++)
            {
                CardRealtimeData cardData = Globals.Instance.GetCardData(__result[i]);
                if (cardData == null) { continue; }

                // LogDebug($"GetLootItemsPostfix - corrupting {cardData.Id} ");
                bool shouldCorrupt = GuaranteeCorruptItems.Value ||
                    (IncreaseItemCorruptionOdds.Value > 0 &&
                     UnityEngine.Random.Range(0, 100) <= IncreaseItemCorruptionOdds.Value);

                if (shouldCorrupt || devMode)
                {
                    __result[i] = cardData?.UpgradesToRare?.Id ?? __result[i];
                }
            }
            // __result[i] = Globals.Instance?.GetCardData(__result[i]) ??__result[i];

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Hero), "SetInitialItems")]
        public static void SetInitialItemsPrefix(ref Hero __instance, ref CardDataNew _cardData, ref int _rankLevel)
        {

            if (_cardData.Id == "harley") { return; }
            if (CorruptStartingItems.Value || devMode)
            {
                LogDebug($"SetInitialItemsPrefix - corrupting {_cardData.Id}");
                _cardData = _cardData?.Upgrade?.UpgradesToRare ?? _cardData;
                _rankLevel = 0;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hero), "SetInitialCards")]
        public static void SetInitialCardsPostfix(ref Hero __instance, HeroData heroData)
        {
            LogDebug("SetInitialCardsPostfix");
            try
            {
                List<string> cards = __instance.Cards;
                if (CompletelyRandomizeStartingDecks.Value || devMode)
                {

                    LogDebug($"SetInitialCardsPostfix - randomizing {__instance.SourceName}");

                    for (int i = 0; i < cards.Count; i++)
                    {
                        string card = cards[i];
                        LogDebug(card);
                        if (Globals.Instance.GetCardData(card).HasFlag(CustomFlags.Starter) || Globals.Instance.GetCardData(card) == null)
                        {
                            continue;
                        }
                        LogDebug("past check");
                        string newCard = GetRandomCardWeighted(__instance, craftableOnly: false).Id;
                        LogDebug($"newCard {newCard}");
                        cards[i] = newCard;
                    }
                    __instance.Cards = cards;
                }
                else if (RandomizeStartingDecks.Value || devMode)
                // if (RandomizeStartingDecks.Value || devMode)
                {

                    for (int i = 0; i < cards.Count; i++)
                    {
                        string card = cards[i];
                        if (Globals.Instance.GetCardData(card).HasFlag(CustomFlags.Starter) || Globals.Instance.GetCardData(card) == null)
                        {
                            continue;
                        }
                        cards[i] = GetRandomCardWeighted(__instance, craftableOnly: true).Id;
                    }
                    __instance.Cards = cards;
                }

                if (CorruptStartingDecks.Value || devMode)
                {
                    LogDebug($"SetInitialCardsPostfix - corrupting {__instance.SourceName}");
                    // List<string> cards = __instance.Cards;
                    for (int i = 0; i < cards.Count; i++)
                    {

                        string card = cards[i];

                        cards[i] = Globals.Instance?.GetCardData(card)?.UpgradesToRare?.Id ?? cards[i];
                    }
                    __instance.Cards = cards;
                }

                if (PandorasBox.Value || devMode)
                {

                    LogDebug($"SetInitialCardsPostfix - Pandoras Box");

                    for (int i = 0; i < cards.Count; i++)
                    {
                        string card = cards[i];
                        LogDebug(card);
                        // if (Globals.Instance.GetCardData(card).Starter || Globals.Instance.GetCardData(card) == null)
                        // {
                        //     continue;
                        // }
                        LogDebug("past check");
                        string newCard = CorruptStartingDecks.Value ? "chaoticchaosrare" : "chaoticchaos";
                        LogDebug($"newCard {newCard}");
                        cards[i] = newCard;
                    }
                    __instance.Cards = cards;
                }
            }
            catch (Exception e)
            {
                LogError($"Error: {e.Message}");
                LogError($"Stack trace: {e.StackTrace}");
                return;
            }
            // UnityEngine.Random.InitState((AtOManager.Instance.GetGameId() + __instance.SourceName + PluginInfo.PLUGIN_GUID).GetDeterministicHashCode());

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hero), "SetInitialCardsSingularity")]
        public static void SetSingularityInitialCardsPostfix(ref Hero __instance, HeroData heroData)
        {
            LogDebug("SetInitialCardsSingularity");
            try
            {

                UnityEngine.Random.InitState((AtOManager.Instance.GetGameId() + __instance.SourceName + PluginInfo.PLUGIN_GUID).GetDeterministicHashCode());
                List<string> cards = __instance.Cards;
                if (CompletelyRandomizeStartingDecks.Value || devMode)
                {

                    LogDebug($"SetInitialCardsPostfix - randomizing {__instance.SourceName}");

                    for (int i = 0; i < cards.Count; i++)
                    {
                        string card = cards[i];
                        LogDebug(card);
                        if (Globals.Instance.GetCardData(card).HasFlag(CustomFlags.Starter) || Globals.Instance.GetCardData(card) == null)
                        {
                            continue;
                        }
                        LogDebug("past check");
                        string newCard = GetRandomCardWeighted(__instance, craftableOnly: false).Id;
                        LogDebug($"newCard {newCard}");
                        cards[i] = newCard;
                    }
                    __instance.Cards = cards;
                }
                else if (RandomizeStartingDecks.Value || devMode)
                // if (RandomizeStartingDecks.Value || devMode)
                {

                    for (int i = 0; i < cards.Count; i++)
                    {
                        string card = cards[i];
                        cards[i] = GetRandomCardWeighted(__instance, craftableOnly: true).Id;
                    }
                    __instance.Cards = cards;
                }



                if (CorruptStartingDecks.Value || devMode)
                {
                    LogDebug($"SetInitialCardsPostfix - corrupting {__instance.SourceName}");
                    // List<string> cards = __instance.Cards;
                    for (int i = 0; i < cards.Count; i++)
                    {
                        string card = cards[i];
                        cards[i] = Globals.Instance?.GetCardData(card)?.UpgradesToRare?.Id ?? cards[i];
                    }
                    __instance.Cards = cards;
                }
            }
            catch (Exception e)
            {
                LogError($"Error: {e.Message}");
                LogError($"Stack trace: {e.StackTrace}");
                return;
            }
        }

        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(RewardsManager), "ShowRewards")]
        // public static void ShowRewardsPrefix(RewardsManager __instance, Dictionary<int, string[]> ___cardsByOrder)
        // {

        //     return;

        // }

        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(CardCraftManager), "SetPrice")]
        // public static void SetPricePostfix(ref CardCraftManager __instance, ref int __result,
        //                                             bool ___isPetShop,
        //                                             string function,
        //                                             string rarity,
        //                                             string cardName = "",
        //                                             int zoneTier = 0,
        //                                             bool useShopDiscount = true)
        // {
        //     LogDebug($"SetPricePostfix");
        //     bool isRare = Globals.Instance.GetCardData(cardName).CardUpgraded == Enums.CardUpgraded.Rare;
        //     if (___isPetShop && (PurchaseableCorruptPets.Value || devMode) && isRare)
        //     {
        //         LogDebug($"SetPricePostfix - {cardName}");
        //         __result *= PurchaseableCorruptPetsMultiplier.Value;
        //     }
        // }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CardCraftManager), "CanCraftThisCard")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool CanCraftThisCard(CardCraftManager instance, CardRealtimeData cData)
        {
            throw new NotImplementedException("Reverse patch stub for CardCraftManager.CanCraftThisCard");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardCraftManager), "CanCraftThisCard")]
        public static void CanCraftThisCardPostfix(ref CardCraftManager __instance, ref bool __result, CardRealtimeData cData)
        {
            // Original method returns false for Rare; use the unupgraded card for the remaining checks.
            bool canCraftBase = CanCraftThisCard(__instance, Functions.GetCardDataFromCardData(cData, ""));
            if ((CraftableCorruptions.Value || devMode) && cData.CardUpgraded == Enums.CardUpgraded.Rare && canCraftBase)
            {
                __result = true;
                return;
            }

            if (OnlyCraftCorrupts.Value || devMode)
            {
                if (cData.CardUpgraded == Enums.CardUpgraded.Rare && canCraftBase)
                {
                    __result = true;
                }
                else
                {
                    __result = false;
                }
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardCraftManager), nameof(CardCraftManager.ShowCardCraft))]
        public static void ShowCardCraftPostfix(CardCraftManager __instance, BotonAdvancedCraft ___buttonAdvancedCraft, int type = 0)
        {
            LogDebug("ShowCardCraftPostfix");
            if ((OnlyCraftCorrupts.Value || devMode) && __instance.craftType == 2)
            {
                LogDebug("ShowCardCraftPostfix - setting Active");
                __instance.AdvancedCraft(change: true);
                // ___buttonAdvancedCraft.gameObject.SetActive(true);
            }
        }
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(CardCraftManager), nameof(CardCraftManager.ShowItemsForBuy))]
        // public static void ShowItemsForBuyPostfix(CardCraftManager __instance, bool ___isPetShop, int pageNum = 1, string itemBought = "")
        // {
        //     LogDebug("ShowItemsForBuyPostfix");
        //     if ((PurchaseableCorruptPets.Value && ___isPetShop) || devMode)
        //     {
        //         LogDebug("ShowItemsForBuyPostfix - pet shop");
        //         __instance.AdvancedCraft(change: true);
        //         // ___buttonAdvancedCraft.gameObject.SetActive(true);
        //     }
        // }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), "GetCraftCost")]
        public static void GetCraftCostPostfix(ref int __result, string cardId, float discountCraft = 0.0f, float discountUpgrade = 0.0f, int zoneTier = 0)
        {
            LogDebug($"GetCraftCostPostfix - {cardId}");
            if (CraftableCorruptionsCost.Value <= 0 || !CraftableCorruptions.Value) { return; }

            CardRealtimeData cardData = Globals.Instance.GetCardData(cardId);
            if (cardData == null || cardData.CardUpgraded != Enums.CardUpgraded.Rare) { return; }

            int costToAdd = CraftableCorruptionsCost.Value;
            costToAdd -= Functions.FuncRoundToInt(costToAdd * discountUpgrade);
            costToAdd += Functions.FuncRoundToInt(costToAdd * SandboxManager.Instance.CardCraftPrice * 0.01f);

            __result += costToAdd;

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), "CreateGameContentRoutine")]
        public static void CreateGameContent(ref IEnumerator __result, Globals __instance)
        {
            __result = RunAfter(__result, CreateGameContentPostfix);
        }
        public static void CreateGameContentPostfix()
        {
            try
            {
                LogDebug("starting");
                Traverse globals = Traverse.Create(Globals.Instance);
                Dictionary<string, CardDataNew> cardsSource = globals.Field("_CardsSource").GetValue<Dictionary<string, CardDataNew>>();
                Dictionary<string, CardRealtimeData> cards = globals.Field("_Cards").GetValue<Dictionary<string, CardRealtimeData>>();
                Dictionary<Enums.CardType, List<string>> cardItemByType = globals.Field("_CardItemByType").GetValue<Dictionary<Enums.CardType, List<string>>>();
                Dictionary<Enums.CardType, List<string>> cardListByType = globals.Field("_CardListByType").GetValue<Dictionary<Enums.CardType, List<string>>>();
                Dictionary<Enums.CardClass, List<string>> cardListByClass = globals.Field("_CardListByClass").GetValue<Dictionary<Enums.CardClass, List<string>>>();
                List<string> cardListNotUpgraded = globals.Field("_CardListNotUpgraded").GetValue<List<string>>();
                Dictionary<Enums.CardClass, List<string>> cardListNotUpgradedByClass = globals.Field("_CardListNotUpgradedByClass").GetValue<Dictionary<Enums.CardClass, List<string>>>();
                Dictionary<string, List<string>> cardListByClassType = globals.Field("_CardListByClassType").GetValue<Dictionary<string, List<string>>>();
                Dictionary<string, int> cardEnergyCost = globals.Field("_CardEnergyCost").GetValue<Dictionary<string, int>>();


                string cardId = "chaos";
                // string cardToCloneFrom = "divineguidancerare";
                string cardToCloneFrom = "lasthope";
                CardRealtimeData oldCard = Globals.Instance.GetCardData(cardToCloneFrom, false);
                if (oldCard == null)
                {
                    LogError($"CreateGameContent: missing source card {cardToCloneFrom}");
                    return;
                }
                CardRealtimeData newCard = AddNewCard($"chaotic{cardId}", cardToCloneFrom, ref cardsSource, ref cards, src =>
                {
                    UpgradeData oldUpgrade = src.Upgrade;
                    SetCardSourceField(src, "upgrade", new UpgradeData(oldUpgrade.UpgradesTo1, oldUpgrade.UpgradesTo2, Enums.CardUpgraded.Rare, "chaoticchaos", oldUpgrade.UpgradesToRare));
                    SetCardSourceField(src, "cardClass", Enums.CardClass.Special);
                    SetCardSourceField(src, "energyCost", 0);
                    SetCardSourceField(src, "cardType", Enums.CardType.None);
                    src.CardManagement.AddCard = 1;
                    src.CardManagement.AddCardChoose = 6;
                    src.CardManagement.AddCardVanish = false;
                    src.CardManagement.AddCardReducedCost = 1;
                    src.CardManagement.AddCardCostTurn = false;
                    src.CardManagement.AddCardPlace = Enums.CardPlace.Hand;
                    SetCardSourceField(src, "cardName", "Chaos");
                    SetCardSourceField(src, "cardRarity", Enums.CardRarity.Epic);
                    SetCardSourceField(src, "playable", true);
                });


                string cardIdrare = "chaosrare";
                CardRealtimeData newCardRare = AddNewCard($"chaotic{cardIdrare}", cardToCloneFrom, ref cardsSource, ref cards, src =>
                {
                    UpgradeData oldUpgrade = src.Upgrade;
                    SetCardSourceField(src, "upgrade", new UpgradeData(oldUpgrade.UpgradesTo1, oldUpgrade.UpgradesTo2, Enums.CardUpgraded.Rare, "chaoticchaosrare", oldUpgrade.UpgradesToRare));
                    SetCardSourceField(src, "cardClass", Enums.CardClass.Special);
                    SetCardSourceField(src, "energyCost", 0);
                    SetCardSourceField(src, "cardType", Enums.CardType.None);
                    src.CardManagement.AddCard = 1;
                    src.Description.RelatedCards = new List<string>();
                    src.CardManagement.AddCardChoose = 10;
                    SetCardSourceField(src, "cardName", "CHAOS!");
                    src.CardManagement.AddCardVanish = false;
                    src.CardManagement.AddCardReducedCost = 2;
                    src.CardManagement.AddCardCostTurn = false;
                    src.CardManagement.AddCardPlace = Enums.CardPlace.Hand;
                    SetCardSourceField(src, "cardRarity", Enums.CardRarity.Mythic);
                    SetCardSourceField(src, "playable", true);
                });

                if (newCard != null)
                    InitNewCard(newCard, ref cardItemByType, ref cardListByType, ref cardListByClass, ref cardListNotUpgraded, ref cardListNotUpgradedByClass, ref cardListByClassType, ref cardEnergyCost);
                if (newCardRare != null)
                    InitNewCard(newCardRare, ref cardItemByType, ref cardListByType, ref cardListByClass, ref cardListNotUpgraded, ref cardListNotUpgradedByClass, ref cardListByClassType, ref cardEnergyCost);

                Traverse.Create(Globals.Instance).Field("_CardsSource").SetValue(cardsSource);
                Traverse.Create(Globals.Instance).Field("_Cards").SetValue(cards);
                Traverse.Create(Globals.Instance).Field("_CardItemByType").SetValue(cardItemByType);
                Traverse.Create(Globals.Instance).Field("_CardListByType").SetValue(cardListByType);
                Traverse.Create(Globals.Instance).Field("_CardListByClass").SetValue(cardListByClass);
                Traverse.Create(Globals.Instance).Field("_CardListNotUpgraded").SetValue(cardListNotUpgraded);
                Traverse.Create(Globals.Instance).Field("_CardListNotUpgradedByClass").SetValue(cardListNotUpgradedByClass);
                Traverse.Create(Globals.Instance).Field("_CardListByClassType").SetValue(cardListByClassType);
                Traverse.Create(Globals.Instance).Field("_CardEnergyCost").SetValue(cardEnergyCost);

                LogDebug("CreateGameContentPostfix - done");
                return;
            }
            catch (Exception e)
            {
                LogError($"error: {e.Message}");
                LogError($"stack trace: {e.StackTrace}");
                return;
            }


        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardCraftManager), "CreateDeck")]
        public static void CreateDeckPrefix(CardCraftManager __instance, Hero hero, bool fast = false)
        {
            LogDebug($"{hero.SourceName} cardsL=: {string.Join(", ", hero.Cards)}");
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardCraftManager), "CreateDeck")]
        public static void SetBlocked(CardCraftManager __instance)
        {
            // Hero currentHero = AtOManager.Instance.GetHero(_heroIndex);
            LogDebug($"SetBlocked");
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardCraftManager), "RedrawGridLayout")]
        public static void RedrawGridLayout(CardCraftManager __instance)
        {
            LogDebug($"RedrawGridLayout");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardVertical), "SetCard")]
        public static void SetCardPrefix(CardVertical __instance, string _cardId, int _cardType = 0, Hero _hero = null)
        {
            LogDebug($"SetCardPrefix - {_cardId} ");
            CardRealtimeData cardData = Globals.Instance.GetCardData(_cardId.Split('_', StringSplitOptions.None)[0], false);
            LogDebug($"{_cardId} carddata {cardData.Id} ");


        }



    }
}