import requests
import json
def get_between(txt, a1, a2):
    aStart = txt.index(a1)+len(a1)
    txt = txt[aStart:]
    aEnd = txt.index(a2)
    txt = txt[:aEnd]
    return txt

def move_forward(txt, a1, a2):
    aStart = txt.index(a1)+len(a1)
    txt = txt[aStart:]
    aEnd = txt.index(a2)+len(a2)
    txt = txt[aEnd:]
    return txt

def kblogic_dl():
    kblayout = {}
    kb = requests.get("https://master.horseisle.com/web/helpcenter.php")
    kbLst = get_between(kb.content, b"<A NAME=KB><TABLE WIDTH=100%><TR>",b"</TR></TABLE>")
#    print(kbLst)
    kbNameLst = []
    
    while True:
        try:
            kbEntry = get_between(kbLst, b"<TD class=forumlist>", b"</TD>")
            
            kbName = get_between(kbEntry, b"<A HREF=\"?MAIN=", b"#KB\">").decode("UTF-8")
            kbNameLst.append(kbName)
            print(kbName)
       
            kbLst = move_forward(kbLst, b"<TD class=forumlist>", b"</TD>")
        except:
            break
    
#    kblayout["kbNames"] = kbNameLst
    kblayout["kbData"] = []
    for kbName in kbNameLst:
        kbSubNameLst = []
        kb = requests.get("https://master.horseisle.com/web/helpcenter.php?MAIN="+kbName)
        print("Downloading subnames for "+kbName)
        kbLst = get_between(kb.content, b"</TD></TR></TABLE><TABLE BORDER=0 CELLPADDiNG=2 CELLSPACING=0 WIDTH=100% BGCOLOR=FFFFFF>",b"</TR></TABLE>")

        while True:
            try:
                kbEntry = get_between(kbLst, b"<TD>", b"</TD>")
                print(kbEntry)
                
                kbSubName = get_between(kbEntry, b"&SUB=", b"#KB").decode("UTF-8")
                print(kbName+":"+kbSubName)
                kbSubNameLst.append(kbSubName)
                
                
                kbLst = move_forward(kbLst, b"<TD>", b"</TD>")
            except:
                break
        
        kbLs = []
        # Get ids in sub
        for kbSubName in kbSubNameLst:
            kbSubObjects = []
            kb = requests.get("https://master.horseisle.com/web/helpcenter.php?MAIN="+kbName+"&SUB="+kbSubName)
            print("Downloading objects for "+kbName+":"+kbSubName)
            kbLst = get_between(kb.content, b"<TABLE WIDTH=100%><TR VALIGN=top><TD WIDTH=250><TABLE BORDER=0 CELLPADDING=2 CELLSPACING=0 WIDTH=100%>",b"</TR></TABLE></TD><TD VALIGN=top BGCOLOR=FFDDDD> </TD></TR>")
            while True:
                try:
                    kbEntry = get_between(kbLst, b"<TD>", b"</TD>")
                    print(kbEntry)
                    
                    kbObjectId = get_between(kbEntry, b"&KBID=", b"#KB").decode("UTF-8")
                    print(kbName+":"+kbSubName+":"+kbObjectId)
                    kbSubObjects.append(int(kbObjectId))
               
                    kbLst = move_forward(kbLst, b"<TD>", b"</TD>")
                except Exception as e:
                    print(e)
                    break
            pData = {"kbSubName":kbSubName, "kbIds":kbSubObjects}
            kbLs.append(pData)
        kblayout["kbData"].append({"kbName":kbName, "kbList": kbLs})

                
    print(json.dumps(kblayout))
    open("kblogic.json","wb").write(json.dumps(kblayout).encode("UTF-8"))
def kbid_dl():
    knowledgeBase = 1
    kbF = open("kb_id.json", "wb")
    failedInARow = 0
    kbF.write(b"[\r\n")
    while True:
        try:
            kb = requests.get("https://master.horseisle.com/web/helpcenter.php?KBID="+str(knowledgeBase))
            kbC = get_between(kb.content, b"</TD></TR></TABLE><TABLE BORDER=0 CELLPADDiNG=4 CELLSPACING=0><TR><TD>", b"</TD></TR></TABLE><BR><TABLE BORDER=0 CELLPADDING=0 CELLSPACING=0 WIDTH=100%>")
            kbTitle = get_between(kbC, b"&nbsp; <B>",b":</B> ")
            kbC = move_forward(kbC, b"&nbsp; <B>",b":</B> ")
            kbData = {"kbId":knowledgeBase, "kbTitle":kbTitle.decode("UTF-8"), "kbContent": kbC.decode("UTF-8")}
            print(json.dumps(kbData))
            kbF.write(json.dumps(kbData).encode("UTF-8")+b",\r\n")
            knowledgeBase+=1
            failedInARow = 0
        except Exception as e:
            print(e)
            knowledgeBase += 1
            failedInARow += 1
            if failedInARow > 50:
                break
            continue
    kbF.write(b"]\r\n")
    kbF.close()
    
#kblogic_dl()
kbid_dl()
