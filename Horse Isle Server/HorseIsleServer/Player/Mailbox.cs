using HISP.Server;

namespace HISP.Player
{
    public class Mailbox
    {
        private User baseUser;
        public int MailCount;

        public Mailbox(User user)
        {
            MailCount = Database.CheckMailcount(user.Id);
            baseUser = user;
        }
    }
}
