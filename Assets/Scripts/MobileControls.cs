#pragma warning disable 0618
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.EventSystems;

public class MobileControls : MonoBehaviour
{
    public static MobileControls instance;
    public bool IsInteractPressedThisFrame { get; private set; }

    [Header("Settings")]
    public bool forceShowInEditor = true;
    public bool isFloatingJoystick = false;

    [Header("Colors")]
    public Color themeColor = new Color(1f, 1f, 1f, 1f); // Elegant white glow
    public Color joystickBackgroundColor = new Color(0f, 0f, 0f, 0.45f);
    public Color joystickHandleColor = new Color(0f, 0f, 0f, 0.85f);
    public Color buttonColor = new Color(0f, 0f, 0f, 0.4f);

    private GameObject canvasInstance;
    private GameObject interactButtonObj;
    private RectTransform interactButtonRect;

    private void Awake()
    {
        instance = this;
    }

    private void LateUpdate()
    {
        IsInteractPressedThisFrame = false;
    }

    public void SetInteractActive(bool active)
    {
        if (interactButtonObj != null)
        {
            interactButtonObj.SetActive(active);
        }
    }

    public void SetActive(bool active)
    {
        if (canvasInstance != null)
        {
            canvasInstance.SetActive(active);
        }
    }

    private void Start()
    {
        // Force themeColor to be neutral white/grey to remove any blue tint
        themeColor = Color.white;

        // Check if we should enable mobile controls
        bool isMobile = Application.platform == RuntimePlatform.Android || 
                        Application.platform == RuntimePlatform.IPhonePlayer;
        
        if (isMobile || forceShowInEditor)
        {
            SetupMobileUI();
        }
    }

    private void SetupMobileUI()
    {
        Font font = GetDefaultFont();

        // 1. Create Canvas
        canvasInstance = new GameObject("MobileControlsCanvas");
        Canvas canvas = canvasInstance.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasInstance.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasInstance.AddComponent<GraphicRaycaster>();

        // Create Touch Camera Zone (taking up the entire screen, placed at the back)
        GameObject cameraZoneObj = new GameObject("TouchCameraZone", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cameraZoneObj.transform.SetParent(canvasInstance.transform, false);
        Image zoneImg = cameraZoneObj.GetComponent<Image>();
        zoneImg.color = new Color(0f, 0f, 0f, 0.005f); // Transparent but raycastable
        
        RectTransform zoneRect = cameraZoneObj.GetComponent<RectTransform>();
        zoneRect.anchorMin = Vector2.zero;
        zoneRect.anchorMax = Vector2.one; // Fullscreen
        zoneRect.offsetMin = Vector2.zero;
        zoneRect.offsetMax = Vector2.zero;
        zoneRect.localScale = Vector3.one;
        
        TouchCameraZone cameraZone = cameraZoneObj.AddComponent<TouchCameraZone>();
        cameraZone.controlPath = "<Gamepad>/rightStick";

        // Ensure EventSystem exists in the scene and works with the new Input System
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
        }

        // Replace old StandaloneInputModule if present because it fails under the new Input System
        StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldModule != null)
        {
            DestroyImmediate(oldModule);
        }

        #if UNITY_2020_3_OR_NEWER
        if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
        #else
        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
        #endif

        // 2. Create Joystick Touch Area (Left side of screen) using RectTransform directly
        GameObject touchAreaObj = new GameObject("JoystickTouchArea", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        touchAreaObj.transform.SetParent(canvasInstance.transform, false);
        Image touchImg = touchAreaObj.GetComponent<Image>();
        touchImg.color = new Color(0, 0, 0, 0.005f); // Almost fully transparent but raycast-able

        RectTransform touchRect = touchAreaObj.GetComponent<RectTransform>();
        touchRect.anchorMin = new Vector2(0, 0);
        touchRect.anchorMax = new Vector2(0.5f, 1f); // Left half of screen
        touchRect.pivot = new Vector2(0f, 0f); // Bottom-left pivot for exact screen coordinates
        touchRect.offsetMin = Vector2.zero;
        touchRect.offsetMax = Vector2.zero;
        touchRect.localScale = Vector3.one;

        // 3. Create Joystick Background (Parented directly to Canvas for rendering stability, using UICircle)
        GameObject joyBg = new GameObject("JoystickBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(UICircle));
        joyBg.transform.SetParent(canvasInstance.transform, false);
        UICircle bgCircle = joyBg.GetComponent<UICircle>();
        bgCircle.color = joystickBackgroundColor;
        bgCircle.raycastTarget = false; // Disable raycast to prevent blocking Touch Area

        RectTransform bgRect = joyBg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(0, 0);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = new Vector2(300, 300);
        bgRect.sizeDelta = new Vector2(240, 240);
        bgRect.localScale = Vector3.one;

        // Joystick Background Outline
        GameObject joyBgBorder = new GameObject("Border", typeof(RectTransform), typeof(CanvasRenderer), typeof(UICircle));
        joyBgBorder.transform.SetParent(joyBg.transform, false);
        UICircle bgBorderCircle = joyBgBorder.GetComponent<UICircle>();
        bgBorderCircle.color = new Color(1f, 1f, 1f, 0.15f);
        bgBorderCircle.thickness = 3f;
        bgBorderCircle.raycastTarget = false;
        
        RectTransform bgBorderRect = joyBgBorder.GetComponent<RectTransform>();
        bgBorderRect.anchorMin = Vector2.zero;
        bgBorderRect.anchorMax = Vector2.one;
        bgBorderRect.offsetMin = Vector2.zero;
        bgBorderRect.offsetMax = Vector2.zero;
        bgBorderRect.localScale = Vector3.one;

        // Directional Indicators (UP, DOWN, LEFT, RIGHT)
        string[] dirs = { "▲", "▼", "◀", "▶" };
        Vector2[] positions = { new Vector2(0, 90), new Vector2(0, -90), new Vector2(-90, 0), new Vector2(90, 0) };
        for (int i = 0; i < 4; i++)
        {
            GameObject dirObj = new GameObject("Indicator_" + i, typeof(RectTransform));
            dirObj.transform.SetParent(joyBg.transform, false);
            Text dirText = dirObj.AddComponent<Text>();
            dirText.text = dirs[i];
            dirText.font = font;
            dirText.fontSize = 14;
            dirText.color = new Color(1f, 1f, 1f, 0.35f);
            dirText.alignment = TextAnchor.MiddleCenter;
            dirText.raycastTarget = false;

            RectTransform dirRect = dirObj.GetComponent<RectTransform>();
            dirRect.anchorMin = new Vector2(0.5f, 0.5f);
            dirRect.anchorMax = new Vector2(0.5f, 0.5f);
            dirRect.pivot = new Vector2(0.5f, 0.5f);
            dirRect.anchoredPosition = positions[i];
            dirRect.sizeDelta = new Vector2(30, 30);
            dirRect.localScale = Vector3.one;
        }

        // 4. Create Joystick Handle (using UICircle)
        GameObject joyHandle = new GameObject("JoystickHandle", typeof(RectTransform), typeof(CanvasRenderer), typeof(UICircle));
        joyHandle.transform.SetParent(joyBg.transform, false);
        UICircle handleCircle = joyHandle.GetComponent<UICircle>();
        handleCircle.color = joystickHandleColor;
        handleCircle.raycastTarget = false; // Disable raycast to prevent blocking Touch Area

        RectTransform handleRect = joyHandle.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        handleRect.sizeDelta = new Vector2(110, 110);
        handleRect.localScale = Vector3.one;

        // Joystick Handle Inner Ring
        GameObject handleRing = new GameObject("InnerRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(UICircle));
        handleRing.transform.SetParent(joyHandle.transform, false);
        UICircle handleRingCircle = handleRing.GetComponent<UICircle>();
        handleRingCircle.color = new Color(1f, 1f, 1f, 0.25f);
        handleRingCircle.thickness = 3f;
        handleRingCircle.raycastTarget = false;
        
        RectTransform handleRingRect = handleRing.GetComponent<RectTransform>();
        handleRingRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRingRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRingRect.pivot = new Vector2(0.5f, 0.5f);
        handleRingRect.anchoredPosition = Vector2.zero;
        handleRingRect.sizeDelta = new Vector2(75, 75);
        handleRingRect.localScale = Vector3.one;

        // Joystick Handle Center Dot
        GameObject handleDot = new GameObject("CenterDot", typeof(RectTransform), typeof(CanvasRenderer), typeof(UICircle));
        handleDot.transform.SetParent(joyHandle.transform, false);
        UICircle handleDotCircle = handleDot.GetComponent<UICircle>();
        handleDotCircle.color = new Color(1f, 1f, 1f, 0.4f);
        handleDotCircle.raycastTarget = false;
        
        RectTransform handleDotRect = handleDot.GetComponent<RectTransform>();
        handleDotRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleDotRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleDotRect.pivot = new Vector2(0.5f, 0.5f);
        handleDotRect.anchoredPosition = Vector2.zero;
        handleDotRect.sizeDelta = new Vector2(18, 18);
        handleDotRect.localScale = Vector3.one;

        // Add PremiumJoystick component to the Touch Area
        PremiumJoystick joystick = touchAreaObj.AddComponent<PremiumJoystick>();
        joystick.movementRange = 85f;
        joystick.isFloating = isFloatingJoystick;
        joystick.backgroundRect = bgRect;
        joystick.handleRect = handleRect;
        joystick.canvasGroup = joyBg.AddComponent<CanvasGroup>();
        joystick.controlPath = "<Gamepad>/leftStick";
        joystick.activeGlowColor = themeColor;

        // 5. Create Jump Button (South Button) with Arrow Icon
        CreatePremiumButton("JumpButton", new Vector2(-250, 200), new Vector2(140, 140), "▲", "<Gamepad>/buttonSouth", font);

        // 6. Create Run/Sprint Button (East Button) with Lightning Bolt Icon
        CreatePremiumButton("SprintButton", new Vector2(-430, 160), new Vector2(110, 110), "⚡", "<Gamepad>/buttonEast", font);

        // 7. Create Interact/Collect Button with Diamond Icon
        PremiumButton interactBtn = CreatePremiumButton("InteractButton", new Vector2(120, -50), new Vector2(120, 120), "◆", "", font);
        interactBtn.OnPressed = () => {
            IsInteractPressedThisFrame = true;
        };
        interactButtonObj = interactBtn.gameObject;
        interactButtonRect = interactBtn.GetComponent<RectTransform>();

        // Re-anchor to the center of the screen
        interactButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        interactButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        interactButtonRect.pivot = new Vector2(0.5f, 0.5f);
        interactButtonRect.anchoredPosition = new Vector2(120, -50);

        // Add Prompt Text above the button
        GameObject promptObj = new GameObject("PromptText", typeof(RectTransform));
        promptObj.transform.SetParent(interactButtonObj.transform, false);
        Text promptText = promptObj.AddComponent<Text>();
        promptText.text = "Ambil Crystal";
        promptText.color = Color.white;
        promptText.fontSize = 20;
        promptText.font = font;
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.raycastTarget = false;
        
        RectTransform promptRect = promptObj.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 1f);
        promptRect.anchorMax = new Vector2(0.5f, 1f);
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.anchoredPosition = new Vector2(0, 15);
        promptRect.sizeDelta = new Vector2(200, 30);
        promptRect.localScale = Vector3.one;

        interactButtonObj.SetActive(false); // Hide by default
    }

    private Font GetDefaultFont()
    {
        Font font = null;
        try
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch {}

        if (font == null)
        {
            try
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch {}
        }

        if (font == null)
        {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            if (fonts != null && fonts.Length > 0)
            {
                font = fonts[0];
            }
        }
        return font;
    }

    private PremiumButton CreatePremiumButton(string name, Vector2 anchoredPosition, Vector2 size, string textLabel, string controlPath, Font font)
    {
        GameObject buttonObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UICircle));
        buttonObj.transform.SetParent(canvasInstance.transform, false);

        // Button Outer Solid Circle
        UICircle outerCircle = buttonObj.GetComponent<UICircle>();
        outerCircle.color = buttonColor;

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        // Button Border Outline
        GameObject borderObj = new GameObject("Border", typeof(RectTransform), typeof(CanvasRenderer), typeof(UICircle));
        borderObj.transform.SetParent(buttonObj.transform, false);
        UICircle borderCircle = borderObj.GetComponent<UICircle>();
        borderCircle.color = new Color(1f, 1f, 1f, 0.2f); // Thin white border
        borderCircle.thickness = 3f;
        borderCircle.raycastTarget = false;
        
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;
        borderRect.localScale = Vector3.one;

        // Inner glowing fill (activated on press)
        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(UICircle));
        fillObj.transform.SetParent(buttonObj.transform, false);
        UICircle fillCircle = fillObj.GetComponent<UICircle>();
        fillCircle.color = new Color(themeColor.r, themeColor.g, themeColor.b, 0f); // Hidden by default
        fillCircle.raycastTarget = false; // Disable raycast to let clicks reach the main button

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(10, 10);
        fillRect.offsetMax = new Vector2(-10, -10);
        fillRect.localScale = Vector3.one;

        // Text/Icon Label
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(buttonObj.transform, false);
        Text text = labelObj.AddComponent<Text>();
        text.text = textLabel;
        text.color = Color.white;
        text.fontSize = size.x > 120 ? 44 : 36;
        text.raycastTarget = false; // Disable raycast to let clicks reach the main button
        text.font = font;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        labelRect.localScale = Vector3.one;

        // Add PremiumButton component
        PremiumButton btn = buttonObj.AddComponent<PremiumButton>();
        btn.controlPath = controlPath;
        btn.buttonImage = outerCircle;
        btn.fillImage = fillCircle;
        btn.normalColor = buttonColor;
        btn.pressedColor = new Color(themeColor.r, themeColor.g, themeColor.b, 0.35f);
        btn.themeGlowColor = themeColor;
        return btn;
    }
}

public class PremiumJoystick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [InputControl(layout = "Vector2")]
    [SerializeField]
    private string m_ControlPath;

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    public float movementRange = 85f;
    public bool isFloating = false;
    public float fadeSpeed = 8f;
    public Color activeGlowColor;

    public RectTransform backgroundRect;
    public RectTransform handleRect;
    public CanvasGroup canvasGroup;

    private Vector2 startPosition;
    private Vector2 defaultPosition;
    private int pointerId = -1;
    private float targetAlpha = 0.8f;

    private Graphic backgroundImage;
    private Graphic handleImage;
    private Color originalBgColor;
    private Color originalHandleColor;

    private Coroutine bgScaleCoroutine;
    private Coroutine handleScaleCoroutine;

    private void Start()
    {
        defaultPosition = backgroundRect.anchoredPosition;
        backgroundImage = backgroundRect.GetComponent<Graphic>();
        handleImage = handleRect.GetComponent<Graphic>();
        
        if (backgroundImage != null) originalBgColor = backgroundImage.color;
        if (handleImage != null) originalHandleColor = handleImage.color;

        if (isFloating)
        {
            targetAlpha = 0f;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }
        else
        {
            targetAlpha = 0.8f;
            if (canvasGroup != null) canvasGroup.alpha = 0.8f;
        }
    }

    private void Update()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pointerId != -1) return;

        pointerId = eventData.pointerId;

        if (isFloating)
        {
            Vector3 worldPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                backgroundRect.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out worldPos
            );
            backgroundRect.position = worldPos;
            startPosition = backgroundRect.anchoredPosition;
            targetAlpha = 1f;
        }
        else
        {
            startPosition = defaultPosition;
        }

        handleRect.anchoredPosition = Vector2.zero;

        // Apply visual highlights & animations
        if (backgroundImage != null) backgroundImage.color = Color.Lerp(originalBgColor, activeGlowColor, 0.4f);
        if (handleImage != null) handleImage.color = Color.Lerp(originalHandleColor, activeGlowColor, 0.3f);

        StartScale(backgroundRect, ref bgScaleCoroutine, Vector3.one * 1.15f, 0.1f);
        StartScale(handleRect, ref handleScaleCoroutine, Vector3.one * 1.1f, 0.1f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId) return;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPos
        );

        float dist = localPos.magnitude;
        if (dist > movementRange)
        {
            localPos = localPos.normalized * movementRange;
        }

        handleRect.anchoredPosition = localPos;

        // Send value to Input System
        Vector2 inputVal = localPos / movementRange;
        SendValueToControl(inputVal);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId) return;

        pointerId = -1;
        handleRect.anchoredPosition = Vector2.zero;
        SendValueToControl(Vector2.zero);

        if (isFloating)
        {
            targetAlpha = 0f;
        }
        else
        {
            targetAlpha = 0.8f;
            backgroundRect.anchoredPosition = defaultPosition;
        }

        // Reset visual highlights & animations
        if (backgroundImage != null) backgroundImage.color = originalBgColor;
        if (handleImage != null) handleImage.color = originalHandleColor;

        StartScale(backgroundRect, ref bgScaleCoroutine, Vector3.one, 0.1f);
        StartScale(handleRect, ref handleScaleCoroutine, Vector3.one, 0.1f);
    }

    private void StartScale(RectTransform rect, ref Coroutine coroutine, Vector3 targetScale, float time)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(ScaleRoutine(rect, targetScale, time));
    }

    private System.Collections.IEnumerator ScaleRoutine(RectTransform rect, Vector3 targetScale, float time)
    {
        Vector3 initialScale = rect.localScale;
        float elapsed = 0f;
        while (elapsed < time)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            rect.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / time);
            yield return null;
        }
        if (rect != null) rect.localScale = targetScale;
    }
}

public class PremiumButton : OnScreenControl, IPointerDownHandler, IPointerUpHandler
{
    [InputControl(layout = "Button")]
    [SerializeField]
    private string m_ControlPath;

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    public Graphic buttonImage;
    public Graphic fillImage;
    public Color normalColor;
    public Color pressedColor;
    public Color themeGlowColor;
    public float scaleAnimationTime = 0.1f;

    public System.Action OnPressed;

    private RectTransform rectTransform;
    private Coroutine scaleCoroutine;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(controlPath))
        {
            SendValueToControl(1f);
        }
        if (OnPressed != null) OnPressed();
        
        if (buttonImage != null) buttonImage.color = pressedColor;
        
        // Activate inner glow fill
        if (fillImage != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(fillImage, 0.25f, 0.1f));
        }
        
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine(Vector3.one * 0.85f, scaleAnimationTime));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(controlPath))
        {
            SendValueToControl(0f);
        }
        
        if (buttonImage != null) buttonImage.color = normalColor;
        
        // Deactivate inner glow fill
        if (fillImage != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(fillImage, 0f, 0.1f));
        }
        
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine(Vector3.one, scaleAnimationTime));
    }

    private System.Collections.IEnumerator ScaleRoutine(Vector3 targetScale, float time)
    {
        Vector3 initialScale = rectTransform.localScale;
        float elapsed = 0f;
        while (elapsed < time)
        {
            if (rectTransform == null) yield break;
            elapsed += Time.deltaTime;
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / time);
            yield return null;
        }
        if (rectTransform != null) rectTransform.localScale = targetScale;
    }

    private System.Collections.IEnumerator FadeRoutine(Graphic img, float targetAlpha, float time)
    {
        Color col = img.color;
        float startAlpha = col.a;
        float elapsed = 0f;
        while (elapsed < time)
        {
            if (img == null) yield break;
            elapsed += Time.deltaTime;
            col.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / time);
            img.color = col;
            yield return null;
        }
        col.a = targetAlpha;
        if (img != null) img.color = col;
    }
}

public class UICircle : MaskableGraphic
{
    [Range(0, 100)]
    public float thickness = 0f; // 0 for solid circle
    [Range(3, 100)]
    public int segments = 40;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        float outerRadius = Mathf.Min(width, height) * 0.5f;
        float innerRadius = thickness > 0f ? outerRadius - thickness : 0f;

        Vector2 center = rectTransform.rect.center;
        
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        float angleStep = 360f / segments;

        if (thickness <= 0f)
        {
            // Solid Circle: fan configuration
            // Center vertex
            vertex.position = center;
            vh.AddVert(vertex);

            for (int i = 0; i <= segments; i++)
            {
                float rad = Mathf.Deg2Rad * (i * angleStep);
                vertex.position = new Vector3(
                    center.x + Mathf.Cos(rad) * outerRadius,
                    center.y + Mathf.Sin(rad) * outerRadius,
                    0f
                );
                vh.AddVert(vertex);

                if (i > 0)
                {
                    vh.AddTriangle(0, i, i + 1);
                }
            }
        }
        else
        {
            // Ring: quad bridge configuration
            for (int i = 0; i <= segments; i++)
            {
                float rad = Mathf.Deg2Rad * (i * angleStep);
                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);

                // Outer vertex
                vertex.position = new Vector3(center.x + cos * outerRadius, center.y + sin * outerRadius, 0f);
                vh.AddVert(vertex);

                // Inner vertex
                vertex.position = new Vector3(center.x + cos * innerRadius, center.y + sin * innerRadius, 0f);
                vh.AddVert(vertex);

                if (i > 0)
                {
                    int currOuter = i * 2;
                    int currInner = i * 2 + 1;
                    int prevOuter = (i - 1) * 2;
                    int prevInner = (i - 1) * 2 + 1;

                    vh.AddTriangle(prevOuter, currOuter, currInner);
                    vh.AddTriangle(prevOuter, currInner, prevInner);
                }
            }
        }
    }
}

public class TouchCameraZone : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [InputControl(layout = "Vector2")]
    [SerializeField]
    private string m_ControlPath;

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    [Header("Sensitivity")]
    public float sensitivityX = 0.15f;
    public float sensitivityY = 0.15f;

    private int pointerId = -1;
    private Vector2 lastPointerPosition;
    private Vector2 currentDelta;
    private bool isDragging = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pointerId != -1) return;
        pointerId = eventData.pointerId;
        lastPointerPosition = eventData.position;
        currentDelta = Vector2.zero;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId) return;

        Vector2 currentPointerPosition = eventData.position;
        Vector2 delta = currentPointerPosition - lastPointerPosition;
        lastPointerPosition = currentPointerPosition;

        currentDelta.x += delta.x * sensitivityX;
        currentDelta.y += delta.y * sensitivityY;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId) return;

        pointerId = -1;
        currentDelta = Vector2.zero;
        SendValueToControl(Vector2.zero);
        isDragging = false;
    }

    private void Update()
    {
        if (isDragging)
        {
            Vector2 clampedDelta = new Vector2(
                Mathf.Clamp(currentDelta.x, -1f, 1f),
                Mathf.Clamp(currentDelta.y, -1f, 1f)
            );
            SendValueToControl(clampedDelta);
            currentDelta = Vector2.zero;
        }
    }
}
