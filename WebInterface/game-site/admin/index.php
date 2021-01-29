<?php
include("../config.php");

session_start(['cookie_lifetime' => 86400]);
$_SESSION["logged_in"] = false;
?>
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



<TABLE CELLPADDING=0 CELLSPACING=2 BORDER=0><FORM METHOD=POST ACTION=/account.php>
<TR><TD><B>USER:</B></TD><TD><INPUT TYPE=TEXT SIZE=14 NAME=USER></TD></TR>
<TR><TD><B>PASS:</B></TD><TD><INPUT TYPE=PASSWORD SIZE=14 NAME=PASS></TD></TR>
<TR><TD></TD><TD><INPUT TYPE=SUBMIT VALUE=LOGIN> (<A HREF=//master.horseisle.com/web/forgotpass.php>Forgot?</A>)</TD></TR></FORM></TABLE>

</TD>
<TD WIDTH=29><IMG SRC=/web/hoilgui5.gif></TD></TR>
<TR>
<TD WIDTH=100% BACKGROUND=/web/hoilgui6.gif>&nbsp;</TD>
<TD WIDTH=29><IMG SRC=/web/hoilgui7.gif></TD></TR>
</TABLE>
<CENTER>

<CENTER>
<FONT FACE=Verdana,arial SIZE=-1>
<BR><B>HISP - Admin Portal</B><BR>
<BR> This page requires a password, please enter it below:</BR>
<BR> <FORM METHOD=POST ACTION=/admin/login.php>
	<INPUT TYPE=PASSWORD SIZE=30 NAME=PASS></INPUT>
	<INPUT TYPE=SUBMIT VALUE=LOGIN>
	</FORM>
</BR>
<BR><B>No idea? check config.php of game-site/</B></BR>


<TABLE BORDER=0 CELLPADDING=0 CELLSPACING=0 WIDTH=100%>
<TR>
<TD><IMG SRC=/web/hoilgui10.gif></TD>
<TD WIDTH=100% BACKGROUND=/web/hoilgui11.gif></TD>
<TD><IMG SRC=/web/hoilgui12.gif></TD>
</TR></TABLE>
<CENTER><B>
[ <A HREF=http://hi1.horseisle.com/web/rules.php>Rules</A> ]
[ <A HREF=http://hi1.horseisle.com/web/termsandconditions.php>Terms and Conditions</A> ]
[ <A HREF=http://hi1.horseisle.com/web/privacypolicy.php>Privacy Policy</A> ]</B><BR>
[ <A HREF=http://hi1.horseisle.com/web/expectedbehavior.php>Expected Behavior</A> ]
[ <A HREF=http://hi1.horseisle.com/web/contactus.php>Contact Us</A> ] 
[ <A HREF=http://hi1.horseisle.com/web/credits.php>Credits</A> ]<BR>
<FONT FACE=Verdana,Arial SIZE=-2>Copyright &copy; 2020 Horse Isle</FONT>

<!-- Google Analytics -->
<script src="http://www.google-analytics.com/urchin.js" type="text/javascript">
</script>
<script type="text/javascript">
_uacct = "UA-1805076-1";
urchinTracker();
</script>

