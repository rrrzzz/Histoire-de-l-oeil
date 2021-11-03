using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTest : MonoBehaviour
{
    [HideInInspector] public bool isGrowing;
    [HideInInspector] public float targScale;
    [HideInInspector] public float lastScale;
    [HideInInspector] public bool growGradually = true;
    
    [SerializeField] private float volumeDivisor = 10f;
    [SerializeField] private float scaleDelta = 0.005f;
    [SerializeField] private float growingTime = 0.5f;
    [SerializeField] private float suckingTime = 0.1f;
    [SerializeField] private Transform dupeParent;

    private bool _wasHit;
    private bool _isSuckingAll;
    private float _totalHeight; 
   
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.I)) return;
        SuckAll();
    }

    private void SuckAll()
    {
        _isSuckingAll = true;
        var volume = 0f;
        suckingTime = 1f;
        List<Transform> children = new List<Transform>();
        
        for (var i = 0; i < dupeParent.childCount; i++)
        {
            children.Add(dupeParent.GetChild(i));
        }

        foreach (var o in children)
        {
            var mr = o.GetComponentInChildren<MeshRenderer>();
            var meshGlobalVolume = mr.bounds.size;
            var currentVol = meshGlobalVolume.x * meshGlobalVolume.y * meshGlobalVolume.z;
            volume += Mathf.Pow(currentVol, 1f / 3f);
            StartCoroutine(ScaleDown(o));
        }
        
        Debug.Log(volume);
        var finalVol = volume / 100;
        targScale = transform.localScale.x + finalVol;
        lastScale = transform.localScale.x;
        growGradually = false;
        StartCoroutine(ScaleUp(volume / 10));
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var tr = other.transform;
        if (tr.CompareTag(Constants.UnsuckTag) || tr.CompareTag(Constants.FloorTag) || tr.CompareTag(Constants.ProcessedTag) || !tr.gameObject.activeInHierarchy)
            return;

        if (tr.CompareTag(Constants.RoofTag))
        {
            SuckAll();
            return;
        }
        
        tr.tag = Constants.ProcessedTag;
        
        ProcessCollision(tr);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Blocker"))
        {
            _wasHit = true;
            StartCoroutine(DisableCollider(other.transform));
        }
    }

    IEnumerator DisableCollider(Transform tr)
    {
        yield return new WaitForSeconds(.5f);
        tr.GetComponent<MeshCollider>().enabled = false;
    }

    public void ProcessCollision(Transform tr)
    {
        if (_isSuckingAll)
            return;
        
        Debug.Log(tr.name);
        var prevParent = tr;
        var currentParent = tr.parent;
        while (currentParent.name != "Dupes")
        {
            prevParent = currentParent;
            currentParent = currentParent.parent;
        }
        
        tr = prevParent;

        if (tr.CompareTag(Constants.UnsuckTag))
            return;

        tr.tag = Constants.UnsuckTag;
        var mr = tr.GetComponentInChildren<MeshRenderer>();
        var meshGlobalVolume = mr.bounds.size;
        
        Debug.Log(_totalHeight);
        var volume = meshGlobalVolume.x * meshGlobalVolume.y * meshGlobalVolume.z;
        StartCoroutine(ScaleDown(tr));
        StartCoroutine(ScaleUp(Mathf.Pow(volume, 1f / 3f)));
    }
    
    private IEnumerator ScaleUp(float volume)
    {
        var scaleVelocity = Vector3.zero;
        var targetScale = transform.localScale.x + volume / volumeDivisor;
        _totalHeight += targetScale;
        targScale = Mathf.Max(targetScale, targScale);
        var finalScale = GetFinalScale(targetScale);
        while (targetScale - transform.localScale.x > scaleDelta)
        {
            isGrowing = true;
            transform.localScale =
                Vector3.SmoothDamp(transform.localScale, finalScale, ref scaleVelocity, growingTime, 1000, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        isGrowing = false;
    }
    
    private IEnumerator ScaleDown(Transform tr)
    {
        if (!tr)
        {
            yield break;
        }
        
        var moveVelocity = Vector3.zero;
        var scaleVelocity = Vector3.zero;
        
        
        var finalScale = tr.localScale.x * 0.1f;
        var targetScale = GetFinalScale(finalScale);
        
        var meshCols = tr.GetComponentsInChildren<MeshCollider>();
        
        foreach (var mc in meshCols)
        {
            mc.enabled = false;
        }
        
        if (!tr)
        {
            yield break;
        }
        
        while (tr.localScale.x - finalScale > finalScale * 0.1f)
        {
            if (!tr)
            {
                yield break;
            }
            tr.position =
                Vector3.SmoothDamp(tr.position, transform.position, ref moveVelocity, suckingTime, 1000, Time.deltaTime);
            tr.localScale =
                Vector3.SmoothDamp(tr.localScale, targetScale, ref scaleVelocity, suckingTime, 1000, Time.deltaTime);
            
            if (!tr)
            {
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }

        Destroy(tr.gameObject);
    }

    private Vector3 GetFinalScale(float s) => new Vector3(s, s, s);
}
