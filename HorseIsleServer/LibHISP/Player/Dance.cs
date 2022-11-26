using HISP.Server;
using System;
using System.Threading;

namespace HISP.Player
{
    public class Dance : IDisposable
    {
        public const int DanceSpeed = 1000;
        private Timer danceTimer;
        public string Moves;
        public int MoveIndex;
        private User baseUser;

        public Dance(User BaseUser, string DanceMoves)
        {
            baseUser = BaseUser;
            baseUser.ActiveDance = this;
            Moves = DanceMoves.ToLower();
            MoveIndex = -1;
            danceTimer = new Timer(new TimerCallback(onDanceTick), null, DanceSpeed, DanceSpeed);
        }

        private void onDanceTick(object state)
        {
            
            MoveIndex++;
            if (MoveIndex >= Moves.Length)
                goto done;


            int onHorse = 0;
            int facing =  baseUser.Facing;
            while (facing >= 5)
            {
                facing -= 5;
                onHorse++;
            }

            char moveInDir = Moves[MoveIndex];

            int direction;

            switch(moveInDir)
            {
                case 'u':
                    direction = PacketBuilder.DIRECTION_UP;
                    break;
                case 'd':
                    direction = PacketBuilder.DIRECTION_DOWN;
                    break;
                case 'l':
                    direction = PacketBuilder.DIRECTION_LEFT;
                    break;
                case 'r':
                    direction = PacketBuilder.DIRECTION_RIGHT;
                    break;
                default:
                    goto done;
            }


            baseUser.Facing = direction + (onHorse * 5);

            byte[] moveResponse = PacketBuilder.CreateMovement(baseUser.X, baseUser.Y, baseUser.CharacterId, baseUser.Facing, PacketBuilder.DIRECTION_NONE, false);
            baseUser.Client.SendPacket(moveResponse);

            GameServer.UpdateUserFacingAndLocation(baseUser);

            done:;
            if (MoveIndex < Moves.Length)
            {
                danceTimer.Change(DanceSpeed, DanceSpeed);
            }
            else
            {
                this.Dispose();
            }
        }

        public void Dispose()
        {
            baseUser.ActiveDance = null;
            baseUser = null;
            Moves = null;
            danceTimer.Dispose();
            danceTimer = null;
            MoveIndex = -1;
        }
    }
}
