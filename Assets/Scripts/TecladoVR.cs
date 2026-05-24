using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;

public class TecladoVR : MonoBehaviour
{
    [Header("Input")]
    public TMP_InputField inputNombre;

    [Header("Configuración")]
    public int maxCaracteres = 12;
    public string nombrePorDefecto = "Dino";
    public string Escenaa = "Dinoo";

    public bool CloseOnInactivity = true;
    public float CloseOnInactivityTime = 15;

    public TMP_InputField InputField = null;
    public event EventHandler OnPlacement = delegate { };
     private float _closingTime;

     public float distancia = 0.5f;
     public float verticalOffset = -0.5f;

     public Transform positionSource;

    private Vector3 m_StartingScale = Vector3.one;

    [SerializeField]
    private float m_MinScale = 1.0f;

    [SerializeField]
    private float m_MaxDistance = 3.5f;

    [SerializeField]
    private float m_MinDistance = 0.25f;
    [SerializeField]
    private float m_MaxScale = 1.0f;

    private Vector3 m_ObjectBounds;

    public static TecladoVR Instance { get; private set; }

    private void Start()
    {
        inputNombre.readOnly = true;
        inputNombre.text = "";
    }
    void Awake()
    {
        Instance = this;

        m_StartingScale = transform.localScale;
        Bounds canvasBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(transform);

        RectTransform rect = GetComponent<RectTransform>();
        m_ObjectBounds = new Vector3(canvasBounds.size.x * rect.localScale.x, canvasBounds.size.y * rect.localScale.y, canvasBounds.size.z * rect.localScale.z);

        InputField.keyboardType = (TouchScreenKeyboardType)(int.MaxValue);

        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        Instance = null;
    }
    public void EscribirLetra(string letra)
    {
        if (inputNombre.text.Length >= maxCaracteres)
            return;

        inputNombre.text += letra;
    }

    public void Borrar()
    {
        if (inputNombre.text.Length <= 0)
            return;

        inputNombre.text = inputNombre.text.Substring(0, inputNombre.text.Length - 1);
    }

    public void Limpiar()
    {
        inputNombre.text = "";
    }

    public void ConfirmarNombre()
    {
        string nombreFinal = inputNombre.text.Trim();

        if (string.IsNullOrWhiteSpace(nombreFinal))
        {
            nombreFinal = nombrePorDefecto;
        }

        GameManager.Instance.nombreDinosaurio = nombreFinal;

        SceneManager.LoadScene(Escenaa);
    }
    public void PresentKeyboard()
    {
        ResetClosingTime();
        gameObject.SetActive(true);

        OnPlacement(this, EventArgs.Empty);
        InputField.ActivateInputField();

        Vector3 direction = positionSource.forward;
        direction.y = 0;
        direction.Normalize();
        Vector3 targetPosition = positionSource.position + direction * distancia + Vector3.up * verticalOffset;
        RepositionKeyboard(targetPosition);
    }
    private void ResetClosingTime()
    {
        if (CloseOnInactivity)
        {
                _closingTime = Time.time + CloseOnInactivityTime;
        }
    }
    public void RepositionKeyboard(Vector3 kbPos, float verticalOffset = 0.0f)
    {
            transform.position = kbPos;
            ScaleToSize();
            LookAtTargetOrigin();
    }
    public void RepositionKeyboard(Transform objectTransform, BoxCollider aCollider = null, float verticalOffset = 0.0f)
    {
        transform.position = objectTransform.position;

        if (aCollider != null)
        {
            float yTranslation = -((aCollider.bounds.size.y * 0.5f) + verticalOffset);
            transform.Translate(0.0f, yTranslation, -0.6f, objectTransform);
        }
        else
        {
            float yTranslation = -((m_ObjectBounds.y * 0.5f) + verticalOffset);
            transform.Translate(0.0f, yTranslation, -0.6f, objectTransform);
        }

        ScaleToSize();
        LookAtTargetOrigin();

    }
    private void ScaleToSize()
    {
        float distance = (transform.position - Camera.main.transform.position).magnitude;
        float distancePercent = (distance - m_MinDistance) / (m_MaxDistance - m_MinDistance);
        float scale = m_MinScale + (m_MaxScale - m_MinScale) * distancePercent;

        scale = Mathf.Clamp(scale, m_MinScale, m_MaxScale);
        transform.localScale = m_StartingScale * scale;

    }

    private void LookAtTargetOrigin()
    {
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(Vector3.up, 180.0f);
    }
}