<?php
include('../dbconfig.php');
include('../common.php');

if(!isset($_POST['user'],$_POST['pass1'],$_POST['pass2'],$_POST['sex'],$_POST['email'],$_POST['age'],$_POST['passreqq'],$_POST['passreqa'],$_POST['cbr'], $_POST['cbt']))
{
	echo('
	<HEAD>
	<TITLE>HORSE ISLE - Online Multiplayer Horse Game</TITLE>
	<META NAME="keywords" CONTENT="Horse Game Online MMORPG Multiplayer Horses RPG Girls Girly Isle World Island Virtual Horseisle Sim Virtual">
	<META NAME="description" CONTENT="A multiplayer online horse world where players can capture, train, care for and compete their horses against other players. A very unique virtual sim horse game.">
	<link rel="shortcut icon" href="/favicon.ico" type="image/x-icon">
	<link rel="icon" href="/favicon.ico" type="image/x-icon">
	<link rel="meta" href="http://horseisle.com/labels.rdf" type="application/rdf+xml" title="ICRA labels" />
	<meta http-equiv="pics-Label" content=\'(pics-1.1 "http://www.icra.org/pics/vocabularyv03/" l gen true for "http://horseisle.com" r (n 0 s 0 v 0 l 0 oa 0 ob 0 oc 0 od 0 oe 0 of 0 og 0 oh 0 c 1)  gen true for "http://hi1.horseisle.com" r (n 0 s 0 v 0 l 0 oa 0 ob 0 oc 0 od 0 oe 0 of 0 og 0 oh 0 c 1))\' />
	<style type="text/css">
	hr {
	height: 1;
	color: #000000;
	background-color: #000000;
	border: 0;
	}
	a {
	font: bold 14px arial;
	color: #6E3278;
	}
	TH {
	background-color: #EDE5B4;
	padding: 1px 6px;
	border: 2px dotted #6E3278;
	font: small-caps 900 14px arial;
	color: #000000;
	}
	TR.a0 {
	background-color: #EDE5B4;
	}
	TR.a1 {
	background-color: #D4CCA1;
	}
	TD {
	font: 14px arial;
	color: #000000;
	}
	TD.forum {
	font: 12px arial;
	color: #000000;
	}
	TD.forumlist {
	padding: 1px 6px;
	border: 2px dotted #6E3278;
	background-color: #EDE5B4;
	text-align: center;
	font: bold 14px arial;
	color: #000000;
	}
	TD.forumpost {
	padding: 5px 10px;
	border: 2px dotted #6E3278;
	background-color: #EDE5B4;
	text-align: left;
	}
	TD.adminforumpost {
	padding: 5px 20px;
	border: 2px dotted #6E3278;
	background-color: #BFE9C9;
	text-align: left;
	}
	TD.newslist {
	padding: 4px 4px;
	border: 2px dotted #6E3278;
	background-color: #FFDDEE;
	text-align: left;
	font: 14px arial;
	color: #000000;
	}
	FORUMSUBJECT {
	font: bold 14px arial;
	color: #004400;
	}
	FORUMUSER {
	font: 12px arial;
	color: #000044;
	}
	FORUMDATE {
	font: 12px arial;
	color: #444444;
	}
	FORUMTEXT {
	font: 14px arial;
	color: #440000;
	}

	</style>
	</HEAD>
	<BODY BGCOLOR=E0D8AA>
	<TABLE BORDER=0 CELLPADDING=0 CELLSPACING=0 WIDTH=100%>
	<TR WIDTH=100%>
	<TD WIDTH=512 ROWSPAN=3><A HREF=/><IMG SRC=/web/hoilgui1.gif ALT="Welcome to Horse Isle" BORDER=0></A></TD>
	<TD WIDTH=100% BACKGROUND=/web/hoilgui2.gif>&nbsp;</TD>
	<TD WIDTH=29><IMG SRC=/web/hoilgui3.gif></TD>
	</TR>
	<TR>
	<TD WIDTH=100% BACKGROUND=/web/hoilgui4.gif align=right>
	<B>

	<TABLE CELLPADDING=0 CELLSPACING=2 BORDER=0><FORM METHOD=POST ACTION=/account.php>
	<TR><TD><B>USER:</B></TD><TD><INPUT TYPE=TEXT SIZE=14 NAME=USER></TD></TR>
	<TR><TD><B>PASS:</B></TD><TD><INPUT TYPE=PASSWORD SIZE=14 NAME=PASS></TD></TR>
	<TR><TD></TD><TD><INPUT TYPE=SUBMIT VALUE=LOGIN> (<A HREF=/web/forgotpass.php>Forgot?</A>)</TD></TR></FORM></TABLE>

	</TD>
	<TD WIDTH=29><IMG SRC=/web/hoilgui5.gif></TD></TR>
	<TR>
	<TD WIDTH=100% BACKGROUND=/web/hoilgui6.gif>&nbsp;</TD>
	<TD WIDTH=29><IMG SRC=/web/hoilgui7.gif></TD></TR>
	</TABLE>
	<CENTER>

	<CENTER><TABLE WIDTH=90% BORDER=0><TR><TD VALIGN=top>

	<FONT SIZE=+2><B>Horse Isle Create New Account:</B></FONT><BR>
	<I>Only one account per person.  Make sure you have parental permission if under 13!</I><BR>
	<BR>
	<FORM METHOD=POST>
	<B>GAME DETAILS (Take time selecting a good username, it will be your game name):</B><BR>
	<FONT COLOR=005500>In order to make the game prettier, please capitalize the first letter of each word in Username:<BR>
	<FONT SIZE=+1 COLOR=502070>Good:<FONT COLOR=GREEN>BlueBunny</FONT>  Not:<FONT COLOR=RED>BLUEBUNNY</FONT> or <FONT COLOR=RED>bluebunny</FONT> or <FONT COLOR=RED>BlUebuNNy</FONT></FONT><BR>
	If the username you choose is offensive in anyway, your account will be deleted as soon as it\'s noticed. 
	Please do not use any part of your real name in the username.  Pick something fun and original.  There are some ideas on right.<BR></FONT>
	Desired username: <INPUT TYPE=TEXT SIZE=16 MAX=16 VALUE="" NAME="user"><I><FONT SIZE-1>[3-16 letters only, capitalize first letter of each word ]</FONT></I><BR>
	Desired password: <INPUT TYPE=PASSWORD SIZE=16 MAX=16 VALUE="" NAME="pass1"><I><FONT SIZE-1>[6-16 both letters and numbers only, case insensitive]</FONT></I><BR>
	Repeat&nbsp; password: <INPUT TYPE=PASSWORD SIZE=16 MAX=16 VALUE="" NAME="pass2"><I><FONT SIZE-1>[ same as above ]</FONT></I><BR>

	GIRL: <INPUT TYPE=RADIO SIZE=30 NAME="sex" VALUE="FEMALE" CHECKED>
	 BOY: <INPUT TYPE=RADIO SIZE=30 NAME="sex" VALUE="MALE" > <I>[Determines whether you are referred to as \'him\' or \'her\' in game.]</I>
	<BR>


	<BR>
	<B>PERSONAL DETAILS (Kept private,  never shared):</B><BR>
	Your Valid Email: <INPUT TYPE=TEXT SIZE=40 NAME=email VALUE=\'\'><I><FONT SIZE-1>[ Login codes sent here ]</FONT></I><BR><FONT SIZE=-1 COLOR=880000>* many mail programs will mistakingly identify the Email as Spam, you may have to check your spam folders.  If the email code is not received within 2 days(50hrs), the account is removed, and you will then have to add it again.</FONT><BR>

	Your Age: <INPUT TYPE=TEXT SIZE=4 NAME="age" VALUE="">
	Your Country: <INPUT TYPE=TEXT SIZE=30 NAME="country" VALUE=""><BR>
	Password Recovery Question:<SELECT NAME=passreqq>
	<OPTION>Select a question
	<OPTION>My favorite food
	<OPTION>My pets name
	<OPTION>My best friends first name
	<OPTION>My favorite singer
	<OPTION>My favorite sports star
	<OPTION>My favorite team
	<OPTION>My favorite cartoon character
	<OPTION>My favorite actor
	</SELECT> Answer:<INPUT TYPE=TEXT SIZE=15 NAME=passreqa VALUE=\'\'><BR><BR>
	<B>LEGALITIES (Only Check if TRUE!):</B><BR>
	I have Read and Understand and will follow the <A HREF=rules.php>Rules</A>: <INPUT TYPE=CHECKBOX NAME="cbr" VALUE="OK" ><BR>
	I have Read and Understand the <A HREF=termsandconditions.php>Terms and Conditions</A>: <INPUT TYPE=CHECKBOX NAME="cbt" VALUE="OK" ><BR>
	<INPUT TYPE=HIDDEN NAME=A VALUE=><BR>
	<INPUT TYPE=SUBMIT VALUE=\'CREATE NEW ACCOUNT\'><BR>
	</FORM>
	<BR>
	<A HREF=/>Go Back to Main Page</A><BR><BR>

	</TD><TD>
	<TABLE BGCOLOR=FFEEEE BORDER=1 CELLPADDING=4><TR BGCOLOR=EEDDEE><TD COLSPAN=2><CENTER>
	<B>Some Random Available Names:</B><BR>(pick one or make up your own)<BR>
	</TD></TR><TR><TD><CENTER><FONT SIZE=-1>
	DesertWhisper<BR>GrapeHorsey<BR>CourageousEquine<BR>SunsetDream<BR>QueenDream<BR>RubyEagerCrow<BR>CoolDearBug<BR>CosmicWorker<BR>CoolTinkerer<BR>OverTheArt<BR>NonsenseTree<BR>CrystalPioneer<BR>BashfulMule<BR>MillionWater<BR>MissBee<BR>TalentAlly<BR>FinalFillyExmoor<BR>HandyRaspberry<BR>MeekIdiotNut<BR>FinalFiend<BR>TanVillagePrince<BR>NormalAdventure<BR>DynastyRider<BR>AbsoluteCreation<BR>FlyingPoet<BR>RightArabian<BR>StarryFriend<BR>VictoriousLight<BR>ClownStory<BR>SappyPinkBreeze<BR></FONT></TD><TD><FONT SIZE=-1><CENTER>ZeroWater<BR>PleasantGuard<BR>ZappySorcerous<BR>IllusionSnow<BR>LackingWizard<BR>DearMonkey<BR>FillyRose<BR>RightEquine<BR>LightEquine<BR>SimpleSilence<BR>EmeraldWonder<BR>FastGemstone<BR>YogurtFlower<BR>FabulousHorsey<BR>SappyMadamTalker<BR>WhitePal<BR>DarkSteed<BR>PiggyMonkey<BR>UberBlackHeart<BR>MeekFantasy<BR>SillyAmatuer<BR>HighlightDaisey<BR>DynastyWitch<BR>RicketyGem<BR>BlackLake<BR>JumpyCuteOrange<BR>LoveHorsey<BR>MythicalShimmer<BR>RightTeacher<BR>DrearyCat<BR></FONT></TD></TR></TABLE>
	</TD></TR></TABLE>

	<TABLE BORDER=0 CELLPADDING=0 CELLSPACING=0 WIDTH=100%>
	<TR>
	<TD><IMG SRC=/web/hoilgui10.gif></TD>
	<TD WIDTH=100% BACKGROUND=/web/hoilgui11.gif></TD>
	<TD><IMG SRC=/web/hoilgui12.gif></TD>
	</TR></TABLE>
	<CENTER><B>
	[ <A HREF=//master.horseisle.com/beginnerguide/>New Player Guide</A> ]<BR>
	[ <A HREF=/web/rules.php>Rules</A> ]
	[ <A HREF=/web/termsandconditions.php>Terms and Conditions</A> ]
	[ <A HREF=/web/privacypolicy.php>Privacy Policy</A> ]</B><BR>
	[ <A HREF=/web/expectedbehavior.php>Expected Behavior</A> ]
	[ <A HREF=/web/contactus.php>Contact Us</A> ] 
	[ <A HREF=/web/credits.php>Credits</A> ]<BR>
	<FONT FACE=Verdana,Arial SIZE=-2>Copyright &copy; 2020 Horse Isle</FONT>

	<!-- Google Analytics -->
	<script src="http://www.google-analytics.com/urchin.js" type="text/javascript">
	</script>
	<script type="text/javascript">
	_uacct = "UA-1805076-1";
	urchinTracker();
	</script>
	');
}
else
{

if($_POST['cbr'] !== "OK")
	die("Please accept the rules");
if($_POST['cbt'] !== "OK")
	die("Please accept the terms and conditions");

if($_POST['pass1'] !== $_POST['pass2'])
	die('Passwords do not match.');
$username = $_POST['user'];
$password = $_POST['pass1'];
$age = intval($_POST['age'],10);
$email = $_POST['email'];
$reset_question = $_POST['passreqq'];
$reset_answer = $_POST['passreqa'];
$country = $_POST['country'];
$gender = $_POST['sex'];

$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
$result = mysqli_query($connect, "SELECT COUNT(1) FROM users");

$user_id = $result->fetch_row()[0] + 1;
$salt = random_bytes ( 64 );
$answer_hash = hash_salt($reset_answer,$salt);
$password_hash = hash_salt($password,$salt);
$hex_salt = bin2hex($salt);

$stmt = $connect->prepare("SELECT COUNT(1) FROM users WHERE Username=?"); 
$stmt->bind_param("s", $username);
$stmt->execute();
$result = $stmt->get_result();
$count = intval($result->fetch_row()[0]);
if($count !== 0)
	die("Username is allready in use.");

$stmt = $connect->prepare("INSERT INTO users VALUES(?,?,?,?,?,?,?,?,?,?,'NO','NO')"); 
$stmt->bind_param("isssssisss", $user_id, $username, $email, $country, $reset_question, $answer_hash, $age, $password_hash, $hex_salt, $gender);
$stmt->execute();
echo('Account Created!');
}
?>