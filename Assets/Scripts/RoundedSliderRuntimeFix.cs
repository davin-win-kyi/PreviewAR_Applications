using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to the same GameObject as BoxScaleUI or to the slider panel root.
/// It reapplies rounded slider visuals at runtime, which helps when another script,
/// layout rebuild, or prefab override makes the sliders look square during Play Mode.
/// </summary>
public class RoundedSliderRuntimeFix : MonoBehaviour
{
    [Header("Sliders")]
    public Slider xSlider;
    public Slider ySlider;
    public Slider zSlider;

    [Header("Optional Rounded Sprites")]
    [Tooltip("Use a rounded/sliced bar sprite. If null, the current slider Background/Fill sprite is preserved.")]
    public Sprite roundedBarSprite;

    [Tooltip("Use a rounded/sliced handle sprite. If null, the current slider Handle sprite is preserved.")]
    public Sprite roundedHandleSprite;

    [Header("Sizing")]
    public float barHeight = 14f;
    public Vector2 handleSize = new Vector2(26f, 26f);

    [Header("Behavior")]
    public bool applyOnStart = true;
    public bool applyOnEnable = true;
    public bool keepApplyingForFirstFrames = true;
    public int framesToReapply = 8;

    private readonly List<Slider> sliders = new List<Slider>();

    private void Awake()
    {
        CacheSliders();
    }

    private void OnEnable()
    {
        CacheSliders();

        if (applyOnEnable)
            ApplyAllRoundedSliderVisuals();

        if (keepApplyingForFirstFrames)
            StartCoroutine(ReapplyForFirstFrames());
    }

    private void Start()
    {
        CacheSliders();

        if (applyOnStart)
            ApplyAllRoundedSliderVisuals();
    }

    private System.Collections.IEnumerator ReapplyForFirstFrames()
    {
        int count = Mathf.Max(1, framesToReapply);

        for (int i = 0; i < count; i++)
        {
            ApplyAllRoundedSliderVisuals();
            yield return null;
        }
    }

    private void CacheSliders()
    {
        sliders.Clear();

        if (xSlider != null) sliders.Add(xSlider);
        if (ySlider != null) sliders.Add(ySlider);
        if (zSlider != null) sliders.Add(zSlider);

        // Fallback: if fields were not assigned, find sliders under this panel.
        if (sliders.Count == 0)
        {
            Slider[] found = GetComponentsInChildren<Slider>(true);
            sliders.AddRange(found);
        }
    }

    [ContextMenu("Apply Rounded Slider Visuals")]
    public void ApplyAllRoundedSliderVisuals()
    {
        CacheSliders();

        foreach (Slider slider in sliders)
        {
            ApplyRoundedSliderVisuals(slider);
        }
    }

    private void ApplyRoundedSliderVisuals(Slider slider)
    {
        if (slider == null)
            return;

        ApplyBarImage(slider.targetGraphic as Image);

        Transform background = slider.transform.Find("Background");
        if (background != null)
            ApplyBarImage(background.GetComponent<Image>());

        if (slider.fillRect != null)
            ApplyBarImage(slider.fillRect.GetComponent<Image>());

        if (slider.handleRect != null)
            ApplyHandleImage(slider.handleRect.GetComponent<Image>());
    }

    private void ApplyBarImage(Image image)
    {
        if (image == null)
            return;

        if (roundedBarSprite != null)
            image.sprite = roundedBarSprite;

        image.type = Image.Type.Sliced;
        image.preserveAspect = false;
        image.raycastTarget = true;

        RectTransform rect = image.rectTransform;
        if (rect != null && barHeight > 0f)
        {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, barHeight);
        }
    }

    private void ApplyHandleImage(Image image)
    {
        if (image == null)
            return;

        if (roundedHandleSprite != null)
            image.sprite = roundedHandleSprite;

        image.type = Image.Type.Sliced;
        image.preserveAspect = false;
        image.raycastTarget = true;

        RectTransform rect = image.rectTransform;
        if (rect != null && handleSize.x > 0f && handleSize.y > 0f)
        {
            rect.sizeDelta = handleSize;
        }
    }
}
