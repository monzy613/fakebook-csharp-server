using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace Fleck
{
    public class P2pServer
    {

        class MessageFormatForP2P {
            public string sender;
            public string receiver;
            public string content;
        }


        public static void Start()
        {

            JavaScriptSerializer serializer = new JavaScriptSerializer();


            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://0.0.0.0:8182");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Debug.WriteLine("Open!");
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Debug.WriteLine("Close!");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    var jsonObj = serializer.Deserialize<MessageFormatForP2P>(message);
                    Debug.WriteLine("[sender]: " + jsonObj.sender);
                    Debug.WriteLine("[receiver]: " + jsonObj.receiver);
                    Debug.WriteLine("[content]: " + jsonObj.content);
                    Debug.WriteLine("");
                    string msg = String.Format("{0}: {1}", jsonObj.sender, jsonObj.content);
                    string jsonStr = "{sender:'"
                                   + jsonObj.sender
                                   + "',receiver:'"
                                   + jsonObj.receiver
                                   + "',content:'"
                                   + jsonObj.content
                                   + "'}";
                    Debug.WriteLine("JSON to html: " + jsonStr);

                    allSockets.ToList().ForEach(s => s.Send(jsonStr));
                };
            });



        }
    }
}
