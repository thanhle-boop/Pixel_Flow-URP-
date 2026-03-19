using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PigComponent : MonoBehaviour
{
    public string color;
    public int Bullet;
    public int laneIndex;
    private float jumpToQueueSpeed;
    public bool isHidden = false;
    private bool isOnTop = false;
    public bool isOnBelt = false;
    private float speedOnStraight = 0f;
    private float speedOnCurve = 0f;
    public bool isLinkPig = false;
    public PigComponent leftPig = null;
    public PigComponent rightPig = null;
    public Transform rayCastPoint;
    private List<Transform> allWaypoints = new List<Transform>();
    public Rigidbody rb;
    private PigState currentState = PigState.InLane;
    private Vector3 _rayCastDirection = Vector3.forward;
    public LayerMask blockLayer;

    private WavyLineRenderer _wavyLine;
    private GameObject _lastCheckedBlock;

    public GameObject bulletText;
    public GameObject pigModel;
    public Material hiddenMaterial;
    public Material normalMaterial;
    private int _lockedTargets = 0;
    public void ChangeState(PigState newState)
    {
        currentState = newState;
    }

    public void Initialize(string color, int bulletCount, int laneIndex, Color lineColor, float speedOnStraight, float speedOnCurve,
    float jumpSpeed, List<Transform> paths, bool isHidden)
    {
        this.isHidden = isHidden;
        this.color = color;
        this.Bullet = bulletCount;
        this.laneIndex = laneIndex;

        this.speedOnCurve = speedOnCurve;
        this.speedOnStraight = speedOnStraight;
        this.jumpToQueueSpeed = jumpSpeed;

        allWaypoints = paths;

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
        var meshRenderer = pigModel.GetComponentInChildren<SkinnedMeshRenderer>();

        if (isHidden)
        {
            meshRenderer.material = hiddenMaterial;
            return;
        }

        if (meshRenderer != null)
        {
            meshRenderer.material.color = GameUtility.GetColorByName(color);
            bulletText.GetComponent<TextMeshProUGUI>().text = bulletCount.ToString();
            bulletText.SetActive(true);
        }
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

    public void SetIsOnTop(bool value)
    {
        isOnTop = value;
        if (value)
        {
            isHidden = false;
            var meshRenderer = pigModel.GetComponentInChildren<SkinnedMeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = normalMaterial;
                meshRenderer.material.color = GameUtility.GetColorByName(color);
                bulletText.GetComponent<TextMeshProUGUI>().text = Bullet.ToString();
                bulletText.SetActive(true);

                if(IsLinkedPig())
                {
                    EventManager.OnPigIsOnTopNoMoreHidden?.Invoke(this);
                }
            }
        }
    }

    private void OnBulletChanged()
    {
        if (_lockedTargets > 0)
        {
            _lockedTargets--;
        }

        if (Bullet <= 0 && isOnBelt)
        {
            EventManager.OnPigOutOfAmmo?.Invoke(this);
        }
    }

    public void ExecuteDestroy()
    {
        if (!isOnBelt) return;

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

    public void JumpTo()
    {
        if (currentState == PigState.InQueue)
        {
            ChangeState(PigState.JumpingFromQueue);
        }
        else
        {
            ChangeState(PigState.JumpingToConveyor);
        }

        //count Straight slots
        EventManager.OnJumpToConveyor?.Invoke();

        StartCoroutine(ConveyorJourney());
    }

    private IEnumerator ConveyorJourney()
    {
        Vector3 firstPoint = allWaypoints[0].position;
        yield return StartCoroutine(JumpCoroutine(firstPoint, 0.4f, 1.5f));

        ChangeState(PigState.OnConveyor);
        StartCoroutine(MovePigThroughWaypoints(0, allWaypoints.Count - 1, allWaypoints));
        StartCoroutine(ShootingRoutine());
    }

    private IEnumerator JumpCoroutine(Vector3 target, float duration, float height)
    {
        isOnTop = true;
        Vector3 startPos = rb.position;
        float elapsed = 0;

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
        isOnTop = false;
        isOnBelt = true;
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
                _lastCheckedBlock = hitObject;
                _wavyLine.AddTarget(hitObject);
                blockComp.isAlreadyDestroyed = true;

                _lockedTargets++;
            }
            else
            {
                _lastCheckedBlock = null;
            }
        }
        else
        {
            _lastCheckedBlock = null;
        }
    }

    public IEnumerator SlideTo(Vector3 target, float duration)
    {
        Vector3 start = rb.position;
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

            if (Mathf.Abs(Vector3.Dot(_rayCastDirection, current.forward)) < 0.01f)
            {
                _rayCastDirection = current.transform.forward;
            }

            if (current.gameObject.CompareTag("ControlPos") && i + 2 <= targetIndex)
            {
                Vector3 start = path[i].position;
                Vector3 control = path[i + 1].position;
                Vector3 end = path[i + 2].position;
                Quaternion startRot = path[i].rotation;
                Quaternion endRot = path[i + 2].rotation;

                yield return StartCoroutine(SlideOnCurve(start, control, end, startRot, endRot, speedOnCurve));
                i += 2;

            }
            else
            {
                rb.MoveRotation(path[i].rotation);
                Vector3 end = path[i + 1].position;
                yield return StartCoroutine(SlideTo(end, speedOnStraight));
                i++;
            }

            if (i >= path.Count - 1)
            {
                i = 0;
            }
        }
    }

    public IEnumerator SlideOnCurve(Vector3 start, Vector3 control, Vector3 end, Quaternion startRotation, Quaternion endRotation, float duration)
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 position = Mathf.Pow(1 - t, 2) * start +
                               2 * (1 - t) * t * control +
                               Mathf.Pow(t, 2) * end;

            rb.MovePosition(position);
            rb.MoveRotation(Quaternion.Slerp(startRotation, endRotation, t));

            yield return new WaitForFixedUpdate();
        }
        rb.MovePosition(end);
        rb.MoveRotation(endRotation);
    }


    public void JumpToQueue(Vector3 targetPosition, Quaternion targetRotation, int targetQueueIndex)
    {
        ChangeState(PigState.MovingToQueue);

        StopAllCoroutines();
        if (_wavyLine != null)
        {
            _wavyLine.ClearAllTargets();
            _wavyLine.HideLineImmediately();
        }

        _lastCheckedBlock = null;
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

            rb.MoveRotation(Quaternion.Lerp(startRot, targetRot, t));

            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(targetPos);
        rb.MoveRotation(targetRot);
        isOnTop = true;

        ChangeState(PigState.InQueue);
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
    {
        Vector3 startPos = rb.position;
        Quaternion startRot = rb.rotation;

        float distance = Vector3.Distance(new Vector3(startPos.x, 0, startPos.z), new Vector3(targetPos.x, 0, targetPos.z));
        float speed = 5f;
        float duration = distance / speed;

        if (duration < 0.2f) duration = 0.2f;

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
            rb.MoveRotation(Quaternion.Lerp(startRot, targetRot, t));

            yield return new WaitForFixedUpdate();
        }

        if (this != null)
        {
            rb.MovePosition(targetPos);
            rb.MoveRotation(targetRot);
            isOnTop = true;

            ChangeState(PigState.InQueue);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EndConveyor") && isOnBelt)
        {
            EventManager.OnPigEnterQueue?.Invoke(this);
        }
    }
}

