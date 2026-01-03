using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace HTTP
{
    class ContentItem
    {

        public String Name;
        public String FilePath;
        public ContentItem(string name,string filePath)
        {
            if(File.Exists(filePath))
            {
                this.FilePath = filePath;
                this.Name = name;
            }   
            else
            {
                throw new FileNotFoundException();
            }
        }

    }
    
    class ContentClient
    {
        private ContentServer baseServ;
        private Socket clientSock;

        public ContentClient(ContentServer server, Socket clientSock)
        {
            this.clientSock = clientSock;
            this.baseServ = server;
            ProcessRequests();
            this.clientSock.Close();
            
        }


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

        private void RespondGet(string name)
        {
         
            if (ContentItemExists(name))
            {
                ContentItem ci = GetContentItem(name);
                FileStream fs = File.OpenRead(ci.FilePath);
                try
                {
                    string requestStr = GenerateHeaders(ci.FilePath, fs.Length - fs.Position);
                   
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
                string body = GeneratePage(name);
                string requestStr = GenerateHeaders(name, body.Length);
                requestStr += body;
                
                SendString(requestStr);
            }

        }

        private void RespondHead(string path)
        {
            string name = Path.GetFileName(path);

            if (ContentItemExists(name))
            {
                ContentItem ci = GetContentItem(name);
                string requestStr = GenerateHeaders(ci.FilePath);
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
                if (ci.Name == name)
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
                if (ci.Name == name)
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
                string body = "Horse Isle Web Server..<br>Fork of LiEnby's \"Content Server\"";
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
        private string ExtractRelativeUrl(string header)
        {
            return HttpUtility.UrlDecode(header.Split(' ')[1]);
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
                    RespondGet(path);
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
        public Socket ServerSocket;
        public string IpAddr;
        public short Port;

        private bool shutdownServer = false;


        public void CreateClient(object sender, SocketAsyncEventArgs e)
        {
            do
            {
                if(shutdownServer)
                   return;
                   
                Socket eSocket = e.AcceptSocket;
                if (eSocket != null)
                    new ContentClient(this, eSocket);
                e.AcceptSocket = null;
            } while (!ServerSocket.AcceptAsync(e));
        }
        
        public void Shutdown() {
            shutdownServer = true;
            if(ServerSocket != null)
            {
                ServerSocket.Dispose();
                ServerSocket = null;
            }
        }

        public ContentServer(string ip, short port)
        {
	        this.IpAddr = ip;
	        this.Port = port;
	    
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(ep);
            ServerSocket.Listen(0x7fffffff);

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += CreateClient;
            CreateClient(this, e);
        }


    }
}
