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

    public bool CheckPositionObstructed(Vector3 mapPosition, Vector3 objectSize, Vector3 spacing, bool useSpacing = true)
    {
        objectSize = useSpacing ? objectSize + spacing : objectSize;
        var hitCount = Physics.OverlapBoxNonAlloc(mapPosition, objectSize / 2, _colliderHits);
   
        for (int i = 0; i < hitCount; i++)
        {
            var go = _colliderHits[i].gameObject;
            if (go.activeInHierarchy && !go.CompareTag(Constants.FloorTag))
            {
                var isWall = go.name.ToLower().Contains("wall");
                if (isWall || !useSpacing)
                { 
                    return true;
                }
            }
        }

        if (!useSpacing) return false;
        
        return CheckPositionObstructed(mapPosition, objectSize, spacing, false);
    }
}