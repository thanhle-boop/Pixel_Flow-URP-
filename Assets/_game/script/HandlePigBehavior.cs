using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandlePigBehavior : MonoBehaviour
{
    private struct PendingLink
    {
        public PigComponent sourcePig;
        public PigComponent targetPig;
    }
    private int _straightSlot = 0;
    private int _maxstraightSlot = 5;
    public bool isTesting = false;

    public bool isProcessingClick = false;
    private bool onHandItemUsed = false;
    [SerializeField] private float jumpFromLaneSpeed = 6f;
    private float jumpFromQueueSpeed = 3.5f;

    public List<Transform> queuePos;
    private List<PigComponent> pigsInQueue = new List<PigComponent>();
    private List<PigComponent> pigsInTempQueue = new List<PigComponent>();
    private List<PigComponent> pigsInConveyor = new List<PigComponent>();
    public Transform startTempQueuePos;

    [Header("Plate Settings")]
    public GameObject platePrefab;
    public Transform traySlotOrigin;
    public float plateStackOffset = 0.1f;
    private Queue<Transform> availablePlates = new Queue<Transform>();
    public Transform tray;
    public GameObject clickVFXPrefab;
    private List<PigComponent> pigStack = new List<PigComponent>();

    [Header("Stack Settings")]
    public float pigHeightOffset = 1.2f;

    public SpawnBlock spawnManager;

    void OnEnable()
    {
        EventManager.OnClickPig += SelectPig;
        EventManager.OnPigEnterQueue += HandlePigEnterQueue;
        EventManager.OnPigDestroyed += RefundStraightSlot;
        EventManager.OnJumpToConveyor += IncreaseStraightSlot;
        EventManager.OnWinGame += WinGame;
        EventManager.OnLoseGame += LoseGame;
        EventManager.OnContinueGame += ContinueGame;
        EventManager.OnPigOutOfAmmo += HandlePigOutOfAmmo;

        EventManager.OnUseAddTray += UseItemAddTray;

        EventManager.OnUseHand += UseItemHand;

        EventManager.OnUseShuffle += UseItemShufflePig;

        EventManager.OnClickBlock += ClickBlock;
    }

    private void OnDisable()
    {
        EventManager.OnClickPig -= SelectPig;
        EventManager.OnPigEnterQueue -= HandlePigEnterQueue;
        EventManager.OnPigDestroyed -= RefundStraightSlot;
        EventManager.OnJumpToConveyor -= IncreaseStraightSlot;
        EventManager.OnWinGame -= WinGame;
        EventManager.OnLoseGame -= LoseGame;
        EventManager.OnContinueGame -= ContinueGame;
        EventManager.OnPigOutOfAmmo -= HandlePigOutOfAmmo;

        EventManager.OnUseAddTray -= UseItemAddTray;

        EventManager.OnUseHand -= UseItemHand;

        EventManager.OnUseShuffle -= UseItemShufflePig;

        EventManager.OnClickBlock -= ClickBlock;
    }
    public void UseItemHand()
    {
        onHandItemUsed = true;
    }

    public void UseItemAddTray()
    {
        _maxstraightSlot++;
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
    }
    private void InitializePlates()
    {
        availablePlates.Clear();
        for (int i = 0; i < 5; i++)
        {
            Vector3 spawnPos = traySlotOrigin.position - Vector3.right * (i * plateStackOffset);

            GameObject go = Instantiate(platePrefab);
            Transform plate = go.transform;
            plate.position = spawnPos;
            plate.localRotation = Quaternion.Euler(0, 0, -45);
            plate.SetParent(tray);
            plate.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            availablePlates.Enqueue(plate);
        }
    }

    private void AssignPlateToPig(PigComponent pig)
    {
        if (pig.currentPlate != null)
        {
            StartCoroutine(ReturnPlateToOrigin(pig.currentPlate));
            pig.currentPlate = null;
        }
        if (availablePlates.Count > 0)
        {
            Transform plate = availablePlates.Dequeue();
            pig.currentPlate = plate;
            StartCoroutine(AnimatePlateToPig(plate, pig));
        }
    }

    private IEnumerator AnimatePlateToPig(Transform plate, PigComponent pig)
    {

        Quaternion startRot = plate.rotation;

        Vector3 intermediatePos = traySlotOrigin.position + Vector3.right * 0.5f - Vector3.up * 0.1f;

        float elapsed = 0;
        float duration = 0.35f;
        RearrangePlatesInTray();

        while (elapsed < duration)
        {
            if (plate.parent != tray) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            plate.position = Vector3.Lerp(plate.position, intermediatePos, t);
            plate.rotation = Quaternion.Lerp(startRot, Quaternion.Euler(0, 0, 90), t);
            yield return null;
        }
    }
    public void ClickBlock(string color)
    {
        foreach (PigComponent pig in spawnManager.pigGroup.GetComponentsInChildren<PigComponent>())
        {
            if (pig.color == color)
            {

                HandlePigClickedFromQueue(pig, () =>
                {
                    pig.ExecuteDestroy();
                    RemovePigFromLane(pig);
                }, pigsInTempQueue.Contains(pig));
            }
        }
    }

    private void HandlePigOutOfAmmo(PigComponent pig)
    {
        if (pig.IsLinkedPig())
        {
            PigComponent leftmost = pig.GetLeftmostPig();
            List<PigComponent> linkedGroup = GetPigChain(leftmost);

            bool isAllGroupEmpty = true;
            foreach (PigComponent p in linkedGroup)
            {
                if (p.Bullet > 0)
                {
                    isAllGroupEmpty = false;
                    break;
                }
            }

            if (isAllGroupEmpty)
            {
                foreach (PigComponent p in linkedGroup)
                {

                    if (p.currentPlate != null)
                    {
                        StartCoroutine(ReturnPlateToOrigin(p.currentPlate));
                        p.currentPlate = null;
                    }
                    p.ExecuteDestroy();
                    pigsInQueue.Remove(p);
                    pigsInTempQueue.Remove(p);
                    EventManager.OnClearLinked?.Invoke(p);
                }
            }
        }

        else
        {
            if (pig.currentPlate != null)
            {
                StartCoroutine(ReturnPlateToOrigin(pig.currentPlate));
                pig.currentPlate = null;
            }
            pig.ExecuteDestroy();

        }
        CheckAndEnableFinalRush();
    }
    private void CheckAndEnableFinalRush()
    {
        PigComponent[] allPigs = spawnManager.pigGroup.GetComponentsInChildren<PigComponent>();
        if (allPigs.Length <= 6 && allPigs.Length > 0)
        {
            foreach (var p in allPigs)
            {
                p.SetConveyorSpeedMultiplier(2f);
                p.isRush = true;
            }
        }
    }
    private void IncreaseStraightSlot()
    {
        _straightSlot = _straightSlot + 1 > _maxstraightSlot ? _maxstraightSlot : _straightSlot + 1;
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
    }

    private void LoseGame()
    {
        foreach (PigComponent pig in pigsInConveyor)
        {
            if (pig != null) pig.StopShooting();
        }
    }
    private void WinGame()
    {
        if (!isTesting)
            LevelController.ClearLevel(LevelController.GetMaxLevelUnlock() + 1);
    }

    private void ContinueGame()
    {
        StopAllCoroutines();
        isProcessingClick = false;

        List<PigComponent> beltPigs = new List<PigComponent>(pigsInConveyor);
        beltPigs.RemoveAll(p => pigsInQueue.Contains(p) || pigsInTempQueue.Contains(p));
        pigsInConveyor.Clear();

        List<PigComponent> queuePigsToFree = new List<PigComponent>();
        if (pigsInQueue.Count >= queuePos.Count)
        {
            int lastSlotIndex = queuePos.Count - 1;
            if (lastSlotIndex < pigsInQueue.Count)
            {
                PigComponent lastPig = pigsInQueue[lastSlotIndex];

                if (lastPig.IsLinkedPig())
                {
                    PigComponent leftmost = lastPig.GetLeftmostPig();
                    PigComponent current = leftmost;
                    while (current != null)
                    {
                        if (pigsInQueue.Contains(current) && !queuePigsToFree.Contains(current))
                            queuePigsToFree.Add(current);
                        current = current.rightPig;
                    }
                }
                else
                {
                    queuePigsToFree.Add(lastPig);
                }
            }
        }

        foreach (PigComponent p in queuePigsToFree)
            pigsInQueue.Remove(p);

        foreach (PigComponent pig in beltPigs)
        {
            if (pig != null) MovePigToTempQueueInternal(pig);
        }

        foreach (PigComponent p in queuePigsToFree)
        {
            if (p != null) MovePigToTempQueueInternal(p);
        }

        _straightSlot = 0;
        UIManager.Instance.UpdateStraightSlot(0, _maxstraightSlot);

        RearrangeQueue(0, false);
        RearrangeQueue(0, true);

        EventManager.OnQueueNotFull?.Invoke();
    }
    private void MovePigToTempQueueInternal(PigComponent pig)
    {
        if (pigsInTempQueue.Contains(pig)) return;
        pigsInTempQueue.Add(pig);
        if (pig.currentPlate != null)
        {
            StartCoroutine(ReturnPlateToOrigin(pig.currentPlate));
            pig.currentPlate = null;
        }
        pig.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        int index = pigsInTempQueue.Count - 1;

        Vector3 targetPos = startTempQueuePos.position + (Vector3.right * (index * 0.9f));

        pig.JumpToQueue(targetPos, 5f);
    }

    private void RefundStraightSlot(PigComponent pig)
    {
        _straightSlot = _straightSlot - 1 < 0 ? 0 : _straightSlot - 1;
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
        pigsInConveyor.Remove(pig);
    }

    private void ResetData()
    {
        pigsInConveyor.Clear();
        pigsInQueue.Clear();
        pigsInTempQueue.Clear();

        _straightSlot = 0;
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
    }

    // public void SelectPig(PigComponent pig)
    // {

    //     if (pig == null || isProcessingClick)
    //     {
    //         return;
    //     }

    //     ParticleSystem clickVFX = Instantiate(clickVFXPrefab).GetComponent<ParticleSystem>();
    //     clickVFX.transform.position = pig.transform.position + Vector3.up - Vector3.forward * 0.2f;
    //     clickVFX.Play();


    //     if (!pig.IsPigValid() && !onHandItemUsed)
    //     {
    //         AudioController.instance.PlaySound(AudioIndex.invalid_cat.ToString());
    //         return;
    //     }

    //     if (onHandItemUsed)
    //     {
    //         onHandItemUsed = false;
    //         EventManager.OnEndHand?.Invoke();
    //         pig.SetIsOnTop(true);
    //     }

    //     if (pig.IsLinkedPig())
    //     {
    //         if (!pig.IsWholeLinkOnTop())
    //         {
    //             AudioController.instance.PlaySound(AudioIndex.invalid_cat.ToString());
    //             return;
    //         }

    //         PigComponent leftmost = pig.GetLeftmostPig();
    //         List<PigComponent> linkedPigs = GetPigChain(leftmost);

    //         if (_straightSlot + linkedPigs.Count > _maxstraightSlot)
    //         {
    //             EventManager.OnFullConveyorSlot?.Invoke();
    //             AudioController.instance.PlaySound(AudioIndex.error.ToString());
    //             return;
    //         }

    //         StartCoroutine(ProcessLinkedPigsRoutine(linkedPigs));
    //         return;
    //     }

    //     if (_straightSlot >= _maxstraightSlot)
    //     {
    //         EventManager.OnFullConveyorSlot?.Invoke();
    //         AudioController.instance.PlaySound(AudioIndex.error.ToString());
    //         return;

    //     }

    //     isProcessingClick = true;
    //     ProcessPigData(pig);
    //     // StartCoroutine(ResetClickFlag(0.15f));

    // }

    public void SelectPig(PigComponent pig)
    {
        if (pig == null) return;

        ParticleSystem clickVFX = Instantiate(clickVFXPrefab).GetComponent<ParticleSystem>();
        clickVFX.transform.position = pig.transform.position + Vector3.up - Vector3.forward * 0.2f;
        clickVFX.Play();

        if (!pig.IsPigValid() && !onHandItemUsed)
        {
            AudioController.instance.PlaySound(AudioIndex.invalid_cat.ToString());
            return;
        }

        if (onHandItemUsed)
        {
            onHandItemUsed = false;
            EventManager.OnEndHand?.Invoke();
        }

        if (pigStack.Contains(pig)) return;

        if (_straightSlot >= _maxstraightSlot)
        {
            EventManager.OnFullConveyorSlot?.Invoke();
            AudioController.instance.PlaySound(AudioIndex.error.ToString());
            return;
        }

        if (pig.IsLinkedPig())
        {
            if(!pig.IsWholeLinkOnTop()) return;
            PigComponent leftmost = pig.GetLeftmostPig();
            List<PigComponent> linkedPigs = GetPigChain(leftmost);
            foreach (PigComponent p in linkedPigs)
            {
                if (!pigStack.Contains(p))
                {
                    ProcessPigData(p, pigStack.Count);
                    pigStack.Add(p);
                }
            }
        }
        else
        {
            ProcessPigData(pig, pigStack.Count);
            pigStack.Add(pig);
        }
    }

    private List<PigComponent> GetPigChain(PigComponent startPig)
    {
        List<PigComponent> chain = new List<PigComponent>();
        HashSet<PigComponent> visited = new HashSet<PigComponent>();
        PigComponent current = startPig;
        while (current != null && visited.Add(current))
        {
            chain.Add(current);
            current = current.rightPig;
        }
        return chain;
    }

    private void ProcessLinkedPigs(List<PigComponent> linkedPigs)
    {
        isProcessingClick = true;

        bool isFromTemp = pigsInTempQueue.Contains(linkedPigs[0]);
        bool isFromQueue = pigsInQueue.Contains(linkedPigs[0]);

        bool isFromLane = !isFromTemp && !isFromQueue;

        int firstQueueIndex = -1;
        if (isFromTemp) firstQueueIndex = pigsInTempQueue.IndexOf(linkedPigs[0]);
        else if (isFromQueue) firstQueueIndex = pigsInQueue.IndexOf(linkedPigs[0]);

        foreach (PigComponent p in linkedPigs)
        {

            if (isFromTemp) p.transform.localScale = Vector3.one;

            if (!isFromLane && !pigsInConveyor.Contains(p)) pigsInConveyor.Add(p);


            p.JumpTo(jumpFromQueueSpeed, pigStack.Count, () =>
            {
                pigStack.Add(p);
            });

        }

        if (!isFromLane)
        {
            List<PigComponent> targetQueue = isFromTemp ? pigsInTempQueue : pigsInQueue;
            foreach (PigComponent p in linkedPigs)
            {
                if (targetQueue.Contains(p)) targetQueue.Remove(p);
            }

            if (firstQueueIndex != -1)
                RearrangeQueue(firstQueueIndex, isFromTemp);
        }
    }

    private void ProcessPigData(PigComponent pig, int count, Action onComplete = null)
    {
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);

        AudioController.instance.PlaySound(AudioIndex.valid_cat.ToString());

        if (pigsInQueue.Contains(pig) || pigsInTempQueue.Contains(pig))
        {
            bool isFromTempQueue = pigsInTempQueue.Contains(pig);
            HandlePigClickedFromQueue(pig, onComplete, isFromTempQueue);
            return;
        }

        RemovePigFromLane(pig);
        pig.JumpTo(jumpFromLaneSpeed, count, onComplete);
    }

    private void RemovePigFromLane(PigComponent removedPig)
    {
        int laneIndex = removedPig.laneIndex;

        if (!spawnManager.pigsByLane.ContainsKey(laneIndex)) return;
        List<PigComponent> pigsInLane = spawnManager.pigsByLane[laneIndex];

        pigsInLane.Remove(removedPig);
        pigsInConveyor.Add(removedPig);

        pigsInLane.Sort((a, b) => b.transform.localPosition.z.CompareTo(a.transform.localPosition.z));

        for (int i = 0; i < pigsInLane.Count; i++)
        {
            Vector3 newLocalPos = pigsInLane[i].transform.localPosition;
            newLocalPos.z = -(i * spawnManager.pigSpacing);

            bool wasHidden = pigsInLane[i].isHidden;
            pigsInLane[i].SetIsOnTop(i == 0);
            if (wasHidden && i == 0)
            {
                pigsInLane[i].JumpToTarget(newLocalPos);
            }
            else
            {
                pigsInLane[i].MoveTo(newLocalPos);
            }
        }
    }

    private void HandlePigEnterQueue(PigComponent pig)
    {
        if (pig == null) return;

        if (pig.currentPlate != null)
        {
            StartCoroutine(ReturnPlateToOrigin(pig.currentPlate));
            pig.currentPlate = null;
        }

        if (queuePos == null || queuePos.Count == 0)
        {
            return;
        }

        int queueIndex;

        if (pig.IsLinkedPig())
        {
            PigComponent leftmost = pig.GetLeftmostPig();
            List<PigComponent> chain = GetPigChain(leftmost);

            int alreadyInQueue = chain.Count(p => pigsInQueue.Contains(p));
            int needToAdd = chain.Count - alreadyInQueue;
            int freeSlots = queuePos.Count - pigsInQueue.Count;
            if (needToAdd > freeSlots)
            {
                GameManager.Instance?.GameOver();
                foreach (PigComponent member in chain)
                {
                    member.GameOver();
                }
                return;
            }

            int insertAfterIndex = -1;
            foreach (PigComponent member in chain)
            {
                if (member == pig) continue;
                int idx = pigsInQueue.IndexOf(member);
                if (idx >= 0 && idx > insertAfterIndex)
                    insertAfterIndex = idx;
            }

            if (insertAfterIndex >= 0)
            {
                queueIndex = insertAfterIndex + 1;
                if (pigsInQueue.Count >= queuePos.Count)
                {
                    GameManager.Instance?.GameOver();
                    return;
                }
                pigsInQueue.Insert(queueIndex, pig);
                for (int i = queueIndex; i < pigsInQueue.Count; i++)
                {
                    pigsInQueue[i].MoveInQueue(queuePos[i].position, queuePos[i].rotation);
                }
            }
            else
            {
                queueIndex = FindNextAvailableQueueIndex();
                if (queueIndex < 0)
                {
                    GameManager.Instance?.GameOver();
                    return;
                }
                pigsInQueue.Add(pig);
                pig.MoveInQueue(queuePos[queueIndex].position, queuePos[queueIndex].rotation);
            }
        }
        else
        {
            queueIndex = FindNextAvailableQueueIndex();
            if (queueIndex >= 0 && queueIndex < queuePos.Count)
            {
                pigsInQueue.Add(pig);
            }
            else
            {
                GameManager.Instance?.GameOver();
                return;
            }
            RearrangeQueue(queueIndex, false);
        }

        pigsInConveyor.Remove(pig);
        pig.JumpToQueue(queuePos[queueIndex].position, 5f);

        if (pigsInQueue.Count >= queuePos.Count)
        {
            EventManager.OnQueueFull?.Invoke();
        }
        _straightSlot = _straightSlot - 1 < 0 ? 0 : _straightSlot - 1;
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
    }

    private IEnumerator ReturnPlateToOrigin(Transform plate)
    {
        Vector3 startPos = plate.position;

        plate.transform.SetParent(null);
        availablePlates.Enqueue(plate);

        Vector3 targetPos = traySlotOrigin.position + Vector3.up * (availablePlates.Count * plateStackOffset);

        float elapsed = 0;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            plate.position = Vector3.Lerp(startPos, targetPos, t);
            plate.rotation = Quaternion.Lerp(plate.rotation, traySlotOrigin.rotation, t);
            yield return null;
        }

        plate.transform.SetParent(tray);
        plate.position = targetPos;
        plate.rotation = Quaternion.Euler(0, 0, -45);
        plate.localScale = Vector3.one;

        RearrangePlatesInTray();
    }

    private void RearrangePlatesInTray()
    {
        int index = 0;
        foreach (Transform p in availablePlates)
        {
            Vector3 localOffset = Vector3.right * (index * plateStackOffset);
            p.position = traySlotOrigin.position - Vector3.right * (index * plateStackOffset);
            index++;
        }
    }

    private int FindNextAvailableQueueIndex()
    {
        int occupiedCount = pigsInQueue.Count;

        if (occupiedCount < queuePos.Count)
        {
            return occupiedCount;
        }

        return -1;
    }

    private void HandlePigClickedFromQueue(PigComponent pig, Action complete, bool isFromTempQueue = false)
    {
        if (pig.IsLinkedPig())
        {
            PigComponent leftmost = pig.GetLeftmostPig();
            List<PigComponent> linkedPigs = GetPigChain(leftmost);

            ProcessLinkedPigs(linkedPigs);
            complete?.Invoke();
            return;
        }
        int removedIndex = isFromTempQueue ? pigsInTempQueue.IndexOf(pig) : pigsInQueue.IndexOf(pig);

        if (removedIndex < 0)
        {
            return;
        }

        if (isFromTempQueue)
        {
            pigsInTempQueue.Remove(pig);
            pig.transform.localScale = Vector3.one;
        }
        else
        {
            pigsInQueue.Remove(pig);
        }

        pigsInConveyor.Add(pig);

        if (pigsInQueue.Count < queuePos.Count)
        {
            EventManager.OnQueueNotFull?.Invoke();
        }

        // AssignPlateToPig(pig);
        pig.JumpTo(jumpFromQueueSpeed, pigStack.Count, () =>
        {
            RearrangeQueue(removedIndex, isFromTempQueue);
            complete?.Invoke();
        });
    }

    private void RearrangeQueue(int startIndex, bool isFromTempQueue = false)
    {

        if (!isFromTempQueue)
        {
            for (int i = startIndex; i < pigsInQueue.Count; i++)
            {
                if (i < pigsInQueue.Count)
                {
                    Vector3 targetPos = queuePos[i].position;
                    Quaternion targetRot = queuePos[i].rotation;
                    PigComponent pig = pigsInQueue[i];
                    pig.MoveInQueue(targetPos, targetRot);
                }
            }
        }
        else
        {
            for (int i = startIndex; i < pigsInTempQueue.Count; i++)
            {
                if (i < pigsInTempQueue.Count)
                {
                    Vector3 targetPos = startTempQueuePos.position + 0.9f * i * Vector3.right;
                    Quaternion targetRot = startTempQueuePos.rotation;
                    PigComponent pig = pigsInTempQueue[i];
                    pig.MoveInQueue(targetPos, targetRot);
                }
            }
        }

    }

    public void UseItemShufflePig()
    {
        spawnManager.pigsByLane = Helper.ShuffleHeoDictionary(spawnManager.pigsByLane);

        int laneCount = spawnManager.pigsByLane.Count;
        float laneOffsetX = (laneCount - 1) * 0.85f / 2f;

        foreach (var laneEntry in spawnManager.pigsByLane)
        {
            int laneIndex = laneEntry.Key;

            for (int pigIndex = 0; pigIndex < laneEntry.Value.Count; pigIndex++)
            {
                PigComponent pig = laneEntry.Value[pigIndex];
                pig.laneIndex = laneIndex;

                Vector3 newLocalPos = new Vector3(
                    (laneIndex * 0.95f) - laneOffsetX,
                    0,
                    -(pigIndex * spawnManager.pigSpacing)
                );

                pig.MoveTo(newLocalPos);
                pig.SetIsOnTop(pigIndex == 0);
            }
        }
    }

    private float timer = 0f;

    void FixedUpdate()
    {
        if (pigStack.Count == 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f)
        {
            PigComponent pigBottom = pigStack[0];
            AssignPlateToPig(pigBottom);
            if (pigBottom.currentState != PigState.CanMove) return;
            timer = 0f;
            pigBottom.StartMove();
            pigStack.RemoveAt(0);
            for (int i = 0; i < pigStack.Count; i++)
            {
                Vector3 newPos = spawnManager.allWaypoints[0].position;
                newPos.y += i * pigHeightOffset;
                pigStack[i].MoveInQueue(newPos, spawnManager.allWaypoints[0].rotation, true);
            }
        }
    }
}
