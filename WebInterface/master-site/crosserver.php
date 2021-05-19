<?php

function  getNoPlayersOnlineInServer($database)
{
	include('dbconfig.php');
	$dbname = $database;
	
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$onlineUsers = mysqli_query($connect, "SELECT COUNT(1) FROM OnlineUsers");
	return $onlineUsers->fetch_row()[0];
}

function  getNoSubbedPlayersOnlineInServer($database)
{
	include('dbconfig.php');
	$dbname = $database;
	
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$onlineSubscribers = mysqli_query($connect, "SELECT COUNT(1) FROM OnlineUsers WHERE Subscribed = 'YES'");
	return $onlineSubscribers->fetch_row()[0];
}

function  getNoModPlayersOnlineInServer($database)
{
	include('dbconfig.php');
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
	include('dbconfig.php');
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
	include('dbconfig.php');
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