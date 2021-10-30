using UnityEngine;

public class ColorChanger : MonoBehaviour
{

    private void Start()
    {
        GetComponent<MeshRenderer>().material.color = Color.cyan;
    }

    private void Update()
    {
        
    }
}
