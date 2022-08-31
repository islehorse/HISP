using HISP.Game.Horse;
using HISP.Player;
using HISP.Server;
using System.Collections.Generic;

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

        public void UnHide(int randomId)
        {
            foreach(HorseInstance inst in horsesList)
            {
                if (inst.RandomId == randomId)
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
                if(horse.RandomId == id)
                {
                    if (removeFromDb)
                        Database.RemoveHorse(horse.RandomId);
                    horsesList.Remove(horse);

                }
            }
        }

        public void DeleteHorse(HorseInstance horse, bool removeFromDb=true)
        {
            DeleteHorseId(horse.RandomId, removeFromDb);
        }
        
        public bool HorseIdExist(int randomId)
        {
            try
            {
                GetHorseById(randomId);
                return true;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }
        }
        public HorseInstance GetHorseById(int randomId)
        {
            foreach(HorseInstance inst in HorseList)
            {
                if (inst.RandomId == randomId)
                    return inst;
            }
            throw new KeyNotFoundException();
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
