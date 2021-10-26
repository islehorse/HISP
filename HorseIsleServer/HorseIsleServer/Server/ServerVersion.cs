using HISP.Properties;

namespace HISP.Server
{
    public class ServerVersion
    {
        public static int MAJOR = 1;
        public static int MINOR = 2;
        public static string PRODUCT = "HISP";

        public static string GetArchitecture()
        {
#if ARCH_ANYCPU
            return "ANYCPU";
#elif ARCH_X86_64
            return "x86_64";
#elif ARCH_X86
            return "x86";
#elif ARCH_ARM
            return "ARM";
#elif ARCH_ARM64
            return "ARM64";
#else
            return "UNK_ARCH";
#endif
        }
        public static string GetPlatform()
        {
#if OS_DEBUG
            return "DEBUG";
#elif OS_WINDOWS
            return "WINDOWS";
#elif OS_LINUX
            return "LINUX";
#elif OS_MACOS
            return "MACOS";
#else
            return "UNK_PLATFORM";
#endif

        }
        public static string GetVersionString()
        {
            return "v" + MAJOR.ToString() + "." + MINOR.ToString();
        }
        public static string GetCommitHash(int TotalBytes)
        {
            return Resources.GitCommit.Substring(0, TotalBytes * 2);
        }
        public static string GetBuildString()
        {
            return PRODUCT + " " + GetVersionString() + " @" + GetCommitHash(4) + "; (" + GetArchitecture() + "; " + GetPlatform() + ")";
        }
    }
}
