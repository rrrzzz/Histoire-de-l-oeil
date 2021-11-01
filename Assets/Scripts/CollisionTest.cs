using System.Collections;
using UnityEngine;

public class CollisionTest : MonoBehaviour
{
    [HideInInspector]
    public bool isGrowing;
    [HideInInspector]
    public float targScale;
    
    [SerializeField] private float volumeDivisor = 10f;
    [SerializeField] private float scaleDelta = 0.005f;
    [SerializeField] private float growingTime = 0.5f;
    [SerializeField] private float suckingTime = 0.1f;

    [SerializeField] private float _totalHeight; 
    // [SerializeField] private Transform[] createdObjects;

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.I))
    //     {
    //         var volume = 0f;
    //         foreach (var o in createdObjects)
    //         {
    //             var mr = o.GetComponentInChildren<MeshRenderer>();
    //             
    //             var meshGlobalVolume = mr.bounds.size;
    //             var currentVol = meshGlobalVolume.x * meshGlobalVolume.y * meshGlobalVolume.z;
    //             volume += Mathf.Pow(currentVol, 1f / 3f);
    //             
    //             StartCoroutine(ScaleDown(mr.transform));
    //         }
    //         
    //         StartCoroutine(ScaleUp(volume));
    //     }
    // }

    // private void OnTriggerEnter(Collider other) => ProcessCollision(other.transform);
    private void OnTriggerEnter(Collider other)
    {
        var tr = other.transform;
        if (tr.CompareTag(Constants.UnsuckTag) || tr.CompareTag(Constants.FloorTag) || tr.CompareTag(Constants.ProcessedTag) || !tr.gameObject.activeInHierarchy)
            return;

        tr.tag = Constants.ProcessedTag;
        ProcessCollision(tr);
    }
    
    public void ProcessCollision(Transform tr)
    {
        tr.tag = Constants.UnsuckTag;
        var mr = tr.GetComponentInChildren<MeshRenderer>();
        var meshGlobalVolume = mr.bounds.size;
        
        Debug.Log(_totalHeight);
        var volume = meshGlobalVolume.x * meshGlobalVolume.y * meshGlobalVolume.z;
        StartCoroutine(ScaleDown(mr.transform));
        StartCoroutine(ScaleUp(Mathf.Pow(volume, 1f / 3f)));
    }
    
    private IEnumerator ScaleUp(float volume)
    {
        var scaleVelocity = Vector3.zero;
        var targetScale = transform.localScale.x + volume / volumeDivisor;
        _totalHeight += targetScale;
        targScale = targetScale;
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
        var moveVelocity = Vector3.zero;
        var scaleVelocity = Vector3.zero;
        
        var isSingleObject = char.IsUpper(tr.name[0]) || tr.name.ToLower().Contains("alt");
        var curTr = isSingleObject ? tr : tr.parent;
        
        if (curTr.name == "Dupes")
        {
            curTr = tr;
        }
        
        var finalScale = curTr.localScale.x * 0.1f;
        var targetScale = GetFinalScale(finalScale);
        
        var meshCols = curTr.GetComponentsInChildren<MeshCollider>();
        
        foreach (var mc in meshCols)
        {
            mc.enabled = false;
        }
        
        while (curTr.localScale.x - finalScale > finalScale * 0.1f)
        {
            curTr.position =
                Vector3.SmoothDamp(curTr.position, transform.position, ref moveVelocity, suckingTime, 1000, Time.deltaTime);
            curTr.localScale =
                Vector3.SmoothDamp(curTr.localScale, targetScale, ref scaleVelocity, suckingTime, 1000, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        Destroy(curTr.gameObject);
    }

    private Vector3 GetFinalScale(float s) => new Vector3(s, s, s);
}
