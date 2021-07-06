<?php
session_start();
include("config.php");
include("header.php");
?>
<BR>
<CENTER><TABLE CELLPADDING=5><TR><TD>

<font size="4" style="COLOR:#990000"><span style="FONT-WEIGHT:bold">Why is play time limited?</span></font><br/>
<br/>
The servers have to work very hard for each player logged in.   We have high-end dedicated servers, 
but they can only run 150-200 players online at once.  Dedicated servers are expensive.
  For these reasons, free players have a limited amount of playtime per day, and are even
 denied access when the server is nearing capacity.  Subscribers have unlimited access, as they are sharing the costs of running the server.

<BR>
<BR><CENTER> <B><A HREF=/account.php>RETURN TO ACCOUNT</A>
</TD></TR></TABLE>
<?php
include("footer.php");
?>