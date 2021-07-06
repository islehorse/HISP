<?php
if(!isset($master_site))
	include('config.php');

if(session_status() !== PHP_SESSION_ACTIVE)
	session_start();

if(!function_exists('is_logged_in'))
	include('common.php');

$host_names = explode(".", $host);
$host = $host_names[count($host_names)-2] . "." . $host_names[count($host_names)-1];

?>
<HEAD>
<TITLE>HORSE ISLE - Online Multiplayer Horse Game</TITLE>
<META NAME="keywords" CONTENT="Horse Game Online MMORPG Multiplayer Horses RPG Girls Girly Isle World Island Virtual Horseisle Sim Virtual">
<META NAME="description" CONTENT="A multiplayer online horse world where players can capture, train, care for and compete their horses against other players. A very unique virtual sim horse game.">
<link rel="shortcut icon" href="/favicon.ico" type="image/x-icon">
<link rel="icon" href="/favicon.ico" type="image/x-icon">
<link rel="meta" href="<?php echo("//".$host); ?>/labels.rdf" type="application/rdf+xml" title="ICRA labels" />
<meta http-equiv="pics-Label" content='(pics-1.1 "//www.icra.org/pics/vocabularyv03/" l gen true for "<?php echo("//".$host); ?>" r (n 0 s 0 v 0 l 0 oa 0 ob 0 oc 0 od 0 oe 0 of 0 og 0 oh 0 c 1)  gen true for "<?php echo($master_site); ?>" r (n 0 s 0 v 0 l 0 oa 0 ob 0 oc 0 od 0 oe 0 of 0 og 0 oh 0 c 1))' />
<style type="text/css">
hr {
height: 1;
color: #000000;
background-color: #000000;
border: 0;
}
a {
font: bold 14px arial;
color: #6E3278;
}
TH {
background-color: #EDE5B4;
padding: 1px 6px;
border: 2px dotted #6E3278;
font: small-caps 900 14px arial;
color: #000000;
}
TR.a0 {
background-color: #EDE5B4;
}
TR.a1 {
background-color: #D4CCA1;
}
TD {
font: 14px arial;
color: #000000;
}
TD.forum {
font: 12px arial;
color: #000000;
}
TD.forumlist {
padding: 1px 6px;
border: 2px dotted #6E3278;
background-color: #EDE5B4;
text-align: center;
font: bold 14px arial;
color: #000000;
}
TD.forumpost {
padding: 5px 10px;
border: 2px dotted #6E3278;
background-color: #EDE5B4;
text-align: left;
}
TD.newslist {
padding: 4px 4px;
border: 2px dotted #6E3278;
background-color: #FFDDEE;
text-align: left;
font: 14px arial;
color: #000000;
}
FORUMSUBJECT {
font: bold 14px arial;
color: #004400;
}
FORUMUSER {
font: 12px arial;
color: #000044;
}
FORUMDATE {
font: 12px arial;
color: #444444;
}
FORUMTEXT {
font: 14px arial;
color: #440000;
}

</style>
</HEAD>
<BODY BGCOLOR=E0D8AA>
<TABLE BORDER=0 CELLPADDING=0 CELLSPACING=0 WIDTH=100%>
<TR WIDTH=100%>
<TD WIDTH=512 ROWSPAN=3><A HREF=/><IMG SRC=/web/hoilgui1.gif ALT="Welcome to Horse Isle" BORDER=0></A></TD>
<TD WIDTH=100% BACKGROUND=/web/hoilgui2.gif>&nbsp;</TD>
<TD WIDTH=29><IMG SRC=/web/hoilgui3.gif></TD>
</TR>
<TR>
<TD WIDTH=100% BACKGROUND=/web/hoilgui4.gif align=right>
<B>

<?php if(isset($login_error)){echo($login_error);} ?>

<?php
	if(is_logged_in())
	{
		$username = $_SESSION['USERNAME'];		
		echo('<TABLE BORDER=0 CELLPADDING=0 CELLSPACING=10><TR><TD><B><A HREF=/account.php>'.strtoupper($_SERVER['HTTP_HOST']).'</A><BR>Logged in as: '.htmlspecialchars($username).'<BR><A HREF=/?LOGOUT=1><img src=/web/but-logout.gif border=0></A><BR><A HREF='.$master_site.'/><img src=/web/but-mainpage.gif border=0></A></TD><TD><BR><A HREF='.$master_site.'/account.php><img src=/web/but-serverlist.gif border=0></A><BR><A HREF='.$master_site.'/web/news.php><img src=/web/but-news.gif border=0></A><BR><A HREF='.$master_site.'/web/forums.php><img src=/web/but-forums.gif border=0></A><BR><A HREF='.$master_site.'/web/helpcenter.php><img src=/web/but-helpcenter.gif border=0></A></TD></TR></TABLE>');
	}
	else
	{
		echo('<TABLE CELLPADDING=0 CELLSPACING=2 BORDER=0><FORM METHOD=POST ACTION=/account.php>
<TR><TD><B>USER:</B></TD><TD><INPUT TYPE=TEXT SIZE=14 NAME=USER></TD></TR>
<TR><TD><B>PASS:</B></TD><TD><INPUT TYPE=PASSWORD SIZE=14 NAME=PASS></TD></TR>
<TR><TD></TD><TD><INPUT TYPE=SUBMIT VALUE=LOGIN> (<A HREF='.$master_site.'/web/forgotpass.php>Forgot?</A>)</TD></TR></FORM></TABLE>');
	}
	
?>

</TD>
<TD WIDTH=29><IMG SRC=/web/hoilgui5.gif></TD></TR>
<TR>
<TD WIDTH=100% BACKGROUND=/web/hoilgui6.gif>&nbsp;</TD>
<TD WIDTH=29><IMG SRC=/web/hoilgui7.gif></TD></TR>
</TABLE>
<CENTER>