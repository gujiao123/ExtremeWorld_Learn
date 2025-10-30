using UnityEngine;
using UnityEngine.UI;
//me 说是UIMainCity实际上每个地图都会用到 改为持久单例最好

public class UIMain : MonoSingleton<UIMain>
{
    public Text textName;
    public Text textLevel;
    //虽然是单例 但是内部组件数据没有更新

    protected override void OnStart()
    {
        Init();
    }
    void Init()
    {
        if (Models.User.Instance.CurrentCharacter != null)
        {
            textName.text = Models.User.Instance.CurrentCharacter.Name + " - " + Models.User.Instance.CurrentCharacter.Id;
            textLevel.text = "Lv." + Models.User.Instance.CurrentCharacter.Level.ToString();
        }
    }
    void Update()
    {

    }
    /// <summary>
    /// 退出游戏目前只是一个按钮的事件
    /// </summary>
    public void BackToCharSelect()
    {
        Debug.Log("谁返回啦SelectCharacter");
        Debug.LogWarningFormat("BackToCharSelect 被调用! 调用堆栈:\n{0}", System.Environment.StackTrace);
        SceneManager.Instance.LoadScene("SelectCharacter");
        Services.UserService.Instance.SendGameLeave();
    }

    /// <summary>
    /// 测试按钮
    /// </summary>
    public void OnClickTest()
    {
        Debug.Log("点击了测试按钮");
        UIManager.Instance.Show<UITest>();

    }
    public void OnClickBag()
    {
        UIManager.Instance.Show<UIBag>();
    }
}