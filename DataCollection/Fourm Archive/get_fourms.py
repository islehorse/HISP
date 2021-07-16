1
import requests, threading
s = requests.Session()
s.headers = {"Cookie":"PHPSESSID=ve8og4p8lk3f163bmu0d90j665"}


class t(threading.Thread):
    def __init__(self, i):
        super().__init__()
        self.i = i
    def run(self):
        while True:
            try:
                tContents = s.get(f"http://master.horseisle.com/web/forums.php?FORUM=GAME&VIEWID={self.i}").content
            except Exception as e:
                print(e)
                continue
            break
        if tContents.find(b'Forum thread not found!?') != -1:
            pass
            #print(f"No such forum id: {i}\n",end="")
        else:
            print(f"forum id found: {i}\n", end="")
            open(f"{self.i}.html", "wb").write(tContents)
        sem.release()

sem = threading.BoundedSemaphore(100)

i = 0
while True:
    i+=1
    sem.acquire(True)
    t(i).start()