using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using GLTFast;

// --- Gemini Response Data Structures ---
[Serializable]
public class GeminiAPIResponse
{
    public Candidate[] candidates;
}

[Serializable]
public class Candidate
{
    public ContentData content;
}

[Serializable]
public class ContentData
{
    public PartData[] parts;
}

[Serializable]
public class PartData
{
    public string text;
}

// --- Your Desired Dimension Output Structures ---
[Serializable]
public class DimensionOutputData
{
    public float length;
    public float width;
}

[Serializable]
public class DimensionResponse
{
    public DimensionOutputData dimension_output;
}

[RequireComponent(typeof(LineRenderer))]
public class LassoDrawer : MonoBehaviour
{
    [Header("References")]
    public Camera arCamera;
    public Transform boxParent;
    public BoxScaleUI boxScaleUI;

    [Header("Mesh Selection")]
    public GLBSelector glbSelector;

    [Header("Mode State")]
    [SerializeField] private bool eraseMode = false;

    [Header("Erase Settings")]
    public float eraseScreenPadding = 40f;

    [Header("Visual Settings")]
    public float drawDistanceFromCamera = 1.0f;
    public float minPointDistance = 0.005f;
    public float lineWidth = 0.025f;
    public Color drawLineColor = Color.green;
    public Color eraseLineColor = Color.red;

    [Header("Rounded Fitted Box Visual")]
    public bool useRoundedFittedBox = true;
    public float roundedBoxCornerRadius = 0.18f;
    public int roundedBoxCornerSegments = 10;
    public Color fittedBoxColor = new Color(0.2f, 1f, 0.55f, 1f);

    [Header("Fitted Box Loading Pulse")]
    public bool pulseBoxWhileLoadingOptions = true;
    public float boxPulseSpeed = 6.0f;
    public float boxPulseScaleAmount = 0.065f;
    public Color boxPulseBaseColor = new Color(0.45f, 1f, 0.65f, 1f);
    public Color boxPulseBrightColor = new Color(1f, 1f, 1f, 1f);

    private Coroutine fittedBoxPulseRoutine;
    private Vector3 fittedBoxBaseScale;

    [Header("Lasso Processing Animation")]
    public bool animateLassoWhileProcessing = true;
    public Color processingLineColor = new Color(0.1f, 1f, 0.65f, 0.75f);
    public Color processingLineBrightColor = new Color(0.85f, 1f, 0.95f, 1f);
    public float processingPulseSpeed = 3.25f;
    public float processingWidthMultiplier = 1.25f;
    private Coroutine lassoProcessingAnimationRoutine;
    private bool isProcessingDrawing = false;

    [Header("Raycast & Thickness")]
    public LayerMask raycastLayers = ~0;
    public float maxRayDistance = 20f;
    public float fixedThickness = 0.035f; // Used for the Height/Depth since API returns L and W

    [Header("Gemini API")]
    [Tooltip("Resources path (without file extension) of a text file that contains your Google Gemini API key. " +
             "The default points to Assets/Resources/Secrets/GeminiApiKey.txt, which is gitignored.")]
    public string geminiApiKeyResourcePath = "Secrets/GeminiApiKey";

    // Loaded at runtime by LoadGeminiApiKey(). [NonSerialized] keeps the key out of
    // scene files, so it can never be committed to source control by accident.
    [NonSerialized] public string geminiApiKey = "";

    [Tooltip("The Gemini model to use for spatial reasoning")]

    // gemini-3.1-flash-lite-preview
    public string geminiModel = "gemini-2.5-flash-lite";

    [Header("Loaded Mesh")]
    public bool destroyPreviousSpawnedMesh = true;
    public Vector3 loadedMeshEulerRotation = Vector3.zero;

    [Header("Placement")]
    public float surfaceOffset = 0.0f;

    [Header("Debug")]
    public bool logInputDebug = true;

    [Header("Fitted Box Visibility Safeguards")]
    [Tooltip("When true, HideFittedBox() will not hide the current resize box while a placement is pending. This helps prevent slider/dropdown/UI refreshes from making the box disappear.")]
    public bool keepFittedBoxVisibleWhileEditing = true;

    [Tooltip("When true, logs whenever HideFittedBox() or DestroyFittedBox() is called. Useful for finding which UI callback is hiding the box.")]
    public bool logFittedBoxVisibilityCalls = true;

    [Header("Candidate Preview UI / Loading")]
    [Tooltip("Minimum time, in seconds, to keep the fitted-box loading pulse visible before the first candidate preview appears.")]
    public float minimumCandidateLoadingFlashSeconds = 0.45f;

    [Tooltip("If true, the resize sliders/dropdown/place UI will hide while the first candidate is loading, then the candidate Prev/Next/Choose UI will show.")]
    public bool switchToCandidateUIAfterValidOptions = true;

    [Tooltip("Deprecated for this flow. The fitted box is force-hidden after the first candidate appears.")]
    public bool keepFittedBoxVisibleWithCandidatePreview = false;

    private const float METERS_TO_INCHES = 39.3701f;
    private const float INCHES_TO_METERS = 0.0254f;

    private LineRenderer lineRenderer;
    private readonly List<Vector3> worldPoints = new List<Vector3>();
    private readonly List<Vector2> screenPoints = new List<Vector2>();

    private bool isDrawing = false;
    private bool hasPendingPlacement = false;
    private bool isSpawning = false;
    private bool allowLassoInput = false;
    private bool isPreviewLoading = false;

    private GameObject fittedBox;
    
    private GameObject spawnedMeshObject;

    private Vector3 pendingSelectionCenter;

    private Vector3 pendingBottomAnchorWorld;
    private Vector3 pendingSurfaceNormal = Vector3.up;
    private Quaternion pendingPlacementRotation = Quaternion.identity;
    private bool hasPendingPlacementRotation = false;

    [Header("Fitted Box Orientation")]
    [Tooltip("If true, the fitted box angle is estimated from the user's drawn lasso instead of always using camera-forward.")]
    public bool orientBoxToLassoAngle = true;

    [Tooltip("Use every Nth drawn screen point when estimating the lasso angle. 1 is most accurate, 2-4 is faster.")]
    public int orientationSampleStride = 2;

    [Tooltip("Draws a short debug ray showing the estimated forward direction of the fitted box.")]
    public bool debugDrawFittedBoxForward = false;

    public float debugForwardRaySeconds = 2.0f;


    private List<MeshEntry> currentValidMeshes = new List<MeshEntry>();
    private int currentMeshIndex = -1;

    private readonly List<GameObject> placedObjects = new List<GameObject>();

    [Header("Gemini Image Scaling")]
    public bool downscaleGeminiImage = true;

    [Tooltip("Max width or height of the image sent to Gemini.")]
    public int geminiMaxImageDimension = 768;

    public bool saveGeminiInputImage = true;
    public bool saveFullResolutionDebugImage = true;

    [Header("Gemini Dimension Application")]
    [Tooltip("When true, the fitted box uses Gemini's returned width and length directly. localScale.x = width, localScale.z = length.")]
    public bool useRawGeminiDimensionsForFittedBox = true;

    [Tooltip("Only used when useRawGeminiDimensionsForFittedBox is false. Example: 0.85 shrinks the returned dimensions.")]
    public float geminiDimensionScale = 1.0f;

    [Tooltip("Minimum fallback size for Gemini dimensions, in meters.")]
    public float minGeminiBoxDimensionMeters = 0.05f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (arCamera == null) arCamera = Camera.main;

        LoadGeminiApiKey();
        SetupLineRenderer();
        allowLassoInput = false;
        isDrawing = false;
    }

    // Reads the Gemini API key from a gitignored text file under Assets/Resources
    // (see "Add the Gemini API key" in the README) so the key never lives in source control.
    private void LoadGeminiApiKey()
    {
        if (!string.IsNullOrWhiteSpace(geminiApiKey)) return;

        TextAsset keyAsset = Resources.Load<TextAsset>(geminiApiKeyResourcePath);
        if (keyAsset != null) geminiApiKey = keyAsset.text.Trim();

        if (string.IsNullOrWhiteSpace(geminiApiKey))
        {
            Debug.LogWarning(
                $"LassoDrawer: No Gemini API key found at Resources/{geminiApiKeyResourcePath}. " +
                "Create Assets/Resources/Secrets/GeminiApiKey.txt containing only your key. " +
                "Until then, sketches fall back to a simulated 0.5 m x 0.3 m box.");
        }
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.numCapVertices = 8;
        lineRenderer.numCornerVertices = 8;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) lineRenderer.material = new Material(shader);

        SetLineColor(eraseMode ? eraseLineColor : drawLineColor);
    }

    private void SetLineColor(Color color)
    {
        if (lineRenderer == null) return;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        if (lineRenderer.material != null) lineRenderer.material.color = color;
    }

    public void SetEraseMode(bool value)
    {
        eraseMode = value;
        ClearLasso();
        SetLineColor(eraseMode ? eraseLineColor : drawLineColor);
    }

    public void SetLassoInputEnabled(bool enabled)
    {
        allowLassoInput = enabled;
        if (!enabled)
        {
            isDrawing = false;
            ClearLasso();
        }
    }

    private void Update()
    {
        if (!allowLassoInput) return;
        if (isProcessingDrawing) return;

        // Important: slider/button touches should never be treated as lasso input.
        // This is stronger than EventSystem.current.IsPointerOverGameObject(),
        // which can be unreliable with the New Input System on mobile.
        if (IsAnyPointerOverUI())
        {
            isDrawing = false;
            return;
        }

        if (boxScaleUI != null && boxScaleUI.CandidatesActive)
        {
            isDrawing = false;
            return;
        }

    #if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
    #elif UNITY_IOS || UNITY_ANDROID
        HandleTouchInput();
    #endif
    }

    private void StartLassoProcessingAnimation()
    {
        if (!animateLassoWhileProcessing || lineRenderer == null)
            return;

        StopLassoProcessingAnimation(false);

        lassoProcessingAnimationRoutine = StartCoroutine(AnimateLassoProcessing());
    }

    private void StopLassoProcessingAnimation(bool clearLassoAfter)
    {
        if (lassoProcessingAnimationRoutine != null)
        {
            StopCoroutine(lassoProcessingAnimationRoutine);
            lassoProcessingAnimationRoutine = null;
        }

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            SetLineColor(eraseMode ? eraseLineColor : drawLineColor);
        }

        if (clearLassoAfter)
        {
            ClearLasso();
        }
    }

    private IEnumerator AnimateLassoProcessing()
    {
        if (lineRenderer == null)
            yield break;

        lineRenderer.numCapVertices = 12;
        lineRenderer.numCornerVertices = 12;

        while (true)
        {
            float wave = (Mathf.Sin(Time.time * processingPulseSpeed) + 1f) * 0.5f;
            float secondaryWave = (Mathf.Sin(Time.time * processingPulseSpeed * 1.7f + 1.5f) + 1f) * 0.5f;

            Color animatedColor = Color.Lerp(processingLineColor, processingLineBrightColor, wave);
            float animatedWidth = Mathf.Lerp(
                lineWidth * 0.95f,
                lineWidth * processingWidthMultiplier,
                secondaryWave
            );

            lineRenderer.startColor = animatedColor;
            lineRenderer.endColor = animatedColor;
            lineRenderer.startWidth = animatedWidth;
            lineRenderer.endWidth = animatedWidth;

            if (lineRenderer.material != null)
            {
                lineRenderer.material.color = animatedColor;
            }

            yield return null;
        }
    }

    private Vector3 GetStablePlacementNormal(Vector3 rawNormal)
    {
        Vector3 normal = rawNormal.normalized;

        if (normal.sqrMagnitude < 0.0001f)
            return Vector3.up;

        // If this is basically a floor, force it perfectly flat.
        // This removes small AR scene mesh normal noise.
        if (Vector3.Dot(normal, Vector3.up) > 0.85f)
            return Vector3.up;

        // Otherwise keep the actual normal for real slanted/wall surfaces.
        return normal;
    }

    public void SetFittedBoxScaleBottomAnchored(Vector3 newScale)
    {
        if (fittedBox == null)
            return;

        // Guard against UI initialization / slider bugs collapsing the box to (0,0,0).
        // fittedBox.localScale.x = width, y = height, z = length.
        newScale = new Vector3(
            Mathf.Max(0.05f, Mathf.Abs(newScale.x)),
            Mathf.Max(0.02f, Mathf.Abs(newScale.y)),
            Mathf.Max(0.05f, Mathf.Abs(newScale.z))
        );

        fittedBox.transform.localScale = newScale;
        Physics.SyncTransforms();

        float worldHeight = GetBoxWorldHeight(fittedBox);

        // Keep the bottom locked to the original surface hit point.
        Vector3 stableNormal = GetStablePlacementNormal(pendingSurfaceNormal);
        fittedBox.transform.position = pendingBottomAnchorWorld + stableNormal * (worldHeight * 0.5f);

        pendingSelectionCenter = fittedBox.transform.position;
        ShowFittedBox();

        if (logInputDebug)
        {
            Debug.Log($"SetFittedBoxScaleBottomAnchored -> scale={fittedBox.transform.localScale}, position={fittedBox.transform.position}");
        }
    }

    private float GetBoxWorldHeight(GameObject box)
    {
        if (box == null)
            return 0f;

        Transform t = box.transform;

        // This gives the world-space height along the box's local Y axis.
        return t.TransformVector(Vector3.up).magnitude;
    }


    private void HandleMouseInput()
    {
        if (Mouse.current == null) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (IsScreenPointOverUI(mousePos)) return;
            StartLasso(mousePos);
        }
        else if (Mouse.current.leftButton.isPressed && isDrawing)
        {
            UpdateLasso(mousePos);
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame && isDrawing)
        {
            EndLasso();
        }
    }

    private void HandleTouchInput()
    {
        if (Touchscreen.current == null) return;
        var primaryTouch = Touchscreen.current.primaryTouch;
        Vector2 touchPos = primaryTouch.position.ReadValue();

        if (primaryTouch.press.wasPressedThisFrame)
        {
            if (IsScreenPointOverUI(touchPos)) return;
            StartLasso(touchPos);
        }
        else if (primaryTouch.press.isPressed && isDrawing)
        {
            UpdateLasso(touchPos);
        }
        else if (primaryTouch.press.wasReleasedThisFrame && isDrawing)
        {
            EndLasso();
        }
    }

    private bool IsAnyPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        // Mouse / editor / simulator
        if (Mouse.current != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (IsScreenPointOverUI(mousePosition))
                return true;
        }

        // Touch / mobile
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (!touch.press.isPressed)
                    continue;

                Vector2 touchPosition = touch.position.ReadValue();

                if (IsScreenPointOverUI(touchPosition))
                    return true;
            }
        }

        return false;
    }

    private bool IsScreenPointOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    private void StartLasso(Vector2 screenPos)
    {
        ClearLasso();
        isDrawing = true;
        SetLineColor(eraseMode ? eraseLineColor : drawLineColor);
        AddPoint(screenPos);
    }

    private void UpdateLasso(Vector2 screenPos)
    {
        AddPoint(screenPos);
    }

private void EndLasso()
    {
        isDrawing = false;

        if (screenPoints.Count < 2)
        {
            ClearLasso();
            return;
        }

        if (eraseMode)
        {
            EraseObjectsTouchedByLasso();
            ClearLasso();
            if (boxScaleUI != null) boxScaleUI.ShowModeSelectionOnly();
            return;
        }

        // Calculate Centroid of the drawing
        Vector2 centroid = Vector2.zero;
        foreach (Vector2 pt in screenPoints)
        {
            centroid += pt;
        }
        centroid /= screenPoints.Count;

        if (logInputDebug) Debug.Log($"Drawing finished. Centroid calculated at screen position: {centroid}");
        
        // Start the Coroutine, but DO NOT clear the lasso here!
        StartCoroutine(ProcessDrawingAndScreenshot(centroid));
    }

    private void AddPoint(Vector2 screenPos)
    {
        Vector3 worldPoint = arCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, drawDistanceFromCamera));

        if (worldPoints.Count > 0 && Vector3.Distance(worldPoints[worldPoints.Count - 1], worldPoint) < minPointDistance)
            return;

        screenPoints.Add(screenPos);
        worldPoints.Add(worldPoint);

        lineRenderer.positionCount = worldPoints.Count;
        lineRenderer.SetPositions(worldPoints.ToArray());
    }

    public void ClearLasso()
    {
        screenPoints.Clear();
        worldPoints.Clear();
        if (lineRenderer != null) lineRenderer.positionCount = 0;
    }

    public void HideFittedBox()
    {
        HideFittedBoxInternal(false);
    }

    private void HideFittedBoxInternal(bool forceHide)
    {
        if (logFittedBoxVisibilityCalls)
        {
            Debug.LogWarning(
                $"HideFittedBox() requested. forceHide={forceHide}, " +
                $"hasPendingPlacement={hasPendingPlacement}, " +
                $"keepFittedBoxVisibleWhileEditing={keepFittedBoxVisibleWhileEditing}\n" +
                Environment.StackTrace
            );
        }

        // During resize / no-valid-options states, do not allow UI refreshes,
        // dropdown changes, or ClearCurrentCandidates() to hide the fitted box.
        if (!forceHide && keepFittedBoxVisibleWhileEditing && hasPendingPlacement && fittedBox != null)
        {
            StopFittedBoxPulse(true);
            ShowFittedBox();
            return;
        }

        StopFittedBoxPulse(true);

        if (fittedBox != null)
        {
            Renderer[] renderers = fittedBox.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                    renderer.enabled = false;
            }

            fittedBox.SetActive(false);
        }
    }

    public void ShowFittedBox()
    {
        if (fittedBox != null)
        {
            fittedBox.SetActive(true);

            Renderer[] renderers = fittedBox.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                    renderer.enabled = true;
            }
        }
    }

    private void HideFittedBoxAfterCandidateAppears()
    {
        // This is the desired candidate-preview behavior:
        // the fitted sizing box flashes while options are loading, then disappears
        // once the first valid GLB option has been loaded into the scene.
        StopFittedBoxPulse(true);

        if (fittedBox == null)
            return;

        Renderer[] renderers = fittedBox.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = false;
        }

        fittedBox.SetActive(false);
    }

    public void DestroyFittedBox()
    {
        if (logFittedBoxVisibilityCalls)
        {
            Debug.LogWarning("DestroyFittedBox() was called.\n" + Environment.StackTrace);
        }

        StopFittedBoxPulse(false);

        if (fittedBox != null)
        {
            Destroy(fittedBox);
            fittedBox = null;
        }

        hasPendingPlacement = false;
        hasPendingPlacementRotation = false;
    }

    [ContextMenu("DEBUG_HideFittedBox")]
    private void DEBUG_HideFittedBox()
    {
        HideFittedBoxInternal(true);
    }

    [ContextMenu("DEBUG_DestroyFittedBox")]
    private void DEBUG_DestroyFittedBox()
    {
        DestroyFittedBox();
    }

    public void ClearCurrentCandidates()
    {
        currentValidMeshes = new List<MeshEntry>();
        currentMeshIndex = -1;

        if (spawnedMeshObject != null)
        {
            Destroy(spawnedMeshObject);
            spawnedMeshObject = null;
        }

        // Do NOT hide the fitted box here.
        // BoxScaleUI may call ClearCurrentCandidates() during slider changes,
        // dropdown changes, valid-option refreshes, or panel transitions.
        // Hiding here makes the resize box disappear even though the user is
        // still editing a valid pending placement.
        if (fittedBox != null && hasPendingPlacement)
        {
            // StopFittedBoxPulse(true);
            ShowFittedBox();
        }
    }

    private Texture2D DownscaleTexture(Texture2D source, int maxDimension)
    {
        if (source == null)
            return null;

        int originalWidth = source.width;
        int originalHeight = source.height;

        if (maxDimension <= 0 || Mathf.Max(originalWidth, originalHeight) <= maxDimension)
        {
            return source;
        }

        float scale = (float)maxDimension / Mathf.Max(originalWidth, originalHeight);
        int targetWidth = Mathf.RoundToInt(originalWidth * scale);
        int targetHeight = Mathf.RoundToInt(originalHeight * scale);

        RenderTexture rt = RenderTexture.GetTemporary(
            targetWidth,
            targetHeight,
            0,
            RenderTextureFormat.ARGB32
        );

        RenderTexture previous = RenderTexture.active;

        Graphics.Blit(source, rt);
        RenderTexture.active = rt;

        Texture2D resized = new Texture2D(
            targetWidth,
            targetHeight,
            TextureFormat.RGB24,
            false
        );

        resized.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        resized.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return resized;
    }

    // --------------------------------------------------
    // Zero-Shot Capture & Gemini Extraction Logic
    // --------------------------------------------------

    private bool TryParseGeminiDimensions(string modelOutputJson, out float length, out float width)
    {
        length = 0f;
        width = 0f;

        if (string.IsNullOrWhiteSpace(modelOutputJson))
            return false;

        string cleaned = modelOutputJson.Trim();
        cleaned = cleaned.Replace("```json", "").Replace("```", "").Trim();

        try
        {
            DimensionResponse parsed = JsonUtility.FromJson<DimensionResponse>(cleaned);
            if (parsed != null && parsed.dimension_output != null)
            {
                length = parsed.dimension_output.length;
                width = parsed.dimension_output.width;

                if (length > 0.0001f && width > 0.0001f)
                    return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("JsonUtility Gemini dimension parse failed: " + e.Message);
        }

        // Fallback parser: handles slight Gemini variations like:
        // {"length": 1.2, "width": 0.8}
        // or nested objects with extra fields.
        try
        {
            System.Text.RegularExpressions.Match lengthMatch =
                System.Text.RegularExpressions.Regex.Match(
                    cleaned,
                    "\\\"length\\\"\\s*:\\s*(-?[0-9]+(?:\\.[0-9]+)?)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

            System.Text.RegularExpressions.Match widthMatch =
                System.Text.RegularExpressions.Regex.Match(
                    cleaned,
                    "\\\"width\\\"\\s*:\\s*(-?[0-9]+(?:\\.[0-9]+)?)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

            if (lengthMatch.Success && widthMatch.Success)
            {
                bool lengthOk = float.TryParse(
                    lengthMatch.Groups[1].Value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out length
                );

                bool widthOk = float.TryParse(
                    widthMatch.Groups[1].Value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out width
                );

                return lengthOk && widthOk && length > 0.0001f && width > 0.0001f;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Regex Gemini dimension parse failed: " + e.Message);
        }

        return false;
    }

    private IEnumerator ProcessDrawingAndScreenshot(Vector2 screenCentroid)
    {
        // Wait until all rendering is complete to capture the screen
        yield return new WaitForEndOfFrame();

        // 1. Take Screenshot
        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenTexture.Apply();

        // // NOW it is safe to clear the visual lasso, because the pixels are captured
        // ClearLasso();
        // Keep the lasso visible, but start animating it after the screenshot is captured.
        // This way Gemini still receives the normal green drawing, not the animated version.
        isProcessingDrawing = true;
        StartLassoProcessingAnimation();

        // Optional: save the full-resolution screenshot for debugging
        if (saveFullResolutionDebugImage)
        {
            byte[] fullImageBytes = screenTexture.EncodeToPNG();
            string fullSavePath = Path.Combine(Application.persistentDataPath, "LassoCapture_Full.png");
            File.WriteAllBytes(fullSavePath, fullImageBytes);
            Debug.Log($"Full screenshot saved locally to: {fullSavePath}");
        }

        // Downscale before sending to Gemini
        Texture2D geminiTexture = screenTexture;

        if (downscaleGeminiImage)
        {
            geminiTexture = DownscaleTexture(screenTexture, geminiMaxImageDimension);

            if (logInputDebug && geminiTexture != null)
            {
                Debug.Log($"Gemini image size: {geminiTexture.width} x {geminiTexture.height}");
            }
        }

        if (geminiTexture == null)
        {
            Debug.LogError("Gemini texture was null. Falling back to simulated box creation.");
            Destroy(screenTexture);
            SimulateBoxCreation(screenCentroid);
            yield break;
        }

        // Encode downscaled image to PNG and Base64 for Gemini
        byte[] imageBytes = geminiTexture.EncodeToPNG();
        string base64Image = Convert.ToBase64String(imageBytes);

        // Save exactly what Gemini receives
        if (saveGeminiInputImage)
        {
            string geminiSavePath = Path.Combine(Application.persistentDataPath, "LassoCapture_GeminiInput.png");
            File.WriteAllBytes(geminiSavePath, imageBytes);
            Debug.Log($"Gemini input image saved locally to: {geminiSavePath}");
        }

        // Clean up textures
        if (geminiTexture != screenTexture)
        {
            Destroy(geminiTexture);
        }

        Destroy(screenTexture);

        // 2. Formulate the Prompt
       string promptText = $@"You are an expert spatial reasoning AI analyzing an AR headset feed. 
            The user has drawn a free-form, bright green shape on the ground.

            Your task is to determine the real-world dimensions of the 'best-fit rectangle' that semantically represents their drawing.

            EXECUTION STEPS:
            1. SEMANTIC FILTERING: Look at the green drawing. Ignore any messy stray lines, accidental squiggles, or imperfect closures. Identify the core, intended mass of the shape.
            2. IDEALIZED RECTANGLE: Mentally project a clean, perfectly proportioned rectangle lying completely flat on the physical floor that best encapsulates this core shape.
            3. SCALING: Assume the camera is at human eye level (1.6 meters high). Use environmental context clues (like the concrete seams, orange cone, and benches) to calculate the distance and perspective.
            4. MEASUREMENT: Estimate the real-world physical length (depth) and width (horizontal spread) of your idealized rectangle in meters.

            CRITICAL RULE:
            Return ONLY a valid, raw JSON object. Do not include markdown formatting, code blocks, or conversational text.

            EXPECTED JSON FORMAT:
            {{\""dimension_output\"": {{\""length\"": <float_in_meters>, \""width\"": <float_in_meters>}}}}";

        // 3. Construct the JSON Payload natively
        string jsonPayload = $@"{{
            ""contents"": [{{
                ""parts"": [
                    {{""text"": ""{promptText}""}},
                    {{""inlineData"": {{""mimeType"": ""image/png"", ""data"": ""{base64Image}""}}}}
                ]
            }}],
            ""generationConfig"": {{
                ""temperature"": 0.1,
                ""responseMimeType"": ""application/json""
            }}
        }}";

        // 4. Send the Request to Gemini
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent?key={geminiApiKey}";
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Sending image and centroid data directly to Gemini API...");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Gemini API Error: {request.error}\n{request.downloadHandler.text}");
                SimulateBoxCreation(screenCentroid); // Fallback if API fails
            }
            else
            {
                // 5. Parse the Response
                try
                {
                    string jsonResponse = request.downloadHandler.text;
                    GeminiAPIResponse apiData = JsonUtility.FromJson<GeminiAPIResponse>(jsonResponse);
                    
                    string modelOutputJson = apiData.candidates[0].content.parts[0].text;
                    Debug.Log($"Raw JSON from Gemini: {modelOutputJson}");

                    if (!TryParseGeminiDimensions(modelOutputJson, out float parsedLength, out float parsedWidth))
                    {
                        Debug.LogError(
                            "Gemini returned JSON, but length/width could not be parsed or were <= 0. " +
                            "Falling back to simulated dimensions. Raw output: " + modelOutputJson
                        );
                        SimulateBoxCreation(screenCentroid);
                        yield break;
                    }

                    Debug.Log(
                        $"Parsed Gemini dimensions -> length={parsedLength:F3}m, " +
                        $"width={parsedWidth:F3}m"
                    );
                    
                    CreateCubeFromDimensions(screenCentroid, parsedLength, parsedWidth);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse Gemini output: {e.Message}");
                    StopLassoProcessingAnimation(true);
                    isProcessingDrawing = false;
                }
            }
        }
    }

    private void SimulateBoxCreation(Vector2 screenCentroid)
    {
        Debug.Log("Simulating API response...");
        CreateCubeFromDimensions(screenCentroid, 0.5f, 0.3f); 
    }

    private GameObject CreateRoundedBoxObject(string objectName)
    {
        GameObject obj = new GameObject(objectName);

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = CreateRoundedRectPrismMesh(
            roundedBoxCornerRadius,
            roundedBoxCornerSegments
        );

        return obj;
    }

    private Mesh CreateRoundedRectPrismMesh(float cornerRadius, int cornerSegments)
    {
        cornerSegments = Mathf.Max(2, cornerSegments);

        // This mesh is built as a 1x1x1 rounded rectangular prism.
        // Scaling is still handled by fittedBox.transform.localScale.
        float halfX = 0.5f;
        float halfZ = 0.5f;
        float halfY = 0.5f;

        float r = Mathf.Clamp(cornerRadius, 0.01f, 0.49f);

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        List<Vector2> outline = BuildRoundedRectOutline(halfX, halfZ, r, cornerSegments);

        int bottomStart = 0;
        for (int i = 0; i < outline.Count; i++)
        {
            vertices.Add(new Vector3(outline[i].x, -halfY, outline[i].y));
        }

        int topStart = vertices.Count;
        for (int i = 0; i < outline.Count; i++)
        {
            vertices.Add(new Vector3(outline[i].x, halfY, outline[i].y));
        }

        int count = outline.Count;

        // Side faces
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;

            int b0 = bottomStart + i;
            int b1 = bottomStart + next;
            int t0 = topStart + i;
            int t1 = topStart + next;

            triangles.Add(b0);
            triangles.Add(t0);
            triangles.Add(t1);

            triangles.Add(b0);
            triangles.Add(t1);
            triangles.Add(b1);
        }

        // Top face fan
        Vector3 topCenter = new Vector3(0f, halfY, 0f);
        int topCenterIndex = vertices.Count;
        vertices.Add(topCenter);

        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;

            // Flipped winding so the top face normal points upward.
            triangles.Add(topCenterIndex);
            triangles.Add(topStart + next);
            triangles.Add(topStart + i);
        }

        // Bottom face fan
        Vector3 bottomCenter = new Vector3(0f, -halfY, 0f);
        int bottomCenterIndex = vertices.Count;
        vertices.Add(bottomCenter);

        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;

            // Flipped winding so the bottom face normal points downward.
            triangles.Add(bottomCenterIndex);
            triangles.Add(bottomStart + i);
            triangles.Add(bottomStart + next);
        }

        Mesh mesh = new Mesh();
        mesh.name = "RoundedRectPrismMesh";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private List<Vector2> BuildRoundedRectOutline(float halfX, float halfZ, float radius, int segments)
    {
        List<Vector2> points = new List<Vector2>();

        Vector2[] centers =
        {
            new Vector2( halfX - radius,  halfZ - radius), // top right
            new Vector2(-halfX + radius,  halfZ - radius), // top left
            new Vector2(-halfX + radius, -halfZ + radius), // bottom left
            new Vector2( halfX - radius, -halfZ + radius)  // bottom right
        };

        float[] startAngles =
        {
            0f,
            90f,
            180f,
            270f
        };

        for (int c = 0; c < 4; c++)
        {
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Deg2Rad * (startAngles[c] + i * 90f / segments);
                Vector2 p = centers[c] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                points.Add(p);
            }
        }

        return points;
    }

    private void ApplyFittedBoxMaterial(GameObject obj)
    {
        if (obj == null) return;

        Renderer r = obj.GetComponent<Renderer>();
        if (r == null) return;

        // Use an unlit shader when possible. In AR/URP, the Standard shader can
        // render very dark/black depending on lighting, which makes the fitted
        // selection box look like a black UI block.
        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit") ??
            Shader.Find("Unlit/Color") ??
            Shader.Find("Sprites/Default") ??
            Shader.Find("Standard");

        if (shader == null) return;

        Material mat = new Material(shader);

        Color color = fittedBoxColor;
        color.a = 1f;

        mat.color = color;

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.7f);
        }

        r.material = mat;
    }

    private void StartFittedBoxPulse()
    {
        if (!pulseBoxWhileLoadingOptions || fittedBox == null)
            return;

        StopFittedBoxPulse(false);

        fittedBoxBaseScale = fittedBox.transform.localScale;
        fittedBoxPulseRoutine = StartCoroutine(AnimateFittedBoxPulse());
    }

    private void StopFittedBoxPulse(bool restoreScale)
    {
        if (fittedBoxPulseRoutine != null)
        {
            StopCoroutine(fittedBoxPulseRoutine);
            fittedBoxPulseRoutine = null;
        }

        if (restoreScale && fittedBox != null)
        {
            fittedBox.transform.localScale = fittedBoxBaseScale;
        }
    }

    private IEnumerator AnimateFittedBoxPulse()
    {
        Renderer r = fittedBox != null ? fittedBox.GetComponent<Renderer>() : null;

        while (fittedBox != null && fittedBox.activeSelf)
        {
            float t = (Mathf.Sin(Time.time * boxPulseSpeed) + 1f) * 0.5f;

            float scalePulse = 1f + t * boxPulseScaleAmount;
            fittedBox.transform.localScale = fittedBoxBaseScale * scalePulse;

            if (r != null && r.material != null)
            {
                Color pulseColor = Color.Lerp(boxPulseBaseColor, boxPulseBrightColor, t);
                pulseColor.a = 1f;
                r.material.color = pulseColor;

                if (r.material.HasProperty("_EmissionColor"))
                {
                    r.material.EnableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", pulseColor * 1.25f);
                }
            }

            yield return null;
        }
    }


    private bool TryGetFallbackSurfaceForward(Vector3 surfaceNormal, out Vector3 forwardOnSurface)
    {
        Vector3 stableNormal = GetStablePlacementNormal(surfaceNormal);

        forwardOnSurface = Vector3.ProjectOnPlane(arCamera.transform.forward, stableNormal);

        if (forwardOnSurface.sqrMagnitude < 0.0001f)
            forwardOnSurface = Vector3.ProjectOnPlane(Vector3.forward, stableNormal);

        if (forwardOnSurface.sqrMagnitude < 0.0001f)
            forwardOnSurface = Vector3.forward;

        forwardOnSurface.Normalize();
        return true;
    }

    private bool TryEstimatePrincipalLassoDirectionOnSurface(Vector3 surfaceNormal, out Vector3 principalDirection)
    {
        principalDirection = Vector3.zero;

        if (!orientBoxToLassoAngle)
            return false;

        if (arCamera == null || screenPoints == null || screenPoints.Count < 3)
            return false;

        Vector3 stableNormal = GetStablePlacementNormal(surfaceNormal);
        int stride = Mathf.Max(1, orientationSampleStride);

        List<Vector3> surfacePoints = new List<Vector3>();

        for (int i = 0; i < screenPoints.Count; i += stride)
        {
            Ray ray = arCamera.ScreenPointToRay(screenPoints[i]);

            if (Physics.Raycast(ray, out RaycastHit pointHit, maxRayDistance, raycastLayers))
            {
                surfacePoints.Add(pointHit.point);
            }
        }

        if (surfacePoints.Count < 3)
            return false;

        Vector3 center = Vector3.zero;

        for (int i = 0; i < surfacePoints.Count; i++)
            center += surfacePoints[i];

        center /= surfacePoints.Count;

        // Build a stable 2D basis on the detected floor / surface plane.
        Vector3 basisX = Vector3.ProjectOnPlane(arCamera.transform.right, stableNormal);

        if (basisX.sqrMagnitude < 0.0001f)
            basisX = Vector3.ProjectOnPlane(Vector3.right, stableNormal);

        if (basisX.sqrMagnitude < 0.0001f)
            basisX = Vector3.Cross(stableNormal, Vector3.forward);

        basisX.Normalize();

        Vector3 basisZ = Vector3.Cross(basisX, stableNormal).normalized;

        // 2D PCA covariance in the surface plane.
        float xx = 0f;
        float zz = 0f;
        float xz = 0f;

        for (int i = 0; i < surfacePoints.Count; i++)
        {
            Vector3 delta = surfacePoints[i] - center;
            float x = Vector3.Dot(delta, basisX);
            float z = Vector3.Dot(delta, basisZ);

            xx += x * x;
            zz += z * z;
            xz += x * z;
        }

        if (xx + zz < 0.000001f)
            return false;

        // Principal axis angle for a 2x2 covariance matrix.
        float angle = 0.5f * Mathf.Atan2(2f * xz, xx - zz);

        principalDirection =
            Mathf.Cos(angle) * basisX +
            Mathf.Sin(angle) * basisZ;

        if (principalDirection.sqrMagnitude < 0.0001f)
            return false;

        principalDirection.Normalize();

        // Flip only for consistency. A rectangle angle is equivalent under 180 degrees,
        // but keeping it roughly camera-forward makes previews less jumpy.
        Vector3 cameraForwardOnSurface;
        TryGetFallbackSurfaceForward(stableNormal, out cameraForwardOnSurface);

        if (Vector3.Dot(principalDirection, cameraForwardOnSurface) < 0f)
            principalDirection = -principalDirection;

        return true;
    }

    private bool TryGetFittedBoxForwardFromLasso(float length, float width, Vector3 surfaceNormal, out Vector3 forwardOnSurface)
    {
        forwardOnSurface = Vector3.zero;

        Vector3 stableNormal = GetStablePlacementNormal(surfaceNormal);

        if (!TryEstimatePrincipalLassoDirectionOnSurface(stableNormal, out Vector3 principalDirection))
            return false;

        // The fitted box uses local X = width and local Z = length.
        // If length is the larger dimension, align the box's local Z/forward to the principal axis.
        // If width is the larger dimension, align the box's local X/right to the principal axis.
        if (length >= width)
        {
            forwardOnSurface = principalDirection;
        }
        else
        {
            forwardOnSurface = Vector3.Cross(principalDirection, stableNormal);
        }

        if (forwardOnSurface.sqrMagnitude < 0.0001f)
            return false;

        forwardOnSurface = Vector3.ProjectOnPlane(forwardOnSurface, stableNormal).normalized;

        Vector3 cameraForwardOnSurface;
        TryGetFallbackSurfaceForward(stableNormal, out cameraForwardOnSurface);

        if (Vector3.Dot(forwardOnSurface, cameraForwardOnSurface) < 0f)
            forwardOnSurface = -forwardOnSurface;

        return true;
    }

    private Quaternion GetPlacementRotationForSelection(float length, float width, Vector3 surfaceNormal)
    {
        Vector3 stableNormal = GetStablePlacementNormal(surfaceNormal);

        Vector3 forwardOnSurface;

        if (!TryGetFittedBoxForwardFromLasso(length, width, stableNormal, out forwardOnSurface))
        {
            TryGetFallbackSurfaceForward(stableNormal, out forwardOnSurface);
        }

        if (debugDrawFittedBoxForward)
        {
            Debug.DrawRay(pendingBottomAnchorWorld, forwardOnSurface.normalized * 0.75f, Color.cyan, debugForwardRaySeconds);
            Debug.DrawRay(pendingBottomAnchorWorld, stableNormal.normalized * 0.35f, Color.yellow, debugForwardRaySeconds);
        }

        return Quaternion.LookRotation(forwardOnSurface.normalized, stableNormal);
    }

    private void CreateCubeFromDimensions(Vector2 screenCentroid, float length, float width)
    {
        // Shoot raycast from the centroid
        Ray ray = arCamera.ScreenPointToRay(screenCentroid);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, raycastLayers))
        {
            Vector3 hitPoint = hit.point;
            Vector3 hitNormal = GetStablePlacementNormal(hit.normal);

            if (fittedBox == null)
            {
                if (useRoundedFittedBox)
                {
                    fittedBox = CreateRoundedBoxObject("FittedBox");
                }
                else
                {
                    fittedBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    fittedBox.name = "FittedBox";
                }

                ApplyFittedBoxMaterial(fittedBox);

                Collider c = fittedBox.GetComponent<Collider>();
                if (c != null) c.enabled = false;

                if (boxParent != null) fittedBox.transform.SetParent(boxParent, true);
            }

            // Use Gemini dimensions directly so the visible resize box and slider labels
            // initialize to the same values returned by Gemini.
            //
            // Axis convention:
            //   fittedBox.localScale.x = width
            //   fittedBox.localScale.y = height/thickness
            //   fittedBox.localScale.z = length
            //
            // IMPORTANT:
            // Do not use the old 0.85 multiplier here unless you intentionally want
            // the preview box to be smaller than Gemini's returned dimensions.
            float safeLength = Mathf.Max(minGeminiBoxDimensionMeters, Mathf.Abs(length));
            float safeWidth = Mathf.Max(minGeminiBoxDimensionMeters, Mathf.Abs(width));

            float appliedLength = useRawGeminiDimensionsForFittedBox
                ? safeLength
                : safeLength * geminiDimensionScale;

            float appliedWidth = useRawGeminiDimensionsForFittedBox
                ? safeWidth
                : safeWidth * geminiDimensionScale;

            Vector3 newSize = new Vector3(appliedWidth, fixedThickness, appliedLength);

            Debug.Log(
                $"CreateCubeFromDimensions -> Gemini length={length:F3}m, width={width:F3}m; " +
                $"applied fittedBox scale x(width)={newSize.x:F3}m, y(height)={newSize.y:F3}m, z(length)={newSize.z:F3}m"
            );

            if (fittedBox.transform.parent != null)
            {
                Vector3 parentScale = fittedBox.transform.parent.lossyScale;
                if (Mathf.Abs(parentScale.x - 1f) > 0.01f ||
                    Mathf.Abs(parentScale.y - 1f) > 0.01f ||
                    Mathf.Abs(parentScale.z - 1f) > 0.01f)
                {
                    Debug.LogWarning(
                        $"FittedBox parent lossyScale={parentScale}. " +
                        "If the visual box size does not match Gemini values, check boxParent scale. " +
                        "BoxScaleUI displays localScale in meters."
                    );
                }
            }

            // Store the original floor/surface point.
            // This is the point the bottom of the box should stay anchored to.
            pendingBottomAnchorWorld = hitPoint;

            Quaternion placementRotation = GetPlacementRotationForSelection(length, width, hitNormal);
            pendingPlacementRotation = placementRotation;
            hasPendingPlacementRotation = true;

            fittedBox.transform.rotation = placementRotation;

            fittedBox.transform.localScale = newSize;
            Physics.SyncTransforms();

            Debug.Log($"FittedBox initialized localScale={fittedBox.transform.localScale}");

            float worldHeight = GetBoxWorldHeight(fittedBox);

            // Place the box center above the floor anchor by half its height.
            fittedBox.transform.position = pendingBottomAnchorWorld + fittedBox.transform.up * (worldHeight * 0.5f);

            ShowFittedBox();

            pendingSelectionCenter = fittedBox.transform.position;
            pendingSurfaceNormal = hitNormal;
            hasPendingPlacement = true;

            if (boxScaleUI != null)
            {
                Debug.Log("LassoDrawer: Calling BoxScaleUI.ShowForBox with fittedBox.");
                boxScaleUI.ShowForBox(fittedBox);

                // New BoxScaleUI exposes this method to ensure the sliders and text labels
                // are synchronized from the box scale after Gemini initializes it.
                // This prevents stale/clamped slider text after the Gemini call completes.
                // Re-apply the Gemini dimensions after BoxScaleUI activates its panel.
                // This protects against UI scripts or slider managers clamping/changing the box scale.
                fittedBox.transform.localScale = newSize;
                Physics.SyncTransforms();
                pendingSelectionCenter = fittedBox.transform.position;
                ShowFittedBox();

                TryCallBoxScaleUIMethod("ForceSyncSlidersAndTextsFromTargetBox");

                Debug.Log($"After BoxScaleUI.ShowForBox + ForceSync -> fittedBox localScale={fittedBox.transform.localScale}");
            }
            else
            {
                Debug.LogError("LassoDrawer: boxScaleUI is NULL, so the resize UI cannot be shown.");
            }

            StopLassoProcessingAnimation(true);
            isProcessingDrawing = false;
        }
        else
        {
            Debug.LogWarning("Raycast from drawing centroid missed all surfaces. Cannot place box.");
            StopLassoProcessingAnimation(true);
            isProcessingDrawing = false;
        }
    }

    // --------------------------------------------------
    // Erase logic: screen-space scribble erase
    // --------------------------------------------------

    private void EraseObjectsTouchedByLasso()
    {
        if (screenPoints == null || screenPoints.Count == 0) return;
        if (arCamera == null) arCamera = Camera.main;
        if (arCamera == null) return;

        placedObjects.RemoveAll(o => o == null);
        if (placedObjects.Count == 0) return;

        HashSet<GameObject> objectsToErase = new HashSet<GameObject>();

        foreach (GameObject obj in placedObjects)
        {
            if (obj == null) continue;
            if (DoesEraseStrokeTouchObject(obj)) objectsToErase.Add(obj);
        }

        foreach (GameObject obj in objectsToErase)
        {
            if (obj == null) continue;
            placedObjects.Remove(obj);
            Destroy(obj);
        }
    }

    private bool DoesEraseStrokeTouchObject(GameObject obj)
    {
        if (obj == null) return false;
        if (!TryGetAccurateBounds(obj, out Bounds bounds)) return false;

        Rect screenRect = GetObjectScreenRect(bounds);
        if (screenRect.width <= 0f || screenRect.height <= 0f) return false;

        screenRect.xMin -= eraseScreenPadding;
        screenRect.xMax += eraseScreenPadding;
        screenRect.yMin -= eraseScreenPadding;
        screenRect.yMax += eraseScreenPadding;

        for (int i = 0; i < screenPoints.Count; i++)
        {
            if (screenRect.Contains(screenPoints[i])) return true;
        }

        for (int i = 1; i < screenPoints.Count; i++)
        {
            if (LineIntersectsRect(screenPoints[i - 1], screenPoints[i], screenRect)) return true;
        }

        return false;
    }

    private Rect GetObjectScreenRect(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3[] corners = new Vector3[8]
        {
            center + new Vector3(-extents.x, -extents.y, -extents.z),
            center + new Vector3(-extents.x, -extents.y,  extents.z),
            center + new Vector3(-extents.x,  extents.y, -extents.z),
            center + new Vector3(-extents.x,  extents.y,  extents.z),
            center + new Vector3( extents.x, -extents.y, -extents.z),
            center + new Vector3( extents.x, -extents.y,  extents.z),
            center + new Vector3( extents.x,  extents.y, -extents.z),
            center + new Vector3( extents.x,  extents.y,  extents.z)
        };

        bool initialized = false;
        float minX = 0f, maxX = 0f, minY = 0f, maxY = 0f;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 screen = arCamera.WorldToScreenPoint(corners[i]);
            if (screen.z < 0f) continue;

            if (!initialized)
            {
                minX = maxX = screen.x;
                minY = maxY = screen.y;
                initialized = true;
            }
            else
            {
                minX = Mathf.Min(minX, screen.x);
                maxX = Mathf.Max(maxX, screen.x);
                minY = Mathf.Min(minY, screen.y);
                maxY = Mathf.Max(maxY, screen.y);
            }
        }

        if (!initialized) return new Rect(0f, 0f, 0f, 0f);
        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    private bool LineIntersectsRect(Vector2 a, Vector2 b, Rect rect)
    {
        if (rect.Contains(a) || rect.Contains(b)) return true;

        Vector2 bottomLeft = new Vector2(rect.xMin, rect.yMin);
        Vector2 bottomRight = new Vector2(rect.xMax, rect.yMin);
        Vector2 topLeft = new Vector2(rect.xMin, rect.yMax);
        Vector2 topRight = new Vector2(rect.xMax, rect.yMax);

        return LinesIntersect(a, b, bottomLeft, bottomRight) ||
               LinesIntersect(a, b, bottomRight, topRight) ||
               LinesIntersect(a, b, topRight, topLeft) ||
               LinesIntersect(a, b, topLeft, bottomLeft);
    }

    private bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        float d = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);
        if (Mathf.Abs(d) < 0.0001f) return false;

        float u = ((b1.x - a1.x) * (b2.y - b1.y) - (b1.y - a1.y) * (b2.x - b1.x)) / d;
        float v = ((b1.x - a1.x) * (a2.y - a1.y) - (b1.y - a1.y) * (a2.x - a1.x)) / d;

        return u >= 0f && u <= 1f && v >= 0f && v <= 1f;
    }

    private void SetupSpawnedObjectForErasing(GameObject obj)
    {
        if (obj == null) return;
        EnsureColliders(obj);
    }

    private void EnsureColliders(GameObject obj)
    {
        if (obj == null) return;
        MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf == null || mf.sharedMesh == null) continue;
            Collider existingCollider = mf.GetComponent<Collider>();

            if (existingCollider == null)
            {
                MeshCollider meshCollider = mf.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = mf.sharedMesh;
                meshCollider.convex = false;
            }
        }
    }

    // --------------------------------------------------
    // Candidate / spawning logic
    // --------------------------------------------------

    private string GetSelectedObjectTypeFromUI()
    {
        if (boxScaleUI == null) return null;
        string selectedObjectType = boxScaleUI.GetSelectedObjectType();
        return string.IsNullOrWhiteSpace(selectedObjectType) ? null : selectedObjectType.Trim();
    }

    public int CountValidMeshesForDimensions(float lengthMeters, float widthMeters, float heightMeters, string selectedObjectType)
    {
        if (glbSelector == null || string.IsNullOrWhiteSpace(selectedObjectType)) return 0;

        float lengthInches = lengthMeters * METERS_TO_INCHES;
        float widthInches = widthMeters * METERS_TO_INCHES;
        float heightInches = heightMeters * METERS_TO_INCHES;

        List<MeshEntry> validMeshes = glbSelector.GetValidMeshes(selectedObjectType.Trim(), lengthInches, widthInches, heightInches);
        return validMeshes != null ? validMeshes.Count : 0;
    }

    public void OnScaleAdjustmentFinished()
    {
        PrepareCandidatesFromCurrentUI();
    }

    public async void PrepareCandidatesFromCurrentUI()
    {
        if (isSpawning || !hasPendingPlacement || fittedBox == null) return;

        string selectedObjectType = GetSelectedObjectTypeFromUI();
        if (string.IsNullOrWhiteSpace(selectedObjectType)) return;

        Vector3 finalBoxSizeMeters = fittedBox.transform.localScale;

        // IMPORTANT:
        // fittedBox.localScale.x = width
        // fittedBox.localScale.y = height
        // fittedBox.localScale.z = length
        float widthMeters = Mathf.Abs(finalBoxSizeMeters.x);
        float heightMeters = Mathf.Abs(finalBoxSizeMeters.y);
        float lengthMeters = Mathf.Abs(finalBoxSizeMeters.z);

        if (widthMeters <= 0.001f || heightMeters <= 0.001f || lengthMeters <= 0.001f)
        {
            Debug.LogWarning(
                $"Place pressed, but fittedBox scale is invalid: {finalBoxSizeMeters}. " +
                "Keeping resize UI visible and skipping candidate search."
            );

            ShowFittedBox();
            ShowResizeControlsOnly();
            if (boxScaleUI != null) boxScaleUI.SetValidOptionsCount(0);
            return;
        }

        Debug.Log(
            $"PrepareCandidatesFromCurrentUI -> " +
            $"fittedBox scale={finalBoxSizeMeters}, " +
            $"length={lengthMeters:F3}m, width={widthMeters:F3}m, height={heightMeters:F3}m"
        );

        isSpawning = true;

        if (fittedBox != null)
        {
            ShowFittedBox();
            StartFittedBoxPulse();
        }

        try
        {
            await FindMatchingMeshesAsync(
                lengthMeters,
                widthMeters,
                heightMeters,
                pendingSelectionCenter,
                pendingSurfaceNormal,
                selectedObjectType
            );
        }
        finally
        {
            isSpawning = false;
        }
    }

    public void ConfirmCurrentCandidate(bool keepFittedBoxVisible = false)
    {
        if (currentValidMeshes == null ||
            currentValidMeshes.Count == 0 ||
            currentMeshIndex < 0 ||
            currentMeshIndex >= currentValidMeshes.Count)
        {
            return;
        }

        if (fittedBox != null)
        {
            if (keepFittedBoxVisible)
            {
                ShowFittedBox();

                // Important:
                // Detach this reference so BoxScaleUI can animate/destroy it later
                // without LassoDrawer destroying it immediately.
                fittedBox = null;
            }
            else
            {
                Destroy(fittedBox);
                fittedBox = null;
            }
        }

        if (spawnedMeshObject != null)
        {
            SetupSpawnedObjectForErasing(spawnedMeshObject);

            if (!placedObjects.Contains(spawnedMeshObject))
                placedObjects.Add(spawnedMeshObject);

            spawnedMeshObject = null;
        }

        hasPendingPlacement = false;
        hasPendingPlacementRotation = false;
        ClearLasso();
        currentValidMeshes = new List<MeshEntry>();
        currentMeshIndex = -1;
    }

    public async void ShowNextCandidate()
    {
        if (currentValidMeshes == null || currentValidMeshes.Count == 0) return;
        currentMeshIndex = (currentMeshIndex + 1) % currentValidMeshes.Count;
        await PreviewCurrentCandidateAsync();
    }

    public async void ShowPreviousCandidate()
    {
        if (currentValidMeshes == null || currentValidMeshes.Count == 0) return;
        currentMeshIndex = (currentMeshIndex - 1 + currentValidMeshes.Count) % currentValidMeshes.Count;
        await PreviewCurrentCandidateAsync();
    }

    private async Task PreviewCurrentCandidateAsync()
    {
        if (isPreviewLoading || currentMeshIndex < 0 || currentMeshIndex >= currentValidMeshes.Count) return;

        MeshEntry chosenMesh = currentValidMeshes[currentMeshIndex];
        isPreviewLoading = true;

        try
        {
            await LoadAndPlaceMeshAsync(
                chosenMesh,
                pendingBottomAnchorWorld,
                pendingSurfaceNormal
            );
        }
        finally
        {
            isPreviewLoading = false;
        }
    }

    private bool TryCallBoxScaleUIMethod(string methodName)
    {
        if (boxScaleUI == null || string.IsNullOrWhiteSpace(methodName))
            return false;

        System.Reflection.MethodInfo method = boxScaleUI.GetType().GetMethod(
            methodName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic
        );

        if (method == null)
            return false;

        method.Invoke(boxScaleUI, null);
        return true;
    }

    private void ShowResizeControlsOnly()
    {
        if (boxScaleUI == null) return;

        if (!TryCallBoxScaleUIMethod("ShowResizeControlsOnly"))
        {
            // Backward-compatible fallback.
            boxScaleUI.SetCandidateButtonsVisible(false);
        }
    }

    private void ShowCandidateLoadingState()
    {
        if (boxScaleUI == null) return;

        if (!TryCallBoxScaleUIMethod("ShowCandidateLoadingState"))
        {
            // Backward-compatible fallback. If your BoxScaleUI implementation only
            // has SetCandidateButtonsVisible, this keeps candidate buttons hidden
            // while the first GLB is loading.
            boxScaleUI.SetCandidateButtonsVisible(false);
        }
    }

    private void ShowCandidateSelectionOnly()
    {
        if (boxScaleUI == null) return;

        if (!TryCallBoxScaleUIMethod("ShowCandidateSelectionOnly"))
        {
            // Backward-compatible fallback.
            boxScaleUI.SetCandidateButtonsVisible(true);
        }
    }

    private async Task WaitForCandidateLoadingFlashAsync()
    {
        // Let Unity render at least one frame with the pulsing fitted box and loading UI
        // before GLB loading/instantiation starts.
        await Task.Yield();
        await Task.Yield();

        if (minimumCandidateLoadingFlashSeconds > 0f)
        {
            int delayMs = Mathf.RoundToInt(minimumCandidateLoadingFlashSeconds * 1000f);
            await Task.Delay(delayMs);
        }
    }

    private async Task FindMatchingMeshesAsync(
        float lengthMeters,
        float widthMeters,
        float heightMeters,
        Vector3 selectionCenter,
        Vector3 surfaceNormal,
        string selectedObjectType
    )
    {
        if (glbSelector == null)
        {
            Debug.LogWarning("FindMatchingMeshesAsync: glbSelector is null.");
            return;
        }

        float lengthInches = lengthMeters * METERS_TO_INCHES;
        float widthInches = widthMeters * METERS_TO_INCHES;
        float heightInches = heightMeters * METERS_TO_INCHES;

        currentValidMeshes = glbSelector.GetValidMeshes(
            selectedObjectType,
            lengthInches,
            widthInches,
            heightInches
        );

        int validCount = currentValidMeshes != null ? currentValidMeshes.Count : 0;

        if (boxScaleUI != null)
        {
            boxScaleUI.SetValidOptionsCount(validCount);
        }

        if (currentValidMeshes == null || currentValidMeshes.Count == 0)
        {
            Debug.LogWarning(
                $"No valid meshes found. " +
                $"Type={selectedObjectType}, " +
                $"L={lengthInches:F1}in, " +
                $"W={widthInches:F1}in, " +
                $"H={heightInches:F1}in"
            );

            // Important:
            // Do NOT destroy the fitted box here.
            // Keep it visible so the user can adjust the sliders and try again.
            StopFittedBoxPulse(true);

            if (fittedBox != null)
            {
                ShowFittedBox();
            }

            hasPendingPlacement = true;
            pendingSelectionCenter = selectionCenter;
            pendingSurfaceNormal = surfaceNormal;

            if (boxScaleUI != null)
            {
                // Keep resize UI visible so the user can adjust sliders and try again.
                ShowResizeControlsOnly();
                boxScaleUI.SetValidOptionsCount(0);
            }

            isSpawning = false;
            return;
        }

        currentMeshIndex = 0;
        pendingSelectionCenter = selectionCenter;
        pendingSurfaceNormal = surfaceNormal;
        hasPendingPlacement = true;

        // Valid options exist. Switch away from the resize UI immediately, then
        // show a visible loading pulse before the first candidate preview appears.
        if (switchToCandidateUIAfterValidOptions)
        {
            ShowCandidateLoadingState();
        }

        if (fittedBox != null)
        {
            ShowFittedBox();
            StartFittedBoxPulse();
        }

        await WaitForCandidateLoadingFlashAsync();
        await PreviewCurrentCandidateAsync();

        // Stop loading animation after the first candidate appears.
        // Then force-hide the fitted sizing box so the user sees the candidate object.
        HideFittedBoxAfterCandidateAppears();

        if (switchToCandidateUIAfterValidOptions)
        {
            ShowCandidateSelectionOnly();
        }
    }

    private async Task LoadAndPlaceMeshAsync(
        MeshEntry chosenMesh,
        Vector3 surfaceAnchorPoint,
        Vector3 surfaceNormal
    )
    {
        if (chosenMesh == null || string.IsNullOrWhiteSpace(chosenMesh.path)) return;

        string glbUrl = glbSelector.BuildBackblazeUrl(chosenMesh);
        if (string.IsNullOrWhiteSpace(glbUrl)) return;

        if (destroyPreviousSpawnedMesh && spawnedMeshObject != null)
        {
            Destroy(spawnedMeshObject);
            spawnedMeshObject = null;
        }

        var gltf = new GltfImport();
        bool success = await gltf.Load(glbUrl);

        if (!success) return;

        spawnedMeshObject = new GameObject(chosenMesh.name);
        if (boxParent != null) spawnedMeshObject.transform.SetParent(boxParent, true);

        bool instantiated = await gltf.InstantiateMainSceneAsync(spawnedMeshObject.transform);

        if (!instantiated)
        {
            Destroy(spawnedMeshObject);
            spawnedMeshObject = null;
            return;
        }

        Vector3 stableNormal = GetStablePlacementNormal(surfaceNormal);

        Quaternion placementRotation = hasPendingPlacementRotation
            ? pendingPlacementRotation
            : GetPlacementRotationForSelection(
                chosenMesh.standardized_dimensions != null ? chosenMesh.standardized_dimensions.l * INCHES_TO_METERS : 1f,
                chosenMesh.standardized_dimensions != null ? chosenMesh.standardized_dimensions.w * INCHES_TO_METERS : 1f,
                stableNormal
            );

        Quaternion modelCorrection = Quaternion.Euler(loadedMeshEulerRotation);

        spawnedMeshObject.transform.rotation = placementRotation * modelCorrection;

        ScaleObjectToJsonDimensions(spawnedMeshObject, chosenMesh);

        // Start roughly at the actual scene mesh hit point.
        spawnedMeshObject.transform.position = surfaceAnchorPoint;

        Physics.SyncTransforms();

        // Snap the bottom of the mesh bounds onto the scene mesh surface.
       PlaceObjectOnTopOfSurface(
            spawnedMeshObject,
            surfaceAnchorPoint,
            stableNormal
        );

        SetupSpawnedObjectForErasing(spawnedMeshObject);
    }

    // --------------------------------------------------
    // Mesh scaling / placement helpers
    // --------------------------------------------------

    private void ScaleObjectToJsonDimensions(GameObject obj, MeshEntry mesh)
    {
        if (obj == null || mesh == null) return;

        float jsonLengthInches = GetJsonLength(mesh);
        float jsonWidthInches = GetJsonWidth(mesh);
        float jsonHeightInches = GetJsonHeight(mesh);

        if (jsonLengthInches <= 0f || jsonWidthInches <= 0f || jsonHeightInches <= 0f) return;

        Vector3 targetSizeMeters = new Vector3(
            jsonLengthInches * INCHES_TO_METERS,
            jsonHeightInches * INCHES_TO_METERS,
            jsonWidthInches * INCHES_TO_METERS
        );

        ApplyScaleWithDebug(obj, targetSizeMeters);
    }

    private void ApplyScaleWithDebug(GameObject targetObj, Vector3 targetSize)
    {
        if (!TryGetAccurateBounds(targetObj, out Bounds bounds)) return;

        Vector3 currentSize = bounds.size;
        if (currentSize.x <= 0f || currentSize.y <= 0f || currentSize.z <= 0f) return;

        Vector3 scaleFactors = new Vector3(targetSize.x / currentSize.x, targetSize.y / currentSize.y, targetSize.z / currentSize.z);
        Vector3 current = targetObj.transform.localScale;
        
        targetObj.transform.localScale = new Vector3(current.x * scaleFactors.x, current.y * scaleFactors.y, current.z * scaleFactors.z);
        Physics.SyncTransforms();
    }

    private bool TryGetAccurateBounds(GameObject obj, out Bounds bounds)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        bool initialized = false;
        bounds = new Bounds();

        foreach (Renderer r in renderers)
        {
            if (r is ParticleSystemRenderer || r is LineRenderer || r is TrailRenderer) continue;
            if (!r.enabled || !r.gameObject.activeInHierarchy) continue;

            if (!initialized)
            {
                bounds = r.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }

        return initialized;
    }

    private void PlaceObjectOnTopOfSurface(GameObject obj, Vector3 surfacePoint, Vector3 surfaceNormal)
    {
        if (obj == null) return;

        Vector3 normal = surfaceNormal.normalized;
        if (normal.sqrMagnitude < 0.0001f)
            normal = Vector3.up;

        // Make sure renderer bounds are updated after scaling / rotation.
        Physics.SyncTransforms();

        if (!TryGetAccurateBounds(obj, out Bounds bounds))
        {
            obj.transform.position = surfacePoint + normal * surfaceOffset;
            return;
        }

        // Find where the bottom of the object's world-space bounds is along the surface normal.
        float bottomDistanceAlongNormal =
            Vector3.Dot(bounds.center, normal) -
            (
                Mathf.Abs(Vector3.Dot(bounds.extents.x * Vector3.right, normal)) +
                Mathf.Abs(Vector3.Dot(bounds.extents.y * Vector3.up, normal)) +
                Mathf.Abs(Vector3.Dot(bounds.extents.z * Vector3.forward, normal))
            );

        float surfaceDistanceAlongNormal = Vector3.Dot(surfacePoint, normal);

        // Move object so bottom touches the surface.
        float correction = surfaceDistanceAlongNormal - bottomDistanceAlongNormal;

        obj.transform.position += normal * correction;

        // Optional tiny offset to prevent z-fighting. Keep this at 0 unless needed.
        obj.transform.position += normal * surfaceOffset;

        Physics.SyncTransforms();
    }
    private float GetJsonLength(MeshEntry mesh)
    {
        return mesh != null && mesh.standardized_dimensions != null ? mesh.standardized_dimensions.l : 0f;
    }

    private float GetJsonWidth(MeshEntry mesh)
    {
        return mesh != null && mesh.standardized_dimensions != null ? mesh.standardized_dimensions.w : 0f;
    }

    private float GetJsonHeight(MeshEntry mesh)
    {
        return mesh != null && mesh.standardized_dimensions != null ? mesh.standardized_dimensions.h : 0f;
    }
}
