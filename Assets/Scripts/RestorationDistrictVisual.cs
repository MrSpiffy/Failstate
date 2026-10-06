using System.Collections.Generic;
using UnityEngine;

public class RestorationDistrictVisual : MonoBehaviour
{
    public InfrastructureNode node;
    public Color unpoweredColor = new Color(0.055f, 0.07f, 0.08f, 1f);
    public Color poweredColor = new Color(0.38f, 0.95f, 0.78f, 1f);
    public float lightHeight = 2.35f;
    public float lightIntensity = 1.3f;
    public float lightRange = 8f;

    private readonly List<Renderer> poweredRenderers = new List<Renderer>();
    private readonly List<Light> poweredLights = new List<Light>();
    private bool lastPoweredState = false;

    public void Configure(InfrastructureNode infrastructureNode, Color districtColor)
    {
        node = infrastructureNode;
        poweredColor = districtColor;
        ApplyPoweredState(node != null && node.restored);
    }

    public void AddFixture(Vector3 worldPosition, float cellSize)
    {
        float postHeight = Mathf.Max(1.4f, lightHeight * 0.72f);
        float markerScale = Mathf.Max(0.28f, cellSize * 0.16f);

        GameObject baseMarker = CreatePrimitive(
            PrimitiveType.Cube,
            "District_DormantRouteMarker",
            worldPosition + Vector3.up * 0.055f,
            new Vector3(cellSize * 0.48f, 0.08f, cellSize * 0.48f),
            unpoweredColor
        );
        poweredRenderers.Add(baseMarker.GetComponent<Renderer>());

        GameObject post = CreatePrimitive(
            PrimitiveType.Cylinder,
            "District_LightPost",
            worldPosition + Vector3.up * (postHeight * 0.5f),
            new Vector3(markerScale * 0.25f, postHeight * 0.5f, markerScale * 0.25f),
            new Color(0.14f, 0.17f, 0.18f, 1f)
        );

        GameObject lamp = CreatePrimitive(
            PrimitiveType.Cube,
            "District_DarkLamp",
            worldPosition + Vector3.up * lightHeight,
            new Vector3(markerScale, markerScale * 0.32f, markerScale),
            unpoweredColor
        );
        poweredRenderers.Add(lamp.GetComponent<Renderer>());

        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = lightRange;
        light.intensity = lightIntensity;
        light.color = poweredColor;
        light.enabled = false;
        poweredLights.Add(light);

        if (post != null)
        {
            post.transform.SetSiblingIndex(Mathf.Max(0, transform.childCount - 2));
        }
    }

    void Update()
    {
        bool powered = node != null && node.restored;

        if (powered != lastPoweredState)
        {
            ApplyPoweredState(powered);
        }
    }

    void ApplyPoweredState(bool powered)
    {
        lastPoweredState = powered;
        Color color = powered ? poweredColor : unpoweredColor;

        for (int i = 0; i < poweredRenderers.Count; i++)
        {
            if (poweredRenderers[i] != null)
            {
                ApplyRendererColor(poweredRenderers[i], color);
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

    GameObject CreatePrimitive(PrimitiveType type, string objectName, Vector3 worldPosition, Vector3 scale, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = objectName;
        obj.transform.SetParent(transform);
        obj.transform.position = worldPosition;
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

    void ApplyRendererColor(Renderer targetRenderer, Color color)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color", color);
        targetRenderer.SetPropertyBlock(block);
    }
}
