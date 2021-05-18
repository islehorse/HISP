<?php
include('config.php');

if($hmac_secret === "!!NOTSET!!") # Defaults bad.
{
	header("HTTP/1.1 403 Forbidden");
	echo("Please set HMAC_SECRET in CONFIG.PHP! for security reasons joining the server is refused.<br>Change it from the default and then try again!");
	exit();
}

if(isset($_POST["ID"], $_POST["USERNAME"], $_POST["USERNAME"], $_POST["PASSHASH"], $_POST["PASSSALT"], $_POST["SEX"], $_POST["MODERATOR"], $_POST["ADMIN"], $_POST["CODE"]))
{
	$id = $_POST["ID"];
	$username = $_POST["USERNAME"];
	$passhash = $_POST["PASSHASH"];
	$passsalt = $_POST["PASSSALT"];
	$sex = $_POST["SEX"];
	$moderator = $_POST["MODERATOR"];
	$admin = $_POST["ADMIN"];
	$code = $_POST["CODE"];
	
	#Verify Input
	$hmac = hash_hmac('sha256', (string)$id.$username.$passhash.$passsalt.$sex.$moderator.$admin, $hmac_secret."HOIL4321"));
	
	if (hash_equals($code, $hmac)) 
	{
		# Create Account.
		$stmt = $connect->prepare("INSERT INTO Users VALUES(?,?,?,?,?,?,?)"); 
		$stmt->bind_param("issssss", $id, $username, $passhash, $passsalt, $sex, $admin, $moderator);
		$stmt->execute();
		echo("OK");
	}
	else
	{
		header("HTTP/1.1 403 Forbidden");
		echo("Invalid HMAC! Please ensure that all game-site's have the same HMAC as the master-site!
	}
}	

?>