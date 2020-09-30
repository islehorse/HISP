using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Mailbox
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
