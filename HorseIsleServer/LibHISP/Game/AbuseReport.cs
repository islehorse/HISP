using HISP.Server;
using System.Collections.Generic;
namespace HISP.Game
{
    public class AbuseReport
    {
        public struct ReportReason
        {
            public string Id;
            public string Name;
            public string Meta;
        }
        private static List<ReportReason> reportReasons = new List<ReportReason>();

        public static ReportReason[] ReportReasons
        {
            get
            {
                return reportReasons.ToArray();
            }
        }
        public static bool DoesReasonExist(string id)
        {
            try
            {
                GetReasonById(id);
                return true;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }
        }
        public static ReportReason GetReasonById(string id)
        {
            foreach(ReportReason reason in ReportReasons)
            {
                if (reason.Id == id)
                    return reason;
            }
            throw new KeyNotFoundException("No reason of: " + id + " Found.");
        }
        public static void AddReason(ReportReason reason)
        {
            reportReasons.Add(reason);
        }
    }
}
