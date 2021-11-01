
using UnityEngine;

public class ColSeparate : MonoBehaviour
{
    [SerializeField] private CollisionTest _colTest;
    [SerializeField] private Transform _eye;

    private void Update()
    {
        transform.localPosition = _eye.localPosition;
        transform.localScale = _eye.localScale;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var tr = other.transform;
        if (tr.CompareTag(Constants.UnsuckTag) || tr.CompareTag(Constants.FloorTag) || tr.CompareTag(Constants.ProcessedTag) || !tr.gameObject.activeInHierarchy)
            return;

        tr.tag = Constants.ProcessedTag;
        _colTest.ProcessCollision(tr);
    }
}
