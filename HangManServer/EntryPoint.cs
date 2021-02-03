using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HangManServer
{
    class EntryPoint
    {
        static private int server_port = 5050;

        static void Main(string[] args)
        {
            AsynchronousSocketListener.StartListening(server_port);
        }     
    }
}
