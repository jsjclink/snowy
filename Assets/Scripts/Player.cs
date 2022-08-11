using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private VirtualJoystick virtualJoystick;
    public float moveSpeed = 5.0f;
    // Update is called once per frame

    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        float jx = virtualJoystick.Horizontal;
        float jy = virtualJoystick.Vertical;

        float ag = virtualJoystick.PlayerAngle;


        this.transform.rotation = Quaternion.AngleAxis(ag-90, Vector3.forward);

        if (x != 0 && y != 0)
        {
            transform.position += new Vector3(x, y, 0) * moveSpeed * 0.7071f * Time.deltaTime;
        }
        else
        {
            transform.position += new Vector3(x, y, 0) * moveSpeed * Time.deltaTime;
            transform.position += new Vector3(jx, jy, 0) * moveSpeed * Time.deltaTime;
        }

    }
}