using HISP.Game.Horse;
using HISP.Game.Items;
using HISP.Player;
using HISP.Server;
using System.Collections.Generic;

namespace HISP.Game.Events
{
    public class RandomEvent
    {
        private static List<RandomEvent> randomEvents = new List<RandomEvent>();
        public static RandomEvent GetRandomEvent()
        {
            int randomEventIndex = GameServer.RandomNumberGenerator.Next(0, randomEvents.Count);
            return randomEvents[randomEventIndex];
        }

        public static void ExecuteRandomEvent(User user)
        {
            while (true)
            {
                RandomEvent rngEvent = RandomEvent.GetRandomEvent();

                if (rngEvent.HorseHealthDown != 0 && user.HorseInventory.HorseList.Length <= 0)
                    continue;
                if (rngEvent.Text.Contains("%HORSENAME%") && user.HorseInventory.HorseList.Length <= 0)
                    continue;

                int moneyEarned = 0;
                if (rngEvent.MinMoney != 0 || rngEvent.MaxMoney != 0)
                    moneyEarned = GameServer.RandomNumberGenerator.Next(rngEvent.MinMoney, rngEvent.MaxMoney);


                if (moneyEarned < 0)
                    if (user.Money + moneyEarned < 0)
                        continue;

                if (rngEvent.GiveObject != 0)
                    user.Inventory.AddIgnoringFull(new ItemInstance(rngEvent.GiveObject));


                if(moneyEarned != 0)
                    user.AddMoney(moneyEarned);

                HorseInstance effectedHorse = null;

                if(user.HorseInventory.HorseList.Length > 0)
                {
                    int randomHorseIndex = GameServer.RandomNumberGenerator.Next(0, user.HorseInventory.HorseList.Length);
                    effectedHorse = user.HorseInventory.HorseList[randomHorseIndex];
                }

                if (rngEvent.HorseHealthDown != 0)
                    effectedHorse.BasicStats.Health -= rngEvent.HorseHealthDown;

                string horseName = "[This Message Should Not Appear, if it does its a bug.]";
                if (effectedHorse != null)
                    horseName = effectedHorse.Name;

                string msg = Messages.FormatRandomEvent(rngEvent.Text, moneyEarned, horseName);
                byte[] chatPacket = PacketBuilder.CreateChat(Messages.RandomEventPrefix + msg, PacketBuilder.CHAT_BOTTOM_RIGHT);
                user.Client.SendPacket(chatPacket);

                return;
            }
        }

        public RandomEvent(int id, string text, int minMoney, int maxMoney, int horseHealth, int giveObject)
        {
            Id = id;
            Text = text;
            MinMoney = minMoney;
            MaxMoney = maxMoney;
            HorseHealthDown = horseHealth;
            GiveObject = giveObject;

            randomEvents.Add(this);
        }
        public int Id;
        public string Text;
        public int MinMoney;
        public int MaxMoney;
        public int HorseHealthDown;
        public int GiveObject;

       

    }
}
