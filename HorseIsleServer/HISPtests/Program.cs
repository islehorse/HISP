using HISP.Server;
using HISP.Tests.UnitTests;
using System;
using System.Threading.Tasks;

namespace HISP.Tests
{
    public static class Program
    {

        public static async Task Main(string[] args)
        {
            ServerStartTest.RunServerStartTest();
            AuthenticationTest.RunAuthenticationTest();
            await UserTest.RunUserTest();
            PacketTest.RunPacketTest();
        }
    }

}