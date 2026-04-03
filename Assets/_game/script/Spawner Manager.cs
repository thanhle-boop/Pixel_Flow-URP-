using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnerManager : MonoBehaviour
{
    private struct PendingLink
    {
        public PigComponent sourcePig;
        public PigComponent targetPig;
    }
    private int _straightSlot = 0;
    private int _maxstraightSlot = 5;
    private int totalBlockCount = 0;

    public bool isTesting = false;

    private bool isProcessingClick = false;
    private bool onHandItemUsed = false;
    private bool _isFinalRush = false;
    public GameObject supertCatPrefab;
    public GameObject blockPrefab;
    public Transform blockSpawnPoint;
    public Transform blockGroup;

    public Transform pigSpawnPoint;

    private float blockSpacing = 1.2f;

    public Transform pigSpawnPos;
    public GameObject pigPrefab;

    public List<Transform> allWaypoints;
    private Dictionary<int, List<PigComponent>> pigsByLane = new Dictionary<int, List<PigComponent>>();
    private List<Link> activeLinks = new List<Link>();

    public List<Transform> queuePos;
    private List<PigComponent> pigsInQueue = new List<PigComponent>();
    private List<PigComponent> pigsInTempQueue = new List<PigComponent>();
    public Transform startTempQueuePos;
    private List<PigComponent> pigsInConveyor = new List<PigComponent>();

    [SerializeField]
    private float speed = 1f;

    [SerializeField]
    private float jumpToQueueSpeed = 5f;

    public Link linkPrefabs;

    public List<GameObject> blockPrefabsByColor;

    public float spacing = 1.2f;

    [Header("Plate Settings")]
    public GameObject platePrefab;
    public Transform traySlotOrigin;
    public float plateStackOffset = 0.1f;

    private List<Transform> allPlates = new List<Transform>();
    private Queue<Transform> availablePlates = new Queue<Transform>();
    private Dictionary<PigComponent, Transform> activePlateMap = new Dictionary<PigComponent, Transform>();

    public Transform tray;
    public GameObject clickVFXPrefab;

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

        EventManager.OnUseAddTray += UseItemAddTray;

        EventManager.OnUseHand += UseItemHand;

        EventManager.OnUseShuffle += UseItemShufflePig;

        EventManager.OnClickBlock += ClickBlock;

        InitializePlates();
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
        foreach (var p in allPlates) if (p != null) Destroy(p.gameObject);
        allPlates.Clear();
        availablePlates.Clear();
        activePlateMap.Clear();

        for (int i = 0; i < 5; i++)
        {
            Vector3 spawnPos = traySlotOrigin.position - Vector3.right * (i * plateStackOffset);

            GameObject go = Instantiate(platePrefab);
            Transform plate = go.transform;
            plate.position = spawnPos;
            plate.localRotation = Quaternion.Euler(0, 0, -45);
            plate.SetParent(tray);
            plate.transform.localScale = Vector3.one;

            allPlates.Add(plate);
            availablePlates.Enqueue(plate);
        }
    }

    private void AssignPlateToPig(PigComponent pig)
    {
        if (availablePlates.Count > 0)
        {
            Transform plate = availablePlates.Dequeue();
            StartCoroutine(AnimatePlateToPig(plate, pig));
        }
    }

    private IEnumerator AnimatePlateToPig(Transform plate, PigComponent pig)
    {
        if (plate == null || pig == null) yield break;

        activePlateMap[pig] = plate;

        Quaternion startRot = plate.rotation;

        Vector3 intermediatePos = traySlotOrigin.position + Vector3.right * 0.5f - Vector3.up * 0.1f;

        float elapsed = 0;
        float duration = 0.35f;


        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            plate.position = Vector3.Lerp(plate.position, intermediatePos, t);

            plate.rotation = Quaternion.Lerp(startRot, Quaternion.Euler(0, 0, 90), t);

            yield return null;
        }

        if (!activePlateMap.TryGetValue(pig, out Transform trackedPlate) || trackedPlate != plate)
        {
            StartCoroutine(ReturnPlateToOrigin(plate));
            yield break;
        }

        if (pig != null)
        {
            plate.SetParent(pig.transform);
            plate.localPosition = new Vector3(-0.02f, 0.184f, -0.101f);
            plate.localRotation = Quaternion.Euler(0, 0, 90);
            plate.localScale = new Vector3(80, 100, 100);

            pig.currentPlate = plate;
        }
        else
        {
            activePlateMap.Remove(pig);
            StartCoroutine(ReturnPlateToOrigin(plate));
        }
    }
    public void ClickBlock(string color)
    {
        foreach (Transform block in blockGroup)
        {
            Block blockComp = block.GetComponent<Block>();
            if (blockComp != null && blockComp.color == color)
            {
                blockPrefabsByColor.Add(block.gameObject);
            }
        }

        foreach (PigComponent pig in pigSpawnPos.GetComponentsInChildren<PigComponent>())
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

        supertCatPrefab.SetActive(true);
        var cat = supertCatPrefab.GetComponent<SuperCat>();
        cat.AddAllTarget(blockPrefabsByColor, ColorGameConfig.instance.GetColorByName(color));

        blockPrefabsByColor.Clear();
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

        StartCoroutine(CheckFinalRushNextFrame());
    }

    private IEnumerator CheckFinalRushNextFrame()
    {
        yield return null;
        CheckAndEnableFinalRush();
    }

    private void CheckAndEnableFinalRush()
    {
        if (_isFinalRush) return;

        PigComponent[] allPigs = pigSpawnPos.GetComponentsInChildren<PigComponent>();

        if (allPigs.Length <= 5 && allPigs.Length > 0)
        {
            _isFinalRush = true;

            foreach (var p in allPigs)
            {
                p.SetConveyorSpeedMultiplier(2f);
            }

            ApplyConveyorSpeedMultiplier(2f);

            Debug.Log("Final Rush Activated! Speed x2");
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
        if (activePlateMap.ContainsKey(pig))
        {
            activePlateMap.Remove(pig);
        }
        pig.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        int index = pigsInTempQueue.Count - 1;

        Vector3 targetPos = startTempQueuePos.position + (Vector3.right * (index * 0.9f));

        pig.JumpToQueue(targetPos, startTempQueuePos.rotation, index);
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

        _isFinalRush = false;
        _straightSlot = 0;
        totalBlockCount = 0;
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
    }

    public void SelectPig(PigComponent pig)
    {
        if (pig == null || isProcessingClick) return;

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
            pig.SetIsOnTop(true);
        }

        if (pig.IsLinkedPig())
        {
            if (!pig.IsWholeLinkOnTop())
            {
                AudioController.instance.PlaySound(AudioIndex.invalid_cat.ToString());
                return;
            }

            PigComponent leftmost = pig.GetLeftmostPig();
            List<PigComponent> linkedPigs = GetPigChain(leftmost);

            if (_straightSlot + linkedPigs.Count > _maxstraightSlot)
            {
                EventManager.OnFullConveyorSlot?.Invoke();
                AudioController.instance.PlaySound(AudioIndex.error.ToString());
                return;
            }

            StartCoroutine(ProcessLinkedPigsRoutine(linkedPigs));
            return;
        }

        if (_straightSlot >= _maxstraightSlot)
        {
            EventManager.OnFullConveyorSlot?.Invoke();
            AudioController.instance.PlaySound(AudioIndex.error.ToString());
            return;
        }

        isProcessingClick = true;
        ProcessPigData(pig);
        StartCoroutine(ResetClickFlag(0.15f));
    }


    private IEnumerator ResetClickFlag(float time)
    {
        yield return new WaitForSeconds(time);
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

        bool isFromTemp = pigsInTempQueue.Contains(linkedPigs[0]);
        bool isFromQueue = pigsInQueue.Contains(linkedPigs[0]);

        bool isFromLane = !isFromTemp && !isFromQueue;

        int firstQueueIndex = -1;
        if (isFromTemp) firstQueueIndex = pigsInTempQueue.IndexOf(linkedPigs[0]);
        else if (isFromQueue) firstQueueIndex = pigsInQueue.IndexOf(linkedPigs[0]);

        StartCoroutine(ResetClickFlag(0.15f));

        foreach (PigComponent p in linkedPigs)
        {
            bool hasFinishedJump = false;

            if (isFromTemp) p.transform.localScale = Vector3.one;

            if (!isFromLane && !pigsInConveyor.Contains(p)) pigsInConveyor.Add(p);

            AssignPlateToPig(p);

            p.JumpTo(() =>
            {
                if (isFromLane)
                {
                    RemovePigFromLane(p);
                }
                hasFinishedJump = true;
            });

            yield return new WaitUntil(() => hasFinishedJump);
        }

        if (!isFromLane)
        {
            List<PigComponent> targetQueue = isFromTemp ? pigsInTempQueue : pigsInQueue;
            foreach (PigComponent p in linkedPigs)
            {
                if (targetQueue.Contains(p)) targetQueue.Remove(p);
            }
            _straightSlot = Mathf.Max(0, _straightSlot - linkedPigs.Count);
            UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);

            if (firstQueueIndex != -1)
                RearrangeQueue(firstQueueIndex, isFromTemp);
        }

        // yield return StartCoroutine(ResetClickFlag());
    }

    private void ProcessPigData(PigComponent pig, Action onComplete = null)
    {
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
        AudioController.instance.PlaySound(AudioIndex.valid_cat.ToString());

        if (pigsInQueue.Contains(pig) || pigsInTempQueue.Contains(pig))
        {
            bool isFromTempQueue = pigsInTempQueue.Contains(pig);
            HandlePigClickedFromQueue(pig, onComplete, isFromTempQueue);
            return;
        }

        int laneIndex = pig.laneIndex;
        if (pigsByLane.ContainsKey(laneIndex))
        {
            List<PigComponent> pigsInLane = pigsByLane[laneIndex];
            if (pigsInLane.Count > 0)
            {

                AssignPlateToPig(pig);
                pig.JumpTo(onComplete: () =>
                {
                    RemovePigFromLane(pig);
                    onComplete?.Invoke();
                });
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
        if (_isFinalRush) removedPig.SetConveyorSpeedMultiplier(2f);

        pigsInLane.Sort((a, b) => b.transform.localPosition.z.CompareTo(a.transform.localPosition.z));

        for (int i = 0; i < pigsInLane.Count; i++)
        {
            Vector3 newLocalPos = pigsInLane[i].transform.localPosition;
            newLocalPos.z = -(i * spacing);

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

    private void SpawnMap()
    {
        SpawnMapAsync().Forget();
    }

    private async UniTask SpawnMapAsync()
    {
        CleanupSpawnedObjects();
        _maxstraightSlot = 5;
        ResetData();
        if (SceneManager.GetActiveScene().name == "6.play_test")
        {
            if (GameManagerForTesting.Instance.TryGetPlayTestConfig(out DataConfig playTestData))
            {
                Debug.Log("<color=green>PlayTest Mode: Loading from Temp Data only.</color>");
                SpawnBlocks(playTestData.width, playTestData.height, playTestData.gridData);
                SpawnPigs(playTestData.lanes);
                return;
            }
            else
            {
                return;
            }
        }

        int levelToLoad = LevelController.GetMaxLevelUnlock();
        LevelData data = await LoadLevelData(levelToLoad);
        if (data != null)
        {
            SpawnBlocks(data.width, data.height, data.gridData);
            SpawnPigs(data.lanes);
        }
        else
        {
            Debug.LogError("Failed to load level data for level: " + levelToLoad);
        }
    }

    public async UniTask<LevelData> LoadLevelData(int levelNumber)
    {
        var filename = $"L{levelNumber:D4}.json";
        var filetext = await StaticUtils.GetStreamingFileText(filename);
        var currentLevel = JsonUtility.FromJson<LevelData>(filetext);
        return currentLevel;
    }

    private void CleanupSpawnedObjects()
    {
        foreach (Transform child in blockGroup)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in pigSpawnPos)
        {
            Destroy(child.gameObject);
        }

        foreach (Link link in activeLinks)
        {
            if (link != null)
            {
                Destroy(link.gameObject);
            }
        }
        activeLinks.Clear();
    }

    private void SpawnBlocks(int width, int height, List<string> gridData)
    {
        if (gridData == null || width <= 0 || height <= 0)
        {
            return;
        }

        Vector3 scale = Vector3.one;

        int nonEmptyCount = gridData.Count(s => s != "empty");
        Debug.Log("count non empty: " + nonEmptyCount);
        if (nonEmptyCount > 1200)
        {
            scale = new Vector3(0.6f, 1, 0.6f);
            blockSpacing = 0.25f;
        }
        else if (nonEmptyCount > 600)
        {
            scale = new Vector3(0.8f, 1, 0.8f);
            blockSpacing = 0.55f;
        }
        else if (nonEmptyCount > 200)
        {
            scale = Vector3.one;
            blockSpacing = 0.75f;
        }
        else
        {
            scale = new Vector3(1.2f, 1, 1.2f);
            blockSpacing = 0.85f;
        }

        int W = width;
        int H = height;
        float offsetX = (W - 1) * blockSpacing / 2f;
        float offsetY = (H - 1) * blockSpacing / 2f;

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                int index = y * W + x;
                if (index < 0 || index >= gridData.Count) continue;

                string colorType = gridData[index];

                if (colorType == "empty") continue;
                Vector3 localPos = new Vector3(
                    (x * blockSpacing) - offsetX,
                    0,
                    ((H - 1 - y) * blockSpacing) - offsetY
                );
                Vector3 worldPos = blockSpawnPoint.TransformPoint(localPos);
                GameObject newBlock = Instantiate(blockPrefab, worldPos, Quaternion.identity, blockGroup);
                newBlock.transform.localScale = scale;

                ApplyMaterial(newBlock, colorType);

                totalBlockCount++;
            }
        }
    }

    private void SpawnPigs(List<LaneConfig> lanes)
    {
        foreach (Transform child in pigSpawnPos)
        {
            Destroy(child.gameObject);
        }

        pigsByLane.Clear();

        if (lanes == null || lanes.Count == 0) return;

        int laneCount = lanes.Count;
        float laneOffsetX = (laneCount - 1) * 1f / 2f;
        for (int i = 0; i < laneCount; i++)
        {
            var currentLane = lanes[i];

            if (!pigsByLane.ContainsKey(i))
            {
                pigsByLane[i] = new List<PigComponent>();
            }
            if (currentLane.pigs == null) continue;

            for (int j = 0; j < currentLane.pigs.Count; j++)
            {
                string colorType = currentLane.pigs[j].colorName;
                var bulletCount = currentLane.pigs[j].bullets;
                var color = ColorGameConfig.instance.GetColorByName(colorType);
                if (colorType == "empty") continue;
                Vector3 localPos = new Vector3(
                    (i * 1f) - laneOffsetX,
                    0,
                    -(j * spacing)
                );

                Vector3 worldPos = pigSpawnPos.TransformPoint(localPos);

                GameObject newPig = Instantiate(pigPrefab, worldPos, Quaternion.identity, pigSpawnPos);

                PigComponent pigComp = newPig.GetComponent<PigComponent>();

                if (pigComp != null)
                {
                    pigComp.Initialize(colorType, bulletCount, i, color, speed, jumpToQueueSpeed, allWaypoints, currentLane.pigs[j].isHidden);
                    pigsByLane[i].Add(pigComp);
                    pigComp.SetIsOnTop(j == 0);
                }

                var renderer = newPig.GetComponentInChildren<Renderer>();

                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }

        }

        BuildPigLinks(lanes);
    }

    private void BuildPigLinks(List<LaneConfig> lanes)
    {
        Dictionary<PigComponent, HashSet<PigComponent>> linkGraph = new Dictionary<PigComponent, HashSet<PigComponent>>();
        List<PendingLink> pendingLinks = new List<PendingLink>();
        HashSet<string> uniqueLinkKeys = new HashSet<string>();

        for (int laneIndex = 0; laneIndex < lanes.Count; laneIndex++)
        {
            var currentLane = lanes[laneIndex];
            if (currentLane.pigs == null) continue;

            for (int pigIndex = 0; pigIndex < currentLane.pigs.Count; pigIndex++)
            {
                if (pigIndex >= pigsByLane[laneIndex].Count) continue;

                PigComponent sourcePig = pigsByLane[laneIndex][pigIndex];
                var pigData = currentLane.pigs[pigIndex];

                TryRegisterPendingLink(sourcePig, pigData.pigLeft, pendingLinks, uniqueLinkKeys);
                TryRegisterPendingLink(sourcePig, pigData.pigRight, pendingLinks, uniqueLinkKeys);
            }
        }

        foreach (var pendingLink in pendingLinks)
        {
            AddLinkToGraph(linkGraph, pendingLink.sourcePig, pendingLink.targetPig);

            Link linkObject = Instantiate(linkPrefabs);
            linkObject.transform.position = Vector3.up * -2f;
            activeLinks.Add(linkObject);
            linkObject.SetColor(
                pendingLink.sourcePig.color,
                pendingLink.targetPig.color,
                pendingLink.sourcePig,
                pendingLink.targetPig);
        }

        ApplyLinkedNeighbors(linkGraph);
    }

    private void TryRegisterPendingLink(
        PigComponent sourcePig,
        PigMarker marker,
        List<PendingLink> pendingLinks,
        HashSet<string> uniqueLinkKeys)
    {
        if (sourcePig == null || marker == null || !marker.IsValid())
        {
            return;
        }

        if (!TryGetPigByMarker(marker, out PigComponent targetPig) || targetPig == sourcePig)
        {
            return;
        }

        string linkKey = GetLinkKey(sourcePig, targetPig);
        if (!uniqueLinkKeys.Add(linkKey))
        {
            return;
        }

        pendingLinks.Add(new PendingLink
        {
            sourcePig = sourcePig,
            targetPig = targetPig
        });
    }

    private bool TryGetPigByMarker(PigMarker marker, out PigComponent pig)
    {
        pig = null;

        if (marker == null || !marker.IsValid())
        {
            return false;
        }

        if (!pigsByLane.TryGetValue(marker.LaneIndex, out List<PigComponent> lanePigs))
        {
            return false;
        }

        if (marker.index < 0 || marker.index >= lanePigs.Count)
        {
            return false;
        }

        pig = lanePigs[marker.index];
        return pig != null;
    }

    private void AddLinkToGraph(Dictionary<PigComponent, HashSet<PigComponent>> linkGraph, PigComponent pigA, PigComponent pigB)
    {
        if (!linkGraph.TryGetValue(pigA, out HashSet<PigComponent> pigALinks))
        {
            pigALinks = new HashSet<PigComponent>();
            linkGraph[pigA] = pigALinks;
        }

        if (!linkGraph.TryGetValue(pigB, out HashSet<PigComponent> pigBLinks))
        {
            pigBLinks = new HashSet<PigComponent>();
            linkGraph[pigB] = pigBLinks;
        }

        pigALinks.Add(pigB);
        pigBLinks.Add(pigA);
    }

    private void ApplyLinkedNeighbors(Dictionary<PigComponent, HashSet<PigComponent>> linkGraph)
    {
        HashSet<PigComponent> visited = new HashSet<PigComponent>();

        foreach (var entry in linkGraph)
        {
            PigComponent startPig = entry.Key;
            if (visited.Contains(startPig))
            {
                continue;
            }

            List<PigComponent> component = CollectLinkedComponent(startPig, linkGraph, visited);
            if (component.Count == 0)
            {
                continue;
            }

            PigComponent orderedStart = GetOrderedComponentStart(component, linkGraph);
            List<PigComponent> orderedPigs = OrderLinkedComponent(orderedStart, linkGraph);

            for (int index = 0; index < orderedPigs.Count; index++)
            {
                PigComponent leftPig = index > 0 ? orderedPigs[index - 1] : null;
                PigComponent rightPig = index < orderedPigs.Count - 1 ? orderedPigs[index + 1] : null;
                orderedPigs[index].SetLinkedNeighbors(leftPig, rightPig);
            }
        }
    }

    private List<PigComponent> CollectLinkedComponent(
        PigComponent startPig,
        Dictionary<PigComponent, HashSet<PigComponent>> linkGraph,
        HashSet<PigComponent> visited)
    {
        List<PigComponent> component = new List<PigComponent>();
        Queue<PigComponent> queue = new Queue<PigComponent>();
        queue.Enqueue(startPig);
        visited.Add(startPig);

        while (queue.Count > 0)
        {
            PigComponent currentPig = queue.Dequeue();
            component.Add(currentPig);

            foreach (PigComponent neighbor in linkGraph[currentPig])
            {
                if (visited.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return component;
    }
    private PigComponent GetOrderedComponentStart(List<PigComponent> component, Dictionary<PigComponent, HashSet<PigComponent>> linkGraph)
    {
        PigComponent startPig = null;

        foreach (PigComponent pig in component)
        {
            if (linkGraph[pig].Count <= 1)
            {
                if (startPig == null || IsPigBefore(pig, startPig))
                {
                    startPig = pig;
                }
            }
        }

        if (startPig != null)
        {
            return startPig;
        }

        startPig = component[0];
        for (int index = 1; index < component.Count; index++)
        {
            if (IsPigBefore(component[index], startPig))
            {
                startPig = component[index];
            }
        }

        return startPig;
    }

    private List<PigComponent> OrderLinkedComponent(PigComponent startPig, Dictionary<PigComponent, HashSet<PigComponent>> linkGraph)
    {
        List<PigComponent> orderedPigs = new List<PigComponent>();
        HashSet<PigComponent> orderedSet = new HashSet<PigComponent>();
        PigComponent previousPig = null;
        PigComponent currentPig = startPig;

        while (currentPig != null && orderedSet.Add(currentPig))
        {
            orderedPigs.Add(currentPig);

            PigComponent nextPig = null;
            foreach (PigComponent neighbor in linkGraph[currentPig])
            {
                if (neighbor != previousPig)
                {
                    if (nextPig == null || IsPigBefore(neighbor, nextPig))
                    {
                        nextPig = neighbor;
                    }
                }
            }

            previousPig = currentPig;
            currentPig = nextPig;
        }

        return orderedPigs;
    }

    private bool IsPigBefore(PigComponent pigA, PigComponent pigB)
    {
        if (pigA.laneIndex != pigB.laneIndex)
        {
            return pigA.laneIndex < pigB.laneIndex;
        }

        return pigA.transform.localPosition.z > pigB.transform.localPosition.z;
    }

    private string GetLinkKey(PigComponent pigA, PigComponent pigB)
    {
        int pigAId = pigA.GetInstanceID();
        int pigBId = pigB.GetInstanceID();
        return pigAId < pigBId ? pigAId + "_" + pigBId : pigBId + "_" + pigAId;
    }

    private void ApplyMaterial(GameObject obj, string colorName)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = obj.GetComponentInChildren<MeshRenderer>();
        }
        var blockComponent = obj.GetComponent<Block>();
        if (renderer == null || blockComponent == null) return;

        blockComponent.color = colorName;
        renderer.material.color = ColorGameConfig.instance.GetColorByName(colorName);
    }

    private void HandlePigEnterQueue(PigComponent pig)
    {
        if (pig == null) return;

        if (activePlateMap.TryGetValue(pig, out Transform plate))
        {
            if (pig.currentPlate != null)
            {
                StartCoroutine(ReturnPlateToOrigin(pig.currentPlate));
                pig.currentPlate = null;
            }
            activePlateMap.Remove(pig);
        }

        if (_isFinalRush)
        {
            pig.SetConveyorSpeedMultiplier(2f);
            return;
        }
        pig.SetConveyorSpeedMultiplier(1f);

        if (queuePos == null || queuePos.Count == 0)
        {
            return;
        }

        int queueIndex;

        if (pig.IsLinkedPig())
        {
            // Tìm vị trí của thành viên nhóm đã vào queue gần nhất
            PigComponent leftmost = pig.GetLeftmostPig();
            List<PigComponent> chain = GetPigChain(leftmost);

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
                for (int i = queueIndex + 1; i < pigsInQueue.Count; i++)
                {
                    pigsInQueue[i].MoveInQueue(queuePos[i].position, queuePos[i].rotation, i);
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
        }

        pigsInConveyor.Remove(pig);
        pig.JumpToQueue(queuePos[queueIndex].position, queuePos[queueIndex].rotation, 1.5f);

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
        availablePlates.Enqueue(plate);

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
    private bool ShouldSkipQueueForFinalRush()
    {
        PigComponent[] allPigs = pigSpawnPos.GetComponentsInChildren<PigComponent>();
        return allPigs.Length <= 5;
    }

    private void ApplyConveyorSpeedMultiplier(float multiplier)
    {
        foreach (PigComponent conveyorPig in pigsInConveyor)
        {
            if (conveyorPig != null)
            {
                conveyorPig.SetConveyorSpeedMultiplier(multiplier);
            }
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
        if (totalBlockCount <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinStage();
            }
        }
    }

    private void HandlePigClickedFromQueue(PigComponent pig, Action complete, bool isFromTempQueue = false)
    {
        if (pig.IsLinkedPig())
        {
            PigComponent leftmost = pig.GetLeftmostPig();
            List<PigComponent> linkedPigs = GetPigChain(leftmost);

            StartCoroutine(ProcessLinkedPigsRoutine(linkedPigs));
            complete?.Invoke();
            return;
        }
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

        AssignPlateToPig(pig);
        pig.JumpTo(() =>
        {
            if (_isFinalRush) pig.SetConveyorSpeedMultiplier(2f);
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

    public void UseItemShufflePig()
    {
        pigsByLane = Helper.ShuffleHeoDictionary(pigsByLane);

        int laneCount = pigsByLane.Count;
        float laneOffsetX = (laneCount - 1) * 0.85f / 2f;

        foreach (var laneEntry in pigsByLane)
        {
            int laneIndex = laneEntry.Key;

            for (int pigIndex = 0; pigIndex < laneEntry.Value.Count; pigIndex++)
            {
                PigComponent pig = laneEntry.Value[pigIndex];
                pig.laneIndex = laneIndex;

                Vector3 newLocalPos = new Vector3(
                    (laneIndex * 0.95f) - laneOffsetX,
                    0,
                    -(pigIndex * spacing)
                );

                pig.MoveTo(newLocalPos);
                pig.SetIsOnTop(pigIndex == 0);
            }
        }
    }
}
