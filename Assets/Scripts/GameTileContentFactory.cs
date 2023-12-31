using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class GameTileContentFactory : ScriptableObject {
    Scene contentScene;
    [SerializeField]
    GameTileContent destinationPrefab = default;

    [SerializeField]
    GameTileContent emptyPrefab = default;
    
    [SerializeField]
    GameTileContent wallPrefab = default;
    public void Reclaim (GameTileContent content) {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }
    
    public GameTileContent Get (GameTileContentType type) {
        switch (type) {
            case GameTileContentType.Destination: return Get(destinationPrefab);
            case GameTileContentType.Empty: return Get(emptyPrefab);
            case GameTileContentType.Wall: return Get(wallPrefab);
        }
        Debug.Assert(false, "Unsupported type: " + type);
        return null;
    }
    GameTileContent Get (GameTileContent prefab) {
        GameTileContent instance = Instantiate(prefab);
        instance.OriginFactory = this;
        MoveToFactoryScene(instance.gameObject);
        return instance;
    }
    void MoveToFactoryScene (GameObject o) {
        if (!contentScene.isLoaded) {
            if (Application.isEditor) {
                contentScene = SceneManager.GetSceneByName(name);
                if (!contentScene.isLoaded) {
                    contentScene = SceneManager.CreateScene(name);
                }
            }
            else {
                contentScene = SceneManager.CreateScene(name);
            }
        }
        SceneManager.MoveGameObjectToScene(o, contentScene);
    }
}