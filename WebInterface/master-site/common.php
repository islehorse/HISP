<?php

function hash_salt(string $input, string $salt)
{
	$output = hash('sha512',$input,true);
	$len=strlen(bin2hex($output))/2;
	$xor_hash = "";
	for($i = 0; $i < $len; $i++)
	{
		$xor_hash .= $output[$i] ^ $salt[$i];
	}
	
	return hash('sha512',$xor_hash,false);
}

function is_logged_in()
{
	if(session_status() !== PHP_SESSION_ACTIVE)
		return false;
	
	if(isset($_SESSION["LOGGED_IN"]))
		if($_SESSION["LOGGED_IN"] === "YES")
			return true;
	return false;
}

function user_exists(string $username)
{
	include('dbconfig.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT COUNT(1) FROM Users WHERE UPPER(Username)=?"); 
	$stmt->bind_param("s", strtoupper($username));
	$stmt->execute();
	$result = $stmt->get_result();
	$count = intval($result->fetch_row()[0]);
	return $count>0;
}

function get_userid(string $username)
{
	include('dbconfig.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	
	$stmt = $connect->prepare("SELECT Id FROM Users WHERE UPPER(Username)=?"); 
	$stmt->bind_param("s", strtoupper($username));
	$stmt->execute();
	$result = $stmt->get_result();
	$id = intval($result->fetch_row()[0]);
	return $id;
}

function get_sex(int $userid)
{
	include('dbconfig.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	
	$stmt = $connect->prepare("SELECT Gender FROM Users WHERE Id=?"); 
	$stmt->bind_param("i", $userid);
	$stmt->execute();
	$result = $stmt->get_result();
	return $result->fetch_row()[0];

}

function get_admin(int $userid)
{
	include('dbconfig.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	
	$stmt = $connect->prepare("SELECT Admin FROM Users WHERE Id=?"); 
	$stmt->bind_param("i", $userid);
	$stmt->execute();
	$result = $stmt->get_result();
	return $result->fetch_row()[0];

}

function get_mod(int $userid)
{
	include('dbconfig.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	
	$stmt = $connect->prepare("SELECT Moderator FROM Users WHERE Id=?"); 
	$stmt->bind_param("i", $userid);
	$stmt->execute();
	$result = $stmt->get_result();
	return $result->fetch_row()[0];

}

function get_password_hash(int $userid)
{
	include('dbconfig.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT PassHash FROM Users WHERE Id=?"); 
	$stmt->bind_param("i", $userid);
	$stmt->execute();
	$result = $stmt->get_result();
	return $result->fetch_row()[0];
	
}

function get_salt(int $userid)
{
	include('dbconfig.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");	
	$stmt = $connect->prepare("SELECT Salt FROM Users WHERE Id=?"); 
	$stmt->bind_param("i", $userid);
	$stmt->execute();
	$result = $stmt->get_result();
	return $result->fetch_row()[0];
}

function check_password(int $userId, string $password)
{
	$passhash = get_password_hash($userId);
	$passsalt = hex2bin(get_salt($userId));
	$acturalhash = hash_salt($password, $passsalt);
	
	if($acturalhash === $passhash)
		return true;
	else
		return false;
}

function populate_db()
{
	include('dbconfig.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	mysqli_query($connect, "CREATE TABLE IF NOT EXISTS Users(Id INT, Username TEXT(16),Email TEXT(128),Country TEXT(128),SecurityQuestion Text(128),SecurityAnswerHash TEXT(128),Age INT,PassHash TEXT(128), Salt TEXT(128),Gender TEXT(16), Admin TEXT(3), Moderator TEXT(3))");

}

function startsWith( $haystack, $needle ) {
     $length = strlen( $needle );
     return substr( $haystack, 0, $length ) === $needle;
}

function endsWith( $haystack, $needle ) {
    $length = strlen( $needle );
    if( !$length ) {
        return true;
    }
    return substr( $haystack, -$length ) === $needle;
}


?>