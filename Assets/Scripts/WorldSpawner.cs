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

    [SerializeField] private Transform floorParent;
    [SerializeField] private float commonTime;
    [SerializeField] private bool useCommonTime = true;
    
    
    [SerializeField] private float wallWideningTime = 1f;
    [SerializeField] private float wallTallingTime = 1f;
    [SerializeField] private float lidClosingTime = 1f;
    [SerializeField] private float floorGrowthTime = 1f;


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
            CreateWorld();
        }    
    }

    void CreateWorld()
    {
        var worldBounds = floorParent.GetChild(0);
        var seq = DOTween.Sequence();

        seq.Append(floorParent.DOScale(new Vector3(TargetFloorXScale, transform.localScale.y, TargetFloorZScale), floorGrowthTime));

        for (int i = 0; i < 4; i++)
        {
            var currentWall = worldBounds.GetChild(i);

            seq.Append(GrowWallWidth(currentWall));
        }
        
        for (int i = 0; i < 4; i++)
        {
            var currentWall = worldBounds.GetChild(i);
            seq.Append(GrowHeight(currentWall, TargetYWallsScale));
        }

        var lid = worldBounds.GetChild(4);
        seq.Append(GrowHeight(lid, TargetYLidScale));
        seq.Append(lid.DORotate(new Vector3(TargetLidXRot, 0, 0), lidClosingTime));

        seq.Play();
    }

    private TweenerCore<Vector3, Vector3, VectorOptions> GrowWallWidth(Transform tr) =>
        tr.DOScale(Vector3.one, wallWideningTime);
    
    TweenerCore<Vector3, Vector3, VectorOptions> GrowHeight(Transform tr, float targetScale) =>
        tr.DOScaleY(targetScale, wallTallingTime);
}
