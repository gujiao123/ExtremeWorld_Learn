using UnityEngine;
using UnityEngine.UI;


public class UIMainCity : MonoBehaviour
{
    public Text textName;
    public Text textLevel;

    void Start()
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
}