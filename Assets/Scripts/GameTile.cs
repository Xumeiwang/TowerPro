using UnityEngine;
public enum GameTileContentType {
	Empty,			//空格
	Destination,	//目的地
	Wall,			//墙
}
public class GameTile : MonoBehaviour {
	
    [SerializeField]
    Transform arrow = default;
    GameTile north, east, south, west, nextOnPath;
	
    public bool IsAlternative { get; set; }
    int distance;
    static Quaternion
	    northRotation = Quaternion.Euler(90f, 0f, 0f),
	    eastRotation = Quaternion.Euler(90f, 90f, 0f),
	    southRotation = Quaternion.Euler(90f, 180f, 0f),
	    westRotation = Quaternion.Euler(90f, 270f, 0f);
    public bool HasPath => distance != int.MaxValue;
    GameTileContent content;

    public GameTileContent Content {
	    get => content;
	    set {
		    Debug.Assert(value != null, "Null assigned to content!");
		    if (content != null) {
			    content.Recycle();
		    }
		    content = value;
		    content.transform.localPosition = transform.localPosition;
	    }
    }
    public void ClearPath () {
	    distance = int.MaxValue;
	    nextOnPath = null;
    }
    
    public void BecomeDestination () {
    		distance = 0;
    		nextOnPath = null;
    	}
	public static void MakeEastWestNeighbors (GameTile east, GameTile west) {
		Debug.Assert(
        			west.east == null && east.west == null, "Redefined neighbors!"
        		);
		west.east = east;
		east.west = west;
	}
	public static void MakeNorthSouthNeighbors (GameTile north, GameTile south) {
		Debug.Assert(
			south.north == null && north.south == null, "Redefined neighbors!"
		);
		south.north = north;
		north.south = south;
	}
	public void HidePath () {
		arrow.gameObject.SetActive(false);
	}
	GameTile GrowPathTo (GameTile neighbor) {
		if (!HasPath || neighbor == null || neighbor.HasPath) {
			return null;
		}
		neighbor.distance = distance + 1;
		neighbor.nextOnPath = this;
		return
			neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null;
	}
	public GameTile GrowPathNorth () => GrowPathTo(north);

	public GameTile GrowPathEast () => GrowPathTo(east);

	public GameTile GrowPathSouth () => GrowPathTo(south);

	public GameTile GrowPathWest () => GrowPathTo(west);
	public void ShowPath () {
		if (distance == 0) {
			arrow.gameObject.SetActive(false);
			return;
		}
		arrow.gameObject.SetActive(true);
		arrow.localRotation =
			nextOnPath == north ? northRotation :
			nextOnPath == east ? eastRotation :
			nextOnPath == south ? southRotation :
			westRotation;
	}
}