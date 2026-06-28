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
                return horsesList.Where(o => !o.Hidden).ToArray();
            }
        }

        public HorseInventory(User user)
        {
            baseUser = user;
            Database.LoadHorseInventory(this, baseUser.Id);
        }

        public void UnHide(int uniqueId)
        {
            HorseInstance inst = horsesList.FirstOrDefault(o => o.UniqueId == uniqueId, null);
            if(inst != null) inst.Hidden = false;
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
            int rm = horsesList.RemoveAll(o => o.UniqueId == id);
            if (removeFromDb && rm > 0) Database.RemoveHorse(id);
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
            return HorseList.Where(o => o.Category == category.Name).ToArray();
        }
    }
}
