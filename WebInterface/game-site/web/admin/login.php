<?php
include("../config.php");

session_start(['cookie_lifetime' => 86400]);
include("../header.php");
?>

<CENTER>
<FONT FACE=Verdana,arial SIZE=-1>
<BR><B>HISP - Super Admin Login</B><BR>
<?php
	if(isset($_POST["PASS"]))
	{
		sleep(3); // Stop bruteforce
		if($_POST["PASS"] == $admin_portal_password)
		{
			if($admin_portal_password == "!!NOTSET!!")
			{
				echo("Refusing to login as password is default password.");
				exit;
			}

			$_SESSION["A_LOGGED_IN"] = "YES";
			header("Location: /web/admin/administrate.php");
		}
		else
		{
			echo("<BR> The password you entered was NOT correct. </BR>");
			echo("<A HREF=\"/web/admin\">Try Again...</A>");
		}
	}
	else
	{
		echo("<BR> You didnt enter a password. </BR>");
		echo("<A HREF=\"/web/admin\">Try Again...</A>");
	}
?>


<?php include("../footer.php"); ?>