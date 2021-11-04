
using UnityEngine;

public class ColSeparate : MonoBehaviour
{
    [SerializeField] private EyeCollisionsController _colTest;
    [SerializeField] private Transform _eye;

    private void Update()
    {
        transform.localPosition = _eye.localPosition;
        transform.localScale = _eye.localScale;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var tr = other.transform;
        if (tr.CompareTag(Constants.RoofTag))
        {
            _colTest.SuckAll();
            return;
        }
        if (tr.CompareTag(Constants.UnsuckTag) || tr.CompareTag(Constants.FloorTag) || tr.CompareTag(Constants.ProcessedTag) || !tr.gameObject.activeInHierarchy || tr.CompareTag("Blocker"))
            return;

        tr.tag = Constants.ProcessedTag;
        _colTest.ProcessCollision(tr);
    }
}
