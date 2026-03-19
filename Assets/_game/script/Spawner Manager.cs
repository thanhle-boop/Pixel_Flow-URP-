using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{

    private struct PendingLink
    {
        public PigComponent sourcePig;
        public PigMarker targetLeftMarker;
        public PigMarker targetRightMarker;
    }

    public GameObject blockPrefab;
    public Transform blockSpawnPoint;
    public Transform blockGroup;

    public Transform pigSpawnPoint;

    public List<LevelDataSO> Levels;
    public float blockSpacing = 1.2f;

    public Transform pigSpawnPos;
    public GameObject pigPrefab;

    public List<Transform> allWaypoints;
    private Dictionary<int, List<PigComponent>> pigsByLane = new Dictionary<int, List<PigComponent>>();

    public List<Transform> queuePos;
    private List<PigComponent> pigsInQueue = new List<PigComponent>();
    private List<PigComponent> pigsInTempQueue = new List<PigComponent>();
    public Transform startTempQueuePos;
    private List<PigComponent> pigsInConveyor = new List<PigComponent>();
    public int _straightSlot = 0;
    public int _maxstraightSlot = 5;
    private int totalBlockCount = 0;

    private int tempScore = 0;

    private bool isProcessingClick = false;

    [SerializeField]
    private float speedOnStraight = 1f;

    [SerializeField]
    private float speedOnCurve = 0.5f;

    [SerializeField]
    private float jumpToQueueSpeed = 5f;

    public Link linkPrefabs;

    void OnEnable()
    {
        EventManager.OnStartGame += SpawnMap;
        EventManager.OnClickPig += SelectPig;
        EventManager.OnPigEnterQueue += HandlePigEnterQueue;
        EventManager.OnBlockDestroyed += OnBlockDestroyed;
        EventManager.OnPigDestroyed += RefundStraightSlot;
        EventManager.OnJumpToConveyor += IncreaseStraightSlot;
        EventManager.OnWinGame += WinGame;
        EventManager.OnLoseGame += LoseGame;
        EventManager.OnContinueGame += ContinueGame;
        EventManager.OnPigOutOfAmmo += HandlePigOutOfAmmo;
    }

    private void OnDisable()
    {
        EventManager.OnStartGame -= SpawnMap;
        EventManager.OnClickPig -= SelectPig;
        EventManager.OnPigEnterQueue -= HandlePigEnterQueue;
        EventManager.OnBlockDestroyed -= OnBlockDestroyed;
        EventManager.OnPigDestroyed -= RefundStraightSlot;

        EventManager.OnJumpToConveyor -= IncreaseStraightSlot;
        EventManager.OnWinGame -= WinGame;
        EventManager.OnLoseGame -= LoseGame;
        EventManager.OnContinueGame -= ContinueGame;
        EventManager.OnPigOutOfAmmo -= HandlePigOutOfAmmo;
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
                    p.ExecuteDestroy();
                }
            }
        }

        else
        {
            pig.ExecuteDestroy();
        }
    }
    private void IncreaseStraightSlot()
    {
        _straightSlot = _straightSlot + 1 > _maxstraightSlot ? _maxstraightSlot : _straightSlot + 1;
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
    }

    private void LoseGame()
    {
        tempScore = 0;
        StopAllPigAnimations();
    }
    private void WinGame()
    {
        DataManager.Instance.IncreaseLevel();
    }

    private void ContinueGame()
    {
        for (int i = 0; i < pigsInConveyor.Count; i++)
        {
            pigsInTempQueue.Add(pigsInConveyor[i]);
            _straightSlot--;
            pigsInConveyor[i].transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);

            int index = pigsInTempQueue.IndexOf(pigsInConveyor[i]);
            pigsInConveyor[i].JumpToQueue(startTempQueuePos.position + 0.9f * index * Vector3.right, startTempQueuePos.rotation, index);
        }
        pigsInConveyor.Clear();

        if (pigsInQueue.Count > 0)
        {
            PigComponent lastPig = pigsInQueue[pigsInQueue.Count - 1];
            List<PigComponent> pigsToMove = new List<PigComponent>();

            if (lastPig.IsLinkedPig())
            {
                PigComponent leftmost = lastPig.GetLeftmostPig();
                PigComponent current = leftmost;

                while (current != null)
                {
                    pigsToMove.Add(current);
                    current = current.rightPig;
                }
            }
            else
            {
                pigsToMove.Add(lastPig);
            }

            foreach (PigComponent pig in pigsToMove)
            {
                if (pigsInQueue.Contains(pig))
                {
                    pigsInQueue.Remove(pig);
                    pigsInTempQueue.Add(pig);

                    int tempIndex = pigsInTempQueue.Count - 1;
                    pig.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    pig.JumpToQueue(startTempQueuePos.position + 0.9f * tempIndex * Vector3.right, startTempQueuePos.rotation, tempIndex);
                }
            }
        }
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
        pigsByLane.Clear();
        pigsInQueue.Clear();
        pigsInTempQueue.Clear();

        _straightSlot = 0;
        totalBlockCount = 0;
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
    }

    public void SelectPig(PigComponent pig)
    {
        if (pig == null || isProcessingClick) return;

        if (!pig.IsPigValid())
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.invalidCat);
            return;
        }

        if (pig.IsLinkedPig())
        {
            if (!pig.IsWholeLinkOnTop())
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.invalidCat);
                return;
            }

            PigComponent leftmost = pig.GetLeftmostPig();
            List<PigComponent> linkedPigs = GetPigChain(leftmost);

            if (_straightSlot + linkedPigs.Count > _maxstraightSlot)
            {
                EventManager.OnFullConveyorSlot?.Invoke();
                SoundManager.Instance.PlaySound(SoundManager.Instance.error);
                return;
            }

            StartCoroutine(ProcessLinkedPigsRoutine(linkedPigs));
            return;
        }

        if (_straightSlot >= _maxstraightSlot)
        {
            EventManager.OnFullConveyorSlot?.Invoke();
            SoundManager.Instance.PlaySound(SoundManager.Instance.error);
            return;
        }

        isProcessingClick = true;
        ProcessPigData(pig);
        StartCoroutine(ResetClickFlag());
    }


    private IEnumerator ResetClickFlag()
    {
        yield return new WaitForSeconds(0.2f);
        isProcessingClick = false;
    }
    private List<PigComponent> GetPigChain(PigComponent startPig)
    {
        List<PigComponent> chain = new List<PigComponent>();
        PigComponent current = startPig;
        while (current != null)
        {
            chain.Add(current);
            current = current.rightPig;
        }
        return chain;
    }

    private IEnumerator ProcessLinkedPigsRoutine(List<PigComponent> linkedPigs)
    {
        isProcessingClick = true;

        foreach (PigComponent p in linkedPigs)
        {
            ProcessPigData(p);
            yield return new WaitForSeconds(0.25f);
        }

        yield return StartCoroutine(ResetClickFlag());
    }

    private void ProcessPigData(PigComponent pig)
    {
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
        SoundManager.Instance.PlaySound(SoundManager.Instance.validCat);

        if (pigsInQueue.Contains(pig) || pigsInTempQueue.Contains(pig))
        {
            bool isFromTempQueue = pigsInTempQueue.Contains(pig);
            HandlePigClickedFromQueue(pig, isFromTempQueue);
            return;
        }

        int laneIndex = pig.laneIndex;
        if (pigsByLane.ContainsKey(laneIndex))
        {
            List<PigComponent> pigsInLane = pigsByLane[laneIndex];
            if (pigsInLane.Count > 0)
            {
                RemovePigFromLane(pig);
                pig.JumpTo();
            }
        }
    }

    private void RemovePigFromLane(PigComponent removedPig)
    {
        int laneIndex = removedPig.laneIndex;

        if (!pigsByLane.ContainsKey(laneIndex)) return;
        List<PigComponent> pigsInLane = pigsByLane[laneIndex];

        pigsInLane.Remove(removedPig);
        pigsInConveyor.Add(removedPig);

        pigsInLane.Sort((a, b) => b.transform.localPosition.z.CompareTo(a.transform.localPosition.z));

        for (int i = 0; i < pigsInLane.Count; i++)
        {
            Vector3 newLocalPos = pigsInLane[i].transform.localPosition;

            newLocalPos.z = -(i * blockSpacing);

            pigsInLane[i].MoveTo(newLocalPos);

            pigsInLane[i].SetIsOnTop(i == 0);
        }
    }

    private void SpawnMap()
    {
        if (DataManager.Instance.CurrentLevel - 1 >= Levels.Count) return;

        ResetData();
        tempScore = DataManager.Instance.Score;
        UIManager.Instance.UpdateScore(tempScore);

        foreach (Transform child in blockGroup)
        {
            Destroy(child.gameObject);
        }

        totalBlockCount = 0;

        LevelDataSO data = Levels[DataManager.Instance.CurrentLevel - 1];

        int W = data.width;
        int H = data.height;
        float offsetX = (W - 1) * blockSpacing / 2f;
        float offsetY = (H - 1) * blockSpacing / 2f;

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                int index = y * W + x;
                string colorType = data.gridData[index];

                if (colorType == "empty") continue;
                Vector3 localPos = new Vector3(
                    (x * blockSpacing) - offsetX,
                    0,
                    ((H - 1 - y) * blockSpacing) - offsetY
                );
                Vector3 worldPos = blockSpawnPoint.TransformPoint(localPos);
                GameObject newBlock = Instantiate(blockPrefab, worldPos, Quaternion.identity, blockGroup);
                ApplyMaterial(newBlock, colorType);

                totalBlockCount++;
            }
        }

        SpawnPigs(Levels[DataManager.Instance.CurrentLevel - 1]);
    }

    private void SpawnPigs(LevelDataSO data)
    {
        foreach (Transform child in pigSpawnPos)
        {
            Destroy(child.gameObject);
        }

        pigsByLane.Clear();

        if (data.lanes == null || data.lanes.Count == 0) return;

        int laneCount = data.lanes.Count;
        float laneOffsetX = (laneCount - 1) * 0.85f / 2f;
        List<PendingLink> pigNeedLink = new List<PendingLink>();

        for (int i = 0; i < laneCount; i++)
        {
            var currentLane = data.lanes[i];

            if (!pigsByLane.ContainsKey(i))
            {
                pigsByLane[i] = new List<PigComponent>();
            }
            if (currentLane.pigs == null) continue;

            for (int j = 0; j < currentLane.pigs.Count; j++)
            {
                string colorType = currentLane.pigs[j].colorName;
                var bulletCount = currentLane.pigs[j].bullets;
                var color = GameUtility.GetColorByName(colorType);
                if (colorType == "empty") continue;
                Vector3 localPos = new Vector3(
                    (i * 0.85f) - laneOffsetX,
                    0,
                    -(j * 0.85f)
                );

                Vector3 worldPos = pigSpawnPos.TransformPoint(localPos);

                GameObject newPig = Instantiate(pigPrefab, worldPos, Quaternion.identity, pigSpawnPos);

                PigComponent pigComp = newPig.GetComponent<PigComponent>();

                if (pigComp != null)
                {
                    pigComp.Initialize(colorType, bulletCount, i, color, speedOnStraight, speedOnCurve, jumpToQueueSpeed, allWaypoints, Random.Range(0, 2) == 0 ? true : false);
                    pigsByLane[i].Add(pigComp);
                    pigComp.SetIsOnTop(j == 0);
                    if (currentLane.pigs[j].pigRight.IsValid())
                    {
                        pigNeedLink.Add(new PendingLink
                        {
                            sourcePig = pigComp,
                            targetLeftMarker = currentLane.pigs[j].pigLeft,
                            targetRightMarker = currentLane.pigs[j].pigRight
                        });
                    }
                }

                var renderer = newPig.GetComponentInChildren<Renderer>();

                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }

        }

        foreach (var pig in pigNeedLink)
        {

            Link linkObject = Instantiate(linkPrefabs);
            PigComponent pig1 = pig.sourcePig;
            PigComponent pig2 = pigsByLane[pig.targetRightMarker.LaneIndex][pig.targetRightMarker.index];
            pig1.SetLinkPig(pig.targetLeftMarker.LaneIndex == -1 ? null : pigsByLane[pig.targetLeftMarker.LaneIndex][pig.targetLeftMarker.index], pig2);

            pig2.SetLinkPig(pig1, null);

            string color1 = pig1.color;
            string color2 = pig2.color;

            linkObject.SetColor(color1, color2, pig1, pig2);
        }
    }

    private void ApplyMaterial(GameObject obj, string colorName)
    {
        var renderer = obj.GetComponent<Renderer>();
        var blockComponent = obj.GetComponent<Block>();
        if (renderer == null || blockComponent == null) return;

        blockComponent.color = colorName;
        renderer.material.color = GameUtility.GetColorByName(colorName);

    }

    private void HandlePigEnterQueue(PigComponent pig)
    {
        if (pig == null) return;

        if (queuePos == null || queuePos.Count == 0)
        {
            return;
        }

        int queueIndex = FindNextAvailableQueueIndex();

        if (queueIndex >= 0 && queueIndex < queuePos.Count)
        {
            Vector3 targetPos = queuePos[queueIndex].position;
            Quaternion targetRot = queuePos[queueIndex].rotation;

            pigsInConveyor.Remove(pig);
            pigsInQueue.Add(pig);
            pig.JumpToQueue(targetPos, targetRot, queueIndex);

            if (pigsInQueue.Count >= queuePos.Count)
            {
                EventManager.OnQueueFull?.Invoke();
            }
            _straightSlot = _straightSlot - 1 < 0 ? 0 : _straightSlot - 1;
            UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
        }
        else
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.GameOver();
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

    private void OnBlockDestroyed()
    {
        totalBlockCount--;
        tempScore += 100;
        UIManager.Instance.UpdateScore(tempScore);
        if (totalBlockCount <= 0)
        {
            if (GameManager.Instance != null)
            {
                DataManager.Instance.AddScore(tempScore);
                GameManager.Instance.WinStage();
            }
        }
    }

    private void HandlePigClickedFromQueue(PigComponent pig, bool isFromTempQueue = false)
    {
        int removedIndex = isFromTempQueue ? pigsInTempQueue.IndexOf(pig) : pigsInQueue.IndexOf(pig);

        if (removedIndex < 0) return;

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

        RearrangeQueue(removedIndex, isFromTempQueue);
        pig.JumpTo();
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
                    pig.MoveInQueue(targetPos, targetRot, i);
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
                    pig.MoveInQueue(targetPos, targetRot, i);
                }
            }
        }

    }
    private void StopAllPigAnimations()
    {
        foreach (Transform kvp in pigSpawnPos)
        {
            PigComponent pigComp = kvp.GetComponent<PigComponent>();
            if (pigComp != null)
            {
                pigComp.StopAllCoroutines();
            }
        }
    }
}
