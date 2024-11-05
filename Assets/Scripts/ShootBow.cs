using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;


//inspiration: https://www.youtube.com/watch?v=tD4tR7zO8y0
// did not use their code but took the general idea of how it could be implemented

public class ShootBow : MonoBehaviour
{
    public InputActionReference triggerRight;
    public GameObject arrowPrefab;

    public float maxPullDistance = 0.5f;
    public float maxArrowSpeed = 50.0f;
    public float hapticIntensity = 0.5f;
    public float hapticDuration = 0.1f;

    private XRBaseController rightHandController;
    private XRBaseController leftHandController;

    private Transform arrowSpawnPoint;
    private Transform stringAttachPoint;
    private Transform stringPullPoint;

    private GameObject currentArrow;
    private bool isArrowLoaded = false;
    private bool isStringPulled = false;

    private Vector3 initialStringPosition;

    void Start(){
        //use both hands like the knives - - to shoot the bow
        rightHandController = GameObject.Find("RightHand Controller").GetComponent<ActionBasedController>();
        leftHandController = GameObject.Find("LeftHand Controller").GetComponent<ActionBasedController>();
        
        //created new componenets as shown in a video tutorial
        //spawn point for the arrow
        arrowSpawnPoint = transform.Find("SpawnPoint");
        // the point where the string is attached to the bow -- not fullly implemented yet TODO: implement
        stringAttachPoint = transform.Find("AttachPoint");
        // the point where the string is pulled-- used to calculate the pull distance
        stringPullPoint = transform.Find("PullPoint");
        initialStringPosition = stringPullPoint.localPosition;

        //triggers to shoot the bow
        triggerRight.action.started += StartPullingString;
        triggerRight.action.canceled += ReleaseString;
        LoadArrow();
    }

    void Update(){
        //update the string and arrow position when the string is pulled and the arrow is loaded, 
        ///if not then what happens is the arrow will be loaded but the string will not be pulled
        if (isArrowLoaded && isStringPulled){
            UpdateBowString();
        }
    }

    private void StartPullingString(InputAction.CallbackContext context) {
        //sanity check
        if (!isArrowLoaded){ 
            return;}

        isStringPulled = true;
    }


    //TODO: how can i combine the two methods into one method? 

    //here the bows string and arrow is updated based on the distance between the left and right hand controllers
    private void UpdateBowString(){
        //distance between the left and right hand controllers
        float pullDistance = Vector3.Distance(rightHandController.transform.position, leftHandController.transform.position);
        //maximum pull distance
        pullDistance = Mathf.Clamp(pullDistance, 0, maxPullDistance);

        // here i had to constrain the pull direction to only the Z-axis of the bow otherwise it was shooting up and down and everywhere that didnt make sense
        ///inverse transform point is used to transform the position of the string attach point to the local space of the bow
        /////this is necessary because the bow is rotated and the string attach point is not at the origin of the bow
        Vector3 localPullDirection = transform.InverseTransformPoint(rightHandController.transform.position) - transform.InverseTransformPoint(stringAttachPoint.position);
        localPullDirection.x = 0;
        localPullDirection.y = 0;
        localPullDirection = localPullDirection.normalized;

        // Set the position of the stringPullPoint based on the pull distance
        stringPullPoint.localPosition = initialStringPosition + localPullDirection * pullDistance;

        //sanity check
        if (currentArrow != null){
            currentArrow.transform.position = stringPullPoint.position;
        }

        //added a pull percentage to the haptic impulse to make it more realistic ;)
        float pullPercentage = pullDistance / maxPullDistance;
        rightHandController.SendHapticImpulse(pullPercentage * hapticIntensity, hapticDuration);
        
    }

    // idea here is to shoot the arrow based on the pull distance between the left and right hand controllers
    private void ReleaseString(InputAction.CallbackContext context){
        //sanity check
        if (!isStringPulled || !isArrowLoaded) {
            return;
        }

        // again calculate the pull distance between the left and right controllers
        float pullDistance = Vector3.Distance(rightHandController.transform.position, leftHandController.transform.position);
        float pullPercentage = Mathf.Clamp01(pullDistance / maxPullDistance);
        float arrowSpeed = pullPercentage * maxArrowSpeed;

        if (currentArrow != null){
            // set her free
            currentArrow.transform.SetParent(null);
            Rigidbody arrowRb = currentArrow.GetComponent<Rigidbody>();
            arrowRb.isKinematic = false;

            // shoot forward
            arrowRb.velocity = transform.forward * arrowSpeed;
            currentArrow = null;
        }

        //reset bowstring for new arrow
        stringPullPoint.localPosition = initialStringPosition;
        isStringPulled = false;
        isArrowLoaded = false;
        LoadArrow();
    }

    private void LoadArrow(){
        // load the arrow
        if (!isArrowLoaded){
            currentArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
            currentArrow.transform.SetParent(transform); 
            currentArrow.GetComponent<Rigidbody>().isKinematic = true; 
            isArrowLoaded = true;
        }
    }

    private void OnDestroy(){
        //remove the triggers
        if (triggerRight != null) {
            triggerRight.action.started -= StartPullingString;
            triggerRight.action.canceled -= ReleaseString;
        }
    }
}
