﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.TaskTest
{
    public class TaskTest01
    {
        Thread t = null;
        ManualResetEvent manualEvent = new ManualResetEvent(false);//为true,一开始就可以执行

        public TaskTest01()
        {
            t = new Thread(this.Run);
            t.Start();
        }

        private void Run()
        {
            while (true)
            {
                 bool result = this.manualEvent.WaitOne(5000, false); //如果10秒
                Console.WriteLine("这里是:  {0}", result.ToString());
                //else
                //    Console.WriteLine("这里是2:  {0}", Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(1000);
            }
        }

        public void Start()
        {
            this.manualEvent.Set();
        }

        public void Stop()
        {
            this.manualEvent.Reset();
        }

        public static void Test()
        {
            TaskTest01 myt = new TaskTest01();
            while (true)
            { 
                Console.WriteLine("输入 stop后台线程挂起 start 开始执行！");
                string str = Console.ReadLine();
                if (str.ToLower().Trim() == "stop")
                    myt.Stop();
                if (str.ToLower().Trim() == "start")
                    myt.Start();
            }
        }

    }
}
