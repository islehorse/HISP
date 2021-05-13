<?php
include('../dbconfig.php');
include('../common.php');
include("header.php");
$atype = 2;
if(isset($_GET["A"]))
	$atype = $_GET["A"];
if($atype > 2 || $atype < 1)
	$atype = 2;

$problems = [];

if(isset($_POST['cbt'], $_POST['user'],$_POST['pass1'],$_POST['pass2'],$_POST['sex'],$_POST['email'],$_POST['age'],$_POST['passreqq'],$_POST['passreqa'],$_POST['cbr'] ,$_POST['A']))
{
	if($_POST['cbr'] !== "OK")
		array_push($problems, "You need to read the RULES and agree to follow them!");
	if($_POST['cbt'] !== "OK")
		array_push($problems, "You need to read the Terms and Conditions agree to be bound by them!");
	if($_POST['A'] == 1)
		if(isset($_POST["cbp"]))
		{
			if($_POST['cbp'] !== "OK")
				array_push($problems, "You need to have Parental Permission!");
		}
		else
		{
			array_push($problems, "You need to have Parental Permission!");
		}
		
	if($_POST['pass1'] !== $_POST['pass2'])
		array_push($problems, "Passwords must match!");
	
	$username = $_POST['user'];
	$password = $_POST['pass1'];
	$age = intval($_POST['age'],10);
	$email = $_POST['email'];
	$reset_question = $_POST['passreqq'];
	$reset_answer = $_POST['passreqa'];
	$country = $_POST['country'];
	$gender = $_POST['sex'];
	
	if(preg_match('/[^A-Za-z]/', $username))
		array_push($problems, "Username must contain ONLY Letters.");
	
	$username_len = strlen($username);
	if($username_len < 3)
		array_push($problems, "Username must be at least 3 characters long.");
	if($username_len > 16)
		array_push($problems, "Username must be less than 16 characters long.");
	
	if(preg_match('/[A-Z]{2,}/',$username))
		array_push($problems, "Username should be formatted with the first letter of each word capitalized. ( For example: BlueBunny )");
	
	if(strtoupper($username)[0] !== $username[0])
		array_push($problems, "Username should be formatted with the first letter of each word capitalized. ( For example: BlueBunny )");
	
	if(preg_match('/[^A-Za-z0-9]/',$password))
		array_push($problems, "Password must contain ONLY Letters and numbers.");
	$password_len = strlen($password);
	if($password_len < 6)
		array_push($problems, "Password must be at least 6 characters long.");

	if($password_len > 16)
		array_push($problems, "Password must be less than 16 characters long.");
	
	if(!preg_match('/[0-9]/',$password))
		array_push($problems, "Password must contain at least one number.");
	
	if(!preg_match('/[a-zA-Z]/',$password))
		array_push($problems, "Password must contain at least one letter.");
	
	if($reset_question == "Select a question")
		array_push($problems, "You must select a Password Recovery Question.");
	if($reset_answer == "")
		array_push($problems, "You must Answer the Password Recovery Question.");
		
	if($country == "")
		array_push($problems, "Please enter your country.");
	
	if($_POST['age'] == "")
		array_push($problems, "Please enter your age.");
		
	if($username == $password)
		array_push($problems, "Username and Password can not be the same!");
	
	if(str_contains($username, $password))
		array_push($problems, "The password cannot be within the username!.");
	
	if(str_contains($password, $username))
		array_push($problems, "The password cannot have the username within it!.");
	
	
	if(!preg_match('/^[A-Za-z0-9]*\@[A-Za-z0-9]*\.[A-Za-z0-9]{1,4}$/',$email))
		array_push($problems, "Email does not appear valid, you will not be able sign in without getting the login mail.");
	
	
	$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
	$result = mysqli_query($connect, "SELECT MAX(Id) FROM Users");

	$user_id = $result->fetch_row()[0] + 1;
	if($user_id == NULL)
		$user_id = 0;
		
	$salt = random_bytes ( 64 );
	$answer_hash = hash_salt($reset_answer,$salt);
	$password_hash = hash_salt($password,$salt);
	$hex_salt = bin2hex($salt);

	$stmt = $connect->prepare("SELECT COUNT(1) FROM Users WHERE Username=?"); 
	$stmt->bind_param("s", $username);
	$stmt->execute();
	$result = $stmt->get_result();
	$count = intval($result->fetch_row()[0]);
	if($count !== 0)
		array_push($problems, "Username taken. Please try a different account name.");


	if(count($problems) <= 0)
	{
		$stmt = $connect->prepare("INSERT INTO Users VALUES(?,?,?,?,?,?,?,?,?,?,'NO','NO')"); 
		$stmt->bind_param("isssssisss", $user_id, $username, $email, $country, $reset_question, $answer_hash, $age, $password_hash, $hex_salt, $gender);
		$stmt->execute();
		
		echo('<TABLE cellpadding=10><TR><TD><B>Your account has been added!</B><BR>Look for the email from support@horseisle.com with your activation code!<BR>You cannot play until you CLICK the link with your code in the email.<BR>  Be sure to check your Spam email box in case it goes there. If you do not get the email soon, feel free to log in with your username and password to re-send the Activation Code to the same or a different email address.<BR><BR><A HREF=/>Go Back to Main Page</A><BR><BR></TD></TR></TABLE>');
		include("footer.php");
		exit();
	}
}
?>
<CENTER><TABLE WIDTH=90% BORDER=0><TR><TD VALIGN=top>

<FONT SIZE=+2><B>Horse Isle Create New Account:</B></FONT><BR>
<I>Only one account per person.  Make sure you have parental permission if under 13!</I><BR>
<BR>
<FORM METHOD=POST>
<?php
if(count($problems) > 0)
{
	echo("<B>There were the following problems with your submission:<BR><FONT COLOR=RED>");
	for($i = 0; $i < count($problems); $i++)
	{
		echo($problems[$i]."<BR>");
	}
	echo("</FONT></B>");
}
?>
<B>GAME DETAILS (Take time selecting a good username, it will be your game name):</B><BR>
<FONT COLOR=005500>In order to make the game prettier, please capitalize the first letter of each word in Username:<BR>
<FONT SIZE=+1 COLOR=502070>Good:<FONT COLOR=GREEN>BlueBunny</FONT>  Not:<FONT COLOR=RED>BLUEBUNNY</FONT> or <FONT COLOR=RED>bluebunny</FONT> or <FONT COLOR=RED>BlUebuNNy</FONT></FONT><BR>
If the username you choose is offensive in anyway, your account will be deleted as soon as it's noticed. 
Please do not use any part of your real name in the username.  Pick something fun and original.  There are some ideas on right.<BR></FONT>
Desired username: <INPUT TYPE=TEXT SIZE=16 MAX=16 VALUE="<?php if(isset($_POST["user"])){echo(htmlspecialchars($_POST["user"],ENT_QUOTES));};?>" NAME="user"><I><FONT SIZE-1>[3-16 letters only, capitalize first letter of each word ]</FONT></I><BR>
Desired password: <INPUT TYPE=PASSWORD SIZE=16 MAX=16 VALUE="<?php if(isset($_POST["pass1"])){echo(htmlspecialchars($_POST["pass1"],ENT_QUOTES));};?>" NAME="pass1"><I><FONT SIZE-1>[6-16 both letters and numbers only, case insensitive]</FONT></I><BR>
Repeat&nbsp; password: <INPUT TYPE=PASSWORD SIZE=16 MAX=16 VALUE="<?php if(isset($_POST["pass2"])){echo(htmlspecialchars($_POST["pass2"],ENT_QUOTES));};?>" NAME="pass2"><I><FONT SIZE-1>[ same as above ]</FONT></I><BR>


GIRL: <INPUT TYPE=RADIO SIZE=30 NAME="sex" VALUE="FEMALE" <?php if(isset($_POST["sex"])){if($_POST["sex"] == "FEMALE"){echo("CHECKED");}}else{echo("CHECKED");}?>>
 BOY: <INPUT TYPE=RADIO SIZE=30 NAME="sex" VALUE="MALE" <?php if(isset($_POST["sex"])){if($_POST["sex"] == "MALE"){echo("CHECKED");}};?>> <I>[Determines whether you are referred to as 'him' or 'her' in game.]</I>
<BR>


<BR>
<B>PERSONAL DETAILS (Kept private,  never shared):</B><BR>
<?php
$email = "";
if(isset($_POST["email"])){
	$email = htmlspecialchars($_POST["email"],ENT_QUOTES);
};

if($atype == 2)
	echo("Your Valid Email: <INPUT TYPE=TEXT SIZE=40 NAME=email VALUE='".$email."'><I><FONT SIZE-1>[ Login codes sent here ]</FONT></I><BR><FONT SIZE=-1 COLOR=880000>* many mail programs will mistakingly identify the Email as Spam, you may have to check your spam folders.  If the email code is not received within 2 days(50hrs), the account is removed, and you will then have to add it again.</FONT><BR>");
else if($atype == 1)
	echo("Your <B>PARENT'S</B> Email: <INPUT TYPE=TEXT SIZE=40 NAME=email VALUE='".$email."'><I><FONT SIZE-1>[ Login codes sent here ]</FONT></I><BR><FONT SIZE=-1 COLOR=880000>* many mail programs will mistakingly identify the Email as Spam, you may have to check your spam folders.  If the email code is not received within 2 days(50hrs), the account is removed, and you will then have to add it again.</FONT><BR>");
?>
Your Age: <INPUT TYPE=TEXT SIZE=4 NAME="age" VALUE="<?php if(isset($_POST["age"])){echo(htmlspecialchars($_POST["age"],ENT_QUOTES));};?>">
Your Country: <INPUT TYPE=TEXT SIZE=30 NAME="country" VALUE="<?php if(isset($_POST["country"])){echo(htmlspecialchars($_POST["country"],ENT_QUOTES));};?>"><BR>
Password Recovery Question:<SELECT NAME=passreqq>
<OPTION><?php 
if(isset($_POST["passreqq"])){echo(htmlspecialchars($_POST["passreqq"],ENT_QUOTES));}else{echo("Select a question");}
?>
<OPTION>My favorite food
<OPTION>My pets name
<OPTION>My best friends first name
<OPTION>My favorite singer
<OPTION>My favorite sports star
<OPTION>My favorite team
<OPTION>My favorite cartoon character
<OPTION>My favorite actor
</SELECT> Answer:<INPUT TYPE=TEXT SIZE=15 NAME=passreqa VALUE='<?php if(isset($_POST["passreqa"])){echo(htmlspecialchars($_POST["passreqa"],ENT_QUOTES));};?>'><BR><BR>
<B>LEGALITIES (Only Check if TRUE!):</B><BR>
I have Read and Understand and will follow the <A HREF=rules.php>Rules</A>: <INPUT TYPE=CHECKBOX NAME="cbr" VALUE="OK" <?php if(isset($_POST["cbr"])){if($_POST["cbr"] == "OK"){echo("CHECKED");}};?>><BR>
I have Read and Understand the <A HREF=termsandconditions.php>Terms and Conditions</A>: <INPUT TYPE=CHECKBOX NAME="cbt" VALUE="OK" <?php if(isset($_POST["cbt"])){if($_POST["cbt"] == "OK"){echo("CHECKED");}};?>><BR>
<?php
echo('<INPUT TYPE=HIDDEN NAME=A VALUE='.$atype.'>');
if($atype == 1){
	$msg = "";
	if(isset($_POST["cbp"]))
		if($_POST["cbp"] == "OK")
			$msg = "CHECKED";
	echo('By clicking this I <B>PROMISE</B> I have parental permission: <INPUT TYPE=CHECKBOX NAME=cbp VALUE=OK '.$msg.'><BR>');
}
?>
<BR>
<INPUT TYPE=SUBMIT VALUE='CREATE NEW ACCOUNT'><BR>
</FORM>
<BR>
<A HREF=/>Go Back to Main Page</A><BR><BR>

</TD><TD>
<TABLE BGCOLOR=FFEEEE BORDER=1 CELLPADDING=4><TR BGCOLOR=EEDDEE><TD COLSPAN=2><CENTER>
<B>Some Random Available Names:</B><BR>(pick one or make up your own)<BR>
</TD></TR><TR><TD><CENTER><FONT SIZE=-1>
DesertWhisper<BR>GrapeHorsey<BR>CourageousEquine<BR>SunsetDream<BR>QueenDream<BR>RubyEagerCrow<BR>CoolDearBug<BR>CosmicWorker<BR>CoolTinkerer<BR>OverTheArt<BR>NonsenseTree<BR>CrystalPioneer<BR>BashfulMule<BR>MillionWater<BR>MissBee<BR>TalentAlly<BR>FinalFillyExmoor<BR>HandyRaspberry<BR>MeekIdiotNut<BR>FinalFiend<BR>TanVillagePrince<BR>NormalAdventure<BR>DynastyRider<BR>AbsoluteCreation<BR>FlyingPoet<BR>RightArabian<BR>StarryFriend<BR>VictoriousLight<BR>ClownStory<BR>SappyPinkBreeze<BR></FONT></TD><TD><FONT SIZE=-1><CENTER>ZeroWater<BR>PleasantGuard<BR>ZappySorcerous<BR>IllusionSnow<BR>LackingWizard<BR>DearMonkey<BR>FillyRose<BR>RightEquine<BR>LightEquine<BR>SimpleSilence<BR>EmeraldWonder<BR>FastGemstone<BR>YogurtFlower<BR>FabulousHorsey<BR>SappyMadamTalker<BR>WhitePal<BR>DarkSteed<BR>PiggyMonkey<BR>UberBlackHeart<BR>MeekFantasy<BR>SillyAmatuer<BR>HighlightDaisey<BR>DynastyWitch<BR>RicketyGem<BR>BlackLake<BR>JumpyCuteOrange<BR>LoveHorsey<BR>MythicalShimmer<BR>RightTeacher<BR>DrearyCat<BR></FONT></TD></TR></TABLE>
</TD></TR></TABLE>
<?php
include("footer.php");
?>
