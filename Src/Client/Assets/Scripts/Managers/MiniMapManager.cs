



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
        public Transform PlayerTransform
        {
            get
            {
                if (User.Instance.CurrentCharacterObject == null)
                    return null;
                return User.Instance.CurrentCharacterObject.transform;
            }
        }
        public UIMinimap minimap;

        //这个就只该 mapcontroller调用
        public Sprite LoadCurrentSprite()
        {
            Debug.LogFormat("LoadCurrentSprite:MiniMap:{0}", User.Instance.CurrentMapData.MiniMap);
            Sprite sprite = Resources.Load<Sprite>($"UI/MiniMap/{User.Instance.CurrentMapData.MiniMap}");
            return sprite;
        }




        //!!下面的两个函数就是让UIminimap 和 MapControlle 握手逻辑 都要有才执行
        /// <summary>
        /// MapController 调用，用来提供 Collider
        /// </summary>
        public void UpdateMinimap(Collider collider)
        {
            this.minimapCollider = collider;

            // 检查 UIMinimap 是否已经注册
            if (this.minimap != null)
            {
                // UI 和 Collider 都已就位，执行更新
                minimap.UpdateMap();
            }
        }

        /// <summary>
        /// UIMinimap 调用，用来注册它自己
        /// </summary>
        public void RegisterMinimap(UIMinimap ui)
        {
            this.minimap = ui;

            // 检查 Collider 是否已经送达
            if (this.MiniMapCollider != null)
            {
                // UI 和 Collider 都已就位，执行更新
                this.minimap.UpdateMap();
            }
        }
    }

}