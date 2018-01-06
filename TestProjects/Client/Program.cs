using System;
using System.Text;
using NetworkCore.IOCP;
using NetworkCore.Utility;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            IOPCTest.IOPCTest.Client();
            //TaskTest.TaskTest01.Test();
        }
    }
}
