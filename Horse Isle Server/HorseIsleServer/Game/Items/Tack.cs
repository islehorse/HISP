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
            
            public string SetName;
            private List<Item.ItemInformation> tackItems;
            public void Add(Item.ItemInformation item)
            {
                Logger.DebugPrint("Added "+item.Name+" To Tack Set: "+this.SetName);
                tackItems.Add(item);
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
        public TackSet[] TackSets
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
                        continue;
                    }
                    
                    TackSet tackSet = new TackSet();
                    tackSet.SetName = capitalizeFirstLetter(itemInfo.EmbedSwf);
                    tackSet.Add(itemInfo);
                    tackSets.Add(tackSet);
                    Logger.DebugPrint("Created Tack Set: "+tackSet.SetName);
                
                }
            }
        }
    }
}