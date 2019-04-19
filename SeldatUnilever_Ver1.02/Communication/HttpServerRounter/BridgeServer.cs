using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SelDatUnilever_Ver1._00.Communication.HttpServerRounter
{
    class BridgeServer
    {

        protected int port;
        TcpListener listener;
        bool is_active = true;
        public BridgeServer(int port)
        {
            this.port = port;
        }
        public void listen()
        {
            //Task.Run(() =>
         //   {
                listener = new TcpListener(port);
                listener.Start();
                while (is_active)
                {
                    TcpClient s = listener.AcceptTcpClient();
                    Thread thread = new Thread(new ThreadStart(Process));
                    thread.Start();
                    Thread.Sleep(1);
                }
          //  });
        }
        public void Process()
        {

        }
        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }
    }
}
