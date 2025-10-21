using System.Collections.Generic;

namespace HISP.Game
{
    public class BBCode
    {
        public BBCode(string tag, string meta)
        {
            Tag = tag;
            MetaTranslation = meta;
            bbcodeTranslations.Add(this);
        }
        private static List<BBCode> bbcodeTranslations = new List<BBCode>();
        public string Tag;
        public string MetaTranslation;

        public static string EncodeMetaToBBCode(string message)
        {
            foreach (BBCode code in bbcodeTranslations)
            {
                message = message.Replace(code.MetaTranslation, code.Tag);
            }
            return message;
        }
        public static string EncodeBBCodeToMeta(string message)
        {
            foreach(BBCode code in bbcodeTranslations)
            {
                message = message.Replace(code.Tag, code.MetaTranslation);
                message = message.Replace(code.Tag.ToUpper(), code.MetaTranslation);
            }
            return message;
        }
    }
}
