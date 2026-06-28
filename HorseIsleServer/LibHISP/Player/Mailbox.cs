using HISP.Game;
using HISP.Game.Items;
using HISP.Server;
using HISP.Util;
using System.Linq;

namespace HISP.Player
{
    public class Mailbox
    {
        public class Mail
        {
            public int UniqueId;
            public bool Read;
            public int FromUser;
            public int ToUser;
            public string Subject;
            public string Message;
            public int Timestamp;
        }

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
                return MailMessages.Count(o => !o.Read);
            }
        }
        public Mail[] MailMessages 
        { 
            get
            {
                return mails.ToArray();
            }
        }


        public void RipUpMessage(Mail message)
        {
            Database.DeleteMail(message.UniqueId);
            mails.Remove(message);

            ItemInstance item = baseUser.Inventory.GetItemByItemId(Item.MailMessage).ItemInstances.FirstOrDefault(o => o.Data == message.UniqueId, null);
            if(item != null) baseUser.Inventory.Remove(item);

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
                    ItemInstance mailMessageFromPlayer = new ItemInstance(Item.MailMessage, -1, mails[i].UniqueId);
                    baseUser.Inventory.AddIgnoringFull(mailMessageFromPlayer);
                }
                mails[i].Read = true;
            }

            GameServer.UpdatePlayer(baseUser.Client);
        }
        public void AddMail(Mail mailMessage)
        {
            mails.Add(mailMessage);
            Database.AddMail(mailMessage.UniqueId, mailMessage.ToUser, mailMessage.FromUser, mailMessage.Subject, mailMessage.Message, mailMessage.Timestamp, mailMessage.Read);
        }
        public bool MessageExists(int uniqueId)
        {
			return MailMessages.Any(o => o.UniqueId == uniqueId);
        }
        public Mail GetMessageByUniqueId(int uniqueId)
        {
            return MailMessages.First(o => o.UniqueId == uniqueId);
        }
        public Mailbox(User user)
        {
            baseUser = user;
            Mail[] mailMessages = Database.LoadMailbox(user.Id);
            mails.AddRange(mailMessages);
        }
    }
}
