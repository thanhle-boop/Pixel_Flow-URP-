using System;
using System.Collections;
using System.Collections.Generic;
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

    public bool isProcessingClick = false;
    private bool onHandItemUsed = false;
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
    [SerializeField] private float speed = 1f;
    [SerializeField] private float jumpFromLaneSpeed = 6f;
    private float jumpFromQueueSpeed = 3.5f;
    public Link linkPrefabs;
    public List<GameObject> blockPrefabsByColor;
    public float spacing = 1.2f;
    [Header("Plate Settings")]
    public GameObject platePrefab;
    public Transform traySlotOrigin;
    public float plateStackOffset = 0.1f;
    private Queue<Transform> availablePlates = new Queue<Transform>();
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
        CheckAndEnableFinalRush();
    }

    private IEnumerator CheckFinalRushNextFrame()
    {
        yield return null;
        CheckAndEnableFinalRush();
    }

    private void CheckAndEnableFinalRush()
    {

        PigComponent[] allPigs = pigSpawnPos.GetComponentsInChildren<PigComponent>();
        if (allPigs.Length <= 6 && allPigs.Length > 0)
        {

            foreach (var p in allPigs)
            {
                p.SetConveyorSpeedMultiplier(2f);
                p.isRush = true;
            }

            // ApplyConveyorSpeedMultiplier(2f);

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
        // if (activePlateMap.ContainsKey(pig))
        // {
        //     activePlateMap.Remove(pig);
        // }
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
        pigsByLane.Clear();
        pigsInQueue.Clear();
        pigsInTempQueue.Clear();

        _straightSlot = 0;
        totalBlockCount = 0;
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
    }

    public void SelectPig(PigComponent pig)
    {

        if (pig == null || isProcessingClick)
        {
            return;
        }

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
    // GetPigChain trong Spawner Manager:
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

            p.JumpTo(jumpFromQueueSpeed, () =>
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
            // _straightSlot = Mathf.Max(0, _straightSlot - linkedPigs.Count);
            // UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);

            if (firstQueueIndex != -1)
                RearrangeQueue(firstQueueIndex, isFromTemp);
        }
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
                pig.JumpTo(jumpFromLaneSpeed, onComplete: () =>
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
        InitializePlates();
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
        Debug.Log("Non-empty block count: " + nonEmptyCount);
        if (nonEmptyCount > 1000)
        {
            scale = new Vector3(0.8f, 1, 0.8f);
            blockSpacing = 0.4f;
        }
        else if (nonEmptyCount > 800)
        {
            scale = new Vector3(0.9f, 1, 0.9f);
            blockSpacing = 0.55f;
        }
        else if (nonEmptyCount > 300)
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
        Debug.Log($"Spawning blocks with  {W} and spacing {H}");
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
                    pigComp.Initialize(colorType, bulletCount, i, color, speed, allWaypoints, currentLane.pigs[j].isHidden);
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

        // BuildPigLinks(lanes);
        SpawnLink(lanes);
    }

    private void SpawnLink(List<LaneConfig> lanes)
    {
        for (int i = 0; i < lanes.Count; i++)
        {
            var currentLane = lanes[i];
            for (int j = 0; j < currentLane.pigs.Count; j++)
            {
                PigComponent pigLeft = (currentLane.pigs[j].pigLeft != null && currentLane.pigs[j].pigLeft.LaneIndex >= 0)
                    ? pigsByLane[currentLane.pigs[j].pigLeft.LaneIndex][currentLane.pigs[j].pigLeft.index]
                    : null;

                PigComponent pigRight = (currentLane.pigs[j].pigRight != null && currentLane.pigs[j].pigRight.LaneIndex >= 0)
                    ? pigsByLane[currentLane.pigs[j].pigRight.LaneIndex][currentLane.pigs[j].pigRight.index]
                    : null;
                var sourcePig = pigsByLane[i][j];
                if (pigLeft != null)
                {
                    Link linkObject = Instantiate(linkPrefabs);
                    linkObject.transform.position = Vector3.up * -2f;
                    activeLinks.Add(linkObject);
                    linkObject.SetColor(
                        sourcePig.color,
                        pigLeft.color,
                        sourcePig,
                        pigLeft);
                }
                if (pigRight != null)
                {
                    Link linkObject = Instantiate(linkPrefabs);
                    linkObject.transform.position = Vector3.up * -2f;
                    activeLinks.Add(linkObject);
                    linkObject.SetColor(
                        sourcePig.color,
                        pigRight.color,
                        sourcePig,
                        pigRight);
                }
                sourcePig.SetLinkPig(pigLeft, pigRight);

            }
        }
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

        AssignPlateToPig(pig);
        pig.JumpTo(jumpFromQueueSpeed, () =>
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
