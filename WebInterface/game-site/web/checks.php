<?php
session_start();
include("config.php");
include("crosserver.php");
include("header.php");
?>
<TABLE WIDTH=100% CELLPADDING=10><TR><TD>
<FONT COLOR=880000 SIZE=+1><B>Alternative Payment Methods</B></FONT><BR>
If you cannot use PayPal(recommended) you may send a payment via snail mail to our U.S. Post Office Box.<BR>
Currency MUST be in U.S. Dollars.  [ USA Check / Money Order / USD Cash Accepted ]<BR>
(One exception, Canadian personal checks made out for slightly more than the current exchange rate in canadian funds can be accepted. No other countries personal checks can be accepted.)<BR>
Checks <B>must be written out to 'Horse Isle'</B>.<BR>
If your check "bounces" we will block the account until our fees have been reimbursed by you.<BR>
Cash is not recommended, but if you need to send it,  be sure to wrap it in another piece of paper so that it cannot be seen through the envelope!<BR>
<B>(Do not send Cash without Parental Permission!)</B><BR>
<BR>
<B>Horse Isle Postal Mailing Address:</B><BR>
<UL><FONT COLOR=440044 SIZE=+0>
Horse Isle<BR>
PO Box 3619<BR>
Duluth, MN 55803-2633<BR>
USA<BR>
</UL></FONT>
<B>Identify Your Payment:</B><BR>
Be sure to include a CLEAR note of what account this is for. Include your email address in case there are problems identifying the account.<BR>
<UL><FONT COLOR=440044 SIZE=+0>
Your USERNAME = <?php echo(htmlspecialchars($_SESSION['USERNAME'])); ?><B></B><BR>
Your ACCOUNT ID = <?php echo(htmlspecialchars($_SESSION['PLAYER_ID'])); ?><B></B><BR>
Your SERVER = <B><?php echo($server_id); ?></B> (make sure this is the one you play on)<BR>

</UL></FONT>
<B>Finally, let us know what it is for:</B><BR>
<UL><FONT COLOR=440044 SIZE=+0>
One Month Horse Isle Membership - $5 (or 2 for $10, etc.)<BR>
One Year Horse Isle Membership - $40 (or 2 for $80, etc.)<BR>
Horse Isle Game Money - $10,000 per $1 ($15 = $150,000 Horse Isle Money)<BR>
Pawneer Order - $8 (or 2 for $16, etc.)<BR>
Pawneer Order Pack(5) - $30 (or 2 for $60, etc.)<BR>
</UL></FONT>
Payments will be credited when received. Mail is handled at least twice per week, so between mail transit and pickup times, expect up to a week for the account to be credited.  Payments lost in the mail are not our responsibility.  Checks which cannot be identified to an account will not be cashed.<BR>
Remember PayPal Payments are instant and more secure! <BR>
Thanks!<BR>
<CENTER>[ <A HREF=/account.php>Return to Account Page</A> ]
</TD></TR></TABLE>
<?php
include("footer.php");
?>