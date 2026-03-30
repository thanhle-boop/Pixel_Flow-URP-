using System.Collections.Generic;
using UnityEngine;

public class GameManagerForTesting : Singleton<GameManagerForTesting>
{

	public int configIndex = -1;
	public DataConfig CurrentPlayTestConfig { get; private set; }
	public string[,] SavedTempGrid { get; private set; }

	public void SetSavedTempGrid(string[,] tempGrid)
	{
		if (tempGrid == null) { SavedTempGrid = null; return; }
		int sizeX = tempGrid.GetLength(0);
		int sizeY = tempGrid.GetLength(1);
		SavedTempGrid = new string[sizeX, sizeY];
		for (int x = 0; x < sizeX; x++)
			for (int y = 0; y < sizeY; y++)
				SavedTempGrid[x, y] = tempGrid[x, y];
	}

	public void SetPlayTestConfig(DataConfig source)
	{
		CurrentPlayTestConfig = source == null ? null : CloneDataConfig(source);
	}

	public bool TryGetPlayTestConfig(out DataConfig data)
	{
		data = CurrentPlayTestConfig;
		return data != null && data.gridData != null && data.lanes != null;
	}

	public void ClearPlayTestConfig()
	{
		CurrentPlayTestConfig = null;
	}

	private DataConfig CloneDataConfig(DataConfig source)
	{
		DataConfig clone = new DataConfig
		{
			sourceJson = source.sourceJson,
			levelIndex = source.levelIndex,
			width = source.width,
			height = source.height,
			gridData = source.gridData != null ? new List<string>(source.gridData) : new List<string>(),
			lanes = new List<LaneConfig>()
		};

		if (source.lanes == null)
		{
			return clone;
		}

		foreach (LaneConfig lane in source.lanes)
		{
			LaneConfig laneClone = new LaneConfig
			{
				pigs = new List<PigLayoutData>()
			};

			if (lane?.pigs != null)
			{
				foreach (PigLayoutData pig in lane.pigs)
				{
					laneClone.pigs.Add(new PigLayoutData
					{
						colorName = pig.colorName,
						bullets = pig.bullets,
						isHidden = pig.isHidden,
						linkId = pig.linkId,
						pigLeft = pig.pigLeft != null ? new PigMarker { LaneIndex = pig.pigLeft.LaneIndex, index = pig.pigLeft.index } : null,
						pigRight = pig.pigRight != null ? new PigMarker { LaneIndex = pig.pigRight.LaneIndex, index = pig.pigRight.index } : null
					});
				}
			}

			clone.lanes.Add(laneClone);
		}

		return clone;
	}
}
