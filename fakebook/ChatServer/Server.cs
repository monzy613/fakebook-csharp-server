using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace Fleck
{
    public class Server
    {

        class MessageFormat {
            public string username;
            public string nickname;
            public string content;
        }


        public static void Start()
        {

            JavaScriptSerializer serializer = new JavaScriptSerializer();


            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://0.0.0.0:8181");
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
                    var jsonObj = serializer.Deserialize<MessageFormat>(message);
                    Debug.WriteLine("[username]: " + jsonObj.username);
                    Debug.WriteLine("[nickname]: " + jsonObj.nickname);
                    Debug.WriteLine("[content]: " + jsonObj.content);
                    Debug.WriteLine("");
                    string msg = String.Format("{0}: {1}", jsonObj.nickname, jsonObj.content);
                    allSockets.ToList().ForEach(s => s.Send(msg));
                };
            });



        }
    }
}
