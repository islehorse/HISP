using HISP.Game;
using HISP.Game.Inventory;
using HISP.Game.Items;
using HISP.Server;
using HISP.Util;
using System.Collections.Generic;
using System.Linq;

namespace HISP.Player
{
    public class Mailbox
    {
        private User baseUser;
        private ThreadSafeList<Mail> mails = new ThreadSafeList<Mail>();
        public int MailCount 
        {
            get
            {
                return MailMessages.Length;
            }
        }
        public int UnreadMailCount
        {
            get
            {
                int i = 0;
                foreach (Mail mail in MailMessages)
                {
                    if (!mail.Read)
                    {
                        i++;
                    }
                }
                return i;
            }
        }
        public Mail[] MailMessages 
        { 
            get
            {
                return mails.ToArray();
            }
        }

        public class Mail
        {
            public int RandomId;
            public bool Read;
            public int FromUser;
            public int ToUser;
            public string Subject;
            public string Message;
            public int Timestamp;
        }

        public void RipUpMessage(Mail message)
        {
            Database.DeleteMail(message.RandomId);
            mails.Remove(message);

            InventoryItem item = baseUser.Inventory.GetItemByItemId(Item.MailMessage);
            foreach(ItemInstance instance in item.ItemInstances)
            {
                if (instance.Data == message.RandomId)
                {
                    baseUser.Inventory.Remove(instance);
                    break;
                }
            }

            byte[] rippedUpMessage = PacketBuilder.CreateChat(Messages.MailRippedMessage, PacketBuilder.CHAT_BOTTOM_RIGHT);
            baseUser.Client.SendPacket(rippedUpMessage);
            GameServer.UpdateInventory(baseUser.Client);
        }

        public void ReadAllMail()
        {

            Database.ReadAllMail(baseUser.Id);

            for (int i = 0; i < MailCount; i++)
            {
                if(!mails[i].Read)
                {
                    ItemInstance mailMessageFromPlayer = new ItemInstance(Item.MailMessage, -1, mails[i].RandomId);
                    baseUser.Inventory.AddIgnoringFull(mailMessageFromPlayer);
                }
                mails[i].Read = true;
            }

            GameServer.UpdatePlayer(baseUser.Client);
        }
        public void AddMail(Mail mailMessage)
        {
            mails.Add(mailMessage);
            Database.AddMail(mailMessage.RandomId, mailMessage.ToUser, mailMessage.FromUser, mailMessage.Subject, mailMessage.Message, mailMessage.Timestamp, mailMessage.Read);
        }
        public bool MessageExists(int randomId)
        {
			return MailMessages.Any(o => o.RandomId == randomId);
        }
        public Mail GetMessageByRandomId(int randomId)
        {
            return MailMessages.First(o => o.RandomId == randomId);
        }
        public Mailbox(User user)
        {
            baseUser = user;
            Mail[] mailMessages = Database.LoadMailbox(user.Id);
            foreach (Mail mailMessage in mailMessages)
                mails.Add(mailMessage);

        }
    }
}
