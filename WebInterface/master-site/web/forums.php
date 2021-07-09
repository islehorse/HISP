<?php
session_start();
include("../common.php");
include("header.php");
?>

<?php
	if(!is_logged_in())
		goto ex;
	if(isset($_POST['SUBJECT'], $_POST['TEXT'], $_POST['TEXT'], $_POST['FORUM']))
	{
		$subject = $_POST['SUBJECT'];
		$text = $_POST['TEXT'];
		$forum = strtoupper($_POST['FORUM']);
		
		if(!($forum === "SUPPORT" || $forum === "BUGS" || $forum === "GENERAL" || $forum === "HORSES" || $forum === "GAME" || $forum === "MOD"))
			goto ex;
		$subject = substr($subject, 0, 100);
		$text = substr($text, 0, 65565);
		
		$thread = create_fourm_thread($subject, $forum);
		create_fourm_reply($thread, $_SESSION['USERNAME'], $text, $forum);
	}
	
	ex:
?>
<B><FONT SIZE=+1>Horse Isle Forums</FONT><BR></B>
Forums for discussing in game topics with other players.  Please use the Contact Us form at the bottom to directly communicate with Horse Isle staff.
<BR>The SUPPORT and BUGS forums have threads removed often to keep them clean and recent. Don't be offended when removed.
<!--<BR><B>Please respect the fact that these forums were not designed for RPG'ing and we do not have the time or ability to properly manage the excessive posting that it entails. We are sorry to those that were not abusing the rules,  but too many others were.  As a result.  NO RPG posting in these forums.  Period.   Thanks for understanding.</B>-->
<TABLE WIDTH=100%><TR><TD class=forumlist><A HREF="?FORUM=SUPPORT">SUPPORT</A><BR>(<?php echo(count_topics("SUPPORT")); ?> topics)</TD><TD class=forumlist><A HREF="?FORUM=BUGS">BUGS</A><BR>(<?php echo(count_topics("BUGS")); ?> topics)</TD><TD class=forumlist><A HREF="?FORUM=GENERAL">GENERAL</A><BR>(<?php echo(count_topics("GENERAL")); ?> topics)</TD><TD class=forumlist><A HREF="?FORUM=HORSES">HORSES</A><BR>(<?php echo(count_topics("HORSES")); ?> topics)</TD><TD class=forumlist><A HREF="?FORUM=GAME">GAME</A><BR>(<?php echo(count_topics("GAME")); ?> topics)</TD></TABLE><?php 
if(isset($_GET['FORUM'])){
	$forum = strtoupper($_GET['FORUM']);
	if(!($forum === "SUPPORT" || $forum === "BUGS" || $forum === "GENERAL" || $forum === "HORSES" || $forum === "GAME" || $forum === "MOD"))
	{
		echo('Unknown Forum');
		exit();
	}
	echo('<HR><B>VIEWING '.htmlspecialchars($forum).' FORUM</B>');
	echo(' &nbsp; current server time: '.date("M j g:ia").'<BR>');
	echo('<TABLE WIDTH=100%><TR><TH>TOPIC</TH><TH>POSTS</TH><TH>ORIGINAL POST</TH></TR>');
	
	$alternate = ['a1', 'a0'];
	$threads = get_fourm_threads($forum);
	for($i = 0; $i < count($threads); $i++)
	{
		echo('<TR class='.$alternate[$i % 2].'>');
		echo('<TD class=forum><A HREF="?FORUM='.htmlspecialchars($forum).'&VIEWID='.htmlspecialchars($threads[$i]['id']).'">');
		echo(htmlspecialchars($threads[$i]['title']).'</A></TD>');
		echo('<TD class=forum><B>'.count_replies($threads[$i]['id']).'</B> (last by <B><FONT COLOR=333399>'.get_last_reply_author($threads[$i]['id']).'</FONT></B> ');
		$createTime = get_last_reply_time($threads[$i]['id']);
		
		$minsAgo = 0;
		$current_time = time();
		$difference = $current_time - $createTime;
		$minsAgo = $difference/60;
		$daysAgo = $difference/86400;
		
		if($minsAgo <= 1440)
			echo('<FONT COLOR=880000><B>'.number_format((float)$minsAgo, 0, '.', '').' min ago</B></FONT>');
		else
			echo(number_format((float)$daysAgo, 0, '.', '').' days ago');
		
		echo(')</TD><TD class=forum>'.date("M j g:ia", get_first_reply_time($threads[$i]['id'])).' by <B><FONT COLOR=333399>'.get_first_reply_author($threads[$i]['id']).'</FONT></B></TD></TR>');
	}
	echo("</TABLE><HR><FORM METHOD=POST>Add a post to this forum: SUBJECT:<INPUT TYPE=TEXT NAME=SUBJECT SIZE=30><BR><TEXTAREA NAME=TEXT ROWS=4 COLS=60></TEXTAREA><BR><INPUT TYPE=SUBMIT VALUE='ADD TOPIC'><INPUT TYPE=HIDDEN NAME=FORUM VALUE='".htmlspecialchars($forum)."'></FORM>[ <A HREF=?>CLOSE FORUMS</A> ]<BR>");
	
}
?><BR><?php
include("footer.php");
?>