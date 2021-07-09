<?php
session_start();
include('config.php');
include('crosserver.php');
include('common.php');

if(isset($_GET['SERVER']))
{
	$server_id = $_GET['SERVER'];
	$server = getServerById($server_id);
	
	if($server !== null)
	{
		if(is_logged_in())
		{
			$playerId = $_SESSION['PLAYER_ID'];
			if(!userid_exists($server['database'], $playerId))
			{				
				createAccountOnServer($server['database']);
				
				$hmac = hash_hmac('sha256', (string)$playerId, $hmac_secret."CrossSiteLogin".$_SERVER['REMOTE_ADDR'].date('m/d/Y'));
				$redirectUrl = $server['site'];
				
				if(!endsWith($redirectUrl, '/'))
					$redirectUrl .= '/';
				
				$redirectUrl .= 'account.php?SLID='.(string)$playerId.'&C='.base64_encode(hex2bin($hmac));
				set_LastOn($playerId, $server_id);
				
				header("Location: ".$redirectUrl);
				exit();
			}
			else
			{
				echo('[Account]Joining the Server Failed.  Please try a different server,  or Try re-logging into the website.  If you continue to have troubles, you may need to enable Cookies in your browser.  Another possibility ONLY if you already have an account is logging directly into the server via: '.$server['site'].'<BR>ERROR: Account is already setup on this server. / <HR><B>If you already have an account on server, try logging in direct: <A HREF=\''.$server['site'].'\'>'.$server['site'].'</A></B>');
			}
		}
		else
		{
			echo('[Account]Joining the Server Failed.  Please try a different server,  or Try re-logging into the website.  If you continue to have troubles, you may need to enable Cookies in your browser.  Another possibility ONLY if you already have an account is logging directly into the server via: '.$server['site'].'/<BR>ERROR: Account Setup Failed.  Please be sure you are logged in. / <HR><B>If you already have an account on server, try logging in direct: <A HREF=\''.$server['site'].'/\'>'.$server['site'].'</A></B>');
		}
	}
	else
	{
		echo('[]Joining the Server Failed.  Please try a different server,  or Try re-logging into the website.  If you continue to have troubles, you may need to enable Cookies in your browser.  Another possibility ONLY if you already have an account is logging directly into the server via: <BR>ERROR:  / The requested URL returned error: 404 Not Found<HR><B>If you already have an account on server, try logging in direct: </B>');
	}
}
?>