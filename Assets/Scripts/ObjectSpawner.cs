using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public class ObjectSpawner : MonoBehaviour
{
    [Header("GameObjects")]
    [SerializeField] private Transform furnishingsRoot;
    [SerializeField] private Transform buildingsRoot;
    [SerializeField] private Transform animalsRoot;
    [SerializeField] private Transform dupesRoot;
    
    [Header("Debug")]
    // [SerializeField] private Transform debugObject;

    [Header("Spawn parameters")]
    [SerializeField] private float buildingsSpacing = 1;
    [SerializeField] private int spawnAttempts = 10;
    [SerializeField] private int buildingsCount = 10;
    [SerializeField] private int objectsCount = 10;
    [SerializeField] private float spawnHeight = 100;
    [SerializeField] private float closerAmount;
    [SerializeField] private float tToEnd = 6;
    
    //use global pos, local scale
    [Header("World")]
    [SerializeField] private Transform tLWorldCorner;
    [SerializeField] private Transform bRWorldCorner;
    [SerializeField] private Transform wall3X;
    [SerializeField] private Transform wall4X;
    [SerializeField] private Transform floor;


    

    private readonly List<Transform> _duplicateObjects = new List<Transform>();
    private readonly List<Transform> _duplicateBuildings = new List<Transform>();
    
    private Transform _wall1Z;
    private Transform _wall2Z;

    private Vector2 _tlCorner;
    private Vector2 _brCorner;
    private MapScanner _mapScanner;
    private Vector3 _spacingVector;
    private Transform[] _allBuildings;
    
    private void Start()
    {
        _wall1Z = tLWorldCorner;
        _wall2Z = bRWorldCorner;

        _tlCorner = new Vector2(tLWorldCorner.position.x, tLWorldCorner.position.z);
        _brCorner = new Vector2(bRWorldCorner.position.x, bRWorldCorner.position.z);
        
        _mapScanner = new MapScanner();

        _spacingVector = new Vector3(buildingsSpacing, buildingsSpacing, buildingsSpacing);
        _allBuildings = buildingsRoot.GetComponentsInChildren<MeshCollider>(true).Select(x => x.transform).ToArray();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < buildingsCount; i++)
            {
                SpawnBuilding();
            }
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            for (int i = 0; i < objectsCount; i++)
            {
                SpawnMisc(spawnHeight);
            }

            // StartCoroutine(SetCollidersToTriggerAfterSpawned());
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TriggerImmediately();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            DestroySpawned();
        }
    }

    private void TriggerImmediately()
    {
        foreach (var b in _duplicateObjects)
        {
            var rbs = b.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
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

    private IEnumerator SetCollidersToTriggerAfterSpawned()
    {
        yield return new WaitForSeconds(tToEnd);
        foreach (var b in _duplicateObjects)
        {
            var rbs = b.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                Destroy(rb);
            }
            
            var colliders = b.GetComponentsInChildren<MeshCollider>();
            foreach (var col in colliders)
            {
                col.enabled = true;
                col.isTrigger = true;
            }
        }
        
        yield return new WaitForSeconds(2);
        
        foreach (var b in _duplicateBuildings)
        {
            var colliders = b.GetComponentsInChildren<MeshCollider>();
            foreach (var col in colliders)
            {
                col.isTrigger = true;
            }
        }
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
    
    private bool SpawnBuildingDebug()
    {
        var building = GetRandomObjectNew(_allBuildings);
        
        var pos = GetRandomPos();
        
       
        
        var extents = building.GetComponentInChildren<MeshRenderer>().bounds.size;
        
        
        
        if (_mapScanner.CheckPositionObstructed(Vector3.zero, extents, _spacingVector))
            return false;

        building = Instantiate(building, dupesRoot, true);
        _duplicateBuildings.Add(building);
        
        pos.y = building.position.y;
        building.position = pos;
 
        
        building.gameObject.SetActive(true);


        return true;
    }

    private void DestroySpawned()
    {
        _duplicateObjects.ForEach(x => Destroy(x.gameObject));
        _duplicateBuildings.ForEach(x => Destroy(x.gameObject));

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
        if (childCount == 0)
            return tr;

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