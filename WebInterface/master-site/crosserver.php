<?php

function GenHmacMessage(string $data, string $channel)
{
	include('config.php');
	if($hmac_secret === "!!NOTSET!!"){
		echo("<script>alert('Please set HMAC_SECRET !')</script>");
		echo("<h1>Set \$hmac_secret in config.php!</h1>");
		exit();
	}
	$hmac = hash_hmac('sha256', $data, $hmac_secret.$channel.$_SERVER['REMOTE_ADDR'].date('mdYhi'));
	return $hmac;
}

function  getNoPlayersOnlineInServer($database)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$onlineUsers = mysqli_query($connect, "SELECT COUNT(1) FROM OnlineUsers");
	return $onlineUsers->fetch_row()[0];
}

function  getNoSubbedPlayersOnlineInServer($database)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$onlineSubscribers = mysqli_query($connect, "SELECT COUNT(1) FROM OnlineUsers WHERE Subscribed = 'YES'");
	return $onlineSubscribers->fetch_row()[0];
}

function getUserMoney($database, $id)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT Money FROM UserExt WHERE Id=?");
	$stmt->bind_param("i", $id);
	$stmt->execute();
	$result = $stmt->get_result();
	
	return intval($result->fetch_row()[0]);
	
}

function setUserMoney($database, $id, $money)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("UPDATE UserExt SET Money=? WHERE Id=?");
	$stmt->bind_param("ii", $money, $id);
	$stmt->execute();
}

function setUserSubbed($database, $id, $subbed)
{
	$subedV = "";
	if($subbed)
		$subedV = "YES";
	else
		$subbedV = "NO";
	
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("UPDATE UserExt SET Subscriber=? WHERE Id=?");
	$stmt->bind_param("si", $subedV, $id);
	$stmt->execute();
}

function setUserSubbedUntil($database, $id, $subbedUntil)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("UPDATE UserExt SET SubscribedUntil=? WHERE Id=?");
	$stmt->bind_param("ii", $subbedUntil, $id);
	$stmt->execute();
}

function getUserBankMoney($database, $id)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT BankBalance FROM UserExt WHERE Id=?");
	$stmt->bind_param("i", $id);
	$stmt->execute();
	$result = $stmt->get_result();
	
	return intval($result->fetch_row()[0]);
	
}

function getUserLoginDate($database, $id)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT LastLogin FROM UserExt WHERE Id=?");
	$stmt->bind_param("i", $id);
	$stmt->execute();
	$result = $stmt->get_result();
	
	return intval($result->fetch_row()[0]);
	
}

function getUserQuestPoints($database, $id)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT QuestPoints FROM UserExt WHERE Id=?");
	$stmt->bind_param("i", $id);
	$stmt->execute();
	$result = $stmt->get_result();
	
	return intval($result->fetch_row()[0]);
	
}

function getUserExistInExt($database, $id)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT COUNT(*) FROM UserExt WHERE Id=?");
	$stmt->bind_param("i", $id);
	$stmt->execute();
	$result = $stmt->get_result();
	
	return intval($result->fetch_row()[0]) <= 0;
	
}

function getUserTotalLogins($database, $id)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT TotalLogins FROM UserExt WHERE Id=?");
	$stmt->bind_param("i", $id);
	$stmt->execute();
	$result = $stmt->get_result();
	
	return intval($result->fetch_row()[0]);
	
}

function getUserPlaytime($database, $id)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT FreeMinutes FROM UserExt WHERE Id=?");
	$stmt->bind_param("i", $id);
	$stmt->execute();
	$result = $stmt->get_result();
	
	return intval($result->fetch_row()[0]);
	
}


function getUserSubTimeRemaining($database, $id)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT SubscribedUntil FROM UserExt WHERE Id=?");
	$stmt->bind_param("i", $id);
	$stmt->execute();
	$result = $stmt->get_result();
	
	return intval($result->fetch_row()[0]);
	
}


function addItemToPuchaseQueue($database, $playerId, $itemId, $itemCount)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("INSERT INTO ItemPurchaseQueue VALUES(?,?,?)");
	$stmt->bind_param("iii", $playerId, $itemId, $itemCount);
	$stmt->execute();
	$result = $stmt->get_result();
	
}

function getUserSubbed($database, $id)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT Subscriber FROM UserExt WHERE Id=?");
	$stmt->bind_param("i", $id);
	$stmt->execute();
	$result = $stmt->get_result();
	
	return $result->fetch_row()[0] == "YES";
	
}

function isUserOnline($database, $id)
{
	include('config.php');
	
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT COUNT(1) FROM OnlineUsers WHERE playerId=?");
	$stmt->bind_param("i", $userid);
	$stmt->execute();
	$result = $stmt->get_result();
	$count = intval($result->fetch_row()[0]);
	return $count>0;	
}

function  getNoModPlayersOnlineInServer($database)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$onlineModerators = mysqli_query($connect, "SELECT COUNT(1) FROM OnlineUsers WHERE Moderator = 'YES' OR Admin='YES'");
	return $onlineModerators->fetch_row()[0];
}

function getServerById(string $id)
{
	include('servers.php');
	for($i = 0; $i < count($server_list); $i++)
	{
		if($server_list[$i]['id'] == $id)
			return $server_list[$i];
	}
	return null;
}


function userid_exists(string $database, string $userid)
{
	include('config.php');
	$dbname = $database;
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT COUNT(1) FROM Users WHERE Id=?");
	$stmt->bind_param("i", $userid);
	$stmt->execute();
	$result = $stmt->get_result();
	$count = intval($result->fetch_row()[0]);
	return $count>0;
}

function createAccountOnServer(string $database)
{
	include('config.php');
	$dbname = $database;

	$id = intval($_SESSION['PLAYER_ID']);
	$username = $_SESSION['USERNAME'];
	$sex = $_SESSION['SEX'];
	$admin = $_SESSION['ADMIN'];
	$mod = $_SESSION['MOD'];
	$passhash = $_SESSION['PASSWORD_HASH'];
	$salt = $_SESSION['SALT'];


	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("INSERT INTO Users VALUES(?,?,?,?,?,?,?)"); 
	$stmt->bind_param("issssss", $id, $username, $passhash, $salt, $sex, $admin, $mod);
	$stmt->execute();
}

# Global Functions
function getNoPlayersOnlineGlobal()
{
	include('servers.php');
	$playersOn = 0;
	for($i = 0; $i < count($server_list); $i++)
	{
		$playersOn += getNoPlayersOnlineInServer($server_list[$i]['database']);
	}
	return $playersOn;
}

function getNoSubbedPlayersOnlineGlobal()
{
	include('servers.php');
	$playersOn = 0;
	for($i = 0; $i < count($server_list); $i++)
	{
		$playersOn += getNoSubbedPlayersOnlineInServer($server_list[$i]['database']);
	}
	return $playersOn;
}

function getNoModPlayersOnlineGlobal()
{
	include('servers.php');
	$playersOn = 0;
	for($i = 0; $i < count($server_list); $i++)
	{
		$playersOn += getNoModPlayersOnlineInServer($server_list[$i]['database']);
	}
	return $playersOn;
}


?>
