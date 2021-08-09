<?php
	include("../config.php");
	include("../servers.php");
	include("../common.php");
	include("../crosserver.php");
	include("header.php");
	$host = 'http://'.htmlspecialchars($_SERVER['HTTP_HOST'], ENT_QUOTES);
	
	if(isset($_POST['PLAYERNAME'], $_POST['SERVER'])){
		$server = htmlspecialchars($_POST['SERVER']);
		$name = htmlspecialchars($_POST['PLAYERNAME']);
		$serverObj = getServerById($server);
		$serverDb = $serverObj['database'];
		
		echo('<CENTER><B>Gift membership payments options</B></CENTER><BR>');
		echo('<FONT COLOR=444444>Verifying existing playername and activity on server...<BR></FONT>');
		if(!user_exists($name)){
			echo('<FONT COLOR=RED>Player: '.$name.' not found on Horse Isle. Please make sure you know the EXACT playername!  Press BACK button.</FONT>');
		}
		else
		{
			$userid = htmlspecialchars(get_userid($name));
			$name = htmlspecialchars(get_username($userid));
			
			echo('</FONT>Player: '.$name.' Found.<BR>');
			echo('Player\'s account ID: '.$userid.'<BR>');
			
			echo('<FONT COLOR=444444>Checking for an active account on server...<BR></FONT>');
			if($serverObj == null)
			{
				echo('<FONT COLOR=RED>Server not found?</FONT>');
				exit();
			}
			if(!userid_exists($serverDb, $userid))
			{
				echo('<FONT COLOR=RED>Player: '.$name.' does not have an active account on Server '.$server.' Please make sure you know the EXACT server they play on!  Press BACK button.</FONT>');
			}
			else
			{
				echo('It appears they have an account on '.$server.'.<BR>');
				
				$newUser = !getUserExistInExt($serverDb, $userid);
		
				if(!$newUser){
					$totalLogins = getUserTotalLogins($serverDb, $userid);
					$subbedUntil = getUserSubTimeRemaining($serverDb, $userid);
				}
				else
				{
					$totalLogins = 0;
					$subbedUntil = 0;
				}
		
				echo('They have logged into it '.htmlspecialchars($totalLogins).' times.<BR>');
				if($subbedUntil <= 0)
				{
					echo('They have never been subscribed to this server.<BR>');
				}
				else
				{
					echo('They are/were subscribed to this server until: '.date("F j, Y", $subbedUntil).'<BR>');
				}
				
				// put payment options here;
				$pp_uri = str_replace('[GAMESITE]', $serverObj['site'], $pp_uri);
				$gameServerDomain = parse_url($serverObj['site'], PHP_URL_HOST);
				echo('<HR>The following Payment Options are Available:<BR>
<CENTER><TABLE WIDTH=500><TR><TD class=forumlist>

</TD></TR><TR><TD class=forumlist>
<TABLE WIDTH=100%>
<TR><TD><B>BUY 1 Month Membership <FONT COLOR=GREEN>$5.00</FONT>usd</B> <I><FONT SIZE=-1>(adds 31 days membership time to the account) Non-refundable.</FONT></I></TD><TD>
<form action="'.$pp_uri.'" method="post">
<input type="hidden" name="cmd" value="_xclick">
<input type="hidden" name="business" value="paypal@horseisle.com">
<input type="hidden" name="undefined_quantity" value="1">
<input type="hidden" name="item_name" value="One Month Horse Isle Membership-gift on '.$gameServerDomain.'">
<input type="hidden" name="item_number" value="'.htmlspecialchars($userid).'">
<input type="hidden" name="custom" value="'.htmlspecialchars($userid).'">
<input type="hidden" name="amount" value="5.00">
<input type="hidden" name="no_shipping" value="1">
<input type="hidden" name="return" value="'.$host.'/web/paypalgiftpayment.php">
<input type="hidden" name="notify_url" value="'.$serverObj['site'].'/web/paypalgateway.php">
<input type="hidden" name="no_note" value="1">
<input type="hidden" name="currency_code" value="USD">
<input type="hidden" name="lc" value="US">
<input type="hidden" name="bn" value="PP-BuyNowBF">
<input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but02.gif" border="0"
 name="submit" alt="Make payments with PayPal - it\'s fast, free and secure!">
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
</TD></TR></TABLE>

</TD></TR>

<TR><TD class=forumlist>
<TABLE WIDTH=100%><TR>
<TD><B>BUY Full Year Membership <FONT COLOR=GREEN>$40.00</FONT>usd</B> <I><FONT SIZE=-1>(adds 366 days membership time to the account. saves $20.00 off monthly subscription) Non-refundable.</FONT></I></TD><TD>
<form action="'.$pp_uri.'" method="post">
<input type="hidden" name="cmd" value="_xclick">
<input type="hidden" name="business" value="paypal@horseisle.com">
<input type="hidden" name="undefined_quantity" value="1">
<input type="hidden" name="item_name" value="Full Year Horse Isle Membership-gift on '.$gameServerDomain.'">
<input type="hidden" name="item_number" value="'.htmlspecialchars($userid).'">
<input type="hidden" name="custom" value="'.htmlspecialchars($userid).'">
<input type="hidden" name="amount" value="40.00">
<input type="hidden" name="no_shipping" value="1">
<input type="hidden" name="return" value="'.$host.'/web/paypalgiftpayment.php">
<input type="hidden" name="notify_url" value="'.$serverObj['site'].'/web/paypalgateway.php">
<input type="hidden" name="no_note" value="1">
<input type="hidden" name="currency_code" value="USD">
<input type="hidden" name="lc" value="US">
<input type="hidden" name="bn" value="PP-BuyNowBF">
<input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but02.gif" border="0"
 name="submit" alt="Make payments with PayPal - it\'s fast, free and secure!">
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
</TD></TR></TABLE>

<TR><TD class=forumlist>


<TABLE WIDTH=100%><TR>
<form action="'.$pp_uri.'" method="post">
<TD><B>BUY $10,000 Horse Isle Currency per <FONT COLOR=GREEN>$1.00</FONT>usd</B><BR>
Select: <SELECT NAME=quantity>
<OPTION VALUE=2>$20,000 Horse Isle for $2.00 USD
<OPTION VALUE=3>$30,000 Horse Isle for $3.00 USD
<OPTION VALUE=4>$40,000 Horse Isle for $4.00 USD
<OPTION VALUE=5>$55,000 Horse Isle for $5.00 USD (10% bonus)
<OPTION SELECTED VALUE=10>$110,000 Horse Isle for $10.00 USD (10% bonus)
<OPTION VALUE=20>$230,000 Horse Isle for $20.00 USD (15% bonus)
<OPTION VALUE=50>$575,000 Horse Isle for $50.00 USD (15% bonus)
<OPTION VALUE=100>$1,200,000 Horse Isle for $100.00 USD (20% bonus)
<OPTION VALUE=250>$3,125,000 Horse Isle for $250.00 USD (25% bonus)
</SELECT><BR>
 <I><FONT SIZE=-1>(gives Horse Isle currency for use in the game.  You can earn Horse Isle money by playing the game,  this is not required.) Non-refundable.</FONT></I></TD><TD>
<input type="hidden" name="cmd" value="_xclick">
<input type="hidden" name="business" value="paypal@horseisle.com">
<input type="hidden" name="item_name" value="10k Horse Isle Money-gift on '.$gameServerDomain.'">
<input type="hidden" name="item_number" value="'.htmlspecialchars($userid).'">
<input type="hidden" name="custom" value="'.htmlspecialchars($userid).'">
<input type="hidden" name="amount" value="1.00">
<input type="hidden" name="no_shipping" value="1">
<input type="hidden" name="return" value="'.$host.'/web/paypalgiftpayment.php">
<input type="hidden" name="notify_url" value="'.$serverObj['site'].'/web/paypalgateway.php">
<input type="hidden" name="no_note" value="1">
<input type="hidden" name="currency_code" value="USD">
<input type="hidden" name="lc" value="US">
<input type="hidden" name="bn" value="PP-BuyNowBF">
<input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but02.gif" border="0"
 name="submit" alt="Make payments with PayPal - it\'s fast, free and secure!">
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
</TD></TR></TABLE>

</TD></TR><TR><TD class=forumlist>
<TABLE WIDTH=100%>
<TR><TD>
<B>BUY Pawneer Order <FONT COLOR=GREEN>$8.00</FONT>usd</B> <I><FONT SIZE=-1>(allows ordering a custom breed/color/gender horse on server from Pawneer. This is not required, you can trade other players to get the breed you desire also.) Non-refundable.</FONT></I></TD><TD>
<form action="'.$pp_uri.'" method="post">
<input type="hidden" name="cmd" value="_xclick">
<input type="hidden" name="business" value="paypal@horseisle.com">
<input type="hidden" name="undefined_quantity" value="1">
<input type="hidden" name="item_name" value="Pawneer Order-gift on '.$gameServerDomain.'">
<input type="hidden" name="item_number" value="'.htmlspecialchars($userid).'">
<input type="hidden" name="custom" value="'.htmlspecialchars($userid).'">
<input type="hidden" name="amount" value="8.00">
<input type="hidden" name="no_shipping" value="1">
<input type="hidden" name="return" value="'.$host.'/web/paypalgiftpayment.php">
<input type="hidden" name="notify_url" value="'.$serverObj['site'].'/web/paypalgateway.php">
<input type="hidden" name="no_note" value="1">
<input type="hidden" name="currency_code" value="USD">
<input type="hidden" name="lc" value="US">
<input type="hidden" name="bn" value="PP-BuyNowBF">
<input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but02.gif" border="0"
 name="submit" alt="Make payments with PayPal - it\'s fast, free and secure!">
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
</TD></TR></TABLE>

</TD></TR><TR><TD class=forumlist>
<TABLE WIDTH=100%>
<TR><TD>
<B>BUY 5 Pawneer Orders <FONT COLOR=GREEN>$30.00</FONT>usd</B> <I><FONT SIZE=-1>(save $10.00 - allows ordering 5 custom horses from Pawneer) Non-refundable.</FONT></I></TD><TD>
<form action="'.$pp_uri.'" method="post">
<input type="hidden" name="cmd" value="_xclick">
<input type="hidden" name="business" value="paypal@horseisle.com">
<input type="hidden" name="undefined_quantity" value="1">
<input type="hidden" name="item_name" value="Five Pawneer Order-gift on '.$gameServerDomain.'">
<input type="hidden" name="item_number" value="'.htmlspecialchars($userid).'">
<input type="hidden" name="custom" value="'.htmlspecialchars($userid).'">
<input type="hidden" name="amount" value="30.00">
<input type="hidden" name="no_shipping" value="1">
<input type="hidden" name="return" value="'.$host.'/web/paypalgiftpayment.php">
<input type="hidden" name="notify_url" value="'.$serverObj['site'].'/web/paypalgateway.php">
<input type="hidden" name="no_note" value="1">
<input type="hidden" name="currency_code" value="USD">
<input type="hidden" name="lc" value="US">
<input type="hidden" name="bn" value="PP-BuyNowBF">
<input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but02.gif" border="0"
 name="submit" alt="Make payments with PayPal - it\'s fast, free and secure!">
<img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
</form>
</TD></TR></TABLE>
/web/checks.php>CASH PAYMENT OPTION</A>
//</TD>
//</TR>
?>
</TABLE></CENTER><BR>








');
				
			}
		}
		
		include("footer.php");
		exit();
	}
?>
<TABLE WIDTH=60%><TR><TD>
<CENTER><B>Gift membership payments.</B></CENTER><BR>
The following will enable you to buy a subscription or bonus for any existing account on Horse Isle. 
Please BE SURE you know the EXACT playername and server that they play on,  we cannot refund accidental payments on the wrong account.  No refunds. ONLY make a payment if over 18.
<BR><CENTER>Horse Isle Gift Purchase For:<BR><FORM METHOD=POST>PLAYER NAME:(<B><FONT COLOR=RED>Exact Game Name!</FONT></B>) <INPUT TYPE=INPUT NAME=PLAYERNAME><BR>ON SERVER: (<B><FONT COLOR=RED>Be Sure!</FONT></B>) <SElECT name=SERVER><?php
	for($i = 0; $i < count($server_list); $i++)
	{
		echo("<OPTION>".htmlspecialchars($server_list[$i]['id']));
	}?></SELECT><BR><BR><INPUT TYPE=SUBMIT VALUE='SHOW PURCHASE OPTIONS'></FORM><FONT SIZE=-1>NOTE: The player given the gift is in no way notified that they have been given the gift.  It is left up to you to notify them. They also have no way to access your payment info whatsoever.</FONT></TD></TR></TABLE><?php
	include("footer.php");
?>