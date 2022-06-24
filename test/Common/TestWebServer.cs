using EmbedIO;
using System;
using System.Threading.Tasks;

namespace SeleniumExtras.Environment
{
    public class TestWebServer
    {
        private IWebServer webServer;

        private readonly string url;
        private readonly string htmlPath;

        public TestWebServer(string url, string htmlPath)
        {
            this.url = url;
            this.htmlPath = htmlPath;
        }

        public void Start()
        {
            if (webServer != null)
            {
                throw new InvalidOperationException("WebServer is already started!");
            }

            webServer = new WebServer(url)
                   .WithStaticFolder("/", htmlPath, true);
            Task.Run(() => webServer.RunAsync());
        }

        public void Stop()
        {
            if (webServer != null)
            {
                (webServer as IDisposable)?.Dispose();
                webServer = null;
            }
        }
    }
}
