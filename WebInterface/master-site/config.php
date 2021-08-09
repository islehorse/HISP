<?php
$dbname = 'master';
$dbuser = 'root';
$dbpass = 'test123';
$dbhost = '127.0.0.1';

$pp_uri = '[GAMESITE]/web/ppemu.php'; # location of paypal emulator on game-servers
# [GAMESITE] is replaced with the URL for the game-site, as specified in servers.php
# original is https://www.paypal.com/cgi-bin/webscr which obviously wont do
# Dont set it to that though, as the paypalgateway.php is not implemented.

#should be same as all game-site's
$hmac_secret = '!!NOTSET!!';
?>
