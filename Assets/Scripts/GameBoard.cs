using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {
	
	[SerializeField]
	GameTile tilePrefab = default;
	[SerializeField]
	Transform ground = default;
	Queue<GameTile> searchFrontier = new Queue<GameTile>();
	Vector2Int size;
	GameTile[] tiles;
	GameTileContentFactory contentFactory;
	bool showGrid, showPaths;
	[SerializeField]
	Texture2D gridTexture = default;
	public bool ShowGrid {
		get => showGrid;
		set {
			showGrid = value;
			Material m = ground.GetComponent<MeshRenderer>().material;
			if (showGrid) {
				m.mainTexture = gridTexture;
				m.SetTextureScale("_MainTex", size);
			}
			else {
				m.mainTexture = null;
			}
		}
	}
	public bool ShowPaths {
		get => showPaths;
		set {
			showPaths = value;
			if (showPaths) {
				foreach (GameTile tile in tiles) {
					tile.ShowPath();
				}
			}
			else {
				foreach (GameTile tile in tiles) {
					tile.HidePath();
				}
			}
		}
	}
	public void Initialize (Vector2Int size, GameTileContentFactory contentFactory) {
		this.size = size;
		ground.localScale = new Vector3(size.x, size.y, 1f);
		this.contentFactory = contentFactory;
		Vector2 offset = new Vector2(
			(size.x - 1) * 0.5f, (size.y - 1) * 0.5f
		);
		tiles = new GameTile[size.x * size.y];

		for (int i = 0, y = 0; y < size.y; y++) {
			for (int x = 0; x < size.x; x++, i++) {
				GameTile tile = tiles[i] = Instantiate(tilePrefab);
				tile.IsAlternative = (x & 1) == 0;
				if ((y & 1) == 0) {
					tile.IsAlternative = !tile.IsAlternative;
				}
				tile.transform.SetParent(transform, false);
				tile.transform.localPosition = new Vector3(
					x - offset.x, 0f, y - offset.y
				);
				if (x > 0) {
					GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
				}
				if (y > 0) {
					GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
				}
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
			}
		}
		ToggleDestination(tiles[tiles.Length / 2]);
	}
	bool  FindPaths () {
		foreach (GameTile tile in tiles) {
			if (tile.Content.Type == GameTileContentType.Destination) {
				tile.BecomeDestination();
				searchFrontier.Enqueue(tile);
			}
			else {
				tile.ClearPath();
			}
		}
		if (searchFrontier.Count == 0) {
			return false;
		}
		while (searchFrontier.Count > 0) {
			GameTile tile = searchFrontier.Dequeue();

			if (tile != null)
			{
				if (tile.IsAlternative)
				{
					searchFrontier.Enqueue(tile.GrowPathNorth());
					searchFrontier.Enqueue(tile.GrowPathSouth());
					searchFrontier.Enqueue(tile.GrowPathEast());
					searchFrontier.Enqueue(tile.GrowPathWest());
				}
				else
				{
					searchFrontier.Enqueue(tile.GrowPathWest());
					searchFrontier.Enqueue(tile.GrowPathEast());
					searchFrontier.Enqueue(tile.GrowPathSouth());
					searchFrontier.Enqueue(tile.GrowPathNorth());
				}
			}
		}
		foreach (GameTile tile in tiles) {
			if (!tile.HasPath) {
				return false;
			}
		}
		if (showPaths) {
			foreach (GameTile tile in tiles) {
				tile.ShowPath();
			}
		}
		return true;
	}
	public GameTile GetTile (Ray ray) {
		if (Physics.Raycast(ray, out RaycastHit hit)) {
			int x = (int)(hit.point.x + size.x * 0.5f);
			int y = (int)(hit.point.z + size.y * 0.5f);
			if (x >= 0 && x < size.x && y >= 0 && y < size.y) {
				return tiles[x + y * size.x];
			}
		}

		return null;
	}
	public void ToggleDestination (GameTile tile) {
		if (tile.Content.Type == GameTileContentType.Destination) {
			tile.Content = contentFactory.Get(GameTileContentType.Empty);
			if (!FindPaths()) {
				tile.Content =
					contentFactory.Get(GameTileContentType.Destination);
				FindPaths();
			}
		}
		else if (tile.Content.Type == GameTileContentType.Empty)
		{
			tile.Content = contentFactory.Get(GameTileContentType.Destination);
			FindPaths();
		}
	}
	public void ToggleWall (GameTile tile) {
		if (tile.Content.Type == GameTileContentType.Wall) {
			tile.Content = contentFactory.Get(GameTileContentType.Empty);
			FindPaths();
		}
		else if (tile.Content.Type == GameTileContentType.Empty) {
			tile.Content = contentFactory.Get(GameTileContentType.Wall);
			if (!FindPaths()) {
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
				FindPaths();
			}
		}
	}
}