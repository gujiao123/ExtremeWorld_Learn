using Common.Data;
using Managers;
using Models;
using System.Collections;
using UnityEngine;



//每个物体都有

public class NPCController : MonoBehaviour
{
    public int npcID;
    SkinnedMeshRenderer render;
    Animator anim;
    NpcDefine npc;
    Color orignColor;//原始颜色 用于高亮后的还原
    private bool inInteractive = false;

    void Start()
    {
        render = this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        anim = this.gameObject.GetComponent<Animator>();
        npc = NpcManager.Instance.GetNpcDefine(npcID);
        orignColor = render.sharedMaterial.color;
        this.StartCoroutine(Actions());
    }

    IEnumerator Actions()
    {
        while (true)
        {
            if (inInteractive)
                yield return new WaitForSeconds(2f);
            else
                yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 10f));
            this.Relax();
        }
    }



    void Interactive()
    {
        //me 防止连续点击
        if (!inInteractive)
        {
            inInteractive = true;
            this.StartCoroutine(DoInteractive());
        }
    }
    IEnumerator DoInteractive()
    {
        yield return FaceTolayer();
        //把交互请求发给manager
        if (NpcManager.Instance.Interactive(npc))
        {
            anim.SetTrigger("Talk");
        }
        yield return new WaitForSeconds(3f);//防止重复点击
        inInteractive = false;

    }



    IEnumerator FaceTolayer()
    {
        Vector3 faceTo = (User.Instance.CurrentCharacterObject.transform.position - this.transform.position).normalized;
        while (Mathf.Abs(Vector3.Angle(this.gameObject.transform.forward, faceTo)) > 5)
        {
            //插值转向
            this.gameObject.transform.forward = Vector3.Lerp(this.gameObject.transform.forward, faceTo, Time.deltaTime * 5f);
            yield return null;
        }
    }
    /// <summary>
    /// 鼠标点击事件
    /// </summary>
    private void OnMouseDown()
    {
        Interactive();
    }
    private void Relax()
    {
        anim.SetTrigger("Relax");
    }

    void Update()
    {

    }
    /// <summary>
    /// 鼠标悬停
    /// </summary>
    private void OnMouseOver()
    {
        Highlight(true);
    }
    /// <summary>
    /// 鼠标移入
    /// </summary>
    private void OnMouseEnter()
    {
        Highlight(true);
    }
    /// <summary>
    /// 鼠标移出
    /// </summary>
    private void OnMouseExit()
    {
        Highlight(false);
    }


    void Highlight(bool highlight)
    {

        Debug.Log("鼠标进入了");
        if (highlight)
        {
            if (render.sharedMaterial.color != Color.white)
                render.sharedMaterial.color = Color.white;
        }
        else
        {
            if (render.sharedMaterial.color != orignColor)
                render.sharedMaterial.color = orignColor;
        }
    }
}