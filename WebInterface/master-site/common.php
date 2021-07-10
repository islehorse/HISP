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

function base64_url_encode($input) {
 return strtr(base64_encode($input), '+/=', '._-');
}

function base64_url_decode($input) {
 return base64_decode(strtr($input, '._-', '+/='));
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
	include('config.php');
	$usernameUppercase = strtoupper($username);
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT COUNT(1) FROM Users WHERE UPPER(Username)=?"); 
	$stmt->bind_param("s", $usernameUppercase);
	$stmt->execute();
	$result = $stmt->get_result();
	$count = intval($result->fetch_row()[0]);
	return $count>0;
}

function get_username(string $id)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT Username FROM Users WHERE Id=?"); 
	$stmt->bind_param("i", $id);
	$stmt->execute();
	$result = $stmt->get_result();
	$usetname = $result->fetch_row()[0];
	return $usetname;
}


function count_topics(string $fourm)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT COUNT(*) FROM FourmThread WHERE Fourm=?"); 
	$stmt->bind_param("s", $fourm);
	$stmt->execute();
	$result = $stmt->get_result();
	$count = intval($result->fetch_row()[0]);
	return $count;
}

function count_replies(int $thread)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT COUNT(*) FROM FourmReply WHERE ThreadId=?"); 
	$stmt->bind_param("i", $thread);
	$stmt->execute();
	$result = $stmt->get_result();
	$count = intval($result->fetch_row()[0]);
	return $count;
}

function get_last_reply_author(string $thread)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT * FROM FourmReply WHERE ThreadId=? ORDER BY CreationTime DESC LIMIT 1"); 
	$stmt->bind_param("i", $thread);
	$stmt->execute();
	$result = $stmt->get_result();
	$author = $result->fetch_row()[2];
	return $author;
}

function get_last_reply_time(string $thread)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT * FROM FourmReply WHERE ThreadId=? ORDER BY CreationTime DESC LIMIT 1"); 
	$stmt->bind_param("i", $thread);
	$stmt->execute();
	$result = $stmt->get_result();
	$author = $result->fetch_row()[5];
	return $author;
}

function get_first_reply_author(string $thread)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT * FROM FourmReply WHERE ThreadId=? ORDER BY CreationTime ASC LIMIT 1"); 
	$stmt->bind_param("i", $thread);
	$stmt->execute();
	$result = $stmt->get_result();
	$author = $result->fetch_row()[2];
	return $author;
}

function get_first_reply_time(string $thread)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT * FROM FourmReply WHERE ThreadId=? ORDER BY CreationTime ASC LIMIT 1"); 
	$stmt->bind_param("i", $thread);
	$stmt->execute();
	$result = $stmt->get_result();
	$author = $result->fetch_row()[5];
	return $author;
}

function create_fourm_thread(string $title, string $fourm)
{
	include('config.php');
	
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$result = mysqli_query($connect, "SELECT MAX(ThreadId) FROM FourmThread");
	
	$thread_id = $result->fetch_row()[0] + 1;
	if($thread_id == NULL)
		$thread_id = 0;
	$curTime = time();

	$stmt = $connect->prepare("INSERT INTO FourmThread VALUES(?,?,?,?,'NO')"); 
	$stmt->bind_param("issi", $thread_id, $title, $fourm, $curTime);
	$stmt->execute();
	
	return $thread_id;
}


function set_thread_update(int $thread_id)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("UPDATE FourmThread SET UpdateTime=? WHERE ThreadId=?");
	$stmt->bind_param("ii", time(), $thread_id);
	$stmt->execute();
}

function create_fourm_reply(int $thread_id, string $username, string $contents, string $fourm, bool $madeByAdmin)
{
	include('config.php');
	
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$result = mysqli_query($connect, "SELECT MAX(ReplyId) FROM FourmReply");
	
	$reply_id = $result->fetch_row()[0] + 1;
	if($reply_id == NULL)
		$reply_id = 0;
	$curTime = time();

	if($madeByAdmin)
		$admin = "YES";
	else
		$admin = "NO";

	$stmt = $connect->prepare("INSERT INTO FourmReply VALUES(?,?,?,?,?,?,?)"); 
	$stmt->bind_param("iisssis", $reply_id, $thread_id, $username, $contents, $fourm, $curTime, $admin);
	$stmt->execute();
	
	set_thread_update($thread_id);
	
	return $reply_id;
}


function get_fourm_thread($threadId)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT * FROM FourmThread WHERE ThreadId=?"); 
	$stmt->bind_param("i", $threadId);
	$stmt->execute();
	$result = $stmt->get_result();
	$row = $result->fetch_row();
	return ['id' => $row[0], 'title' => $row[1], 'fourm' => $row[2], 'creation_time' => $row[3], 'locked' => ($row[4] === "YES")];;
}

function get_fourm_replies($threadId)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT * FROM FourmReply WHERE ThreadId=?"); 
	$stmt->bind_param("i", $threadId);
	$stmt->execute();
	$result = $stmt->get_result();
	$replies = [];
		

	while ($row = $result->fetch_row()) {
		$arr = [ ['reply_id' => $row[0], 'thread_id' => $row[1], 'author' => $row[2], 'contents' => $row[3], 'fourm' => $row[4], 'creation_time' => $row[5], 'admin' => ($row[6] === "YES")] ];
		$replies = array_merge($replies, $arr);
	}
	
	return $replies;
}


function get_fourm_threads($fourm)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT * FROM FourmThread WHERE Fourm=? ORDER BY UpdateTime DESC"); 
	$stmt->bind_param("s", $fourm);
	$stmt->execute();
	$result = $stmt->get_result();
	$threads = [];
		

	while ($row = $result->fetch_row()) {
		$arr = [ ['id' => $row[0], 'title' => $row[1], 'fourm' => $row[2], 'update_time' => $row[3], 'locked' => ($row[4] === "YES")] ];
		$threads = array_merge($threads, $arr);
	}
	
	return $threads;
}

function get_userid(string $username)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$usernameUppercase = strtoupper($username);
	$stmt = $connect->prepare("SELECT Id FROM Users WHERE UPPER(Username)=?"); 
	$stmt->bind_param("s", $usernameUppercase);
	$stmt->execute();
	$result = $stmt->get_result();
	$id = intval($result->fetch_row()[0]);
	return $id;
}

function get_sex(int $userid)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	
	$stmt = $connect->prepare("SELECT Gender FROM Users WHERE Id=?"); 
	$stmt->bind_param("i", $userid);
	$stmt->execute();
	$result = $stmt->get_result();
	return $result->fetch_row()[0];

}

function get_admin(int $userid)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	
	$stmt = $connect->prepare("SELECT Admin FROM Users WHERE Id=?"); 
	$stmt->bind_param("i", $userid);
	$stmt->execute();
	$result = $stmt->get_result();
	return $result->fetch_row()[0] === "YES";

}

function get_mod(int $userid)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	
	$stmt = $connect->prepare("SELECT Moderator FROM Users WHERE Id=?"); 
	$stmt->bind_param("i", $userid);
	$stmt->execute();
	$result = $stmt->get_result();
	return $result->fetch_row()[0] === "YES";

}

function get_password_hash(int $userid)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT PassHash FROM Users WHERE Id=?"); 
	$stmt->bind_param("i", $userid);
	$stmt->execute();
	$result = $stmt->get_result();
	return $result->fetch_row()[0];
	
}

function get_salt(int $userid)
{
	include('config.php');
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

function count_LastOn(int $userId)
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT COUNT(*) FROM LastOn WHERE Id=?");
	$stmt->bind_param("i", $userId);
	$stmt->execute();
	$result = $stmt->get_result();
	$v = $result->fetch_row();	
	return intval($v[0]);
}


function get_LastOn(int $userId)
{
	if(count_LastOn($userId) <= 0){
		return "NONE";
	}


	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$stmt = $connect->prepare("SELECT ServerId FROM LastOn WHERE Id=?");
	$stmt->bind_param("i", $userId);
	$stmt->execute();
	$result = $stmt->get_result();
	$v = $result->fetch_row();
	
    
	return $v[0];
}

function set_LastOn(int $userId, string $lastOn)
{
	include('config.php');
	
	if(get_LastOn($userId) === "NONE")
	{
		$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
		$stmt = $connect->prepare("INSERT INTO LastOn VALUES(?, ?)");
		$stmt->bind_param("is", $userId, $lastOn);
		$stmt->execute();
	}
	else
	{
		$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
		$stmt = $connect->prepare("UPDATE LastOn SET ServerId=? WHERE Id=?");
		$stmt->bind_param("si", $lastOn, $userId);
		$stmt->execute();
	}
}



function populate_db()
{
	include('config.php');
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	mysqli_query($connect, "CREATE TABLE IF NOT EXISTS Users(Id INT, Username TEXT(16),Email TEXT(128),Country TEXT(128),SecurityQuestion Text(128),SecurityAnswerHash TEXT(128),Age INT,PassHash TEXT(128), Salt TEXT(128),Gender TEXT(16), Admin TEXT(3), Moderator TEXT(3))");
	mysqli_query($connect, "CREATE TABLE IF NOT EXISTS LastOn(Id INT, ServerId TEXT(1028))");
	mysqli_query($connect, "CREATE TABLE IF NOT EXISTS FourmThread(ThreadId INT, Title TEXT(100), Fourm TEXT(10), UpdateTime INT, Locked TEXT(3))");
	mysqli_query($connect, "CREATE TABLE IF NOT EXISTS FourmReply(ReplyId INT, ThreadId INT, CreatedBy TEXT(1028), Contents TEXT(65565), Fourm TEXT(10), CreationTime INT, MadeByAdmin TEXT(3))");
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