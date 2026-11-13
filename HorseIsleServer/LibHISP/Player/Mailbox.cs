using HISP.Game;
using HISP.Game.Items;
using HISP.Security;
using HISP.Server;
using HISP.Util;
using System.Linq;

namespace HISP.Player
{
    public class Mailbox
    {
        

        private User baseUser;
        private ThreadSafeList<MailItem> mails = new ThreadSafeList<MailItem>();
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
        public MailItem[] MailMessages 
        { 
            get
            {
                return mails.ToArray();
            }
        }


        public void RipUpMessage(MailItem message)
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
        public void AddMail(MailItem mailMessage)
        {
            mails.Add(mailMessage);
            Database.AddMail(mailMessage.UniqueId, mailMessage.ToUser, mailMessage.FromUser, mailMessage.Subject, mailMessage.Message, mailMessage.Timestamp, mailMessage.Read);
        }
        public bool MessageExists(int uniqueId)
        {
			return MailMessages.Any(o => o.UniqueId == uniqueId);
        }
        public MailItem GetMessageByUniqueId(int uniqueId)
        {
            return MailMessages.First(o => o.UniqueId == uniqueId);
        }
        public Mailbox(User user)
        {
            baseUser = user;
            MailItem[] mailMessages = Database.LoadMailbox(user.Id);
            mails.AddRange(mailMessages);
        }
    }
}
