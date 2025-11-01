using UnityEngine;
using UnityEngine.UI;
//只负责高亮 效果

//其余逻辑都由tabView处理 如果一个数据结构一般
public class TabButton : MonoBehaviour
{

    public Sprite activeImage;
    private Sprite normalImage;

    public TabView tabView;

    public int tabIndex = 0;//由tabView 赋值 方便tabView识别是哪个tabButton
    public bool selected = false;

    private Image tabImage;

    // Use this for initialization
    void Start()
    {
        tabImage = this.GetComponent<Image>();
        normalImage = tabImage.sprite;
        //绑定按钮 事件 点击就是选择这个tabView绑定的
        this.GetComponent<Button>().onClick.AddListener(OnClick);
    }


    /// <summary>
    /// 按钮点击事件
    /// </summary>
    /// 就连你的回调函数也是由tabView来生成的分配啊
    void OnClick()
    {
        //但是还是由tabView来处理选择逻辑
        //!!不能自己处理 交给tabView 还能统筹其他tabButton的选中状态
        this.tabView.SelectTab(this.tabIndex);
    }

    /// <summary>
    /// 由tabView调用 设置选中状态
    /// </summary>
    /// <param name="select"></param>
    public void Select(bool select)
    {
        tabImage.overrideSprite = select ? activeImage : normalImage;
    }
}
