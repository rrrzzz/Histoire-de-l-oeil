using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

using Random = UnityEngine.Random;

public class ObjectSpawner : MonoBehaviour
{
    private const float TargetYWallsScale = 12.2f;
    private const float TargetYLidScale = 13.6965f;
    private const float TargetLidXRot = -90;
    private const float TargetFloorXScale = 45.762f;
    private const float TargetFloorZScale = 48.873f;

    [Header("World")]
    [SerializeField] private Transform floorOrig;
    
    [Header("Animating World")]
    [SerializeField] private float commonTime;
    [SerializeField] private float buildingGrowthTime = 1f;
    [SerializeField] private bool useCommonTime = true;

    [SerializeField] private float wallWideningTime = 1f;
    [SerializeField] private float wallTallingTime = 1f;
    [SerializeField] private float lidClosingTime = 1f;
    [SerializeField] private float floorGrowthTime = 1f;
    
    [Header("GameObjects")]
    [SerializeField] private Transform furnishingsRoot;
    [SerializeField] private Transform buildingsRoot;
    [SerializeField] private Transform animalsRoot;
    [SerializeField] private Transform dupesRoot;
    
    [Header("Debug")]
    [SerializeField] private bool debugSpawn;

    [Header("Spawn parameters")]
    [SerializeField] private float buildingsSpacing = 1;
    [SerializeField] private int spawnAttempts = 10;
    [SerializeField] private int buildingsCount = 10;
    [SerializeField] private int objectsCount = 10;
    [SerializeField] private float spawnHeight = 100;
    [SerializeField] private float closerAmount;
    [SerializeField] private float tToEnd = 6;

    private readonly List<Transform> _duplicateObjects = new List<Transform>();
    private readonly List<Transform> _duplicateBuildings = new List<Transform>();

    private Transform _tLWorldCorner; //zwall1, zwall2
    private Transform _bRWorldCorner;
    
    private Transform _floor;
    private Vector2 _tlCorner;
    private Vector2 _brCorner;
    private MapScanner _mapScanner;
    private Vector3 _spacingVector;
    private Transform[] _allBuildings;

    private void Start()
    {
        if (useCommonTime)
            wallWideningTime = wallTallingTime = lidClosingTime = floorGrowthTime = commonTime;
        
        _mapScanner = new MapScanner();

        _spacingVector = new Vector3(buildingsSpacing, buildingsSpacing, buildingsSpacing);
        _allBuildings = buildingsRoot.GetComponentsInChildren<MeshCollider>(true).Select(x => x.transform).ToArray();
    }

    private void CreateWorld()
    {
        _floor = Instantiate(floorOrig);
        _floor.gameObject.SetActive(true);

        var worldBounds = _floor.GetChild(0);

        _tLWorldCorner = worldBounds.GetChild(0);
        _bRWorldCorner = worldBounds.GetChild(2);
        
        var seq = DOTween.Sequence();

        seq.Append(_floor.DOScale(new Vector3(TargetFloorXScale, _floor.localScale.y, TargetFloorZScale), floorGrowthTime));

        for (int i = 0; i < 4; i++)
        {
            var currentWall = worldBounds.GetChild(i);

            seq.Append(GrowWallWidth(currentWall));
        }

        seq.OnComplete(() =>
        {
            SetWorldCorners();
            
            for (int i = 0; i < buildingsCount; i++)
            {
                SpawnBuilding();
            }
        
            foreach (var b in _duplicateBuildings)
            {
                b.DOScale(new Vector3(0.01f, 0.01f, 0.01f), buildingGrowthTime);
            }
        
            StartCoroutine(SpawnObjectsSeries(worldBounds));
        });

        seq.Play();
    }
    
    private IEnumerator SpawnObjectsSeries(Transform worldBounds)
    {
        yield return new WaitForSeconds(buildingGrowthTime);
        
        for (int i = 0; i < objectsCount; i++)
            SpawnMisc(spawnHeight);
        
        StartCoroutine(SetCollidersToTriggerLater(0, tToEnd + 2));
        yield return StartCoroutine(SetCollidersToTriggerAfterSpawned());
        
        var finalSeq = DOTween.Sequence();
                
        for (int i = 0; i < 4; i++)
        {
            var currentWall = worldBounds.GetChild(i);
            finalSeq.Append(GrowHeight(currentWall, TargetYWallsScale));
        }
        
        var lid = worldBounds.GetChild(4);
        finalSeq.Append(GrowHeight(lid, TargetYLidScale));
        finalSeq.Append(lid.DORotate(new Vector3(TargetLidXRot, 0, 0), lidClosingTime));
        
        finalSeq.Play();
    }
    
    private void SetWorldCorners()
    {
        _tlCorner = new Vector2(_tLWorldCorner.position.x, _tLWorldCorner.position.z);
        _brCorner = new Vector2(_bRWorldCorner.position.x, _bRWorldCorner.position.z);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            CreateWorld();
        
        //
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     for (int i = 0; i < buildingsCount; i++)
        //     {
        //         SpawnBuilding();
        //     }
        // }
        //
        // if (Input.GetKeyDown(KeyCode.J))
        // {
        //     for (int i = 0; i < objectsCount; i++)
        //     {
        //         SpawnMisc(spawnHeight);
        //     }
        //
        //     if (!debugSpawn)
        //     {
        //         StartCoroutine(SetCollidersToTriggerLater(0, tToEnd + 2));
        //         StartCoroutine(SetCollidersToTriggerAfterSpawned());
        //     }
        // }
        //
        // if (Input.GetKeyDown(KeyCode.T))
        // {
        //     StartCoroutine(SetCollidersToTriggerLater(0, 3f));
        // }
        //
        // if (Input.GetKeyDown(KeyCode.L))
        // {
        //     DestroySpawned();
        // }
    }
    
    private TweenerCore<Vector3, Vector3, VectorOptions> GrowWallWidth(Transform tr) =>
        tr.DOScale(Vector3.one, wallWideningTime);
    
    TweenerCore<Vector3, Vector3, VectorOptions> GrowHeight(Transform tr, float targetScale) =>
        tr.DOScaleY(targetScale, wallTallingTime);

    private void TriggerImmediately()
    {
        foreach (var b in _duplicateObjects)
        {
            if (!b)
            {
                continue;
            }
            if (b.position.y < -10)
            {
                Destroy(b.gameObject);
            }
            var rbs = b.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                if (rb.position.y < -10)
                {
                    Destroy(b.gameObject);
                    continue;
                }
                Destroy(rb);
            }
            
            var colliders = b.GetComponentsInChildren<MeshCollider>();
            foreach (var col in colliders)
            {
                col.enabled = true;
                col.isTrigger = true;
            }
        }
        
        foreach (var b in _duplicateBuildings)
        {
            var colliders = b.GetComponentsInChildren<MeshCollider>();
            foreach (var col in colliders)
            {
                col.isTrigger = true;
            }
        }
    }
    
    private IEnumerator SetCollidersToTriggerLater(int offset, float waitTime)
    {
        var rigidBds = new List<Rigidbody>();
        foreach (var b in _duplicateObjects.Skip(offset))
        {
            if (!b)
            {
                continue;
            }

            var rbs = b.GetComponentsInChildren<Rigidbody>();
            rigidBds.AddRange(rbs);
            foreach (var rb in rbs)
            {
                rb.AddForce(Vector3.down * Random.Range(100, 1000));
                rb.AddTorque(Random.onUnitSphere * Random.Range(10000, 100000), ForceMode.Acceleration);
            }
        }

        yield return new WaitForSeconds(waitTime);
        
        foreach (var b in _duplicateObjects.Skip(offset))
        {
            if (!b)
            {
                continue;
            }

            var colliders = b.GetComponentsInChildren<MeshCollider>();
            foreach (var col in colliders)
            {
                col.enabled = true;
                col.isTrigger = true;
            }
        }

        foreach (var rb in rigidBds)
        {
            if (!rb)
            {
                continue;
            }
            if (rb.position.y < -10)
            {
                var parent = rb.transform.parent;
                if (parent.name != "Dupes")
                {
                    Destroy(parent.gameObject);
                    continue;
                }
             
                Destroy(rb.gameObject);
            }
            Destroy(rb);
        }
    }

    private IEnumerator SetCollidersToTriggerAfterSpawned()
    {
        yield return new WaitForSeconds(tToEnd);
        
        foreach (var b in _duplicateBuildings)
        {
            var colliders = b.GetComponentsInChildren<MeshCollider>();
            foreach (var col in colliders)
            {
                col.isTrigger = true;
            }
        }

        yield return new WaitForSeconds(2);

        for (int i = 0; i < objectsCount; i++)
        {
            SpawnMisc(spawnHeight);
        }
        
        yield return StartCoroutine(SetCollidersToTriggerLater(objectsCount, 3.3f));

        yield return new WaitForSeconds(1f);
        
        for (int i = 0; i < objectsCount; i++)
        {
            SpawnMisc(spawnHeight);
        }
        
        yield return StartCoroutine(SetCollidersToTriggerLater(objectsCount * 2, 2.5f));
    }
    
    private Vector3 GetRandomPos() =>
        new Vector3(Random.Range(_tlCorner.x, _brCorner.x), 0, Random.Range(_tlCorner.y, _brCorner.y));

    private Vector3 GetRandomCloser() =>
        new Vector3(Random.Range(_tlCorner.x - closerAmount, _brCorner.x + closerAmount), 0, Random.Range(_tlCorner.y + closerAmount, _brCorner.y - closerAmount));
    
    private void SpawnBuilding()
    {
        var spawnCounter = spawnAttempts;
        while (spawnCounter-- >= 0)
        {
            var building = GetRandomObjectNew(_allBuildings);
            var spawnAttemptsLeft = spawnAttempts;
            
            var size = building.GetComponentInChildren<MeshCollider>().GetComponent<MeshRenderer>().bounds.size;
            for (var i = 0; i <= spawnAttemptsLeft; i++)
            {
                var pos = GetRandomPos();
                
                if (_mapScanner.CheckPositionObstructed(pos, size, _spacingVector))
                    continue;
                
                building = Instantiate(building, dupesRoot, true);
                _duplicateBuildings.Add(building);
                
                pos.y = building.position.y;
                building.position = pos;
                building.localScale = Vector3.zero;
                building.gameObject.SetActive(true);
                return;
            }
        }
    }

    private void SpawnMisc(float height)
    {
        var isAnimal = Random.Range(0, 2) == 0;
        var miscObj = isAnimal ? GetRandomObject(animalsRoot) : GetRandomObject(furnishingsRoot);

        var pos = GetRandomCloser();
        pos.y = height;

        miscObj = Instantiate(miscObj, dupesRoot, true);
        _duplicateObjects.Add(miscObj);

        miscObj.position = pos;
        miscObj.rotation = Random.rotation;
        miscObj.gameObject.SetActive(true);
    }
    
    private void DestroySpawned()
    {
        foreach (var x in _duplicateObjects.Where(x => x))
        {
            Destroy(x.gameObject);
        }
        
        foreach (var x in _duplicateBuildings.Where(x => x))
        {
            Destroy(x.gameObject);
        }
       
        _duplicateObjects.Clear();
        _duplicateBuildings.Clear();
    } 
    
    private Transform GetRandomObject(Transform root)
    {
        var categoryIndex = Random.Range(0, root.childCount);
        var categoryRoot = root.GetChild(categoryIndex);
        var objectIndex = Random.Range(0, categoryRoot.childCount);
        var obj = categoryRoot.GetChild(objectIndex);
        if (obj.childCount != 0 && obj.GetChild(0).name.Contains("alt"))
        {
            var altIndex = Random.Range(0, obj.childCount);
            return obj.GetChild(altIndex);
        }

        return obj;
    }

    private Transform GetRandomObjectNew(Transform[] objects)
    {
        var idx = Random.Range(0, objects.Length);

        var tr = objects[idx];
        var childCount = tr.parent.childCount;
        
        if (childCount == 1)
            return tr.parent;
     
        var secondChild = tr.parent.GetChild(1);
        if (secondChild.GetComponent<MeshFilter>() != null && secondChild.GetComponent<MeshCollider>() == null)
        {
            return tr.parent;
        }
        
        var thirdChild = childCount > 2 ? tr.parent.GetChild(2) : null;
        if (thirdChild != null && thirdChild.GetComponent<MeshFilter>() != null &&
            thirdChild.GetComponent<MeshCollider>() == null)
        {
            return tr.parent;
        }
        return tr;
    }

    private void OnValidate()
    {
        _spacingVector = new Vector3(buildingsSpacing, buildingsSpacing, buildingsSpacing);
    }
}