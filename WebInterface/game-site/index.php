<?php 
include("config.php");

// Handle logout
if(isset($_GET["LOGOUT"]))
{
	if($_GET["LOGOUT"] == 1)
	{
		session_destroy();
	}
}

include("web/header.php"); 


?>

<CENTER>
<FONT FACE=Verdana,arial SIZE=-1>
<BR>
If you have an account on this server (<?php echo(strtoupper($_SERVER['HTTP_HOST']))?>) please login in at upper right.<BR>
 Otherwise click for <A href=http:<?php echo($master_site); ?>>Main Horse Isle 1 Site</A>.
<BR><BR>


<?php include("web/footer.php"); ?>