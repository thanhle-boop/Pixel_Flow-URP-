using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    // private int tempScore = 0;

    private bool isProcessingClick = false;
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

    private void OnDisable() {
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

            GameObject go = Instantiate(platePrefab, spawnPos, traySlotOrigin.rotation);
            Transform plate = go.transform;
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
            pig.currentPlate = plate;
            plate.transform.SetParent(pig.transform);
            plate.transform.localPosition = new Vector3(0.051f, 0.184f, -0.067f);
            plate.transform.localRotation = Quaternion.identity;
            plate.transform.localRotation = new Quaternion(90, 90, 0, 0);
            plate.transform.localScale = new Vector3(80, 100, 100);

            activePlateMap[pig] = plate;
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
                pig.ExecuteDestroy();
                RemovePigFromLane(pig);
                HandlePigClickedFromQueue(pig, pigsInTempQueue.Contains(pig));
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
    }
    private void IncreaseStraightSlot()
    {
        _straightSlot = _straightSlot + 1 > _maxstraightSlot ? _maxstraightSlot : _straightSlot + 1;
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);
    }

    private void LoseGame()
    {
        // tempScore = 0;
        StopAllPigAnimations();
    }
    private void WinGame()
    {
        DataManager.instance.IncreaseLevel();
    }

    private void ContinueGame()
    {
        List<PigComponent> tempMovingPigs = new List<PigComponent>(pigsInConveyor);
        pigsInConveyor.Clear();

        foreach (PigComponent pig in tempMovingPigs)
        {
            if (pig == null) continue;

            MovePigToTempQueue(pig);
        }

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


            foreach (PigComponent p in pigsToMove)
            {
                if (pigsInQueue.Contains(p))
                {
                    pigsInQueue.Remove(p);
                    MovePigToTempQueue(p);
                }
            }
        }
    }

    private void MovePigToTempQueue(PigComponent pig)
    {
        pigsInTempQueue.Add(pig);

        if (pig.currentPlate != null)
        {
            StartCoroutine(ReturnPlateToOrigin(pig.currentPlate));
            pig.currentPlate = null;
            activePlateMap.Remove(pig);
        }

        _straightSlot = Mathf.Max(0, _straightSlot - 1);
        UIManager.Instance.UpdateStraightSlot(_straightSlot, _maxstraightSlot);

        pig.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        int index = pigsInTempQueue.Count - 1;
        Vector3 targetPos = startTempQueuePos.position + 0.9f * index * Vector3.right;

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
            SoundManager.Instance.PlaySound(SoundManager.Instance.invalidCat);
            return;
        }

        if (onHandItemUsed)
        {
            onHandItemUsed = false;
            EventManager.OnEndHand?.Invoke();
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
                // AssignPlateToPig(pig);

                pig.JumpTo(onComplete: () =>
                {
                    RemovePigFromLane(pig);
                    AssignPlateToPig(pig);
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

            pigsInLane[i].MoveTo(newLocalPos);

            pigsInLane[i].SetIsOnTop(i == 0);
        }
    }

    private void SpawnMap()
    {
        CleanupSpawnedObjects();
        ResetData();

        // Kiểm tra tên scene mới: 6.play_test
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

        int levelToLoad = DataManager.instance.CurrentLevel;
        LevelData data = LoadLevelData(levelToLoad);
        if (data != null)
        {
            SpawnBlocks(data.width, data.height, data.gridData);
            SpawnPigs(data.lanes);
        }
    }

    public LevelData LoadLevelData(int levelNumber)
    {
        LevelData currentLevel = null;
        string fileName = $"L{levelNumber + 1:D4}_V1.json";
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            currentLevel = JsonUtility.FromJson<LevelData>(jsonContent);
        }

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

    private bool TryGetPlayTestData(out DataConfig playTestData)
    {
        playTestData = null;

        if (SceneManager.GetActiveScene().name != "6.playTest")
        {
            return false;
        }

        return GameManagerForTesting.Instance.TryGetPlayTestConfig(out playTestData);
    }

    private void SpawnBlocks(int width, int height, List<string> gridData)
    {
        if (gridData == null || width <= 0 || height <= 0)
        {
            return;
        }

        Vector3 scale = Vector3.one;

        int nonEmptyCount = gridData.Count(s => s != "empty");
        if(nonEmptyCount > 400)
        {
            scale = new Vector3(0.8f, 1, 0.8f);
            blockSpacing = 0.45f;
        }
        else if (nonEmptyCount > 900)
        {
            scale = new Vector3(0.6f, 1, 0.6f);
            blockSpacing = 0.25f;
        }
        else
        {
            scale = Vector3.one;
            blockSpacing = 0.75f;
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
            // Bắt đầu quá trình thu hồi đĩa
            if (pig.currentPlate != null)
            {
                StartCoroutine(ReturnPlateToOrigin(pig.currentPlate));
                pig.currentPlate = null; // Xóa tham chiếu trên pig để tránh thu hồi 2 lần
            }
            activePlateMap.Remove(pig);
        }

        if (ShouldSkipQueueForFinalRush())
        {
            var pigRemaining = pigSpawnPos.GetComponentsInChildren<PigComponent>();
            for (int i = 0; i < pigRemaining.Length; i++)
            {
                ApplyConveyorSpeedMultiplier(2f);
                pigRemaining[i].SetConveyorSpeedMultiplier(2f);

            }
            return;
        }
        pig.SetConveyorSpeedMultiplier(1f);

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
        plate.rotation = traySlotOrigin.rotation;
        plate.localScale = Vector3.one;
        availablePlates.Enqueue(plate);

        RearrangePlatesInTray();
    }

    private void RearrangePlatesInTray()
    {
        int index = 0;
        // CHỈ sắp xếp những đĩa đang rảnh (nằm trong Queue)
        foreach (Transform p in availablePlates)
        {
            // Bước 1: Tính toán vị trí Local (Tương đối)
            // Chỉ truyền giá trị khoảng cách vào đây, KHÔNG cộng thêm .position
            ;
            Vector3 localOffset = Vector3.right * (index * plateStackOffset);

            // Bước 2: Chuyển sang World Position
            // TransformPoint sẽ tự động tính toán dựa trên Vị trí, Xoay và Scale của traySlotOrigin
            // Vector3 targetWorldPos = traySlotOrigin.TransformPoint(localOffset);

            // Bước 3: Cập nhật (Dùng MovePosition nếu đĩa có Rigidbody, hoặc .position nếu không)
            p.position = traySlotOrigin.position - Vector3.right * (index * plateStackOffset);
            // p.rotation = traySlotOrigin.rotation;

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
        // tempScore += 100;
        // UIManager.Instance.UpdateScore(tempScore);
        if (totalBlockCount <= 0)
        {
            if (GameManager.Instance != null)
            {
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

        pig.JumpTo(() =>
        {
            AssignPlateToPig(pig);
            RearrangeQueue(removedIndex, isFromTempQueue);

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
