<?php
session_start();
include("../common.php");
include("header.php");

if(!is_logged_in()){
	echo('<B>Must be logged in to use this tool!<BR>');
	include("footer.php");
	exit();
}
?>

<CENTER><TABLE WIDTH=90% BORDER=0><TR><TD VALIGN=top>

<FONT SIZE=+2><B>Horse Isle Modify Account:</B></FONT><BR>
<I>Use the following to change your account details</I><BR>
<BR>
<FORM METHOD=POST>
<B>CHANGE PASSWORD</B> For player <?php echo(htmlspecialchars($_SESSION['USERNAME'])); ?><BR>
Existing password: <INPUT TYPE=PASSWORD SIZE=16 MAX=16 VALUE="" NAME="opass"><I><FONT SIZE-1></FONT></I><BR>
Desired NEW password: <INPUT TYPE=PASSWORD SIZE=16 MAX=16 VALUE="" NAME="pass1"><I><FONT SIZE-1>[6-16 both letters and numbers only, case insensitive]</FONT></I><BR>
Repeat&nbsp; NEW password: <INPUT TYPE=PASSWORD SIZE=16 MAX=16 VALUE="" NAME="pass2"><I><FONT SIZE-1>[ same as above ]</FONT></I><BR>


<BR>
<INPUT TYPE=SUBMIT VALUE='CHANGE MY PASSWORD'><BR>
</FORM>
Please be patient while the script attempts to update your password on all servers.
<BR><BR>
<A HREF=/>Go Back to Main Page</A><BR><BR>


</TD></TR></TABLE>

<?php
include("footer.php");
?>