using UnityEngine;

public class MainPlayerCamera : MonoSingleton<MainPlayerCamera>
{
    public Camera camera;

    public Transform viewPoint;
    public Vector3 vector3Offset = new Vector3(0, 2, -4);
    public GameObject player;

    private void LateUpdate()
    {
        if (player == null)
            return;

        this.transform.position = player.transform.position - player.transform.forward * vector3Offset.z + Vector3.up * vector3Offset.y;
        this.transform.rotation = player.transform.rotation;
    }
}