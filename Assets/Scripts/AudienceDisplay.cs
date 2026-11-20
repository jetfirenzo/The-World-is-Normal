using UnityEngine;

/// <summary>
/// Zet deze camera op het tweede scherm (beamer) voor het publiek.
/// Display 2 activeren werkt alleen in een build, niet in de Editor
/// (in de Editor kun je in de Game view "Display 2" kiezen om te kijken).
/// </summary>
[RequireComponent(typeof(Camera))]
public class AudienceDisplay : MonoBehaviour
{
    [Header("Langzaam rondcirkelen (optioneel)")]
    public bool orbit = true;
    public Vector3 orbitCenter = Vector3.zero;
    [Tooltip("Graden per seconde")]
    public float orbitSpeed = 4f;

    Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();

        // Het publiek hoort niets via deze camera: er mag maar één AudioListener zijn (die van de speler).
        var listener = GetComponent<AudioListener>();
        if (listener != null) Destroy(listener);
    }

    void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            _cam.targetDisplay = 1;
            Debug.Log($"[AudienceDisplay] Publiekscamera op scherm 2 ({Display.displays[1].systemWidth}x{Display.displays[1].systemHeight}).");
        }
        else
        {
            Debug.LogWarning("[AudienceDisplay] Maar één scherm gevonden (of je zit in de Editor). Sluit een tweede scherm/beamer aan en start de build.");
        }
    }

    void LateUpdate()
    {
        if (!orbit || orbitSpeed == 0f) return;
        transform.RotateAround(orbitCenter, Vector3.up, orbitSpeed * Time.deltaTime);
        transform.LookAt(orbitCenter);
    }
}
