using Managers;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestStatus : MonoBehaviour
{
    public Image[] statusImages;
    private NpcQuestStatus questStatus;
    /// <summary>
    /// 让NPC 决定自己头上是什么
    /// </summary>
    /// <param name="status"></param>
    //UI只负责自己的更新 
    public void SetQuestStatus(NpcQuestStatus status)
    {
        this.questStatus = status;

        for (int i = 0; i < 4; i++)
        {
            if (this.statusImages[i] != null)
            {
                this.statusImages[i].gameObject.SetActive(i == (int)status);
            }
        }
    }
}

