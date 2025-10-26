using System;
using System.Collections.Generic;
using System.Linq;

using HISP.Server;
using HISP.Util;

namespace HISP.Game.Items
{
    public class Tack
    {
        public class TackSet
        {
            public TackSet()
            {
                tackItems = new List<Item.ItemInformation>();
            }

            public int IconId
            {
                get
                {
                    return Saddle.IconId;
                }
            }
            public string SetName;
            private List<Item.ItemInformation> tackItems;
            public void Add(Item.ItemInformation item)
            {
                Logger.DebugPrint("Added " + item.Name + " To Tack Set: " + this.SetName);
                tackItems.Add(item);
            }

            public Item.ItemInformation Saddle 
            {
                get 
                {
                    return TackItems.First(o => o.GetMiscFlag(0) == 1); // Saddle
                }
            }

            public Item.ItemInformation SaddlePad
            {
                get
                {
                    return TackItems.First(o => o.GetMiscFlag(0) == 2); // SaddlePad
                }
            }

            public Item.ItemInformation Bridle
            {
                get
                {
                    return TackItems.First(o => o.GetMiscFlag(0) == 3); // Bridle
                }
            }

            public string[] GetSwfNames()
            {
                string[] swfs = new string[3];
                swfs[0] = Saddle.EmbedSwf;
                swfs[1] = SaddlePad.EmbedSwf;
                swfs[2] = Bridle.EmbedSwf;

                return swfs;
            }

            public Item.ItemInformation[] TackItems
            {
                get
                {
                    return tackItems.ToArray();
                }
            }
            public int SortPosition()
            {
                int pos = 0;
                foreach(Item.ItemInformation tackitem in TackItems)
                {
                    foreach(Item.Effects effect in tackitem.Effects)
                    {
                        pos += effect.EffectAmount;
                    }
                }
                return pos;
            }
        }

        private static List<TackSet> tackSets = new List<TackSet>();
        public static TackSet[] TackSets
        {
            get
            {
                return tackSets.ToArray();
            }
        }
        public static TackSet GetSetByName(string name)
        {
            return TackSets.First(o => o.SetName == name);
        }

        public static void GenerateTackSets()
        {
            foreach(Item.ItemInformation itemInfo in Item.Items)
            {
                if(itemInfo.Type == "TACK")
                {

                    try
                    {
                        TackSet set = GetSetByName(Helper.CapitalizeFirstLetter(itemInfo.EmbedSwf));
                        set.Add(itemInfo);
                    }
                    catch(InvalidOperationException)
                    {                   
                        TackSet tackSet = new TackSet();
                        tackSet.SetName = Helper.CapitalizeFirstLetter(itemInfo.EmbedSwf);
                        tackSet.Add(itemInfo);
                        tackSets.Add(tackSet);
                    }
                
                }
            }
            foreach(TackSet set in TackSets)
            {
                if(set.TackItems.Length < 3)
                {
                    Logger.DebugPrint("Removing set: "+set.SetName);
                    tackSets.Remove(set);
                }
            }
        }
    }
}