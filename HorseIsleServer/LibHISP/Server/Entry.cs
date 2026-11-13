using HISP.Game;
using HISP.Game.Chat;
using HISP.Game.Horse;
using HISP.Game.Items;
using HISP.Game.Services;
using HISP.Game.SwfModules;
using HISP.Security;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace HISP.Server
{
    public static class Entry
    {
        // "Entry Point"
        private static void defaultOnShutdownCallback()
        {
            Process.GetCurrentProcess().Kill();
        }

        public static Action OnShutdown = defaultOnShutdownCallback;

        public static void SetShutdownCallback(Action callback)
        {
            OnShutdown = callback;
        }
        
        public static void RegisterCrashHandler()
        {
#if (!OS_DEBUG)
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HispCrashHandler);
#endif
        }

        public static Action[] ShutdownSteps =
        {
            GameServer.OnShutdown,
            GameClient.OnShutdown,
            Database.OnShutdown,

            Entry.OnShutdown,
        };

        public static Action[] StartupSteps = {
            // setup crash handler
            Entry.CreateSignals,
            Entry.RegisterCrashHandler,

            // config
            ConfigReader.OpenConfig,
            Database.OpenDatabase,
            GameDataJson.ReadGamedata,
            
            // map, game time, etc
            Map.OpenMap,
            World.ReadWorldData,

            // initalize map objects 
            Treasure.Init,
            DroppedItems.Init,
            WildHorse.Init,

            // load / setup data
            Drawingroom.LoadAllDrawingRooms,
            Brickpoet.LoadPoetryRooms,
            Multiroom.CreateMultirooms,
            Auction.LoadAllAuctionRooms,
            Tack.GenerateTackSets,
            Command.RegisterCommands,

            // server
            PacketSigning.Init,
            GameServer.StartServer
        };

        public static void CreateSignals()
        {
            PosixSignalRegistration.Create(PosixSignal.SIGTERM, (_) => { Entry.Shutdown("Server process received SIGTERM."); });
            PosixSignalRegistration.Create(PosixSignal.SIGQUIT, (_) => { Entry.Shutdown("Server process received SIGQUIT."); });
            PosixSignalRegistration.Create(PosixSignal.SIGINT, (_) => { Entry.Shutdown("Server process received SIGINT."); });
        }


        public static void Shutdown(string shutdownReason = "No reason provided.")
        {
            Logger.InfoPrint(shutdownReason);
            Logger.InfoPrint("Shutting down server ...");

            try
            {
                foreach (Action shutdownStep in ShutdownSteps) shutdownStep();
            }
            catch (Exception)
            {
                Process.GetCurrentProcess().Kill();
            }
        }
        public static void Start()
        {
            Directory.SetCurrentDirectory(ConfigReader.ConfigDirectory);
            Console.Title = ServerVersion.GetBuildString();

            Logger.InfoPrint("Starting " + ServerVersion.GetBuildString());
            Logger.InfoPrint("Config Directory: "+ ConfigReader.ConfigDirectory);
            Logger.InfoPrint("Assets Directory: " + ConfigReader.AssetsDirectory);

            foreach(Action startupStep in StartupSteps) startupStep();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static void HispCrashHandler(object sender, UnhandledExceptionEventArgs e)
        {

            string[] becauseRustWont = {
                "It was from the artists and poets that the pertinent answers came, and I\nknow that panic would have broken loose had they been able to compare notes.\nAs it was, lacking their original letters, I half suspected the compiler of\nhaving asked leading questions, or of having edited the correspondence in\ncorroboration of what he had latently resolved to see.",
                "There are not many persons who know what wonders are opened to them in the\nstories and visions of their youth; for when as children we listen and dream,\nwe think but half-formed thoughts, and when as men we try to remember, we are\ndulled and prosaic with the poison of life. But some of us awake in the night\nwith strange phantasms of enchanted hills and gardens, of fountains that sing\nin the sun, of golden cliffs overhanging murmuring seas, of plains that stretch\ndown to sleeping cities of bronze and stone, and of shadowy companies of heroes\nthat ride caparisoned white horses along the edges of thick forests; and then\nwe know that we have looked back through the ivory gates into that world of\nwonder which was ours before we were wise and unhappy.",
                "Instead of the poems I had hoped for, there came only a shuddering blackness\nand ineffable loneliness; and I saw at last a fearful truth which no one had\never dared to breathe before — the unwhisperable secret of secrets — The fact\nthat this city of stone and stridor is not a sentient perpetuation of Old New\nYork as London is of Old London and Paris of Old Paris, but that it is in fact\nquite dead, its sprawling body imperfectly embalmed and infested with queer\nanimate things which have nothing to do with it as it was in life.",
                "The ocean ate the last of the land and poured into the smoking gulf, thereby\ngiving up all it had ever conquered. From the new-flooded lands it flowed\nagain, uncovering death and decay; and from its ancient and immemorial bed it\ntrickled loathsomely, uncovering nighted secrets of the years when Time was\nyoung and the gods unborn. Above the waves rose weedy remembered spires. The\nmoon laid pale lilies of light on dead London, and Paris stood up from its damp\ngrave to be sanctified with star-dust. Then rose spires and monoliths that were\nweedy but not remembered; terrible spires and monoliths of lands that men never\nknew were lands...",
                "There was a night when winds from unknown spaces whirled us irresistibly into\nlimitless vacuum beyond all thought and entity. Perceptions of the most\nmaddeningly untransmissible sort thronged upon us; perceptions of infinity\nwhich at the time convulsed us with joy, yet which are now partly lost to my\nmemory and partly incapable of presentation to others.",
                "You've met with a terrible fate, haven't you?"
            };

            Exception execpt = (Exception)e.ExceptionObject;

            string crashMsg = becauseRustWont[execpt.GetHashCode() % becauseRustWont.Length] + "\n";
            crashMsg += "HISP HAS CRASHED" + "\n";
            crashMsg += "Build: " + ServerVersion.GetBuildString() + "\n";
            crashMsg += "Unhandled Exception: " + execpt.Message + "\n";
            crashMsg += execpt.StackTrace + "\n";

            Logger.CrashPrint(crashMsg);

        }
    }
}
