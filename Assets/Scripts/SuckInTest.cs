using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SuckInTest : MonoBehaviour
{
    [SerializeField] private Transform eye;
    [SerializeField] private float suckingTime;
    [SerializeField] private float finalScale = 0.01f;
    [SerializeField] private float scaleDelta = 0.005f;
    

    private Vector3 _startPos;
    
    Vector3 _targetScale;
    Vector3 _moveVelocity = Vector3.zero;
    Vector3 _scaleVelocity = Vector3.zero;
    private bool _isActive;
    private IEnumerable<Transform> _children;
    
    private void Start()
    {
        _startPos = transform.position;
        _targetScale = new Vector3(finalScale, finalScale, finalScale);
        _children = GetComponentsInChildren<Transform>().Skip(1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // _isActive = true;
            
            StartCoroutine(ScaleDown());
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = _startPos;
            transform.localScale = Vector3.one;
            foreach (var child in _children)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator ScaleDown()
    {
        transform.GetChild(0).GetComponent<MeshCollider>().enabled = false;

        while (transform.localScale.x - finalScale > scaleDelta)
        {
            transform.position =
                Vector3.SmoothDamp(transform.position, eye.position, ref _moveVelocity, 0.1f, 1000, Time.deltaTime);
            transform.localScale =
                Vector3.SmoothDamp(transform.localScale, _targetScale, ref _scaleVelocity, 0.1f, 1000, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        
        foreach (var child in _children)
        {
            child.gameObject.SetActive(false);
        }
    }
}