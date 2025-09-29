using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 就是角色选择界面中 角色职业信息显示 包含所有UI要素只管展示 Content下面的每一个角色信息
/// </summary>
public class UICharInfo : MonoBehaviour
{


    public SkillBridge.Message.NCharacterInfo info;

    public Text charClass;//职业
    public Text charName;//角色名称
    public Image highlight;

    /// <summary>
    /// 是否被选中了 就来展示高亮图片
    /// </summary>
    public bool Selected
    {
        get { return highlight.IsActive(); }
        set
        {
            highlight.gameObject.SetActive(value);
        }
    }

    // Use this for initialization
    void Start()
    {
        if (info != null)
        {
            this.charClass.text = this.info.Class.ToString();
            this.charName.text = this.info.Name;
        }
        else
        {
            Debug.Log("真的空的啊");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
