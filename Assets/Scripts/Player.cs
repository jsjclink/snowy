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

        // transform.position += moveDirection * moveSpeed * Time.deltaTime;
        // moveDirection = new Vector3(x, y, 0);

        if (x != 0 && y != 0)
        {
            transform.position += new Vector3(x, y, 0) * moveSpeed * 0.625f * Time.deltaTime;
        }
        else
        {
            transform.position += new Vector3(x, y, 0) * moveSpeed * Time.deltaTime;
            transform.position += new Vector3(jx, jy, 0) * moveSpeed * Time.deltaTime;
        }

    }
}