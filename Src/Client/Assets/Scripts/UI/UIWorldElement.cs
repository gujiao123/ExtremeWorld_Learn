using UnityEngine;

public class UIWorldElement : MonoBehaviour
{


    //UI拥有者 作为位置参照
    public Transform owner;

    public float height = 1.5f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (owner != null)
        {
            this.transform.position = owner.position + Vector3.up * height;
        }
        //这下将属于世界UI的元素始终面向摄像机
        if (Camera.main != null)
        {
            this.transform.forward = Camera.main.transform.forward;
        }
    }
}