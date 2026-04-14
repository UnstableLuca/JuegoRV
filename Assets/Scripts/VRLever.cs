using UnityEngine;

public class VRLever : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    HingeJoint hinge;
    public float leverOutput;
    public float minValue, maxValue;

    public float startingValue;
    
    void Start()
    {
        hinge = GetComponent<HingeJoint>();

        if(startingValue >= minValue && startingValue <= maxValue)
        {
            float rangeFraction = (startingValue - minValue) / (maxValue - minValue);
            float angle = hinge.limits.min + rangeFraction * (hinge.limits.max - hinge.limits.min);
            
            Vector3 worldSpaceHingeAxis = transform.TransformDirection(hinge.axis);
            transform.rotation = Quaternion.AngleAxis(angle, worldSpaceHingeAxis) * transform.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float betweenZeroAndOne = 
            (hinge.angle - hinge.limits.min) / (hinge.limits.max - hinge.limits.min);

        leverOutput = minValue + (maxValue - minValue) * betweenZeroAndOne;
    }
}
