import os
import requests
files = os.listdir(".")
forums = ['SUPPORT', 'BUGS', 'GENERAL', 'HORSES', 'GAME']
tcontents = {}


def get_between(txt, a1, a2):
    aStart = txt.index(a1)+len(a1)
    txt = txt[aStart:]
    aEnd = txt.index(a2)
    txt = txt[:aEnd]
    return txt


for forum in forums:
    tcontents[forum] = requests.get('http://master.horseisle.com/web/forums.php?FORUM='+forum, headers={"Cookie":"PHPSESSID=ve8og4p8lk3f163bmu0d90j665"}).content
"""
for file in files:
    if file.endswith('.html'):
        data = open(file, 'rb').read()
        try:
            rid = get_between(data, b"NAME=VIEWID VALUE='", b"'></FORM>").decode('UTF-8')
            print(rid)
            while True:
                try:
                    os.rename(file, rid+'.html')
                    break
                except:
                    pass
        except:
            print("wut? "+file)"""
for file in files:
    if file.endswith('.html'):
        file = file.replace('.html', '')
        found = False
        print(b'&VIEWID='+file.encode('UTF-8'))
        for forum in forums:
            fcontents = tcontents[forum]
            if fcontents.find(b'&VIEWID='+file.encode('UTF-8')) != -1:
                data = open(file+'.html', 'rb').read().replace(b'VIEWING GAME FORUM THREAD', b'VIEWING '+forum.encode("UTF-8")+b' FORUM THREAD')
                open(file+'.html', 'wb').write(data)
                os.rename(file+'.html', forum+'_'+file+'.html')
                found = True
                break
        
        if not found:
            data = open(file+'.html', 'rb').read().replace(b'VIEWING GAME FORUM THREAD', b'VIEWING MOD FORUM THREAD')
            open(file+'.html', 'wb').write(data)
            while True:
                try:
                    os.rename(file+'.html', 'MOD_'+file+'.html') # MUST be Mod Forum
                    break
                except:
                    pass
            