using UnityEngine;

public class MapScanner
{
    private const int RayHeight = 100;
    private const int MaxHits = 1000;
    private const float RayDistance = 1000;

    private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];
    private readonly Collider[] _colliderHits = new Collider[MaxHits];

        
    public bool CheckPositionOnMapOccupied(Vector3 mapPosition, Vector3 normal)
    {
        var origin = mapPosition + normal * RayHeight;
        var ray = new Ray(origin, -normal);
        var hitCount = Physics.RaycastNonAlloc(ray, _hits, RayDistance);

        for (int i = 0; i < hitCount; i++)
        {
            if (!_hits[i].transform.CompareTag(Constants.FloorTag))
                return true;
        }
        return false;
    }

    public bool CheckPositionObstructed(Vector3 mapPosition, Vector3 objectSize)
    {
        var hitCount = Physics.OverlapBoxNonAlloc(mapPosition, objectSize / 2, _colliderHits);
   
        for (int i = 0; i < hitCount; i++)
        {
            var go = _colliderHits[i].gameObject;
            if (go.activeSelf && !go.CompareTag(Constants.FloorTag))
            {
                Debug.Log($"Obstructing go is {go.name}");
                return true;
            }
                
        }
        return false;
    }
}