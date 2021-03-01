using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.Chat
{
    public class SocialType
    {
        public SocialType(string type)
        {
            Socials = new List<Social>();
            Type = type;
            SocialTypes.Add(this);
        }
        public static List<SocialType> SocialTypes = new List<SocialType>();
        public string Type;
        public List<Social> Socials;
        public class Social
        {
            public SocialType BaseSocialType;

            public int Id;
            public string ButtonName;
            public string ForSender;
            public string ForTarget;
            public string ForEveryone;
            public string SoundEffect;
        }

        public static Social GetSocial(int socialId)
        {
            foreach (SocialType sType in SocialTypes)
                foreach (Social social in sType.Socials)
                    if (social.Id == socialId)
                        return social;
            throw new KeyNotFoundException("Social " + socialId.ToString() + " not found!");
        }
        public static SocialType GetSocialType(string type)
        {
            foreach (SocialType stype in SocialTypes)
                if (stype.Type == type)
                    return stype;
            throw new KeyNotFoundException("SocialType " + type + " NOT FOUND!");
        }
        public static void AddNewSocial(string type, Social social)
        {
            foreach(SocialType stype in SocialTypes)
            {
                if(stype.Type == type)
                {
                    social.BaseSocialType = stype;
                    stype.Socials.Add(social);
                    return; 
                }
            }
            SocialType sType = new SocialType(type);
            social.BaseSocialType = sType;
            sType.Socials.Add(social);
            return;
        }
    }
}
