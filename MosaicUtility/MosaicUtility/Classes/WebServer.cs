using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MosaicUtility.Classes
{
    public class WebServer
    {
        private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };
        private Thread _serverThread;
        private string _rootDirectory;
        private HttpListener _listener;
        private string TempFolder = "";
        private int _port;
        private string _url;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        /// <param name="port">Port of the server.</param>
        public WebServer(string destination, int port)
        {
            TempFolder = Globals.DataFolder + "temp\\";
            if (!string.IsNullOrEmpty(destination))
                this._rootDirectory = destination;
            else
                this._rootDirectory = Globals.RootFolder + "server\\";


            this.Initialize(port);
        }

        /// <summary>
        /// Construct server with suitable port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        public WebServer(string path)
        {
            //get an empty port
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            this.Initialize(port);
        }

        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://+:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    if (context.Request.Url.AbsolutePath == "/")
                    {
                        //Process(context);
                    }
                    else if (SaveFile(context))
                    {
                        // success
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        private bool SaveFile(HttpListenerContext context)
        {
            try
            {
                Stream input = context.Request.InputStream;
                //Byte[] boundaryBytes = enc.GetBytes(boundary);
                //Int32 boundaryLen = boundaryBytes.Length;
                string targetPath = "";
                MultipartFormDataParser parser = new MultipartFormDataParser(input);
                var file = parser.Files[0];
                string filePath = Path.Combine(this._rootDirectory, file.FileName);
                long ticks = DateTime.Now.Ticks;
                if (File.Exists(filePath))
                {
                    filePath = Path.Combine(this.TempFolder, ticks.ToString()+ "_" + file.FileName);
                    targetPath = Path.Combine(this._rootDirectory, ticks.ToString() + "_" + file.FileName);
                }
                else
                {
                    filePath = Path.Combine(this.TempFolder, file.FileName);
                    targetPath = Path.Combine(this._rootDirectory, file.FileName);
                }

                using (FileStream output = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    file.Data.CopyTo(output);
                    output.Close();
                }

                File.Move(filePath, targetPath);
                ProcessResponse(context, Path.GetFileName(targetPath));

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private void Initialize(int port)
        {
            if (!Directory.Exists(this._rootDirectory))
                Directory.CreateDirectory(this._rootDirectory);

            if (!Directory.Exists(this.TempFolder))
                Directory.CreateDirectory(this.TempFolder);

            this._port = port;
            FirewallHelper fhelper = new FirewallHelper();
            //fhelper.AddApplicationRule("MYLOCALWEB1", AppDomain.CurrentDomain.BaseDirectory + "WpfApp1.exe");
            fhelper.AddPortRule("MYLOCALWEB2", (ushort)port);
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }

        private void ProcessResponse(HttpListenerContext context, string fileName)
        {

            try
            {
                //Stream input = new FileStream(filename, FileMode.Open);

                //Adding permanent http response headers
                string mime;
                context.Response.ContentType = "application/octet-stream";
                
                //context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                //context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                string filePath = "/uploads/" + fileName;
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(filePath);

                context.Response.ContentLength64 = buffer.Length;
                System.IO.Stream output = context.Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();

                
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                //context.Response.StatusDescription = ex.Message;
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(ex.Message);
                context.Response.ContentLength64 = buffer.Length;
                System.IO.Stream output = context.Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();

            }

            context.Response.OutputStream.Close();
        }
    }
}
