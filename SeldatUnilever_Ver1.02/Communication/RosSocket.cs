/*
© Siemens AG, 2017-2018
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

<http://www.apache.org/licenses/LICENSE-2.0>.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/


using System;
using System.Linq;
using System.Collections.Generic;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using SeldatMRMS.Communication;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;

namespace SeldatMRMS.Management.RobotManagent
{
    public class RosSocket
    {
        public enum ConnectionStatus
        {
            CON_OK = 0,
            CON_FAILED,
            CON_CLOSED
        }
        #region Public
        private bool IsDisposed = false;
        protected bool onBinding = true;
        public int timeOutReConnection = 0;
        protected virtual void OnClosedEvent(object sender, CloseEventArgs e)
        {
            if (timeOutReConnection++<10)
            {
                if (!IsDisposed)
                {

                    try
                    {
                        Close();
                        webSocket.Connect();
                    }
                    catch { }


                }
                else
                {
                    timeOutReConnection = 0;
                    Dispose();
                }
            }
        }
        protected virtual void OnOpenedEvent() { }
        public String url { get; set; }
        public RosSocket()
        {

        }
        public void Start(string url)
        {
            /* new Thread(() =>
             {
                 while (onBinding)
                 {
                     this.url = url;
                     IsDisposed = false;
                     webSocket = new WebSocket(url);
                     webSocket.OnOpen += (sender, e) => OnOpenedEvent();
                     webSocket.OnMessage += (sender, e) => recievedOperation((WebSocket)sender, e);
                     webSocket.Connect();
                     if (webSocket.IsAlive)
                         break;
                     else
                         webSocket=null;
                     Thread.Sleep(4000);
                 }

                 if (webSocket != null)
                 {
                     if(webSocket.IsAlive)
                         webSocket.OnClose += (sender, e) => OnClosedEvent((WebSocket)sender, e);
                 }

             }).Start();*/
            //  Console.Write("");

            new Thread(() =>
            {
                    this.url = url;
                    IsDisposed = false;
                    webSocket = new WebSocket(url);
                    webSocket.OnClose += (sender, e) => OnClosedEvent((WebSocket)sender, e);
                    webSocket.OnOpen += (sender, e) => OnOpenedEvent();
                    webSocket.OnMessage += (sender, e) => recievedOperation((WebSocket)sender, e);
                    webSocket.Connect();
                    
            }).Start(); 
        }
        public virtual void Dispose()
        {
            try
            {
                if (webSocket != null)
                {
                    IsDisposed = true;
                    //webSocket.OnMessage -= (sender, e) => recievedOperation((WebSocket)sender, e);
                    webSocket.OnClose -= (sender, e) => OnClosedEvent((WebSocket)sender, e);
                    //webSocket.OnOpen -= (sender, e) => OnOpenedEvent();
                    if (webSocket.IsAlive)
                        Close();
                    webSocket = null;
                }
            }
            catch
            {
                Console.WriteLine("Error Dispose in RosSocket !!");
            }
        }
        public void Close()
        {
            
            while (publishers.Count > 0)
            {
                Unadvertize(publishers.First().Key);
            }

            while (subscribers.Count > 0)
                Unsubscribe(subscribers.First().Key);
            try
            {
                webSocket.Close();
            }
            catch { }
        }
        public delegate void ServiceHandler(object obj);
        public delegate void MessageHandler(Message message);

        public int Advertise(string topic, string type)
        {
            int id = generateId();
            publishers.Add(id, new Publisher(topic));

            sendOperation(new Adverisement(id, topic, type));
            return id;
        }

        public void Publish(int id, Message msg)
        {
            Publisher publisher;
            if (publishers.TryGetValue(id, out publisher))
                sendOperation(new Publication(id, publisher.Topic, msg));
        }

        public void Unadvertize(int id)
        {
            sendOperation(new Unadverisement(id, publishers[id].Topic));
            publishers.Remove(id);
        }



        public int Subscribe(string topic, string rosMessageType, MessageHandler messageHandler, int throttle_rate = 0, int queue_length = 1, int fragment_size = int.MaxValue, string compression = "none")
        {

            Type messageType = MessageTypes.MessageType(rosMessageType);
            if (messageType == null)
                return 0;

            int id = generateId();
            subscribers.Add(id, new Subscriber(topic, messageType, messageHandler));
            sendOperation(new Subscription(id, topic, rosMessageType, throttle_rate, queue_length, fragment_size, compression));
            return id;

        }

        public int Subscribe(string topic, Type messageType, MessageHandler messageHandler, int throttle_rate = 0, int queue_length = 1, int fragment_size = int.MaxValue, string compression = "none")
        {
            string rosMessageType = MessageTypes.RosMessageType(messageType);
            if (rosMessageType == null)
                return 0;

            return Subscribe(topic, messageType, messageHandler, throttle_rate, queue_length, fragment_size, compression);
        }

        public void Unsubscribe(int id)
        {
            sendOperation(new Unsubscription(id, subscribers[id].topic));
            subscribers.Remove(id);
        }

        public int CallService(string service, Type objectType, ServiceHandler serviceHandler, object args = null)
        {
            int id = generateId();
            serviceCallers.Add(id, new ServiceCaller(service, objectType, serviceHandler));

            sendOperation(new ServiceCall(id, service, args));
            return id;
        }
        #endregion

        #region Private

        internal struct Publisher
        {
            internal string Topic;
            internal Publisher(string topic)
            {
                Topic = topic;
            }
        }

        internal struct Subscriber
        {
            internal string topic;
            internal Type messageType;
            internal MessageHandler messageHandler;
            internal Subscriber(string Topic, Type MessageType, MessageHandler MessageHandler)
            {
                topic = Topic;
                messageType = MessageType;
                messageHandler = MessageHandler;
            }
        }
        internal struct ServiceCaller
        {
            internal string service;
            internal Type objectType;
            internal ServiceHandler serviceHandler;
            internal ServiceCaller(string Service, Type ObjectType, ServiceHandler ServiceHandler)
            {
                service = Service;
                objectType = ObjectType;
                serviceHandler = ServiceHandler;
            }
        }

        public WebSocket webSocket;
        private Dictionary<int, Publisher> publishers = new Dictionary<int, Publisher>();
        private Dictionary<int, Subscriber> subscribers = new Dictionary<int, Subscriber>();
        private Dictionary<int, ServiceCaller> serviceCallers = new Dictionary<int, ServiceCaller>();

        private void recievedOperation(object sender, MessageEventArgs e)
        {
            JObject operation = Deserialize(e.RawData);

#if DEBUG
          //  Console.WriteLine("Recieved " + operation.GetOperation());
          //  Console.WriteLine(JsonConvert.SerializeObject(operation, Formatting.Indented));
#endif

            switch (operation.GetOperation())
            {
                case "publish":
                    {
                        recievedPublish(operation, e.RawData);
                        return;
                    }
                case "service_response":
                    {
                        recievedServiceResponse(operation, e.RawData);
                        return;
                    }
            }
        }

        private void recievedServiceResponse(JObject serviceResponse, byte[] rawData)
        {
            ServiceCaller serviceCaller;
            bool foundById = serviceCallers.TryGetValue(serviceResponse.GetServiceId(), out serviceCaller);

            if (!foundById)
                serviceCaller = serviceCallers.Values.FirstOrDefault(x => x.service.Equals(serviceResponse.GetService()));


            JObject jObject = serviceResponse.GetValues();
            Type type = serviceCaller.objectType;
            if (type != null)
                serviceCaller.serviceHandler?.Invoke(jObject.ToObject(type));
            else
                serviceCaller.serviceHandler?.Invoke(jObject);
        }

        private void recievedPublish(JObject publication, byte[] rawData)
        {
            Subscriber subscriber;

            bool foundById = subscribers.TryGetValue(publication.GetServiceId(), out subscriber);

            if (!foundById)
                subscriber = subscribers.Values.FirstOrDefault(x => x.topic.Equals(publication.GetTopic()));

            subscriber.messageHandler?.Invoke((Message)publication.GetMessage().ToObject(subscriber.messageType));
        }

        private void sendOperation(Operation operation)
        {
#if DEBUG
            //     Console.WriteLine(JsonConvert.SerializeObject(operation, Formatting.Indented));
#endif
            try
            {
                webSocket.SendAsync(Serialize(operation), null);
            }
            catch { }
        }
        public static byte[] Serialize(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            byte[] buffer = Encoding.ASCII.GetBytes(json);
            int I = json.Length;
            int J = buffer.Length;
            return buffer;
        }

        public static JObject Deserialize(byte[] buffer)
        {
            string ascii = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            int I = ascii.Length;
            int J = buffer.Length;
            return JsonConvert.DeserializeObject<JObject>(ascii);
        }

        private static int generateId()
        {
            return Guid.NewGuid().GetHashCode();
        }
        #endregion       
    }
}
