using GameServer.Services;
using Network;
using System.Threading;
namespace GameServer
{
    class GameServer
    {
        NetService network;
        Thread thread;
        bool running = false;
        public bool Init()
        {
            //默认端口在配置文件中 默认8000
            int Port = Properties.Settings.Default.ServerPort;//就是8000
            network = new NetService();
            network.Init(Port);

            DBService.Instance.Init();
            UserService.Instance.Init();//初始化一个用于处理用户注册的服务

            thread = new Thread(new ThreadStart(this.Update));
            return true;
        }

        public void Start()
        {
            network.Start();

            running = true;
            thread.Start();



        }


        public void Stop()
        {
            network.Stop();

            running = false;
            thread.Join();
        }

        public void Update()
        {
            while (running)
            {
                Time.Tick();
                Thread.Sleep(100);
                //Console.WriteLine("{0} {1} {2} {3} {4}", Time.deltaTime, Time.frameCount, Time.ticks, Time.time, Time.realtimeSinceStartup);
            }
        }
    }
}
