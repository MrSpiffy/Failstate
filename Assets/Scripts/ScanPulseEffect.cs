using UnityEngine;

public class ScanPulseEffect : MonoBehaviour
{
    public float radius = 20f;
    public float duration = 0.8f;
    public float startWidth = 0.2f;
    public float endWidth = 0.04f;
    public Color color = new Color(0.25f, 0.9f, 1f, 0.9f);

    private const int SegmentCount = 72;
    private LineRenderer lineRenderer;
    private Material lineMaterial;
    private float elapsed;

    public static void Spawn(Vector3 position, float pulseRadius, Color pulseColor)
    {
        GameObject pulseObject = new GameObject("PlayerScanPulse");
        pulseObject.transform.position = new Vector3(position.x, position.y + 0.08f, position.z);

        ScanPulseEffect effect = pulseObject.AddComponent<ScanPulseEffect>();
        effect.radius = pulseRadius;
        effect.color = pulseColor;
        effect.Setup();
    }

    void Setup()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = SegmentCount;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.textureMode = LineTextureMode.Stretch;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            lineMaterial = new Material(shader);
            lineRenderer.material = lineMaterial;
        }

        DrawRing(0.1f, color, startWidth);
    }

    void Update()
    {
        if (lineRenderer == null)
        {
            Setup();
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float easedRadius = Mathf.Lerp(0.1f, radius, 1f - Mathf.Pow(1f - t, 2f));
        Color fadedColor = color;
        fadedColor.a *= 1f - t;

        DrawRing(easedRadius, fadedColor, Mathf.Lerp(startWidth, endWidth, t));

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    void DrawRing(float currentRadius, Color currentColor, float width)
    {
        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        for (int i = 0; i < SegmentCount; i++)
        {
            float angle = (Mathf.PI * 2f * i) / SegmentCount;
            lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * currentRadius, 0f, Mathf.Sin(angle) * currentRadius));
        }
    }

    void OnDestroy()
    {
        if (lineMaterial != null)
        {
            Destroy(lineMaterial);
        }
    }
}
