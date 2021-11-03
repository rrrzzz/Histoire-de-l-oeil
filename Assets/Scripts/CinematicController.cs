using DG.Tweening;
using UnityEngine;

public class CinematicController : MonoBehaviour

{
    public Sequence CamSeq;
    [HideInInspector]
    public Camera CineCam;

    [SerializeField] private ObjectSpawner objSpawner;
    
    [SerializeField] private float rotationSpeed;
    
    [Header("TargetsCreation")]
    [SerializeField] private Transform OrbitingStart;
    [SerializeField] private Transform BoxOverviewCutTo;
    
    [Header("TargetsEye")]
    [SerializeField] private Transform PortalOpening;
    // [SerializeField] private Transform PortalSegway;
    [SerializeField] private Transform LadderGrowth;
    [SerializeField] private Transform BeforeEye;
    
    [Header("Objects")]
    [SerializeField] public Transform eye;
    [SerializeField] private Transform worldCenter;
    [SerializeField] private Transform doorParent;
    [SerializeField] private Transform ladderParent;

    [Header("Timings")]
    [SerializeField] private float defaultSegwayTime;
    // [SerializeField] private float overViewTime;
    [SerializeField] private float ladderGrowthTime;
    // [SerializeField] private float rotatingTime;
    // [SerializeField] private float boxCreationTime;
    [SerializeField] private float portalOpeningTime;
    // [SerializeField] private float portalStayTime;
    [SerializeField] private float toBeforeEyeTime = 2;
    [SerializeField] private float ladderGrowthStayTime;
    [SerializeField] private float lookAtEyeSegwayTime;
    [SerializeField] private float suckInEyeTime;

    [SerializeField] private float _strength = 5;
    [SerializeField] private int _vibration = 7;
    [SerializeField] private float _randomness = 20;
    
    private const float TargetLadderScale = 0.9362f;

    private bool _isRotating;
    
    public void Start()
    {
        var eyeRb = eye.GetComponent<Rigidbody>();
        var eyeMover = eye.GetComponent<Mover>();
        
        CineCam = transform.GetComponent<Camera>();
        
        var buildingsObjectsSpawnTime = objSpawner.buildingGrowthTime + 8 + 3.3f + 2.5f;
        
        CamSeq = DOTween.Sequence();
        CamSeq.AppendInterval(objSpawner.floorGrowthTime + objSpawner.wallWideningTime * 4);
        MRto(OrbitingStart);
        CamSeq.AppendCallback(() => _isRotating = true);
        CamSeq.AppendInterval(buildingsObjectsSpawnTime);

        var boxGrowingTime = objSpawner.wallTallingTime * 2 + objSpawner.lidClosingTime;
        
        CamSeq.AppendCallback(() =>
        {
            _isRotating = false;
            transform.position = BoxOverviewCutTo.position;
            transform.rotation = BoxOverviewCutTo.rotation;
        }).AppendInterval(boxGrowingTime);
        
        CamSeq.AppendCallback(() => _isRotating = false);
        
        MRto(PortalOpening, 2f);
        
        CamSeq.Append(doorParent.DOScaleZ(0, portalOpeningTime));
        CamSeq.AppendInterval(.5f);
        
        
        CamSeq.AppendCallback(() =>
        {
            transform.position = LadderGrowth.position;
            transform.rotation = LadderGrowth.rotation;
        });
        
        CamSeq.AppendInterval(.5f);
        
        CamSeq.AppendCallback(() => ladderParent.DOScaleZ(TargetLadderScale, ladderGrowthTime));
        CamSeq.AppendInterval(ladderGrowthStayTime);

        CamSeq.AppendCallback(() => MRto(BeforeEye, toBeforeEyeTime));
        
        CamSeq.AppendInterval(toBeforeEyeTime - 1);
        

        CamSeq.AppendCallback(() => eye.DOShakeRotation(suckInEyeTime + 2, _strength, _vibration, _randomness, false));
        
        CamSeq.AppendInterval(1);
        CamSeq.Append(transform.DOMove(eye.position, suckInEyeTime).SetEase(Ease.InQuart));
        CamSeq.AppendInterval(1);
        CamSeq.AppendCallback(() =>
        {
            eyeRb.isKinematic = false;
            eyeRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }).AppendInterval(.3f).AppendCallback(() =>
        {
            CineCam.enabled = false;
            eye.parent.GetComponentInChildren<Camera>(true).gameObject.SetActive(true);
            eyeMover.enabled = true;
        });
        
        CamSeq.Pause();
    }

    private void MRto(Transform tr, float time = -1)
    {
        if (time < 0) time = defaultSegwayTime;
        CamSeq.Append(transform.DOMove(tr.position, time)).Join(transform.DORotateQuaternion(tr.rotation, time));
    }

    private void LateUpdate()
    {
        // if (Input.GetKeyDown(KeyCode.N))
        // {
        //     CamSeq.Play();
        // }
        
        if (!_isRotating)
        {
            return;
        }

        transform.RotateAround(worldCenter.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
