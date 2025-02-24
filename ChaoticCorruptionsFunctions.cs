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

namespace ChaoticCorruptions
{
    public class ChaoticCorruptionsFunctions
    {

        public static CardData GetRandomCraftableCard()
        {
            return null;
        }

        public static CardData GetFullyRandomCard(Hero hero)
        {
            int commonChance = 35;
            int uncommonChance = 30;
            int rareChance = 20;
            int epicChance = 10;
            // int mythicChance = 5;
    
            Enums.CardClass result1 = Enums.CardClass.None;
            Enum.TryParse<Enums.CardClass>(Enum.GetName(typeof (Enums.HeroClass), (object) hero.HeroData.HeroClass), out result1);
            Enums.CardClass result2 = Enums.CardClass.None;
            Enum.TryParse<Enums.CardClass>(Enum.GetName(typeof(Enums.HeroClass), (object)hero.HeroData.HeroSubClass.HeroClassSecondary), out result2);
            List<string> stringList1 = Globals.Instance.CardListNotUpgradedByClass[result1];
            List<string> stringList2 = result2 == Enums.CardClass.None ? new List<string>() : Globals.Instance.CardListNotUpgradedByClass[result2];
            int index1 = 1;
            int num10 = UnityEngine.Random.Range(0, 100);
            CardData _cardData = Globals.Instance.GetCardData(index1 < 2 || result2 == Enums.CardClass.None ? stringList1[UnityEngine.Random.Range(0, stringList1.Count)] : stringList2[UnityEngine.Random.Range(0, stringList2.Count)], false);
            bool flag2 = true;
            while (flag2)
            {
                flag2 = false;
                bool flag3 = false;
                while (!flag3)
                {
                    flag2 = false;
                    // CardData _cardData = Globals.Instance.GetCardData(index1 < 2 || result2 == Enums.CardClass.None ? stringList1[UnityEngine.Random.Range(0, stringList1.Count)] : stringList2[UnityEngine.Random.Range(0, stringList2.Count)], false);
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
    }
}

