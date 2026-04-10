using System;
using System.Collections;
using System.Collections.Generic;
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
    // public Animator animator;
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

    public TextMeshProUGUI bulletText;
    public Material hiddenMaterial;
    public Material normalMaterial;
    public Material normalFaceMaterial;
    public List<GameObject> ammoCircles;
    public Transform model;

    public Transform canvasTransform;
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
    private void ChangeState(PigState newState)
    {
        int animValue = 0;
        switch (newState)
        {
            case PigState.InLane:
            case PigState.InQueue:
                // model.localScale = initScale;
                animValue = 0;
                break;
            case PigState.Jumping:
                // animValue = 2;
                break;
            case PigState.Destroying:
            case PigState.OnConveyor:
            case PigState.DoNothing:
                animValue = 1;
                break;
            case PigState.ReadyToJump:
                animValue = 3;
                break;

            case PigState.Shooting:
                animValue = 4;
                break;
        }
        // if (animator.GetInteger("state") != animValue)
        // {
        //     animator.SetInteger("state", animValue);
        // }
        currentState = newState;
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
        initCanvasLocalPos = canvasTransform.localPosition;
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
            bulletText.fontSize = 50f;
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
        currentState = PigState.LoseGame;
        StopCoroutine(_mainCoroutine);
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
                bulletText.fontSize = 40f;
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
        StartMainCoroutine(DestroyAnimationInternal());
    }
    private IEnumerator DestroyAnimationInternal()
    {
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = rb.rotation;
        Vector3 currentPos = rb.position;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
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

            yield return new WaitForFixedUpdate();
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
        StartCoroutine(ReadyToJump(0.1f, onComplete, speed, count));
    }

    public void JumpToTarget(Vector3 localTargetPos)
    {
        Vector3 worldTargetPos = transform.parent.TransformPoint(localTargetPos);
        var distance = Vector3.Distance(this.transform.position, worldTargetPos);
        var intervalDuration = distance / 2;
        StartCoroutine(JumpArcCoroutine(this.transform.position, worldTargetPos, intervalDuration));
    }
    private IEnumerator ConveyorJourney(Action onComplete, float speed, int counter)
    {
        Vector3 firstPoint = allWaypoints[0].position + new Vector3(0, 0.5f * counter, 0);
        float jumpDist = Vector3.Distance(rb.position, firstPoint);
        float jumpDuration = Mathf.Max(0.1f, jumpDist / speed);
        ChangeState(PigState.Jumping);
        yield return StartCoroutine(JumpArcCoroutine(rb.position, firstPoint, jumpDuration, null, jumpToConveyorFB));

        ParticleSystem ps = Instantiate(landOnDiskVFX).GetComponent<ParticleSystem>();
        ps.transform.position = transform.position;
        ps.Play();
        onComplete?.Invoke();
        ChangeState(PigState.CanMove);
        this.model.localScale = new Vector3(0.85f, 0.85f, 0.85f);
        model.rotation = Quaternion.LookRotation(allWaypoints[0].up, allWaypoints[0].forward);
        yield return new WaitForFixedUpdate();

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
        StartMainCoroutine(MovePigThroughWaypoints(0, allWaypoints.Count - 1, allWaypoints));
    }

    private IEnumerator ReadyToJump(float duration, Action onComplete, float speed, int count)
    {
        isOnTop = false;
        float elapsed = 0;
        ChangeState(PigState.ReadyToJump);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        StartMainCoroutine(ConveyorJourney(onComplete, speed, count)); // thay StartCoroutine
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

    public IEnumerator SlideTo(Vector3 target, float speed)
    {
        Vector3 start = rb.position;
        float duration = Vector3.Distance(start, target) / speed;
        float elapsed = 0;
        while (elapsed < duration)
        {
            // if (currentState == PigState.Destroying || currentState == PigState.InQueue) yield break;
            elapsed += Time.deltaTime;
            Vector3 newPos = Vector3.Lerp(start, target, elapsed / duration);
            rb.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }
        rb.MovePosition(target);
    }

    private IEnumerator MovePigThroughWaypoints(int startIndex, int targetIndex, List<Transform> path)
    {
        int i = startIndex;
        while (true)
        {
            // if (currentState == PigState.LoseGame)
            // {
            //     yield break;
            // }
            var current = path[i];
            var next = path[(i + 1) % path.Count];

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

                yield return StartCoroutine(SlideOnCurve(start, control, end, startRotAligned, endRotAligned, speed));
                i += 2;
            }
            else
            {
                // if (currentState == PigState.Destroying || currentState == PigState.InQueue) yield break;
                model.rotation = targetRot;
                Vector3 end = path[i + 1].position;
                yield return StartCoroutine(SlideTo(end, speed));
                i++;
            }

            if (i >= path.Count - 1) i = 0;
        }
    }

    public IEnumerator SlideOnCurve(Vector3 start, Vector3 control, Vector3 end, Quaternion startRotation, Quaternion endRotation, float speed)
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
            // if (currentState == PigState.Destroying || currentState == PigState.InQueue) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 position = Mathf.Pow(1 - t, 2) * start +
                               2 * (1 - t) * t * control +
                               Mathf.Pow(t, 2) * end;

            rb.MovePosition(position);

            model.rotation = Quaternion.Lerp(startRotation, endRotation, t);

            yield return new WaitForFixedUpdate();
        }
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

        // jumpToQueueFB?.PlayFeedbacks();
        _lockedTargets = 0;
        model.rotation = Quaternion.identity;
        ChangeState(PigState.Jumping); // Stop FixedUpdate interference and prevent OnTriggerEnter re-firing
        var distance = Vector3.Distance(this.transform.position, targetPosition);
        // var intervalDuration = distance / speed;


        StartMainCoroutine(JumpArcCoroutine(rb.position, targetPosition, 0.4f, () =>
        {
            ChangeState(PigState.InQueue);
            canvasTransform.localPosition = initCanvasLocalPos;
            StartCoroutine(ScaleUpAndDownWhenEnterQueue());
            onComplete?.Invoke();
        }));
    }

    private IEnumerator JumpArcCoroutine(Vector3 startPos, Vector3 endPos, float duration, Action onComplete = null, MMF_Player jumpFeedback = null)
    {
        float timeSpent = 0f;
        jumpFeedback?.PlayFeedbacks();
        while (timeSpent < duration)
        {
            // if (currentState == PigState.OnConveyor) yield break;

            timeSpent += Time.fixedDeltaTime;
            float percent = timeSpent / duration;

            Vector3 currentPos = MMTween.Tween(timeSpent, 0f, duration, startPos, endPos, jumpCurve);

            float arcOffset = Mathf.Sin(percent * Mathf.PI) * 2f;

            currentPos.y += arcOffset;
            rb.MovePosition(currentPos);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(endPos);
        onComplete?.Invoke();
    }

    private IEnumerator ScaleUpAndDownWhenEnterQueue()
    {
        float duration = 0.2f;
        float elapsed = 0;
        model.localScale = initScale;
        Vector3 startScale = initScale;
        Vector3 maxScale = startScale * 1.2f;

        while (elapsed < duration)
        {
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

            yield return null;
        }
        // model.localScale = startScale;
        model.localScale = initScale;
    }
    public void MoveInQueue(Vector3 targetPos, Quaternion targetRot)
    {

        ChangeState(PigState.InQueue);
        StartMainCoroutine(MoveInQueueCoroutine(targetPos, targetRot));
    }

    public void MoveToStack(Vector3 targetPos)
    {
        StartCoroutine(MoveToStackCoroutine(targetPos));
    }

    private IEnumerator MoveToStackCoroutine(Vector3 targetPos)
    {
        ChangeState(PigState.InQueue);
        Vector3 start = rb.position;
        float distance = Vector3.Distance(start, targetPos);
        float duration = Mathf.Max(0.1f, distance / moveSpeed);
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector3.Lerp(start, targetPos, elapsed / duration));
            yield return new WaitForFixedUpdate();
        }
        rb.MovePosition(targetPos);
        ChangeState(PigState.CanMove);
    }

    private IEnumerator MoveInQueueCoroutine(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = rb.position;
        float distance = Vector3.Distance(new Vector3(startPos.x, 0, startPos.z), new Vector3(targetPos.x, 0, targetPos.z));
        float duration = distance / moveSpeed;
        float jumpHeight = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            if (this == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            rb.MovePosition(currentPos);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(targetPos);
        yield return new WaitForFixedUpdate();

    }

    public void SetConveyorSpeedMultiplier(float multiplier)
    {
        float clamped = Mathf.Max(0.1f, multiplier);
        speed = baseConveyorSpeed * clamped;
        _wavyLine.SetSpeedMultiplier(clamped);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("currentState: " + currentState + ", isRush: " + isRush);
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
    private void StartMainCoroutine(IEnumerator routine)
    {
        Debug.Log("Starting new main coroutine: " + routine);
        if (_mainCoroutine != null)
            StopCoroutine(_mainCoroutine);
        _mainCoroutine = StartCoroutine(routine);
    }
}

