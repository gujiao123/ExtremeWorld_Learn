using GameServer.Models;
using System.Collections.Generic;

namespace GameServer.Managers
{
    class SpawnManager
    {
        //读取刷怪的规则
        //刷怪规则列表 怪物可以随机在里面选择地点刷怪
        private List<Spawner> Rules = new List<Spawner>();
        private Map Map;
        //根据地图ID初始化刷怪规则
        public void Init(Map map)
        {
            this.Map = map;
            if (DataManager.Instance.SpawnRules.ContainsKey(map.ID))
            {
                foreach (var define in DataManager.Instance.SpawnRules[map.Define.ID].Values)
                {
                    this.Rules.Add(new Spawner(define, this.Map));
                }
            }
        }

        public void Update()
        {
            if (Rules.Count == 0)
                return;

            for (int i = 0; i < Rules.Count; i++)
            {
                this.Rules[i].Update();
            }
        }
    }
}
