using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed;

    private void FixedUpdate()
    {
        var currentCamRot = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up);
        
        var hor = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");

        var dir = currentCamRot * Vector3.forward * vert + currentCamRot * Vector3.right * hor;
        dir.Normalize();
        rb.AddForce(dir * speed * Time.deltaTime);
    }
}
