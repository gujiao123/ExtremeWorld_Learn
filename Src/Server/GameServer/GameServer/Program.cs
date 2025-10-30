using Common;
using GameServer.Services;
using System;
using System.IO;
using System.Linq;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            FileInfo fi = new System.IO.FileInfo("log4net.xml");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(fi);
            Log.Init("GameServer");
            Log.Info("Game Server Init");

            GameServer server = new GameServer();
            server.Init();
            server.Start();

            TCharacter dbchar = DBService.Instance.Entities.Characters.Where(c => c.ID == 1).FirstOrDefault();
            //Log.InfoFormat("X;{0}Y:{1}Z:{2}", dbchar.MapPosX, dbchar.MapPosY, dbchar.MapPosZ);

            Console.WriteLine("Game Server Running......");
            CommandHelper.Run();
            Log.Info("Game Server Exiting...");
            server.Stop();
            Log.Info("Game Server Exited");
        }
    }
}
