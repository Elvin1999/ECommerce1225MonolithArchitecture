using App.Business.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App.Server
{
    public class Program
    {
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 27001;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        static void Main(string[] args)
        {
            //Console.Title = "Server";
            //SetupServer();
            //Console.ReadLine();
            //CloseAllSockets();

            GetAllServicesAsText();
        }

        private static void CloseAllSockets()
        {
            throw new NotImplementedException();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server . . .");
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse("10.1.18.1"), PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket socket;
            try
            {
                socket = serverSocket.EndAccept(ar);
            }
            catch (Exception ex)
            {
                return;
            }

            SendServiceResponseToClient(socket);
            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallBack, socket);
            Console.WriteLine("Client connected, waiting for request . . . ");
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallBack(IAsyncResult ar)
        {

        }

        private static void SendServiceResponseToClient(Socket client)
        {
            var result = GetAllServicesAsText();
            byte[] data = Encoding.ASCII.GetBytes(result);
            client.Send(data);
        }

        private static string GetAllServicesAsText()
        {
            var myTypes = Assembly.GetAssembly(typeof(ProductService)).GetTypes()
                .Where(t => t.Name.EndsWith("Service") && !t.Name.StartsWith("I"));

            var sb = new StringBuilder();
            foreach (var type in myTypes)
            {
                var className = type.Name.Remove(type.Name.Length - 7, 7);
                var methods = type.GetMethods().Reverse().Skip(4);

                foreach (var m in methods)
                {
                    string responseText = $@"{className}\{m.Name}";
                    var parameters=m.GetParameters();
                    foreach (var param in parameters)
                    {
                        if(param.ParameterType != typeof(string) && param.ParameterType.IsClass) {
                            responseText += $@"\{param.Name}[json]";
                        }
                        else
                        {
                            responseText += $@"\{param.Name}";
                        }
                    }
                    sb.AppendLine(responseText);
                }
            }
            var result=sb.ToString();
            return result;

        }
    }
}

