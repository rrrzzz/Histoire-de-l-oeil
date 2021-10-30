using EasyButtons;
using UnityEngine;

[ExecuteAlways]
public class AddComponents : MonoBehaviour
{

    [SerializeField] private float mass = 1;
    

    [Button]
    private void AddCollider()
    {
        Transform previousParent = null;
        foreach (var mf in transform.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.transform.parent == previousParent)
            {
                continue;
            }

            previousParent = mf.transform.parent;
            var col = mf.gameObject.AddComponent<MeshCollider>();
            col.convex = true;
        }
    }

    [Button]
    private void AddRigidBody()
    {
        Transform previousParent = null;
        foreach (var mf in transform.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.transform.parent == previousParent)
            {
                continue;
            }

            previousParent = mf.transform.parent;
            var rb = mf.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = mass;
                return;
            }
            
            rb = mf.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = mass;
        }
    }
}
