using System.Collections;
using UnityEngine;

public class CamMover : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSensitivity;
    [SerializeField] private float zoomSensitivity;
    [SerializeField] private float zoomMin = 3;
    [SerializeField] private float coolDown = 1;
    
    private const float OrigHw = 1.1f;
    
    private Vector3 _offset;
    private CollisionTest _colTest;
    private bool _isCoroutineExecuting;
    private float _currentMag;
    private bool _isStopped;
    
    private void Start()
    {
        _offset = transform.position - target.position;
        _colTest = target.GetComponent<CollisionTest>();
    } 
        
    private void LateUpdate()
    {
        var width = OrigHw * target.localScale.x;
        
        var scroll = -Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity * target.localScale.x;
        if (scroll != 0)
        {
            var newOff = transform.position - target.position;
            var newMag = _offset.magnitude + scroll;
            if (scroll < 0)
            {
                StopCoroutine(StopGrowing());
                StartCoroutine(StopGrowing());
            }
            
            var realMag = newMag - width;
            if (realMag > zoomMin)
            {
                _offset = newOff.normalized * newMag;
                _currentMag = newMag;
            }
        }
        
        if (_colTest.isGrowing && !_isCoroutineExecuting)
        {
            _isCoroutineExecuting = true;
            StartCoroutine(IncreaseOffset());
        }

        var inX = Input.GetAxis("Mouse X");
        var inY = Input.GetAxis("Mouse Y");

        var newOffset = Quaternion.AngleAxis(inX * rotationSensitivity, Vector3.up) * Quaternion.AngleAxis(-inY * rotationSensitivity, transform.right) * _offset;
        
        var dotToGround = Vector3.Dot(-Vector3.up, transform.forward);
        
        if (dotToGround >= .9f && inY < 0 || dotToGround < 0.1f && inY > 0)
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

    IEnumerator StopGrowing()
    {
        _isStopped = true;
        yield return new WaitForSeconds(coolDown);
        _isStopped = false;
    }
    
    IEnumerator IncreaseOffset()
    {
        var vel = 0f;
        var scaleDif = _colTest.targScale - target.localScale.x;

        var mul = scaleDif < 1 ? 10 / scaleDif : 2f;
     
        var startingWidth = OrigHw * target.localScale.x;
        var endingWidth = OrigHw * _colTest.targScale;
        var distToSurface = _offset.magnitude - startingWidth;
        var distToSurfaceFinal = _offset.magnitude - endingWidth;
        _currentMag = _offset.magnitude;
        var dif = distToSurface - distToSurfaceFinal;
        var currentDif = 0f;

        while (dif - currentDif > 0.1)
        {
            if (_isStopped)            
                break;
            
            currentDif = Mathf.SmoothDamp(currentDif, dif, ref vel, 0.5f, 1000, Time.deltaTime);
            _offset = _offset.normalized * (_currentMag + currentDif * (scaleDif * mul));
            yield return new WaitForEndOfFrame();
        }

        _isCoroutineExecuting = false;
    }
}
