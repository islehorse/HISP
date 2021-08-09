<?php
session_start();
include("../config.php");
include("../common.php");
include("header.php");

if(isset($_POST['TITLE'], $_POST['CONTENT'])){
	if(is_logged_in()){
		if($_SESSION['ADMIN'] == "YES"){
			if(!($_POST['TITLE'] == "" || $_POST['CONTENT'] == ""))
				post_news($_POST['TITLE'], $_POST['CONTENT']);			
		}
	}
}
?>

<FONT SIZE=+1><B>Horse Isle News</B></FONT><BR>
Welcome to Horse Isle News.  Changes and additions to game are posted here regularly. Newest entries are on top.
<BR>

<TABLE WIDTH=80% BGCOLOR=FFAABB BORDER=0 CELLPADDING=4 CELLSPACING=0><TR><TD class=newslist><?php
if(isset($_GET['NEWSALL'])){
	echo('<B>All Horse Isle News:</B> [ <A HREF=?>CLOSE</A> ]<BR>');
}
else if(isset($_GET['NEWSID'])){
	echo('<B>Selected Horse Isle News:</B> [ <A HREF=?NEWSALL=1>SHOW ALL</A> ] [ <A HREF=?>CLOSE</A> ]<BR>');
}
else{
	echo('<B>Most Recent Horse Isle News:</B> [ <A HREF=?NEWSALL=1>SHOW ALL</A> ]<BR>');
}
?><BR><?php 
$news_list = null;
if(isset($_GET['NEWSALL'])){
	$news_list = get_all_news();
}
else if(isset($_GET['NEWSID'])){
	$news_list = get_news_id(intval($_GET['NEWSID']));
}
else{
	$news_list = get_recent_news();	
}

for($i = 0; $i < count($news_list); $i++)
{
	$news = $news_list[$i];
	
	echo('<B> [ '.date("F j, Y",$news['date']).' ] <FONT COLOR=880000>'.$news['title'].'</FONT>:</B><BR> &nbsp;&nbsp;&nbsp;&nbsp;');
	echo($news['contents'].'<BR><BR>');
}
?></TD></TR></TABLE><BR><?php
if(is_logged_in()){
	if($_SESSION['ADMIN'] == 'YES'){
		echo("<HR><FORM METHOD=POST>Add a news post: TITLE:<INPUT TYPE=TEXT NAME=TITLE SIZE=30><BR><TEXTAREA NAME=CONTENT ROWS=4 COLS=60></TEXTAREA><BR><INPUT TYPE=SUBMIT VALUE='POST NEWS'><HR>");
	}
}
include("footer.php");
?>