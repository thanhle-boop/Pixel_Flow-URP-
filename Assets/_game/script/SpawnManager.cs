using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    [Header("Block Settings")]
    public Transform blockGroup;
    private float blockSpacing = 1.2f;

    public GameObject blockPrefab;
    public Transform blockSpawnPoint;
    private int totalBlockCount = 0;
    public List<GameObject> blockPrefabsByColor;
    public GameObject supertCatPrefab;

    [Header("Pig Settings")]
    public Dictionary<int, List<PigComponent>> pigsByLane = new Dictionary<int, List<PigComponent>>();
    private List<Link> activeLinks = new List<Link>();
    public Transform pigSpawnPoint;
    public Transform pigGroup;
    public GameObject pigPrefab;
    public float pigSpeed = 1f;
    public List<Transform> allWaypoints;
    public float pigSpacing = 1.5f;

    [Header("Link Settings")]
    public Link linkPrefabs;

    void OnEnable()
    {
        EventManager.OnStartGame += SpawnBlockLogic;
        EventManager.OnBlockDestroyed += OnBlockDestroyed;
        EventManager.OnClickBlock += ClickBlock;
    }

    void OnDestroy()
    {
        EventManager.OnStartGame -= SpawnBlockLogic;
        EventManager.OnBlockDestroyed -= OnBlockDestroyed;
        EventManager.OnClickBlock -= ClickBlock;
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

        supertCatPrefab.SetActive(true);
        var cat = supertCatPrefab.GetComponent<SuperCat>();
        cat.AddAllTarget(blockPrefabsByColor, ColorGameConfig.instance.GetColorByName(color));

        blockPrefabsByColor.Clear();
    }
    private void SpawnBlockLogic()
    {
        SpawnMapAsync().Forget();
    }

    private async UniTask SpawnMapAsync()
    {
        CleanupSpawnedObjects();
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
        LevelData data = await GameUtility.LoadLevelData(levelToLoad);
        if (data != null)
        {
            SpawnBlocks(data.width, data.height, data.gridData);
            SpawnPigs(data.lanes);


        }
        else
        {
            Debug.LogError("Failed to load level data for level: " + levelToLoad);
        }
        LoaderOverlayManager.instance.EndOverlay();
    }

    private void CleanupSpawnedObjects()
    {
        foreach (Transform child in blockGroup)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in pigGroup)
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

    private void SpawnPigs(List<LaneConfig> lanes)
    {
        foreach (Transform child in pigGroup)
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
                    -(j * pigSpacing)
                );

                Vector3 worldPos = pigGroup.TransformPoint(localPos);

                GameObject newPig = Instantiate(pigPrefab, worldPos, Quaternion.identity, pigGroup);

                PigComponent pigComp = newPig.GetComponent<PigComponent>();

                if (pigComp != null)
                {
                    pigComp.Initialize(colorType, bulletCount, i, color, pigSpeed, allWaypoints, currentLane.pigs[j].isHidden);
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
        SpawnLink(lanes);
    }

    private void ResetData()
    {
        pigsByLane.Clear();
        totalBlockCount = 0;
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
}
