using Common;
using GameServer.Models;
using System.Collections.Generic;

namespace GameServer.Managers
{
    class MapManager : Singleton<MapManager>
    {
        //mapID管理所有Map
        Dictionary<int, Map> Maps = new Dictionary<int, Map>();

        public void Init()
        {
            //初始化所有地图
            foreach (var mapdefine in DataManager.Instance.Maps.Values)
            {
                Map map = new Map(mapdefine);

                Log.InfoFormat("MapManager.Init > Map:{0}:{1}", mapdefine.ID, mapdefine.Name);

                this.Maps[mapdefine.ID] = map;

            }
        }


        /// <summary>
        /// 语法简化
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Map this[int key]
        {
            get
            {
                return this.Maps[key];
            }
        }

        public void Update()
        {
            foreach (var maps in this.Maps.Values)
            {
                maps.Update();

            }

        }

    }
}
