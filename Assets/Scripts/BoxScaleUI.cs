using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using TMPro;

// Modern UI Pack v5.5+ namespace
using Michsky.MUIP;

// For Modern UI Pack v5.4 or older, comment out Michsky.MUIP above
// and uncomment this older namespace instead:
// using Michsky.UI.ModernUIPack;

public class BoxScaleUI : MonoBehaviour{public enum InteractionMode{None,Draw,Erase}

[Header("Generated Rounded Sprites")]
public bool useGeneratedRoundedSprites = true;
public int generatedSpriteSize = 64;
public float generatedSpritePixelsPerUnit = 100f;

private Sprite roundedButtonSprite;
private Sprite roundedPanelSprite;
private Sprite roundedSliderSprite;
private Sprite circularHandleSprite;

[Header("Modern UI Style")]
public bool applyModernStyleOnStart = true;

public Color panelColor = new Color(0.025f, 0.030f, 0.045f, 0.97f);

public Color primaryButtonColor = new Color(0.10f, 0.38f, 1.00f, 1f);
public Color secondaryButtonColor = new Color(0.11f, 0.13f, 0.18f, 1f);
public Color dangerButtonColor = new Color(0.68f, 0.04f, 0.07f, 1f);

public Color buttonTextColor = new Color(1f, 1f, 1f, 0.98f);

public Color sliderFillColor = new Color(0.45f, 0.45f, 0.45f, 1f);
public Color sliderBackgroundColor = new Color(0.75f, 0.75f, 0.75f, 0.45f);
public Color sliderHandleColor = new Color(0.35f, 0.35f, 0.35f, 1f);

public Color dropdownColor = new Color(0.09f, 0.105f, 0.145f, 1f);
public Color dropdownTextColor = new Color(0.94f, 0.97f, 1f, 0.96f);

public float buttonHeight = 54f;
public int buttonFontSize = 18;
public float buttonPressScale = 0.96f;

[Header("Fixed MUIP Button Shape")]
[Tooltip("When true, BoxScaleUI forces MUIP buttons to a fixed width/height so text length does not resize the button.")]
public bool forceFixedButtonSize = true;

[Tooltip("Default width used when a button does not match one of the more specific categories below.")]
public float defaultButtonWidth = 180f;

[Tooltip("Width for Draw and Erase buttons.")]
public float modeButtonWidth = 170f;

[Tooltip("Width for Place button.")]
public float placeButtonWidth = 210f;

[Tooltip("Width for Previous, Next, and Confirm buttons.")]
public float candidateButtonWidth = 165f;

[Tooltip("Extra width kept inside the text object so long text ellipsizes instead of resizing the button.")]
public float buttonTextHorizontalPadding = 24f;

[Tooltip("When true, the Normal/Highlighted/Disabled/Ripple visual children are stretched to match the root button size.")]
public bool resizeMUIPVisualLayers = true;

[Tooltip("When true, button Visual Layer Images are forced to sliced rounded sprites, preserving the pill shape when stretched.")]
public bool forceSlicedButtonVisuals = true;

[Tooltip("When true, BoxScaleUI does not overwrite the existing MUIP button background colors. This keeps your original Modern UI Pack colors instead of forcing black/red/blue/etc.")]
public bool preserveOriginalMUIPButtonColors = true;

[Tooltip("When preserveOriginalMUIPButtonColors is true, this controls whether BoxScaleUI may still replace button sprites with generated rounded sprites. Keep false to preserve the original MUIP sprite/material look.")]
public bool replaceButtonSpritesWhenPreservingColors = false;

[Tooltip("When true, the root Image on a button is recolored. Keep false with MUIP buttons if the visible color comes from Normal/Highlighted/Disabled child layers.")]
public bool recolorButtonRootImage = false;

[Tooltip("When true, fallback Unity Button ColorBlock values are overwritten. Keep false when preserving MUIP's original theme colors.")]
public bool overwriteUnityButtonColorBlock = false;

[Tooltip("Names of MUIP child objects that represent visual button states.")]
public string[] muipButtonVisualLayerNames = new string[]
{
    "Normal",
    "Highlighted",
    "Disabled",
    "Ripple"
};

[Header("Modern Button Border")]
public bool useButtonBorders = true;
public Color buttonBorderColor = new Color(1f, 1f, 1f, 0.24f);
public Vector2 buttonBorderDistance = new Vector2(2f, -2f);
public float moreRoundedButtonRadius = 26f;

[Header("Dynamic Slider Colors")]
public bool useDynamicSliderColors = false;

public Color smallSizeColor = new Color(0.45f, 0.45f, 0.45f, 1f);
public Color mediumSizeColor = new Color(0.45f, 0.45f, 0.45f, 1f);
public Color largeSizeColor = new Color(0.45f, 0.45f, 0.45f, 1f);

public Color sliderHandleGlowColor = new Color(0.45f, 0.45f, 0.45f, 1f);
public float sliderColorLerpSpeed = 12f;

[Header("Modern Text Style")]
public Color primaryTextColor = new Color(1f, 1f, 1f, 0.98f);
public Color secondaryTextColor = new Color(0.80f, 0.86f, 0.94f, 0.86f);

public int dimensionTextFontSize = 15;
public int validOptionsFontSize = 100;
public int dropdownFontSize = 26;
public float textCharacterSpacing = 0.5f;

[Header("Global Text Modernization")]
public bool styleAllChildText = false;

public Color headingTextColor = new Color(1f, 1f, 1f, 0.98f);
public Color bodyTextColor = new Color(0.86f, 0.90f, 0.96f, 0.88f);
public Color mutedTextColor = new Color(0.70f, 0.76f, 0.84f, 0.78f);

public int headingFontSize = 22;
public int bodyFontSize = 16;
public int smallFontSize = 14;

public float modernTextSpacing = 1.4f;

[Header("Modern Layout Sizing")]
public float dropdownHeight = 52f;
public float sliderHeight = 44f;
public float sliderTrackHeight = 8f;
public float sliderHandleSize = 30f;

[Header("MUIP Slider Sizing / Interaction")]
[Tooltip("Width of each Modern UI Pack slider root row. Increase this if the sliders look too short.")]
public float muipSliderRowWidth = 460f;

[Tooltip("Width of the actual Unity Slider component inside the Modern UI Pack slider prefab.")]
public float muipSliderControlWidth = 360f;

[Tooltip("Height of the actual Unity Slider component inside the Modern UI Pack slider prefab.")]
public float muipSliderControlHeight = 44f;

[Tooltip("Forces Slider.interactable and relevant CanvasGroup states on when the resize panel is visible.")]
public bool forceMUIPSlidersInteractable = true;

[Tooltip("Disables raycast targets on TMP text inside slider prefabs so labels do not block slider dragging.")]
public bool disableSliderTextRaycastTargets = true;

[Header("MUIP Slider Runtime Fix")]
[Tooltip("When true, BoxScaleUI applies a safe runtime fix to the outer Modern UI Pack slider root.")]
public bool fixMUIPSliderRuntimeSize = true;

[Tooltip("Width of the whole Modern UI Pack slider root object/row. Only the outer root is resized.")]
public float muipSliderRootWidth = 430f;

[Tooltip("Height of the whole Modern UI Pack slider root object/row. Only the outer root is resized.")]
public float muipSliderRootHeight = 72f;

[Tooltip("Keep true for MUIP sliders. This prevents BoxScaleUI from resizing Background, Fill, Fill Area, Handle Slide Area, etc.")]
public bool preserveMUIPSliderInternalLayout = true;

[Tooltip("When true, TMP text labels inside the slider will not block pointer dragging.")]
public bool disableTMPTextRaycastsForSliders = true;


[Header("Rounded MUIP Slider Style")]
[Tooltip("When true, BoxScaleUI gives MUIP slider tracks/fills rounded sliced sprites and circular handle sprites.")]
public bool styleMUIPSlidersRounded = true;

[Tooltip("Height of the rounded slider track/background and fill.")]
public float roundedSliderTrackHeight = 18f;

[Tooltip("Square size of the moving slider handle. Equal width/height keeps it circular.")]
public float roundedSliderHandleSize = 42f;

[Tooltip("Corner radius, in pixels, for generated track/fill sprite.")]
public int roundedSliderTrackCornerRadius = 18;

[Tooltip("Corner radius, in pixels, for generated handle sprite. Use about half generatedSpriteSize for a circle.")]
public int roundedSliderHandleCornerRadius = 32;

public Color roundedSliderBackgroundColor = new Color(0.75f, 0.75f, 0.75f, 0.45f);
public Color roundedSliderFillColor = new Color(0.45f, 0.45f, 0.45f, 1f);
public Color roundedSliderHandleColor = new Color(0.35f, 0.35f, 0.35f, 1f);

[Header("MUIP Slider Position Offset")]
[Tooltip("When true, shifts the Modern UI Pack slider roots left at runtime without accumulating the offset each time the UI refreshes.")]
public bool shiftMUIPSlidersLeft = true;

[Tooltip("Negative values move sliders left. Try -40 to -90 depending on your canvas layout.")]
public float muipSliderLeftShift = -55f;

[Header("MUIP Slider Circular Handle Fix")]
[Tooltip("When true, forces both the Slider.handleRect and child handle images to a square size so the handle looks circular instead of oval.")]
public bool forceCircularSliderHandle = true;

[Tooltip("Square width/height for the slider handle.")]
public float circularSliderHandleSize = 44f;

[Header("Fitted Box Visibility")]
[Tooltip("If true, keeps the fitted 3D box visible while the user adjusts the sliders. If false, hides it so it cannot appear as a black box in front of the UI.")]
public bool showFittedBoxDuringResizePanel = true;

[Header("Gemini / Slider Synchronization")]
[Tooltip("When true, after ShowForBox the script waits a few frames and forces slider/value text sync again. This helps with Modern UI Pack sliders that refresh after activation.")]
public bool delayedForceSyncAfterShowForBox = true;

[Tooltip("How many frames to keep re-syncing sliders after the Gemini box appears.")]
public int delayedForceSyncFrames = 3;

[Header("Panel Layout Behavior")]
[Tooltip("Keep this false when your UI elements are manually positioned in the Canvas. If true, BoxScaleUI may add layout groups that can move children to the top/center.")]
public bool autoAddPanelLayoutGroups = false;

[Header("Panel Background Behavior")]
[Tooltip("When false, BoxScaleUI will not add Image backgrounds to container objects such as DrawOrErase or CandidateButtons.")]
public bool addBackgroundImagesToUIPanels = false;

[Tooltip("When true, only the resize panel can receive a generated background Image. DrawOrErase and CandidateButtons stay transparent.")]
public bool addBackgroundToResizePanelOnly = false;

[Tooltip("When true, accidental Image components on transparent container groups are disabled so they cannot appear as floating black/white boxes.")]
public bool disableAccidentalContainerImages = true;

[Header("Dynamic UI Animation")]
public bool usePanelFadeScaleAnimation = false;
public float panelAnimationSpeed = 14f;
public float panelStartScale = 0.94f;

[Header("Confirm Box Hold Animation")]
public bool keepBoxVisibleAfterConfirm = true;
public float boxHoldSecondsAfterConfirm = 5f;
public float confirmedBoxPulseSpeed = 4f;
public float confirmedBoxPulseScaleAmount = 0.035f;
public Color confirmedBoxPulseColor = new Color(0.55f, 1f, 0.58f, 0.85f);
public Color confirmedBoxBrightColor = new Color(0.90f, 1f, 0.95f, 1f);
public bool destroyConfirmedBoxAfterHold = true;

[Header("Mode Selection UI")]
public GameObject modeSelectionPanel;

[Header("UI References")]
public GameObject panel;
public GameObject candidateButtonsParent;

[Tooltip("The large bottom background/container panel behind the UI. This is disabled during draw/erase/loading modes.")]
public GameObject bottomPanelRoot;

[Header("Bottom Panel Visibility")]
[Tooltip("When true, the bottom background panel is hidden only while the user is actively drawing/erasing. It reappears after a fitted box is generated so sliders are visible.")]
public bool hideBottomPanelWhileDrawingOrErasing = true;

[Tooltip("When true, loading states such as candidate/3D generation loading hide the bottom panel background.")]
public bool hideBottomPanelDuringLoadingStates = true;

[Header("Legacy Unity UI Fallbacks - Hidden")]
[HideInInspector] public Button drawModeButton;
[HideInInspector] public Button eraseModeButton;
[HideInInspector] public Button loadOptionsButton;
[HideInInspector] public Button confirmButton;
[HideInInspector] public Button previousButton;
[HideInInspector] public Button nextButton;

[HideInInspector] public Slider xSlider;
[HideInInspector] public Slider ySlider;
[HideInInspector] public Slider zSlider;

[HideInInspector] public TMP_Dropdown objectTypeDropdown;

[Header("Modern UI Pack Root Objects")]
[Tooltip("Assign the root objects from the Modern UI Pack prefabs here. The script will search inside them for Button, Slider, TMP_Dropdown, and TMP_Text components.")]
public GameObject modernModeSelectionPanelRoot;
public GameObject modernDrawButtonRoot;
public GameObject modernEraseButtonRoot;

public GameObject modernPanelRoot;
public GameObject modernCandidateButtonsRoot;

public GameObject modernXSliderRoot;
public GameObject modernYSliderRoot;
public GameObject modernZSliderRoot;

public GameObject modernPlaceButtonRoot;
public GameObject modernConfirmButtonRoot;
public GameObject modernPreviousButtonRoot;
public GameObject modernNextButtonRoot;

public GameObject modernDropdownRoot;

[Header("Modern UI Pack Direct Components")]
[Tooltip("Use these for Michsky Modern UI Pack buttons. If you assign root objects above, these are auto-found.")]
public ButtonManager drawModeButtonMUIP;
public ButtonManager eraseModeButtonMUIP;
public ButtonManager loadOptionsButtonMUIP;
public ButtonManager confirmButtonMUIP;
public ButtonManager previousButtonMUIP;
public ButtonManager nextButtonMUIP;

[Tooltip("Use this for Michsky Modern UI Pack CustomDropdown. If you assign modernDropdownRoot, it is auto-found.")]
public CustomDropdown objectTypeDropdownMUIP;

[Header("Modern UI Pack Sliders")]
[Tooltip("Assign the MUIP SliderManager components here. If you assign root objects above, these are auto-found.")]
public SliderManager xSliderMUIP;
public SliderManager ySliderMUIP;
public SliderManager zSliderMUIP;

[Header("Modern UI Dropdown Options")]
[Tooltip("When true, the CustomDropdown items are generated from objectTypeDropdownOptions on Start.")]
public bool populateDropdownFromList = true;

[Tooltip("When true, existing CustomDropdown items are cleared before adding objectTypeDropdownOptions.")]
public bool clearExistingDropdownItems = true;

[Tooltip("Set these in the Inspector. These become the selectable object types in the Modern UI Pack dropdown.")]
public List<string> objectTypeDropdownOptions = new List<string>
{
    "bed",
    "chair",
    "table",
    "sofa",
    "lamp",
    "special"
};

[Header("Modern UI Pack Slider Rows")]
[Tooltip("Use this when each Modern UI slider prefab contains both the text label and the slider under the same root.")]
public bool autoFindSliderTextsFromRows = false;
public bool forceSliderRowsHorizontalLayout = false;
public float sliderRowHeight = 54f;
public float sliderLabelWidth = 105f;
public float sliderRowSpacing = 10f;

[Header("Modern UI Dropdown Fallback")]
[Tooltip("Used when the Modern UI Pack dropdown is not a TMP_Dropdown. Call SetSelectedObjectTypeFromModernDropdown from the dropdown option events.")]
public string selectedObjectTypeFromModernDropdown = "bed";

[Header("Dimension / Options Text")]
public TMP_Text xDimensionText;
public TMP_Text yDimensionText;
public TMP_Text zDimensionText;
public TMP_Text validOptionsText;

[Header("Dimension Text Settings")]
[Tooltip("Set this false when the Modern UI Pack slider prefab already contains its own text/value label. Then xDimensionText/yDimensionText/zDimensionText can be left empty.")]
public bool useSeparateDimensionText = false;

[Header("Dimension / Valid Options Emphasis")]
[Tooltip("When true, width/height/depth and valid-options text are rendered larger and bold.")]
public bool emphasizeDimensionAndOptionsText = true;
public int emphasizedDimensionFontSize = 24;
public int emphasizedValidOptionsFontSize = 100;
public bool useLengthLabelForZDimension = true;

[Tooltip("When useSeparateDimensionText is false, this updates the text inside each MUIP SliderManager prefab instead.")]
public bool updateMUIPSliderText = true;

public bool showDimensionsInInches = true;

[Header("MUIP Slider Text Display")]
public float muipSliderTextWidth = 230f;
public int muipSliderTextFontSize = 22;

[Header("Compact Bottom UI Overrides")]
[Tooltip("When true, BoxScaleUI applies smaller runtime values for the MUIP sliders and Valid Options text. This is useful for a compact bottom panel.")]
public bool useCompactBottomUIStyle = true;

public float compactMUIPSliderRootWidth = 340f;
public float compactMUIPSliderRootHeight = 48f;
public float compactRoundedSliderTrackHeight = 6f;
public float compactRoundedSliderHandleSize = 18f;
public float compactCircularSliderHandleSize = 18f;
public float compactMUIPSliderTextWidth = 220f;
public int compactMUIPSliderTextFontSize = 16;
public int compactDimensionFontSize = 16;
public int compactValidOptionsFontSize = 24;
public Vector2 compactValidOptionsSize = new Vector2(420f, 38f);
public Color sliderTextColor = new Color(0.16f, 0.16f, 0.16f, 1f);
public Color validOptionsCompactTextColor = new Color(0.12f, 0.12f, 0.12f, 1f);

[Tooltip("When true, the script reapplies slider label color every frame so Modern UI Pack cannot repaint it white.")]
public bool forceSliderTextColorEveryFrame = true;

[Header("Dropdown Defaults")]
public string defaultObjectType = "bed";

[Header("Scene References")]
public LassoDrawer lassoDrawer;

[Header("Lasso Cleanup Behavior")]
[Tooltip("Keep this false for MUIP mode-selection UI. LassoDrawer.ClearCurrentCandidates() reactivates the fittedBox, which can appear as a black rounded box in front of the camera.")]
public bool clearLassoCandidatesWhenReturningToModeSelection = false;

[Header("Slider Ranges")]
public float minScale = 0.01f;
public float maxScale = 3.0f;

private const float METERS_TO_INCHES = 39.3701f;

private Transform targetBox;
private bool suppressCallbacks = false;
private bool candidatesActive = false;
private Coroutine confirmBoxHoldRoutine;

private readonly Dictionary<Slider, Color> currentSliderColors =
    new Dictionary<Slider, Color>();

private readonly Dictionary<RectTransform, Vector2> originalSliderAnchoredPositions =
    new Dictionary<RectTransform, Vector2>();

public InteractionMode currentMode = InteractionMode.None;

public bool CandidatesActive => candidatesActive;

private IEnumerator Start()
{
    Debug.Log("BoxScaleUI Start");

    ResolveModernUIReferences();
    PopulateModernDropdownOptions();

    if (modeSelectionPanel != null)
        modeSelectionPanel.SetActive(true);

    if (panel != null)
        panel.SetActive(false);

    if (candidateButtonsParent != null)
        candidateButtonsParent.SetActive(false);

    ConfigureSlider(xSlider);
    ConfigureSlider(ySlider);
    ConfigureSlider(zSlider);

    if (xSlider != null)
        xSlider.onValueChanged.AddListener(OnXSliderChanged);

    if (ySlider != null)
        ySlider.onValueChanged.AddListener(OnYSliderChanged);

    if (zSlider != null)
        zSlider.onValueChanged.AddListener(OnZSliderChanged);

    RegisterButtonListeners();

    if (objectTypeDropdown != null)
        objectTypeDropdown.onValueChanged.AddListener(OnDropdownChanged);

    if (objectTypeDropdownMUIP != null)
        objectTypeDropdownMUIP.onValueChanged.AddListener(OnDropdownChanged);

    yield return null;

    SetDefaultDropdownOption(defaultObjectType);

    if (applyModernStyleOnStart)
    {
        ApplyModernUIStyle();
    }

    ShowModeSelectionOnly();
}

private void Update()
{
    // Keep the fill/handle colors stable when dynamic colors are disabled.
    // This also helps Modern UI Pack prefabs that refresh their slider visuals at runtime.
    UpdateSliderVisuals();

    if (forceSliderTextColorEveryFrame)
    {
        ForceAllSliderTextColors();
    }

    ForceAllDropdownTextSizes();
}

private void ApplyModernUIStyle()
{
    ApplyCompactBottomUIOverrides();

    if (useGeneratedRoundedSprites)
    {
        CreateGeneratedUISprites();
    }

    StylePanel(panel);
    StylePanel(modeSelectionPanel);
    StylePanel(candidateButtonsParent);

    // Prefer Modern UI Pack button managers when present.
    StylePrimaryButton(loadOptionsButtonMUIP, "Complete");
    StylePrimaryButton(confirmButtonMUIP, "Confirm");

    StyleSecondaryButton(previousButtonMUIP, "Previous");
    StyleSecondaryButton(nextButtonMUIP, "Next");

    StyleSecondaryButton(drawModeButtonMUIP, "Draw");
    StyleDangerButton(eraseModeButtonMUIP, "Erase");

    // Legacy fallback buttons, used only if the prefab also exposes normal Unity Button components.
    StylePrimaryButton(loadOptionsButton, "Complete");
    StylePrimaryButton(confirmButton, "Confirm");

    StyleSecondaryButton(previousButton, "Previous");
    StyleSecondaryButton(nextButton, "Next");

    StyleSecondaryButton(drawModeButton, "Draw");
    StyleDangerButton(eraseModeButton, "Erase");

    // For Modern UI Pack sliders, preserve the prefab's internal layout.
    // Only fall back to StyleSlider for plain Unity sliders without SliderManager.
    if (xSliderMUIP == null) StyleSlider(xSlider);
    if (ySliderMUIP == null) StyleSlider(ySlider);
    if (zSliderMUIP == null) StyleSlider(zSlider);

    if (useSeparateDimensionText)
    {
        StyleSliderRow(modernXSliderRoot, xSlider, xDimensionText);
        StyleSliderRow(modernYSliderRoot, ySlider, yDimensionText);
        StyleSliderRow(modernZSliderRoot, zSlider, zDimensionText);
    }

    FixMUIPSliderInteractionAndSize(modernXSliderRoot, xSliderMUIP, xSlider);
    FixMUIPSliderInteractionAndSize(modernYSliderRoot, ySliderMUIP, ySlider);
    FixMUIPSliderInteractionAndSize(modernZSliderRoot, zSliderMUIP, zSlider);

    StyleRoundedMUIPSlider(modernXSliderRoot, xSliderMUIP, xSlider);
    StyleRoundedMUIPSlider(modernYSliderRoot, ySliderMUIP, ySlider);
    StyleRoundedMUIPSlider(modernZSliderRoot, zSliderMUIP, zSlider);

    StyleDropdown(objectTypeDropdownMUIP);
    StyleDropdown(objectTypeDropdown);

    if (useSeparateDimensionText)
    {
        StyleInfoText(xDimensionText, dimensionTextFontSize);
        StyleInfoText(yDimensionText, dimensionTextFontSize);
        StyleInfoText(zDimensionText, dimensionTextFontSize);
    }
    else
    {
        StyleMUIPSliderText(xSliderMUIP);
        StyleMUIPSliderText(ySliderMUIP);
        StyleMUIPSliderText(zSliderMUIP);
    }

    StyleValidOptionsText();

    if (styleAllChildText)
    {
        StyleAllTextUnder(panel);
        StyleAllTextUnder(modeSelectionPanel);
        StyleAllTextUnder(candidateButtonsParent);
    }

    if (usePanelFadeScaleAnimation)
    {
        AddFadeScaleAnimation(panel);
        AddFadeScaleAnimation(modeSelectionPanel);
        AddFadeScaleAnimation(candidateButtonsParent);
    }

    UpdateSliderVisuals();

    Debug.Log("Modern UI style applied.");
}

private void StyleValidOptionsText()
{
    if (validOptionsText == null)
        return;

    validOptionsText.gameObject.SetActive(true);
    validOptionsText.richText = true;

    validOptionsText.enableAutoSizing = false;
    validOptionsText.fontSize = emphasizeDimensionAndOptionsText
        ? emphasizedValidOptionsFontSize
        : validOptionsFontSize;

    validOptionsText.fontStyle = FontStyles.Bold;
    validOptionsText.color = useCompactBottomUIStyle ? validOptionsCompactTextColor : new Color(0.15f, 0.16f, 0.20f, 1f);
    validOptionsText.alignment = TextAlignmentOptions.Center;
    validOptionsText.enableWordWrapping = false;
    validOptionsText.overflowMode = TextOverflowModes.Overflow;
    validOptionsText.characterSpacing = textCharacterSpacing;

    validOptionsText.transform.localScale = Vector3.one;

    RectTransform rect = validOptionsText.GetComponent<RectTransform>();
    if (rect != null)
    {
        rect.localScale = Vector3.one;
        rect.sizeDelta = useCompactBottomUIStyle ? compactValidOptionsSize : new Vector2(650f, 120f);
    }

    LayoutElement layout = validOptionsText.GetComponent<LayoutElement>();
    if (layout == null)
        layout = validOptionsText.gameObject.AddComponent<LayoutElement>();

    layout.ignoreLayout = false;
    layout.preferredWidth = useCompactBottomUIStyle ? compactValidOptionsSize.x : 650f;
    layout.minWidth = useCompactBottomUIStyle ? compactValidOptionsSize.x : 650f;
    layout.preferredHeight = useCompactBottomUIStyle ? compactValidOptionsSize.y : 120f;
    layout.minHeight = useCompactBottomUIStyle ? compactValidOptionsSize.y : 120f;
    layout.flexibleWidth = 0f;
    layout.flexibleHeight = 0f;
}

private void ResolveModernUIReferences()
{
    // Root panels
    if (modernModeSelectionPanelRoot != null)
        modeSelectionPanel = modernModeSelectionPanelRoot;

    if (modernPanelRoot != null)
        panel = modernPanelRoot;

    if (modernCandidateButtonsRoot != null)
        candidateButtonsParent = modernCandidateButtonsRoot;

    // Modern UI Pack ButtonManager components
    drawModeButtonMUIP = ResolveComponentFromRoot(modernDrawButtonRoot, drawModeButtonMUIP);
    eraseModeButtonMUIP = ResolveComponentFromRoot(modernEraseButtonRoot, eraseModeButtonMUIP);
    loadOptionsButtonMUIP = ResolveComponentFromRoot(modernPlaceButtonRoot, loadOptionsButtonMUIP);
    confirmButtonMUIP = ResolveComponentFromRoot(modernConfirmButtonRoot, confirmButtonMUIP);
    previousButtonMUIP = ResolveComponentFromRoot(modernPreviousButtonRoot, previousButtonMUIP);
    nextButtonMUIP = ResolveComponentFromRoot(modernNextButtonRoot, nextButtonMUIP);

    // Fallback native Button references. Many MUIP prefabs still include a Unity Button.
    drawModeButton = ResolveComponentFromRoot(modernDrawButtonRoot, drawModeButton);
    eraseModeButton = ResolveComponentFromRoot(modernEraseButtonRoot, eraseModeButton);
    loadOptionsButton = ResolveComponentFromRoot(modernPlaceButtonRoot, loadOptionsButton);
    confirmButton = ResolveComponentFromRoot(modernConfirmButtonRoot, confirmButton);
    previousButton = ResolveComponentFromRoot(modernPreviousButtonRoot, previousButton);
    nextButton = ResolveComponentFromRoot(modernNextButtonRoot, nextButton);

    // Modern UI Pack SliderManager components.
    // MUIP sliders expose their actual Unity slider through .mainSlider.
    xSliderMUIP = ResolveComponentFromRoot(modernXSliderRoot, xSliderMUIP);
    ySliderMUIP = ResolveComponentFromRoot(modernYSliderRoot, ySliderMUIP);
    zSliderMUIP = ResolveComponentFromRoot(modernZSliderRoot, zSliderMUIP);

    if (xSliderMUIP != null && xSliderMUIP.mainSlider != null)
        xSlider = xSliderMUIP.mainSlider;

    if (ySliderMUIP != null && ySliderMUIP.mainSlider != null)
        ySlider = ySliderMUIP.mainSlider;

    if (zSliderMUIP != null && zSliderMUIP.mainSlider != null)
        zSlider = zSliderMUIP.mainSlider;

    // Slider rows and labels. These roots can contain both the text label and SliderManager.
    ResolveSliderRow(modernXSliderRoot, ref xSlider, ref xDimensionText);
    ResolveSliderRow(modernYSliderRoot, ref ySlider, ref yDimensionText);
    ResolveSliderRow(modernZSliderRoot, ref zSlider, ref zDimensionText);

    // Modern UI Pack dropdown.
    objectTypeDropdownMUIP = ResolveComponentFromRoot(modernDropdownRoot, objectTypeDropdownMUIP);

    // Legacy fallback only. Prefer objectTypeDropdownMUIP.
    objectTypeDropdown = ResolveComponentFromRoot(modernDropdownRoot, objectTypeDropdown);
}

private T ResolveComponentFromRoot<T>(GameObject root, T current) where T : Component
{
    if (current != null)
        return current;

    if (root == null)
        return null;

    T component = root.GetComponent<T>();
    if (component != null)
        return component;

    return root.GetComponentInChildren<T>(true);
}

private void ResolveSliderRow(GameObject rowRoot, ref Slider slider, ref TMP_Text dimensionText)
{
    if (rowRoot == null)
        return;

    SliderManager sliderManager = rowRoot.GetComponent<SliderManager>();
    if (sliderManager == null)
        sliderManager = rowRoot.GetComponentInChildren<SliderManager>(true);

    if (sliderManager != null && sliderManager.mainSlider != null)
        slider = sliderManager.mainSlider;
    else
        slider = ResolveComponentFromRoot(rowRoot, slider);

    if (useSeparateDimensionText && autoFindSliderTextsFromRows && dimensionText == null)
        dimensionText = FindBestSliderText(rowRoot);

    if (forceSliderRowsHorizontalLayout && useSeparateDimensionText)
        ApplySliderRowLayout(rowRoot, slider, dimensionText);
}

private TMP_Text FindBestSliderText(GameObject sliderRoot)
{
    if (sliderRoot == null)
        return null;

    TMP_Text[] texts = sliderRoot.GetComponentsInChildren<TMP_Text>(true);
    foreach (TMP_Text text in texts)
    {
        if (text == null) continue;

        // Avoid grabbing dropdown item text. Button text is allowed only if the slider row itself is not a button.
        if (text.GetComponentInParent<TMP_Dropdown>() != null) continue;

        return text;
    }

    return texts.Length > 0 ? texts[0] : null;
}

private void ApplySliderRowLayout(GameObject rowRoot, Slider slider, TMP_Text dimensionText)
{
    if (rowRoot == null)
        return;

    HorizontalLayoutGroup layout = rowRoot.GetComponent<HorizontalLayoutGroup>();
    if (layout == null)
        layout = rowRoot.AddComponent<HorizontalLayoutGroup>();

    layout.padding = new RectOffset(0, 0, 0, 0);
    layout.spacing = sliderRowSpacing;
    layout.childAlignment = TextAnchor.MiddleCenter;
    layout.childControlWidth = true;
    layout.childControlHeight = true;
    layout.childForceExpandWidth = false;
    layout.childForceExpandHeight = false;

    LayoutElement rowLayout = rowRoot.GetComponent<LayoutElement>();
    if (rowLayout == null)
        rowLayout = rowRoot.AddComponent<LayoutElement>();

    rowLayout.preferredHeight = sliderRowHeight;
    rowLayout.minHeight = sliderRowHeight;

    if (dimensionText != null)
    {
        LayoutElement textLayout = dimensionText.GetComponent<LayoutElement>();
        if (textLayout == null)
            textLayout = dimensionText.gameObject.AddComponent<LayoutElement>();

        textLayout.preferredWidth = sliderLabelWidth;
        textLayout.minWidth = sliderLabelWidth;
        textLayout.flexibleWidth = 0f;

        dimensionText.alignment = TextAlignmentOptions.MidlineLeft;
        dimensionText.enableWordWrapping = false;
        dimensionText.overflowMode = TextOverflowModes.Ellipsis;
    }

    if (slider != null)
    {
        LayoutElement sliderLayout = slider.GetComponent<LayoutElement>();
        if (sliderLayout == null)
            sliderLayout = slider.gameObject.AddComponent<LayoutElement>();

        sliderLayout.preferredHeight = sliderHeight;
        sliderLayout.minHeight = sliderHeight;
        sliderLayout.flexibleWidth = 1f;
    }
}

private void PopulateModernDropdownOptions()
{
    if (!populateDropdownFromList || objectTypeDropdownMUIP == null)
        return;

    if (objectTypeDropdownOptions == null || objectTypeDropdownOptions.Count == 0)
    {
        Debug.LogWarning("BoxScaleUI: objectTypeDropdownOptions is empty. Add values in the Inspector.");
        return;
    }

    if (clearExistingDropdownItems && objectTypeDropdownMUIP.items != null)
        objectTypeDropdownMUIP.items.Clear();

    for (int i = 0; i < objectTypeDropdownOptions.Count; i++)
    {
        string option = objectTypeDropdownOptions[i];

        if (string.IsNullOrWhiteSpace(option))
            continue;

        bool initializeImmediately = false;
        objectTypeDropdownMUIP.CreateNewItem(option.Trim(), null, initializeImmediately);
    }

    objectTypeDropdownMUIP.SetupDropdown();
}

private void RegisterButtonListeners()
{
    AddButtonListener(loadOptionsButtonMUIP, loadOptionsButton, OnLoadOptionsClicked);
    AddButtonListener(confirmButtonMUIP, confirmButton, OnConfirmClicked);
    AddButtonListener(previousButtonMUIP, previousButton, OnPreviousClicked);
    AddButtonListener(nextButtonMUIP, nextButton, OnNextClicked);
    AddButtonListener(drawModeButtonMUIP, drawModeButton, EnterDrawMode);
    AddButtonListener(eraseModeButtonMUIP, eraseModeButton, EnterEraseMode);

    if (drawModeButtonMUIP == null)
        Debug.LogWarning("BoxScaleUI: Draw ButtonManager is not assigned. Assign drawModeButtonMUIP or modernDrawButtonRoot.");

    if (eraseModeButtonMUIP == null)
        Debug.LogWarning("BoxScaleUI: Erase ButtonManager is not assigned. Assign eraseModeButtonMUIP or modernEraseButtonRoot.");
}

private void AddButtonListener(ButtonManager muipButton, Button unityButton, UnityAction action)
{
    // Register on both the Modern UI Pack ButtonManager and the underlying
    // Unity Button when both exist. Some MUIP prefabs fire ButtonManager.onClick,
    // while others are wired through the normal Button component.
    if (muipButton != null)
    {
        muipButton.onClick.RemoveListener(action);
        muipButton.onClick.AddListener(action);
    }

    if (unityButton != null)
    {
        unityButton.onClick.RemoveListener(action);
        unityButton.onClick.AddListener(action);
    }
}

private void CreateGeneratedUISprites()
{
    roundedButtonSprite = CreateRoundedRectSprite(
        generatedSpriteSize,
        generatedSpriteSize,
        Mathf.RoundToInt(moreRoundedButtonRadius)
    );

    roundedPanelSprite = CreateRoundedRectSprite(
        generatedSpriteSize,
        generatedSpriteSize,
        24
    );

    roundedSliderSprite = CreateRoundedRectSprite(
        generatedSpriteSize,
        generatedSpriteSize,
        12
    );

    circularHandleSprite = CreateRoundedRectSprite(
        generatedSpriteSize,
        generatedSpriteSize,
        generatedSpriteSize / 2
    );
}

private Sprite CreateRoundedRectSprite(int width, int height, int radius)
{
    Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
    texture.name = "GeneratedRoundedUISprite";
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.filterMode = FilterMode.Bilinear;

    Color clear = new Color(1f, 1f, 1f, 0f);
    Color white = Color.white;

    radius = Mathf.Clamp(radius, 1, Mathf.Min(width, height) / 2);

    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            bool inside = IsInsideRoundedRect(x, y, width, height, radius);
            texture.SetPixel(x, y, inside ? white : clear);
        }
    }

    texture.Apply();

    Rect rect = new Rect(0, 0, width, height);
    Vector2 pivot = new Vector2(0.5f, 0.5f);
    Vector4 border = new Vector4(radius, radius, radius, radius);

    return Sprite.Create(
        texture,
        rect,
        pivot,
        generatedSpritePixelsPerUnit,
        0,
        SpriteMeshType.FullRect,
        border
    );
}

private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
{
    int left = radius;
    int right = width - radius - 1;
    int bottom = radius;
    int top = height - radius - 1;

    if (x >= left && x <= right) return true;
    if (y >= bottom && y <= top) return true;

    int cx = x < left ? left : right;
    int cy = y < bottom ? bottom : top;

    int dx = x - cx;
    int dy = y - cy;

    return dx * dx + dy * dy <= radius * radius;
}

private void StylePanel(GameObject panelObj)
{
    if (panelObj == null) return;

    bool shouldAddBackground = addBackgroundImagesToUIPanels;

    // Optional safer mode: only the resize controls panel gets a background.
    // Button containers like DrawOrErase and CandidateButtons should usually stay transparent.
    if (addBackgroundToResizePanelOnly)
    {
        shouldAddBackground = panelObj == panel;
    }

    Image image = panelObj.GetComponent<Image>();

    if (shouldAddBackground)
    {
        if (image == null)
            image = panelObj.AddComponent<Image>();

        image.enabled = true;
        image.color = panelColor;
        image.raycastTarget = false;

        if (roundedPanelSprite != null)
        {
            image.sprite = roundedPanelSprite;
            image.type = Image.Type.Sliced;
        }
    }
    else if (image != null && disableAccidentalContainerImages)
    {
        // Important:
        // DrawOrErase, CandidateButtons, and similar parent objects are layout containers.
        // They should not have visible Image components. If an Image was added by an older
        // BoxScaleUI version, disable it so it does not appear as a floating square/black box.
        image.raycastTarget = false;
        image.enabled = false;
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
    }

    // Important: do not auto-add layout groups by default.
    // Layout groups override manually placed RectTransforms and can make all
    // dropdowns, sliders, and buttons collapse toward the top/center at runtime.
    if (!autoAddPanelLayoutGroups)
        return;

    VerticalLayoutGroup verticalLayout = panelObj.GetComponent<VerticalLayoutGroup>();
    HorizontalLayoutGroup horizontalLayout = panelObj.GetComponent<HorizontalLayoutGroup>();

    if (verticalLayout == null && horizontalLayout == null)
    {
        verticalLayout = panelObj.AddComponent<VerticalLayoutGroup>();
        verticalLayout.padding = new RectOffset(20, 20, 20, 20);
        verticalLayout.spacing = 14f;
        verticalLayout.childAlignment = TextAnchor.MiddleCenter;
        verticalLayout.childControlWidth = true;
        verticalLayout.childControlHeight = false;
        verticalLayout.childForceExpandWidth = true;
        verticalLayout.childForceExpandHeight = false;
    }
    else if (verticalLayout != null)
    {
        verticalLayout.padding = new RectOffset(20, 20, 20, 20);
        verticalLayout.spacing = 14f;
        verticalLayout.childAlignment = TextAnchor.MiddleCenter;
    }
    else if (horizontalLayout != null)
    {
        horizontalLayout.padding = new RectOffset(20, 20, 20, 20);
        horizontalLayout.spacing = 12f;
        horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
    }
}

private void StylePrimaryButton(ButtonManager button, string fallbackText = null)
{
    StyleButton(button, primaryButtonColor, buttonTextColor, fallbackText);
}

private void StyleSecondaryButton(ButtonManager button, string fallbackText = null)
{
    StyleButton(button, secondaryButtonColor, buttonTextColor, fallbackText);
}

private void StyleDangerButton(ButtonManager button, string fallbackText = null)
{
    StyleButton(button, dangerButtonColor, buttonTextColor, fallbackText);
}

private void StylePrimaryButton(Button button, string fallbackText = null)
{
    StyleButton(button, primaryButtonColor, buttonTextColor, fallbackText);
}

private void StyleSecondaryButton(Button button, string fallbackText = null)
{
    StyleButton(button, secondaryButtonColor, buttonTextColor, fallbackText);
}

private void StyleDangerButton(Button button, string fallbackText = null)
{
    StyleButton(button, dangerButtonColor, buttonTextColor, fallbackText);
}

private void StyleButton(ButtonManager button, Color baseColor, Color textColor, string fallbackText = null)
{
    if (button == null) return;

    if (!string.IsNullOrWhiteSpace(fallbackText))
        button.SetText(fallbackText);

    StyleButtonVisuals(button.gameObject, baseColor, textColor, fallbackText);
    AddPressScaleEffect(button.gameObject);
}

private void StyleButtonVisuals(GameObject buttonObject, Color baseColor, Color textColor, string fallbackText = null)
{
    if (buttonObject == null) return;

    if (forceFixedButtonSize)
    {
        ApplyFixedPillButtonShape(buttonObject, baseColor);
    }

    Image image = buttonObject.GetComponent<Image>();
    if (image != null)
    {
        if (!preserveOriginalMUIPButtonColors && recolorButtonRootImage)
            image.color = baseColor;

        bool canReplaceSprite = !preserveOriginalMUIPButtonColors || replaceButtonSpritesWhenPreservingColors;

        if (roundedButtonSprite != null && canReplaceSprite)
        {
            image.sprite = roundedButtonSprite;
            image.type = Image.Type.Sliced;
        }
        else if (forceSlicedButtonVisuals && image.sprite != null)
        {
            image.type = Image.Type.Sliced;
        }
    }

    if (useButtonBorders)
    {
        Outline outline = buttonObject.GetComponent<Outline>();
        if (outline == null)
            outline = buttonObject.AddComponent<Outline>();

        outline.effectColor = buttonBorderColor;
        outline.effectDistance = buttonBorderDistance;
        outline.useGraphicAlpha = true;
    }

    LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
    if (layout == null)
        layout = buttonObject.AddComponent<LayoutElement>();

    float preferredWidth = forceFixedButtonSize
        ? GetPreferredButtonWidth(buttonObject)
        : Mathf.Max(defaultButtonWidth, layout.preferredWidth);

    layout.preferredWidth = preferredWidth;
    layout.minWidth = preferredWidth;
    layout.flexibleWidth = 0f;
    layout.preferredHeight = buttonHeight;
    layout.minHeight = buttonHeight;
    layout.flexibleHeight = 0f;

    RectTransform rect = buttonObject.GetComponent<RectTransform>();
    if (rect != null)
        rect.sizeDelta = new Vector2(preferredWidth, buttonHeight);

    ContentSizeFitter fitter = buttonObject.GetComponent<ContentSizeFitter>();
    if (fitter != null && forceFixedButtonSize)
        fitter.enabled = false;

    TMP_Text tmpText = buttonObject.GetComponentInChildren<TMP_Text>(true);
    if (tmpText != null)
    {
        if (!string.IsNullOrWhiteSpace(fallbackText))
            tmpText.text = fallbackText;

        StyleButtonText(tmpText, textColor);
        ForceButtonTextToFixedArea(tmpText, preferredWidth);
    }
}

private void ApplyFixedPillButtonShape(GameObject buttonObject, Color baseColor)
{
    if (buttonObject == null) return;

    float preferredWidth = GetPreferredButtonWidth(buttonObject);

    RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
    if (buttonRect != null)
    {
        buttonRect.sizeDelta = new Vector2(preferredWidth, buttonHeight);
    }

    LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
    if (layout == null)
        layout = buttonObject.AddComponent<LayoutElement>();

    layout.ignoreLayout = false;
    layout.minWidth = preferredWidth;
    layout.preferredWidth = preferredWidth;
    layout.flexibleWidth = 0f;
    layout.minHeight = buttonHeight;
    layout.preferredHeight = buttonHeight;
    layout.flexibleHeight = 0f;

    ContentSizeFitter fitter = buttonObject.GetComponent<ContentSizeFitter>();
    if (fitter != null)
        fitter.enabled = false;

    if (resizeMUIPVisualLayers)
        ResizeMUIPButtonVisualLayers(buttonObject, baseColor);
}

private float GetPreferredButtonWidth(GameObject buttonObject)
{
    if (buttonObject == null)
        return defaultButtonWidth;

    if (buttonObject == modernDrawButtonRoot || buttonObject == modernEraseButtonRoot)
        return modeButtonWidth;

    if (drawModeButtonMUIP != null && buttonObject == drawModeButtonMUIP.gameObject)
        return modeButtonWidth;

    if (eraseModeButtonMUIP != null && buttonObject == eraseModeButtonMUIP.gameObject)
        return modeButtonWidth;

    if (buttonObject == modernPlaceButtonRoot)
        return placeButtonWidth;

    if (loadOptionsButtonMUIP != null && buttonObject == loadOptionsButtonMUIP.gameObject)
        return placeButtonWidth;

    if (buttonObject == modernConfirmButtonRoot ||
        buttonObject == modernPreviousButtonRoot ||
        buttonObject == modernNextButtonRoot)
        return candidateButtonWidth;

    if (confirmButtonMUIP != null && buttonObject == confirmButtonMUIP.gameObject)
        return candidateButtonWidth;

    if (previousButtonMUIP != null && buttonObject == previousButtonMUIP.gameObject)
        return candidateButtonWidth;

    if (nextButtonMUIP != null && buttonObject == nextButtonMUIP.gameObject)
        return candidateButtonWidth;

    string lowerName = buttonObject.name.ToLowerInvariant();

    if (lowerName.Contains("draw") || lowerName.Contains("erase"))
        return modeButtonWidth;

    if (lowerName.Contains("place") || lowerName.Contains("load"))
        return placeButtonWidth;

    if (lowerName.Contains("previous") || lowerName.Contains("prev") ||
        lowerName.Contains("next") || lowerName.Contains("confirm"))
        return candidateButtonWidth;

    return defaultButtonWidth;
}

private void ResizeMUIPButtonVisualLayers(GameObject buttonObject, Color baseColor)
{
    if (buttonObject == null) return;

    foreach (Transform child in buttonObject.GetComponentsInChildren<Transform>(true))
    {
        if (child == null || child == buttonObject.transform)
            continue;

        if (!IsMUIPButtonVisualLayer(child.name))
            continue;

        RectTransform childRect = child.GetComponent<RectTransform>();
        if (childRect != null)
        {
            childRect.anchorMin = Vector2.zero;
            childRect.anchorMax = Vector2.one;
            childRect.pivot = new Vector2(0.5f, 0.5f);
            childRect.offsetMin = Vector2.zero;
            childRect.offsetMax = Vector2.zero;
            childRect.sizeDelta = Vector2.zero;
        }

        Image childImage = child.GetComponent<Image>();
        if (childImage != null)
        {
            if (!preserveOriginalMUIPButtonColors)
            {
                Color layerColor = GetButtonLayerColor(child.name, baseColor);
                childImage.color = layerColor;
            }

            bool canReplaceSprite = !preserveOriginalMUIPButtonColors || replaceButtonSpritesWhenPreservingColors;

            if (forceSlicedButtonVisuals && roundedButtonSprite != null && canReplaceSprite)
            {
                childImage.sprite = roundedButtonSprite;
                childImage.type = Image.Type.Sliced;
            }
            else if (forceSlicedButtonVisuals && childImage.sprite != null)
            {
                childImage.type = Image.Type.Sliced;
            }
        }
    }
}

private bool IsMUIPButtonVisualLayer(string objectName)
{
    if (string.IsNullOrWhiteSpace(objectName))
        return false;

    for (int i = 0; i < muipButtonVisualLayerNames.Length; i++)
    {
        string layerName = muipButtonVisualLayerNames[i];
        if (string.IsNullOrWhiteSpace(layerName))
            continue;

        if (string.Equals(objectName, layerName, System.StringComparison.OrdinalIgnoreCase))
            return true;
    }

    return false;
}

private Color GetButtonLayerColor(string layerName, Color baseColor)
{
    string lowerName = layerName.ToLowerInvariant();

    if (lowerName.Contains("highlight"))
        return Lighten(baseColor, 0.12f);

    if (lowerName.Contains("disabled"))
        return new Color(baseColor.r, baseColor.g, baseColor.b, 0.35f);

    if (lowerName.Contains("ripple"))
        return new Color(1f, 1f, 1f, 0.16f);

    return baseColor;
}

private void ForceButtonTextToFixedArea(TMP_Text text, float preferredWidth)
{
    if (text == null) return;

    RectTransform textRect = text.GetComponent<RectTransform>();
    if (textRect != null)
    {
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = new Vector2(buttonTextHorizontalPadding, 0f);
        textRect.offsetMax = new Vector2(-buttonTextHorizontalPadding, 0f);
    }

    LayoutElement textLayout = text.GetComponent<LayoutElement>();
    if (textLayout == null)
        textLayout = text.gameObject.AddComponent<LayoutElement>();

    textLayout.ignoreLayout = true;

    ContentSizeFitter textFitter = text.GetComponent<ContentSizeFitter>();
    if (textFitter != null)
        textFitter.enabled = false;
}

private void StyleButton(Button button, Color baseColor, Color textColor, string fallbackText = null)
{
    if (button == null) return;

    StyleButtonVisuals(button.gameObject, baseColor, textColor, fallbackText);

    if (overwriteUnityButtonColorBlock && !preserveOriginalMUIPButtonColors)
    {
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = Lighten(baseColor, 0.14f);
        colors.pressedColor = Darken(baseColor, 0.14f);
        colors.selectedColor = Lighten(baseColor, 0.08f);
        colors.disabledColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.35f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    AddPressScaleEffect(button.gameObject);
}

private void StyleButtonText(TMP_Text text, Color color)
{
    if (text == null) return;

    text.color = color;
    text.fontSize = buttonFontSize;
    text.fontStyle = FontStyles.Bold;
    text.alignment = TextAlignmentOptions.Center;
    text.enableWordWrapping = false;
    text.overflowMode = TextOverflowModes.Ellipsis;
    text.characterSpacing = 1.4f;
    text.richText = true;
}

private void StyleAllTextUnder(GameObject root)
{
    if (root == null) return;

    TMP_Text[] allTexts = root.GetComponentsInChildren<TMP_Text>(true);

    foreach (TMP_Text text in allTexts)
    {
        if (text == null) continue;

        string lowerName = text.gameObject.name.ToLower();
        string lowerText = text.text != null ? text.text.ToLower() : "";

        bool looksLikeButton = text.GetComponentInParent<Button>() != null ||
            text.GetComponentInParent<ButtonManager>() != null;
        bool looksLikeDropdown = text.GetComponentInParent<TMP_Dropdown>() != null;

        bool looksLikeDimension =
            lowerText.Contains("width") ||
            lowerText.Contains("height") ||
            lowerText.Contains("depth") ||
            lowerName.Contains("dimension");

        bool looksLikeOptions =
            lowerText.Contains("option") ||
            lowerName.Contains("option");

        bool looksLikeHeading =
            lowerName.Contains("title") ||
            lowerName.Contains("heading") ||
            lowerText.Contains("select") ||
            lowerText.Contains("choose");

        if (looksLikeButton)
        {
            text.color = buttonTextColor;
            text.fontSize = buttonFontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.characterSpacing = 1.4f;
        }
        else if (looksLikeDropdown)
        {
            text.color = dropdownTextColor;
            text.fontSize = dropdownFontSize;
            text.fontStyle = FontStyles.Normal;
            text.characterSpacing = modernTextSpacing;
        }
        else if (looksLikeDimension)
        {
            text.color = bodyTextColor;
            text.fontSize = dimensionTextFontSize;
            text.fontStyle = FontStyles.Normal;
            text.alignment = TextAlignmentOptions.Center;
            text.characterSpacing = modernTextSpacing;
        }
        else if (looksLikeOptions)
        {
            text.color = mutedTextColor;
            text.fontSize = validOptionsFontSize;
            text.fontStyle = FontStyles.Normal;
            text.alignment = TextAlignmentOptions.Center;
            text.characterSpacing = modernTextSpacing;
        }
        else if (looksLikeHeading)
        {
            text.color = headingTextColor;
            text.fontSize = headingFontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.characterSpacing = modernTextSpacing;
        }
        else
        {
            text.color = bodyTextColor;
            text.fontSize = bodyFontSize;
            text.fontStyle = FontStyles.Normal;
            text.characterSpacing = modernTextSpacing;
        }

        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.richText = true;
    }
}

private void AddPressScaleEffect(GameObject obj)
{
    if (obj == null) return;

    UIButtonPressScale existing = obj.GetComponent<UIButtonPressScale>();
    if (existing == null)
        existing = obj.AddComponent<UIButtonPressScale>();

    existing.pressedScale = buttonPressScale;
}

private void AddFadeScaleAnimation(GameObject obj)
{
    if (obj == null) return;

    UIFadeScaleAnimator animator = obj.GetComponent<UIFadeScaleAnimator>();
    if (animator == null)
        animator = obj.AddComponent<UIFadeScaleAnimator>();

    animator.animationSpeed = panelAnimationSpeed;
    animator.startScale = panelStartScale;
}


private void FixMUIPSliderInteractionAndSize(GameObject sliderRoot, SliderManager sliderManager, Slider slider)
{
    if (!fixMUIPSliderRuntimeSize)
        return;

    if (sliderRoot == null && sliderManager == null && slider == null)
        return;

    GameObject root = sliderRoot;

    if (root == null && sliderManager != null)
        root = sliderManager.gameObject;

    if (root == null && slider != null)
        root = slider.gameObject;

    if (root == null)
        return;

    // Resize only the OUTER MUIP slider root. Do not resize internal children
    // such as Background, Fill, Fill Area, or Handle Slide Area. MUIP prefabs
    // rely on their own internal RectTransform layout.
    RectTransform rootRect = root.GetComponent<RectTransform>();
    if (rootRect != null)
        rootRect.sizeDelta = new Vector2(muipSliderRootWidth, muipSliderRootHeight);

    LayoutElement rootLayout = root.GetComponent<LayoutElement>();
    if (rootLayout == null)
        rootLayout = root.AddComponent<LayoutElement>();

    rootLayout.preferredWidth = muipSliderRootWidth;
    rootLayout.minWidth = muipSliderRootWidth;
    rootLayout.preferredHeight = muipSliderRootHeight;
    rootLayout.minHeight = muipSliderRootHeight;
    rootLayout.flexibleWidth = 0f;
    rootLayout.flexibleHeight = 0f;

    CanvasGroup group = root.GetComponent<CanvasGroup>();
    if (group == null)
        group = root.AddComponent<CanvasGroup>();

    group.alpha = 1f;
    group.interactable = true;
    group.blocksRaycasts = true;

    // Resolve the actual Unity Slider inside the Modern UI Pack SliderManager.
    if (slider == null && sliderManager != null && sliderManager.mainSlider != null)
        slider = sliderManager.mainSlider;

    if (slider == null)
        slider = root.GetComponentInChildren<Slider>(true);

    if (slider == null)
    {
        Debug.LogWarning("BoxScaleUI: Could not find a Unity Slider inside " + root.name);
        return;
    }

    if (forceMUIPSlidersInteractable)
        slider.interactable = true;

    CanvasGroup sliderCanvasGroup = slider.GetComponent<CanvasGroup>();
    if (sliderCanvasGroup == null)
        sliderCanvasGroup = slider.gameObject.AddComponent<CanvasGroup>();

    sliderCanvasGroup.alpha = 1f;
    sliderCanvasGroup.interactable = true;
    sliderCanvasGroup.blocksRaycasts = true;

    // Preserve internal MUIP layout. The previous runtime fix resized Background,
    // Fill, Fill Area, and Handle Slide Area by name, which can collapse the slider
    // in Play Mode. We intentionally do not do that here.
    if (preserveMUIPSliderInternalLayout)
    {
        Image[] images = root.GetComponentsInChildren<Image>(true);

        foreach (Image img in images)
        {
            if (img == null) continue;

            string lowerName = img.gameObject.name.ToLower();

            // Keep slider graphics raycastable so the user can click/drag along
            // the bar and not only on a tiny handle.
            if (lowerName.Contains("background") ||
                lowerName.Contains("handle") ||
                lowerName.Contains("slider") ||
                lowerName.Contains("bar"))
            {
                img.raycastTarget = true;
            }
        }
    }

    // Text labels can sit on top of the slider track in MUIP prefabs and block dragging.
    if (disableSliderTextRaycastTargets || disableTMPTextRaycastsForSliders)
    {
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            if (text != null)
                text.raycastTarget = false;
        }
    }

    if (slider.targetGraphic != null)
        slider.targetGraphic.raycastTarget = true;
}

private void ResizeRectByName(Transform root, string namePart, float width, float height)
{
    if (root == null || string.IsNullOrWhiteSpace(namePart))
        return;

    string target = namePart.ToLower();
    RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);

    foreach (RectTransform rect in rects)
    {
        if (rect == null)
            continue;

        string lower = rect.gameObject.name.ToLower();

        if (lower.Contains(target))
        {
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}


private void StyleRoundedMUIPSlider(GameObject sliderRoot, SliderManager sliderManager, Slider slider)
{
    if (!styleMUIPSlidersRounded)
        return;

    GameObject root = sliderRoot;

    if (root == null && sliderManager != null)
        root = sliderManager.gameObject;

    if (root == null && slider != null)
        root = slider.gameObject;

    if (root == null)
        return;

    if (slider == null && sliderManager != null && sliderManager.mainSlider != null)
        slider = sliderManager.mainSlider;

    if (slider == null)
        slider = root.GetComponentInChildren<Slider>(true);

    if (slider == null)
    {
        Debug.LogWarning("StyleRoundedMUIPSlider: Could not find Slider inside " + root.name);
        return;
    }

    // Reuse the generated rounded-sprite function already in this file.
    // The track sprite is a sliced rounded rectangle; the handle sprite is a circle.
    Sprite trackSprite = CreateRoundedRectSprite(
        generatedSpriteSize,
        generatedSpriteSize,
        Mathf.Clamp(roundedSliderTrackCornerRadius, 1, generatedSpriteSize / 2)
    );

    Sprite handleSprite = CreateRoundedRectSprite(
        generatedSpriteSize,
        generatedSpriteSize,
        Mathf.Clamp(roundedSliderHandleCornerRadius, 1, generatedSpriteSize / 2)
    );

    Image backgroundImage = FindSliderImage(root, "background", "track", "bar");

    if (backgroundImage != null)
    {
        backgroundImage.color = roundedSliderBackgroundColor;
        backgroundImage.sprite = trackSprite;
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.raycastTarget = true;

        RectTransform rect = backgroundImage.GetComponent<RectTransform>();
        if (rect != null)
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, roundedSliderTrackHeight);
    }

    Image fillImage = null;

    if (slider.fillRect != null)
        fillImage = slider.fillRect.GetComponent<Image>();

    if (fillImage == null)
        fillImage = FindSliderImage(root, "fill");

    if (fillImage != null)
    {
        fillImage.color = roundedSliderFillColor;
        fillImage.sprite = trackSprite;
        fillImage.type = Image.Type.Sliced;
        fillImage.raycastTarget = false;

        RectTransform rect = fillImage.GetComponent<RectTransform>();
        if (rect != null)
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, roundedSliderTrackHeight);
    }

    Image handleImage = null;

    if (slider.handleRect != null)
        handleImage = slider.handleRect.GetComponent<Image>();

    if (handleImage == null)
        handleImage = FindSliderImage(root, "handle", "knob", "thumb");

    if (handleImage != null)
    {
        handleImage.color = roundedSliderHandleColor;
        handleImage.sprite = handleSprite;
        handleImage.type = Image.Type.Sliced;
        handleImage.raycastTarget = true;
        handleImage.preserveAspect = true;

        RectTransform handleImageRect = handleImage.GetComponent<RectTransform>();

        // Only center/anchor child visuals. If this image is the actual Slider.handleRect,
        // changing anchors/position can stop it from following the fill.
        if (handleImageRect != null && handleImageRect != slider.handleRect)
            MakeRectSquareCentered(handleImageRect, circularSliderHandleSize);
    }

    if (slider.handleRect != null)
    {
        // Equal width and height is the key part that makes the moving component circular.
        // Do not modify anchors or anchoredPosition of Slider.handleRect.
        float size = forceCircularSliderHandle ? circularSliderHandleSize : roundedSliderHandleSize;
        SizeMovingHandleRectWithoutBreakingSlider(slider.handleRect, size);

        // Some MUIP prefabs put the Image on the handle RectTransform.
        Image handleRectImage = slider.handleRect.GetComponent<Image>();
        if (handleRectImage != null)
        {
            handleRectImage.sprite = handleSprite;
            handleRectImage.type = Image.Type.Sliced;
            handleRectImage.color = roundedSliderHandleColor;
            handleRectImage.raycastTarget = true;
            handleRectImage.preserveAspect = true;
        }
    }

    // Important: style the actual moving Slider.handleRect, not a random handle-looking image.
    ForceCircularSliderHandle(root, slider, handleSprite);
    ShiftSliderRootLeft(root);

    slider.interactable = true;

    if (slider.targetGraphic != null)
        slider.targetGraphic.raycastTarget = true;

    // Text labels can sit over the track and block pointer dragging.
    // Make them larger and wide enough so values like "Width: 24.5 in" do not collapse to "Width: ...".
    TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
    foreach (TMP_Text text in texts)
    {
        if (text == null)
            continue;

        text.color = useCompactBottomUIStyle ? sliderTextColor : secondaryTextColor;
        text.overrideColorTags = true;
        text.enableAutoSizing = false;
        text.fontSize = emphasizeDimensionAndOptionsText
            ? emphasizedDimensionFontSize
            : muipSliderTextFontSize;
        text.fontStyle = emphasizeDimensionAndOptionsText ? FontStyles.Bold : text.fontStyle;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;

        RectTransform textRect = text.GetComponent<RectTransform>();
        if (textRect != null)
            textRect.sizeDelta = new Vector2(muipSliderTextWidth, textRect.sizeDelta.y);

        LayoutElement textLayout = text.GetComponent<LayoutElement>();
        if (textLayout == null)
            textLayout = text.gameObject.AddComponent<LayoutElement>();

        textLayout.preferredWidth = muipSliderTextWidth;
        textLayout.minWidth = muipSliderTextWidth;
        textLayout.flexibleWidth = 0f;
    }
}

private void ShiftSliderRootLeft(GameObject sliderRoot)
{
    if (!shiftMUIPSlidersLeft || sliderRoot == null)
        return;

    RectTransform rect = sliderRoot.GetComponent<RectTransform>();
    if (rect == null)
        return;

    if (!originalSliderAnchoredPositions.ContainsKey(rect))
        originalSliderAnchoredPositions[rect] = rect.anchoredPosition;

    Vector2 original = originalSliderAnchoredPositions[rect];
    rect.anchoredPosition = new Vector2(original.x + muipSliderLeftShift, original.y);
}

private void ForceCircularSliderHandle(GameObject sliderRoot, Slider slider, Sprite handleSprite)
{
    if (!forceCircularSliderHandle)
        return;

    if (sliderRoot == null && slider == null)
        return;

    if (slider == null && sliderRoot != null)
        slider = sliderRoot.GetComponentInChildren<Slider>(true);

    if (slider == null)
        return;

    float size = circularSliderHandleSize;

    // Make sure Slider.handleRect points to the actual moving handle.
    // If it is null, find a likely Handle/Knob/Thumb image and assign it.
    if (slider.handleRect == null && sliderRoot != null)
    {
        Image[] possibleHandleImages = sliderRoot.GetComponentsInChildren<Image>(true);

        foreach (Image img in possibleHandleImages)
        {
            if (img == null)
                continue;

            string lower = img.gameObject.name.ToLowerInvariant();

            if (lower.Contains("handle") || lower.Contains("knob") || lower.Contains("thumb"))
            {
                slider.handleRect = img.GetComponent<RectTransform>();
                slider.targetGraphic = img;
                break;
            }
        }
    }

    if (slider.handleRect == null)
    {
        Debug.LogWarning("BoxScaleUI: Could not find Slider.handleRect for " + slider.name);
        return;
    }

    // This is the actual moving RectTransform used by Unity's Slider.
    // Resize it, but do not change its anchors or anchored position.
    SizeMovingHandleRectWithoutBreakingSlider(slider.handleRect, size);

    Image movingHandleImage = slider.handleRect.GetComponent<Image>();

    if (movingHandleImage == null)
        movingHandleImage = slider.handleRect.GetComponentInChildren<Image>(true);

    if (movingHandleImage != null)
    {
        movingHandleImage.sprite = handleSprite;
        movingHandleImage.type = Image.Type.Sliced;
        movingHandleImage.color = roundedSliderHandleColor;
        movingHandleImage.raycastTarget = true;
        movingHandleImage.preserveAspect = true;

        RectTransform imageRect = movingHandleImage.GetComponent<RectTransform>();

        // If the image is a child of handleRect, center the child visual.
        // If the image is the handleRect itself, do not recenter it.
        if (imageRect != null && imageRect != slider.handleRect)
            MakeRectSquareCentered(imageRect, size);

        slider.targetGraphic = movingHandleImage;
    }

    slider.interactable = true;
}

private void MakeRectSquareCentered(RectTransform rect, float size)
{
    if (rect == null)
        return;

    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);
    rect.sizeDelta = new Vector2(size, size);
    rect.anchoredPosition = Vector2.zero;
    rect.localScale = Vector3.one;
}

private void SizeMovingHandleRectWithoutBreakingSlider(RectTransform rect, float size)
{
    if (rect == null)
        return;

    // Do NOT change anchors or anchoredPosition on Slider.handleRect.
    // Unity's Slider moves the handle by driving that RectTransform.
    // Changing anchors/position here can make the visual knob stop following the fill.
    rect.sizeDelta = new Vector2(size, size);
    rect.localScale = Vector3.one;

    LayoutElement layout = rect.GetComponent<LayoutElement>();
    if (layout == null)
        layout = rect.gameObject.AddComponent<LayoutElement>();

    layout.minWidth = size;
    layout.minHeight = size;
    layout.preferredWidth = size;
    layout.preferredHeight = size;
    layout.flexibleWidth = 0f;
    layout.flexibleHeight = 0f;
}

private Image FindSliderImage(GameObject root, params string[] nameParts)
{
    if (root == null || nameParts == null)
        return null;

    Image[] images = root.GetComponentsInChildren<Image>(true);

    foreach (Image image in images)
    {
        if (image == null)
            continue;

        string lowerName = image.gameObject.name.ToLowerInvariant();

        for (int i = 0; i < nameParts.Length; i++)
        {
            string part = nameParts[i];

            if (string.IsNullOrWhiteSpace(part))
                continue;

            if (lowerName.Contains(part.ToLowerInvariant()))
                return image;
        }
    }

    return null;
}

private void StyleSliderRow(GameObject rowRoot, Slider slider, TMP_Text label)
{
    if (rowRoot == null)
        return;

    RectTransform rowRect = rowRoot.GetComponent<RectTransform>();
    if (rowRect != null)
    {
        rowRect.sizeDelta = new Vector2(rowRect.sizeDelta.x, sliderRowHeight);
    }

    LayoutElement rowLayout = rowRoot.GetComponent<LayoutElement>();
    if (rowLayout == null)
        rowLayout = rowRoot.AddComponent<LayoutElement>();

    rowLayout.preferredHeight = sliderRowHeight;
    rowLayout.minHeight = sliderRowHeight;
    rowLayout.flexibleWidth = 1f;

    if (forceSliderRowsHorizontalLayout)
    {
        HorizontalLayoutGroup horizontal = rowRoot.GetComponent<HorizontalLayoutGroup>();
        if (horizontal == null)
            horizontal = rowRoot.AddComponent<HorizontalLayoutGroup>();

        horizontal.padding = new RectOffset(0, 0, 0, 0);
        horizontal.spacing = sliderRowSpacing;
        horizontal.childAlignment = TextAnchor.MiddleCenter;
        horizontal.childControlWidth = true;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandWidth = false;
        horizontal.childForceExpandHeight = false;
    }

    if (label != null)
    {
        StyleInfoText(label, dimensionTextFontSize);
        label.alignment = TextAlignmentOptions.MidlineLeft;

        LayoutElement labelLayout = label.GetComponent<LayoutElement>();
        if (labelLayout == null)
            labelLayout = label.gameObject.AddComponent<LayoutElement>();

        labelLayout.preferredWidth = sliderLabelWidth;
        labelLayout.minWidth = sliderLabelWidth;
        labelLayout.flexibleWidth = 0f;
    }

    if (slider != null)
    {
        LayoutElement sliderLayout = slider.GetComponent<LayoutElement>();
        if (sliderLayout == null)
            sliderLayout = slider.gameObject.AddComponent<LayoutElement>();

        sliderLayout.preferredHeight = sliderHeight;
        sliderLayout.minHeight = sliderHeight;
        sliderLayout.flexibleWidth = 1f;
    }
}

private void StyleSlider(SliderManager sliderManager)
{
    if (sliderManager == null) return;

    if (sliderManager.mainSlider != null)
        StyleSlider(sliderManager.mainSlider);
}

private void StyleSlider(Slider slider)
{
    if (slider == null) return;

    slider.transition = Selectable.Transition.ColorTint;

    RectTransform sliderRect = slider.GetComponent<RectTransform>();
    if (sliderRect != null)
    {
        sliderRect.sizeDelta = new Vector2(sliderRect.sizeDelta.x, sliderHeight);
    }

    Image background = slider.transform.Find("Background")?.GetComponent<Image>();
    if (background != null)
    {
        background.color = sliderBackgroundColor;

        if (roundedSliderSprite != null)
        {
            background.sprite = roundedSliderSprite;
            background.type = Image.Type.Sliced;
        }
    }

    Image fill = slider.fillRect != null
        ? slider.fillRect.GetComponent<Image>()
        : null;

    if (fill != null)
    {
        fill.color = sliderFillColor;

        if (roundedSliderSprite != null)
        {
            fill.sprite = roundedSliderSprite;
            fill.type = Image.Type.Sliced;
        }
    }

    Image handle = slider.handleRect != null
        ? slider.handleRect.GetComponent<Image>()
        : null;

    if (handle != null)
    {
        handle.color = sliderHandleColor;

        if (circularHandleSprite != null)
        {
            handle.sprite = circularHandleSprite;
            handle.type = Image.Type.Sliced;
        }
    }

    LayoutElement layout = slider.GetComponent<LayoutElement>();
    if (layout == null)
        layout = slider.gameObject.AddComponent<LayoutElement>();

    layout.preferredHeight = sliderHeight;
    layout.minHeight = sliderHeight;

    ResizeSliderParts(slider);
    UpdateSingleSliderVisual(slider);
}

private void ResizeSliderParts(Slider slider)
{
    if (slider == null) return;

    if (slider.handleRect != null)
    {
        slider.handleRect.sizeDelta = new Vector2(sliderHandleSize, sliderHandleSize);
    }

    Transform background = slider.transform.Find("Background");
    if (background != null)
    {
        RectTransform bgRect = background.GetComponent<RectTransform>();
        if (bgRect != null)
        {
            bgRect.sizeDelta = new Vector2(bgRect.sizeDelta.x, sliderTrackHeight);
        }
    }

    if (slider.fillRect != null)
    {
        slider.fillRect.sizeDelta = new Vector2(
            slider.fillRect.sizeDelta.x,
            sliderTrackHeight
        );
    }

    Transform fillArea = slider.transform.Find("Fill Area");
    if (fillArea != null)
    {
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        if (fillAreaRect != null)
        {
            fillAreaRect.offsetMin = new Vector2(0f, fillAreaRect.offsetMin.y);
            fillAreaRect.offsetMax = new Vector2(0f, fillAreaRect.offsetMax.y);
        }
    }

    Transform handleSlideArea = slider.transform.Find("Handle Slide Area");
    if (handleSlideArea != null)
    {
        RectTransform handleAreaRect = handleSlideArea.GetComponent<RectTransform>();
        if (handleAreaRect != null)
        {
            handleAreaRect.offsetMin = Vector2.zero;
            handleAreaRect.offsetMax = Vector2.zero;
        }
    }
}

private void UpdateSliderVisuals()
{
    UpdateSingleSliderVisual(xSlider);
    UpdateSingleSliderVisual(ySlider);
    UpdateSingleSliderVisual(zSlider);
}

private void UpdateSingleSliderVisual(Slider slider)
{
    if (slider == null) return;

    Color targetColor = useDynamicSliderColors
        ? GetColorForSliderValue(slider.value)
        : sliderFillColor;

    if (!currentSliderColors.ContainsKey(slider))
        currentSliderColors[slider] = targetColor;

    Color currentColor = Color.Lerp(
        currentSliderColors[slider],
        targetColor,
        Time.unscaledDeltaTime * sliderColorLerpSpeed
    );

    currentSliderColors[slider] = currentColor;

    Image fill = slider.fillRect != null
        ? slider.fillRect.GetComponent<Image>()
        : null;

    if (fill != null)
    {
        fill.color = currentColor;

        if (roundedSliderSprite != null)
        {
            fill.sprite = roundedSliderSprite;
            fill.type = Image.Type.Sliced;
        }
    }

    Image handle = slider.handleRect != null
        ? slider.handleRect.GetComponent<Image>()
        : null;

    if (handle != null)
    {
        handle.color = Color.Lerp(sliderHandleGlowColor, currentColor, 0.45f);

        if (circularHandleSprite != null)
        {
            handle.sprite = circularHandleSprite;
            handle.type = Image.Type.Sliced;
            handle.preserveAspect = true;
        }

        // Do not call MakeRectSquareCentered on Slider.handleRect here.
        // The slider must remain free to move this RectTransform as its value changes.
        if (forceCircularSliderHandle)
            SizeMovingHandleRectWithoutBreakingSlider(handle.GetComponent<RectTransform>(), circularSliderHandleSize);
    }

    if (slider.handleRect != null && forceCircularSliderHandle)
        SizeMovingHandleRectWithoutBreakingSlider(slider.handleRect, circularSliderHandleSize);
}

private Color GetColorForSliderValue(float value)
{
    float t = Mathf.InverseLerp(minScale, maxScale, value);

    if (t < 0.5f)
    {
        return Color.Lerp(smallSizeColor, mediumSizeColor, t * 2f);
    }

    return Color.Lerp(mediumSizeColor, largeSizeColor, (t - 0.5f) * 2f);
}

private void StyleDropdown(CustomDropdown dropdown)
{
    if (dropdown == null) return;

    Image image = dropdown.GetComponent<Image>();
    if (image != null)
    {
        image.color = dropdownColor;

        if (roundedButtonSprite != null)
        {
            image.sprite = roundedButtonSprite;
            image.type = Image.Type.Sliced;
        }
    }

    LayoutElement layout = dropdown.GetComponent<LayoutElement>();
    if (layout == null)
        layout = dropdown.gameObject.AddComponent<LayoutElement>();

    layout.preferredHeight = dropdownHeight;
    layout.minHeight = dropdownHeight;

    RectTransform rect = dropdown.GetComponent<RectTransform>();
    if (rect != null)
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, dropdownHeight);

    TMP_Text[] texts = dropdown.GetComponentsInChildren<TMP_Text>(true);
    foreach (TMP_Text text in texts)
    {
        if (text == null) continue;
        text.color = dropdownTextColor;
        text.overrideColorTags = true;
        text.enableAutoSizing = false;
        text.fontSize = dropdownFontSize;
        text.fontStyle = FontStyles.Normal;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.characterSpacing = textCharacterSpacing;
    }
}

private void StyleDropdown(TMP_Dropdown dropdown)
{
    if (dropdown == null) return;

    Image image = dropdown.GetComponent<Image>();
    if (image == null)
        image = dropdown.gameObject.AddComponent<Image>();

    image.color = dropdownColor;

    if (roundedButtonSprite != null)
    {
        image.sprite = roundedButtonSprite;
        image.type = Image.Type.Sliced;
    }

    LayoutElement layout = dropdown.GetComponent<LayoutElement>();
    if (layout == null)
        layout = dropdown.gameObject.AddComponent<LayoutElement>();

    layout.preferredHeight = dropdownHeight;
    layout.minHeight = dropdownHeight;

    RectTransform rect = dropdown.GetComponent<RectTransform>();
    if (rect != null)
    {
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, dropdownHeight);
    }

    TMP_Text label = dropdown.captionText;
    if (label != null)
    {
        label.color = dropdownTextColor;
        label.overrideColorTags = true;
        label.enableAutoSizing = false;
        label.fontSize = dropdownFontSize;
        label.fontStyle = FontStyles.Normal;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.characterSpacing = textCharacterSpacing;
    }

    TMP_Text itemLabel = dropdown.itemText;
    if (itemLabel != null)
    {
        itemLabel.color = dropdownTextColor;
        itemLabel.overrideColorTags = true;
        itemLabel.enableAutoSizing = false;
        itemLabel.fontSize = dropdownFontSize;
        itemLabel.fontStyle = FontStyles.Normal;
        itemLabel.enableWordWrapping = false;
        itemLabel.overflowMode = TextOverflowModes.Ellipsis;
        itemLabel.characterSpacing = textCharacterSpacing;
    }

    StyleDropdownTemplate(dropdown);
}

private void StyleDropdownTemplate(TMP_Dropdown dropdown)
{
    if (dropdown == null || dropdown.template == null) return;

    Image templateImage = dropdown.template.GetComponent<Image>();
    if (templateImage != null)
    {
        templateImage.color = new Color(0.08f, 0.09f, 0.11f, 0.98f);

        if (roundedPanelSprite != null)
        {
            templateImage.sprite = roundedPanelSprite;
            templateImage.type = Image.Type.Sliced;
        }
    }

    ScrollRect scrollRect = dropdown.template.GetComponentInChildren<ScrollRect>();
    if (scrollRect != null)
    {
        Image viewportImage = scrollRect.viewport != null
            ? scrollRect.viewport.GetComponent<Image>()
            : null;

        if (viewportImage != null)
            viewportImage.color = new Color(0.08f, 0.09f, 0.11f, 0.98f);
    }
}

private void StyleInfoText(TMP_Text text, int fontSize)
{
    if (text == null) return;

    text.color = secondaryTextColor;
    text.fontSize = fontSize;
    text.fontStyle = FontStyles.Normal;
    text.alignment = TextAlignmentOptions.Center;
    text.enableWordWrapping = false;
    text.overflowMode = TextOverflowModes.Ellipsis;
    text.characterSpacing = textCharacterSpacing;
    text.richText = true;
}

private Color Lighten(Color color, float amount)
{
    return new Color(
        Mathf.Clamp01(color.r + amount),
        Mathf.Clamp01(color.g + amount),
        Mathf.Clamp01(color.b + amount),
        color.a
    );
}

private Color Darken(Color color, float amount)
{
    return new Color(
        Mathf.Clamp01(color.r - amount),
        Mathf.Clamp01(color.g - amount),
        Mathf.Clamp01(color.b - amount),
        color.a
    );
}


private void SetUIGroupVisible(GameObject root, bool visible, bool interactable)
{
    if (root == null)
        return;

    // Enable first so CanvasGroup state can be applied safely.
    root.SetActive(true);

    CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
    if (canvasGroup == null)
        canvasGroup = root.AddComponent<CanvasGroup>();

    canvasGroup.alpha = visible ? 1f : 0f;
    canvasGroup.interactable = visible && interactable;
    canvasGroup.blocksRaycasts = visible && interactable;

    // Disable hidden groups so they do not block layout/input.
    root.SetActive(visible);
}

private void ForceVisible(GameObject root, bool interactable = true)
{
    if (root == null)
        return;

    root.SetActive(true);

    CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
    if (canvasGroup == null)
        canvasGroup = root.AddComponent<CanvasGroup>();

    canvasGroup.alpha = 1f;
    canvasGroup.interactable = interactable;
    canvasGroup.blocksRaycasts = interactable;
}

private void SetRootVisible(GameObject root, bool visible)
{
    SetUIGroupVisible(root, visible, visible);
}

private GameObject GetButtonRoot(ButtonManager muipButton, Button unityButton, GameObject fallbackRoot)
{
    if (fallbackRoot != null)
        return fallbackRoot;

    if (muipButton != null)
        return muipButton.gameObject;

    if (unityButton != null)
        return unityButton.gameObject;

    return null;
}

private void SetButtonObjectVisible(ButtonManager muipButton, Button unityButton, GameObject rootObject, bool visible)
{
    GameObject root = GetButtonRoot(muipButton, unityButton, rootObject);

    if (root != null)
        SetUIGroupVisible(root, visible, visible);
}

private void SetModeButtonsVisible(bool visible)
{
    SetButtonObjectVisible(drawModeButtonMUIP, drawModeButton, modernDrawButtonRoot, visible);
    SetButtonObjectVisible(eraseModeButtonMUIP, eraseModeButton, modernEraseButtonRoot, visible);
}

private void SetResizeControlsVisible(bool visible)
{
    // Usually these are children of panel/modernPanelRoot, so showing panel is enough.
    // These extra calls protect you if one of them has its own CanvasGroup alpha = 0
    // or if one of them is accidentally outside Panel_2.
    SetButtonObjectVisible(loadOptionsButtonMUIP, loadOptionsButton, modernPlaceButtonRoot, visible);
    SetUIGroupVisible(modernDropdownRoot, visible, visible);
    SetUIGroupVisible(modernXSliderRoot, visible, visible);
    SetUIGroupVisible(modernYSliderRoot, visible, visible);
    SetUIGroupVisible(modernZSliderRoot, visible, visible);
}

private void SetCandidateNavigationButtonsVisible(bool visible)
{
    SetButtonObjectVisible(previousButtonMUIP, previousButton, modernPreviousButtonRoot, visible);
    SetButtonObjectVisible(nextButtonMUIP, nextButton, modernNextButtonRoot, visible);
    SetButtonObjectVisible(confirmButtonMUIP, confirmButton, modernConfirmButtonRoot, visible);
}

private void SetBottomPanelVisible(bool visible)
{
    if (bottomPanelRoot != null)
        bottomPanelRoot.SetActive(visible);
}

private bool ShouldShowBottomPanelForCurrentMode()
{
    if (!hideBottomPanelWhileDrawingOrErasing)
        return true;

    // Keep the bottom panel hidden only while actively drawing or erasing.
    // Once ShowForBox assigns targetBox, the user is no longer drawing; they are resizing,
    // so the bottom panel should come back with the sliders/dropdown/place button.
    if (currentMode == InteractionMode.Erase)
        return false;

    if (currentMode == InteractionMode.Draw && targetBox == null)
        return false;

    return true;
}

private void SetBottomPanelVisibleForNormalUI(bool visible)
{
    SetBottomPanelVisible(visible && ShouldShowBottomPanelForCurrentMode());
}

private void ApplyCompactBottomUIOverrides()
{
    if (!useCompactBottomUIStyle)
        return;

    muipSliderRootWidth = compactMUIPSliderRootWidth;
    muipSliderRootHeight = compactMUIPSliderRootHeight;
    roundedSliderTrackHeight = compactRoundedSliderTrackHeight;
    roundedSliderHandleSize = compactRoundedSliderHandleSize;
    circularSliderHandleSize = compactCircularSliderHandleSize;
    muipSliderTextWidth = compactMUIPSliderTextWidth;
    muipSliderTextFontSize = compactMUIPSliderTextFontSize;
    emphasizedDimensionFontSize = compactDimensionFontSize;
    dimensionTextFontSize = compactDimensionFontSize;
    emphasizedValidOptionsFontSize = compactValidOptionsFontSize;
    validOptionsFontSize = compactValidOptionsFontSize;

    // Gray slider highlight/fill defaults for the compact bottom UI.
    useDynamicSliderColors = false;
    sliderFillColor = roundedSliderFillColor;
    sliderHandleColor = roundedSliderHandleColor;
    sliderHandleGlowColor = roundedSliderFillColor;
    sliderBackgroundColor = roundedSliderBackgroundColor;
    smallSizeColor = roundedSliderFillColor;
    mediumSizeColor = roundedSliderFillColor;
    largeSizeColor = roundedSliderFillColor;

    secondaryTextColor = sliderTextColor;
}

private void ShowOnlyModeSelectionUI()
{
    Debug.Log("UI State: Mode Selection");

    SetBottomPanelVisibleForNormalUI(true);

    SetUIGroupVisible(modeSelectionPanel, true, true);
    SetUIGroupVisible(panel, false, false);
    SetUIGroupVisible(candidateButtonsParent, false, false);

    SetModeButtonsVisible(true);
    SetResizeControlsVisible(false);
    SetCandidateNavigationButtonsVisible(false);
}

private void ShowOnlyResizePanelUI()
{
    Debug.Log("UI State: Resize Panel");

    SetBottomPanelVisibleForNormalUI(true);

    SetUIGroupVisible(modeSelectionPanel, false, false);
    SetUIGroupVisible(candidateButtonsParent, false, false);
    SetUIGroupVisible(panel, true, true);

    // Extra safety: explicitly reveal MUIP resize children. This fixes cases where
    // the parent is visible but one of the children still has CanvasGroup.alpha = 0.
    ForceVisible(modernDropdownRoot, true);
    ForceVisible(modernXSliderRoot, true);
    ForceVisible(modernYSliderRoot, true);
    ForceVisible(modernZSliderRoot, true);
    ForceVisible(modernPlaceButtonRoot, true);

    SetButtonObjectVisible(loadOptionsButtonMUIP, loadOptionsButton, modernPlaceButtonRoot, true);

    FixMUIPSliderInteractionAndSize(modernXSliderRoot, xSliderMUIP, xSlider);
    FixMUIPSliderInteractionAndSize(modernYSliderRoot, ySliderMUIP, ySlider);
    FixMUIPSliderInteractionAndSize(modernZSliderRoot, zSliderMUIP, zSlider);

    StyleRoundedMUIPSlider(modernXSliderRoot, xSliderMUIP, xSlider);
    StyleRoundedMUIPSlider(modernYSliderRoot, ySliderMUIP, ySlider);
    StyleRoundedMUIPSlider(modernZSliderRoot, zSliderMUIP, zSlider);

    SetModeButtonsVisible(false);
    SetCandidateNavigationButtonsVisible(false);
    ShiftSliderRootLeft(modernXSliderRoot);
    ShiftSliderRootLeft(modernYSliderRoot);
    ShiftSliderRootLeft(modernZSliderRoot);

}

private void ShowOnlyCandidateButtonsUI(bool visible)
{
    Debug.Log("UI State: Candidate Buttons = " + visible);

    SetBottomPanelVisibleForNormalUI(visible);

    SetUIGroupVisible(modeSelectionPanel, false, false);
    SetUIGroupVisible(panel, false, false);
    SetUIGroupVisible(candidateButtonsParent, visible, visible);

    SetResizeControlsVisible(false);
    SetModeButtonsVisible(false);
    SetCandidateNavigationButtonsVisible(visible);
}

private void DebugResizePanelState()
{
    Debug.Log("panel = " + (panel != null ? panel.name : "NULL"));

    if (panel != null)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        Debug.Log(
            $"Panel activeSelf={panel.activeSelf}, activeInHierarchy={panel.activeInHierarchy}, " +
            $"alpha={(cg != null ? cg.alpha : -1f)}, " +
            $"interactable={(cg != null && cg.interactable)}, " +
            $"blocksRaycasts={(cg != null && cg.blocksRaycasts)}"
        );
    }

    Debug.Log("modernXSliderRoot active = " + (modernXSliderRoot != null && modernXSliderRoot.activeInHierarchy));
    Debug.Log("modernYSliderRoot active = " + (modernYSliderRoot != null && modernYSliderRoot.activeInHierarchy));
    Debug.Log("modernZSliderRoot active = " + (modernZSliderRoot != null && modernZSliderRoot.activeInHierarchy));
    Debug.Log("modernPlaceButtonRoot active = " + (modernPlaceButtonRoot != null && modernPlaceButtonRoot.activeInHierarchy));
}

public void ShowModeSelectionOnly()
{
    Debug.Log("Showing Draw / Erase buttons only.");

    currentMode = InteractionMode.None;
    targetBox = null;
    candidatesActive = false;

    ShowOnlyModeSelectionUI();

    UpdateDimensionTexts();
    ClearValidOptionsCount();
    UpdateSliderVisuals();

    if (lassoDrawer != null)
    {
        lassoDrawer.SetEraseMode(false);
        lassoDrawer.SetLassoInputEnabled(false);
        lassoDrawer.ClearLasso();

        // Important: do not call ClearCurrentCandidates() by default here.
        // In your LassoDrawer, ClearCurrentCandidates() does:
        //     if (fittedBox != null) fittedBox.SetActive(true);
        // That can make the fitted box show up as a black rounded box while
        // only the Draw / Erase UI should be visible.
        if (clearLassoCandidatesWhenReturningToModeSelection)
            lassoDrawer.ClearCurrentCandidates();
    }
}

public void EnterDrawMode()
{
    Debug.Log("Draw button clicked. Entering Draw Mode.");

    currentMode = InteractionMode.Draw;
    targetBox = null;
    candidatesActive = false;

    // Hide all UI while the user is drawing, and make sure it cannot block lasso input.
    SetBottomPanelVisible(false);

    SetUIGroupVisible(modeSelectionPanel, false, false);
    SetUIGroupVisible(panel, false, false);
    SetUIGroupVisible(candidateButtonsParent, false, false);
    SetModeButtonsVisible(false);
    SetResizeControlsVisible(false);
    SetCandidateNavigationButtonsVisible(false);

    ClearValidOptionsCount();

    if (lassoDrawer != null)
    {
        lassoDrawer.ClearLasso();
        lassoDrawer.SetEraseMode(false);
        lassoDrawer.SetLassoInputEnabled(true);
    }
    else
    {
        Debug.LogWarning("BoxScaleUI: LassoDrawer is missing.");
    }
}

public void EnterEraseMode()
{
    Debug.Log("Erase button clicked. Entering Erase Mode.");

    currentMode = InteractionMode.Erase;
    targetBox = null;
    candidatesActive = false;

    // Hide all UI while the user is erasing, and make sure it cannot block lasso input.
    SetBottomPanelVisible(false);

    SetUIGroupVisible(modeSelectionPanel, false, false);
    SetUIGroupVisible(panel, false, false);
    SetUIGroupVisible(candidateButtonsParent, false, false);
    SetModeButtonsVisible(false);
    SetResizeControlsVisible(false);
    SetCandidateNavigationButtonsVisible(false);

    ClearValidOptionsCount();

    if (lassoDrawer != null)
    {
        lassoDrawer.ClearLasso();
        lassoDrawer.SetEraseMode(true);
        lassoDrawer.SetLassoInputEnabled(true);
    }
    else
    {
        Debug.LogWarning("BoxScaleUI: LassoDrawer is missing.");
    }
}

public void ShowForBox(GameObject box)
{
    Debug.Log("ShowForBox called. currentMode = " + currentMode);

    if (box == null)
    {
        Debug.LogWarning("ShowForBox called with null box.");
        return;
    }

    if (currentMode == InteractionMode.None)
    {
        Debug.LogWarning("ShowForBox called while mode was None. Switching to Draw.");
        currentMode = InteractionMode.Draw;
    }

    if (currentMode == InteractionMode.Erase)
    {
        Debug.LogWarning("ShowForBox ignored because current mode is Erase.");
        return;
    }

    targetBox = box.transform;

    // A box now exists, so drawing is complete and the resize UI should be visible.
    // This also allows SetBottomPanelVisibleForNormalUI(true) to re-enable the bottom panel.
    SetBottomPanelVisibleForNormalUI(true);

    // If you do not want the 3D fitted preview box visible during resizing,
    // hide the actual GameObject directly. This does not depend on the
    // BoxScaleUI.lassoDrawer reference being assigned. The hidden transform
    // can still be scaled by the sliders, and LassoDrawer will re-enable it
    // when Place is pressed to search/load candidates.
    if (!showFittedBoxDuringResizePanel)
    {
        if (lassoDrawer != null)
            lassoDrawer.HideFittedBox();

        if (box != null)
            box.SetActive(false);
    }

    // After the lasso creates a box, show only the resize panel
    // with dropdown, sliders, and Place button.
    ShowOnlyResizePanelUI();

    if (lassoDrawer != null)
        lassoDrawer.SetLassoInputEnabled(false);

    Vector3 scale = targetBox.localScale;

    // Important: if Gemini returns a dimension larger than the default slider max,
    // Unity's Slider.value would silently clamp to maxScale. Expand the ranges
    // before assigning values so the UI actually reflects Gemini's dimensions.
    ExpandSliderRangesToFitScale(scale);

    Debug.Log($"BoxScaleUI.ShowForBox -> targetBox localScale before slider sync = {scale}");

    suppressCallbacks = true;

    if (xSlider != null)
        xSlider.value = scale.x;

    if (ySlider != null)
        ySlider.value = scale.y;

    if (zSlider != null)
        zSlider.value = scale.z;

    suppressCallbacks = false;

    // Ensure the labels use the targetBox dimensions after slider synchronization.
    // This is especially important for Gemini dimensions that exceed the old slider range.
    ForceSyncSlidersAndTextsFromTargetBox();

    if (delayedForceSyncAfterShowForBox)
        StartCoroutine(ForceSyncSlidersAndTextsNextFrames());

    UpdateDimensionTexts();
    UpdateValidOptionsCountFromCurrentBox();
    UpdateSliderVisuals();

    FixMUIPSliderInteractionAndSize(modernXSliderRoot, xSliderMUIP, xSlider);
    FixMUIPSliderInteractionAndSize(modernYSliderRoot, ySliderMUIP, ySlider);
    FixMUIPSliderInteractionAndSize(modernZSliderRoot, zSliderMUIP, zSlider);

    StyleRoundedMUIPSlider(modernXSliderRoot, xSliderMUIP, xSlider);
    StyleRoundedMUIPSlider(modernYSliderRoot, ySliderMUIP, ySlider);
    StyleRoundedMUIPSlider(modernZSliderRoot, zSliderMUIP, zSlider);

    ShowOnlyResizePanelUI();
    Debug.Log("Resize panel shown.");
    DebugResizePanelState();
}

public void ForceSyncSlidersAndTextsFromTargetBox()
{
    if (targetBox == null)
        return;

    Vector3 scale = targetBox.localScale;
    ExpandSliderRangesToFitScale(scale);

    suppressCallbacks = true;

    SetSliderValueFromBox(xSlider, xSliderMUIP, scale.x, "Width/X");
    SetSliderValueFromBox(ySlider, ySliderMUIP, scale.y, "Height/Y");
    SetSliderValueFromBox(zSlider, zSliderMUIP, scale.z, "Length/Z");

    suppressCallbacks = false;

    UpdateDimensionTexts();
    UpdateValidOptionsCountFromCurrentBox();
    UpdateSliderVisuals();

    Debug.Log(
        $"BoxScaleUI.ForceSyncSlidersAndTextsFromTargetBox -> " +
        $"targetBox scale={scale}, " +
        $"slider values=({GetSliderValueDebug(xSlider):F3}, {GetSliderValueDebug(ySlider):F3}, {GetSliderValueDebug(zSlider):F3})"
    );
}

private IEnumerator ForceSyncSlidersAndTextsNextFrames()
{
    int frames = Mathf.Max(1, delayedForceSyncFrames);

    for (int i = 0; i < frames; i++)
    {
        yield return null;
        ForceSyncSlidersAndTextsFromTargetBox();
    }
}

private float GetSliderValueDebug(Slider slider)
{
    return slider != null ? slider.value : -1f;
}

private void SetSliderValueFromBox(Slider slider, SliderManager sliderManager, float value, string debugName)
{
    float safeValue = Mathf.Abs(value);

    if (slider == null && sliderManager != null && sliderManager.mainSlider != null)
        slider = sliderManager.mainSlider;

    if (slider == null)
    {
        Debug.LogWarning($"BoxScaleUI: Missing Unity Slider for {debugName}. Assign the SliderManager/mainSlider in the Inspector.");
        return;
    }

    ExpandSliderRangeToFitValue(slider, safeValue);

    // SetValueWithoutNotify avoids calling OnX/Y/ZSliderChanged while we are only
    // synchronizing UI from the Gemini-created fitted box.
    slider.SetValueWithoutNotify(safeValue);

    if (sliderManager != null && sliderManager.mainSlider != null && sliderManager.mainSlider != slider)
    {
        ExpandSliderRangeToFitValue(sliderManager.mainSlider, safeValue);
        sliderManager.mainSlider.SetValueWithoutNotify(safeValue);
    }

    RefreshMUIPSliderManager(sliderManager);
}

private void RefreshMUIPSliderManager(SliderManager sliderManager)
{
    if (sliderManager == null)
        return;

    TryInvokeNoArg(sliderManager, "UpdateUI");
    TryInvokeNoArg(sliderManager, "UpdateSlider");
    TryInvokeNoArg(sliderManager, "UpdateValue");
    TryInvokeNoArg(sliderManager, "Refresh");
}

private void TryInvokeNoArg(object target, string methodName)
{
    if (target == null || string.IsNullOrWhiteSpace(methodName))
        return;

    System.Reflection.MethodInfo method = target.GetType().GetMethod(
        methodName,
        System.Reflection.BindingFlags.Instance |
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.NonPublic,
        null,
        Type.EmptyTypes,
        null
    );

    if (method == null)
        return;

    try
    {
        method.Invoke(target, null);
    }
    catch (Exception e)
    {
        Debug.LogWarning($"BoxScaleUI: {methodName} failed on {target.GetType().Name}: {e.Message}");
    }
}

public void HidePanel()
{
    targetBox = null;

    if (!candidatesActive)
        ShowOnlyModeSelectionUI();
    else
        ShowOnlyCandidateButtonsUI(true);

    UpdateDimensionTexts();
    ClearValidOptionsCount();
    UpdateSliderVisuals();

    if (lassoDrawer != null && currentMode == InteractionMode.Draw)
        lassoDrawer.SetLassoInputEnabled(true);
}

public void SetCandidateButtonsVisible(bool visible)
{
    if (visible)
    {
        ShowCandidateSelectionOnly();
    }
    else
    {
        // If the user is still resizing a pending box, return to the resize panel
        // instead of Draw/Erase. This is important for the no-valid-options case
        // after pressing Place.
        candidatesActive = false;
        SetUIGroupVisible(candidateButtonsParent, false, false);
        SetCandidateNavigationButtonsVisible(false);

        if (targetBox != null && currentMode == InteractionMode.Draw)
        {
            ShowOnlyResizePanelUI();
        }
    }
}

public void ShowResizeControlsOnly()
{
    candidatesActive = false;
    ShowOnlyResizePanelUI();
}

public void ShowCandidateLoadingState()
{
    // Called immediately after Place is pressed and valid options are about to load.
    // Hide sliders/dropdown/place so the user sees the fitted-box pulse while
    // the first candidate is loading.
    candidatesActive = false;

    if (hideBottomPanelDuringLoadingStates)
        SetBottomPanelVisible(false);
    else
        SetBottomPanelVisibleForNormalUI(false);

    SetUIGroupVisible(modeSelectionPanel, false, false);
    SetUIGroupVisible(panel, false, false);
    SetUIGroupVisible(candidateButtonsParent, false, false);

    SetModeButtonsVisible(false);
    SetResizeControlsVisible(false);
    SetCandidateNavigationButtonsVisible(false);
}

public void ShowCandidateSelectionOnly()
{
    candidatesActive = true;
    ShowOnlyCandidateButtonsUI(true);
}

public void SetValidOptionsCount(int count)
{
    if (validOptionsText == null)
        return;

    StyleValidOptionsText();

    validOptionsText.text = $"<b>VALID OPTIONS   {count}</b>";

    // Force TextMeshPro to rebuild with the new font size immediately.
    validOptionsText.ForceMeshUpdate(true, true);
    Canvas.ForceUpdateCanvases();

    Debug.Log(
        $"VALID OPTIONS SIZE CHECK: " +
        $"name={validOptionsText.name}, " +
        $"fontSize={validOptionsText.fontSize}, " +
        $"autoSize={validOptionsText.enableAutoSizing}, " +
        $"localScale={validOptionsText.transform.localScale}, " +
        $"lossyScale={validOptionsText.transform.lossyScale}"
    );
}

public void ClearValidOptionsCount()
{
    if (validOptionsText == null)
        return;

    StyleValidOptionsText();

    validOptionsText.text = "<b>VALID OPTIONS   0</b>";

    validOptionsText.ForceMeshUpdate(true, true);
    Canvas.ForceUpdateCanvases();
}

private void UpdateValidOptionsCountFromCurrentBox()
{
    if (validOptionsText == null)
        return;

    if (targetBox == null)
    {
        SetValidOptionsCount(0);
        return;
    }

    if (lassoDrawer == null)
    {
        StyleValidOptionsText();
        validOptionsText.text = emphasizeDimensionAndOptionsText
            ? $"<size={emphasizedValidOptionsFontSize}><b>VALID OPTIONS   ?</b></size>"
            : "<b>VALID OPTIONS   ?</b>";
        return;
    }

    string selectedObjectType = GetSelectedObjectType();

    if (string.IsNullOrWhiteSpace(selectedObjectType))
    {
        SetValidOptionsCount(0);
        return;
    }

    Vector3 sizeMeters = targetBox.localScale;

    // IMPORTANT: fittedBox.localScale.x = width, y = height, z = length.
    // LassoDrawer.CountValidMeshesForDimensions expects length, width, height.
    int count = lassoDrawer.CountValidMeshesForDimensions(
        Mathf.Abs(sizeMeters.z),
        Mathf.Abs(sizeMeters.x),
        Mathf.Abs(sizeMeters.y),
        selectedObjectType
    );

    SetValidOptionsCount(count);
}

private void OnLoadOptionsClicked()
{
    if (currentMode != InteractionMode.Draw)
    {
        Debug.LogWarning("Load Options ignored because mode is not Draw.");
        return;
    }

    UpdateValidOptionsCountFromCurrentBox();

    if (lassoDrawer != null)
    {
        // Hide the slider/dropdown/place UI immediately. LassoDrawer will restore
        // the resize panel if no valid options are found, or show candidate buttons
        // after the first preview loads.
        ShowCandidateLoadingState();
        lassoDrawer.PrepareCandidatesFromCurrentUI();
    }
    else
    {
        Debug.LogWarning("BoxScaleUI: LassoDrawer is missing.");
    }
}

private void OnPreviousClicked()
{
    if (lassoDrawer != null)
        lassoDrawer.ShowPreviousCandidate();
}

private void OnNextClicked()
{
    if (lassoDrawer != null)
        lassoDrawer.ShowNextCandidate();
}

private void OnConfirmClicked()
{
    if (lassoDrawer == null)
        return;

    if (confirmBoxHoldRoutine != null)
    {
        StopCoroutine(confirmBoxHoldRoutine);
        confirmBoxHoldRoutine = null;
    }

    // Hide candidate buttons immediately.
    SetCandidateButtonsVisible(false);

    // Confirm the object and let LassoDrawer clear/destroy the fitted box.
    // Passing false means: do NOT keep/re-show the old fitted box.
    lassoDrawer.ConfirmCurrentCandidate(false);

    candidatesActive = false;
    targetBox = null;

    if (panel != null)
        panel.SetActive(false);

    ClearValidOptionsCount();

    ShowModeSelectionOnly();
}

private IEnumerator ClearConfirmedBoxAfterDelay(GameObject confirmedBox)
{
    yield return new WaitForSeconds(boxHoldSecondsAfterConfirm);

    if (confirmedBox != null)
    {
        BoxConfirmPulse pulse = confirmedBox.GetComponent<BoxConfirmPulse>();
        if (pulse != null)
            pulse.BeginFadeOut();
    }

    yield return new WaitForSeconds(0.35f);

    if (confirmedBox != null)
    {
        if (destroyConfirmedBoxAfterHold)
            Destroy(confirmedBox);
        else
            confirmedBox.SetActive(false);
    }

    confirmBoxHoldRoutine = null;
    ShowModeSelectionOnly();
}

private void AddConfirmedBoxAnimation(GameObject box)
{
    if (box == null) return;

    BoxConfirmPulse pulse = box.GetComponent<BoxConfirmPulse>();
    if (pulse == null)
        pulse = box.AddComponent<BoxConfirmPulse>();

    pulse.pulseSpeed = confirmedBoxPulseSpeed;
    pulse.pulseScaleAmount = confirmedBoxPulseScaleAmount;
    pulse.pulseColor = confirmedBoxPulseColor;
    pulse.brightColor = confirmedBoxBrightColor;
}

private void ConfigureSlider(Slider slider)
{
    if (slider == null)
        return;

    slider.minValue = minScale;
    slider.maxValue = maxScale;
    slider.wholeNumbers = false;
}

private void ExpandSliderRangeToFitValue(Slider slider, float value)
{
    if (slider == null)
        return;

    float safeValue = Mathf.Abs(value);

    if (safeValue < slider.minValue)
        slider.minValue = Mathf.Min(minScale, safeValue);

    if (safeValue > slider.maxValue)
    {
        slider.maxValue = Mathf.Max(safeValue * 1.15f, safeValue + 0.05f);
        maxScale = Mathf.Max(maxScale, slider.maxValue);
    }

    if (slider.maxValue <= slider.minValue)
        slider.maxValue = slider.minValue + 0.1f;
}

private void ExpandSliderRangesToFitScale(Vector3 scale)
{
    ExpandSliderRangeToFitValue(xSlider, scale.x);
    ExpandSliderRangeToFitValue(ySlider, scale.y);
    ExpandSliderRangeToFitValue(zSlider, scale.z);
}

private void MarkCandidateSelectionDirty(bool clearValidCount = true)
{
    // Keep this method for non-resize flows, but avoid sending the UI back to
    // Draw/Erase just because the current candidate list became stale.
    SetCandidateButtonsVisible(false);

    if (clearValidCount)
        ClearValidOptionsCount();

    if (lassoDrawer != null)
        lassoDrawer.ClearCurrentCandidates();
}

private void ClearOnlyCurrentCandidatePreview(bool clearValidCount = false)
{
    // Used by sliders/dropdown while the resize panel is open.
    // It clears stale mesh previews, but preserves the resize UI state.
    candidatesActive = false;

    SetUIGroupVisible(candidateButtonsParent, false, false);
    SetCandidateNavigationButtonsVisible(false);

    if (clearValidCount)
        ClearValidOptionsCount();

    if (lassoDrawer != null)
        lassoDrawer.ClearCurrentCandidates();

    ShowOnlyResizePanelUI();
}

private void ApplyBoxScale(Vector3 newScale)
{
    if (targetBox == null)
        return;

    if (lassoDrawer != null)
    {
        lassoDrawer.SetFittedBoxScaleBottomAnchored(newScale);
    }
    else
    {
        Vector3 bottomBefore = GetTargetBoxBottomWorldPosition();

        targetBox.localScale = newScale;
        Physics.SyncTransforms();

        Vector3 bottomAfter = GetTargetBoxBottomWorldPosition();

        targetBox.position += bottomBefore - bottomAfter;
    }
}

private Vector3 GetTargetBoxBottomWorldPosition()
{
    if (targetBox == null)
        return Vector3.zero;

    float worldHeight = targetBox.TransformVector(Vector3.up).magnitude;

    return targetBox.position - targetBox.up * (worldHeight * 0.5f);
}

private void OnXSliderChanged(float value)
{
    if (suppressCallbacks || targetBox == null)
        return;

    Vector3 scale = targetBox.localScale;
    scale.x = value;

    ApplyBoxScale(scale);

    UpdateDimensionTexts();
    UpdateValidOptionsCountFromCurrentBox();
    UpdateSliderVisuals();
    ClearOnlyCurrentCandidatePreview(false);
}

private void OnYSliderChanged(float value)
{
    if (suppressCallbacks || targetBox == null)
        return;

    Vector3 scale = targetBox.localScale;
    scale.y = value;

    ApplyBoxScale(scale);

    UpdateDimensionTexts();
    UpdateValidOptionsCountFromCurrentBox();
    UpdateSliderVisuals();
    ClearOnlyCurrentCandidatePreview(false);
}

private void OnZSliderChanged(float value)
{
    if (suppressCallbacks || targetBox == null)
        return;

    Vector3 scale = targetBox.localScale;
    scale.z = value;

    ApplyBoxScale(scale);

    UpdateDimensionTexts();
    UpdateValidOptionsCountFromCurrentBox();
    UpdateSliderVisuals();
    ClearOnlyCurrentCandidatePreview(false);
}

public void SetSelectedObjectTypeFromModernDropdown(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return;

    selectedObjectTypeFromModernDropdown = value.Trim();

    UpdateValidOptionsCountFromCurrentBox();

    if (targetBox != null)
        ClearOnlyCurrentCandidatePreview(false);
    else
        MarkCandidateSelectionDirty(false);
}

private void OnDropdownChanged(int value)
{
    if (suppressCallbacks)
        return;

    UpdateValidOptionsCountFromCurrentBox();

    if (targetBox != null)
        ClearOnlyCurrentCandidatePreview(false);
    else
        MarkCandidateSelectionDirty(false);
}

private void UpdateDimensionTexts()
{
    if (!useSeparateDimensionText)
    {
        if (updateMUIPSliderText)
            UpdateMUIPSliderLabels();

        return;
    }

    if (targetBox == null)
    {
        SetDimensionText(xDimensionText, "X", 0f);
        SetDimensionText(yDimensionText, "Y", 0f);
        SetDimensionText(zDimensionText, "Z", 0f);
        return;
    }

    Vector3 scale = targetBox.localScale;

    SetDimensionText(xDimensionText, "X", scale.x);
    SetDimensionText(yDimensionText, "Y", scale.y);
    SetDimensionText(zDimensionText, "Z", scale.z);
}

private void SetDimensionText(TMP_Text textField, string label, float meters)
{
    if (textField == null)
        return;

    string displayLabel = label;

    if (label == "X") displayLabel = "Width";
    if (label == "Y") displayLabel = "Height";
    if (label == "Z") displayLabel = useLengthLabelForZDimension ? "Length" : "Depth";

    textField.richText = true;
    textField.fontStyle = FontStyles.Bold;
    textField.fontSize = emphasizeDimensionAndOptionsText
        ? emphasizedDimensionFontSize
        : dimensionTextFontSize;
    textField.color = useCompactBottomUIStyle ? sliderTextColor : secondaryTextColor;
    textField.overrideColorTags = true;

    float displayValue = showDimensionsInInches ? meters * METERS_TO_INCHES : meters;
    string unit = showDimensionsInInches ? "in" : "m";
    string valueText = showDimensionsInInches ? displayValue.ToString("F1") : displayValue.ToString("F2");
    string displayText = $"{displayLabel.ToUpper()}   {valueText} {unit}";

    textField.text = emphasizeDimensionAndOptionsText
        ? $"<size={emphasizedDimensionFontSize}><b>{displayText}</b></size>"
        : $"{displayLabel.ToUpper()}   <b>{valueText}</b> {unit}";
    textField.SetVerticesDirty();
}

private void UpdateMUIPSliderLabels()
{
    UpdateMUIPSliderLabel(xSliderMUIP, "Width", xSlider, targetBox != null ? targetBox.localScale.x : (xSlider != null ? xSlider.value : 0f));
    UpdateMUIPSliderLabel(ySliderMUIP, "Height", ySlider, targetBox != null ? targetBox.localScale.y : (ySlider != null ? ySlider.value : 0f));
    UpdateMUIPSliderLabel(zSliderMUIP, useLengthLabelForZDimension ? "Length" : "Depth", zSlider, targetBox != null ? targetBox.localScale.z : (zSlider != null ? zSlider.value : 0f));
}

private void UpdateMUIPSliderLabel(SliderManager sliderManager, string label, Slider slider, float metersFromBox)
{
    if (sliderManager == null || slider == null)
        return;

    // Use the target box scale rather than Slider.value. Slider.value can be clamped
    // if the previous maxScale was smaller than Gemini's returned dimension.
    float meters = metersFromBox;
    float displayValue = showDimensionsInInches ? meters * METERS_TO_INCHES : meters;
    string unit = showDimensionsInInches ? "in" : "m";
    string valueText = showDimensionsInInches ? displayValue.ToString("F1") : displayValue.ToString("F2");
    string displayText = $"{label}: {valueText} {unit}";

    TMP_Text text = FindBestMUIPSliderText(sliderManager.gameObject);
    if (text == null)
        return;

    text.richText = true;
    text.fontStyle = FontStyles.Bold;
    text.fontSize = emphasizeDimensionAndOptionsText
        ? emphasizedDimensionFontSize
        : muipSliderTextFontSize;
    text.color = useCompactBottomUIStyle ? sliderTextColor : secondaryTextColor;
    text.overrideColorTags = true;
    text.text = emphasizeDimensionAndOptionsText
        ? $"<size={emphasizedDimensionFontSize}><b>{displayText}</b></size>"
        : displayText;
    text.alignment = TextAlignmentOptions.MidlineLeft;
    text.enableWordWrapping = false;
    text.overflowMode = TextOverflowModes.Overflow;
    text.raycastTarget = false;

    RectTransform rect = text.GetComponent<RectTransform>();
    if (rect != null)
        rect.sizeDelta = new Vector2(muipSliderTextWidth, rect.sizeDelta.y);

    LayoutElement layout = text.GetComponent<LayoutElement>();
    if (layout == null)
        layout = text.gameObject.AddComponent<LayoutElement>();

    layout.preferredWidth = muipSliderTextWidth;
    layout.minWidth = muipSliderTextWidth;
    layout.flexibleWidth = 0f;

    text.SetVerticesDirty();
}

private TMP_Text FindBestMUIPSliderText(GameObject root)
{
    if (root == null)
        return null;

    TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
    if (texts == null || texts.Length == 0)
        return null;

    // Prefer text objects that look like the built-in MUIP slider label/value.
    foreach (TMP_Text text in texts)
    {
        if (text == null) continue;

        string name = text.gameObject.name.ToLowerInvariant();
        if (name.Contains("value") || name.Contains("label") || name.Contains("title") || name.Contains("text"))
            return text;
    }

    return texts[0];
}

private void StyleMUIPSliderText(SliderManager sliderManager)
{
    if (sliderManager == null)
        return;

    TMP_Text[] texts = sliderManager.GetComponentsInChildren<TMP_Text>(true);

    foreach (TMP_Text text in texts)
    {
        if (text == null)
            continue;

        text.color = useCompactBottomUIStyle ? sliderTextColor : secondaryTextColor;
        text.overrideColorTags = true;
        text.enableAutoSizing = false;
        text.fontSize = emphasizeDimensionAndOptionsText
            ? emphasizedDimensionFontSize
            : muipSliderTextFontSize;
        text.fontStyle = emphasizeDimensionAndOptionsText ? FontStyles.Bold : FontStyles.Normal;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.characterSpacing = textCharacterSpacing;
        text.richText = true;
        text.overrideColorTags = true;
        text.raycastTarget = false;

        RectTransform textRect = text.GetComponent<RectTransform>();
        if (textRect != null)
            textRect.sizeDelta = new Vector2(muipSliderTextWidth, textRect.sizeDelta.y);

        LayoutElement textLayout = text.GetComponent<LayoutElement>();
        if (textLayout == null)
            textLayout = text.gameObject.AddComponent<LayoutElement>();

        textLayout.preferredWidth = muipSliderTextWidth;
        textLayout.minWidth = muipSliderTextWidth;
        textLayout.flexibleWidth = 0f;

        text.SetVerticesDirty();
    }
}

private void ForceAllSliderTextColors()
{
    ForceSliderTextColorForRoot(modernXSliderRoot);
    ForceSliderTextColorForRoot(modernYSliderRoot);
    ForceSliderTextColorForRoot(modernZSliderRoot);

    if (xSliderMUIP != null) ForceSliderTextColorForRoot(xSliderMUIP.gameObject);
    if (ySliderMUIP != null) ForceSliderTextColorForRoot(ySliderMUIP.gameObject);
    if (zSliderMUIP != null) ForceSliderTextColorForRoot(zSliderMUIP.gameObject);

    if (useSeparateDimensionText)
    {
        ForceSingleTMPColor(xDimensionText);
        ForceSingleTMPColor(yDimensionText);
        ForceSingleTMPColor(zDimensionText);
    }
}

private void ForceSliderTextColorForRoot(GameObject root)
{
    if (root == null)
        return;

    TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
    foreach (TMP_Text text in texts)
    {
        ForceSingleTMPColor(text);
    }
}

private void ForceSingleTMPColor(TMP_Text text)
{
    if (text == null)
        return;

    text.color = useCompactBottomUIStyle ? sliderTextColor : secondaryTextColor;
    text.overrideColorTags = true;
    text.enableAutoSizing = false;
    text.fontSize = useCompactBottomUIStyle
        ? compactMUIPSliderTextFontSize
        : muipSliderTextFontSize;
    text.fontStyle = FontStyles.Bold;
    text.enableWordWrapping = false;
    text.overflowMode = TextOverflowModes.Overflow;
    text.raycastTarget = false;

    RectTransform rect = text.GetComponent<RectTransform>();
    if (rect != null)
    {
        float width = useCompactBottomUIStyle
            ? compactMUIPSliderTextWidth
            : muipSliderTextWidth;

        rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);
    }

    LayoutElement layout = text.GetComponent<LayoutElement>();
    if (layout == null)
        layout = text.gameObject.AddComponent<LayoutElement>();

    layout.preferredWidth = useCompactBottomUIStyle
        ? compactMUIPSliderTextWidth
        : muipSliderTextWidth;
    layout.minWidth = layout.preferredWidth;
    layout.flexibleWidth = 0f;

    text.SetVerticesDirty();
}

private void ForceAllDropdownTextSizes()
{
    ForceDropdownTextSize(modernDropdownRoot);

    if (objectTypeDropdownMUIP != null)
        ForceDropdownTextSize(objectTypeDropdownMUIP.gameObject);

    if (objectTypeDropdown != null)
        ForceDropdownTextSize(objectTypeDropdown.gameObject);
}

private void ForceDropdownTextSize(GameObject dropdownRoot)
{
    if (dropdownRoot == null)
        return;

    TMP_Text[] texts = dropdownRoot.GetComponentsInChildren<TMP_Text>(true);

    foreach (TMP_Text text in texts)
    {
        if (text == null)
            continue;

        text.color = dropdownTextColor;
        text.overrideColorTags = true;
        text.enableAutoSizing = false;
        text.fontSize = dropdownFontSize;
        text.fontStyle = FontStyles.Normal;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.characterSpacing = textCharacterSpacing;
        text.SetVerticesDirty();
    }
}

public void SetDefaultDropdownOption(string defaultText)
{
    if (string.IsNullOrWhiteSpace(defaultText))
        return;

    string trimmedDefault = defaultText.Trim();
    selectedObjectTypeFromModernDropdown = trimmedDefault;

    // Modern UI Pack dropdown
    if (objectTypeDropdownMUIP != null &&
        objectTypeDropdownMUIP.items != null &&
        objectTypeDropdownMUIP.items.Count > 0)
    {
        for (int i = 0; i < objectTypeDropdownMUIP.items.Count; i++)
        {
            string itemName = objectTypeDropdownMUIP.items[i].itemName;

            if (!string.IsNullOrWhiteSpace(itemName) &&
                string.Equals(itemName.Trim(), trimmedDefault, System.StringComparison.OrdinalIgnoreCase))
            {
                objectTypeDropdownMUIP.ChangeDropdownInfo(i);
                return;
            }
        }
    }

    // Fallback TMP dropdown
    if (objectTypeDropdown != null &&
        objectTypeDropdown.options != null &&
        objectTypeDropdown.options.Count > 0)
    {
        for (int i = 0; i < objectTypeDropdown.options.Count; i++)
        {
            string optionText = objectTypeDropdown.options[i].text;

            if (!string.IsNullOrWhiteSpace(optionText) &&
                string.Equals(
                    optionText.Trim(),
                    trimmedDefault,
                    System.StringComparison.OrdinalIgnoreCase
                ))
            {
                objectTypeDropdown.value = i;
                objectTypeDropdown.RefreshShownValue();
                return;
            }
        }
    }

    Debug.LogWarning("Default dropdown option not found: " + defaultText);
}

public string GetSelectedObjectType()
{
    // Modern UI Pack CustomDropdown
    if (objectTypeDropdownMUIP != null &&
        objectTypeDropdownMUIP.items != null &&
        objectTypeDropdownMUIP.items.Count > 0)
    {
        int index = objectTypeDropdownMUIP.selectedItemIndex;

        if (index >= 0 && index < objectTypeDropdownMUIP.items.Count)
        {
            string selected = objectTypeDropdownMUIP.items[index].itemName;

            if (!string.IsNullOrWhiteSpace(selected))
            {
                selectedObjectTypeFromModernDropdown = selected.Trim();
                return selectedObjectTypeFromModernDropdown;
            }
        }
    }

    // Fallback TMP dropdown
    if (objectTypeDropdown != null &&
        objectTypeDropdown.options != null &&
        objectTypeDropdown.options.Count > 0)
    {
        int index = objectTypeDropdown.value;

        if (index >= 0 && index < objectTypeDropdown.options.Count)
        {
            string selected = objectTypeDropdown.options[index].text;

            if (!string.IsNullOrWhiteSpace(selected))
            {
                selectedObjectTypeFromModernDropdown = selected.Trim();
                return selectedObjectTypeFromModernDropdown;
            }
        }
    }

    // Manual fallback for older/custom dropdown setups.
    if (!string.IsNullOrWhiteSpace(selectedObjectTypeFromModernDropdown))
        return selectedObjectTypeFromModernDropdown.Trim();

    Debug.LogWarning("No object type selected. Assign objectTypeDropdownMUIP, a TMP_Dropdown, or call SetSelectedObjectTypeFromModernDropdown.");
    return null;
}

}
public class UIButtonPressScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler{public float pressedScale = 0.96f;public float animationSpeed = 16f;

private Vector3 originalScale;
private Vector3 targetScale;

private void Awake()
{
    originalScale = transform.localScale;
    targetScale = originalScale;
}

private void OnEnable()
{
    originalScale = transform.localScale;
    targetScale = originalScale;
}

private void Update()
{
    transform.localScale = Vector3.Lerp(
        transform.localScale,
        targetScale,
        Time.unscaledDeltaTime * animationSpeed
    );
}

public void OnPointerDown(PointerEventData eventData)
{
    targetScale = originalScale * pressedScale;
}

public void OnPointerUp(PointerEventData eventData)
{
    targetScale = originalScale;
}

public void OnPointerExit(PointerEventData eventData)
{
    targetScale = originalScale;
}

}

public class UIFadeScaleAnimator : MonoBehaviour{public float animationSpeed = 14f;public float startScale = 0.94f;

private CanvasGroup canvasGroup;
private Vector3 originalScale;
private bool initialized = false;

private void Awake()
{
    Initialize();
}

private void OnEnable()
{
    Initialize();

    canvasGroup.alpha = 0f;
    transform.localScale = originalScale * startScale;
}

private void Update()
{
    if (!initialized) return;

    canvasGroup.alpha = Mathf.Lerp(
        canvasGroup.alpha,
        1f,
        Time.unscaledDeltaTime * animationSpeed
    );

    transform.localScale = Vector3.Lerp(
        transform.localScale,
        originalScale,
        Time.unscaledDeltaTime * animationSpeed
    );
}

private void Initialize()
{
    if (initialized) return;

    canvasGroup = GetComponent<CanvasGroup>();
    if (canvasGroup == null)
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

    originalScale = transform.localScale;
    initialized = true;
}

}

public class BoxConfirmPulse : MonoBehaviour{public float pulseSpeed = 4f;public float pulseScaleAmount = 0.035f;public Color pulseColor = new Color(0.55f, 1f, 0.58f, 0.85f);public Color brightColor = new Color(0.90f, 1f, 0.95f, 1f);

private Vector3 originalScale;
private Renderer[] renderers;
private Material[] runtimeMaterials;

private bool fadingOut = false;
private float fadeAlpha = 1f;

private void Awake()
{
    Initialize();
}

private void OnEnable()
{
    Initialize();
    fadeAlpha = 1f;
    fadingOut = false;
}

private void Initialize()
{
    originalScale = transform.localScale;

    renderers = GetComponentsInChildren<Renderer>(true);
    List<Material> materials = new List<Material>();

    foreach (Renderer renderer in renderers)
    {
        if (renderer == null) continue;

        Material mat = renderer.material;
        materials.Add(mat);

        Color startColor = pulseColor;
        startColor.a = Mathf.Max(startColor.a, 0.75f);
        mat.color = startColor;

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", startColor * 0.8f);
        }
    }

    runtimeMaterials = materials.ToArray();
}

private void Update()
{
    float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
    float pulse = 1f + wave * pulseScaleAmount;

    transform.localScale = originalScale * pulse;

    Color animatedColor = Color.Lerp(pulseColor, brightColor, wave);
    animatedColor.a = pulseColor.a * fadeAlpha;

    foreach (Material mat in runtimeMaterials)
    {
        if (mat == null) continue;

        mat.color = animatedColor;

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", animatedColor * 0.8f);
        }
    }

    if (fadingOut)
    {
        fadeAlpha = Mathf.MoveTowards(fadeAlpha, 0f, Time.deltaTime * 3f);

        if (fadeAlpha <= 0.01f)
        {
            gameObject.SetActive(false);
        }
    }
}

public void BeginFadeOut()
{
    fadingOut = true;
}

}
