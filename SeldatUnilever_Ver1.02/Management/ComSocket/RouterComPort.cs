using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SelDatUnilever_Ver1._00.Management.ComSocket
{
    public class RouterComPort
    {
        // ManualResetEvent instances signal completion.  
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        protected const UInt32 TIME_OUT_WAIT_RESPONSE = 10000;
        protected const UInt32 TIME_OUT_WAIT_CONNECT = 60000*10;

        // The response from the remote device.  
        private String response = String.Empty;

        public bool flagReadyReadData { get; private set; }
        public bool flagConnected { get; private set; }

        public Socket client = null;
        public String Ip { get; set; }
        public Int32 Port { get; set; }

        public struct DataReceive
        {
            public int length;
            public byte[] data;
        }
        public class StateObject
        {
            // Client socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int BufferSize = 256;
            // Receive buffer.  
            // public byte[] buffer = new byte[BufferSize];
            public DataReceive buffer;

            // Received data string.  
            public StringBuilder sb = new StringBuilder();
            public StateObject()
            {
                buffer.data = new byte[BufferSize];
                buffer.length = 0;
            }
        }

        private StateObject state;


        public RouterComPort()
        {

        }
        private void Connect(EndPoint remoteEP, Socket client)
        {
            if (client != null)
            {
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);

                connectDone.WaitOne();
            }
            else
            {
                Console.WriteLine("Please create socket\r\n");
            }
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                //connectDone.Set();
                flagConnected = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Close()
        {
            if (client != null)
            {
                if (client.Connected)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            }
        }
        private void Receive(Socket client)
        {
            if (client != null)
            {
                try
                {
                    // Create the state object.  
                    state = new StateObject();
                    state.workSocket = client;

                    // Begin receiving the data from the remote device.  
                    client.BeginReceive(state.buffer.data, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            else
            {
                Console.WriteLine("Please create socket\r\n");
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                // Read data from the remote device.  
                state.buffer.length = client.EndReceive(ar);
                if (state.buffer.length > 0)
                {
                    // There might be more data, so store the data received so far.  
                    // state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    // //  Get the rest of the data.  
                    // client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    //     new AsyncCallback(ReceiveCallback), state);
                    // response = state.sb.ToString();
                    flagReadyReadData = true;
                }
                //else
                //{
                //    // All the data has arrived; put it in response.  
                //    if (state.sb.Length > 1)
                //    {
                //        response = state.sb.ToString();
                //        FlagReadyReadData = true;
                //    }
                //    // Signal that all bytes have been received.  
                //    receiveDone.Set();
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        protected bool WaitConnected(UInt32 timeOut)
        {
            bool result = true;
            Stopwatch sw = new Stopwatch();
            Receive(client);
            sw.Start();
            while (flagConnected == false)
            {
                if (sw.ElapsedMilliseconds > timeOut)
                {
                    result = false;
                    break;
                }
                Thread.Sleep(50);
            }
            sw.Stop();
            return result;
        }

        protected bool WaitForReadyRead(UInt32 timeOut)
        {
            bool result = true;
            Stopwatch sw = new Stopwatch();
            Receive(client);
            sw.Start();
            while (flagReadyReadData == false)
            {
                if (sw.ElapsedMilliseconds > timeOut)
                {
                    result = false;
                    break;
                }
                Thread.Sleep(50);
            }
            sw.Stop();
            return result;
        }
        protected bool Send(Socket client, byte[] byteData)
        {
            bool ret = true;
            // Begin sending the data to the remote device.  
            if (client != null)
            {
                try
                {
                    client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    ret = false;
                }
            }
            else
            {
                Console.WriteLine("Please create socket\r\n");
                ret = false;
            }
            return ret;
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        protected void StartClient(/*String ip, Int32 port*/)
        {
            //this.Ip = ip;
            //this.Port = port;
            // Connect to a remote device.  
            flagConnected = false;
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the   
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(this.Ip), this.Port);
                // Create a TCP/IP socket.  
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                //connectDone.WaitOne();

                if (false == this.WaitConnected(TIME_OUT_WAIT_CONNECT))
                {
                    Console.WriteLine("Connnect fail______<->________");
                }
                // Send test data to the remote device.  
                // Send(client, "This is a LLLLLlll test<EOF>");
                //  sendDone.WaitOne();

                // Receive the response from the remote device.  
                //  Receive(client);
                //  receiveDone.WaitOne();

                // Write the response to the console.  
                // Console.WriteLine("Response received : {0}", response);

                // Release the socket.  
                // client.Shutdown(SocketShutdown.Both);
                // client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        protected bool SendCMD(byte[] bData)
        {
            return Send(client, bData);
        }

        protected bool SendString(String sData)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(sData);
            return Send(client, byteData);
        }

        protected DataReceive GetDataRec()
        {
            DataReceive data = state.buffer;
            flagReadyReadData = false;
            return data;
        }
        public virtual void CheckAlive() { }
    }
}
