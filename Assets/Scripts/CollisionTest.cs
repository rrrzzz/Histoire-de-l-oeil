using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class CollisionTest : MonoBehaviour
{
    [SerializeField] private float volumeDivisor = 10f;
    [SerializeField] private float scaleDelta = 0.005f;
    [SerializeField] private float growingTime = 0.5f;
    [SerializeField] private Transform[] createdObjects;
    

    private Transform _currentTr;
    private Vector3 _colOriginalPos;
    public bool isGrowing;
    public float tScale; 

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _currentTr.position = _colOriginalPos;
            _currentTr.localScale = Vector3.one;

            foreach (var child in _currentTr.GetComponentsInChildren<Transform>(true).Skip(1))
            {
                child.gameObject.SetActive(true);
            }
            transform.localScale = Vector3.one;
            _currentTr.GetChild(0).GetComponent<MeshCollider>().enabled = true;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            var volume = 0f;
            foreach (var o in createdObjects)
            {
                var mr = o.GetComponentInChildren<MeshRenderer>();
                
                var meshGlobalVolume = mr.bounds.size;
                volume += Mathf.Pow(meshGlobalVolume.x * meshGlobalVolume.y * meshGlobalVolume.z, 1f / 3f);
                
                StartCoroutine(ScaleDown(mr.transform));
            }
            
            StartCoroutine(ScaleUp(volume));
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        var tr = other.transform;
        var name = tr.name.ToLower();
        
        if (name.Contains("eye") || name.Contains("floor") || name.Contains("wall") || name.Contains("camera") || !tr.gameObject.activeInHierarchy)
            return;
        
        var meshGlobalVolume = tr.GetComponent<MeshRenderer>().bounds.size;
        var volume = meshGlobalVolume.x * meshGlobalVolume.y * meshGlobalVolume.z;
       
        StartCoroutine(ScaleDown(tr));
        StartCoroutine(ScaleUp(Mathf.Pow(volume, 1f / 3f)));
    }

    private IEnumerator ScaleUp(float volume)
    {
        var scaleVelocity = Vector3.zero;
        var targetScale = transform.localScale.x + volume / volumeDivisor;
        tScale = targetScale;
        var finalScale = GetFinalScale(targetScale);
        isGrowing = true;
        while (targetScale - transform.localScale.x > scaleDelta)
        {
            transform.localScale =
                Vector3.SmoothDamp(transform.localScale, finalScale, ref scaleVelocity, growingTime, 1000, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        isGrowing = false;
    }
    private IEnumerator ScaleDown(Transform tr)
    {
        var moveVelocity = Vector3.zero;
        var scaleVelocity = Vector3.zero;
        var finalScale = 0.01f;
        var targetScale = GetFinalScale(finalScale);
        var suckingTime = 0.1f;
        
        var isSingleObject = char.IsUpper(tr.name[0]) || tr.name.ToLower().Contains("alt");
        
        _currentTr = isSingleObject ? tr : tr.parent;
        _colOriginalPos = _currentTr.position;

        var children = _currentTr.GetComponentsInChildren<Transform>().Skip(1);
        tr.GetComponent<MeshCollider>().enabled = false;
        
        while (_currentTr.localScale.x - finalScale > scaleDelta)
        {
            _currentTr.position =
                Vector3.SmoothDamp(_currentTr.position, transform.position, ref moveVelocity, suckingTime, 1000, Time.deltaTime);
            _currentTr.localScale =
                Vector3.SmoothDamp(_currentTr.localScale, targetScale, ref scaleVelocity, suckingTime, 1000, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        
        foreach (var child in children)
        {
            child.gameObject.SetActive(false);
        }
    }

    private Vector3 GetFinalScale(float s) => new Vector3(s, s, s);
}
