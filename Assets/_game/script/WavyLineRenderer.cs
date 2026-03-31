using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WavyLineRenderer : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    
    [Header("Wave Settings")]
    public int waveSegments = 30;
    public float waveAmplitudeMin = 0.05f;
    public float waveAmplitudeMax = 0.1f;
    public float amplitudeChangeSpeed = 2f;
    public float waveFrequency = 1.5f;
    public float waveSpeed = 30f; 

    private float _waveTime;
    private float _amplitudeTime;
    private float _currentAmplitude = 0.03f;
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private Vector3 _targetEndPoint;
    private Color _baseColor = Color.yellow;
    
    [Header("Target Management")]
    private float targetDuration = 0.03f; 
    private List<GameObject> _targetBlocks = new List<GameObject>();
    private Coroutine _targetProcessCoroutine;
    private GameObject _currentTarget;
    private PigComponent _pigComponent;
    private System.Action _onBulletChanged;
    
    public Material lineMaterial;
    
    private void Awake()
    {
        InitializeLineRenderer();
        _pigComponent = GetComponent<PigComponent>();
    }
    
    private void OnDestroy()
    {
        ClearAllTargets();
    }
    
    public void SetBulletChangedCallback(System.Action callback)
    {
        _onBulletChanged = callback;
    }
    
    public void InitializeLineRenderer(float startWidth = 0.08f, float endWidth = 0.08f, float amplitude = 0.03f)
    {
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            return;
        }
        
        _lineRenderer.startWidth = startWidth;
        _lineRenderer.endWidth = endWidth;
        _lineRenderer.positionCount = waveSegments;
        _currentAmplitude = amplitude;
        
        _lineRenderer.material = lineMaterial;
        
        _lineRenderer.startColor = Color.yellow;
        _lineRenderer.endColor = Color.yellow;
        _lineRenderer.enabled = false;
        _lineRenderer.sortingOrder = 100;
        _lineRenderer.useWorldSpace = true;
    }
    
    public void SetColor(Color color)
    {
        _baseColor = color;
        if (_lineRenderer != null)
        {
            _lineRenderer.material = lineMaterial;
            _lineRenderer.material.color = _baseColor;
            // _lineRenderer.endColor = _baseColor;
        }
    }
    
    public void AddTarget(GameObject block)
    {
        _targetBlocks.Add(block);
        
        if (_targetProcessCoroutine == null)
        {
            _targetProcessCoroutine = StartCoroutine(ProcessTargets());
            // SoundManager.Instance.PlaySoundLoop(SoundManager.Instance.yarn);
        }
    }

    private IEnumerator ProcessTargets()
    {
        while (_targetBlocks.Count > 0)
        {
            
            if (_targetBlocks.Count == 0)
            {
                break;
            }
            
            _currentTarget = _targetBlocks[0];
            
            if (_currentTarget == null)
            {
                if (_targetBlocks.Count > 0)
                {
                    _targetBlocks.RemoveAt(0);
                }
                continue;
            }
            
            _targetEndPoint = _currentTarget.transform.position;
            _endPoint = _targetEndPoint;
            
            Vector3 direction = _endPoint - _startPoint;
            Vector3 right = Vector3.Cross(direction.normalized, Vector3.up).normalized;
            
            if (right == Vector3.zero)
            {
                right = Vector3.Cross(direction.normalized, Vector3.forward).normalized;
            }
            
            for (int i = 0; i < waveSegments; i++)
            {
                float t = i / (float)(waveSegments - 1);
                Vector3 point = Vector3.Lerp(_startPoint, _endPoint, t);
                
                float wave = Mathf.Sin((t * waveFrequency * 2f * Mathf.PI) + _waveTime) * _currentAmplitude;
                point += right * wave;
                
                _lineRenderer.SetPosition(i, point);
            }
            
            _lineRenderer.enabled = true;
            SoundManager.Instance.PlaySoundWhenSourceAvailable(SoundManager.Instance.yarn);

            
            float elapsed = 0f;
            while (elapsed < targetDuration)
            {
                if (_targetBlocks.Count == 0 || _currentTarget == null)
                {
                    break;
                }

                _targetEndPoint = _currentTarget.transform.position;
                _endPoint = _targetEndPoint;
                
                _waveTime += Time.deltaTime * waveSpeed;
                _amplitudeTime += Time.deltaTime * amplitudeChangeSpeed;
                _currentAmplitude = Mathf.Lerp(waveAmplitudeMin, waveAmplitudeMax, 
                    (Mathf.Sin(_amplitudeTime) + 1f) * 0.5f);
                
                Vector3 waveDirection = _endPoint - _startPoint;
                Vector3 waveRight = Vector3.Cross(waveDirection.normalized, Vector3.up).normalized;
                
                if (waveRight == Vector3.zero)
                {
                    waveRight = Vector3.Cross(waveDirection.normalized, Vector3.forward).normalized;
                }
                
                for (int i = 0; i < waveSegments; i++)
                {
                    float t = i / (float)(waveSegments - 1);
                    Vector3 point = Vector3.Lerp(_startPoint, _endPoint, t);
                    float wave = Mathf.Sin((t * waveFrequency * 2f * Mathf.PI) + _waveTime) * _currentAmplitude;
                    point += waveRight * wave;
                    
                    _lineRenderer.SetPosition(i, point);
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_targetBlocks.Count == 0)
            {
                break;
            }

            if (_currentTarget != null)
            {
                _currentTarget.SetActive(false);
                Destroy(_currentTarget);
                EventManager.OnBlockDestroyed?.Invoke();
                
                if (_pigComponent != null)
                {
                    _pigComponent.Bullet--;
                    if (_pigComponent.bulletText != null)
                    {
                        _pigComponent.bulletText.text = _pigComponent.Bullet.ToString();
                    }
                    _onBulletChanged?.Invoke();
                }
            }
            if (_targetBlocks.Count > 0)
            {
                _targetBlocks.RemoveAt(0);
            }
        }
        
        SoundManager.Instance.StopSound(SoundManager.Instance.yarn);
        _lineRenderer.enabled = false;
        _targetProcessCoroutine = null;
        _currentTarget = null;
    }
    
    public void ClearAllTargets()
    {
        if (_targetProcessCoroutine != null)
        {
            StopCoroutine(_targetProcessCoroutine);
            _targetProcessCoroutine = null;
        }

        foreach (GameObject go in _targetBlocks)
        {
            go.GetComponent<Block>().isAlreadyDestroyed = false;
        }
        _targetBlocks.Clear();
        _currentTarget = null;
        _lineRenderer.enabled = false;
        SoundManager.Instance.StopSound(SoundManager.Instance.yarn);
    }


    public void UpdateStartPoint(Vector3 startPoint)
    {
        _startPoint = new Vector3(startPoint.x, startPoint.y + 0.5f, startPoint.z);
    }
    
    public void HideLineImmediately()
    {
        _lineRenderer.enabled = false;
    }

    public bool IsProcessing => _targetProcessCoroutine != null;
    
    public Vector3? GetCurrentTargetPosition()
    {
        if (_currentTarget != null)
        {
            return _currentTarget.transform.position;
        }
        return null;
    }
}



