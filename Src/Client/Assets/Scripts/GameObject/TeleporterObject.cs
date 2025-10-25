using System.Collections;
using System.Collections.Generic;
using Common.Data;
using Services;
using UnityEngine;

public class TeleporterObject : MonoBehaviour
{
    public int ID;//区分不同的传送点

    Mesh mesh = null;


    void Start()
    {
        this.mesh = this.GetComponent<MeshFilter>().sharedMesh;
    }
    //编辑器扩展显示效果
#if UNITY_EDITOR
    //这里相当于我们 不需要meshrender来显示物体了 ,自己画出来 
    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        if (mesh == null)
        {
            Gizmos.DrawWireMesh(this.mesh, transform.position * this.transform.localPosition.y * 0.5f, transform.rotation, transform.localScale);
        }
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.ArrowHandleCap(0, transform.position, transform.rotation, 1.0f, EventType.Repaint);
    }
#endif


    void OnTriggerEnter(Collider other)
    {
        Debug.LogFormat("OnTriggerEnter 被触发:{0}, Tag:{1}", other.name, other.tag);
        PlayerInputController controller = other.GetComponent<PlayerInputController>();
        if (controller != null && controller.isActiveAndEnabled)
        {
            TeleporterDefine teleporter = DataManager.Instance.Teleporters[ID];
            //查看是有否有这个传送点 的定义
            if (teleporter == null)
            {
                Debug.LogErrorFormat("传送点数据不存在 id={0}", this.ID);
                return;

            }
            //是否有链接的传送点
            if (teleporter.LinkTo > 0)
            {
                //检查要传送的地图是否存在
                if (DataManager.Instance.Teleporters.ContainsKey(teleporter.LinkTo))
                {
                    //切换地图

                    MapService.Instance.SendMapTeleport(this.ID);
                }
                else
                {
                    Debug.LogErrorFormat("传送点链接的地图不存在 id={0}", teleporter.LinkTo);
                }
            }

        }
    }
}
