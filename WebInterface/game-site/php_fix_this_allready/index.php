<?php
# Decide which version to use
if($_SERVER['HTTP_USER_AGENT'] == "Shockwave Flash") # Projector
{
	$file = file_get_contents("horseisle_projector.swf");
	header("Content-Type: application/x-shockwave-flash");
	header("Content-Length: ".sizeof($file));
	echo($file);
}
else
{
	$file = file_get_contents("horseisle_patched.swf");
	header("Content-Type: application/x-shockwave-flash");
	header("Content-Length: ".sizeof($file));
	echo($file);
}

?>