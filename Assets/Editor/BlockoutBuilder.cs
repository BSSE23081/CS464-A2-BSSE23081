#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Put this file in Assets/Editor/. Menu: CS464 > Build Five Blockouts.
// Scenes are written to Assets/Scenes/Blockouts/ so nothing of yours is overwritten.
public static class BlockoutBuilder
{
    const string OutDir = "Assets/Scenes/Blockouts";
    const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";
    const string CameraPrefabPath = "Assets/Prefabs/CameraRig.prefab";

    static int groundLayer;
    static Material groundMat, goalMat;

    // A platform: x range, z range, height of its top surface. 1 m thick.
    struct Plat
    {
        public float x0, x1, z0, z1, top;
        public Plat(float x0, float x1, float z0, float z1, float top)
        { this.x0 = x0; this.x1 = x1; this.z0 = z0; this.z1 = z1; this.top = top; }
    }

    [MenuItem("CS464/Build Five Blockouts")]
    static void BuildAll()
    {
        groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0)
        {
            EditorUtility.DisplayDialog("Missing layer",
                "Add a layer called Ground first (Project Settings > Tags and Layers).", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder(OutDir)) AssetDatabase.CreateFolder("Assets/Scenes", "Blockouts");
        groundMat = MakeMat("GroundGrey", new Color(0.6f, 0.6f, 0.6f));
        goalMat = MakeMat("GoalGreen", new Color(0.2f, 0.85f, 0.3f));

        // Level 01: walk, then two gaps (2 m, then 3 m).
        Build("Level01", new[] {
            new Plat(-3, 3,  0, 10, 0),
            new Plat(-3, 3, 12, 20, 0),
            new Plat(-3, 3, 23, 33, 0) });

        // Level 02: climb two 1.2 m ledges.
        Build("Level02", new[] {
            new Plat(-3, 3,  0,  8, 0),
            new Plat(-3, 3,  8, 12, 1.2f),
            new Plat(-3, 3, 12, 16, 2.4f),
            new Plat(-3, 3, 16, 24, 2.4f) });

        // Level 03: run-off jumps over 3 m gaps from narrow platforms (coyote time).
        Build("Level03", new[] {
            new Plat(-3, 3,  0, 10, 0),
            new Plat(-2, 2, 13, 18, 0),
            new Plat(-3, 3, 21, 31, 0) });

        // Level 04: gaps that also change height (up 1.2, down 1.2, up 1.2).
        Build("Level04", new[] {
            new Plat(-3, 3,  0,  8, 0),
            new Plat(-3, 3, 10.5f, 14.5f, 1.2f),
            new Plat(-3, 3, 17, 21, 0),
            new Plat(-3, 3, 24, 32, 1.2f) });

        // Level 05: an L-shaped route, to test camera-relative movement.
        Build("Level05", new[] {
            new Plat(-3, 3,   0, 8, 0),
            new Plat(-3, 3,  10.5f, 16, 0),
            new Plat(-0.5f, 5.5f, 16, 22, 0),
            new Plat( 8, 14, 16, 22, 1.2f),
            new Plat(17, 25, 16, 22, 1.2f) });

        AssetDatabase.SaveAssets();
        Debug.Log("CS464: five blockouts written to " + OutDir);
    }

    static void Build(string name, Plat[] plats)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        for (int i = 0; i < plats.Length; i++)
        {
            var p = plats[i];
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Platform" + (i + 1).ToString("00");
            go.layer = groundLayer;
            go.transform.position = new Vector3((p.x0 + p.x1) / 2f, p.top - 0.5f, (p.z0 + p.z1) / 2f);
            go.transform.localScale = new Vector3(p.x1 - p.x0, 1f, p.z1 - p.z0);
            go.GetComponent<Renderer>().sharedMaterial = groundMat;
        }

        // Spawn: first platform, near its start. Capsule centre sits 1 m above the surface.
        var first = plats[0];
        Vector3 spawn = new Vector3((first.x0 + first.x1) / 2f, first.top + 1.05f, first.z0 + 2f);
        var spawnMarker = new GameObject("PlayerSpawn");
        spawnMarker.transform.position = spawn;

        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (playerPrefab != null)
        {
            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = spawn;
        }
        else Debug.LogWarning("No Player prefab at " + PlayerPrefabPath + ", only the PlayerSpawn marker was added.");

        // Goal: a green marker on the last platform, trigger collider.
        var last = plats[plats.Length - 1];
        var goal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goal.name = "Goal";
        goal.transform.position = new Vector3((last.x0 + last.x1) / 2f, last.top + 1f, last.z1 - 2f);
        goal.transform.localScale = new Vector3(1.5f, 2f, 1.5f);
        goal.GetComponent<Collider>().isTrigger = true;
        goal.GetComponent<Renderer>().sharedMaterial = goalMat;

        // Camera: prefab if you have one, otherwise a plain camera with CameraFollow.
        var camPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CameraPrefabPath);
        if (camPrefab != null)
        {
            PrefabUtility.InstantiatePrefab(camPrefab);
        }
        else
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<CameraFollow>();
            camGo.transform.position = spawn + new Vector3(0f, 5f, -7f);
        }

        EditorSceneManager.SaveScene(scene, OutDir + "/" + name + ".unity");
    }

    static Material MakeMat(string name, Color color)
    {
        string path = "Assets/Scenes/Blockouts/" + name + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        var m = new Material(sh) { color = color };
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
#endif
