using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HISP.Server;

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
                    return GetSaddle().IconId;
                }
            }
            public string SetName;
            private List<Item.ItemInformation> tackItems;
            public void Add(Item.ItemInformation item)
            {
                Logger.DebugPrint("Added "+item.Name+" To Tack Set: "+this.SetName);
                tackItems.Add(item);
            }

            public Item.ItemInformation GetSaddle()
            {
                foreach(Item.ItemInformation tackItem in TackItems)
                {
                    if(tackItem.MiscFlags[0] == 1) // Saddle
                        return tackItem;
                }
                throw new KeyNotFoundException("Saddle not found.");
            }


            public Item.ItemInformation GetSaddlePad()
            {
                foreach(Item.ItemInformation tackItem in TackItems)
                {
                    if(tackItem.MiscFlags[0] == 2) // SaddlePad
                        return tackItem;
                }
                throw new KeyNotFoundException("SaddlePad not found.");
            }

            public Item.ItemInformation GetBridle()
            {
                foreach(Item.ItemInformation tackItem in TackItems)
                {
                    if(tackItem.MiscFlags[0] == 3) // Bridle
                        return tackItem;
                }
                throw new KeyNotFoundException("GetBridle not found.");
            }

            public string[] GetSwfNames()
            {
                string[] swfs = new string[3];
                swfs[0] = GetSaddle().EmbedSwf;
                swfs[1] = GetSaddlePad().EmbedSwf;
                swfs[2] = GetBridle().EmbedSwf;

                return swfs;
            }

            public Item.ItemInformation[] TackItems
            {
                get
                {
                    return tackItems.ToArray();
                }
            }
        }
        private static string capitalizeFirstLetter(string str)
        {
            char firstChar = char.ToUpper(str[0]);
            return firstChar + str.Substring(1);
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
            foreach(TackSet set in tackSets)
            {
                if(set.SetName == name)
                {
                    return set;
                }
            }
            throw new KeyNotFoundException("No TackSet with name: "+name+" was found.");
        }

        public static void GenerateTackSets()
        {
            foreach(Item.ItemInformation itemInfo in Item.Items)
            {
                if(itemInfo.Type == "TACK")
                {

                    try
                    {
                        TackSet set = GetSetByName(capitalizeFirstLetter(itemInfo.EmbedSwf));
                        set.Add(itemInfo);
                    }
                    catch(KeyNotFoundException)
                    {                   
                        TackSet tackSet = new TackSet();
                        tackSet.SetName = capitalizeFirstLetter(itemInfo.EmbedSwf);
                        Logger.DebugPrint("Created Tack Set: "+tackSet.SetName); 
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