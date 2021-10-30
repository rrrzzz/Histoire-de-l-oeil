using EasyButtons;
using UnityEngine;

[ExecuteAlways]
public class AddComponents : MonoBehaviour
{

    [SerializeField] private float mass = 1;
    [SerializeField] private bool addCollider;
    [SerializeField] private bool addRigidBody;
    
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

            var isSingleObject = char.IsUpper(mf.name[0]) || mf.name.ToLower().Contains("alt");
            if (mf.transform.parent == previousParent && !isSingleObject)
            {
                continue;
            }

            previousParent = mf.transform.parent;
            var col = mf.gameObject.AddComponent<MeshCollider>();
            col.convex = true;
            
            rb = mf.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = mass;
        }
    }
}
