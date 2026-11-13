using HISP.Security;

namespace HISP.Player
{
    public class MailItem
    {
        private string message;
        private string subject;

        public int UniqueId;
        public bool Read;
        public int FromUser;
        public int ToUser;
        public string Subject
        {
            get
            {
                return MetaSanitizer.Sanitize(subject);
            }
            set
            {
                subject = value;
            }
        }
        public string Message
        {
            get
            {
                return MetaSanitizer.Sanitize(message);
            }
            set
            {
                message = value;
            }
        }
        public int Timestamp;
    }
}
