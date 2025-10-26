using System.Collections.Generic;
using System.Linq;
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
            return ReportReasons.Any(o => o.Id == id);
        }
        public static ReportReason GetReasonById(string id)
        {
            return ReportReasons.First(o => o.Id == id);
        }
        public static void AddReason(ReportReason reason)
        {
            reportReasons.Add(reason);
        }
    }
}
