using HISP.Tests.Properties;
using HISP.Game.SwfModules;
using HISP.Game;
using HISP.Server;

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HISP.Tests.UnitTests
{
    public class PacketTest
    {
        private const bool GENERATE = false;

        private static Dictionary<string, string> knownGoodPackets = new Dictionary<string, string>();

        public static bool Test(string name, byte[] packet)
        {

            if (GENERATE)
            {
                knownGoodPackets.Add(name, Convert.ToBase64String(packet));
            }
            else
            {
                string goodPacketStr = null;
                knownGoodPackets.TryGetValue(name, out goodPacketStr);
                byte[] goodPacket = Convert.FromBase64String(goodPacketStr);


                if(!goodPacket.SequenceEqual(packet))
                {
                    ResultLogger.LogTestResult(false, "PACKET_TEST "+name, BitConverter.ToString(packet).Replace("-", ""), goodPacket.ToString().Replace("-", ""));
                }
                else
                {
                    ResultLogger.LogTestStatus(true, "PACKET_TEST " + name, "Success.");
                }
            }
            return true;
        }

        public static bool RunPacketTest()
        {
            if (!GENERATE)
            {
                JObject jobj = JsonConvert.DeserializeObject(Resources.PacketTestDataSet) as JObject;
                knownGoodPackets = jobj.ToObject<Dictionary<string, string>>();
            }

            List<bool> results = new List<bool>();

            results.Add(Test("2PlayerClose", PacketBuilder.Create2PlayerClose()));
            
            // Test Map
            results.Add(Test("BirdMap_OutsideMapTop", PacketBuilder.CreateBirdMap(-100, -800)));
            results.Add(Test("BirdMap_OutsideMapBottom", PacketBuilder.CreateBirdMap(Map.Height+100, Map.Width+600)));
            results.Add(Test("BirdMap_InsideMap", PacketBuilder.CreateBirdMap(100, 200)));

            // Test Brickpoet
            results.Add(Test("BrickPoetList", PacketBuilder.CreateBrickPoetList(Brickpoet.GetPoetryRoom(1))));
            results.Add(Test("BrickPoetMove", PacketBuilder.CreateBrickPoetMove(Brickpoet.GetPoetryPeice(Brickpoet.GetPoetryRoom(1), 30))));

            // Test Chat
            results.Add(Test("ChatBottomLeft", PacketBuilder.CreateChat("Trans Rights", PacketBuilder.CHAT_BOTTOM_LEFT)));
            results.Add(Test("ChatBottomRight", PacketBuilder.CreateChat("Are Human", PacketBuilder.CHAT_BOTTOM_RIGHT)));
            results.Add(Test("ChatDmRight", PacketBuilder.CreateChat("Rights", PacketBuilder.CHAT_DM_RIGHT)));
            results.Add(Test("ChatMotd", PacketBuilder.CreateMotd("Enbies are valid!")));

            // Test Drawing Room 
            results.Add(Test("DrawingRoomUpdate", PacketBuilder.CreateDrawingUpdate("C959|64^67|66^73|68^79|69^87")));

            // Test Dressup Room
            results.Add(Test("DressupRoomPeiceLoad", PacketBuilder.CreateDressupRoomPeiceLoad(Dressup.GetDressupRoom(2).DressupPeices)));
            results.Add(Test("DressupRoomPeiceMove", PacketBuilder.CreateDressupRoomPeiceMove(11, 14.4, 2.4, true)));

            // Test SwfModule Forwarding
            results.Add(Test("ForwardedSwfModule_516152", PacketBuilder.CreateForwardedSwfModule(new byte[] { 0x51, 0x61, 0x52 })));
            results.Add(Test("ForwardedSwfModule_AF8D91C8", PacketBuilder.CreateForwardedSwfModule(new byte[] { 0xAF, 0x8D, 0x91, 0xC8 })));

            // Test KeepAlive
            results.Add(Test("KeepAlive", PacketBuilder.CreateKeepAlive()));

            // Test KickMessage
            results.Add(Test("KickMessage", PacketBuilder.CreateKickMessage("Transphobia")));

            // Test Login
            results.Add(Test("LoginSuccess", PacketBuilder.CreateLogin(true)));
            results.Add(Test("LoginFail", PacketBuilder.CreateLogin(false)));
            results.Add(Test("LoginFail_ReasonBanned", PacketBuilder.CreateLogin(false, "You are banned.")));
            
            // Test Meta
            results.Add(Test("CreateMeta", PacketBuilder.CreateMeta("^R1Trans Rights^X^Z")));

            // Test Money, PlayerCount, Mail 
            results.Add(Test("MoneyPlayerCountAndMail", PacketBuilder.CreateMoneyPlayerCountAndMail(100, 1, 40)));

            // Test Movement
            results.Add(Test("Movement_DirectionUpMoveUpInsideMapWalk", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionUpMoveDownInsideMapWalk", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionUpMoveLeftInsideMapWalk", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionUpMoveRightInsideMapWalk", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionUpMoveUpdateInsideMapWalk", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionUpMoveEscapeInsideMapWalk", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionDownMoveUpInsideMapWalk", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionDownMoveDownInsideMapWalk", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionDownMoveLeftInsideMapWalk", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionDownMoveRightInsideMapWalk", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionDownMoveUpdateInsideMapWalk", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionDownMoveEscapeInsideMapWalk", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionLeftMoveUpInsideMapWalk", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionLeftMoveDownInsideMapWalk", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionLeftMoveLeftInsideMapWalk", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionLeftMoveRightInsideMapWalk", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionLeftMoveUpdateInsideMapWalk", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionLeftMoveEscapeInsideMapWalk", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionRightMoveUpInsideMapWalk", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionRightMoveDownInsideMapWalk", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionRightMoveLeftInsideMapWalk", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionRightMoveRightInsideMapWalk", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionRightMoveUpdateInsideMapWalk", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionRightMoveEscapeInsideMapWalk", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionTeleportMoveUpInsideMapWalk", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionTeleportMoveDownInsideMapWalk", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionTeleportMoveLeftInsideMapWalk", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionTeleportMoveRightInsideMapWalk", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionTeleportMoveUpdateInsideMapWalk", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionTeleportMoveEscapeInsideMapWalk", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionNoneMoveUpInsideMapWalk", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionNoneMoveDownInsideMapWalk", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionNoneMoveLeftInsideMapWalk", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionNoneMoveRightInsideMapWalk", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionNoneMoveUpdateInsideMapWalk", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionNoneMoveEscapeInsideMapWalk", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_ESCAPE, true)));

            // walk = false

            results.Add(Test("Movement_DirectionUpMoveUpInsideMap", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionUpMoveDownInsideMap", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionUpMoveLeftInsideMap", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionUpMoveRightInsideMap", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionUpMoveUpdateInsideMap", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionUpMoveEscapeInsideMap", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionDownMoveUpInsideMap", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionDownMoveDownInsideMap", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionDownMoveLeftInsideMap", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionDownMoveRightInsideMap", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionDownMoveUpdateInsideMap", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionDownMoveEscapeInsideMap", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionLeftMoveUpInsideMap", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionLeftMoveDownInsideMap", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionLeftMoveLeftInsideMap", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionLeftMoveRightInsideMap", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionLeftMoveUpdateInsideMap", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionLeftMoveEscapeInsideMap", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionRightMoveUpInsideMap", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionRightMoveDownInsideMap", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionRightMoveLeftInsideMap", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionRightMoveRightInsideMap", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionRightMoveUpdateInsideMap", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionRightMoveEscapeInsideMap", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionTeleportMoveUpInsideMap", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionTeleportMoveDownInsideMap", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionTeleportMoveLeftInsideMap", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionTeleportMoveRightInsideMap", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionTeleportMoveUpdateInsideMap", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionTeleportMoveEscapeInsideMap", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionNoneMoveUpInsideMap", PacketBuilder.CreateMovement(100, 131, 10, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionNoneMoveDownInsideMap", PacketBuilder.CreateMovement(123, 170, 12, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionNoneMoveLeftInsideMap", PacketBuilder.CreateMovement(210, 156, 14, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionNoneMoveRightInsideMap", PacketBuilder.CreateMovement(51, 53, 16, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionNoneMoveUpdateInsideMap", PacketBuilder.CreateMovement(90, 150, 1, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionNoneMoveEscapeInsideMap", PacketBuilder.CreateMovement(87, 43, 7, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_ESCAPE, false)));

            /*
             *  Test Outside Map (From Top)
             */

            results.Add(Test("Movement_DirectionUpMoveUpOutsideMapTopWalk", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionUpMoveDownOutsideMapTopWalk", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionUpMoveLeftOutsideMapTopWalk", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionUpMoveRightOutsideMapTopWalk", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionUpMoveUpdateOutsideMapTopWalk", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionUpMoveEscapeOutsideMapTopWalk", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionDownMoveUpOutsideMapTopWalk", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionDownMoveDownOutsideMapTopWalk", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionDownMoveLeftOutsideMapTopWalk", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionDownMoveRightOutsideMapTopWalk", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionDownMoveUpdateOutsideMapTopWalk", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionDownMoveEscapeOutsideMapTopWalk", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionLeftMoveUpOutsideMapTopWalk", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionLeftMoveDownOutsideMapTopWalk", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionLeftMoveLeftOutsideMapTopWalk", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionLeftMoveRightOutsideMapTopWalk", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionLeftMoveUpdateOutsideMapTopWalk", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionLeftMoveEscapeOutsideMapTopWalk", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionRightMoveUpOutsideMapTopWalk", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionRightMoveDownOutsideMapTopWalk", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionRightMoveLeftOutsideMapTopWalk", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionRightMoveRightOutsideMapTopWalk", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionRightMoveUpdateOutsideMapTopWalk", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionRightMoveEscapeOutsideMapTopWalk", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionTeleportMoveUpOutsideMapTopWalk", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionTeleportMoveDownOutsideMapTopWalk", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionTeleportMoveLeftOutsideMapTopWalk", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionTeleportMoveRightOutsideMapTopWalk", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionTeleportMoveUpdateOutsideMapTopWalk", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionTeleportMoveEscapeOutsideMapTopWalk", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionNoneMoveUpOutsideMapTopWalk", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionNoneMoveDownOutsideMapTopWalk", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionNoneMoveLeftOutsideMapTopWalk", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionNoneMoveRightOutsideMapTopWalk", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionNoneMoveUpdateOutsideMapTopWalk", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionNoneMoveEscapeOutsideMapTopWalk", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_ESCAPE, true)));

            // walk = false

            results.Add(Test("Movement_DirectionUpMoveUpOutsideMapTop", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionUpMoveDownOutsideMapTop", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionUpMoveLeftOutsideMapTop", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionUpMoveRightOutsideMapTop", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionUpMoveUpdateOutsideMapTop", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionUpMoveEscapeOutsideMapTop", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionDownMoveUpOutsideMapTop", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionDownMoveDownOutsideMapTop", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionDownMoveLeftOutsideMapTop", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionDownMoveRightOutsideMapTop", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionDownMoveUpdateOutsideMapTop", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionDownMoveEscapeOutsideMapTop", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionLeftMoveUpOutsideMapTop", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionLeftMoveDownOutsideMapTop", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionLeftMoveLeftOutsideMapTop", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionLeftMoveRightOutsideMapTop", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionLeftMoveUpdateOutsideMapTop", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionLeftMoveEscapeOutsideMapTop", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionRightMoveUpOutsideMapTop", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionRightMoveDownOutsideMapTop", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionRightMoveLeftOutsideMapTop", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionRightMoveRightOutsideMapTop", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionRightMoveUpdateOutsideMapTop", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionRightMoveEscapeOutsideMapTop", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionTeleportMoveUpOutsideMapTop", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionTeleportMoveDownOutsideMapTop", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionTeleportMoveLeftOutsideMapTop", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionTeleportMoveRightOutsideMapTop", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionTeleportMoveUpdateOutsideMapTop", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionTeleportMoveEscapeOutsideMapTop", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionNoneMoveUpOutsideMapTop", PacketBuilder.CreateMovement(-100, -131, 10, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionNoneMoveDownOutsideMapTop", PacketBuilder.CreateMovement(-123, -170, 12, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionNoneMoveLeftOutsideMapTop", PacketBuilder.CreateMovement(-210, -156, 14, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionNoneMoveRightOutsideMapTop", PacketBuilder.CreateMovement(-51, -53, 16, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionNoneMoveUpdateOutsideMapTop", PacketBuilder.CreateMovement(-90, -150, 1, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionNoneMoveEscapeOutsideMapTop", PacketBuilder.CreateMovement(-87, -43, 7, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_ESCAPE, false)));

            /*
             *  Test Outside Map (From Bottom)
             */

            results.Add(Test("Movement_DirectionUpMoveUpOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionUpMoveDownOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionUpMoveLeftOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionUpMoveRightOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionUpMoveUpdateOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionUpMoveEscapeOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionDownMoveUpOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionDownMoveDownOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionDownMoveLeftOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionDownMoveRightOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionDownMoveUpdateOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionDownMoveEscapeOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionLeftMoveUpOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionLeftMoveDownOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionLeftMoveLeftOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionLeftMoveRightOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionLeftMoveUpdateOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionLeftMoveEscapeOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionRightMoveUpOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionRightMoveDownOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionRightMoveLeftOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionRightMoveRightOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionRightMoveUpdateOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionRightMoveEscapeOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionTeleportMoveUpOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionTeleportMoveDownOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionTeleportMoveLeftOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionTeleportMoveRightOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionTeleportMoveUpdateOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionTeleportMoveEscapeOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_ESCAPE, true)));

            results.Add(Test("Movement_DirectionNoneMoveUpOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UP, true)));
            results.Add(Test("Movement_DirectionNoneMoveDownOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_DOWN, true)));
            results.Add(Test("Movement_DirectionNoneMoveLeftOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_LEFT, true)));
            results.Add(Test("Movement_DirectionNoneMoveRightOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_RIGHT, true)));
            results.Add(Test("Movement_DirectionNoneMoveUpdateOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UPDATE, true)));
            results.Add(Test("Movement_DirectionNoneMoveEscapeOutsideMapBottomWalk", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_ESCAPE, true)));

            // walk = false

            results.Add(Test("Movement_DirectionUpMoveUpOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionUpMoveDownOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionUpMoveLeftOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionUpMoveRightOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionUpMoveUpdateOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionUpMoveEscapeOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_UP, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionDownMoveUpOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionDownMoveDownOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionDownMoveLeftOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionDownMoveRightOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionDownMoveUpdateOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionDownMoveEscapeOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_DOWN, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionLeftMoveUpOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionLeftMoveDownOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionLeftMoveLeftOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionLeftMoveRightOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionLeftMoveUpdateOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionLeftMoveEscapeOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_LEFT, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionRightMoveUpOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionRightMoveDownOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionRightMoveLeftOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionRightMoveRightOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionRightMoveUpdateOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionRightMoveEscapeOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_RIGHT, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionTeleportMoveUpOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionTeleportMoveDownOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionTeleportMoveLeftOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionTeleportMoveRightOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionTeleportMoveUpdateOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionTeleportMoveEscapeOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_TELEPORT, PacketBuilder.MOVE_ESCAPE, false)));

            results.Add(Test("Movement_DirectionNoneMoveUpOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+100, Map.Height+131, 10, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UP, false)));
            results.Add(Test("Movement_DirectionNoneMoveDownOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+123, Map.Height+170, 12, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_DOWN, false)));
            results.Add(Test("Movement_DirectionNoneMoveLeftOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+210, Map.Height+156, 14, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_LEFT, false)));
            results.Add(Test("Movement_DirectionNoneMoveRightOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+51, Map.Height+53, 16, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_RIGHT, false)));
            results.Add(Test("Movement_DirectionNoneMoveUpdateOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+90, Map.Height+150, 1, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_UPDATE, false)));
            results.Add(Test("Movement_DirectionNoneMoveEscapeOutsideMapBottom", PacketBuilder.CreateMovement(Map.Width+87, Map.Height+43, 7, PacketBuilder.DIRECTION_NONE, PacketBuilder.MOVE_ESCAPE, false)));

            // Test Place Data Packet
            results.Add(Test("PlaceData", PacketBuilder.CreatePlaceData(World.Isles.ToArray(), World.Towns.ToArray(), World.Areas.ToArray())));
            
            // Test Player Update
            results.Add(Test("PlayerInfoUpdateOrCreate", PacketBuilder.CreatePlayerInfoUpdateOrCreate(100, 200, PacketBuilder.DIRECTION_UP, 10, "Li")));
            results.Add(Test("PlayerLeave", PacketBuilder.CreatePlayerLeave("Li")));

            // Test PlaySound Packet
            results.Add(Test("PlaySound", PacketBuilder.CreatePlaySound("PM")));
            
            // Test Profile Page
            results.Add(Test("ProfilePage", PacketBuilder.CreateProfilePage("TRANS RIGHTS ARE HUMAN RIGHTS")));

            // Test SecCode
            results.Add(Test("SecCode_AdminMod", PacketBuilder.CreateSecCode(new byte[] { 0x31, 0x52, 0x22 }, 0x34, true, true)));
            results.Add(Test("SecCode_Admin", PacketBuilder.CreateSecCode(new byte[] { 0x54, 0x32, 0x24 }, 0x34, true, false)));
            results.Add(Test("SecCode_Mod", PacketBuilder.CreateSecCode(new byte[] { 0x22, 0x12, 0x12 }, 0x34, false, true)));
            results.Add(Test("SecCode_User", PacketBuilder.CreateSecCode(new byte[] { 0x12, 0x32, 0x14 }, 0x34, false, false)));

            // Test SwfModule
            results.Add(Test("SwfModule_Force", PacketBuilder.CreateSwfModule("test.swf", PacketBuilder.PACKET_SWF_MODULE_FORCE)));
            results.Add(Test("SwfModule_Gentle", PacketBuilder.CreateSwfModule("test.swf", PacketBuilder.PACKET_SWF_MODULE_GENTLE)));
            results.Add(Test("SwfModule_Cutscene", PacketBuilder.CreateSwfModule("test.swf", PacketBuilder.PACKET_SWF_MODULE_CUTSCENE)));

            // Test TileClickInfo
            results.Add(Test("TileClickInfo", PacketBuilder.CreateTileClickInfo("Trans Rights Are Human Rights")));

            // Test TileOverlayFlags
            results.Add(Test("TileOverlayFlags", PacketBuilder.CreateTileOverlayFlags(new int[] { 1, 2, 3, 1, 2, 3, 1, 2, 3 })));

            // Test TimeAndWeatherUpdate
            results.Add(Test("TimeAndWeatherUpdate", PacketBuilder.CreateTimeAndWeatherUpdate(10, 4, 541, "SUNNY")));
            results.Add(Test("WeatherUpdate", PacketBuilder.CreateWeatherUpdate("CLOUD")));

            if (GENERATE)
            {
                string resultsStr = JsonConvert.SerializeObject(knownGoodPackets, Formatting.Indented);
                File.WriteAllText("test.json", resultsStr);
            }

            foreach(bool result in results)
            {
                if (result == false)
                    return false;
            }
            return true;
        }
    }
}
