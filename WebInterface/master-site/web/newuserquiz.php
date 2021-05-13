<?php
$obj = json_decode(file_get_contents("newuserquizquestions.json"), true);
$chatpoint = 0;
//echo $obj["questions"][0]["title"]
if (isset($_GET["CHATPOINT"])) {
    if ($_GET["CHATPOINT"] === "-1") {
        header('Location: /');
    }
    $chatpoint = intval($_GET["CHATPOINT"]);
}
if (isset($obj["questions"][$chatpoint])) {
    $question = $obj["questions"][$chatpoint];
}

if (isset($question["redirect"])) {
    header('Location: '.$question["redirect"]);
}
include("header.php");
?>
<BR><TABLE BORDER=0 CELLPADDING=20><TR><TD><B><?php echo $question["title"] ?></B><BR><BR><?php foreach ($question["answers"] as &$value) { echo "<LI>REPLY WITH: <A HREF=?CHATPOINT=".$value["chatpoint"].">" . $value["title"] .  "</A></LI><BR>";}?><BR></TD></TR></TABLE><BR><TABLE BORDER=0 CELLPADDING=0 CELLSPACING=0 WIDTH=100%>
<?php include("footer.php"); ?>