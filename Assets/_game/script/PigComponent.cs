using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PigComponent : MonoBehaviour
{
    [Header("Pig Info")]
    public string color;
    public int Bullet;
    public int laneIndex;
    private float jumpToQueueSpeed;
    public bool isHidden = false;
    private bool isOnTop = false;
    public bool isOnBelt = false;
    private float speed = 0f;
    private float baseConveyorSpeed = 0f;
    private int _lockedTargets = 0;

    private int _bulletsPerCircle;
    private int _currentCircleIndex = -1;

    private int _bulletsFiredCount = 0;
    private Quaternion initialRotation;

    public PigComponent leftPig { get; private set; } = null;
    public PigComponent rightPig { get; private set; } = null;
    private PigState currentState = PigState.InLane;
    private Vector3 _rayCastDirection = Vector3.forward;
    private WavyLineRenderer _wavyLine;

    [Header("References")]
    public Animator animator;
    private List<Transform> allWaypoints = new List<Transform>();
    public Transform rayCastPoint;
    public Rigidbody rb;
    public LayerMask blockLayer;

    public GameObject faceModel;
    public GameObject bodyModel;

    public TextMeshProUGUI bulletText;
    public Material hiddenMaterial;
    public Material normalMaterial;
    public List<GameObject> ammoCircles;
    public Transform model;

    public Transform canvasTransform;
    private Vector3 initCanvasLocalPos;
    public Transform currentPlate;
    private float _currentShakeAngle = 0f;
    private void ChangeState(PigState newState)
    {

        int animValue = 0;
        switch (newState)
        {
            case PigState.InLane:
            case PigState.InQueue:
                animValue = 0; // Ứng với State 1 trong Animator
                break;
            case PigState.Jumping:
                animValue = 2; // Ứng với State 2 (id 2) trong Animator
                break;
            case PigState.OnConveyor:
                animValue = 1; // Ứng với State 3 (id 1) trong Animator
                break;
            case PigState.ReadyToJump:
                animValue = 3; // Ứng với State 4 (id 3) trong Animator
                break;

            case PigState.Shooting:
                animValue = 4; // Ứng với State 5 (id 4) trong Animator
                break;
        }

        if (animator.GetInteger("state") != animValue)
        {
            animator.SetInteger("state", animValue);
        }
        currentState = newState;
    }

    // public void OnEnable()
    // {
    //     animator = GetComponent<Animator>();
    // }
    public void Initialize(string color, int bulletCount, int laneIndex, Color lineColor, float _speed,
    float jumpSpeed, List<Transform> paths, bool isHidden)
    {
        this.isHidden = isHidden;
        this.color = color;
        this.Bullet = bulletCount;
        this.laneIndex = laneIndex;

        this.speed = _speed;
        this.baseConveyorSpeed = _speed;
        this.jumpToQueueSpeed = jumpSpeed;
        this.initialRotation = model.rotation;
        this._bulletsFiredCount = 0;
        initCanvasLocalPos = canvasTransform.localPosition;

        ChangeState(PigState.InLane);
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
        var meshRenderer = faceModel.GetComponent<MeshRenderer>();
        var bodyMeshRenderer = bodyModel.GetComponent<MeshRenderer>();

        for (int i = 0; i < ammoCircles.Count; i++)
        {
            var meshRenderer2 = ammoCircles[i].GetComponent<MeshRenderer>();
            meshRenderer2.material.color = ColorGameConfig.instance.GetColorByName(color);

        }
        if (isHidden)
        {
            meshRenderer.material = hiddenMaterial;
            bodyMeshRenderer.material = hiddenMaterial;
            bulletText.text = "?";
            bulletText.fontSize = 50f;
            return;
        }
        bulletText.text = bulletCount.ToString();
        meshRenderer.material.color = ColorGameConfig.instance.GetColorByName(color);
        bodyMeshRenderer.material.color = ColorGameConfig.instance.GetColorByName(color);
    }

    public bool IsOnFirstRow()
    {
        return isOnTop;
    }

    public bool IsLinkedPig()
    {
        return (leftPig != null || rightPig != null);
    }
    public bool IsWholeLinkOnTop()
    {
        PigComponent leftmost = GetLeftmostPig();
        PigComponent current = leftmost;
        while (current != null)
        {
            if (!current.IsOnFirstRow())
            {
                return false;
            }
            current = current.rightPig;
        }

        return true;
    }
    public bool IsPigValid()
    {
        if (currentState != PigState.InLane && currentState != PigState.InQueue || !IsOnFirstRow())
        {
            return false;
        }


        return true;
    }

    public PigComponent GetLeftmostPig()
    {
        PigComponent current = this;

        while (current.leftPig != null)
        {
            current = current.leftPig;
        }
        return current;
    }

    public void StartJumpSequence()
    {
        StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        JumpTo();

        yield return new WaitForSeconds(0.25f);

        if (rightPig != null)
        {
            rightPig.StartJumpSequence();
        }
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

    public void SetIsOnTop(bool value)
    {
        isOnTop = value;
        if (value)
        {
            isHidden = false;   
            var meshRenderer = faceModel.GetComponent<MeshRenderer>();
            var bodyMeshRenderer = bodyModel.GetComponent<MeshRenderer>();
            meshRenderer.material = normalMaterial;
            bodyMeshRenderer.material = normalMaterial;

            meshRenderer.material.color = ColorGameConfig.instance.GetColorByName(color);
            bodyMeshRenderer.material.color = ColorGameConfig.instance.GetColorByName(color);

            bulletText.text = Bullet.ToString();
            bulletText.fontSize = 40f;

            if (IsLinkedPig())
            {
                EventManager.OnPigIsOnTopNoMoreHidden?.Invoke(this);
            }
        }
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

        // Xử lý khi hết đạn
        if (Bullet <= 0 && isOnBelt)
        {
            // Chạy hiệu ứng cho vòng cuối cùng
            if (_currentCircleIndex >= 0)
            {
                StartCoroutine(ScaleCircleRoutine(ammoCircles[_currentCircleIndex]));
            }

            EventManager.OnPigOutOfAmmo?.Invoke(this);
        }
    }

    private IEnumerator ScaleCircleRoutine(GameObject circle)
    {
        if (circle == null) yield break;
        Transform t = circle.transform;
        float durationScaleUp = 0.1f; // Tốc độ hiệu ứng
        float durationScaleDown = 0.25f; // Tốc độ hiệu ứng

        // 1. Phóng to lên 1.5x (Z và X)
        float elapsed = 0;
        Vector3 startScale = new Vector3(1f, 1f, 1f);
        Vector3 peakScale = new Vector3(1.7f, 1f, 1.7f);

        while (elapsed < durationScaleUp)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(startScale, peakScale, elapsed / durationScaleUp);
            yield return null;
        }

        // 2. Co lại về 1.2f
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
        // if (!isOnBelt) return;

        isOnBelt = false;
        if (_wavyLine != null)
        {
            _wavyLine.ClearAllTargets();
            _wavyLine.HideLineImmediately();
        }
        StartCoroutine(DestroyAnimation());
    }

    private IEnumerator DestroyAnimation()
    {
        ChangeState(PigState.Destroying);
        isOnBelt = false;
        StopAllCoroutines();
        StartCoroutine(DestroyAnimationInternal());
        SoundManager.Instance.StopSound(SoundManager.Instance.yarn);

        yield break;
    }

    private IEnumerator DestroyAnimationInternal()
    {
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = rb.rotation;
        Vector3 currentPos = rb.position;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rb.MovePosition(currentPos);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            rb.MoveRotation(startRotation * Quaternion.Euler(0f, t * 360f, 0f));

            yield return new WaitForFixedUpdate();
        }

        EventManager.OnPigDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    public void JumpTo(Action onComplete = null)
    {
        SetConveyorSpeedMultiplier(1f);

        //count Straight slots
        EventManager.OnJumpToConveyor?.Invoke();
        StartCoroutine(ReadyToJump(0.1f, onComplete));

    }

    private IEnumerator ConveyorJourney(Action onComplete)
    {
        ChangeState(PigState.Jumping);
        Vector3 firstPoint = allWaypoints[0].position;
        yield return StartCoroutine(JumpCoroutine(firstPoint, 0.4f, 1.5f));

        onComplete?.Invoke();
        ChangeState(PigState.OnConveyor);
        yield return new WaitForFixedUpdate();

        _rayCastDirection = allWaypoints[0].forward;
        if (_wavyLine != null)
        {
            _wavyLine.UpdateStartPoint(rayCastPoint.position);
        }
        CheckAndAddTargetBlocks();

        StartCoroutine(ShootingRoutine());
        StartCoroutine(MovePigThroughWaypoints(0, allWaypoints.Count - 1, allWaypoints));
    }

    private IEnumerator ReadyToJump(float duration, Action onComplete)
    {
        float elapsed = 0;
        ChangeState(PigState.ReadyToJump);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine(ConveyorJourney(onComplete));
        canvasTransform.localPosition = new Vector3(0.053f,1.488f,-0.215f);
    }

    private IEnumerator JumpCoroutine(Vector3 target, float duration, float height)
    {
        isOnTop = true;
        Vector3 startPos = rb.position;
        float elapsed = 0;
        isOnBelt = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(startPos, target, t);

            currentPos.y += Mathf.Sin(t * Mathf.PI) * height;

            rb.MovePosition(currentPos);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(target);
        // ChangeState(PigState.InLane);
        isOnTop = false;
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

    private IEnumerator ShootingRoutine()
    {
        while (isOnBelt && Bullet > 0)
        {
            if (_wavyLine != null)
            {
                _wavyLine.UpdateStartPoint(rayCastPoint.position);
            }

            CheckAndAddTargetBlocks();

            if (_lockedTargets > 0)
            {
                // Bật animation lắc (giả sử bạn đặt trigger hoặc bool trong Animator)
                ChangeState(PigState.Shooting);
            }
            else
            {
                ChangeState(PigState.OnConveyor);
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private void CheckAndAddTargetBlocks()
    {

        if (Bullet - _lockedTargets <= 0) return;

        if (rayCastPoint == null || _wavyLine == null) return;

        float checkDistance = 10f;
        Vector3 currentPos = rayCastPoint.position;

        if (Physics.Raycast(currentPos, _rayCastDirection, out RaycastHit hit, checkDistance, blockLayer))
        {
            var blockComp = hit.collider.gameObject.GetComponent<Block>();
            GameObject hitObject = hit.collider.gameObject;

            if (hit.collider.CompareTag("Block") && blockComp != null && blockComp.color == color && !blockComp.isAlreadyDestroyed)
            {
                _wavyLine.AddTarget(hitObject);
                blockComp.isAlreadyDestroyed = true;
                _lockedTargets++;
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


    public void JumpToQueue(Vector3 targetPosition, Quaternion targetRotation, int targetQueueIndex)
    {

        ChangeState(PigState.MovingToQueue);
        canvasTransform.localPosition = initCanvasLocalPos;
        if (_wavyLine != null)
        {
            _wavyLine.ClearAllTargets();
            _wavyLine.HideLineImmediately();
        }

        isOnBelt = false;
        StartCoroutine(JumpToQueueCoroutine(targetPosition, targetRotation));
    }

    private IEnumerator JumpToQueueCoroutine(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = rb.position;
        Quaternion startRot = rb.rotation;

        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / jumpToQueueSpeed;

        float height = 1.0f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * height;
            rb.MovePosition(currentPos);
            model.rotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(targetPos);
        model.rotation = targetRot;
        model.rotation = Quaternion.identity;
        model.localRotation = initialRotation;
        isOnTop = true;

        ChangeState(PigState.InQueue);
        StopAllCoroutines();
    }

    public void MoveInQueue(Vector3 targetPos, Quaternion targetRot, int newQueueIndex)
    {
        if (currentState == PigState.MovingToQueue)
        {
            StopAllCoroutines();
            isOnBelt = false;
            ChangeState(PigState.MovingInQueue);
            StartCoroutine(MoveInQueueCoroutine(targetPos, targetRot));
        }
        else if (currentState == PigState.InQueue)
        {
            ChangeState(PigState.MovingInQueue);
            StartCoroutine(MoveInQueueCoroutine(targetPos, targetRot));
        }
        else if (currentState == PigState.MovingInQueue)
        {
            StopAllCoroutines();
            ChangeState(PigState.MovingInQueue);
            StartCoroutine(MoveInQueueCoroutine(targetPos, targetRot));
        }
    }

    private IEnumerator MoveInQueueCoroutine(Vector3 targetPos, Quaternion targetRot)
    { // Slightly above to avoid ground collision
        Vector3 startPos = rb.position;
        Quaternion startRot = rb.rotation;

        float distance = Vector3.Distance(new Vector3(startPos.x, 0, startPos.z), new Vector3(targetPos.x, 0, targetPos.z));
        float moveSpeed = Mathf.Max(1f, 5);
        float duration = distance / moveSpeed;

        if (duration < 0.08f) duration = 0.08f;

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
            model.rotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return new WaitForFixedUpdate();
        }

        if (this != null)
        {
            rb.MovePosition(targetPos);
            model.rotation = targetRot;
            isOnTop = true;

            ChangeState(PigState.InQueue);
        }
    }

    public void SetConveyorSpeedMultiplier(float multiplier)
    {
        float clamped = Mathf.Max(0.1f, multiplier);
        speed = baseConveyorSpeed * clamped;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EndConveyor") && isOnBelt)
        {
            EventManager.OnPigEnterQueue?.Invoke(this);
        }
    }
}

