using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerThrowKnives : MonoBehaviour
{
    public InputActionReference triggerLeft;
    public InputActionReference triggerRight;
    private XRBaseController rightHandController;
    private XRBaseController leftHandController;

    public float hapticIntensity = 0.5f;
    public float hapticDuration = 0.1f;
    public float throwForceMultiplier = 1.5f;
    public float timeToDestroy = 3.0f;
    public float spinSpeed = 10.0f;

    private Rigidbody rb;

    private bool isRightHandKnife = false; 
    private bool hasBeenThrown = false;

    private Vector3 previousHandPosition;
    private Vector3 currentHandVelocity;

    void Start(){
        //using both hands in throwing knives
        rightHandController = GameObject.Find("RightHand Controller").GetComponent<ActionBasedController>();
        leftHandController = GameObject.Find("LeftHand Controller").GetComponent<ActionBasedController>();

        //have to check which hand the knife belongs to
        if (transform.parent.name == "RightHand Controller"){
            isRightHandKnife = true;
            ////Debug.Log("Right hand knife: " + gameObject.name);
        } else {
            isRightHandKnife = false;
            //////Debug.Log("Left hand knife: " + gameObject.name);
        }

        if (isRightHandKnife) {
            triggerRight.action.performed += ThrowKnife;
        } else {
            triggerLeft.action.performed += ThrowKnife;
        }

        rb = GetComponent<Rigidbody>();
        //not sure why this fixes the bug where the knife is moving around the controller - slowly drifting away
        // its quite literally the opposite on the javelin n not sure why
        rb.isKinematic = true;
    }

    void Update() {
        ///determine which controller is holding the knife
        XRBaseController controller = isRightHandKnife ? rightHandController : leftHandController;

        if (controller != null){
            currentHandVelocity = (controller.transform.position - previousHandPosition) / Time.deltaTime;
            previousHandPosition = controller.transform.position;
        }
    }

    void ThrowKnife(InputAction.CallbackContext __)
    {
        if (hasBeenThrown){
            return; 
        }
        hasBeenThrown = true;
        transform.parent = null;
        rb.isKinematic = false;
        rb.velocity = currentHandVelocity * throwForceMultiplier;

        //different from javelin, this is the spin of the knife , at least i envision it that way
        rb.angularVelocity = transform.right * spinSpeed;
        rb.useGravity = true;

        XRBaseController controller = isRightHandKnife ? rightHandController : leftHandController;
        controller.SendHapticImpulse(hapticIntensity, hapticDuration);
        Destroy(gameObject, timeToDestroy);
    }

    private void OnDestroy()
    {
        if (triggerRight != null && isRightHandKnife){
            triggerRight.action.performed -= ThrowKnife;
        }

        if (triggerLeft != null && !isRightHandKnife){
            triggerLeft.action.performed -= ThrowKnife;
        }
    }
}
