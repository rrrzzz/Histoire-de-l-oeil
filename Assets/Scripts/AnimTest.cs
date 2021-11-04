using DG.Tweening;
using UnityEngine;

public class AnimTest : MonoBehaviour
{
    [SerializeField] private Animator anim;
    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.enabled = true;
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            anim.StopPlayback();
            var info = anim.GetCurrentAnimatorClipInfo(0);
        }
    }
}
