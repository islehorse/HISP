<?php
	include("../common.php");
	include("../config.php");
	include("header.php");
	$obj = json_decode(file_get_contents("knowledge_base.json"), true);

	function get_kb_id(int $kbId){
		global $obj;
		foreach ($obj["kbIds"] as &$kbIdEnt){
			if($kbIdEnt['kbId'] !== $kbId)
				continue;
			
			return $kbIdEnt; 
		}
		
		return null;
	}
	
	function write_kb_id(int $kbId){		
		$kbIdEnt = get_kb_id($kbId);
		$kbTitle = $kbIdEnt['kbTitle'];
		$kbContent = $kbIdEnt['kbContent'];
		echo('<TABLE BORDER=0 CELLPADDiNG=4 CELLSPACING=0><TR><TD>');
		echo(' &nbsp; <B>'.htmlspecialchars($kbTitle, ENT_QUOTES).':</B> ');
		echo($kbContent);
		echo('</TD></TR></TABLE>');
	}
?>
<FONT SIZE=+1><B>Horse Isle Help Center</B></FONT><BR>
Browse the different categories for detailed game info and help with any problems you may have. 
<A NAME=KB><TABLE WIDTH=100%><TR><TD class=forumlist WIDTH=150>Main Category</TD><?php
	#<TD class=forumlist><A HREF="?MAIN=ECONOMY#KB">ECONOMY</A><BR>(12 topics)</TD><TD class=forumlist><A HREF="?MAIN=FAQ#KB">FAQ</A><BR>(10 topics)</TD><TD class=forumlist><A HREF="?MAIN=GAME#KB">GAME</A><BR>(24 topics)</TD><TD class=forumlist><A HREF="?MAIN=HORSES#KB">HORSES</A><BR>(38 topics)</TD><TD class=forumlist><A HREF="?MAIN=SUPPORT#KB">SUPPORT</A><BR>(38 topics)</TD><TD class=forumlist><A HREF="?MAIN=TOOL BAR#KB">TOOL BAR</A><BR>(36 topics)</TD></TR></TABLE><BR>
	foreach ($obj["kbData"] as &$kbData){
		$kbName = $kbData["kbName"];
		# get count of topics
		$topicCount = 0;
		foreach ($kbData["kbList"] as &$kbList){
			$topicCount += count($kbList["kbIds"]);
		}
		
		echo("<TD class=forumlist><A HREF=\"?MAIN=".htmlspecialchars($kbName, ENT_QUOTES)."#KB\">".htmlspecialchars($kbName, ENT_QUOTES)."</A><BR>(".(string)$topicCount." topics)</TD>");
	}
	echo('</TR></TABLE>');
	if(isset($_GET["MAIN"])){
		$MAIN_CATEGORY = $_GET["MAIN"];
		echo('</TABLE><TABLE BORDER=0 CELLPADDiNG=2 CELLSPACING=0 WIDTH=100% BGCOLOR=FFFFFF><TR><TD class=forumlist WIDTH=150>Sub Category:</TD>');
		foreach ($obj["kbData"] as &$kbData){
			$kbName = $kbData["kbName"];
			if($kbName !== $MAIN_CATEGORY)
				continue;
			
			foreach ($kbData["kbList"] as &$kbList){
				$kbSubName = $kbList['kbSubName'];
				echo('<TD><CENTER><A HREF="?MAIN='.htmlspecialchars($kbName, ENT_QUOTES).'&SUB='.htmlspecialchars($kbSubName, ENT_QUOTES).'#KB">'.htmlspecialchars($kbSubName, ENT_QUOTES).'</A></CENTER></TD>');
			}
		}
		echo('</TR></TABLE>');
	}
	if(isset($_GET["SUB"])){
		$SUB_CATEGORY = $_GET["SUB"];
		
		echo('<TABLE WIDTH=100%><TR VALIGN=top><TD WIDTH=250><TABLE BORDER=0 CELLPADDING=2 CELLSPACING=0 WIDTH=100%>');
		
		foreach ($obj["kbData"] as &$kbData){
			$kbName = $kbData["kbName"];
			foreach ($kbData["kbList"] as &$kbList){
				$kbSubName = $kbList['kbSubName'];
				if($kbSubName !== $SUB_CATEGORY)
					continue;
				
				$numb = 1;
				$alternate = ["a0", "a1"];
				foreach ($kbList['kbIds'] as &$kbId){
					$kbIdEnt = get_kb_id($kbId);
					$kbTitle = $kbIdEnt['kbTitle'];
					echo('<TR class='.htmlspecialchars($alternate[$numb % 2], ENT_QUOTES).'><TD>'.htmlspecialchars((string)$numb, ENT_QUOTES).') <A HREF="?MAIN='.htmlspecialchars($kbName, ENT_QUOTES).'&SUB='.htmlspecialchars($kbSubName, ENT_QUOTES).'&KBID='.htmlspecialchars((string)$kbId, ENT_QUOTES).'#KB">'.htmlspecialchars($kbTitle, ENT_QUOTES).'</A>');
					
					if(isset($_GET['KBID']))
						if($_GET['KBID'] == $kbId)
							echo('<B> >>></B>');
					
					echo('</TD></TR>');
					$numb++;
				}
			}
		}
		
		echo('</TABLE></TD><TD VALIGN=top BGCOLOR=FFDDDD> ');
		
		if(isset($_GET['KBID'])){
			$kbId = intval($_GET['KBID']);
			write_kb_id($kbId);
		}

		echo('</TD></TR></TABLE>');
	}
	if(isset($_GET['KBID']) && !isset($_GET["SUB"])){
			$kbId = intval($_GET['KBID']);
			write_kb_id($kbId);
	}
	
	?><BR><?php
include("footer.php");
?>