using UnityEngine;
[RequireComponent(typeof(HingeJoint))]
[RequireComponent(typeof(AudioSource))]
public class girarBoton : MonoBehaviour
{[Header("Referencias del Tocadiscos")]
    [SerializeField] private Animator tocadiscosAnimator; 
    [SerializeField] private string animacionTocadiscos = "girando";

    [Header("Configuración del Dial")]
    [Range(0f, 1f)] 
    [SerializeField] private float porcentajeActivacion = 0.8f; 

    private HingeJoint hinge;
    private AudioSource audioSource;
    private bool turntableIsOn = false;
    private float maxAngle;

    void Start()
    {
        hinge = GetComponent<HingeJoint>();
        audioSource = GetComponent<AudioSource>();

        maxAngle = hinge.limits.max;
    }

    void Update()
    {
        float currentAngle = hinge.angle;
        float currentPercent = Mathf.Clamp01(currentAngle / maxAngle);

        CheckDialState(currentPercent);
    }

    private void CheckDialState(float currentPercent)
    {
        if (currentPercent >= porcentajeActivacion && !turntableIsOn)
        {
            turntableIsOn = true;
            audioSource.Play();
            
            if (tocadiscosAnimator != null) 
                tocadiscosAnimator.SetBool(animacionTocadiscos, true);
        }
        else if (currentPercent < porcentajeActivacion && turntableIsOn)
        {
            turntableIsOn = false;
            audioSource.Stop();
            
            if (tocadiscosAnimator != null) 
                tocadiscosAnimator.SetBool(animacionTocadiscos, false);
        }
    }
}
