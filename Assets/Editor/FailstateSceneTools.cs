using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FailstateSceneTools
{
    private const string mainScenePath = "Assets/Scenes/MainScene.unity";

    [MenuItem("Tools/Failstate/Clean Main Scene Layout")]
    public static void CleanupMainSceneFromMenu()
    {
        CleanupActiveScene();
        SaveOpenScenesIfAllowed();
    }

    [MenuItem("Tools/Failstate/Regenerate Upper City Layer")]
    public static void RegenerateUpperCityLayerFromMenu()
    {
        EnsureUpperCityLayerGenerator(true);
        SaveOpenScenesIfAllowed();
    }

    [MenuItem("Tools/Failstate/Regenerate City Blockout")]
    public static void RegenerateCityBlockoutFromMenu()
    {
        CityBlockoutGenerator city = Object.FindFirstObjectByType<CityBlockoutGenerator>();

        if (city == null)
        {
            Debug.LogWarning("FailstateSceneTools: No CityBlockoutGenerator found.");
            return;
        }

        city.GenerateCityBlockout();
        CleanupActiveScene();
        SaveOpenScenesIfAllowed();
    }

    public static void CleanupMainSceneFromCommandLine()
    {
        EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);
        CleanupActiveScene();
        EditorSceneManager.SaveOpenScenes();
    }

    public static void CleanupActiveScene()
    {
        DeleteOldTestObjects();
        EnsureInfrastructureNetworkManager();
        EnsureFirstRunObjectiveManager();
        EnsureNetworkObjectiveUI();
        EnsureRobotSystemStatusHUD();
        EnsureSystemMessageUI();
        EnsureUpperCityLayerGenerator(false);
        EnsureBaseCampLayout();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    static void DeleteOldTestObjects()
    {
        DeleteIfExists("CoreHazardZone");
        DeleteIfExists("MobilityHazardZone");
        DeleteIfExists("PerceptionHazardZone");
        DeleteIfExists("MetalScrapPickup");
        DeleteIfExists("WiringPickup");
        DeleteIfExists("CoreFragmentPickup");
    }

    static void EnsureInfrastructureNetworkManager()
    {
        if (Object.FindFirstObjectByType<InfrastructureNetworkManager>() != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("InfrastructureNetwork");
        managerObject.AddComponent<InfrastructureNetworkManager>();
    }

    static void EnsureNetworkObjectiveUI()
    {
        if (Object.FindFirstObjectByType<NetworkObjectiveUI>() != null)
        {
            return;
        }

        GameObject uiObject = GameObject.Find("UIManager");

        if (uiObject == null)
        {
            uiObject = new GameObject("UIManager");
        }

        uiObject.AddComponent<NetworkObjectiveUI>();
    }

    static void EnsureFirstRunObjectiveManager()
    {
        if (Object.FindFirstObjectByType<FirstRunObjectiveManager>() != null)
        {
            return;
        }

        GameObject managerObject = GameObject.Find("GameManager");

        if (managerObject == null)
        {
            managerObject = new GameObject("GameManager");
        }

        managerObject.AddComponent<FirstRunObjectiveManager>();
    }

    static void EnsureRobotSystemStatusHUD()
    {
        if (Object.FindFirstObjectByType<RobotSystemStatusHUD>() != null)
        {
            return;
        }

        GameObject uiObject = GameObject.Find("UIManager");

        if (uiObject == null)
        {
            uiObject = new GameObject("UIManager");
        }

        uiObject.AddComponent<RobotSystemStatusHUD>();
    }

    static void EnsureSystemMessageUI()
    {
        if (Object.FindFirstObjectByType<SystemMessageUI>() != null)
        {
            return;
        }

        GameObject uiObject = GameObject.Find("UIManager");

        if (uiObject == null)
        {
            uiObject = new GameObject("UIManager");
        }

        uiObject.AddComponent<SystemMessageUI>();
    }

    static void EnsureBaseCampLayout()
    {
        BaseCampLayoutManager layout = Object.FindFirstObjectByType<BaseCampLayoutManager>();

        if (layout == null)
        {
            GameObject layoutObject = GameObject.Find("GameManager");

            if (layoutObject == null)
            {
                layoutObject = new GameObject("BaseCampLayoutManager");
            }

            layout = layoutObject.AddComponent<BaseCampLayoutManager>();
        }

        layout.city = Object.FindFirstObjectByType<CityBlockoutGenerator>();
        layout.baseCampZone = Object.FindFirstObjectByType<BaseCampZone>();
        layout.workbenchStation = Object.FindFirstObjectByType<WorkbenchStation>();
        layout.rechargeStation = Object.FindFirstObjectByType<RechargeStation>();
        layout.cleanupOldTestObjectsOnStart = false;
        layout.ApplyLayout();
    }

    static void EnsureUpperCityLayerGenerator(bool regenerate)
    {
        UpperCityLayerGenerator upperLayer = Object.FindFirstObjectByType<UpperCityLayerGenerator>();

        if (upperLayer == null)
        {
            GameObject upperLayerObject = new GameObject("UpperCityLayerGenerator");
            upperLayer = upperLayerObject.AddComponent<UpperCityLayerGenerator>();
        }

        upperLayer.city = Object.FindFirstObjectByType<CityBlockoutGenerator>();

        if (regenerate)
        {
            upperLayer.GenerateUpperCityLayer();
        }
    }

    static void DeleteIfExists(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);

        if (obj != null)
        {
            Object.DestroyImmediate(obj);
        }
    }

    static void SaveOpenScenesIfAllowed()
    {
        if (EditorApplication.isPlaying)
        {
            return;
        }

        EditorSceneManager.SaveOpenScenes();
    }
}
