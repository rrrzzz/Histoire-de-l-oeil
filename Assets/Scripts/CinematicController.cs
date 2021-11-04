using System;
using DG.Tweening;
using UnityEngine;

public class CinematicController : MonoBehaviour
{
    public static EventHandler GameStartEvent;
    public static EventHandler RotateToDeathEvent;
    private static readonly int FaderColor = Shader.PropertyToID("_Color");
    
    [SerializeField] private float rotationSpeed;
    
    [Header("TargetsCreation")]
    [SerializeField] private Transform widthPOV;
    [SerializeField] private Transform OrbitingStart;
    [SerializeField] private Transform BoxOverviewCutTo;
    
    [Header("TargetsEye")]
    [SerializeField] private Transform PortalOpening;
    // [SerializeField] private Transform PortalSegway;
    [SerializeField] private Transform LadderGrowth;
    [SerializeField] private Transform BeforeEye;
    
    [Header("TargetsEnding")]
    [SerializeField] private Transform BehindEye;
    [SerializeField] private Transform SupLarge;
    [SerializeField] private Transform CloseUpBehindEye;
    [SerializeField] private Transform CloseUpBehindSkull;

    [Header("Objects")]
    [SerializeField] public ObjectSpawner objSpawner;
    [SerializeField] private Transform worldCenter;
    [SerializeField] private Transform doorParent;
    [SerializeField] private Transform ladderParent;
    [SerializeField] private Transform fader;

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
    [SerializeField] private float suckInEyeTime;
    
    [SerializeField] private float BehindEyeT;
    [SerializeField] private float SupLargeT;
    [SerializeField] private float CloseUpBehindEyeT;

    [SerializeField] private float _strength = 5;
    [SerializeField] private int _vibration = 7;
    [SerializeField] private float _randomness = 20;
    
    private const float TargetLadderScale = 0.9362f;

    private bool _isRotating;
    private Sequence _seq;
    private Camera _cam;
    private Camera _eyeThirdPersonCam;
    private Transform _eye;
    private Material _faderMat;
    private Transform _death;
    
    private void Start()
    {
        Cursor.visible = false;
        _faderMat = fader.GetComponent<MeshRenderer>().material;
        EyeCollisionsController.WorldEnteredEvent += DisableIntroObjects;
        ObjectSpawner.WorldCreatedEvent += EnableIntroObjects;
        ObjectSpawner.FirstCreationEvent += PlayIntroSequence;
        DeathController.CameraSwitchEvent += PlayEndingSequence;
        DeathController.GameOverEvent += OnGameOver;
        _cam = GetComponent<Camera>();
        ShowWhiteThenBlack();
    }

    private void MRto(Transform tr, float time = -1)
    {
        if (time < 0) time = defaultSegwayTime;
        _seq.Append(transform.DOMove(tr.position, time)).Join(transform.DORotateQuaternion(tr.rotation, time));
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        
        if (!_isRotating)
            return;

        transform.RotateAround(worldCenter.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void PlayIntroSequence(object o, Transform eye)
    {
        transform.position = widthPOV.position;
        transform.rotation = widthPOV.rotation;
        _eye = eye;
        _eyeThirdPersonCam = _eye.parent.GetComponentInChildren<Camera>(true);
        var eyeRb = _eye.GetComponent<Rigidbody>();
        var eyeMover = _eye.GetComponent<Mover>();
        var buildingsObjectsSpawnTime = objSpawner.buildingGrowthTime + 8 + 3.3f + 2.5f;
        
        _seq = DOTween.Sequence();
        _seq.AppendInterval(objSpawner.floorGrowthTime + objSpawner.wallWideningTime * 4);
        MRto(OrbitingStart, 4);
        _seq.AppendCallback(() => _isRotating = true);
        _seq.AppendInterval(buildingsObjectsSpawnTime - 2);

        var boxGrowingTime = objSpawner.wallTallingTime * 2 + objSpawner.lidClosingTime + 1.5f;
        
        _seq.AppendCallback(() =>
        {
            _isRotating = false;
            transform.position = BoxOverviewCutTo.position;
            transform.rotation = BoxOverviewCutTo.rotation;
        }).AppendInterval(boxGrowingTime);
        
        _seq.AppendCallback(() => _isRotating = false);
        
        MRto(PortalOpening, 2f);
        
        _seq.Append(doorParent.DOScaleZ(0, portalOpeningTime));
        _seq.AppendInterval(.5f);
        
        MRto(LadderGrowth, 3f);

        _seq.AppendCallback(() => ladderParent.DOScaleZ(TargetLadderScale, ladderGrowthTime));
        _seq.AppendInterval(ladderGrowthStayTime);

        _seq.AppendCallback(() =>
        {
            transform.DOMove(BeforeEye.position, toBeforeEyeTime);
            transform.DORotateQuaternion(BeforeEye.rotation, toBeforeEyeTime);
        });
        
        _seq.AppendInterval(toBeforeEyeTime - 1);
        _seq.AppendCallback(() => _eye.DOShakeRotation(suckInEyeTime + 2, _strength, _vibration, _randomness, false));
        
        _seq.AppendInterval(1);
        _seq.Append(transform.DOMove(_eye.position, suckInEyeTime).SetEase(Ease.InQuart));
        _seq.AppendInterval(1);
        _seq.AppendCallback(() =>
        {
            eyeRb.isKinematic = false;
            eyeRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }).AppendInterval(.3f).AppendCallback(() =>
        {
            _eyeThirdPersonCam.gameObject.SetActive(true);
            _cam.enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
            eyeMover.enabled = true;
        });
    }

    private void EnableIntroObjects(object o, Transform eye)
    {
        _eye = eye;
        _eyeThirdPersonCam = _eye.parent.GetComponentInChildren<Camera>(true);
        var eyeRb = _eye.GetComponent<Rigidbody>();
        var eyeMover = _eye.GetComponent<Mover>();
        var seq = DOTween.Sequence();
        
        seq.AppendCallback(() =>
        {
            eyeRb.isKinematic = false;
            eyeRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            transform.SetPositionAndRotation(_eyeThirdPersonCam.transform.position, _eyeThirdPersonCam.transform.rotation);
        }).AppendInterval(.1f).AppendCallback(() =>
        {
            _cam.enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
            _eyeThirdPersonCam.gameObject.SetActive(true);
            eyeMover.enabled = true;
        });

        ladderParent.gameObject.SetActive(true);
        doorParent.parent.parent.gameObject.SetActive(true);
    }
    
    private void DisableIntroObjects(object o, EventArgs args)
    {
        ladderParent.gameObject.SetActive(false);
        doorParent.parent.parent.gameObject.SetActive(false);
    }

    private void PlayEndingSequence(object o, Transform deathIn)
    {
        _death = deathIn;
        var finalTarget = _death.GetChild(0);
        _cam.farClipPlane = 250000;
        _seq = DOTween.Sequence();
        _seq.AppendInterval(2);

        _seq.AppendCallback(() =>
        {
            transform.position = _eyeThirdPersonCam.transform.position;
            transform.rotation = _eyeThirdPersonCam.transform.rotation;

            _eyeThirdPersonCam.gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(true);
            _cam.enabled = true;
        });


        MRto(BehindEye, 1f);
        _seq.AppendCallback(() => RotateToDeathEvent?.Invoke(this, EventArgs.Empty));
        _seq.AppendInterval(BehindEyeT);
        MRto(SupLarge, 2f);
        _seq.AppendInterval(SupLargeT);
        MRto(CloseUpBehindEye, 2);
        _seq.AppendInterval(CloseUpBehindEyeT);
        _seq.AppendCallback(() =>
        {
            transform.position = finalTarget.position;
            transform.rotation = finalTarget.rotation;
        });
    }

    private void OnGameOver(object o, EventArgs args)
    {
        ShowWhiteThenBlack();
    }
    
    private void ShowWhiteThenBlack()
    {
        _cam.farClipPlane = 100000;
        _faderMat.SetColor(FaderColor, Color.white);
        bool isSecondTime = _death;
        if (isSecondTime)
        {
            Destroy(_death.gameObject);
        }
       
        transform.position = widthPOV.position;
        transform.rotation = widthPOV.rotation;
        

        var seq = DOTween.Sequence();
        seq.AppendInterval(2f);
        seq.Append(_faderMat.DOColor(Color.black, FaderColor, 2f));
        seq.AppendInterval(1f);
        seq.AppendCallback(() => _faderMat.DOColor(new Color(0, 0, 0, 0), FaderColor, 2f));
      
        seq.OnComplete(() =>
        {
            GameStartEvent?.Invoke(this, EventArgs.Empty);
        });
    }
}
