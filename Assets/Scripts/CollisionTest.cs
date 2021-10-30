using System.Collections;
using System.Linq;
using UnityEngine;

public class CollisionTest : MonoBehaviour
{
    [SerializeField] private float volumeDivisor = 10f;
    [SerializeField] private float scaleDelta = 0.005f;
    [SerializeField] private float growingTime = 0.5f;

    private Transform _currentTr;
    private Vector3 _colOriginalPos; 


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
    }

    private void OnCollisionEnter(Collision other)
    {
        var tr = other.transform;
        var name = tr.name.ToLower();

        if (name.Contains("eye") || name.Contains("floor") || name.Contains("wall") || !tr.gameObject.activeSelf) return;
        
        var meshGlobalVolume = tr.GetComponent<MeshRenderer>().bounds.size;
        var volume = meshGlobalVolume.x * meshGlobalVolume.y * meshGlobalVolume.z;
       
        StartCoroutine(ScaleDown(tr));
        StartCoroutine(ScaleUp(Mathf.Pow(volume, 1f / 3f)));
    }

    private IEnumerator ScaleUp(float volume)
    {
        var scaleVelocity = Vector3.zero;
        Debug.Log(volume);
        var targetScale = transform.localScale.x + volume / volumeDivisor;
        var finalScale = GetFinalScale(targetScale);
        
        while (targetScale - transform.localScale.x > scaleDelta)
        {
            transform.localScale =
                Vector3.SmoothDamp(transform.localScale, finalScale, ref scaleVelocity, growingTime, 1000, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        
    }
    private IEnumerator ScaleDown(Transform tr)
    {
        var moveVelocity = Vector3.zero;
        var scaleVelocity = Vector3.zero;
        var finalScale = 0.01f;
        var targetScale = GetFinalScale(finalScale);
        var suckingTime = 0.1f;
        
        _currentTr = tr.parent;
        _colOriginalPos = tr.parent.position;

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
