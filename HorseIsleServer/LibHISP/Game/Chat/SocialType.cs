using System.Collections.Generic;

namespace HISP.Game.Chat
{
    public class SocialType
    {
        public SocialType(string type)
        {
            socials = new List<Social>();
            Type = type;
            socialTypes.Add(this);
        }
        private static List<SocialType> socialTypes = new List<SocialType>();
        public string Type;
        private List<Social> socials;
        public void AddSocial(Social social)
        {
            socials.Add(social);
        }
        public static SocialType[] SocialTypes
        {
            get
            {
                return socialTypes.ToArray();
            }
        }
        public Social[] Socials
        {
            get
            {
                return socials.ToArray();
            }
        }
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
                    stype.AddSocial(social);
                    return; 
                }
            }
            SocialType sType = new SocialType(type);
            social.BaseSocialType = sType;
            sType.AddSocial(social);
            return;
        }
    }
}
