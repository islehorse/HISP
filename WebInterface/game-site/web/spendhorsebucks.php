<?php
session_start();
include("common.php");
include("config.php");
include("header.php");
if(!is_logged_in()){
	echo('Account information not found.  please login again.');
	exit();
}
?>
<B><FONT SIZE=+1>Horse Isle Horse Bucks Redemption</FONT></B><BR>You Currently have 0 Horse Bucks from Referrals/Prizes.<BR>You do not have at least 5 Horse Bucks to make an exchange.<BR><A HREF=/account.php>ACCOUNT PAGE</A><?php
include("footer.php");
?>