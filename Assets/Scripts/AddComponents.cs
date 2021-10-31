using System;
using EasyButtons;
using UnityEngine;

[ExecuteAlways]
public class AddComponents : MonoBehaviour
{

    [SerializeField] private float mass = 1;
    [SerializeField] private bool addCollider;
    [SerializeField] private bool addRigidBody;
    [SerializeField] private bool addAllColliders;

    [Button]
    private void AddSelectedComponents()
    {
        Transform previousParent = null;
        foreach (var mf in transform.GetComponentsInChildren<MeshFilter>(true))
        {
            var rb = mf.GetComponent<Rigidbody>();
            
            if (mf.GetComponent<MeshCollider>() != null)
            {
                if (rb != null)
                    rb.mass = mass;
                
                previousParent = mf.transform.parent;
                continue;
            }

            var name = mf.name.ToLower();
            // var isSingleObject = char.IsUpper(mf.name[0]) || name.Contains("alt") || name.Contains("base");
            var isSingleObject = name.Contains("alt") || name.Contains("base");
            if (mf.transform.parent == previousParent && !isSingleObject)
                continue;
            
            previousParent = mf.transform.parent;

            if (addCollider)
            {
                var col = mf.gameObject.AddComponent<MeshCollider>();
                col.convex = true;
            }

            if (addRigidBody)
            {
                rb = mf.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.mass = mass;
            }
        }
    }

    [Button]
    private void RemoveRigidBodies()
    {
        foreach (var mf in transform.GetComponentsInChildren<MeshCollider>(true))
        {
            var tr = mf.transform;
            var rb = mf.GetComponent<Rigidbody>();
            var childCount = tr.parent.childCount;

            if (childCount <= 1)
                continue;
            
            var secondChild = tr.parent.GetChild(1);
            if (secondChild.GetComponent<MeshFilter>() != null && secondChild.GetComponent<MeshCollider>() == null)
            {
                DestroyImmediate(rb);
                if (tr.parent.GetComponent<Rigidbody>())
                {
                    continue;
                }
                tr.parent.gameObject.AddComponent<Rigidbody>().mass = 1;
            }
        }

        if (addAllColliders)
        {
            foreach (var mf in transform.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.gameObject.GetComponent<MeshCollider>())
                    continue;
                
                mf.gameObject.AddComponent<MeshCollider>().convex = true;
            } 
        }
    }

    [Button]
    private void AddAllColliders()
    {
        foreach (var mf in transform.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.gameObject.GetComponent<MeshCollider>())
                continue;
                
            mf.gameObject.AddComponent<MeshCollider>().convex = true;
        } 
    }
    
    [Button]
    private void ChangeRbCollisionDetection()
    {
        foreach (var rb in transform.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        } 
    }

    [Button]
    private void EnableColliders()
    {
        foreach (var mf in transform.GetComponentsInChildren<MeshCollider>(true))
        {
            mf.enabled = true;
        }
    }
}
