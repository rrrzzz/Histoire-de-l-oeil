using System;
using DG.Tweening;
using UnityEngine;

public class DeathController : MonoBehaviour
{

    public static EventHandler<Transform> CameraSwitchEvent;
    public static EventHandler GameOverEvent;
    
    
    [SerializeField] private float moveTime;
    [SerializeField] private Animator anim;
    [SerializeField] private float distToEye;
    [SerializeField] private float eyeOffsetToGetInside;
    [SerializeField] private float pauseTime;
    [SerializeField] private float animWaitTime;
    [SerializeField] private float toEyeFinal;
    [SerializeField] private float flareTime;

    [SerializeField] private Material _origSb;
    [SerializeField] private Material _newSb;
    
    private static readonly int AtmTh = Shader.PropertyToID("_AtmosphereThickness");
    private static readonly int Exp = Shader.PropertyToID("_Exposure");
    private static readonly int GrColor = Shader.PropertyToID("_GroundColor");
    private static readonly int SkTint = Shader.PropertyToID("_SkyTint");

    private float _origAtTh;
    private float _origExp;
    private Color _origGrCol;
    private Color _origSkyTint;
    
    private float _newAtTh;
    private float _newExp;
    private Color _newGrCol;
    private Color _newSkyTint;

    private Material _curSb;
    private float _initialX;
    private float _initialDist;

    private bool _isMoving;
    
    private LensFlare _flare;
    private Transform _eye;

    private void Start()
    {
        _flare = GetComponentInChildren<LensFlare>();

        _curSb = RenderSettings.skybox;
        _origAtTh = _origSb.GetFloat(AtmTh);
        _origExp = _origSb.GetFloat(Exp);
        _origGrCol = _origSb.GetColor(GrColor);
        _origSkyTint = _origSb.GetColor(SkTint);
        
        _newAtTh = _newSb.GetFloat(AtmTh);
        _newExp = _newSb.GetFloat(Exp);
        _newGrCol =_newSb.GetColor(GrColor); 
        _newSkyTint = _newSb.GetColor(SkTint);

        ObjectSpawner.EndingStartedEvent += EnterDeath;
    }

    private void Update()
    {
        if (!_isMoving) return;
        var distanceTraveled = _initialX - transform.position.x;
        var p = (distanceTraveled / _initialDist - .75f) * 4;
        var curAtTh = Mathf.Lerp(_origAtTh, _newAtTh, p);
        var curExp = Mathf.Lerp(_origExp, _newExp, p);
        var curGrCol = Color.Lerp(_origGrCol, _newGrCol, p);
        var curSkyTint = Color.Lerp(_origSkyTint, _newSkyTint, p);
            
        _curSb.SetFloat(AtmTh, curAtTh);
        _curSb.SetFloat(Exp, curExp);
        _curSb.SetColor(GrColor, curGrCol);
        _curSb.SetColor(SkTint, curSkyTint);
    }                    
    
    private void EnterDeath(object sender, Transform eyeInstance)
    {
        _eye = eyeInstance;
        PlayEndingDeathSequence();
    }

    private void PlayEndingDeathSequence()
    {
        var target = new Vector3(_eye.position.x - distToEye, transform.position.y, _eye.position.z);
        _initialX = transform.position.x;
        _initialDist = transform.position.x - target.x;

        _isMoving = true;
        CameraSwitchEvent?.Invoke(this, transform);
        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(target, moveTime).OnComplete(() => _isMoving = false));
        
        seq.AppendInterval(pauseTime);
        seq.AppendCallback(() =>
        {
            anim.enabled = true;
        });
        seq.AppendInterval(animWaitTime);
        
        seq.AppendCallback(() =>
        {
            transform.DOMoveX(_eye.position.x + eyeOffsetToGetInside, toEyeFinal);
        });
        
        seq.AppendInterval(1);
        seq.AppendCallback(() => _flare.enabled = true);
        seq.Append(
            DOTween.To(() => _flare.brightness, x => _flare.brightness = x, 150, flareTime).SetEase(Ease.InQuint));
        seq.AppendCallback(() =>
        {
            ResetState();
            GameOverEvent?.Invoke(this, EventArgs.Empty);
        });
    }

    private void OnDestroy()
    {
        ResetState();
    }

    private void ResetState()
    {
        _curSb.SetFloat(AtmTh, _origAtTh);
        _curSb.SetFloat(Exp, _origExp);
        _curSb.SetColor(GrColor, _origGrCol);
        _curSb.SetColor(SkTint, _origSkyTint);
    }
}
