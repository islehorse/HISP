using HISP.Game;
using HISP.Game.Horse;
using HISP.Game.Items;
using HISP.Game.Services;
using HISP.Game.SwfModules;
using HISP.Game.Chat;
using HISP.Security;

using System;
using System.Diagnostics;

namespace HISP.Server
{
    public static class Entry
    {
        // "Entry Point"

        private static void defaultOnShutdownCallback()
        {
            Process.GetCurrentProcess().Close();
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

        public static void Start()
        {
            RegisterCrashHandler();
            Console.Title = ServerVersion.GetBuildString();
            ConfigReader.OpenConfig();
            CrossDomainPolicy.GetPolicyFile();
            Database.OpenDatabase();
            GameDataJson.ReadGamedata();

            Map.OpenMap();
            World.ReadWorldData();
            Treasure.Init();

            DroppedItems.Init();
            WildHorse.Init();

            Drawingroom.LoadAllDrawingRooms();
            Brickpoet.LoadPoetryRooms();
            Multiroom.CreateMultirooms();

            Auction.LoadAllAuctionRooms();

            Item.DoSpecialCases();
            Command.RegisterCommands();

            GameServer.StartServer();
        }

        private static void HispCrashHandler(object sender, UnhandledExceptionEventArgs e)
        {

            string[] becauseRustWont = {
                "It was from the artists and poets that the pertinent answers came, and I\r\nknow that panic would have broken loose had they been able to compare notes.\r\nAs it was, lacking their original letters, I half suspected the compiler of\r\nhaving asked leading questions, or of having edited the correspondence in\r\ncorroboration of what he had latently resolved to see.",
                "There are not many persons who know what wonders are opened to them in the\r\nstories and visions of their youth; for when as children we listen and dream,\r\nwe think but half-formed thoughts, and when as men we try to remember, we are\r\ndulled and prosaic with the poison of life. But some of us awake in the night\r\nwith strange phantasms of enchanted hills and gardens, of fountains that sing\r\nin the sun, of golden cliffs overhanging murmuring seas, of plains that stretch\r\ndown to sleeping cities of bronze and stone, and of shadowy companies of heroes\r\nthat ride caparisoned white horses along the edges of thick forests; and then\r\nwe know that we have looked back through the ivory gates into that world of\r\nwonder which was ours before we were wise and unhappy.",
                "Instead of the poems I had hoped for, there came only a shuddering blackness\r\nand ineffable loneliness; and I saw at last a fearful truth which no one had\r\never dared to breathe before — the unwhisperable secret of secrets — The fact\r\nthat this city of stone and stridor is not a sentient perpetuation of Old New\r\nYork as London is of Old London and Paris of Old Paris, but that it is in fact\r\nquite dead, its sprawling body imperfectly embalmed and infested with queer\r\nanimate things which have nothing to do with it as it was in life.",
                "The ocean ate the last of the land and poured into the smoking gulf, thereby\r\ngiving up all it had ever conquered. From the new-flooded lands it flowed\r\nagain, uncovering death and decay; and from its ancient and immemorial bed it\r\ntrickled loathsomely, uncovering nighted secrets of the years when Time was\r\nyoung and the gods unborn. Above the waves rose weedy remembered spires. The\r\nmoon laid pale lilies of light on dead London, and Paris stood up from its damp\r\ngrave to be sanctified with star-dust. Then rose spires and monoliths that were\r\nweedy but not remembered; terrible spires and monoliths of lands that men never\r\nknew were lands...",
                "There was a night when winds from unknown spaces whirled us irresistibly into\r\nlimitless vacuum beyond all thought and entity. Perceptions of the most\r\nmaddeningly untransmissible sort thronged upon us; perceptions of infinity\r\nwhich at the time convulsed us with joy, yet which are now partly lost to my\r\nmemory and partly incapable of presentation to others.",
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
