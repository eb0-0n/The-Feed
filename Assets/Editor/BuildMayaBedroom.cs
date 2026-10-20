using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.IO;

public static class BuildMayaBedroom
{
    private const string RootName = "MAYA BEDROOM - Scene 1";
    private const string MaterialFolder = "Assets/MayaBedroom/Materials";

    [MenuItem("The Feed/Build Maya Bedroom")]
    public static void BuildOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || GameObject.Find(RootName) != null) return;

        EnsureFolder("Assets/MayaBedroom");
        EnsureFolder(MaterialFolder);
        EnsureFolder("Assets/MayaBedroom/ConvertedMaterials");

        GameObject root = new GameObject(RootName);
        Material wall = Mat("Warm Ivory", new Color(0.92f, 0.89f, 0.84f), 0.10f);
        Material wood = Mat("Warm Wood", new Color(0.24f, 0.105f, 0.055f), 0.18f);
        Material teal = Mat("Feed Teal", new Color(0.08f, 0.34f, 0.36f), 0.25f);
        Material navy = Mat("Blanket Navy", new Color(0.07f, 0.14f, 0.24f), 0.25f);
        Material white = Mat("Soft White", new Color(0.94f, 0.91f, 0.86f), 0.20f);
        Material bedding = Mat("Bedding Cream", new Color(0.88f, 0.87f, 0.83f), 0.24f);
        Material curtain = Mat("Curtain Linen", new Color(0.70f, 0.66f, 0.60f), 0.12f);
        Material mirror = Mat("Mirror Blue", new Color(0.34f, 0.50f, 0.58f), 0.75f);
        Material dark = Mat("Charcoal", new Color(0.035f, 0.045f, 0.055f), 0.35f);
        Material sky = Mat("Morning Window", new Color(0.42f, 0.68f, 0.82f), 0.10f, new Color(0.35f, 0.75f, 1f) * 1.8f);

        GameObject architecture = Group(root, "ARCHITECTURE - PRIMITIVES");
        Cube("Floor - Primitive", architecture.transform, new Vector3(0, -0.1f, 0), new Vector3(9, 0.2f, 7), wood);
        Cube("Back Wall - Primitive", architecture.transform, new Vector3(0, 1.6f, 3.5f), new Vector3(9, 3.2f, 0.18f), wall);
        Cube("Left Wall - Primitive", architecture.transform, new Vector3(-4.5f, 1.6f, 0), new Vector3(0.18f, 3.2f, 7), wall);
        Cube("Right Wall - Primitive", architecture.transform, new Vector3(4.5f, 1.6f, 0), new Vector3(0.18f, 3.2f, 7), wall);
        Cube("Front Wall Left - Primitive", architecture.transform, new Vector3(-0.95f, 1.6f, -3.5f), new Vector3(7.1f, 3.2f, 0.18f), wall);
        Cube("Front Wall Right - Primitive", architecture.transform, new Vector3(4.25f, 1.6f, -3.5f), new Vector3(0.5f, 3.2f, 0.18f), wall);
        Cube("Door Header - Primitive", architecture.transform, new Vector3(3.3f, 2.75f, -3.5f), new Vector3(1.4f, 0.9f, 0.18f), wall);

        GameObject window = Group(architecture, "Window - Primitive Frame");
        Cube("Morning Sky", window.transform, new Vector3(1.65f, 1.85f, 3.39f), new Vector3(3.1f, 1.55f, 0.05f), sky, false);
        Cube("Frame Top", window.transform, new Vector3(1.65f, 2.65f, 3.30f), new Vector3(3.3f, 0.12f, 0.12f), white, false);
        Cube("Frame Bottom", window.transform, new Vector3(1.65f, 1.05f, 3.30f), new Vector3(3.3f, 0.12f, 0.12f), white, false);
        Cube("Frame Left", window.transform, new Vector3(0.03f, 1.85f, 3.30f), new Vector3(0.12f, 1.7f, 0.12f), white, false);
        Cube("Frame Right", window.transform, new Vector3(3.27f, 1.85f, 3.30f), new Vector3(0.12f, 1.7f, 0.12f), white, false);
        Cube("Frame Center", window.transform, new Vector3(1.65f, 1.85f, 3.30f), new Vector3(0.09f, 1.7f, 0.12f), white, false);
        Cube("Curtain Left", window.transform, new Vector3(-0.08f, 1.82f, 3.20f), new Vector3(0.24f, 1.95f, 0.08f), curtain, false);
        Cube("Curtain Right", window.transform, new Vector3(3.38f, 1.82f, 3.20f), new Vector3(0.24f, 1.95f, 0.08f), curtain, false);

        GameObject required = Group(root, "FIVE REQUIRED 3D ELEMENTS");
        GameObject bed = Group(required, "1 REQUIRED - BED");
        GameObject bedAsset = AddAsset(bed.transform, "Apartment Kit Bed", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Furniture/Bedroom/Bed_Apt_01_01.prefab", Vector3.zero, Vector3.zero, bedding);
        PlaceWithBackEdge(bedAsset, -2.05f, 3.18f, 0f, 1.00f, new Vector3(0, 90, 0));
        RecolorNamedParts(bedAsset, bedding, "blanket", "comforter", "duvet", "sheet", "pillow", "mattress");
        Bounds bedBounds = WorldBounds(bedAsset);

        GameObject desk = Group(required, "2 REQUIRED - DESK");
        GameObject deskAsset = AddAsset(desk.transform, "Apartment Kit Computer Desk", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Furniture/Living Room/Table_Computer_01.prefab", Vector3.zero, Vector3.zero, wood);
        PlaceAgainstRightWall(deskAsset, 4.28f, 0.35f, 0f, 0.72f, new Vector3(0, -90, 0));
        Bounds deskBounds = WorldBounds(deskAsset);
        GameObject deskChair = AddAsset(desk.transform, "Apartment Kit Desk Chair", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Furniture/Living Room/Chair_Apt_01.prefab", Vector3.zero, Vector3.zero, teal);
        PlaceAtFloor(deskChair, 0f, deskBounds.center.z, 0f, 0.70f, new Vector3(0, 90, 0));
        Bounds chairBounds = WorldBounds(deskChair);
        deskChair.transform.position += new Vector3((deskBounds.min.x - 0.04f) - chairBounds.max.x, 0f, 0f);

        GameObject laptop = Group(desk, "Laptop - Primitive Schoolwork");
        Cube("Laptop Base", laptop.transform, new Vector3(deskBounds.center.x - 0.10f, deskBounds.max.y + 0.045f, deskBounds.center.z), new Vector3(0.42f, 0.05f, 0.52f), dark, false);
        Cube("Laptop Screen", laptop.transform, new Vector3(deskBounds.center.x + 0.10f, deskBounds.max.y + 0.28f, deskBounds.center.z), new Vector3(0.045f, 0.43f, 0.52f), dark, false);
        Cube("Laptop Display", laptop.transform, new Vector3(deskBounds.center.x + 0.072f, deskBounds.max.y + 0.28f, deskBounds.center.z), new Vector3(0.018f, 0.35f, 0.44f), teal, false);
        Cube("Book 1", desk.transform, new Vector3(deskBounds.center.x, deskBounds.max.y + 0.035f, deskBounds.center.z + 0.52f), new Vector3(0.34f, 0.05f, 0.28f), white, false);
        Cube("Book 2", desk.transform, new Vector3(deskBounds.center.x, deskBounds.max.y + 0.095f, deskBounds.center.z + 0.52f), new Vector3(0.31f, 0.05f, 0.25f), teal, false);
        Cylinder("Water Bottle", desk.transform, new Vector3(deskBounds.center.x, deskBounds.max.y + 0.20f, deskBounds.center.z - 0.50f), new Vector3(0.10f, 0.20f, 0.10f), teal);

        GameObject dresser = Group(required, "3 REQUIRED - DRESSER");
        GameObject dresserAsset = AddAsset(dresser.transform, "Apartment Kit Wardrobe", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Furniture/Bedroom/Dresser Closet_01.prefab", Vector3.zero, Vector3.zero, wood);
        PlaceAgainstLeftWall(dresserAsset, -4.28f, -1.95f, 0f, 0.72f, new Vector3(0, 90, 0));

        GameObject mirrorObject = Group(required, "4 REQUIRED - MIRROR");
        Cube("Mirror Glass", mirrorObject.transform, new Vector3(-4.38f, 1.72f, 0.55f), new Vector3(0.06f, 1.72f, 1.15f), mirror, false);
        Cube("Mirror Top", mirrorObject.transform, new Vector3(-4.31f, 2.60f, 0.55f), new Vector3(0.12f, 0.10f, 1.30f), wood, false);
        Cube("Mirror Bottom", mirrorObject.transform, new Vector3(-4.31f, 0.84f, 0.55f), new Vector3(0.12f, 0.10f, 1.30f), wood, false);
        Cube("Mirror Side A", mirrorObject.transform, new Vector3(-4.31f, 1.72f, -0.05f), new Vector3(0.12f, 1.75f, 0.10f), wood, false);
        Cube("Mirror Side B", mirrorObject.transform, new Vector3(-4.31f, 1.72f, 1.15f), new Vector3(0.12f, 1.75f, 0.10f), wood, false);

        GameObject door = Group(required, "5 REQUIRED - BEDROOM DOOR");
        AddAsset(door.transform, "Apartment Kit Door Frame", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Apt Build Kit/Interiors/Frames Windows Stairs & Doors/DoorFrame_Apt_01.prefab", new Vector3(3.30f, 0f, -3.39f), Vector3.zero, wood);
        Cube("Door Slab", door.transform, new Vector3(3.30f, 1.10f, -3.39f), new Vector3(1.34f, 2.20f, 0.12f), teal);
        Sphere("Door Handle", door.transform, new Vector3(2.80f, 1.08f, -3.29f), Vector3.one * 0.10f, mirror);

        GameObject details = Group(root, "STORY DETAILS + FREE APARTMENT KIT ASSETS");
        GameObject rug = AddAsset(details.transform, "Apartment Kit Rug", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Props/Art/Rug_Apt_01.prefab", Vector3.zero, Vector3.zero, teal);
        PlaceAtFloor(rug, bedBounds.center.x, bedBounds.center.z - 0.48f, 0.012f, 0.58f, Vector3.zero);

        GameObject leftTable = AddAsset(details.transform, "Left Bedside Table", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Furniture/Bedroom/Table_End_01.prefab", Vector3.zero, Vector3.zero, wood);
        PlaceAtFloor(leftTable, 0f, 0f, 0f, 0.86f, Vector3.zero);
        Bounds leftTableSize = WorldBounds(leftTable);
        PlaceAtFloor(leftTable, bedBounds.min.x - 0.20f - leftTableSize.extents.x, bedBounds.max.z - leftTableSize.extents.z - 0.05f, 0f, 0.86f, Vector3.zero);

        GameObject rightTable = AddAsset(details.transform, "Right Bedside Table", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Furniture/Bedroom/Table_End_01.prefab", Vector3.zero, Vector3.zero, wood);
        PlaceAtFloor(rightTable, 0f, 0f, 0f, 0.86f, Vector3.zero);
        Bounds rightTableSize = WorldBounds(rightTable);
        PlaceAtFloor(rightTable, bedBounds.max.x + 0.20f + rightTableSize.extents.x, bedBounds.max.z - rightTableSize.extents.z - 0.05f, 0f, 0.86f, Vector3.zero);

        Bounds leftTableBounds = WorldBounds(leftTable);
        Bounds rightTableBounds = WorldBounds(rightTable);
        GameObject leftLamp = AddAsset(details.transform, "Left Bedside Lamp", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Props/Lighting/Lamp_Table_Apt_01.prefab", Vector3.zero, Vector3.zero, white);
        PlaceOnSurface(leftLamp, leftTableBounds.center.x, leftTableBounds.center.z, leftTableBounds.max.y, 0.78f, Vector3.zero);
        GameObject rightLamp = AddAsset(details.transform, "Right Bedside Lamp", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Props/Lighting/Lamp_Table_Apt_01.prefab", Vector3.zero, Vector3.zero, white);
        PlaceOnSurface(rightLamp, rightTableBounds.center.x, rightTableBounds.center.z, rightTableBounds.max.y, 0.78f, Vector3.zero);

        GameObject hamper = AddAsset(details.transform, "Apartment Kit Hamper", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Furniture/Bathroom/Hamper_01.prefab", Vector3.zero, Vector3.zero, navy);
        PlaceAgainstRightWall(hamper, 4.18f, -1.65f, 0f, 0.72f, Vector3.zero);
        GameObject ceilingLamp = AddAsset(details.transform, "Apartment Kit Ceiling Light", "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Props/Lighting/Lamp_Ceiling_Apt_02.prefab", Vector3.zero, Vector3.zero, white);
        PlaceCeilingObject(ceilingLamp, -0.35f, 0.15f, 3.10f, 0.68f, Vector3.zero);

        GameObject backpack = Group(details, "Backpack - Primitive Detail");
        Cube("Bag", backpack.transform, new Vector3(2.35f, 0.35f, -2.55f), new Vector3(0.58f, 0.70f, 0.30f), navy);
        Cube("Pocket", backpack.transform, new Vector3(2.35f, 0.25f, -2.72f), new Vector3(0.42f, 0.30f, 0.08f), teal, false);
        Cube("Shoe Left", details.transform, new Vector3(-3.15f, 0.10f, -2.75f), new Vector3(0.28f, 0.18f, 0.58f), dark);
        Cube("Shoe Right", details.transform, new Vector3(-2.78f, 0.10f, -2.68f), new Vector3(0.28f, 0.18f, 0.58f), dark);
        GameObject phone = Cube("Phone - Primitive Detail", details.transform, new Vector3(leftTableBounds.center.x, leftTableBounds.max.y + 0.035f, leftTableBounds.center.z - 0.18f), new Vector3(0.28f, 0.035f, 0.15f), dark, false);
        phone.transform.rotation = Quaternion.Euler(0, 18, 0);

        GameObject deskArt = Group(details, "Desk Wall Art - Primitive Frames");
        Cube("Photo Frame A", deskArt.transform, new Vector3(4.38f, 2.02f, -0.05f), new Vector3(0.05f, 0.62f, 0.48f), wood, false);
        Cube("Photo A", deskArt.transform, new Vector3(4.35f, 2.02f, -0.05f), new Vector3(0.025f, 0.50f, 0.37f), teal, false);
        Cube("Photo Frame B", deskArt.transform, new Vector3(4.38f, 1.82f, 0.72f), new Vector3(0.05f, 0.48f, 0.38f), wood, false);
        Cube("Photo B", deskArt.transform, new Vector3(4.35f, 1.82f, 0.72f), new Vector3(0.025f, 0.37f, 0.28f), white, false);

        BuildFeed(root.transform, dark, teal, bedBounds.center.x);
        ValidateFurnitureSpacing(bedAsset, leftTable, rightTable, deskAsset, deskChair, dresserAsset);
        BuildLighting(root.transform);
        BuildCamera(root.transform);

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = root;
        Debug.Log("The Feed: Maya's bedroom was added without deleting existing scene objects.");
    }

    [MenuItem("The Feed/Rebuild With Apartment Kit")]
    public static void RebuildWithApartmentKit()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
        GameObject existing = GameObject.Find(RootName);
        if (existing != null) Object.DestroyImmediate(existing);
        BuildOnce();
        AssetDatabase.SaveAssets();
        Debug.Log("The Feed: Maya's bedroom rebuilt with Apartment Kit furniture.");
    }

    public static void RebuildAndCapture()
    {
        RebuildWithApartmentKit();
        CaptureSubmissionMedia();
    }

    [MenuItem("The Feed/Capture Submission Media")]
    public static void CaptureSubmissionMedia()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
        Camera camera = Camera.main;
        if (camera == null) camera = Object.FindFirstObjectByType<Camera>();
        if (camera == null) throw new System.InvalidOperationException("No camera found in SampleScene.");

        string output = "/Users/efabunmi/Documents/Codex/2026-09-15/i-x20/outputs";
        string frames = "/Users/efabunmi/Documents/Codex/2026-09-15/i-x20/work/walkthrough_frames";
        Directory.CreateDirectory(output);
        Directory.CreateDirectory(frames);

        Vector3[] screenshotPositions =
        {
            new Vector3(2.95f, 1.65f, -2.82f),
            new Vector3(-0.85f, 1.35f, 0.78f)
        };
        Vector3[] screenshotTargets =
        {
            new Vector3(-0.45f, 1.18f, 0.75f),
            new Vector3(3.55f, 1.30f, 1.15f)
        };
        Quaternion[] screenshotRotations = new Quaternion[screenshotPositions.Length];
        for (int i = 0; i < screenshotPositions.Length; i++) screenshotRotations[i] = Quaternion.LookRotation((screenshotTargets[i] - screenshotPositions[i]).normalized, Vector3.up);

        camera.transform.SetPositionAndRotation(screenshotPositions[0], screenshotRotations[0]);
        RenderPng(camera, Path.Combine(output, "TheFeed_Screenshot_1_Wide.png"), 1280, 720);
        camera.transform.SetPositionAndRotation(screenshotPositions[1], screenshotRotations[1]);
        RenderPng(camera, Path.Combine(output, "TheFeed_Screenshot_2_BedToDesk.png"), 1280, 720);

        Vector3[] tourPositions =
        {
            new Vector3(-2.05f, 1.14f, 1.30f),
            new Vector3(-0.35f, 1.42f, 0.72f),
            new Vector3(0.45f, 1.52f, -0.10f),
            new Vector3(0.80f, 1.55f, -1.05f)
        };
        Vector3[] tourTargets =
        {
            new Vector3(3.35f, 1.30f, 1.25f),
            new Vector3(4.05f, 1.15f, 0.35f),
            new Vector3(-4.30f, 1.65f, 0.55f),
            new Vector3(3.30f, 1.22f, -3.40f)
        };
        Quaternion[] tourRotations = new Quaternion[tourPositions.Length];
        for (int i = 0; i < tourPositions.Length; i++) tourRotations[i] = Quaternion.LookRotation((tourTargets[i] - tourPositions[i]).normalized, Vector3.up);

        const int fps = 15;
        const int framesPerSegment = 75;
        int frameNumber = 0;
        for (int segment = 0; segment < tourPositions.Length - 1; segment++)
        {
            for (int frame = 0; frame < framesPerSegment; frame++)
            {
                float t = Mathf.SmoothStep(0f, 1f, frame / (float)(framesPerSegment - 1));
                camera.transform.position = Vector3.Lerp(tourPositions[segment], tourPositions[segment + 1], t);
                camera.transform.rotation = Quaternion.Slerp(tourRotations[segment], tourRotations[segment + 1], t);
                RenderJpg(camera, Path.Combine(frames, $"frame_{frameNumber:0000}.jpg"), 960, 540);
                frameNumber++;
            }
        }

        File.WriteAllText(Path.Combine(frames, "fps.txt"), fps.ToString());
        Debug.Log($"The Feed submission media captured: 2 screenshots and {frameNumber} walkthrough frames.");
    }

    private static void RenderPng(Camera camera, string path, int width, int height)
    {
        Texture2D image = Render(camera, width, height);
        File.WriteAllBytes(path, image.EncodeToPNG());
        Object.DestroyImmediate(image);
    }

    private static void RenderJpg(Camera camera, string path, int width, int height)
    {
        Texture2D image = Render(camera, width, height);
        File.WriteAllBytes(path, image.EncodeToJPG(88));
        Object.DestroyImmediate(image);
    }

    private static Texture2D Render(Camera camera, int width, int height)
    {
        RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;
        camera.targetTexture = target;
        RenderTexture.active = target;
        camera.Render();
        Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();
        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        target.Release();
        Object.DestroyImmediate(target);
        return image;
    }

    private static void BuildFeed(Transform root, Material panel, Material accent, float bedCenterX)
    {
        GameObject feed = Group(root.gameObject, "THE FEED - Morning Display");
        Cube("Feed Panel", feed.transform, new Vector3(bedCenterX, 2.27f, 3.29f), new Vector3(2.20f, 0.82f, 0.06f), panel, false);
        Cube("Feed Accent", feed.transform, new Vector3(bedCenterX - 1.02f, 2.27f, 3.23f), new Vector3(0.06f, 0.68f, 0.035f), accent, false);
        GameObject label = new GameObject("Feed Text");
        label.transform.SetParent(feed.transform);
        label.transform.position = new Vector3(bedCenterX - 0.91f, 2.56f, 3.20f);
        label.transform.rotation = Quaternion.identity;
        TextMesh text = label.AddComponent<TextMesh>();
        text.text = "8:42 AM\nGood morning, Maya.\nFirst class: 10:10 AM\nWeather: 78 F\n2 unread messages";
        text.fontSize = 80;
        text.characterSize = 0.017f;
        text.anchor = TextAnchor.UpperLeft;
        text.color = new Color(0.78f, 0.96f, 1f);
    }

    private static void BuildLighting(Transform root)
    {
        Light sun = null;
        foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (light.type == LightType.Directional) { sun = light; break; }
        if (sun == null)
        {
            GameObject sunObject = new GameObject("Morning Sun - Directional Light");
            sunObject.transform.SetParent(root);
            sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
        }
        sun.name = "Morning Sun - Directional Light";
        sun.transform.rotation = Quaternion.Euler(38, -42, 0);
        sun.color = new Color(1f, 0.82f, 0.64f);
        sun.intensity = 0.82f;
        sun.shadows = LightShadows.Soft;

        GameObject fill = new GameObject("Window Fill - Point Light");
        fill.transform.SetParent(root);
        fill.transform.position = new Vector3(1.5f, 2.1f, 2.7f);
        Light point = fill.AddComponent<Light>();
        point.type = LightType.Point;
        point.color = new Color(0.62f, 0.82f, 1f);
        point.range = 9f;
        point.intensity = 5.2f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.41f, 0.39f, 0.39f);
    }

    private static void BuildCamera(Transform root)
    {
        Camera camera = Camera.main;
        if (camera == null) camera = Object.FindFirstObjectByType<Camera>();
        if (camera == null)
        {
            GameObject created = new GameObject("Main Camera");
            created.tag = "MainCamera";
            camera = created.AddComponent<Camera>();
            created.AddComponent<AudioListener>();
        }
        camera.name = "Main Camera - Auto Walkthrough";
        camera.fieldOfView = 60f;
        camera.nearClipPlane = 0.08f;

        Vector3[] positions =
        {
            new Vector3(-2.05f, 1.14f, 1.30f),
            new Vector3(-0.35f, 1.42f, 0.72f),
            new Vector3(0.45f, 1.52f, -0.10f),
            new Vector3(0.80f, 1.55f, -1.05f)
        };
        Vector3[] targets =
        {
            new Vector3(3.35f, 1.30f, 1.25f),
            new Vector3(4.05f, 1.15f, 0.35f),
            new Vector3(-4.30f, 1.65f, 0.55f),
            new Vector3(3.30f, 1.22f, -3.40f)
        };
        Transform[] points = new Transform[positions.Length];
        GameObject pointRoot = Group(root.gameObject, "CAMERA WAYPOINTS - Bed to Desk to Mirror to Door");
        for (int i = 0; i < positions.Length; i++)
        {
            string[] names = { "1 - Wake at Bed", "2 - Approach Desk", "3 - Look at Mirror", "4 - Head to Door" };
            GameObject point = new GameObject(names[i]);
            point.transform.SetParent(pointRoot.transform);
            point.transform.position = positions[i];
            point.transform.rotation = Quaternion.LookRotation((targets[i] - positions[i]).normalized, Vector3.up);
            points[i] = point.transform;
        }
        BedroomCameraTour tour = camera.GetComponent<BedroomCameraTour>();
        if (tour == null) tour = camera.gameObject.AddComponent<BedroomCameraTour>();
        tour.waypoints = points;
        tour.secondsPerSegment = 5.5f;
        tour.playOnStart = true;
        camera.transform.SetPositionAndRotation(points[0].position, points[0].rotation);
    }

    private static Bounds WorldBounds(GameObject item)
    {
        if (item == null) return new Bounds(Vector3.zero, Vector3.zero);
        Renderer[] renderers = item.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(item.transform.position, Vector3.zero);
        Bounds result = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) result.Encapsulate(renderers[i].bounds);
        return result;
    }

    private static void PlaceAtFloor(GameObject item, float centerX, float centerZ, float floorY, float scale, Vector3 rotation)
    {
        if (item == null) return;
        item.transform.position = Vector3.zero;
        item.transform.rotation = Quaternion.Euler(rotation);
        item.transform.localScale = Vector3.one * scale;
        Bounds bounds = WorldBounds(item);
        item.transform.position += new Vector3(centerX - bounds.center.x, floorY - bounds.min.y, centerZ - bounds.center.z);
    }

    private static void PlaceWithBackEdge(GameObject item, float centerX, float backEdgeZ, float floorY, float scale, Vector3 rotation)
    {
        PlaceAtFloor(item, centerX, 0f, floorY, scale, rotation);
        if (item == null) return;
        Bounds bounds = WorldBounds(item);
        item.transform.position += new Vector3(0f, 0f, backEdgeZ - bounds.max.z);
    }

    private static void PlaceAgainstRightWall(GameObject item, float rightEdgeX, float centerZ, float floorY, float scale, Vector3 rotation)
    {
        PlaceAtFloor(item, 0f, centerZ, floorY, scale, rotation);
        if (item == null) return;
        Bounds bounds = WorldBounds(item);
        item.transform.position += new Vector3(rightEdgeX - bounds.max.x, 0f, 0f);
    }

    private static void PlaceAgainstLeftWall(GameObject item, float leftEdgeX, float centerZ, float floorY, float scale, Vector3 rotation)
    {
        PlaceAtFloor(item, 0f, centerZ, floorY, scale, rotation);
        if (item == null) return;
        Bounds bounds = WorldBounds(item);
        item.transform.position += new Vector3(leftEdgeX - bounds.min.x, 0f, 0f);
    }

    private static void PlaceAgainstFrontWall(GameObject item, float centerX, float frontEdgeZ, float floorY, float scale, Vector3 rotation)
    {
        PlaceAtFloor(item, centerX, 0f, floorY, scale, rotation);
        if (item == null) return;
        Bounds bounds = WorldBounds(item);
        item.transform.position += new Vector3(0f, 0f, frontEdgeZ - bounds.min.z);
    }

    private static void PlaceOnSurface(GameObject item, float centerX, float centerZ, float surfaceY, float scale, Vector3 rotation)
    {
        PlaceAtFloor(item, centerX, centerZ, surfaceY + 0.015f, scale, rotation);
    }

    private static void PlaceCeilingObject(GameObject item, float centerX, float centerZ, float ceilingY, float scale, Vector3 rotation)
    {
        if (item == null) return;
        item.transform.position = Vector3.zero;
        item.transform.rotation = Quaternion.Euler(rotation);
        item.transform.localScale = Vector3.one * scale;
        Bounds bounds = WorldBounds(item);
        item.transform.position += new Vector3(centerX - bounds.center.x, ceilingY - bounds.max.y, centerZ - bounds.center.z);
    }

    private static void RecolorNamedParts(GameObject item, Material replacement, params string[] keywords)
    {
        if (item == null) return;
        foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>())
        {
            Material[] materials = renderer.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                string searchable = (renderer.name + " " + (materials[i] == null ? "" : materials[i].name)).ToLowerInvariant();
                foreach (string keyword in keywords)
                {
                    if (!searchable.Contains(keyword)) continue;
                    materials[i] = replacement;
                    changed = true;
                    break;
                }
            }
            if (changed) renderer.sharedMaterials = materials;
        }
    }

    private static void ValidateFurnitureSpacing(params GameObject[] items)
    {
        int overlaps = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null) continue;
            Bounds a = WorldBounds(items[i]);
            a.Expand(new Vector3(-0.08f, -0.08f, -0.08f));
            for (int j = i + 1; j < items.Length; j++)
            {
                if (items[j] == null) continue;
                Bounds b = WorldBounds(items[j]);
                b.Expand(new Vector3(-0.08f, -0.08f, -0.08f));
                if (!a.Intersects(b)) continue;
                overlaps++;
                Debug.LogWarning("Layout overlap check: " + items[i].name + " intersects " + items[j].name);
            }
        }
        Debug.Log(overlaps == 0 ? "The Feed layout audit: no furniture overlaps detected." : "The Feed layout audit found " + overlaps + " overlap(s).");
    }

    private static GameObject AddAsset(Transform parent, string name, string path, Vector3 position, Vector3 rotation, Material material)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogWarning("Optional Apartment Kit asset missing: " + path); return null; }
        GameObject item = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.rotation = Quaternion.Euler(rotation);
        foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>())
        {
            Material[] originals = renderer.sharedMaterials;
            Material[] converted = new Material[originals.Length];
            for (int i = 0; i < originals.Length; i++) converted[i] = ConvertToUrp(originals[i], material);
            renderer.sharedMaterials = converted;
        }
        return item;
    }

    private static Material ConvertToUrp(Material source, Material fallback)
    {
        if (source == null) return fallback;
        if (source.shader != null && source.shader.name.Contains("Universal Render Pipeline")) return source;

        string sourcePath = AssetDatabase.GetAssetPath(source);
        string guid = string.IsNullOrEmpty(sourcePath) ? "embedded" : AssetDatabase.AssetPathToGUID(sourcePath);
        string assetPath = "Assets/MayaBedroom/ConvertedMaterials/" + guid + "_" + SafeName(source.name) + ".mat";
        Material result = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (result != null) return result;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) return fallback;
        result = new Material(shader) { name = "URP " + source.name };

        if (source.HasProperty("_Color")) result.SetColor("_BaseColor", source.GetColor("_Color"));
        if (source.HasProperty("_MainTex"))
        {
            Texture texture = source.GetTexture("_MainTex");
            result.SetTexture("_BaseMap", texture);
            result.SetTextureScale("_BaseMap", source.GetTextureScale("_MainTex"));
            result.SetTextureOffset("_BaseMap", source.GetTextureOffset("_MainTex"));
        }
        if (source.HasProperty("_BumpMap"))
        {
            result.SetTexture("_BumpMap", source.GetTexture("_BumpMap"));
            result.EnableKeyword("_NORMALMAP");
        }
        if (source.HasProperty("_Metallic")) result.SetFloat("_Metallic", source.GetFloat("_Metallic"));
        if (source.HasProperty("_Glossiness")) result.SetFloat("_Smoothness", source.GetFloat("_Glossiness"));
        if (source.IsKeywordEnabled("_EMISSION") && source.HasProperty("_EmissionColor"))
        {
            result.EnableKeyword("_EMISSION");
            result.SetColor("_EmissionColor", source.GetColor("_EmissionColor"));
        }
        AssetDatabase.CreateAsset(result, assetPath);
        return result;
    }

    private static string SafeName(string value)
    {
        char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
            if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '-' && chars[i] != '_') chars[i] = '_';
        return new string(chars);
    }

    private static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collider = true)
    {
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.localScale = scale;
        item.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static GameObject Sphere(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.localScale = scale;
        item.GetComponent<Renderer>().sharedMaterial = material;
        return item;
    }

    private static GameObject Cylinder(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.localScale = scale;
        item.GetComponent<Renderer>().sharedMaterial = material;
        return item;
    }

    private static GameObject Group(GameObject parent, string name)
    {
        GameObject item = new GameObject(name);
        item.transform.SetParent(parent.transform);
        return item;
    }

    private static Material Mat(string name, Color color, float smoothness, Color? emission = null)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        material.SetFloat("_Smoothness", smoothness);
        if (emission.HasValue)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission.Value);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int split = path.LastIndexOf('/');
        string parent = path.Substring(0, split);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, path.Substring(split + 1));
    }
}
