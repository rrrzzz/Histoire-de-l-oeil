using UnityEngine;

public class InputBehaviourTests : MonoBehaviour
{
    
    [Header("O to increase local scale")] 
    [SerializeField]
    private float scaleMul = 1.5f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            transform.localScale *= scaleMul;
        }
    }
}
