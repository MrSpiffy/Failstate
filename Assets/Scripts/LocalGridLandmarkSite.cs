using System.Collections.Generic;
using UnityEngine;

public class LocalGridLandmarkSite : MonoBehaviour
{
    public InfrastructureNode node;
    public float poweredLightIntensity = 2.2f;
    public float poweredLightRange = 10f;

    private readonly List<Renderer> poweredRenderers = new List<Renderer>();
    private readonly List<Light> poweredLights = new List<Light>();
    private bool lastPoweredState = false;
    private Color poweredColor;

    public void Configure(InfrastructureNode infrastructureNode)
    {
        node = infrastructureNode;
        poweredColor = GetPoweredColor();
        BuildSite();
        ApplyPoweredState(node != null && node.restored);
    }

    void Update()
    {
        bool powered = node != null && node.restored;

        if (powered != lastPoweredState)
        {
            ApplyPoweredState(powered);
        }
    }

    void BuildSite()
    {
        Color deckColor = new Color(0.12f, 0.16f, 0.18f, 1f);
        Color structureColor = new Color(0.2f, 0.24f, 0.25f, 1f);

        CreatePrimitive(PrimitiveType.Cube, "Site_Deck", new Vector3(0f, 0.055f, 0f), new Vector3(15.5f, 0.11f, 15.5f), deckColor);
        CreatePrimitive(PrimitiveType.Cube, "Site_PerimeterNorth", new Vector3(0f, 0.12f, 7.45f), new Vector3(15.4f, 0.18f, 0.14f), structureColor);
        CreatePrimitive(PrimitiveType.Cube, "Site_PerimeterWest", new Vector3(-7.45f, 0.12f, 0f), new Vector3(0.14f, 0.18f, 15.4f), structureColor);
        CreateLabel(GetSiteTitle(), new Vector3(0f, 3.65f, 6.65f), new Color(0.7f, 0.82f, 0.82f, 1f));

        switch (node != null ? node.nodeType : InfrastructureNodeType.SignalRelay)
        {
            case InfrastructureNodeType.PowerJunction:
                BuildPowerSite();
                break;

            case InfrastructureNodeType.TransitLift:
                BuildTransitSite();
                break;

            default:
                BuildSignalSite();
                break;
        }

        BuildFixture(new Vector3(-6.1f, 0f, 5.9f));
        BuildFixture(new Vector3(6.1f, 0f, 5.9f));
    }

    void BuildSignalSite()
    {
        Color equipment = new Color(0.12f, 0.27f, 0.3f, 1f);
        CreatePrimitive(PrimitiveType.Cube, "Signal_ServiceCabinet", new Vector3(-3.45f, 0.7f, -3.6f), new Vector3(1.8f, 1.4f, 0.72f), equipment);
        CreatePrimitive(PrimitiveType.Cylinder, "Signal_Mast", new Vector3(3.25f, 2.1f, -3.6f), new Vector3(0.2f, 2.1f, 0.2f), equipment);
        CreatePrimitive(PrimitiveType.Cube, "Signal_ArrayBar", new Vector3(3.25f, 3.35f, -3.6f), new Vector3(1.6f, 0.12f, 0.12f), equipment);
        AddPoweredPanel(new Vector3(-3.45f, 0.8f, -3.2f), new Vector3(0.9f, 0.2f, 0.04f));
    }

    void BuildPowerSite()
    {
        Color equipment = new Color(0.28f, 0.22f, 0.13f, 1f);
        CreatePrimitive(PrimitiveType.Cube, "Power_TransformerLeft", new Vector3(-3.45f, 0.75f, -3.25f), new Vector3(1.4f, 1.5f, 1.1f), equipment);
        CreatePrimitive(PrimitiveType.Cube, "Power_TransformerRight", new Vector3(3.45f, 0.75f, -3.25f), new Vector3(1.4f, 1.5f, 1.1f), equipment);
        CreatePrimitive(PrimitiveType.Cube, "Power_Conduit", new Vector3(0f, 0.14f, -3.25f), new Vector3(5.4f, 0.18f, 0.26f), equipment);
        AddPoweredPanel(new Vector3(-3.45f, 1.08f, -2.67f), new Vector3(0.72f, 0.16f, 0.04f));
        AddPoweredPanel(new Vector3(3.45f, 1.08f, -2.67f), new Vector3(0.72f, 0.16f, 0.04f));
    }

    void BuildTransitSite()
    {
        Color equipment = new Color(0.25f, 0.19f, 0.31f, 1f);
        CreatePrimitive(PrimitiveType.Cube, "Transit_LeftGuide", new Vector3(-3.6f, 1.5f, -3.5f), new Vector3(0.48f, 3f, 0.54f), equipment);
        CreatePrimitive(PrimitiveType.Cube, "Transit_RightGuide", new Vector3(3.6f, 1.5f, -3.5f), new Vector3(0.48f, 3f, 0.54f), equipment);
        CreatePrimitive(PrimitiveType.Cube, "Transit_Header", new Vector3(0f, 2.9f, -3.5f), new Vector3(7.7f, 0.32f, 0.54f), equipment);
        CreatePrimitive(PrimitiveType.Cube, "Transit_Threshold", new Vector3(0f, 0.12f, -2.1f), new Vector3(7f, 0.12f, 1.5f), equipment);
        AddPoweredPanel(new Vector3(0f, 2.9f, -3.2f), new Vector3(2.8f, 0.12f, 0.04f));
    }

    void BuildFixture(Vector3 localPosition)
    {
        Color postColor = new Color(0.2f, 0.24f, 0.25f, 1f);
        CreatePrimitive(PrimitiveType.Cylinder, "Fixture_Post", localPosition + new Vector3(0f, 1.35f, 0f), new Vector3(0.07f, 1.35f, 0.07f), postColor);
        GameObject lamp = CreatePrimitive(PrimitiveType.Cube, "Fixture_DeadLamp", localPosition + new Vector3(0f, 2.7f, 0f), new Vector3(0.42f, 0.14f, 0.3f), new Color(0.07f, 0.09f, 0.1f, 1f));
        poweredRenderers.Add(lamp.GetComponent<Renderer>());

        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = poweredLightRange;
        light.intensity = poweredLightIntensity;
        light.color = poweredColor;
        light.enabled = false;
        poweredLights.Add(light);
    }

    void AddPoweredPanel(Vector3 localPosition, Vector3 scale)
    {
        GameObject panel = CreatePrimitive(PrimitiveType.Cube, "Site_DormantIndicator", localPosition, scale, new Color(0.06f, 0.08f, 0.09f, 1f));
        poweredRenderers.Add(panel.GetComponent<Renderer>());
    }

    void ApplyPoweredState(bool powered)
    {
        lastPoweredState = powered;
        Color presentationColor = powered ? poweredColor : new Color(0.06f, 0.08f, 0.09f, 1f);

        for (int i = 0; i < poweredRenderers.Count; i++)
        {
            if (poweredRenderers[i] != null)
            {
                ApplyRendererColor(poweredRenderers[i], presentationColor);
            }
        }

        for (int i = 0; i < poweredLights.Count; i++)
        {
            if (poweredLights[i] != null)
            {
                poweredLights[i].enabled = powered;
            }
        }
    }

    GameObject CreatePrimitive(PrimitiveType type, string objectName, Vector3 localPosition, Vector3 scale, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = objectName;
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = scale;

        Collider collider = obj.GetComponent<Collider>();

        if (collider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(collider);
            }
            else
            {
                DestroyImmediate(collider);
            }
        }

        Renderer renderer = obj.GetComponent<Renderer>();

        if (renderer != null)
        {
            ApplyRendererColor(renderer, color);
        }

        return obj;
    }

    void CreateLabel(string text, Vector3 localPosition, Color color)
    {
        GameObject labelObject = new GameObject("Site_Label");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = localPosition;
        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = text;
        label.fontSize = 34;
        label.characterSize = 0.055f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = color;
    }

    string GetSiteTitle()
    {
        if (node == null)
        {
            return "LOCAL GRID";
        }

        switch (node.nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return "AUX POWER";

            case InfrastructureNodeType.TransitLift:
                return "TRANSIT ACCESS";

            default:
                return "SIGNAL SERVICE";
        }
    }

    Color GetPoweredColor()
    {
        if (node == null)
        {
            return new Color(0.45f, 0.95f, 0.75f, 1f);
        }

        switch (node.nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return new Color(1f, 0.7f, 0.22f, 1f);

            case InfrastructureNodeType.TransitLift:
                return new Color(0.74f, 0.48f, 1f, 1f);

            default:
                return new Color(0.3f, 0.92f, 0.82f, 1f);
        }
    }

    void ApplyRendererColor(Renderer targetRenderer, Color color)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color", color);
        targetRenderer.SetPropertyBlock(block);
    }
}
