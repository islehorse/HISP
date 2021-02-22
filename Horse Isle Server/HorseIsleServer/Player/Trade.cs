using HISP.Game.Horse;
using HISP.Game.Items;
using HISP.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Player
{
    public class Trade
    {
        public Trade(User me)
        {
            RandomId = RandomID.NextRandomId();
            Trader = me;
        }

        public int RandomId;
        public User Trader;
        public Trade OtherTrade;

        public string Stage = "OPEN";

        public int MoenyOffered = 0;
        public List<HorseInstance> HorsesOffered = new List<HorseInstance>();
        public List<ItemInstance> ItemsOffered = new List<ItemInstance>();
        
    }
}
