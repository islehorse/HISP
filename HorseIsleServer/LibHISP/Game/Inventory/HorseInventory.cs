using HISP.Game.Horse;
using HISP.Player;
using HISP.Server;
using HISP.Util;
using System.Collections.Generic;
using System.Linq;

namespace HISP.Game.Inventory
{
    public class HorseInventory
    {
        private User baseUser;
        private ThreadSafeList<HorseInstance> horsesList = new ThreadSafeList<HorseInstance>();
        public HorseInstance[] HorseList
        {
            get
            {
                List<HorseInstance> filteredHorseList = new List<HorseInstance>();
                foreach(HorseInstance horse in horsesList)
                {
                    if (!horse.Hidden)
                        filteredHorseList.Add(horse);
                }

                return filteredHorseList.ToArray();
            }
        }

        public HorseInventory(User user)
        {
            baseUser = user;
            Database.LoadHorseInventory(this, baseUser.Id);
        }

        public void UnHide(int uniqueId)
        {
            foreach(HorseInstance inst in horsesList)
            {
                if (inst.UniqueId == uniqueId)
                {
                    inst.Hidden = false;
                    break;
                }
            }
        }
        public void AddHorse(HorseInstance horse, bool addToDb=true, bool ignoreFull=false)
        {
            if (HorseList.Length + 1 > baseUser.MaxHorses && !ignoreFull)
                throw new InventoryFullException();

            horse.Owner = baseUser.Id;
            if(addToDb)
                Database.AddHorse(horse);
            horsesList.Add(horse);
        }

        public void DeleteHorseId(int id, bool removeFromDb = true)
        {
            foreach(HorseInstance horse in HorseList)
            {
                if(horse.UniqueId == id)
                {
                    if (removeFromDb)
                        Database.RemoveHorse(horse.UniqueId);
                    horsesList.Remove(horse);

                }
            }
        }

        public void DeleteHorse(HorseInstance horse, bool removeFromDb=true)
        {
            DeleteHorseId(horse.UniqueId, removeFromDb);
        }
        
        public bool HorseIdExist(int uniqueId)
        {
            return HorseList.Any(o => o.UniqueId == uniqueId);
        }
        public HorseInstance GetHorseById(int uniqueId)
        {
            return HorseList.First(o => o.UniqueId == uniqueId);
        }

        public HorseInstance[] GetHorsesInCategory(HorseInfo.Category category)
        {
            List<HorseInstance> instances = new List<HorseInstance>();
            foreach(HorseInstance horse in HorseList)
            {
                if (horse.Category == category.Name)
                {
                    instances.Add(horse);
                }
            }
            return instances.ToArray();
        }
    }
}
