using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    Define.CameraMode _mode = Define.CameraMode.TopView;

    [SerializeField]
    Vector3 _delta;

    [SerializeField]
    GameObject Player;

    void LateUpdate()
    {
        if (_mode == Define.CameraMode.TopView)
        {
            transform.position = Player.transform.position + _delta;
            transform.LookAt(Player.transform);
        }
    }
}