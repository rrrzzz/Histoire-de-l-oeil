using System.Collections;
using UnityEngine;

public class CollisionTest : MonoBehaviour
{
    [SerializeField] private float volumeDivisor = 10f;
    [SerializeField] private float scaleDelta = 0.005f;
    [SerializeField] private float growingTime = 0.5f;
    [SerializeField] private float suckingTime = 0.1f;
    [SerializeField] private Transform[] createdObjects;
   
    [HideInInspector]
    public bool isGrowing;
    [HideInInspector]
    public float targScale; 

    private void Update()
    {
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

    // private void OnCollisionEnter(Collision other) => ProcessCollision(other.transform);
    private void OnTriggerEnter(Collider other) => ProcessCollision(other.transform);
    
    private void ProcessCollision(Transform tr)
    {
        if (tr.CompareTag(Constants.UnsuckTag) || tr.CompareTag(Constants.FloorTag) || !tr.gameObject.activeInHierarchy)
            return;
        
        var mr = tr.GetComponentInChildren<MeshRenderer>();
        var meshGlobalVolume = mr.bounds.size;
        var volume = meshGlobalVolume.x * meshGlobalVolume.y * meshGlobalVolume.z;
        Debug.Log(tr.name);
        StartCoroutine(ScaleDown(mr.transform));
        StartCoroutine(ScaleUp(Mathf.Pow(volume, 1f / 3f)));
    }
    
    private IEnumerator ScaleUp(float volume)
    {
        var scaleVelocity = Vector3.zero;
        var targetScale = transform.localScale.x + volume / volumeDivisor;
        targScale = targetScale;
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
        
        var isSingleObject = char.IsUpper(tr.name[0]) || tr.name.ToLower().Contains("alt");
        var curTr = isSingleObject ? tr : tr.parent;
        
        var finalScale = curTr.localScale.x / 10;
        var targetScale = GetFinalScale(finalScale);
        
        var meshCols = curTr.GetComponentsInChildren<MeshCollider>();
        
        foreach (var mc in meshCols)
        {
            mc.enabled = false;
        }

        while (curTr.localScale.x - finalScale > scaleDelta)
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
