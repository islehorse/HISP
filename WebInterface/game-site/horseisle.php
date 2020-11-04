<?php
	include("config.php");
?>
<html lang="en">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
<title>HORSEISLE - Secret Land of Horses</title>
<link rel="shortcut icon" href="/favicon.ico" type="image/x-icon">
<link rel="icon" href="/favicon.ico" type="image/x-icon">
<!-- Google Analytics -->
<script src="http://www.google-analytics.com/urchin.js" type="text/javascript">
</script>
<script type="text/javascript">
_uacct = "UA-1805076-1";
urchinTracker();
</script>

<script language="javascript"><!--
// Intercept Browser X and give user choice (on firefox)

//window.onbeforeunload= function (evt) {  return false; }

  var ConfirmClose = true;

  window.onbeforeunload = confirmExit;
  function confirmExit()
  {
    if (ConfirmClose)
      return "[ Please use QUIT GAME button to exit Horse Isle ]";
  }
  function allowExit()
  {
    ConfirmClose = false;
  }


///  Every half second, put focus on Flash window.
//function getfocus(){
//  horseisle.focus();
//  mytimer = setTimeout('getfocus()', 500);
//}
//-->
</script>
<script language="JavaScript" type="text/javascript">
<!--
// -----------------------------------------------------------------------------
// Globals
// Major version of Flash required
var requiredMajorVersion = 8;
// Minor version of Flash required
var requiredMinorVersion = 0;
// Revision of Flash required
var requiredRevision = 0;
// the version of javascript supported
var jsVersion = 1.0;
// -----------------------------------------------------------------------------
// -->
</script>
<script language="VBScript" type="text/vbscript">
<!-- // Visual basic helper required to detect Flash Player ActiveX control version information
Function VBGetSwfVer(i)
  on error resume next
  Dim swControl, swVersion
  swVersion = 0
  
  set swControl = CreateObject("ShockwaveFlash.ShockwaveFlash." + CStr(i))
  if (IsObject(swControl)) then
    swVersion = swControl.GetVariable("$version")
  end if
  VBGetSwfVer = swVersion
End Function
// -->
</script>
<script language="JavaScript1.1" type="text/javascript">
<!-- // Detect Client Browser type
var isIE  = (navigator.appVersion.indexOf("MSIE") != -1) ? true : false;
var isWin = (navigator.appVersion.toLowerCase().indexOf("win") != -1) ? true : false;
var isOpera = (navigator.userAgent.indexOf("Opera") != -1) ? true : false;
jsVersion = 1.1;
// JavaScript helper required to detect Flash Player PlugIn version information
function JSGetSwfVer(i){
	// NS/Opera version >= 3 check for Flash plugin in plugin array
	if (navigator.plugins != null && navigator.plugins.length > 0) {
		if (navigator.plugins["Shockwave Flash 2.0"] || navigator.plugins["Shockwave Flash"]) {
			var swVer2 = navigator.plugins["Shockwave Flash 2.0"] ? " 2.0" : "";
      		var flashDescription = navigator.plugins["Shockwave Flash" + swVer2].description;
			descArray = flashDescription.split(" ");
			tempArrayMajor = descArray[2].split(".");
			versionMajor = tempArrayMajor[0];
			versionMinor = tempArrayMajor[1];
			if ( descArray[3] != "" ) {
				tempArrayMinor = descArray[3].split("r");
			} else {
				tempArrayMinor = descArray[4].split("r");
			}
      		versionRevision = tempArrayMinor[1] > 0 ? tempArrayMinor[1] : 0;
            flashVer = versionMajor + "." + versionMinor + "." + versionRevision;
      	} else {
			flashVer = -1;
		}
	}
	// MSN/WebTV 2.6 supports Flash 4
	else if (navigator.userAgent.toLowerCase().indexOf("webtv/2.6") != -1) flashVer = 4;
	// WebTV 2.5 supports Flash 3
	else if (navigator.userAgent.toLowerCase().indexOf("webtv/2.5") != -1) flashVer = 3;
	// older WebTV supports Flash 2
	else if (navigator.userAgent.toLowerCase().indexOf("webtv") != -1) flashVer = 2;
	// Can't detect in all other cases
	else {
		
		flashVer = -1;
	}
	return flashVer;
} 
// If called with no parameters this function returns a floating point value 
// which should be the version of the Flash Player or 0.0 
// ex: Flash Player 7r14 returns 7.14
// If called with reqMajorVer, reqMinorVer, reqRevision returns true if that version or greater is available
function DetectFlashVer(reqMajorVer, reqMinorVer, reqRevision) 
{
 	reqVer = parseFloat(reqMajorVer + "." + reqRevision);
   	// loop backwards through the versions until we find the newest version	
	for (i=25;i>0;i--) {	
		if (isIE && isWin && !isOpera) {
			versionStr = VBGetSwfVer(i);
		} else {
			versionStr = JSGetSwfVer(i);		
		}
		if (versionStr == -1 ) { 
			return false;
		} else if (versionStr != 0) {
			if(isIE && isWin && !isOpera) {
				tempArray         = versionStr.split(" ");
				tempString        = tempArray[1];
				versionArray      = tempString .split(",");				
			} else {
				versionArray      = versionStr.split(".");
			}
			versionMajor      = versionArray[0];
			versionMinor      = versionArray[1];
			versionRevision   = versionArray[2];
			
			versionString     = versionMajor + "." + versionRevision;   // 7.0r24 == 7.24
			versionNum        = parseFloat(versionString);
        	// is the major.revision >= requested major.revision AND the minor version >= requested minor
			if ( (versionMajor > reqMajorVer) && (versionNum >= reqVer) ) {
				return true;
			} else {
				return ((versionNum >= reqVer && versionMinor >= reqMinorVer) ? true : false );	
			}
		}
	}	
	return (reqVer ? false : 0.0);
}
// -->
</script>
</head>
<body bgcolor="#A797A7" MARGINWIDTH=0 MARGINHEIGHT=0 LEFTMARGIN=0 TOPMARGIN=0 onLoad="">
<!--url's used in the movie-->
<!--text used in the movie-->
<CENTER>
<!--
<p align="center"></p>
<p align="left"></p>
<p align="left"><font face="Arial" size="9" color="#000000" letterSpacing="0.000000" kerning="1"><b>FPS</b></font></p>
<p align="center"><font face="Times New Roman" size="18" color="#000000" letterSpacing="0.000000" kerning="1"><b>CONNECTION TO SERVER LOST:</b></font></p><p align="center"></p><p align="center"><font face="Times New Roman" size="18" color="#000000" letterSpacing="0.000000" kerning="1"><b> Either your Internet connection is down, or the <sbr />server is restarting or possibly down. &nbsp;</b></font></p><p align="center"></p><p align="center"><font face="Times New Roman" size="18" color="#000000" letterSpacing="0.000000" kerning="1"><b>Please try again shortly.</b></font></p><p align="center"></p><p align="center"><font face="Times New Roman" size="18" color="#000066" letterSpacing="0.000000" kerning="1"><a href="http://hi1.horseisle.com/" target = "_self"><b>HI1.HORSEISLE.COM</b></a></font></p>
-->
<script language="JavaScript" type="text/javascript">
<!-- 
<?php
echo("var hasRightVersion = DetectFlashVer(requiredMajorVersion, requiredMinorVersion, requiredRevision);
if(hasRightVersion) {  // if we've detected an acceptable version
    var oeTags = '<object classid=\"clsid:D27CDB6E-AE6D-11cf-96B8-444553540000\"'
    + 'width=\"790\" height=\"500\" id=\"horseisle\" name=\"horseisle\"'
    + 'codebase=\"http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab\">'
    + '<param name=\"movie\" value=\"horseisle_patched.swf?SERVER=".$server_ip."&PORT=".$server_port."&USER=&2158322\" /><param name=\"loop\" value=\"false\" /><param name=\"menu\" value=\"false\" /><param name=\"quality\" value=\"high\" /><param name=\"scale\" value=\"noscale\" /><param name=\"salign\" value=\"t\" /><param name=\"bgcolor\" value=\"#ffffff\" />'
    + '<embed src=\"horseisle_patched.swf?SERVER=".$server_ip."&PORT=".$server_port."&USER=&2158322\" loop=\"false\" menu=\"false\" quality=\"high\" scale=\"noscale\" salign=\"t\" bgcolor=\"#ffffff\" '
    + 'width=\"790\" height=\"500\" name=\"horseisle\" align=\"top\"'
    + 'play=\"true\"'
    + 'loop=\"false\"'
    + 'quality=\"high\"'
    + 'allowScriptAccess=\"sameDomain\"'
    + 'type=\"application/x-shockwave-flash\"'
    + 'pluginspage=\"http://www.macromedia.com/go/getflashplayer\">'
    + '<\/embed>'
    + '<\/object>';");
?>
    document.write(oeTags);   // embed the flash movie
  } else {  // flash is too old or we can't detect the plugin
    var alternateContent = 'Alternate HTML content should be placed here.'
  	+ 'This content requires the Macromedia Flash Player.'
   	+ '<a href=http://www.macromedia.com/go/getflash/>Get Flash</a>';
    document.write(alternateContent);  // insert non-flash content
  }
// -->
</script>
<noscript><CENTER>
It appears you do not have the required Flash Player Software.<BR>
<B>Horse Isle requires the Adobe Flash Player 9+.</B><BR>
It is a free and easy download - <a href="http://www.macromedia.com/go/getflash/">Get Flash</a><BR>
</noscript>




</body>
</html>
