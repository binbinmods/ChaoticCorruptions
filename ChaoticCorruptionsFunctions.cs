using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
// using Obeliskial_Content;
// using Obeliskial_Essentials;
using System.IO;
using static UnityEngine.Mathf;
using UnityEngine.TextCore.LowLevel;
using static ChaoticCorruptions.Plugin;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ChaoticCorruptions
{
    public class ChaoticCorruptionsFunctions
    {

        public static bool CanCraftRarity(CardCraftManager __instance, CardData cardData)
        {
            CardData cData = cardData;
            cData = Functions.GetCardDataFromCardData(cData, "");
            Enums.CardRarity maxCraftRarity = Traverse.Create(__instance).Field("maxCraftRarity")?.GetValue<Enums.CardRarity>() ?? Enums.CardRarity.Epic;
            if ((bool)(UnityEngine.Object)MapManager.Instance && GameManager.Instance.IsObeliskChallenge())
                return maxCraftRarity == Enums.CardRarity.Mythic || maxCraftRarity == Enums.CardRarity.Epic && cData.CardRarity != Enums.CardRarity.Mythic || maxCraftRarity == Enums.CardRarity.Rare && cData.CardRarity != Enums.CardRarity.Mythic && cData.CardRarity != Enums.CardRarity.Epic || maxCraftRarity == Enums.CardRarity.Uncommon && cData.CardRarity != Enums.CardRarity.Mythic && cData.CardRarity != Enums.CardRarity.Epic && cData.CardRarity != Enums.CardRarity.Rare || maxCraftRarity == Enums.CardRarity.Common && cData.CardRarity == Enums.CardRarity.Common;
            if (AtOManager.Instance.Sandbox_allRarities)
                return true;
            if (cData.CardRarity == Enums.CardRarity.Mythic)
                return false;
            if (AtOManager.Instance.GetTownTier() == 0)
            {
                if (cData.CardRarity == Enums.CardRarity.Rare && (!PlayerManager.Instance.PlayerHaveSupply("townUpgrade_1_4") || AtOManager.Instance.GetNgPlus() >= 8) || cData.CardRarity == Enums.CardRarity.Epic || cData.CardRarity == Enums.CardRarity.Mythic)
                    return false;
            }
            else if (AtOManager.Instance.GetTownTier() == 1 && cData.CardRarity == Enums.CardRarity.Epic && (!PlayerManager.Instance.PlayerHaveSupply("townUpgrade_1_6") || AtOManager.Instance.GetNgPlus() >= 8))
                return false;
            return true;
        }

        public static CardData GetRandomCardWeighted(Hero hero, bool craftableOnly = true)
        {
            int madness = AtOManager.Instance?.GetNgPlus() ?? 0;
            int commonChance = craftableOnly ? (madness < 5 ? 10 : 37) : 30;
            // int commonChance = craftableOnly ? 70: 35;
            int uncommonChance = craftableOnly ? (madness < 5 ? 65 : 60) : 35;
            int rareChance = craftableOnly ? (madness < 5 ? 20 : 3) : 20;
            int epicChance = craftableOnly ? (madness < 5 ? 5 : 0) : 10;
            // int mythicChance = craftableOnly ? (madness < 5 ? 0 : 0) : 5;

            Enums.CardClass result1 = Enums.CardClass.None;
            Enum.TryParse<Enums.CardClass>(Enum.GetName(typeof(Enums.HeroClass), (object)hero.HeroData.HeroClass), out result1);
            Enums.CardClass result2 = Enums.CardClass.None;
            Enum.TryParse<Enums.CardClass>(Enum.GetName(typeof(Enums.HeroClass), (object)hero.HeroData.HeroSubClass.HeroClassSecondary), out result2);
            List<string> stringList1 = Globals.Instance.CardListNotUpgradedByClass[result1];
            List<string> stringList2 = result2 == Enums.CardClass.None ? new List<string>() : Globals.Instance.CardListNotUpgradedByClass[result2];
            int index1 = UnityEngine.Random.Range(0, 2);
            int num10 = UnityEngine.Random.Range(0, 100);
            CardData _cardData = Globals.Instance.GetCardData(index1 < 1 || result2 == Enums.CardClass.None ? stringList1[UnityEngine.Random.Range(0, stringList1.Count)] : stringList2[UnityEngine.Random.Range(0, stringList2.Count)], false);
            LogDebug($"Randomizing card: {_cardData.Id}");

            bool flag2 = true;
            while (flag2)
            {
                flag2 = false;
                bool flag3 = false;
                while (!flag3)
                {
                    flag2 = false;
                    _cardData = Globals.Instance.GetCardData(index1 < 1 || result2 == Enums.CardClass.None ? stringList1[UnityEngine.Random.Range(0, stringList1.Count)] : stringList2[UnityEngine.Random.Range(0, stringList2.Count)], false);
                    if (!flag2)
                    {
                        if (num10 < commonChance)
                        {
                            if (_cardData.CardRarity == Enums.CardRarity.Common)
                                flag3 = true;
                        }
                        else if (num10 < commonChance + uncommonChance)
                        {
                            if (_cardData.CardRarity == Enums.CardRarity.Uncommon)
                                flag3 = true;
                        }
                        else if (num10 < commonChance + uncommonChance + rareChance)
                        {
                            if (_cardData.CardRarity == Enums.CardRarity.Rare)
                                flag3 = true;
                        }
                        else if (num10 < commonChance + uncommonChance + rareChance + epicChance)
                        {
                            if (_cardData.CardRarity == Enums.CardRarity.Epic)
                                flag3 = true;
                        }
                        else if (_cardData.CardRarity == Enums.CardRarity.Mythic)
                            flag3 = true;
                    }
                }
            }

            return _cardData;
        }

        // public static void HandlePetShop(CardCraftManager __instance)
        // {
        //     List<string> itemList = Globals.Instance.CardItemByType[Enums.CardType.Pet];
        //     Dictionary<int, CardCraftItem> craftCardItemDict = Traverse.Create(__instance).Field("craftCardItemDict").GetValue<Dictionary<int, CardCraftItem>>();
        //     Hero currentHero = Traverse.Create(__instance).Field("currentHero").GetValue<Hero>();
        //     int currentItemsPageNum = Traverse.Create(__instance).Field("currentItemsPageNum").GetValue<int>();
        //     string itemListId = "petShop";
        //     bool flag1 = true;
        //     int pageNum = 1;
        //     int num1 = 0;
        //     float num2 = 4f;
        //     float num3 = num2 * 2f;
        //     int total = Mathf.CeilToInt((float)itemList.Count / num3);
        //     if (pageNum < 1 || pageNum > total)
        //     {
        //         pageNum = currentItemsPageNum;
        //     }
        //     else
        //     {
        //         currentItemsPageNum = pageNum;
        //         __instance.ClearCraftPages();
        //         int num4 = 0;
        //         for (int index = 0; index < itemList.Count; ++index)
        //         {
        //             if ((double)num4 >= (double)(pageNum - 1) * (double)num3 && (double)num4 < (double)pageNum * (double)num3)
        //             {
        //                 CardCraftItem component;
        //                 CardData cardData;
        //                 if (!craftCardItemDict.ContainsKey(num1))
        //                 {
        //                     GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.cardCraftItem, new Vector3(0.0f, 0.0f, -3f), Quaternion.identity, __instance.cardItemContainer);
        //                     component = gameObject.transform.GetComponent<CardCraftItem>();
        //                     gameObject.name = itemList[index];
        //                     craftCardItemDict.Add(num1, component);
        //                     int num5 = Mathf.FloorToInt((float)num1 / num2);
        //                     float x = (float)((double)__instance.cardItemContainer.transform.localPosition.x - 1.2000000476837158 + (double)num1 % (double)num2 * 2.5);
        //                     float y = (float)((double)__instance.cardItemContainer.transform.localPosition.y + 3.0499999523162842 - 4.1999998092651367 * (double)num5);
        //                     component.SetPosition(new Vector3(x, y, 0.0f));
        //                     component.SetIndex(num1);
        //                     component.SetHero(currentHero);
        //                     component.SetGenericCard(true);
        //                     cardData = Globals.Instance.GetCardData(itemList[index], false);
        //                     // int cost = __instance.SetPrice("Item", Enum.GetName(typeof(Enums.CardRarity), (object)cardData.CardRarity), itemList[index], __instance.craftTierZone);
        //                     // component.SetButtonTextItem(__instance.ButtonText(cost));
        //                     component.SetCard(itemList[index], itemListId, currentHero);
        //                 }
        //                 else
        //                 {
        //                     component = craftCardItemDict[num1];
        //                     component.gameObject.name = itemList[index];
        //                     component.gameObject.SetActive(true);
        //                     cardData = Globals.Instance.GetCardData(itemList[index], false);
        //                     // int cost = __instance.SetPrice("Item", Enum.GetName(typeof(Enums.CardRarity), (object)cardData.CardRarity), itemList[index], __instance.craftTierZone);
        //                     // component.SetButtonTextItem(__instance.ButtonText(cost));
        //                     component.SetCard(itemList[index], itemListId, currentHero);
        //                 }
        //                 bool flag3 = false;
        //                 if ((UnityEngine.Object)component != (UnityEngine.Object)null)
        //                 {
        //                     string key = itemListId + component.cardId;
        //                     if (AtOManager.Instance.boughtItemInShopByWho != null && AtOManager.Instance.boughtItemInShopByWho.ContainsKey(key))
        //                     {
        //                         __instance.ShowPortraitItemBought(AtOManager.Instance.boughtItemInShopByWho[key], _CCI: component);
        //                         component.EnableButton(false);
        //                         component.ShowDisable(true);
        //                         component.ShowSold(true);
        //                         flag3 = true;
        //                     }
        //                     else
        //                     {
        //                         Transform transform = component.transform.GetChild(1).transform.GetChild(0).transform.Find("itemBuyer");
        //                         if ((UnityEngine.Object)transform != (UnityEngine.Object)null)
        //                             UnityEngine.Object.Destroy((UnityEngine.Object)transform.gameObject);
        //                     }
        //                 }
        //                 else if ((UnityEngine.Object)component != (UnityEngine.Object)null)
        //                 {
        //                     if (!flag3)
        //                         component.ShowSold(false);
        //                     if (!flag1 || !PlayerManager.Instance.IsCardUnlocked(itemList[index]))
        //                     {
        //                         component.ShowDisable(true);
        //                         component.EnableButton(false);
        //                     }
        //                     else if (!flag3)
        //                     {
        //                         component.ShowSold(false);
        //                         if (CardCraftManager.Instance.CanBuy("Item", component.cardId))
        //                         {
        //                             component.EnableButton(true);
        //                             component.ShowDisable(false);
        //                         }
        //                         else
        //                         {
        //                             component.EnableButton(false);
        //                             component.ShowDisable(true);
        //                         }
        //                     }
        //                 }
        //                 if ((UnityEngine.Object)component != (UnityEngine.Object)null && component.enabled)
        //                 {
        //                     if ((UnityEngine.Object)cardData != (UnityEngine.Object)null && cardData.CardType == Enums.CardType.Pet && cardData.Sku != "" && !SteamManager.Instance.PlayerHaveDLC(cardData.Sku))
        //                     {
        //                         component.EnableButton(false);
        //                         component.ShowDisable(true);
        //                         string _text = string.Format(Texts.Instance.GetText("requiredDLC"), (object)SteamManager.Instance.GetDLCName(cardData.Sku));
        //                         component.ShowLock(true, _text);
        //                     }
        //                     else
        //                         component.ShowLock(false);
        //                 }
        //                 ++num1;
        //             }
        //             ++num4;
        //         }
        //         if (total > 1)
        //         {
        //             if ((double)num1 < (double)num2 * 2.0)
        //             {
        //                 for (int key = num1; (double)key < (double)num2 * 2.0; ++key)
        //                 {
        //                     if (craftCardItemDict.ContainsKey(key))
        //                         craftCardItemDict[key].transform.gameObject.SetActive(false);
        //                 }
        //             }
        //             // __instance.CreateCraftPages(pageNum, total);
        //         }
                
        //     }
        // }
    }
}

