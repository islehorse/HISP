<?php

session_start();
include("../config.php");
include("crosserver.php");
include("common.php");

if(!is_logged_in()){
	include("header.php");
	echo("Login First.");
	include("footer.php");
	exit();
}

$money = getUserMoney($dbname, $_SESSION['PLAYER_ID']);

if(isset($_GET["go"], $_GET["qnt"], $_GET["itm"], $_GET['to'], $_GET["ret"], $_GET['sign']))
{
	
	$targetUser = $_GET['to'];
	$subbed = getUserSubbed($dbname, $targetUser);
	$subbedUntil = getUserSubTimeRemaining($dbname, $targetUser);
	$moneyTarget = getUserMoney($dbname, $targetUser);

	if(!$subbed)
		$subbedUntil = time();


	if($_GET["go"] == 1)
	{
		$msg = $_GET['itm'].$_GET["qnt"].$_GET["to"].$_GET["ret"].$_SESSION['USERNAME'].$_SESSION['PLAYER_ID'];
		$expectedSignature = GenHmacMessage($msg, "PPEMU");
		$gotHmacSignature = $_GET['sign'];
		
		if(!hash_equals($gotHmacSignature,$expectedSignature)){
			include("header.php");
			echo("Invalid Signature. Are you trying to scam people?");
			include("footer.php");
			exit();
		}
		
		$itm = $_GET["itm"];		
		if(strpos($itm, "One Month Horse Isle Membership") === 0){
			$amount = 5; // NO CHEATING!
			$cost = $amount*$EXHANGE_RATE;
			if($money >= $cost)
			{
				setUserMoney($dbname, $_SESSION['PLAYER_ID'], $money-$cost);
				setUserSubbed($dbname,$targetUser, true);
				setUserSubbedUntil($dbname, $targetUser, $subbedUntil + 2678400);
				
				header("Location: ".$_GET["ret"]);
			}
			else
			{
				include("header.php");
				echo("Not enough money.");
				include("footer.php");
				exit();
			}

		}
		else if(strpos($itm, "Full Year Horse Isle Membership") === 0){
			$amount = 40; // NO CHEATING!
			$cost = $amount*$EXHANGE_RATE;
			if($money >= $cost)
			{
				setUserMoney($dbname, $_SESSION['PLAYER_ID'], $money-$cost);
				setUserSubbed($dbname, $targetUser, true);
				setUserSubbedUntil($dbname, $targetUser, $subbedUntil + 31622400);
				
				header("Location: ".$_GET["ret"]);
				
			}
			else
			{
				include("header.php");
				echo("Not enough money.");
				include("footer.php");
				exit();
			}

		
		}
		else if(strpos($itm, "100k Horse Isle Money") === 0){ // Why thou?
			$amount = 1; // NO CHEATING!
			$quantity = intval($_GET["qnt"]);
			$cost = ($amount*$EXHANGE_RATE)*$quantity;
			if($money >= $cost)
			{
				$amountGained = (100000 * $quantity);
				if($quantity == 5)
					$amountGained = 550000;
				if($quantity == 10)
					$amountGained = 1100000;
				if($quantity == 10)
					$amountGained = 1100000;
				if($quantity == 20)
					$amountGained = 2300000;
				if($quantity == 50)
					$amountGained = 5750000;
				if($quantity == 100)
					$amountGained = 12000000;
				if($quantity == 250)
					$amountGained = 31250000;

				setUserMoney($dbname, $_SESSION['PLAYER_ID'], $money-$cost);
				$money-=$cost;
				setUserMoney($dbname, $targetUser, $moneyTarget+=$amountGained);					
				header("Location: ".$_GET["ret"]);
				
			}
			else
			{
				include("header.php");
				echo("Not enough money.");
				include("footer.php");
				exit();
			}

		
		}
		else if(strpos($itm, "Pawneer Order") === 0){
			$amount = 8; // NO CHEATING!
			$cost = $amount*$EXHANGE_RATE;
			if($money >= $cost)
			{
				setUserMoney($dbname, $_SESSION['PLAYER_ID'], $money-$cost);
				addItemToPuchaseQueue($dbname, $targetUser, 559, 1);
				
				header("Location: ".$_GET["ret"]);
				
			}
			else
			{
				include("header.php");
				echo("Not enough money.");
				include("footer.php");
				exit();
			}

		
		}
		else if(strpos($itm, "Five Pawneer Order") === 0){
			$amount = 30; // NO CHEATING!
			$cost = $amount*$EXHANGE_RATE;
			if($money >= $cost)
			{
				setUserMoney($dbname, $_SESSION['PLAYER_ID'], $money-$cost);
				addItemToPuchaseQueue($dbname, $targetUser, 559, 5);
				
				header("Location: ".$_GET["ret"]);
				
			}
			else
			{
				include("header.php");
				echo("Not enough money.");
				include("footer.php");
				exit();
			}

		
		}

		exit();
	}
}

$quantity = 1;
if(!isset($_POST['item_name'], $_POST['amount'],  $_POST['item_number'], $_POST['custom'], $_POST['return']))
{
	
	include("header.php");
	echo("Some data was invalid");
	include("footer.php");
	exit();
}

if(isset($_POST['quantity']))
	$quantity = intval($_POST['quantity']);


$hasIntl = function_exists('numfmt_create');

if($hasIntl)
	$fmt = numfmt_create( 'en_US', NumberFormatter::DECIMAL );

$toUser = $_POST['custom'];
$toUsername = "";
if(!getUserExistInExt($dbname, $toUser))
{
	include("header.php");
	echo("Cannot buy for a user who does not exist on this server.");
	include("footer.php");
	exit();
}
else{
	$toUsername = get_username($toUser);
}

include("header.php");
?>
<h1>HISP - PayPal Emulator</h1>
<b>Purchase Information:</b>
<table>
  <tr>
    <th>Item</th>
    <th>Quantity</th>
    <th>Item number</th>
    <th>Price (USD)</th>
    <th>Price (HorseIsle)</th>
  </tr>
  <tr>
    <td><?php echo(htmlspecialchars($_POST['item_name'])) ?></td>
    <td><?php echo(htmlspecialchars((string)$quantity)); ?></td>
    <td><?php echo(htmlspecialchars($_POST['item_number'])) ?></td>
	<td><?php
			if($hasIntl)					
				$cost = numfmt_format($fmt, intval(htmlspecialchars($_POST['amount']*$quantity)));
			else
				$cost = $_POST['amount']*$quantity;


			echo('$'.$cost);
		?></td>
	<td><?php 
			if($hasIntl)					
				$cost = numfmt_format($fmt, intval(htmlspecialchars((($_POST['amount']) * $EXHANGE_RATE)*$quantity)));
			else
				$cost = (($_POST['amount']) * $EXHANGE_RATE)*$quantity;


			echo('$'.$cost);
		?></td>
  </tr>
</table>
<h3><b>NOTE: $1USD = $<?php echo($EXHANGE_RATE)?> HorseIsle Money! (you have $<?php echo($money) ?>)</b></h3><br><b>This purchase is for User: <?php echo(htmlspecialchars($toUser)." (".$toUsername.")"); ?></b></br>Do you want to purchase?</br><br><a href="?go=1&itm=<?php echo(urlencode(htmlspecialchars($_POST['item_name']))); ?>&qnt=<?php echo(urlencode(htmlspecialchars($quantity)));?>&to=<?php echo(urlencode(htmlspecialchars($_POST['custom']))); ?>&ret=<?php echo(urlencode(htmlspecialchars($_POST['return']))); ?>&sign=<?php 
	$msg = htmlspecialchars($_POST['item_name']).htmlspecialchars($quantity).htmlspecialchars($_POST['custom']).htmlspecialchars($_POST['return']).$_SESSION['USERNAME'].$_SESSION['PLAYER_ID'];
	echo(urlencode(GenHmacMessage($msg, "PPEMU"))); 
?>">Yes</a> | <a href="/account.php">No</a> 
<?php
include("footer.php");
?>