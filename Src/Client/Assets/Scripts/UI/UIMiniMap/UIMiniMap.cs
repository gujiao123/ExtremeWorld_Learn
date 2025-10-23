using Managers;
using Models;
using UnityEngine;
using UnityEngine.UI;

public class UIMiniMap : MonoBehaviour
{
    public Image miniMap;
    public Image Arrow;//代表玩家的地点和方向
    public Text mapName;
    public Collider miniMapCollider;
    public Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        InitMap();
    }

    //小地图切换都要初始化 就提取出来

    //TODO 后续优化UI和资源加载的逻辑 
    void InitMap()
    {
        this.mapName.text = User.Instance.CurrentMapData.Name;
        //这里吧小地图的加载逻辑放在manager里面去了
        this.miniMap.sprite = MiniMapManager.Instance.LoadCurrentSprite();

        miniMap.SetNativeSize();//设置图片的原始大小 我们使用的小地图的原始大小
        miniMap.transform.localPosition = Vector3.zero;//把小地图放在中心点  先重置一下

        //别人人物没有加载完你就拿他的Transform是不行的
        //this.playerTransform = User.Instance.CurrentCharacterObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        //!! 一般是UI先加载  玩家对象后加载 
        //me 这一步完全是执行顺序的问题 
        if (this.playerTransform == null || this.playerTransform.gameObject == null)
        {
            // 检查 CurrentCharacterObject 是否存在且未被销毁
            //!!退出游戏时候CurrentCharacterObject可能为空 顺序问题 问题不大 
            if (User.Instance.CurrentCharacterObject != null)
            {
                this.playerTransform = User.Instance.CurrentCharacterObject.transform;
            }
            else
            {
                // 如果玩家对象不存在，跳过本次更新
                return;
            }
        }

        // 再次检查 playerTransform 是否有效（双重保险）
        if (this.playerTransform == null)
        {
            return;
        }

        //!! 世界坐标转换到小地图的坐标
        float realWidth = miniMapCollider.bounds.size.x;
        float realHeight = miniMapCollider.bounds.size.z;

        //!! Collider一点用都没有  只是为了获取小地图的实际大小 也就是说 这个collider的大小要和小地图的大小是一样的 也就是说跟Collider这个脚本没什么关系
        //获取相对于Collider的坐标
        float relaX = playerTransform.position.x - miniMapCollider.bounds.min.x;
        float relaY = playerTransform.position.z - miniMapCollider.bounds.min.z;


        //计算中心点 就是把 小地图的pivot点 设置到玩家位置 这样显示的就是玩家为中心的小地图
        float pivotX = relaX / realWidth;//0-1
        float pivotY = relaY / realHeight;//0-1


        miniMap.rectTransform.pivot = new Vector2(pivotX, pivotY);
        miniMap.rectTransform.localPosition = Vector3.zero;//把小地图放在中心点

        //设置箭头的旋转  箭头的上方是0度  顺时针旋转
        //玩家的方向是Transform的y轴旋转  这里要取反 
        //人物是xzy 坐标系 UI是xyz坐标系 所以转换一下
        this.Arrow.rectTransform.localEulerAngles = new Vector3(0, 0, -playerTransform.eulerAngles.y);
    }
}
