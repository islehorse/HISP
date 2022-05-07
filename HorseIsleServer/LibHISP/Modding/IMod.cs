namespace HISP.Modding
{
    public interface IMod
    {
        public void OnModLoad();
        public void OnModUnload();
        public string ModName
        {
            get;
        }
        public string ModVersion
        {
            get;
        }
        public string ModId
        {
            get;
        }
    }
}
