using HISP.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HISP.Modding
{
    public class ModLoader
    {
        private static List<IMod> modList = new List<IMod>();
        public static IMod[] ModList
        {
            get
            {
                return modList.ToArray();
            }
        }
        public static void LoadMod(IMod mod)
        {
            mod.OnModLoad(); // Call OnModLoad();
            modList.Add(mod); // add to the list of mods
            Logger.InfoPrint("Loaded mod: "+mod.ModName + " v" + mod.ModVersion + " SUCCESS.");
        }
        public static void UnloadMod(IMod mod)
        {
            mod.OnModUnload();
            modList.Remove(mod);
            Logger.InfoPrint("Unloading mod: " + mod.ModName);
        }
        public static void UnloadAllMods()
        {
            foreach (IMod loadedMod in ModList)
            {
                UnloadMod(loadedMod);
            }
        }
        public static void LoadModFromFilesystem(string dllfile)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(dllfile);
                Type[] types = assembly.GetTypes();
                // Search for classes that implement IMod
                foreach (Type type in types)
                {
                    if (type.GetInterfaces().Contains(typeof(IMod)))
                    {
                        IMod mod = (IMod)Activator.CreateInstance(type); // Crate an instance of the class
                        LoadMod(mod); // Load it into memory
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorPrint("Failed to load mod: " + dllfile + " - [EXCEPTION]" + e.Message + " " + e.InnerException + "\n" + e.StackTrace);
            }
        }
        public static void OnShutdown()
        {
            UnloadAllMods();
        }
        public static void ReloadModsFromFilesystem()
        {
            UnloadAllMods();
            if (Directory.Exists(ConfigReader.ModsFolder))
            {
                string[] filelist = Directory.GetFiles(ConfigReader.ModsFolder, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (string file in filelist)
                {
                    LoadModFromFilesystem(file);
                }
            }
        }
    }
}
