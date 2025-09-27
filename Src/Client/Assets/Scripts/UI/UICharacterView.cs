using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICharacterView : MonoBehaviour
{
    public GameObject[] Characters;

    public int currentCharacter = 0;

    /// <summary>
    /// 设置展示的角色索引
    /// </summary>
    public int CurrentCharacter
    {
        get { return currentCharacter; }
        set
        {
            currentCharacter = value;
            UpdateCharacterDisplay();
        }
    }

    void Start()
    {

    }


    /// <summary>
    /// 更新角色展示只有第currentCharacter才设置为激活状态
    /// </summary>
    private void UpdateCharacterDisplay()
    {
        for (int i = 0; i < Characters.Length; i++)
        {
            Characters[i].SetActive(i == currentCharacter);
        }
    }

}



