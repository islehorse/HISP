<?php
include("web/common.php");
include("web/crosserver.php");
include("config.php");

if(isset($_POST["USER"], $_POST["PASS"]))
{
	$username = $_POST["USER"];
	$password = $_POST["PASS"];
	if(!user_exists($username))
		goto auth_failed;
	$id = get_userid($username);
	if(check_password($id, $password))
	{
		$_SESSION['LOGGED_IN'] = "YES";
		$_SESSION['PLAYER_ID'] = $id;
		$_SESSION['USERNAME'] = $username;
		$_SESSION['SEX'] = get_sex($id);
		$_SESSION['ADMIN'] = get_admin($id);
		$_SESSION['MOD'] = get_mod($id);
		$_SESSION['PASSWORD_HASH'] = get_password_hash($id);
		$_SESSION['SALT'] = get_salt($id);
	}
	else
	{ 
auth_failed:
		include("web/header.php");
		echo('<TABLE CELLPADDING=10 WIDTH=100%><TR><TD><HR><B>Username or Password is not valid. Please try again. <BR>Note: Upon too many attempts the account will be temporarily blocked from your IP.</B><BR><BR>If you have not logged on yet,  make sure you have clicked the activation link in the email that was sent to you.<BR><BR>click <A HREF=/web/forgotpass.php>HERE</A> to Have your password emailed to you.<HR></TD></TR></TABLE>');
		include('web/footer.php');
		exit();
	}
}


if(isset($_GET["SLID"], $_GET["C"]))
{
	$id = $_GET['SLID'];
	$code = $_GET['C'];
	
	$hmac = GenHmacMessage((string)$playerId, "CrossSiteLogin");
	$hmacSent = base64_decode(bin2hex($hmac));
	if(hash_equals($hmacSent,$hmac))
	{
		$_SESSION['LOGGED_IN'] = "YES";
		$_SESSION['PLAYER_ID'] = $id;
		$_SESSION['USERNAME'] = get_username($id);
		$_SESSION['SEX'] = get_sex($id);
		$_SESSION['ADMIN'] = get_admin($id);
		$_SESSION['MOD'] = get_mod($id);
		$_SESSION['PASSWORD_HASH'] = get_password_hash($id);
		$_SESSION['SALT'] = get_salt($id);
	}
	else
	{
		$login_error = "Error in Automatic Login Authentication!";
	}
}

if(!is_logged_in())
{
	include("web/header.php");
	echo("
	<B>Username or Password is not valid or your account has timed out. Please Log in again.</B><BR><BR></TD></TR></TABLE>");
	$_SESSION['LOGGED_IN'] = "NO";
	include("web/footer.php");
	exit();
}

include("web/header.php");
?>



<script language="javascript1.3">
<!--
function ajax(url,target) {
    // native XMLHttpRequest object
    //document.getElementById(target).innerHTML = 'sending...';
    if (window.XMLHttpRequest) {
        req = new XMLHttpRequest();
        req.onreadystatechange = function() {ajaxDone(target);};
        req.open("GET", url, true);
        req.send(null);
    // IE/Windows ActiveX version
    } else if (window.ActiveXObject) {
        req = new ActiveXObject("Microsoft.XMLHTTP");
        if (req) {
            req.onreadystatechange = function() {ajaxDone(target);};
            req.open("GET", url, true);
            req.send();
        }
    }
}    

function ajaxDone(target) {
    // only if req is "loaded"
    if (req.readyState == 4) {
        // only if "OK"
        if (req.status == 200) {
            results = req.responseText;
            document.getElementById(target).innerHTML = results;
        } else {
            document.getElementById(target).innerHTML="ajax error:\n" +
                req.statusText;
        }
    }
}

function loadplayers() {
  ajax('web/playersonline.php?id=588112','PLAYERS');
  window.setTimeout("loadplayers()", 30000);  //reload player list every millisecs
}
window.setTimeout("loadplayers()", 10); ///load player list first time quick
window.setTimeout("loadplayers()", 3000); ///load player list first time quick

-->
</script>
<script>
<!--

function wopen(url, name, w, h)
{
// Fudge factors for window decoration space.
 // In my tests these work well on all platforms & browsers.
w+=20;//w += 32;
h+=60;//h += 96;
 var win = window.open(url,
  name,
  'width=' + w + ', height=' + h + ', ' +
  'location=no, menubar=no, ' +
  'status=no, toolbar=no, scrollbars=no, resizable=no');
 win.resizeTo(w, h);
 win.focus();
}
// -->
</script>

<TABLE WIDTH=100% CELLPADDING=5><TR><TD VALIGN=TOP><TABLE BORDER=0 CELLPADDING=5><TR><TD VALIGN=top><CENTER>When Ready, <a href='/horseisle.php?USER=SilicaAndPina' target=popup onClick="wopen('/horseisle.php?USER=SilicaAndPina', 'popup', 790, 522); return false;">Enter the World<BR><BR><IMG BORDER=0 SRC=/web/screenshots/enterhorseisle.png></A><BR><BR>(<a href='/horseisle.php?USER=SilicaAndPina' target=popup onClick="wopen('/horseisle.php?USER=SilicaAndPina', 'popup', 846, 542); return false;">bigger borders version</A>)<BR>(<A HREF=horseisle.php?USER=SilicaAndPina>same window version</A>)</TD><TD VALIGN=top>Welcome back <B>SilicaAndPina</B>, Here is your account info and Horse Isle server status: (<A HREF=?>refresh</A>)<BR><BR>It has been: 0.2 hours since you were last online. You have logged in 2 times.<BR>You have <B><FONT COLOR=005500>$5,910</FONT></B> in Horse Isle money on hand and <B><FONT COLOR=005500>$0</FONT></B> in the bank.<BR>You have earned <B>25</B> of <B>63005</B> total quest points  (<B>0%</B> Complete)<BR></TD></TR></TABLE><BR><HR>



<CENTER><TABLE WIDTH=500><TR><TD class=forumlist>

<FONT SIZE=+1>SILICAANDPINA'S PINTO SUBSCRIPTION STATUS:<BR></FONT><FONT SIZE=+2><FONT COLOR=GREEN>ACTIVE</FONT></FONT><BR>(31 days remain in your subscription)</FONT> (<A HREF=web/reasonstosubscribe.php>Subscription Benefits</A>)
</TD></TR><TR><TD class=forumlist>
<TABLE WIDTH=100%>
<TR><TD><B>BUY 1 Month Membership <FONT COLOR=GREEN>$5.00</FONT>usd</B> <I><FONT SIZE=-1>(adds 31 days membership time to the account that you are currently logged in with.) Non-refundable.</FONT></I></TD><TD>
<form action="https://www.paypal.com/cgi-bin/webscr" method="post">
<input type="hidden" name="cmd" value="_xclick">
<input type="hidden" name="business" value="paypal@horseisle.com">
<input type="hidden" name="undefined_quantity" value="1">
<input type="hidden" name="item_name" value="One Month Horse Isle Membership-on pinto.horseisle.com">
<input type="hidden" name="item_number" value="588112">
<input type="hidden" name="custom" value="588112">
<input type="hidden" name="amount" value="5.00">
<input type="hidden" name="no_shipping" value="1">
<input type="hidden" name="return" value="http://pinto.horseisle.com/web/paypalpayment.php">
<input type="hidden" name="notify_url" value="http://pinto.horseisle.com/web/paypalgateway.php">
<input type="hidden" name="no_note" value="1">
<input type="hidden" name="currency_code" value="USD">
<input type="hidden" name="lc" value="US">
<input type="hidden" name="bn" value="PP-BuyNowBF">
<input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but02.gif" border="0"
 name="submit" alt="Make payments with PayPal - it's fast, free and secure!">
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
</TD></TR></TABLE>

</TD></TR>

<TR><TD class=forumlist>
<TABLE WIDTH=100%><TR>
<TD><B>BUY Full Year Membership <FONT COLOR=GREEN>$40.00</FONT>usd</B> <I><FONT SIZE=-1>(adds 366 days membership time to the account you are logged in with. saves $20.00 off monthly subscription) Non-refundable.</FONT></I></TD><TD>
<form action="https://www.paypal.com/cgi-bin/webscr" method="post">
<input type="hidden" name="cmd" value="_xclick">
<input type="hidden" name="business" value="paypal@horseisle.com">
<input type="hidden" name="undefined_quantity" value="1">
<input type="hidden" name="item_name" value="Full Year Horse Isle Membership-on pinto.horseisle.com">
<input type="hidden" name="item_number" value="588112">
<input type="hidden" name="custom" value="588112">
<input type="hidden" name="amount" value="40.00">
<input type="hidden" name="no_shipping" value="1">
<input type="hidden" name="return" value="http://pinto.horseisle.com/web/paypalpayment.php">
<input type="hidden" name="notify_url" value="http://pinto.horseisle.com/web/paypalgateway.php">
<input type="hidden" name="no_note" value="1">
<input type="hidden" name="currency_code" value="USD">
<input type="hidden" name="lc" value="US">
<input type="hidden" name="bn" value="PP-BuyNowBF">
<input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but02.gif" border="0"
 name="submit" alt="Make payments with PayPal - it's fast, free and secure!">
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
</TD></TR></TABLE>


<!--
<TR><TD class=forumlist>

<TABLE WIDTH=100%><TR>
<TD><B>BUY 100k Horse Isle Currency <FONT COLOR=GREEN>$1.00</FONT>usd</B> <I><FONT SIZE=-1>(each one you buy gives your account $10,000 Horse Isle currency for use in the game.) Non-refundable.</FONT></I></TD><TD>
<form action="https://www.paypal.com/cgi-bin/webscr" method="post">
<input type="hidden" name="cmd" value="_xclick">
<input type="hidden" name="business" value="paypal@horseisle.com">
<input type="hidden" name="undefined_quantity" value="1">
<input type="hidden" name="item_name" value="100k Horse Isle Money-on pinto.horseisle.com">
<input type="hidden" name="item_number" value="588112">
<input type="hidden" name="custom" value="588112">
<input type="hidden" name="amount" value="1.00">
<input type="hidden" name="no_shipping" value="1">
<input type="hidden" name="return" value="http://pinto.horseisle.com/web/paypalpayment.php">
<input type="hidden" name="notify_url" value="http://pinto.horseisle.com/web/paypalgateway.php">
<input type="hidden" name="no_note" value="1">
<input type="hidden" name="currency_code" value="USD">
<input type="hidden" name="lc" value="US">
<input type="hidden" name="bn" value="PP-BuyNowBF">
<input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but02.gif" border="0"
 name="submit" alt="Make payments with PayPal - it's fast, free and secure!">
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
</TD></TR></TABLE>

</TD><TR>-->

<TR><TD class=forumlist>



<TABLE WIDTH=100%><TR>
<form action="https://www.paypal.com/cgi-bin/webscr" method="post">
<TD><B>BUY $100,000 Horse Isle Currency per <FONT COLOR=GREEN>$1.00</FONT>usd</B><BR>
Select: <SELECT NAME=quantity>
<!-<OPTION VALUE=1>$10,000 Horse Isle for $1.00 USD->
<OPTION VALUE=2>$200,000 Horse Isle for $2.00 USD
<OPTION VALUE=3>$300,000 Horse Isle for $3.00 USD
<OPTION VALUE=4>$400,000 Horse Isle for $4.00 USD
<OPTION VALUE=5>$550,000 Horse Isle for $5.00 USD (10% bonus)
<OPTION SELECTED VALUE=10>$1,100,000 Horse Isle for $10.00 USD (10% bonus)
<OPTION VALUE=20>$2,300,000 Horse Isle for $20.00 USD (15% bonus)
<OPTION VALUE=50>$5,750,000 Horse Isle for $50.00 USD (15% bonus)
<OPTION VALUE=100>$12,000,000 Horse Isle for $100.00 USD (20% bonus)
<OPTION VALUE=250>$31,250,000 Horse Isle for $250.00 USD (25% bonus)
</SELECT><BR>
 <I><FONT SIZE=-1>(Gives your account Horse Isle currency for use in the game.  You can earn Horse Isle money by playing the game.  This is not required.) Non-refundable.</FONT></I></TD><TD>
<input type="hidden" name="cmd" value="_xclick">
<input type="hidden" name="business" value="paypal@horseisle.com">
<input type="hidden" name="item_name" value="100k Horse Isle Money-on pinto.horseisle.com">
<input type="hidden" name="item_number" value="588112">
<input type="hidden" name="custom" value="588112">
<input type="hidden" name="amount" value="1.00">
<input type="hidden" name="no_shipping" value="1">
<input type="hidden" name="return" value="http://pinto.horseisle.com/web/paypalpayment.php">
<input type="hidden" name="notify_url" value="http://pinto.horseisle.com/web/paypalgateway.php">
<input type="hidden" name="no_note" value="1">
<input type="hidden" name="currency_code" value="USD">
<input type="hidden" name="lc" value="US">
<input type="hidden" name="bn" value="PP-BuyNowBF">
<input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but02.gif" border="0"
 name="submit" alt="Make payments with PayPal - it's fast, free and secure!">
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
</TD></TR></TABLE>

</TD></TR><TR><TD class=forumlist>
<TABLE WIDTH=100%>
<TR><TD>
<B>BUY Pawneer Order <FONT COLOR=GREEN>$8.00</FONT>usd</B> <I><FONT SIZE=-1>(allows you to order a custom breed/color/gender horse on server from Pawneer. This is not required, you can trade other players to get the breed you desire also.) Non-refundable.</FONT></I></TD><TD>
<form action="https://www.paypal.com/cgi-bin/webscr" method="post">
<input type="hidden" name="cmd" value="_xclick">
<input type="hidden" name="business" value="paypal@horseisle.com">
<input type="hidden" name="undefined_quantity" value="1">
<input type="hidden" name="item_name" value="Pawneer Order-on pinto.horseisle.com">
<input type="hidden" name="item_number" value="588112">
<input type="hidden" name="custom" value="588112">
<input type="hidden" name="amount" value="8.00">
<input type="hidden" name="no_shipping" value="1">
<input type="hidden" name="return" value="http://pinto.horseisle.com/web/paypalpayment.php">
<input type="hidden" name="notify_url" value="http://pinto.horseisle.com/web/paypalgateway.php">
<input type="hidden" name="no_note" value="1">
<input type="hidden" name="currency_code" value="USD">
<input type="hidden" name="lc" value="US">
<input type="hidden" name="bn" value="PP-BuyNowBF">
<input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but02.gif" border="0"
 name="submit" alt="Make payments with PayPal - it's fast, free and secure!">
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
</TD></TR></TABLE>

</TD></TR><TR><TD class=forumlist>
<TABLE WIDTH=100%>
<TR><TD>
<B>BUY 5 Pawneer Orders <FONT COLOR=GREEN>$30.00</FONT>usd</B> <I><FONT SIZE=-1>(save $10.00 - allows you to order 5 custom horses from Pawneer) Non-refundable.</FONT></I></TD><TD>
<form action="https://www.paypal.com/cgi-bin/webscr" method="post">
<input type="hidden" name="cmd" value="_xclick">
<input type="hidden" name="business" value="paypal@horseisle.com">
<input type="hidden" name="undefined_quantity" value="1">
<input type="hidden" name="item_name" value="Five Pawneer Order-on pinto.horseisle.com">
<input type="hidden" name="item_number" value="588112">
<input type="hidden" name="custom" value="588112">
<input type="hidden" name="amount" value="30.00">
<input type="hidden" name="no_shipping" value="1">
<input type="hidden" name="return" value="http://pinto.horseisle.com/web/paypalpayment.php">
<input type="hidden" name="notify_url" value="http://pinto.horseisle.com/web/paypalgateway.php">
<input type="hidden" name="no_note" value="1">
<input type="hidden" name="currency_code" value="USD">
<input type="hidden" name="lc" value="US">
<input type="hidden" name="bn" value="PP-BuyNowBF">
<input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but02.gif" border="0"
 name="submit" alt="Make payments with PayPal - it's fast, free and secure!">
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
</TD></TR></TABLE>


</TD></TR>



<TR><TD BGCOLOR=WHITE><CENTER>If you happen to have any:<B> <A HREF=/web/spendhorsebucks.php>Redeem Horse Bucks</A></TD></TR>

<TR><TD class=forumlist>
<BR>Alternative Payment Methods: <A HREF=/web/checks.php>Check/Cash via postal mail</A>
<BR><BR>Gift Payments: <A HREF=//hi1.horseisle.com/web/giftmembership.php>Pay for a different player</A>
<BR><BR></TD></TR>



</TD></TR></TABLE></CENTER>



<HR>

</TD><TD VALIGN=top><DIV ID="PLAYERS"><BR></DIV></TD></TR></TABLE><?php include("web/footer.php"); ?>

