using UnityEngine;

public class CamMover : MonoBehaviour
{
    private const float MinYPos = -2.5f;
    private const float MaxYPos = 4;

    [SerializeField] private Transform target;
    [SerializeField] private float rotationSensitivity;
    [SerializeField] private float zoomSensitivity;
    [SerializeField] private float zoomMax = 45;
    [SerializeField] private float zoomMin = 3;
    private Vector3 _offset;
    private float _defMag;
    private void Start()
    {
        _offset = transform.position - target.transform.position;
        _defMag = _offset.magnitude;
    } 
        
    private void LateUpdate()
    {
        var scroll = -Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
        if (scroll != 0)
        {
            var newOff = transform.position - target.transform.position;

            var newMag = newOff.magnitude + scroll;
            {
                if (newMag > zoomMin && newMag < zoomMax)
                {
                    _offset = newOff.normalized * newMag;
                }
            }
        }
        
        var inX = Input.GetAxis("Mouse X");
        var inY = Input.GetAxis("Mouse Y");

        var newOffset = Quaternion.AngleAxis (inX * rotationSensitivity, Vector3.up) * Quaternion.AngleAxis(-inY * rotationSensitivity, transform.right) * _offset;
        var pos = target.position + newOffset;

        if (pos.y < MinYPos * (_defMag / _offset.magnitude) * target.localScale.x || pos.y > MaxYPos * (_offset.magnitude / _defMag) * target.localScale.x)
        {
            _offset = Quaternion.AngleAxis (inX * rotationSensitivity, Vector3.up) * _offset;
        }
        else
        {
            _offset = newOffset;
        }

        transform.position = target.position + _offset;
        transform.LookAt(target.position);
    }
}
