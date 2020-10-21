using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Isle_Server
{
    class Meta
    {
        // Meta

        private static string buildLocationString(int x, int y)
        {
            string locationString = "";

            if (World.InArea(x, y))
                locationString += Messages.AreaFormat.Replace("%AREA%", World.GetArea(x, y).Name);
            if (World.InTown(x, y))
                locationString += Messages.TownFormat.Replace("%TOWN%", World.GetTown(x, y).Name);
            if (World.InIsle(x, y))
                locationString += Messages.IsleFormat.Replace("%ISLE%", World.GetIsle(x, y).Name);
            if (locationString != "")
                locationString = Messages.LocationFormat.Replace("%META%", locationString);
            return locationString;
        }


        private static string buildNearbyString(int x, int y)
        {
            string playersNearby = "";

            User[] nearbyUsers = Server.GetNearbyUsers(x, y, true, true);
            if (nearbyUsers.Length > 1)
            {
                playersNearby += Messages.NearbyPlayers;
                playersNearby += Messages.Seperator;

                string usersWest = "";
                string usersNorth = "";
                string usersEast = "";
                string usersSouth = "";
                foreach (User nearbyUser in nearbyUsers)
                {
                    if (nearbyUser.X < x)
                    {
                        usersWest += " " + nearbyUser.Username + " ";
                    }
                    else if (nearbyUser.X > x)
                    {
                        usersEast += " " + nearbyUser.Username + " ";
                    }
                    else if (nearbyUser.Y > y)
                    {
                        usersSouth += " " + nearbyUser.Username + " ";
                    }
                    else if (nearbyUser.Y < y)
                    {
                        usersNorth += " " + nearbyUser.Username + " ";
                    }
                }

                if (usersEast != "")
                    playersNearby += " " + Messages.East + usersEast + Messages.Seperator;
                if (usersWest != "")
                    playersNearby += " " + Messages.West + usersWest + Messages.Seperator;
                if (usersSouth != "")
                    playersNearby += " " + Messages.South + usersSouth + Messages.Seperator;
                if (usersNorth != "")
                    playersNearby += " " + Messages.North + usersNorth + Messages.Seperator;



            }

            return playersNearby;

        }
        private static string buildCommonInfo(int x, int y)
        {
            string message = "";

            message += Messages.Seperator + buildNearbyString(x, y);

            // Dropped Items
            DroppedItems.DroppedItem[] Items = DroppedItems.GetItemsAt(x, y);
            if (Items.Length == 0)
                message += Messages.NothingMessage;
            else
            {
                message += Messages.ItemsOnGroundMessage;
                foreach(DroppedItems.DroppedItem item in Items)
                {
                    Item.ItemInformation itemInfo = item.instance.GetItemInfo();
                    message += Messages.FormatGrabItemMessage(itemInfo.Name, item.instance.RandomID, itemInfo.IconId);
                }
                message += Messages.GrabAllItemsMessage;
            }
            Logger.DebugPrint(message);
            return message;
        }
        public static string BuildTransportInfo(Transport.TransportPoint transportPoint)
        {
            string message = "";
            // Build list of locations
            foreach(int transportLocationId in transportPoint.Locations)
            {
                Transport.TransportLocation transportLocation = Transport.GetTransportLocation(transportLocationId);
                message += Messages.FormatTransportMessage(transportLocation.Type, transportLocation.LocationTitle, transportLocation.Cost, transportLocation.Id, transportLocation.GotoX, transportLocation.GotoY);
            }
            return message;
        }
        public static string BuildSpecialTileInfo(World.SpecialTile specialTile)
        {
            string message = "";

            if (specialTile.Title != null)
                message += Messages.TileFormat.Replace("%TILENAME%", specialTile.Title);
            else
                message += buildLocationString(specialTile.X, specialTile.Y);

            if (specialTile.Description != null)
                message += Messages.Seperator + specialTile.Description;

            if (specialTile.Code == null)
                message += buildCommonInfo(specialTile.X, specialTile.Y);
            if (specialTile.Code == "TRANSPORT")
            {
                Transport.TransportPoint point = Transport.GetTransportPoint(specialTile.X, specialTile.Y);
                message +=  Meta.BuildTransportInfo(point)+ "^R1";
            }
            
            if (specialTile.ExitX != 0 && specialTile.ExitY != 0)
                message += Messages.ExitThisPlace + Messages.MetaTerminator;

            return message;
        }

        public static string BuildInventoryInfo(IInventory inv)
        {
            string message = "";
            message += Messages.FormatPlayerInventoryHeaderMeta(inv.Count, Messages.DefaultInventoryMax);
            return message;
        }
        public static string BuildMetaInfo(int x, int y)
        {
            string message = "";
            message += buildLocationString(x, y);

            message += buildCommonInfo(x, y);
            return message;
        }

    }
}
