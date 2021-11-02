using System.Collections;
using UnityEngine;

public class CamMover : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSensitivity;
    [SerializeField] private float zoomSensitivity;
    [SerializeField] private float zoomMin = 3;
    [SerializeField] private float coolDown = 1;
    [SerializeField] private float scaleOffset = 2;
    [SerializeField] private float scaleSmallMul = 10;
    
    
    private const float OrigHw = 1.1f;
    
    private Vector3 _offset;
    private CollisionTest _colTest;
    private bool _isCoroutineExecuting;
    private float _currentMag;
    private float _lastTargetReached;
    private bool _isStopped;
    private bool _changeScale = true;
    
    private void Start()
    {
        _offset = transform.position - target.position;
        _colTest = target.GetComponent<CollisionTest>();
        _lastTargetReached = target.localScale.x;
        
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

        if (_changeScale)
        {
            if (_colTest.growGradually)
            {
                if ((_colTest.isGrowing || (target.localScale.x - _lastTargetReached > 0.01f)) && !_isCoroutineExecuting)
                {
                    _isCoroutineExecuting = true;
                    StartCoroutine(IncreaseOffset());
                }
            }
            else
            {
                StartCoroutine(IncreaseOffsetLast());
            }
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
    
    IEnumerator IncreaseOffsetLast()
    {
        _changeScale = false;
        var vel = 0f;
        var startingWidth = OrigHw * _colTest.lastScale;
        var endingWidth = OrigHw * _colTest.targScale;
        var distToSurface = _offset.magnitude - startingWidth;
        var distToSurfaceFinal = _offset.magnitude - endingWidth;
        _currentMag = _offset.magnitude;
        var dif = distToSurface - distToSurfaceFinal;
        dif *= 3;
        var currentDif = 0f;

        while (dif - currentDif > 0.1)
        {
            if (_isStopped)
            {
                _isCoroutineExecuting = false;
                yield break;
            }                
            
            currentDif = Mathf.SmoothDamp(currentDif, dif, ref vel, 0.5f, 1000, Time.deltaTime);
            _offset = _offset.normalized * (_currentMag + currentDif);
            yield return new WaitForEndOfFrame();
        }
        
        _isCoroutineExecuting = false;
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
        var scaleDif = _colTest.targScale - _lastTargetReached;
        if (scaleDif < 0.001f)
        {
            _isCoroutineExecuting = false;
            yield break;
        }
        var startingScale = _colTest.targScale;
        var mul = scaleDif < 1 ? scaleSmallMul / scaleDif : scaleOffset;
        var startingWidth = OrigHw * target.localScale.x;
        var endingWidth = OrigHw * _colTest.targScale;
        var distToSurface = _offset.magnitude - startingWidth;
        var distToSurfaceFinal = _offset.magnitude - endingWidth;
        _currentMag = _offset.magnitude;
        var dif = distToSurface - distToSurfaceFinal;
        var currentDif = 0f;

        while (dif > 0.001f && dif - currentDif > 0.1)
        {
            if (_isStopped)
            {
                _isCoroutineExecuting = false;
                yield break;
            }                
            
            currentDif = Mathf.SmoothDamp(currentDif, dif, ref vel, 0.5f, 1000, Time.deltaTime);
            _offset = _offset.normalized * (_currentMag + currentDif * (scaleDif * mul));
            yield return new WaitForEndOfFrame();
        }
        
        _lastTargetReached = startingScale;
        _isCoroutineExecuting = false;
    }
}
