<?php
include("../../config.php");

session_start(['cookie_lifetime' => 86400]);
$_SESSION["logged_in"] = false;
include("../header.php");
?>


<CENTER>
<FONT FACE=Verdana,arial SIZE=-1>
<BR><B>HISP - Super Admin Login</B><BR>
<BR> This page requires a password, please enter it below:</BR>
<BR> <FORM METHOD=POST ACTION=/admin/login.php>
	<INPUT TYPE=PASSWORD SIZE=30 NAME=PASS></INPUT>
	<INPUT TYPE=SUBMIT VALUE=LOGIN>
	</FORM>
</BR>
<BR><B>No idea? check config.php of game-site/</B></BR>


<?php
include("../footer.php");
?>