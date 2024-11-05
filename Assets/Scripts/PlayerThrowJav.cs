using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerThrowJav : MonoBehaviour
{
    public InputActionReference trigger;
    private XRBaseController rightHandController;
    public float hapticIntensity = 0.5f; // 0 to 1 only
    public float hapticDuration = 0.1f;
    public float throwForceMultiplier = 1.5f;
    public float timeToDestroy = 3.0f;

    private Vector3 previousHandPosition;
    private Vector3 currentHandVelocity;

    private Rigidbody rb;
    private bool hasBeenThrown = false;

    void Start(){
        //need this for the haptic feedback to work - idk how to do it without casting
        rightHandController = GameObject.Find("RightHand Controller").GetComponent<ActionBasedController>();
        trigger.action.performed += ThrowJavelin;

        rb = GetComponent<Rigidbody>();
        rb.angularDrag = 2.0f; 
    }

    void Update(){
        //findcontroller velocity as the difference in position over time
        currentHandVelocity = (rightHandController.transform.position - previousHandPosition) / Time.deltaTime;
        previousHandPosition = rightHandController.transform.position;
        
    }

    void ThrowJavelin(InputAction.CallbackContext __) {
        //stop the player from throwing multiple javelins bc there is only one javelin lol
        if (hasBeenThrown){
            return;
        }

        hasBeenThrown = true; 
        //remove the parent so the javelin can move with gravity and apply vel before turning it back on
        transform.parent = null;
        rb.useGravity = false;
        rb.velocity = currentHandVelocity * throwForceMultiplier;
        rb.useGravity = true;

        //use a coroutine to align the javelin with its velocity i.e. dip towards the ground if thrown correctly
        StartCoroutine(Dip());
        rightHandController.SendHapticImpulse(hapticIntensity, hapticDuration);
        Destroy(gameObject, timeToDestroy);
    }


    private IEnumerator Dip(){
        //update the orientation to align with the velocity
        while (rb.useGravity){
            //only rotate if the javelin is moving
            if (rb.velocity.magnitude > 0.1f){
                // Quaternion.LookRotation returns a rotation that points the z-axis forward, and the y-axis up
                //so we need to rotate it 90 degrees around the x-axis to make it point the y-axis forward
                Quaternion targetRotation = Quaternion.LookRotation(rb.velocity.normalized);
                
            
                //Lerp - current rotation of the javelin -> rotation we want the javelin to have, to how quickly the interpolation should happen
                // MoveRotation - sets the rotation of the Rigidbody of an object with physics
                rb.MoveRotation(Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10.0f));
            }

            yield return null;
        }
    }

    private void OnDestroy(){
        if (trigger != null){
            trigger.action.performed -= ThrowJavelin;
        }
    }
}


