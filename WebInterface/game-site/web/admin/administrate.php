<?php
include("../config.php");
include("common.php");

session_start(['cookie_lifetime' => 86400]);
include("../header.php");
?>

<CENTER>
<FONT FACE=Verdana,arial SIZE=-1>
<?php
	if($_SESSION["A_LOGGED_IN"] !== "YES") 
	{
		header("Location: /web/admin"); # Fuck off.
		exit();
	}
	
	if(isset($_POST["TYPE"]))
	{
		if($_POST["TYPE"] == "CHANGEPERMS")
		{
			$connect = mysqli_connect($dbhost, $dbuser, $dbpass,$dbname) or die("Unable to connect to '$dbhost'");
			$stmt = $connect->prepare("SELECT id FROM Users WHERE username=?");
			$stmt->bind_param("s", $_POST["USERNAME"]);
			$stmt->execute();
			$result = $stmt->get_result();
			$user_id = intval($result->fetch_row()[0]);
			
			if(isset($_POST["RESETPASS1"], $_POST["RESETPASS2"]))
			{
				$pass1 = $_POST["RESETPASS1"];
				$pass2 = $_POST["RESETPASS2"];
				
				if($pass1 == $pass2)
				{
					if($pass1 !== "" || $pass1 !== null)
					{
						$password_hash = hash_salt($pass1,$salt);
						$stmt = $connect->prepare("UPDATE Users SET Password=? WHERE Id=?");
						$stmt->bind_param("s",$password_hash, "i", $user_id);
						$stmt->execute();

					}
				}
			}
			
			if(isset($_POST["ADMIN"]))
			{
				$stmt = $connect->prepare("UPDATE Users SET Admin=\"YES\" WHERE Id=?");
				$stmt->bind_param("i", $user_id);
				$stmt->execute();
			}
			else
			{
				$stmt = $connect->prepare("UPDATE Users SET Admin=\"NO\" WHERE Id=?");
				$stmt->bind_param("i", $user_id);
				$stmt->execute();
			}
			if(isset($_POST["MOD"]))
			{
				$stmt = $connect->prepare("UPDATE Users SET Moderator=\"YES\" WHERE Id=?");
				$stmt->bind_param("i", $user_id);
				$stmt->execute();
			}
			else
			{
				$stmt = $connect->prepare("UPDATE Users SET Moderator=\"NO\" WHERE Id=?");
				$stmt->bind_param("i", $user_id);
				$stmt->execute();
			}
			echo("<BR><B>Permissions updated successfully.</B></BR>");
			echo("<A HREF=/web/admin/administrate.php>Go back</A>");
			include("../footer.php");
			exit();
		}
	}
?>
<BR><B>HISP - Admin Portal</B><BR>
<BR>Player Operations</BR>
<BR> <FORM METHOD=POST ACTION=/web/admin/administrate.php>
	Username:
	<INPUT TYPE=HIDDEN NAME=TYPE VALUE=CHANGEPERMS>
	<INPUT TYPE=TEXT SIZE=30 NAME=USERNAME></INPUT><BR>
	<INPUT TYPE=CHECKBOX NAME=ADMIN VALUE="ADMIN"> Administrator</INPUT>
	<INPUT TYPE=CHECKBOX NAME=MOD VALUE="MOD"> Moderator</INPUT>
	<BR>
	<P>Reset Password</P>
	<INPUT TYPE=TEXT NAME=RESETPASS1 VALUE="" PASSWORD></INPUT>
	<P>Reset Password(confirm)</P>
	<INPUT TYPE=TEXT NAME=RESETPASS2 VALUE="" PASSWORD></INPUT>
	<!-- <INPUT TYPE=CHECKBOX NAME=DELETE VALUE="DELETE"> Delete Account</INPUT><BR> !-->
	<INPUT TYPE=SUBMIT VALUE="Apply"</INPUT>
	</FORM>
</BR>
<BR>
		<A HREF=/web/admin>Logout from admin portal</A><BR>
</BR>


<?php include("../footer.php"); ?>