
using Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//世界地图中的角色头顶信息

public class UINameBar : MonoBehaviour
{

    public Text avatarName;//这个手动设定



    public Character character;


    // Use this for initialization
    void Start()
    {
        if (this.character != null)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        this.UpdateInfo();
        //me 这个属于浮动UI 所以要面向摄像机 实时更新

        //me 这里意思就是 摄像机正方向和物体正方向保持一致 对的
        this.transform.forward = Camera.main.transform.forward;
    }
    /// <summary>
    /// 更新显示信息
    /// </summary>
    void UpdateInfo()
    {
        if (this.character != null)
        {
            string name = this.character.Name + " Lv." + this.character.Info.Level;
            if (name != this.avatarName.text)
            {
                this.avatarName.text = name;
            }
        }
    }
}