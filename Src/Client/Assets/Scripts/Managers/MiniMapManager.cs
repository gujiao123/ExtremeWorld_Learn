



//UI里面不做复杂逻辑 全靠Manager来驱动实现

using Models;
using UnityEngine;

namespace Managers
{
    class MiniMapManager : Singleton<MiniMapManager>
    {
        public Sprite CurrentMapSprite { get; set; }
        public Sprite LoadCurrentSprite()
        {
            Debug.LogFormat("LoadCurrentSprite:MiniMap:{0}", User.Instance.CurrentMapData.MiniMap);
            return Resources.Load<Sprite>($"UI/MiniMap/{User.Instance.CurrentMapData.MiniMap}");
        }
    }

}