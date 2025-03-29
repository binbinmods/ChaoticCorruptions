using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
// using static Obeliskial_Essentials.Essentials;
using System;
using static ChaoticCorruptions.Plugin;
using static ChaoticCorruptions.CustomFunctions;
using static ChaoticCorruptions.ChaoticCorruptionsFunctions;
using System.Collections.Generic;
using static Functions;
using UnityEngine;
using System.ComponentModel;

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
        public static void GetCardByRarityPostfix(ref string __result, CardData _cardData)
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
                CardData cardData = Globals.Instance.GetCardData(__result[i]);
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
        public static void SetInitialItemsPrefix(ref Hero __instance, ref CardData _cardData, ref int _rankLevel)
        {
            LogDebug($"SetInitialItemsPrefix {_cardData.Id}");
            if (_cardData.Id == "harley") { return; }
            if (CorruptStartingItems.Value || devMode)
            {
                _cardData = _cardData?.UpgradesToRare ?? _cardData;
                _rankLevel = 0;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hero), "SetInitialCards")]
        public static void SetInitialCardsPostfix(ref Hero __instance, HeroData heroData)
        {
            LogDebug("SetInitialCardsPostfix");
            // UnityEngine.Random.InitState((AtOManager.Instance.GetGameId() + __instance.SourceName + PluginInfo.PLUGIN_GUID).GetDeterministicHashCode());
            List<string> cards = __instance.Cards;
            if (CompletelyRandomizeStartingDecks.Value || devMode)
            {

                LogDebug($"SetInitialCardsPostfix - randomizing {__instance.SourceName}");

                for (int i = 0; i < cards.Count; i++)
                {
                    string card = cards[i];
                    LogDebug(card);
                    if (Globals.Instance.GetCardData(card).Starter || Globals.Instance.GetCardData(card) == null)
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
                    if (Globals.Instance.GetCardData(card).Starter || Globals.Instance.GetCardData(card) == null)
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


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hero), "SetInitialCardsSingularity")]
        public static void SetSingularityInitialCardsPostfix(ref Hero __instance, HeroData heroData)
        {
            LogDebug("SetInitialCardsSingularity");
            UnityEngine.Random.InitState((AtOManager.Instance.GetGameId() + __instance.SourceName + PluginInfo.PLUGIN_GUID).GetDeterministicHashCode());
            List<string> cards = __instance.Cards;
            if (CompletelyRandomizeStartingDecks.Value || devMode)
            {

                LogDebug($"SetInitialCardsPostfix - randomizing {__instance.SourceName}");

                for (int i = 0; i < cards.Count; i++)
                {
                    string card = cards[i];
                    LogDebug(card);
                    if (Globals.Instance.GetCardData(card).Starter || Globals.Instance.GetCardData(card) == null)
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardCraftManager), "CanCraftThisCard")]
        public static void CanCraftThisCardPostfix(ref CardCraftManager __instance, ref bool __result, CardData cData)
        {
            // LogDebug("CanCraftThisCardPostfix");    
            if ((CraftableCorruptions.Value || devMode) && cData.CardUpgraded == Enums.CardUpgraded.Rare && CanCraftRarity(__instance, cData))
            {
                __result = true;
                return;
            }

            if (OnlyCraftCorrupts.Value || devMode)
            {
                if (cData.CardUpgraded == Enums.CardUpgraded.Rare && CanCraftRarity(__instance, cData))
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
        public static void ShowCardCraftPostfix(CardCraftManager __instance, BotonAdvancedCraft ___buttonAdvancedCraft)
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
            if ((CraftableCorruptionsCost.Value <= 0 || !CraftableCorruptions.Value) && !devMode) { return; }

            CardData cardData = Globals.Instance.GetCardData(cardId);
            if (cardData == null || cardData.CardUpgraded != Enums.CardUpgraded.Rare) { return; }

            int costToAdd = CraftableCorruptionsCost.Value;
            costToAdd -= Functions.FuncRoundToInt(costToAdd * discountUpgrade);
            costToAdd += Functions.FuncRoundToInt((float)((double)costToAdd * (double)AtOManager.Instance.Sandbox_cardCraftPrice * 0.0099999997764825821));

            __result += costToAdd;

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetObeliskScore")]
        public static bool SetObeliskScorePrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetScore")]
        public static bool SetScorePrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetSingularityScore")]
        public static bool SetSingularityScorePrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetObeliskScoreLeaderboard")]
        public static bool SetObeliskScoreLeaderboardPrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetScoreLeaderboard")]
        public static bool SetScoreLeaderboardPrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "SetSingularityScoreLeaderboard")]
        public static bool SetSingularityScoreLeaderboardPrefix(ref SteamManager __instance, int score, bool singleplayer = true)
        {
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), nameof(Globals.CreateGameContent))]
        public static void CreateGameContent(ref Globals __instance,
        ref Dictionary<string, CardData> ____CardsSource,
        ref Dictionary<string, CardData> ____Cards,
        ref Dictionary<Enums.CardType, List<string>> ____CardItemByType,
        ref Dictionary<Enums.CardType, List<string>> ____CardListByType,
        ref Dictionary<Enums.CardClass, List<string>> ____CardListByClass,
        ref List<string> ____CardListNotUpgraded,
        ref Dictionary<Enums.CardClass, List<string>> ____CardListNotUpgradedByClass,
        ref Dictionary<string, List<string>> ____CardListByClassType,
        ref Dictionary<string, int> ____CardEnergyCost
        )
        {
            
            string cardId = "chaos";
            // string cardToCloneFrom = "divineguidancerare";
            string cardToCloneFrom = "lasthope";
            CardData newCard = AddNewCard($"chaotic{cardId}", cardToCloneFrom, ref ____CardsSource, ref ____Cards);

            newCard.UpgradedFrom = "chaoticchaos";
            newCard.CardClass = Enums.CardClass.Special;
            newCard.EnergyCost = 0;
            newCard.CardType = Enums.CardType.None;
            newCard.AddCard = 1;
            newCard.AddCardChoose = 6;
            newCard.AddCardVanish = false;
            newCard.AddCardReducedCost = 1;
            newCard.AddCardCostTurn = false;
            newCard.AddCardPlace = Enums.CardPlace.Hand;
            newCard.CardName = "Chaos";
            newCard.CardUpgraded = Enums.CardUpgraded.Rare;
            newCard.CardRarity = Enums.CardRarity.Epic;
            // newCard.Corrupted = true;
            // newCard.Visible = true;                
            newCard.Playable = true;
            

            string cardIdrare = "chaosrare";
            CardData newCardRare = AddNewCard($"chaotic{cardIdrare}", cardToCloneFrom, ref ____CardsSource, ref ____Cards);

            newCardRare.UpgradedFrom = "chaoticchaosrare";
            newCardRare.CardClass = Enums.CardClass.Special;
            newCardRare.EnergyCost = 0;
            newCardRare.CardType = Enums.CardType.None;
            newCardRare.AddCard = 1;
            newCardRare.RelatedCard = "";
            newCardRare.AddCardChoose = 10;
            newCardRare.CardName = "CHAOS!";
            newCardRare.AddCardVanish = false;
            newCardRare.AddCardReducedCost = 2;
            newCardRare.AddCardCostTurn = false;
            newCardRare.AddCardPlace = Enums.CardPlace.Hand;
            newCardRare.CardUpgraded = Enums.CardUpgraded.Rare;
            newCardRare.CardRarity = Enums.CardRarity.Mythic;
            newCardRare.Playable = true;
            // newCardRare.Visible = true;

            InitNewCard(newCard, ref ____CardItemByType, ref ____CardListByType, ref ____CardListByClass, ref ____CardListNotUpgraded, ref ____CardListNotUpgradedByClass, ref ____CardListByClassType, ref ____CardEnergyCost);
            InitNewCard(newCardRare, ref ____CardItemByType, ref ____CardListByType, ref ____CardListByClass, ref ____CardListNotUpgraded, ref ____CardListNotUpgradedByClass, ref ____CardListByClassType, ref ____CardEnergyCost);



        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardCraftManager), "CreateDeck")]
        public static void CreateDeckPrefix(CardCraftManager __instance, int _heroIndex, bool fast = false)
        {
            Hero currentHero = AtOManager.Instance.GetHero(_heroIndex);
            LogDebug($"{currentHero.SourceName} cardsL=: {string.Join(", ", currentHero.Cards)}");
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
            CardData cardData = Globals.Instance.GetCardData(_cardId.Split('_', StringSplitOptions.None)[0], false);
            LogDebug($"{_cardId} carddata {cardData.Id} ");
            
            
        }
        


    }
}