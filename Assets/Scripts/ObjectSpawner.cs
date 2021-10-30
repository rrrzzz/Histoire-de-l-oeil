using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectSpawner : MonoBehaviour
{
    [Header("GameObjects")]
    [SerializeField] private Transform furnishingsRoot;
    [SerializeField] private Transform buildingsRoot;
    [SerializeField] private Transform animalsRoot;

    //use global pos, local scale
    [SerializeField] private Transform tLWorldCorner;
    [SerializeField] private Transform bRWorldCorner;
    [SerializeField] private Transform wall3X;
    [SerializeField] private Transform wall4X;
    [SerializeField] private Transform floor;

    [Header("Spawn parameters")]
    [SerializeField] private float buildingsSpacing = 1;
    [SerializeField] private int spawnAttempts = 10;
    [SerializeField] private int buildingsCount = 10;
    [SerializeField, Range(0,1)] private float spaceFillPercentage;

    private Transform _wall1Z;
    private Transform _wall2Z;

    private Vector2 _tlCorner;
    private Vector2 _brCorner;
    private MapScanner _mapScanner;
    private readonly HashSet<Transform> _spawnedObjects = new HashSet<Transform>();
    private readonly List<Transform> _duplicateObjects = new List<Transform>();

    private void Start()
    {
        _wall1Z = tLWorldCorner;
        _wall2Z = bRWorldCorner;

        _tlCorner = new Vector2(tLWorldCorner.position.x, tLWorldCorner.position.z);
        _brCorner = new Vector2(bRWorldCorner.position.x, bRWorldCorner.position.z);
        
        _mapScanner = new MapScanner();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < buildingsCount; i++)
            {
                SpawnBuildingDebug();
            }
            
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            DisableSpawned();
        }
    }

    private Vector3 GetRandomPos() =>
        new Vector3(Random.Range(_tlCorner.x, _brCorner.x), 0, Random.Range(_tlCorner.y, _brCorner.y));

    private bool SpawnBuilding()
    {
        var spawnCounter = spawnAttempts;
        while (spawnCounter-- >= 0)
        {
            var building = GetRandomObject(buildingsRoot);
            Debug.Log(building.name);

            var extents = building.GetComponentInChildren<MeshRenderer>().bounds.extents;
            var spawnAttemptsLeft = spawnAttempts;

            for (var i = 0; i <= spawnAttemptsLeft; i++)
            {
                var pos = GetRandomPos();

                if (_mapScanner.CheckPositionObstructed(pos, 
                    extents + new Vector3(buildingsSpacing, buildingsSpacing, buildingsSpacing))) continue;
                
                pos.y = building.position.y;
                if (_spawnedObjects.Contains(building))
                {
                    var dupe = Instantiate(building, pos, building.rotation, building.parent);
                    Debug.Log("Got dupe! " + dupe.name);
                    _duplicateObjects.Add(dupe);
                }
              
                building.position = pos;
                building.gameObject.SetActive(true);
                
                return true;
            }
        }
        return false;
    }
    
    private bool SpawnBuildingDebug()
    {
        
            var building = GetRandomObject(buildingsRoot);
            var extents = building.GetComponentInChildren<MeshRenderer>().bounds.extents;
        
            var pos = GetRandomPos();

            if (_mapScanner.CheckPositionObstructed(pos, extents)) Debug.Log($"Building is {building.name} obstructed");
            
            pos.y = building.position.y;
            building.position = pos;
                
            building.gameObject.SetActive(true);
            _spawnedObjects.Add(building);
            return true;
    }

    private void DisableSpawned()
    {
        foreach (var o in _spawnedObjects)
        {
            o.gameObject.SetActive(false);
        }
        
        _duplicateObjects.ForEach(x => Destroy(x.gameObject));
        
        _spawnedObjects.Clear();
        _duplicateObjects.Clear();
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
}