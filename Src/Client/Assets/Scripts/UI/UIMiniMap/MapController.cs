using Managers;
using UnityEngine;


//me  当作场景与脚本的桥梁 负责把场景中的碰撞体传递给MiniMapManager


///!!必须更改脚本顺序这个要在minimap初始化前面
public class MapController : MonoBehaviour
{
    //你负责在每个场景中获得提前布置好的碰撞体
    public Collider mapCollider;
    //每个地图 
    private void Start()
    {
        //小地图通过这里切换 
        MiniMapManager.Instance.UpdateMinimap(mapCollider);
    }
}