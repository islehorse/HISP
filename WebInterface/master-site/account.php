<?php
session_start();
include("servers.php");
include("common.php");
include("crosserver.php");


if(isset($_POST["USER"], $_POST["PASS"]))
{
	$username = $_POST["USER"];
	$password = $_POST["PASS"];
	if(!user_exists($username))
		goto auth_failed;
	$id = get_userid($username);
	if(check_password($id, $password))
	{
		$_SESSION['LOGGED_IN'] = "YES";
		$_SESSION['PLAYER_ID'] = $id;
		$_SESSION['USERNAME'] = $username;
		$_SESSION['SEX'] = get_sex($id);
		$_SESSION['ADMIN'] = get_admin($id);
		$_SESSION['MOD'] = get_mod($id);
		$_SESSION['PASSWORD_HASH'] = get_password_hash($id);
		$_SESSION['SALT'] = get_salt($id);
		
		if($_SESSION['ADMIN'] == 'YES')
			$_SESSION['MOD'] = 'YES';
	}
	else
	{ 
auth_failed:
		include("web/header.php");
		echo('<TABLE CELLPADDING=10 WIDTH=100%><TR><TD><HR><B>Username or Password is not valid. Please try again. <BR>Note: Upon too many attempts the account will be temporarily blocked from your IP.</B><BR><BR>If you have not logged on yet,  make sure you have clicked the activation link in the email that was sent to you.<BR><BR>click <A HREF=/web/forgotpass.php>HERE</A> to Have your password emailed to you.<HR></TD></TR></TABLE>');
		include('web/footer.php');
		exit();
	}
}

if(!is_logged_in())
{
	include("web/header.php");
	echo("
	<B>Username or Password is not valid or your account has timed out. Please Log in again.</B><BR><BR></TD></TR></TABLE>");
	$_SESSION['LOGGED_IN'] = "NO";
	include("web/footer.php");
	exit();
}

if(isset($_GET['CONNECT']))
{
	$server_id = $_GET['CONNECT'];
	$server = getServerById($server_id);
	
	if($server !== null)
	{
		$playerId = $_SESSION['PLAYER_ID'];
		
		$hmac = GenHmacMessage((string)$playerId, "CrossSiteLogin");
		$redirectUrl = $server['site'];
		
		if(!endsWith($redirectUrl, '/'))
			$redirectUrl .= '/';
		
		$redirectUrl .= 'account.php?SLID='.(string)$playerId.'&C='.base64_url_encode(hex2bin($hmac));
		set_LastOn($playerId, $server_id);
		header("Location: ".$redirectUrl);
		exit();
	}
}
include("web/header.php");

$player_id = $_SESSION['PLAYER_ID'];
$lastOnServer = get_LastOn($player_id);
$moveToFront = getServerById($lastOnServer);

if($moveToFront !== null){
	for($i = 0; $i < count($server_list); $i++){
		if($server_list[$i]['id'] == $lastOnServer)
			unset($server_list[$i]);
	}
	array_unshift($server_list, $moveToFront);
}

?>
<?php #<TABLE WIDTH=80% BGCOLOR=FFAABB BORDER=0 CELLPADDING=4 CELLSPACING=0><TR><TD class=newslist><B>[June 23, 2020 Latest Horse Isle News] Horse Isle 1 Compromise:</B><BR>Unfortunately, some troublemakers made a mess of HI1.<BR>We have reverted to a backup from 4am PST and taken some precautions. So, anything you "did" this morning was reverted.<br>We have also given all subs +12hrs to cover the down time.<br><br>Because passwords for accounts were likely compromised, we setup a system to verify and unlock for players' protection. When you try to login you will be prompted to reset your password.  We can automatically unlock most players' accounts, but some will require manual support via email.  Just follow the directions and please be patient with us.<br><br>Sorry about the trouble.  HI1 was never designed to survive so long into this new mean digital world. ;)<br><br>P.S.  The XSS alert was a simple javascript alert, just meaningless and harmless.<br><br>Thanks!<BR></TD></TR></TABLE> ?>
<?php
if(!userExistAny($player_id))
	echo('<BR><B>We have a <A HREF=//'.$_SERVER['HTTP_HOST'].'/beginnerguide/>Beginner Guide</A> online to help new players learn how to play.</B><BR>');
?><BR><B><FONT SIZE=+1>Horse Isle Server List</FONT></B><BR>Each server is completely independent and has identical game content. Money/horses/subscriptions are all tied to a particular server. 
Normally you will only play on one server.  <B>Playing on any server uses up playtime on all servers</B>, so you do not gain any free time. Reasons for playing on more than one include joining a friend, or in case your normal server is down. 
Multiple servers are required since there is a max capacity of around 150 players online per server.<BR><B>Please note, a profile on any individual server will be permanently deleted after 183 days (6 months) of not logging into the game on that specific server or your subscription expiring, whichever is later.</b><TABLE CELLPADDING=5 CELLSPACING=0 BORDER=0 BGCOLOR=FFFFFF><TR><TD COLSPAN=5><?php # <BR><FONT COLOR=550000><B>You have 8 rule violation points against your account. [ <A HREF=/web/rulesbroken.php>REVIEW VIOLATIONS</A> ]</B></FONT><BR> ?></TD></TR><TR><TD COLSPAN=2><B>GAME SERVERS</B> (all identical please only join 1 or 2)</TD><TD><B>PROFILE</B> (not current)</TD><TD><B>ONLINE</B></TD><TD><B>LOGIN</B></TD></TR></TD></TR><TR><TD COLSPAN=5><HR></TD></TR>
<?php


for($i = 0; $i < count($server_list); $i++)
{
	$server = $server_list[$i];
	$icon = $server['icon'];
	$url = $server['site'];
	$desc = $server['desc'];
	$id = $server['id'];
	$database = $server['database'];
	
	$domain = parse_url($url, PHP_URL_HOST);
	$join = '';
	$num_on = getNoSubbedPlayersOnlineInServer($database);
	
	$pExist = userid_exists($database, $player_id);
	if(!$pExist)
		$join = '<A HREF=joinserver.php?SERVER='.$id.'>[JOIN]</A>';
	else
		$join = '<A HREF=?CONNECT='.$id.'>[LOG IN]</A>';
	
	
	echo('<TR><TD><IMG SRC=/web/servericons/'.$icon.'></TD><TD><B>');
	if($lastOnServer === $id)
		echo('<FONT COLOR=GREEN>You were on this server last time:</FONT><BR>');
	echo('SERVER: '.strtoupper($domain).'</B><BR>'.$desc.'</BR></TD>');
	if(!$pExist)
	{
		echo('<TD>no existing profile</TD>');
	}
	else
	{
		$newUser = getUserExistInExt($database, $player_id);
		
		if(!$newUser){
			$loginDate = getUserLoginDate($database, $player_id);
			$questPoints = getUserQuestPoints($database, $player_id);
			$totalLogins = getUserTotalLogins($database, $player_id);
			$subbed = getUserSubbed($database, $player_id);
		}
		else
		{
			$loginDate = time();
			$questPoints = 0;
			$totalLogins = 0;
			$subbed = false;
		}
		
		echo('<TD>');
		if($subbed)
			echo('<FONT COLOR=GREEN><B>ACTIVE SUBSCRIPTION</B></FONT>');
		else
			echo('<B>Not Subscribed</B>');
		
		$lastOn = 0.00;
		$current_time = time();
		$difference = $current_time - $loginDate;
		$lastOn = $difference/86400;


		echo('<BR>Quest Points: '.$questPoints.'.pts<BR>');
		echo('Times Online: '.$totalLogins.'<BR>');
		echo('Last On: '.number_format((float)$lastOn, 2, '.', '').' days ago<BR>');
		echo('</TD>');
	}
	echo('<TD><B>'.$num_on.'<BR>players<BR>online<BR>now</B></TD><TD><B>'.$join.'</B></TD></TR><TR><TD COLSPAN=5><HR></TD></TR>');
}

?>
</TABLE><BR>Account Settings: <A HREF=/web/accountchange.php>CHANGE MY PASSWORD</A><BR>Refer other players and earn Game Credit!: <A HREF=/web/referral.php>REFERRAL PROGRAM</A><BR>
<?php
include("web/footer.php");
?>