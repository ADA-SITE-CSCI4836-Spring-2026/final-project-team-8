using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public enum ButtonVariant { Primary, Secondary, Icon, Danger }

[RequireComponent(typeof(Button), typeof(CanvasGroup))]
public class UIButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Variant")]
    public ButtonVariant variant = ButtonVariant.Primary;

    [Header("References")]
    public Image background;
    public Image border;
    public TextMeshProUGUI label;
    public Image icon;
    public Image fxLayer;

    [Header("Theme Colors")]
    public Color primaryColor = new Color(0.18f, 0.78f, 0.56f);
    public Color secondaryColor = new Color(0.12f, 0.12f, 0.18f);
    public Color dangerColor = new Color(0.85f, 0.22f, 0.22f);
    public Color textColor = Color.white;

    [Header("Audio")]
    public AudioClip hoverSFX;
    public AudioClip clickSFX;
    private AudioSource _audio;

    [Header("Glow")]
    public Image[] glowLayers;
    public Color glowColor = new Color(0.11f, 0.79f, 0.54f, 1f);
    public bool pulseOnStart = false;

    private float[] _glowBaseAlphas = { 0.35f, 0.15f, 0.06f };
    private Coroutine _glowAnim;


    private Button _button;
    private CanvasGroup _group;
    private Vector3 _baseScale;
    private Coroutine _anim;

    void Awake()
    {
        _button = GetComponent<Button>();
        _group = GetComponent<CanvasGroup>();
        _audio = GetComponent<AudioSource>();
        _baseScale = transform.localScale;
        ApplyTheme();

        SetGlowActive(false);
    }

    void Start()
    {
        if (pulseOnStart) StartGlow();
    }

    void ApplyTheme()
    {
        Color fill;
        switch (variant)
        {
            case ButtonVariant.Primary:
                fill = primaryColor;
                break;
            case ButtonVariant.Danger:
                fill = dangerColor;
                break;
            case ButtonVariant.Secondary:
                fill = secondaryColor;
                textColor = new Color(0f, 0f, 0f, 1f);
                break;
            case ButtonVariant.Icon:
                fill = Color.clear;
                break;
            default:
                fill = primaryColor;
                break;
        }

        // Standard boolean logic
        bool showBorder = /* (variant == ButtonVariant.Secondary || variant == ButtonVariant.Icon) */ false;

        if (background) background.color = fill;
        if (border) { border.enabled = showBorder; border.color = primaryColor; }
        if (label) label.color = textColor;
        if (icon) icon.color = textColor;
        if (fxLayer) fxLayer.color = new Color(1, 1, 1, 0);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        PlaySFX(hoverSFX);
        Animate(1.06f, 0.12f);
        if (fxLayer) fxLayer.color = new Color(1, 1, 1, 0.08f);
    }

    public void OnPointerExit(PointerEventData e)
    {
        Animate(1f, 0.12f);
        if (fxLayer) fxLayer.color = new Color(1, 1, 1, 0);
    }

    public void OnPointerDown(PointerEventData e)
    {
        PlaySFX(clickSFX);
        Animate(0.95f, 0.07f);
    }

    public void OnPointerUp(PointerEventData e) => Animate(1f, 0.1f);

    void Animate(float targetScale, float duration)
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(ScaleTo(_baseScale * targetScale, duration));
    }

    IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float t = 0;
        while (t < 1)
        {
            t += Time.unscaledDeltaTime / duration;
            transform.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        transform.localScale = target;
    }

    void PlaySFX(AudioClip clip)
    {
        if (_audio && clip) _audio.PlayOneShot(clip);
    }

    public void StartGlow()
    {
        SetGlowActive(true);
        if (_glowAnim != null) StopCoroutine(_glowAnim);
        _glowAnim = StartCoroutine(PulseGlow());
    }

    public void StopGlow()
    {
        if (_glowAnim != null) StopCoroutine(_glowAnim);
        _glowAnim = null;
        SetGlowActive(false);
    }

    IEnumerator PulseGlow()
    {
        float t = 0f;
        while (true)
        {
            t += Time.unscaledDeltaTime;
            float multiplier = 0.6f + 0.4f * Mathf.Sin(t * 2.5f);
            SetGlowAlphas(multiplier);
            yield return null;
        }
    }

    void SetGlowAlphas(float multiplier)
    {
        for (int i = 0; i < glowLayers.Length; i++)
        {
            if (glowLayers[i] == null) continue;
            Color c = glowColor;
            c.a = _glowBaseAlphas[i] * multiplier;
            glowLayers[i].color = c;
        }
    }

    void SetGlowActive(bool active)
    {
        foreach (var layer in glowLayers)
            if (layer != null) layer.gameObject.SetActive(active);
    }
}
