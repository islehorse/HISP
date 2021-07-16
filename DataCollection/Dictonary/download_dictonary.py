import requests
import os
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


def dictDl():
    rounds = 50
    leak_url = "https://master.horseisle.com/web/newuser.php"
    words = set(open("dictonary.dic", "rb").read().split(b'\r\n'))
    for i in range(0,rounds):
        r = requests.get(leak_url)
        content = r.content.replace(b'\r', b'').replace(b'\n', b'').replace(b'</FONT></TD><TD><FONT SIZE=-1><CENTER>', b'')
        generated_names = get_between(content, b"</TD></TR><TR><TD><CENTER><FONT SIZE=-1>",b"<BR></FONT></TD></TR></TABLE>")
        names = generated_names.split(b"<BR>")
        #print(names)
        #print(generated_names)
        for name in names:
            curWord = ""
            for c in name:
                if chr(c).upper() == chr(c):
                    if curWord != '':
                        words.add(curWord.encode("utf-8"))
                        curWord = ''
                curWord += chr(c)
        
        dictFile = open("dictonary.dic", "wb")
        for word in words:
            if word == '':
                continue
            print(word)
            dictFile.write(word+b"\r\n")
       
def bannedDl():
    words = requests.get("https://raw.githubusercontent.com/dwyl/english-words/master/words.txt").content.replace(b'\r', b'').split(b'\n')
    banned = set()
    for word in words:
        print(b"word: "+word)
        req = requests.post("https://master.horseisle.com/web/newuser.php", data={"user":word.decode("UTF-8"),"pass1":"", "pass2":"", "sex":"FEMALE", "email":"", "country":"", "passreqq":"Select a question", "passreqa":"", "A":""})
        
        if req.content.find(b'THIS IS A FAMILY GAME!!') != -1:
            print(b"banned word: "+word)
            banned.add(word)
            
            dictFile = open("banned.dic", "wb")
            for bword in banned:
                if bword == '':
                    continue
                dictFile.write(bword+b"\r\n")
            dictFile.close()

bannedDl()