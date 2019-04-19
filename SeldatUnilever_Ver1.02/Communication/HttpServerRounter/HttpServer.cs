using SeldatUnilever_Ver1._02.DTO;
using SelDatUnilever_Ver1._00.Communication.HttpServerRounter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SelDatUnilever_Ver1._00.Communication
{
     public abstract class HttpServer : NotifyUIBase
    {

            protected int port;
            TcpListener listener;
            bool is_active = true;
            public HttpServer(int port)
            {
                this.port = port;
            }

        public void listen()
        {

            Task.Run(() =>
            {
                try
                {
                    listener = new TcpListener(port);
                    listener.Start();
                    while (is_active)
                    {
                        TcpClient s = listener.AcceptTcpClient();
                        HttpProcessor processor = new HttpProcessor(s, this);
                        Thread thread = new Thread(new ThreadStart(processor.process));
                        thread.Start();
                        Thread.Sleep(1);

                    }
                }
                catch
                {
                    MessageBox.Show("Chương Trình Khác Đã Mở !");
                    Environment.Exit(0);
                }
            });
                   
    
              
         }
        public virtual void handleGETRequest(HttpProcessor p) { }
        public virtual void handlePOSTRequest(HttpProcessor p, StreamReader inputData) { }
    }
}
