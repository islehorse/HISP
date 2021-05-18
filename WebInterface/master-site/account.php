<?php
include("servers.php");
include("common.php");
session_start();

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

if(isset($_SESSION["LOGGED_IN"]))
{
	if($_SESSION["LOGGED_IN"] !== "YES")
	{
		goto error;
	}
	else
	{
		goto pass;
	}
	
	error: 
		include("web/header.php");
		echo("
<B>Username or Password is not valid or your account has timed out. Please Log in again.</B><BR><BR></TD></TR></TABLE>");
		$_SESSION['LOGGED_IN'] = "NO";
		include("web/footer.php");
		exit();
	pass:
}
else
{
	goto error;
}
include("web/header.php");
?>
<?php #<TR><TD><IMG SRC=/web/servericons/pinto.gif></TD><TD><B><FONT COLOR=GREEN>You were on this server last time:</FONT><BR>SERVER: PINTO.HORSEISLE.COM</B><BR><BR></TD><TD><B>Not Subscribed</B><BR>Quest Points: 75pts<BR>Times Online: 3<BR>Last On: 0.84 days ago<BR></TD><TD><B>17<BR>players<BR>online<BR>now</B></TD><TD><B><A HREF=?CONNECT=pinto>[LOG IN]</A></B></TD></TR><TR><TD COLSPAN=5><HR>?>
<?php #<TABLE WIDTH=80% BGCOLOR=FFAABB BORDER=0 CELLPADDING=4 CELLSPACING=0><TR><TD class=newslist><B>[June 23, 2020 Latest Horse Isle News] Horse Isle 1 Compromise:</B><BR>Unfortunately, some troublemakers made a mess of HI1.<BR>We have reverted to a backup from 4am PST and taken some precautions. So, anything you "did" this morning was reverted.<br>We have also given all subs +12hrs to cover the down time.<br><br>Because passwords for accounts were likely compromised, we setup a system to verify and unlock for players' protection. When you try to login you will be prompted to reset your password.  We can automatically unlock most players' accounts, but some will require manual support via email.  Just follow the directions and please be patient with us.<br><br>Sorry about the trouble.  HI1 was never designed to survive so long into this new mean digital world. ;)<br><br>P.S.  The XSS alert was a simple javascript alert, just meaningless and harmless.<br><br>Thanks!<BR></TD></TR></TABLE> ?><BR><B>We have a <A HREF=//master.horseisle.com/beginnerguide/>Beginner Guide</A> online to help new players learn how to play.</B><BR><BR><B><FONT SIZE=+1>Horse Isle Server List</FONT></B><BR>Each server is completely independent and has identical game content. Money/horses/subscriptions are all tied to a particular server. 
Normally you will only play on one server.  <B>Playing on any server uses up playtime on all servers</B>, so you do not gain any free time. Reasons for playing on more than one include joining a friend, or in case your normal server is down. 
Multiple servers are required since there is a max capacity of around 150 players online per server.<BR><B>Please note, a profile on any individual server will be permanently deleted after 183 days (6 months) of not logging into the game on that specific server or your subscription expiring, whichever is later.</b><TABLE CELLPADDING=5 CELLSPACING=0 BORDER=0 BGCOLOR=FFFFFF><TR><TD COLSPAN=5></TD></TR><TR><TD COLSPAN=2><B>GAME SERVERS</B> (all identical please only join 1 or 2)</TD><TD><B>PROFILE</B> (not current)</TD><TD><B>ONLINE</B></TD><TD><B>LOGIN</B></TD></TR></TD></TR><TR><TD COLSPAN=5><HR></TD></TR><?php
for($i = 0; $i < count($server_list); $i++)
{
	$server = $server_list[$i];
	$icon = $server['icon'];
	$url = $server['site'];
	$desc = $server['desc'];
	
	echo('<TR><TD><IMG SRC=/web/servericons/'.$icon.'></TD><TD><B>SERVER: '.strtoupper($url).'</B><BR>'.$desc.'</BR></TD><TD>no existing profile</TD><TD><B>0<BR>players<BR>online<BR>now</B></TD><TD><B><A HREF=joinserver.php?SERVER='.$url.'>[JOIN]</A></B></TD></TR><TR><TD COLSPAN=5><HR></TD></TR>');
}

?>
</TABLE><BR>Account Settings: <A HREF=/web/accountchange.php>CHANGE MY PASSWORD</A><BR>Refer other players and earn Game Credit!: <A HREF=/web/referral.php>REFERRAL PROGRAM</A><BR>
<?php
include("web/footer.php");
?>