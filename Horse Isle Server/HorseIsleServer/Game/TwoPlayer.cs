using HISP.Player;
using HISP.Security;
using HISP.Server;
using System.Collections.Generic;
using System.Threading;

namespace HISP.Game
{
    public class TwoPlayer
    {
        public static List<TwoPlayer> TwoPlayerGames = new List<TwoPlayer>();
        public static void TwoPlayerRemove(User user)
        {
            foreach(TwoPlayer twoPlayerGame in TwoPlayerGames.ToArray())
            {
                if((twoPlayerGame.Invitee.Id == user.Id))
                {
                    twoPlayerGame.CloseGame(user, true);
                }
            }
        }
        public static bool IsPlayerInvitingPlayer(User sender, User checkInvites)
        {
            foreach (TwoPlayer twoPlayerGame in TwoPlayerGames.ToArray())
            {
                if ((twoPlayerGame.Invitee.Id == sender.Id && twoPlayerGame.Inviting.Id == checkInvites.Id) && !twoPlayerGame.Accepted)
                {
                    return true;
                }
            }
            return false;
        }
        public static TwoPlayer GetGameInvitingPlayer(User sender, User checkInvites)
        {
            foreach (TwoPlayer twoPlayerGame in TwoPlayerGames.ToArray())
            {
                if ((twoPlayerGame.Invitee.Id == sender.Id && twoPlayerGame.Inviting.Id == checkInvites.Id) && !twoPlayerGame.Accepted)
                {
                    return twoPlayerGame;
                }
            }
            throw new KeyNotFoundException("not found.");
        }

        public static bool IsPlayerInGame(User user)
        {
            foreach (TwoPlayer twoPlayerGame in TwoPlayerGames.ToArray())
            {
                if ((twoPlayerGame.Invitee.Id == user.Id || twoPlayerGame.Inviting.Id == user.Id) && twoPlayerGame.Accepted)
                {
                    return true;
                }
            }
            return false;
        }

        public static TwoPlayer GetTwoPlayerGameInProgress(User user)
        {
            foreach (TwoPlayer twoPlayerGame in TwoPlayerGames.ToArray())
            {
                if ((twoPlayerGame.Invitee.Id == user.Id || twoPlayerGame.Inviting.Id == user.Id) && twoPlayerGame.Accepted)
                {
                    return twoPlayerGame;
                }
            }
            throw new KeyNotFoundException("No game found");
        }

        public TwoPlayer(User inviting, User invitee, bool accepted, int randomId=-1)
        {
            RandomId = RandomID.NextRandomId(randomId);
            Inviting = inviting;
            Invitee = invitee;
            Accepted = accepted;

            PosX = Invitee.X;
            PosY = Invitee.Y;

            byte[] youHaveInvited = PacketBuilder.CreateChat(Messages.Format2PlayerYouInvited(inviting.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
            byte[] yourInvited = PacketBuilder.CreateChat(Messages.Format2PlayerYourInvited(invitee.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);

            Invitee.LoggedinClient.SendPacket(youHaveInvited);
            Inviting.LoggedinClient.SendPacket(yourInvited);

            deleteTimer = new Timer(new TimerCallback(deleteTwoPlayer), null, 2 * 60 * 1000, 2 * 60 * 1000);

            TwoPlayerGames.Add(this);

            update();

            if (Multiroom.IsMultiRoomAt(PosX, PosY))
            {
                this.Multiroom = Multiroom.GetMultiroom(PosX, PosY);
            }
        }

        private void deleteTwoPlayer(object state)
        {
            if (!Accepted)
            {
                Accepted = false; 

                Invitee.MetaPriority = false;
                Inviting.MetaPriority = false;

                update();

                TwoPlayerGames.Remove(this);
            }
            deleteTimer.Dispose();
        }

        private void update()
        {
            GameServer.UpdateArea(Invitee.LoggedinClient);
            GameServer.UpdateArea(Inviting.LoggedinClient);
        }
        private void updateOthers()
        {
            foreach(User user in this.Multiroom.JoinedUsers.ToArray())
                if (IsPlayerInGame(user))
                    if(user.Id != Invitee.Id && user.Id != Inviting.Id)
                        GameServer.UpdateArea(user.LoggedinClient);
            
        }
        public void UpdateAll()
        {
            update();
            updateOthers();
        }
        private string buildSwf(int playerNumb)
        {
            if(World.InSpecialTile(PosX, PosY))
            {
                World.SpecialTile tile = World.GetSpecialTile(PosX, PosY);
                if(tile.Code != null)
                {
                    if(tile.Code.StartsWith("2PLAYER-"))
                    {
                        string swf = tile.Code.Split('-')[1].ToLower();
                        if (!swf.Contains(".swf"))
                            swf += ".swf";
                        if (swf.Contains("?"))
                            swf += "&";
                        else
                            swf += "?";

                        swf += "playernum=" + playerNumb.ToString();
                        return swf;
                    }
                }
            }
            return "test";
        }

        public int RandomId = 0;
        public User Inviting = null;
        public User Invitee = null;
        public bool Accepted = false;
        public Multiroom Multiroom;
        public int PosX;
        public int PosY;

        private Timer deleteTimer;
        public void Accept(User user)
        {
            if(user.Id == Inviting.Id)
            {
                Accepted = true;
                deleteTimer.Dispose();

                UpdateAll();


                byte[] startingUpGameInvitee = PacketBuilder.CreateChat(Messages.Format2PlayerStartingGame(Inviting.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);
                byte[] startingUpGameInvited = PacketBuilder.CreateChat(Messages.Format2PlayerStartingGame(Invitee.Username), PacketBuilder.CHAT_BOTTOM_RIGHT);

                Invitee.LoggedinClient.SendPacket(startingUpGameInvitee);
                Inviting.LoggedinClient.SendPacket(startingUpGameInvited);

                byte[] loadSwfInvitee = PacketBuilder.CreateSwfModulePacket(buildSwf(2), PacketBuilder.PACKET_SWF_MODULE_FORCE);
                byte[] loadSwfInvited = PacketBuilder.CreateSwfModulePacket(buildSwf(1), PacketBuilder.PACKET_SWF_MODULE_FORCE);

                Invitee.LoggedinClient.SendPacket(loadSwfInvitee);
                Inviting.LoggedinClient.SendPacket(loadSwfInvited);

            }
        }

        public void CloseGame(User userWhoClosed, bool closeSilently=false)
        {
            if(userWhoClosed.Id == Inviting.Id || userWhoClosed.Id == Invitee.Id && Accepted)
            {
                Accepted = false;

                if(!closeSilently)
                {
                    byte[] gameClosedByOther = PacketBuilder.CreateChat(Messages.TwoPlayerGameClosedOther, PacketBuilder.CHAT_BOTTOM_RIGHT);
                    byte[] gameClosed = PacketBuilder.CreateChat(Messages.TwoPlayerGameClosed, PacketBuilder.CHAT_BOTTOM_RIGHT);

                    if (userWhoClosed.Id == Inviting.Id)
                    {
                        Invitee.LoggedinClient.SendPacket(gameClosedByOther);
                        Inviting.LoggedinClient.SendPacket(gameClosed);
                    }
                    else if (userWhoClosed.Id == Invitee.Id)
                    {
                        Invitee.LoggedinClient.SendPacket(gameClosed);
                        Inviting.LoggedinClient.SendPacket(gameClosedByOther);
                    }
                }

                Invitee.MetaPriority = false;
                Inviting.MetaPriority = false;

                
                if (!closeSilently)
                    UpdateAll();
                else
                    updateOthers();
                
                TwoPlayerGames.Remove(this);
            }
        }


    }
}
