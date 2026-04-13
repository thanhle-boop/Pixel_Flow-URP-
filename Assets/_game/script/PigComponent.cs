using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

public class PigComponent : MonoBehaviour
{
    [Header("Pig Info")]
    public string color;
    public int Bullet;
    public int laneIndex;
    public float moveSpeed = 6f;
    public bool isHidden = false;
    public bool isOnTop = false;

    public bool isRush = false;
    private float speed = 0f;
    private float baseConveyorSpeed = 0f;
    private int _lockedTargets = 0;

    private int _bulletsPerCircle;
    private int _currentCircleIndex = -1;

    private int _bulletsFiredCount = 0;

    public PigComponent leftPig { get; private set; } = null;
    public PigComponent rightPig { get; private set; } = null;
    public PigState currentState = PigState.InLane;
    private Vector3 _rayCastDirection = Vector3.forward;
    private WavyLineRenderer _wavyLine;

    private Vector3 initScale;

    [Header("References")]
    public Animator animator;
    private List<Transform> allWaypoints = new List<Transform>();
    public Transform rayCastPoint;
    public Transform wavyPoint;
    public Rigidbody rb;
    public LayerMask blockLayer;
    public GameObject faceModel;
    public GameObject face2Model;
    public GameObject bodyModel;
    public GameObject body2Model;
    public GameObject tailModel;
    public GameObject Model;

    public TextMeshPro bulletText;
    public Material hiddenMaterial;
    public Material normalMaterial;
    public Material normalFaceMaterial;
    public List<GameObject> ammoCircles;
    public Transform model;

    // public Transform canvasTransform;
    private Vector3 initCanvasLocalPos;
    public Transform currentPlate;
    public GameObject spoolDissapearVFX;
    public GameObject landOnDiskVFX;
    public GameObject unlockVFX;
    public bool isPlayeVFX = false;
    public AnimationCurve ArcCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    public MMTween.MMTweenCurve jumpCurve;
    public MMTween.MMTweenCurve moveCurve;
    public MMF_Player jumpToConveyorFB;
    // public MMF_Player jumpToQueueFB;
    // public MMF_Player scaleDownFB;
    public MMF_Player changeColorFeedBack;
    private Coroutine _mainCoroutine;
    private System.Threading.CancellationTokenSource _stateCTS;
    private void ChangeState(PigState newState)
    {
        currentState = newState;

        if (newState == PigState.DoNothing)
        {
            animator.enabled = false;
            return; 
        }

        animator.enabled = true;
        int animValue = 0;
        switch (newState)
        {
            case PigState.InQueue: animValue = 0; break;
            case PigState.ReadyToJump: animValue = 1; break;
            case PigState.OnConveyor: animValue = 2; break;
            case PigState.Shooting: animValue = 3; break;
            case PigState.Jumping: animValue = 4; break;
        }

        if (animator.runtimeAnimatorController != null)
        {
            animator.SetInteger("state", animValue);
        }
    }
    public void Initialize(string color, int bulletCount, int laneIndex, Color lineColor, float _speed, List<Transform> paths, bool isHidden)
    {
        this.isHidden = isHidden;
        this.color = color;
        this.Bullet = bulletCount;
        this.laneIndex = laneIndex;

        this.speed = _speed;
        this.baseConveyorSpeed = _speed;
        this._bulletsFiredCount = 0;
        initCanvasLocalPos = bulletText.transform.localPosition;
        initScale = model.localScale;

        ChangeState(PigState.DoNothing);
        allWaypoints = paths;

        _bulletsPerCircle = Mathf.CeilToInt((float)bulletCount / 5f);

        foreach (var circle in ammoCircles)
        {
            if (circle != null) circle.SetActive(false);
        }
        _currentCircleIndex = -1;

        if (_wavyLine == null)
        {
            _wavyLine = GetComponent<WavyLineRenderer>();
            if (_wavyLine == null)
            {
                _wavyLine = gameObject.AddComponent<WavyLineRenderer>();
            }
        }

        _wavyLine.SetColor(lineColor);
        _wavyLine.SetBulletChangedCallback(OnBulletChanged);
        var faceMeshRenderer = faceModel.GetComponent<MeshRenderer>();
        var face2MeshRenderer = face2Model.GetComponent<MeshRenderer>();
        var bodyMeshRenderer = bodyModel.GetComponent<MeshRenderer>();
        var body2MeshRenderer = body2Model.GetComponent<MeshRenderer>();
        var tailMeshRenderer = tailModel.GetComponent<MeshRenderer>();

        for (int i = 0; i < ammoCircles.Count; i++)
        {
            var meshRenderer2 = ammoCircles[i].GetComponent<MeshRenderer>();
            meshRenderer2.material.color = ColorGameConfig.instance.GetColorByName(color);

        }
        if (isHidden)
        {
            faceMeshRenderer.material = hiddenMaterial;
            face2MeshRenderer.material = hiddenMaterial;
            bodyMeshRenderer.material = hiddenMaterial;
            body2MeshRenderer.material = hiddenMaterial;
            tailMeshRenderer.material = hiddenMaterial;

            bulletText.text = "?";
            bulletText.fontSize = 5f;
            return;
        }
        bulletText.text = bulletCount.ToString();
        tailMeshRenderer.material.color = ColorGameConfig.instance.GetColorByName(color);
        faceMeshRenderer.material.color = ColorGameConfig.instance.GetColorByName(color);
        face2MeshRenderer.material.color = ColorGameConfig.instance.GetColorByName(color);
        bodyMeshRenderer.material.color = ColorGameConfig.instance.GetColorByName(color);
        body2MeshRenderer.material.color = ColorGameConfig.instance.GetColorByName(color);
    }

    public bool IsOnFirstRow()
    {
        return isOnTop;
    }

    public bool IsLinkedPig()
    {
        return leftPig != null || rightPig != null;
    }
    public bool IsWholeLinkOnTop()
    {
        PigComponent leftmost = GetLeftmostPig();
        PigComponent current = leftmost;
        HashSet<PigComponent> visited = new HashSet<PigComponent>();
        while (current != null && visited.Add(current))
        {
            if (!current.IsOnFirstRow()) return false;
            current = current.rightPig;
        }
        return true;
    }

    public bool IsPigValid()
    {
        return (currentState == PigState.InLane || currentState == PigState.InQueue) && isOnTop;
    }
    public PigComponent GetLeftmostPig()
    {
        PigComponent current = this;
        HashSet<PigComponent> visited = new HashSet<PigComponent> { current };
        while (current.leftPig != null && visited.Add(current.leftPig))
        {
            current = current.leftPig;
        }
        return current;
    }
    public void SetLinkPig(PigComponent left, PigComponent right)
    {
        if (leftPig == null)
            leftPig = left;
        if (rightPig == null)
        {
            rightPig = right;
        }
    }
    public void SetLinkedNeighbors(PigComponent left, PigComponent right)
    {
        leftPig = left;
        rightPig = right;
    }
    public void GameOver()
    {
        if(currentState != PigState.Destroying)
            StopCurrentLogic();

    }
    public void SetIsOnTop(bool value)
    {
        isOnTop = value;
        if (value)
        {
            ChangeState(PigState.InLane);

            if (isHidden)
            {
                ParticleSystem unlockEffect = Instantiate(unlockVFX).GetComponent<ParticleSystem>();
                unlockEffect.transform.SetParent(transform);
                unlockEffect.transform.localPosition = Vector3.up * 0.5f;
                unlockEffect.Play();

                isHidden = false;
                var faceRenderer = faceModel.GetComponent<MeshRenderer>();
                var face2Renderer = face2Model.GetComponent<MeshRenderer>();
                var tailRenderer = tailModel.GetComponent<MeshRenderer>();
                var bodyMeshRenderer = bodyModel.GetComponent<MeshRenderer>();
                var body2MeshRenderer = body2Model.GetComponent<MeshRenderer>();
                tailRenderer.material = normalMaterial;
                bodyMeshRenderer.material = normalMaterial;
                body2MeshRenderer.material = normalMaterial;
                faceRenderer.material = normalFaceMaterial;
                face2Renderer.material = normalFaceMaterial;

                bulletText.text = Bullet.ToString();
                bulletText.fontSize = 4f;
                var gradient = new Gradient()
                {
                    colorKeys = new GradientColorKey[]
                    {
                        new GradientColorKey(Color.white, 0f),
                        new GradientColorKey(ColorGameConfig.instance.GetColorByName(color), 1f)
                    },
                    alphaKeys = new GradientAlphaKey[]
                    {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 1f)
                    }
                };
                var shaderControllerBody = bodyModel.GetComponent<ShaderController>();
                SetTranslateColor(shaderControllerBody, gradient, bodyMeshRenderer);
                var shaderControllerBody2 = body2Model.GetComponent<ShaderController>();
                SetTranslateColor(shaderControllerBody2, gradient, body2MeshRenderer);
                var shaderControllerTail = tailModel.GetComponent<ShaderController>();
                SetTranslateColor(shaderControllerTail, gradient, tailRenderer);
                var shaderControllerFace = faceModel.GetComponent<ShaderController>();
                SetTranslateColor(shaderControllerFace, gradient, faceRenderer);
                var shaderControllerFace2 = face2Model.GetComponent<ShaderController>();
                SetTranslateColor(shaderControllerFace2, gradient, face2Renderer);

                changeColorFeedBack.PlayFeedbacks();
            }
            if (IsLinkedPig())
            {
                EventManager.OnPigIsOnTopNoMoreHidden?.Invoke(this);
            }
        }
    }

    private void SetTranslateColor(ShaderController controller, Gradient gradient, MeshRenderer renderer)
    {
        controller.TargetMaterial = renderer.material;
        controller.FromColor = Color.white;
        controller.ColorRamp = gradient;
        controller.ToColor = ColorGameConfig.instance.GetColorByName(color);
    }

    private void OnBulletChanged()
    {
        if (_lockedTargets > 0)
        {
            _lockedTargets--;
        }

        _bulletsFiredCount++;

        int targetCircleIdx = (_bulletsFiredCount - 1) / _bulletsPerCircle;
        if (targetCircleIdx >= 0 && targetCircleIdx < ammoCircles.Count && targetCircleIdx > _currentCircleIndex)
        {
            if (_currentCircleIndex >= 0)
            {
                StartCoroutine(ScaleCircleRoutine(ammoCircles[_currentCircleIndex]));
            }

            _currentCircleIndex = targetCircleIdx;
            ammoCircles[_currentCircleIndex].SetActive(true);
            ammoCircles[_currentCircleIndex].transform.localScale = new Vector3(1f, 1f, 1f);
        }

        if (Bullet <= 0)
        {

            if (_currentCircleIndex >= 0)
            {
                StartCoroutine(ScaleCircleRoutine(ammoCircles[_currentCircleIndex]));
            }

            ChangeState(PigState.OnConveyor);
            bulletText.text = "0";
            EventManager.OnPigOutOfAmmo?.Invoke(this);
        }
    }

    private IEnumerator ScaleCircleRoutine(GameObject circle)
    {
        if (circle == null) yield break;
        Transform t = circle.transform;
        float durationScaleUp = 0.1f;
        float durationScaleDown = 0.25f;

        float elapsed = 0;
        Vector3 startScale = new Vector3(1f, 1f, 1f);
        Vector3 peakScale = new Vector3(1.7f, 1f, 1.7f);

        while (elapsed < durationScaleUp)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(startScale, peakScale, elapsed / durationScaleUp);
            yield return null;
        }

        elapsed = 0;
        while (elapsed < durationScaleDown)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(peakScale, startScale, elapsed / durationScaleDown);
            yield return null;
        }

        t.localScale = startScale;
    }

    public void ExecuteDestroy()
    {
        if (_wavyLine != null)
            _wavyLine.HideLineImmediately();
        _lockedTargets = 0;
        AudioController.instance.PlaySound(AudioIndex.destroy_cat.ToString());
        ChangeState(PigState.Destroying);
        bulletText.text = "";
        StartNewLogic(async token => await DestroyAnimationInternal(token));
    }
    private async UniTask DestroyAnimationInternal(System.Threading.CancellationToken token)
    {
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = rb.rotation;
        Vector3 currentPos = rb.position;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested) return;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rb.MovePosition(currentPos);

            float scaleT;
            if (t <= 0.4f)
            {
                scaleT = 0f;
            }
            else if (t <= 0.9f)
            {
                scaleT = (t - 0.4f) / 0.5f;
            }
            else
            {
                scaleT = 1f;
                if (!isPlayeVFX)
                {
                    ParticleSystem ps = Instantiate(spoolDissapearVFX, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity).GetComponent<ParticleSystem>();
                    ps.Play();
                    isPlayeVFX = true;
                }
            }

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, scaleT);
            rb.MoveRotation(startRotation * Quaternion.Euler(0f, t * 360f, 0f));

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }

        EventManager.OnPigDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    public void StopShooting()
    {
        if (_wavyLine != null)
        {
            _wavyLine.ClearAllTargets();
            _wavyLine.HideLineImmediately();
        }
        _lockedTargets = 0;
    }

    public void JumpTo(float speed, int count, Action onComplete = null)
    {
        SetConveyorSpeedMultiplier(isRush ? 2f : 1f);
        EventManager.OnJumpToConveyor?.Invoke();
        StartNewLogic(async token => await ReadyToJump(0.1f, onComplete, speed, count, token));
    }

    public void JumpToTarget(Vector3 localTargetPos)
    {
        Vector3 worldTargetPos = transform.parent.TransformPoint(localTargetPos);
        var distance = Vector3.Distance(this.transform.position, worldTargetPos);
        var intervalDuration = distance / 2;
        StartNewLogic(async token =>
            await JumpArcCoroutine(this.transform.position, worldTargetPos, intervalDuration, token));
    }
    private async UniTask ConveyorJourney(Action onComplete, float jumpDuration, int counter, System.Threading.CancellationToken token)
    {
        Vector3 firstPoint = allWaypoints[0].position + new Vector3(0, 0.5f * counter, 0);
        ChangeState(PigState.Jumping);

        await JumpArcCoroutine(rb.position, firstPoint, jumpDuration, token, jumpToConveyorFB);
        this.model.localScale = new Vector3(0.85f, 0.85f, 0.85f);
        model.rotation = Quaternion.Euler(-90, 0, 0);
        ParticleSystem ps = Instantiate(landOnDiskVFX).GetComponent<ParticleSystem>();
        ps.transform.position = transform.position;
        ps.Play();
        ChangeState(PigState.CanMove);
        onComplete?.Invoke();

        _rayCastDirection = allWaypoints[0].forward;
        if (_wavyLine != null)
        {
            _wavyLine.UpdateStartPoint(wavyPoint.position);
        }
    }

    public bool isFirstPgInStack()
    {
        return Vector3.Distance(transform.position, allWaypoints[0].position) < 0.05f;
    }

    public void StartMove()
    {
        ChangeState(PigState.OnConveyor);
        currentPlate.SetParent(transform);
        currentPlate.localPosition = new Vector3(-0.035f, -0.25f, -0.079f);
        currentPlate.localRotation = Quaternion.Euler(0, 0, 90);
        StartNewLogic(async token => await MovePigThroughWaypoints(0, allWaypoints.Count - 1, allWaypoints, token));
    }

    private async UniTask ReadyToJump(float duration, Action onComplete, float speed, int count, System.Threading.CancellationToken token)
    {
        isOnTop = false;
        float elapsed = 0;
        ChangeState(PigState.ReadyToJump);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }

        if (token.IsCancellationRequested) return;
        await ConveyorJourney(onComplete, speed, count, token);
    }
    public void MoveTo(Vector3 newLocalPos)
    {
        StartCoroutine(MoveCoroutine(newLocalPos));
    }
    private IEnumerator MoveCoroutine(Vector3 targetLocalPos)
    {
        Vector3 startLocalPos = transform.localPosition;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 newLocalPos = Vector3.Lerp(startLocalPos, targetLocalPos, t);
            Vector3 newWorldPos = transform.parent.TransformPoint(newLocalPos);
            rb.MovePosition(newWorldPos);
            yield return new WaitForFixedUpdate();
        }

        Vector3 finalWorldPos = transform.parent.TransformPoint(targetLocalPos);
        rb.MovePosition(finalWorldPos);
    }

    private void CheckAndAddTargetBlocks()
    {
        if (Bullet - _lockedTargets <= 0) return;

        float checkDistance = 10f;
        Vector3 currentPos = rayCastPoint.position;

        if (Physics.Raycast(currentPos, _rayCastDirection, out RaycastHit hit, checkDistance, blockLayer))
        {
            var blockComp = hit.collider.gameObject.GetComponent<Block>();

            if (blockComp != null && blockComp.color == color)
            {
                if (!blockComp.isAlreadyDestroyed)
                {
                    blockComp.isAlreadyDestroyed = true;

                    _wavyLine.AddTarget(hit.collider.gameObject);
                    _lockedTargets++;
                }
            }
        }
    }

    public async UniTask SlideTo(Vector3 target, float speed, System.Threading.CancellationToken token)
    {
        Vector3 start = rb.position;
        float duration = Vector3.Distance(start, target) / speed;
        float elapsed = 0;
        while (elapsed < duration)
        {
            if (token.IsCancellationRequested) return;
            elapsed += Time.deltaTime;
            Vector3 newPos = Vector3.Lerp(start, target, elapsed / duration);
            rb.MovePosition(newPos);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
        if (this == null || token.IsCancellationRequested) return;
        rb.MovePosition(target);
    }

    private async UniTask MovePigThroughWaypoints(int startIndex, int targetIndex, List<Transform> path, System.Threading.CancellationToken token)
    {
        int i = startIndex;
        while (true)
        {
            if (token.IsCancellationRequested) return;
            var current = path[i];

            Quaternion targetRot = Quaternion.LookRotation(current.up, current.forward);

            if (Mathf.Abs(Vector3.Dot(_rayCastDirection, current.forward)) < 0.01f)
            {
                _rayCastDirection = current.transform.forward;
            }

            if (current.gameObject.CompareTag("ControlPos") && i + 2 <= targetIndex)
            {
                Vector3 start = path[i].position;
                Vector3 control = path[i + 1].position;
                Vector3 end = path[i + 2].position;

                Quaternion startRotAligned = Quaternion.LookRotation(path[i].up, path[i].forward);
                Quaternion endRotAligned = Quaternion.LookRotation(path[i + 2].up, path[i + 2].forward);

                await SlideOnCurve(start, control, end, startRotAligned, endRotAligned, speed, token);
                i += 2;
            }
            else
            {
                model.rotation = targetRot;
                Vector3 end = path[i + 1].position;
                await SlideTo(end, speed, token);
                i++;
            }

            if (i >= path.Count - 1) i = 0;
        }
    }

    public async UniTask SlideOnCurve(Vector3 start, Vector3 control, Vector3 end, Quaternion startRotation, Quaternion endRotation, float speed, System.Threading.CancellationToken token)
    {
        const int samples = 20;
        float curveLength = 0f;
        Vector3 prev = start;
        for (int s = 1; s <= samples; s++)
        {
            float st = s / (float)samples;
            Vector3 pt = Mathf.Pow(1 - st, 2) * start +
                         2 * (1 - st) * st * control +
                         Mathf.Pow(st, 2) * end;
            curveLength += Vector3.Distance(prev, pt);
            prev = pt;
        }

        float duration = curveLength / speed;
        float elapsed = 0;

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested) return;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 position = Mathf.Pow(1 - t, 2) * start +
                               2 * (1 - t) * t * control +
                               Mathf.Pow(t, 2) * end;

            rb.MovePosition(position);

            model.rotation = Quaternion.Lerp(startRotation, endRotation, t);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
        if (this == null || token.IsCancellationRequested) return;
        rb.MovePosition(end);
        model.rotation = endRotation;
    }

    public void JumpToQueue(Vector3 targetPosition, float speed, Action onComplete = null)
    {
        if (_wavyLine != null)
        {
            _wavyLine.ClearAllTargets();
            _wavyLine.HideLineImmediately();
        }


        _lockedTargets = 0;
        model.rotation = Quaternion.identity;
        ChangeState(PigState.DoNothing);
        StartNewLogic(async token =>
        {
            await JumpArcCoroutine(rb.position, targetPosition, speed, token);
            if (token.IsCancellationRequested) return;
            ScaleUpAndDownWhenEnterQueue(token).Forget();
            bulletText.transform.localPosition = initCanvasLocalPos;
            isOnTop = true;
            ChangeState(PigState.InQueue);
            onComplete?.Invoke();
        });

    }

    private async UniTask JumpArcCoroutine(Vector3 startPos, Vector3 endPos, float duration, System.Threading.CancellationToken token, MMF_Player jumpFeedback = null)
    {
        float timeSpent = 0f;
        if (jumpFeedback != null)
        {
            jumpFeedback.StopFeedbacks();
            jumpFeedback.PlayFeedbacks();
        }

        while (timeSpent < duration)
        {
            if (token.IsCancellationRequested) return;
            timeSpent += Time.fixedDeltaTime;
            float percent = timeSpent / duration;
            Vector3 currentPos = MMTween.Tween(timeSpent, 0f, duration, startPos, endPos, jumpCurve);
            float arcOffset = Mathf.Sin(percent * Mathf.PI) * 2f;
            currentPos.y += arcOffset;
            rb.MovePosition(currentPos);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
        if (this == null || token.IsCancellationRequested) return;
        rb.MovePosition(endPos);
        // onComplete?.Invoke();
    }

    private async UniTask ScaleUpAndDownWhenEnterQueue(System.Threading.CancellationToken token)
    {

        float duration = 0.2f;
        float elapsed = 0;
        model.localScale = initScale;
        Vector3 startScale = initScale;
        Vector3 maxScale = startScale * 1.2f;

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested) return;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t <= 0.5f)
            {
                model.localScale = Vector3.Lerp(startScale, maxScale, t * 2f);
            }
            else
            {
                model.localScale = Vector3.Lerp(maxScale, startScale, (t - 0.5f) * 2f);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        ChangeState(PigState.InQueue);
        model.localScale = initScale;
    }
    public void MoveInQueue(Vector3 targetPos, Quaternion targetRot)
    {
        // ChangeState(PigState.InQueue);
        StartNewLogic(async token => await MoveInQueueCoroutine(targetPos, targetRot, token));
    }

    public void MoveToStack(Vector3 targetPos)
    {
        StartNewLogic(async token => await MoveToStackCoroutine(targetPos, token));
    }

    private async UniTask MoveToStackCoroutine(Vector3 targetPos, System.Threading.CancellationToken token)
    {
        // ChangeState(PigState.InQueue);
        Vector3 start = rb.position;
        float distance = Vector3.Distance(start, targetPos);
        float duration = Mathf.Max(0.1f, distance / moveSpeed);
        float elapsed = 0;
        // rb.rotation = Quaternion.Euler(-90, 0, 0);
        while (elapsed < duration)
        {
            if (token.IsCancellationRequested) return;
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector3.Lerp(start, targetPos, elapsed / duration));
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
        // ChangeState(PigState.CanMove);
        if (this == null || token.IsCancellationRequested) return;
        rb.MovePosition(targetPos);
        ChangeState(PigState.CanMove);
    }

    private async UniTask MoveInQueueCoroutine(Vector3 targetPos, Quaternion targetRot, System.Threading.CancellationToken token)
    {
        Vector3 startPos = rb.position;
        float distance = Vector3.Distance(new Vector3(startPos.x, 0, startPos.z), new Vector3(targetPos.x, 0, targetPos.z));
        float duration = distance / moveSpeed;
        float jumpHeight = 0.5f;
        float elapsed = 0;
        // ChangeState(PigState.InQueue);

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested) return;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            rb.MovePosition(currentPos);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
        if (this == null || token.IsCancellationRequested) return;
        rb.MovePosition(targetPos);
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

    }

    public void SetConveyorSpeedMultiplier(float multiplier)
    {
        float clamped = Mathf.Max(0.1f, multiplier);
        speed = baseConveyorSpeed * clamped;
        _wavyLine.SetSpeedMultiplier(clamped);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EndConveyor") && !isRush)
        {
            EventManager.OnPigEnterQueue?.Invoke(this);
        }
    }

    private void FixedUpdate()
    {
        if (Bullet > 0 && (currentState == PigState.OnConveyor || currentState == PigState.Shooting))
        {
            _wavyLine.UpdateStartPoint(wavyPoint.position);

            CheckAndAddTargetBlocks();

            if (_lockedTargets > 0)
            {
                ChangeState(PigState.Shooting);
            }
            else
            {
                ChangeState(PigState.OnConveyor);
            }
        }
    }


    public void StartNewLogic(Func<System.Threading.CancellationToken, UniTask> taskFactory)
    {
        _stateCTS?.Cancel();
        _stateCTS?.Dispose();
        _stateCTS = new System.Threading.CancellationTokenSource();

        var linkedCTS = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(
            _stateCTS.Token,
            this.GetCancellationTokenOnDestroy()
        );

        taskFactory(linkedCTS.Token)
            .ContinueWith(() => linkedCTS.Dispose())
            .Forget();
    }

    public void StopCurrentLogic()
    {
        _stateCTS?.Cancel();
        _stateCTS?.Dispose();
        _stateCTS = null;
    }
}

