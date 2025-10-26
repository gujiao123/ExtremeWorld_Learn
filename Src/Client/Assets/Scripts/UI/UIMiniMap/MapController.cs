using Managers;
using UnityEngine;


//me  当作场景与脚本的桥梁 负责把场景中的碰撞体传递给MiniMapManager

public class MapController : MonoBehaviour
{

    public Collider mapCollider;
    //每个地图 
    private void Start()
    {
        //小地图通过这里切换 
        MiniMapManager.Instance.UpdateMinimap(mapCollider);
    }
}