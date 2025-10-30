using Managers;
using Models;
using UnityEngine;
using UnityEngine.UI;

public class UIMinimap : MonoBehaviour
{

    public Collider minimapBoudingBox;
    /// <summary>
    /// image是一个组件 一个图片对应的是srite这个没有设定自己再场景中设定
    /// </summary>
    public Image minimap;
    public Image arrow;
    public Text mapName;
    private Transform playerTransform;

    void Start()
    {
        MiniMapManager.Instance.minimap = this;
        //this.UpdateMap();

        MiniMapManager.Instance.RegisterMinimap(this);
    }
    //只是更新地图信息 不需要实时更新
    public void UpdateMap()
    {
        this.mapName.text = User.Instance.CurrentMapData.Name;
        this.minimap.overrideSprite = MiniMapManager.Instance.LoadCurrentSprite();

        this.minimap.SetNativeSize();
        this.minimap.transform.localPosition = Vector3.zero;
        this.minimapBoudingBox = MiniMapManager.Instance.MiniMapCollider;
        this.playerTransform = null;
    }

    void Update()
    {
        if (playerTransform == null)
            playerTransform = MiniMapManager.Instance.PlayerTransform;

        if (minimapBoudingBox == null || playerTransform == null)
            return;

        float realWidth = minimapBoudingBox.bounds.size.x;
        float realHeight = minimapBoudingBox.bounds.size.z;

        float relaX = playerTransform.position.x - minimapBoudingBox.bounds.min.x;
        float relaY = playerTransform.position.z - minimapBoudingBox.bounds.min.z;

        float pivotX = relaX / realWidth;
        float pivotY = relaY / realHeight;

        this.minimap.rectTransform.pivot = new Vector2(pivotX, pivotY);
        this.minimap.rectTransform.localPosition = Vector2.zero;
        this.arrow.transform.eulerAngles = new Vector3(0, 0, -playerTransform.eulerAngles.y);
    }
}
