using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class WorldSpawner : MonoBehaviour
{
    private const float TargetYWallsScale = 12.2f;
    private const float TargetYLidScale = 13.6965f;
    private const float TargetLidXRot = -90;
    private const float TargetFloorXScale = 45.762f;
    private const float TargetFloorZScale = 48.873f;

    [SerializeField] private float commonTime;
    [SerializeField] private bool useCommonTime = true;
    
    
    [SerializeField] private float wallWideningTime = 1f;
    [SerializeField] private float wallTallingTime = 1f;
    [SerializeField] private float lidClosingTime = 1f;
    [SerializeField] private float floorGrowthTime = 1f;

    [SerializeField] private float _force = 100000f;
    [SerializeField] private float _mass = 1;
    


    private void Start()
    {
        if (useCommonTime)
        {
            wallWideningTime = wallTallingTime = lidClosingTime = floorGrowthTime = commonTime;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(CreateWorld());
        }    
    }

    IEnumerator CreateWorld()
    {
        var worldBounds = transform.GetChild(0);
        worldBounds.parent = null;
        var lid = worldBounds.GetChild(4).GetChild(0);

        var dif = lid.position - transform.position;

        var expPos = transform.position + dif.normalized * (dif.magnitude / 2);

        transform.GetComponent<MeshCollider>().enabled = false;
        var crb = gameObject.AddComponent<Rigidbody>();
        crb.AddExplosionForce(200000, expPos, 10000);
        
        for (int i = 0; i < 5; i++)
        {
            var mfs = worldBounds.GetChild(i).GetComponentsInChildren<MeshFilter>();
            foreach (var mf in mfs)
            {
                var mc = mf.GetComponent<MeshCollider>();
                if (mc)
                {
                    mc.enabled = false;
                }
                crb = mf.gameObject.AddComponent<Rigidbody>();
                crb.AddExplosionForce(200000, expPos, 10000);
            }
        }

        yield return new WaitForSeconds(4);
        Destroy(worldBounds.gameObject);
        Destroy(transform.gameObject);
    }

    private TweenerCore<Vector3, Vector3, VectorOptions> GrowWallWidth(Transform tr) =>
        tr.DOScale(Vector3.one, wallWideningTime);
    
    TweenerCore<Vector3, Vector3, VectorOptions> GrowHeight(Transform tr, float targetScale) =>
        tr.DOScaleY(targetScale, wallTallingTime);
}
