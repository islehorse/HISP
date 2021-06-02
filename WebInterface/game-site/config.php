<?php
$server_ip = '127.0.0.1';
$server_port = 12321;

$dbname = 'beta';
$dbuser = 'root';
$dbpass = 'test123';
$dbhost = '127.0.0.1';

# == hmac_secret ==
# Used for master-site to communicate with game-sites,
# Should be set to the same value on all game sites and the master site.
# NOTE: if someone knows this secret they can login to ANYONES account
# Ideally, this would be a random string of numbers, letters and symbols like 20 characters long T-T
$hmac_secret = "!!NOTSET!!";
$master_site = "http://server.islehorse.com";
# Password for /web/admin
$admin_portal_password = "!!NOTSET!!";
?>
