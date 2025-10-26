



//UI里面不做复杂逻辑 全靠Manager来驱动实现

using Models;
using UnityEngine;
//me 一切UI小地图资源入口 让别人通过这个Manager来获取资源  
namespace Managers
{
    class MiniMapManager : Singleton<MiniMapManager>
    {
        //负责资源的管理中介
        private Collider minimapCollider;
        public Collider MiniMapCollider
        {
            get { return minimapCollider; }

        }

        public UIMiniMap minimap;

        public Sprite LoadCurrentSprite()
        {
            Debug.LogFormat("LoadCurrentSprite:MiniMap:{0}", User.Instance.CurrentMapData.MiniMap);
            return Resources.Load<Sprite>($"UI/MiniMap/{User.Instance.CurrentMapData.MiniMap}");
        }
        /// <summary>
        /// 接口 让别人通知自己更新碰撞体 和小地图
        /// </summary>
        /// <param name="collider"></param>
        public void UpdateMinimap(Collider collider)
        {
            this.minimapCollider = collider;
            if (this.minimap != null)
            {
                //这个函数里面 minimap 通过manager请求数据来更新 碰撞体
                minimap.UpdateMap();
            }
        }
    }

}