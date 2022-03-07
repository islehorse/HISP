using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HTTP
{
    class ContentItem
    {

        public String name;
        public String filePath;
        public ContentItem(string Name,string FilePath)
        {
            if(File.Exists(FilePath))
            {
                filePath = FilePath;
                name = Name;
            }   
            else
            {
                throw new FileNotFoundException();
            }
        }

    }
    
    class ContentClient
    {

        public ContentClient(ContentServer Server, Socket ClientSocket)
        {
            clientSock = ClientSocket;
            baseServ = Server;
            Server.Clients.Add(this);
            baseServ.WriteDebugOutput("Client Connected @ " + clientSock.RemoteEndPoint.ToString());
            ProcessRequests();
            clientSock.Close();
            
        }

        private ContentServer baseServ;
        private Socket clientSock;
        

        private byte[] ReadData()
        {
            while (clientSock.Available < 1) { }
            byte[] by = new byte[clientSock.Available];
            clientSock.Receive(by);
            return by;
        }

        private void SendString(string str)
        {
            byte[] response = Encoding.UTF8.GetBytes(str);
            clientSock.Send(response);
        }

        private string GenerateHeaders(string path, long content_length = 0)
        {
            string headers = "";

            if (path == "/") 
            {
                headers += "HTTP/1.1 200 OK\r\n";
                headers += "Content-Type: text/html\r\n";
                headers += "Accept-Ranges: bytes\r\n";
                headers += "Server: ContentServer\r\n";
                headers += "Content-Length: " + content_length + "\r\n";
                headers += "Cache-Control: max-age=0\r\n";
                headers += "Connection: keep-alive\r\n";
                headers += "\r\n";
            }
            else if (File.Exists(path))
            {
                FileInfo info = new FileInfo(path);
                long length = info.Length;

                if (content_length != 0)
                    length = content_length;


                headers += "HTTP/1.1 200 OK\r\n";
                headers += "Content-Type: application/x-shockwave-flash\r\n";
                headers += "Accept-Ranges: bytes\r\n";
                headers += "Server: ContentServer\r\n";
                headers += "Content-Length: " + length + "\r\n";
                headers += "Cache-Control: max-age=0\r\n";
                headers += "Connection: keep-alive\r\n";
                headers += "\r\n";
            }
            else
            {
                headers += "HTTP/1.1 404 Not Found\r\n";
                headers += "Content-Type: text/plain\r\n";
                headers += "Accept-Ranges: bytes\r\n";
                headers += "Server: ContentServer\r\n";
                headers += "Content-Length: " + content_length + "\r\n";
                headers += "Cache-Control: max-age=3600\r\n";
                headers += "Connection: keep-alive\r\n";
                headers += "\r\n";
            }
            return headers;
        }

        private void RespondGet(string path, Dictionary<string, string> query)
        {
            baseServ.WriteDebugOutput("GET " + path);
            string name = path.Remove(0, 1);

            if (ContentItemExists(name))
            {
                ContentItem ci = GetContentItem(name);
                FileStream fs = File.OpenRead(ci.filePath);
                try
                {
                    string requestStr = GenerateHeaders(ci.filePath, fs.Length - fs.Position);
                   
                    SendString(requestStr);
                    
                    while(fs.Position < fs.Length)
                    {
                        int BUFFER_SIZE = 0x8500000;
                        if(fs.Position + BUFFER_SIZE <= fs.Length)
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            fs.Read(buffer, 0x00, BUFFER_SIZE);
                            clientSock.Send(buffer);
                        }
                        else
                        {
                            byte[] buffer = new byte[fs.Length - fs.Position];
                            fs.Read(buffer, 0x00, buffer.Length);
                            clientSock.Send(buffer);
                        }
                        
                    }
                }
                catch (Exception) {
                    fs.Close();
                };

            }
            else
            {
                string body = GeneratePage(path);
                string requestStr = GenerateHeaders(path, body.Length);
                requestStr += body;
                
                SendString(requestStr);
            }

        }

        private void RespondHead(string path)
        {
            string name = Path.GetFileName(path);
            baseServ.WriteDebugOutput("HEAD " + path);

            if (ContentItemExists(name))
            {
                ContentItem ci = GetContentItem(name);
                string requestStr = GenerateHeaders(ci.filePath);
                SendString(requestStr);
            }
            else
            {
                string body = GeneratePage(path);
                string requestStr = GenerateHeaders(path, body.Length);
                SendString(requestStr);
            }

        }

        private bool ContentItemExists(string name)
        {
            bool exists = false;


            foreach (ContentItem ci in baseServ.Contents)
            {
                if (ci.name == name)
                {
                    exists = true;
                }
            }
            return exists;
        }

        private ContentItem GetContentItem(string name)
        {

            foreach (ContentItem ci in baseServ.Contents)
            {
                if (ci.name == name)
                {
                    return ci;
                }
            }
            throw new FileNotFoundException();
        }

        private string GeneratePage(string path)
        {
            if (path == "/")
            {
                string body = "Content Downloader Server.<br>Open this url in PSVita's \"Content Downloader\" To view avalible files.";
                foreach (ContentItem content in baseServ.Contents)
                {
                    body += "<a href=\"" + content.name + "\"></a>";
                }

                return body;
            }
            else
            {
                string body = "File not found.";
                return body;
            }
        }

        private string ExtractPath(string relativeUri)
        {
            int questionIndex = relativeUri.IndexOf("?");
            if (questionIndex != -1)
                return relativeUri.Substring(0, questionIndex);
            else
                return relativeUri;
        }
        private Dictionary<string,string> ExtractQuery(string relativeUri)
        {
            int questionIndex = relativeUri.IndexOf("?");
            if (questionIndex != -1)
            {
                string[] queryStrList = relativeUri.Substring(questionIndex + 1).Split('&');
                Dictionary<string,string> queryDict = new Dictionary<string, string>();
                foreach(string queryStr in queryStrList)
                {
                    string[] qStr = queryStr.Split('=');
                    queryDict.Add(qStr[0], qStr[1]);
                }
                return queryDict;
            }
            else
                return new Dictionary<string, string>();
        }
        private string ExtractRelativeUrl(string header)
        {
            return header.Split(' ')[1];
        }
        private void ProcessRequests()
        {
            byte[] data = ReadData();

            // Parse Request

            string curReq = Encoding.UTF8.GetString(data);

            curReq = curReq.Replace("\r\n", "\n");
            string[] reqLines = curReq.Split('\n');
            foreach (string line in reqLines)
            {
                if (line.StartsWith("GET"))
                {
                    string relUrl = ExtractRelativeUrl(line);
                    string path = ExtractPath(relUrl);
                    Dictionary<string,string> query = ExtractQuery(relUrl);
                    RespondGet(path, query);
                    return;
                }
                else if (line.StartsWith("HEAD"))
                {
                    string relUrl = ExtractRelativeUrl(line);
                    string path = ExtractPath(relUrl);
                    RespondHead(path);
                    return;
                }
            }

        }
    }
    class ContentServer
    {
        public List<ContentItem> Contents = new List<ContentItem>();
        public List<ContentClient> Clients = new List<ContentClient>();

        public void WriteDebugOutput(string txt)
        {
            Console.WriteLine("[HTTP] "+txt);
        }

        public ContentServer()
        {

            new Thread(() =>
            {
                WriteDebugOutput("Listening for connections on port 12515.");
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 12515);
                Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                newsock.Bind(localEndPoint);
                newsock.Listen(20);

                while(true)
                {
                    Socket clientSock = newsock.Accept();

                    new Thread(() =>
                    {
                        ContentClient client = new ContentClient(this, clientSock);
                        Clients.Remove(client);
                    }).Start();
                }
                
            }).Start();
        }


    }
}
