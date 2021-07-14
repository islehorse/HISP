<?php
include("../common.php");
include("../config.php");
include("header.php");
?>
<BR>
<CENTER><TABLE CELLPADDING=5><TR><TD>

<?php
if(isset($_GET["ACCEPT"]))
	echo('<I>In order to play Horse Isle,  you must Read, Understand,  and Accept the Privacy Policy Below:</I><BR>
<BR>


');
?>
<div style="TEXT-ALIGN:center">
  <font size="4" style="COLOR:#990000"><span style="FONT-WEIGHT:bold">Horse Isle
  Privacy Statement</span></font><br/>
</div>
<br/>
<br style="FONT-WEIGHT:bold; COLOR:#990000"/>
<span style="FONT-WEIGHT:bold"><span style="COLOR:#990000">YOUR PERSONAL
INFORMATION:</span><br/>
</span><span style="FONT-STYLE:italic">UNDER
13:</span><span style="FONT-WEIGHT:bold">&nbsp; </span>If a player is 12 years
old or younger, we will collect the Parent/Guardian's email instead of the
player's.&nbsp; We collect no personally identifiable information from players
under 13 years old.<br/>
<br/>
Horse Isle recognizes your privacy. We collect certain information to allow us
to run the game. We store usernames and passwords along with a player's email,
age, and country when you sign-up. We use cookies to allow secure access by
users. Subscribers who do not accept cookies from the domain "horseisle.com"
cannot access most areas of the site.&nbsp; We log site and game activity (such
as the IP address of users) to allow us to better manage the site.&nbsp; We may
use this information to exclude visitors who violate our rules.<br/>
<br/>
We have clear rules disallowing communication between players of personally
identifiable information.&nbsp; We also strive to have usernames that do not
contain any "hints" to the player's true identity.&nbsp; Emails and IP addresses
of players are not visible to anyone who is not a Horse Isle staff member.<br/>
<br/>
The only personally identifiable information we require from anyone is a valid
email address.&nbsp; The email address is only used to send the initial
activation of the account, and to make sure only one account is setup per email.
The email will not be used again except for password recovery by request of a
player.&nbsp; Other information asked for is not personally identifiable.&nbsp;
That is country of residence, gender (determines whether you are referred to as
he/she in the game) and age .<br/>
<br/>
Horse Isle does not use advertising as a source of revenue.&nbsp; So NO
information is sold to others or provided to others for marketing information.<br/>
<br/>
All actions within the game may be logged and reviewed by Horse Isle
staff.&nbsp; This includes, but is not limited to, chats and private chats.<br/>
<br/>
Upon closing an account ALL information is deleted about a player.&nbsp;
including the email and game profile we stored.<br/>
<br/>
<span style="FONT-WEIGHT:bold; COLOR:#990000">YOUR ONLINE ACTIVITY:</span><br/>
All chats, including private chats, and descriptions (player, horse or ranch)
may be monitored by Horse Isle staff and moderators.&nbsp; At our discretion
this content may be reproduced for any purpose,&nbsp; or given to any authority
such as parents, FBI, etc.<br/>
<br/>
If we ever discover any sign of predator behavior we will pass it along to the
proper authorities with as much identifiable information that we may have.&nbsp;
Including player's username, password, email, IP address, chat logs, payment
information, etc..&nbsp; We put online safety above the privacy of an
individual.&nbsp; NEVER make any attempt to find out where another player lives
or any personally identifiable information.<br/>
<br/>
<?php
if(isset($_GET['ACCEPT'])){
	echo('<BR><CENTER><BR>
In order to play Horse Isle,  you must Read, Understand,  and Accept the Privacy Policy Above.<BR>
<BR>
Answer honestly here. You will still get to play if you are 12 or younger.<BR>
<FONT SIZE=+1><B>[ <A HREF=newuser.php?A=1>I ACCEPT AND I AM 12 OR YOUNGER</A> ]</B><BR>
<B>[ <A HREF=newuser.php?A=2>I ACCEPT AND I AM 13 OR OLDER</A> ]</B><BR>
<B>[ <A HREF=/>I DO NOT ACCEPT</A> ]</B><BR>
</FONT></CENTER><BR>');
}
?>
</TD></TR></TABLE>
<?php
include('footer.php');
?>