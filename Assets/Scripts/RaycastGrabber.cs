using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastGrabber : MonoBehaviour
{
    [SerializeField]
    private GameObject playerOrigin;
    [SerializeField]
    private LayerMask teleportMask;
    [SerializeField]
    private InputActionReference teleportButtonPress;
    [SerializeField]   
    private Transform rightHandTransform;
    [SerializeField]   
    private Transform leftHandTransform;

    //////////////////////////////////////////////////
    // weapon prefabs

    [SerializeField]
    private GameObject prefabAKM;
    [SerializeField]
    private GameObject prefabThrowable;
    [SerializeField]
    private GameObject prefabJavelin;
    [SerializeField]
    private GameObject prefabBow;


    //////////////////////////////////////////////////

    private bool holdingWeapon = false;
    private GameObject currentRightHandWeapon = null;
    private GameObject currentLeftHandWeapon = null;

    void Start()
    {
        teleportButtonPress.action.performed += DoRayCast;
        
    }

    // Update is called once per frame
    void DoRayCast(InputAction.CallbackContext __) {
        //Debug.Log("processing");

        // The Unity raycast hit object, which will store the output of our raycast
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        // Parameters: position to start the ray, direction to project the ray, output for raycast, limit of ray length, and layer mask
        //Debug.Log(Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, teleportMask));
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, teleportMask)) {

            // The object we hit will be in the collider property of our hit object.
            // We can get the name of that object by accessing it's Game Object then the name property
            
            // Don't forget to attach the player origin in the editor!
            //playerOrigin.transform.position = hit.collider.gameObject.transform.position;

            ////Debug.Log("bool check: " + holdingWeapon);
            // if currently holding weapon, destroy the weapon being held, and replace it with the new weapon
            if (holdingWeapon){
                DestroyCurrentWeapon();
            }

            ///if its a throwing knife , then instantiate it in both hands
            if (hit.collider.gameObject.name == "throwable") {

                ///instantiate the weapon in both hands and set the rotation
                currentRightHandWeapon = Instantiate(prefabThrowable, rightHandTransform.position, rightHandTransform.rotation, rightHandTransform);
                currentLeftHandWeapon = Instantiate(prefabThrowable, leftHandTransform.position, leftHandTransform.rotation, leftHandTransform);

                currentRightHandWeapon.transform.localRotation = Quaternion.Euler(0, 0, 0);
                currentLeftHandWeapon.transform.localRotation = Quaternion.Euler(0, 0, 0);
                
                    //Debug.Log("Throwable object added to both hands: " + currentRightHandWeapon.name);
            }
            else if (hit.collider.gameObject.name == "akm") {
                // otherwise, instantiate the weapon in the right hand with the correct rotation
                currentRightHandWeapon = Instantiate(prefabAKM, rightHandTransform.position, rightHandTransform.rotation, rightHandTransform);
                currentRightHandWeapon.transform.localRotation = Quaternion.Euler(0, -90, 0);
                
                    //Debug.Log("Object added to right hand: " + currentRightHandWeapon.name);
            } else if (hit.collider.gameObject.name == "Bow") {     
                
                currentRightHandWeapon = Instantiate(prefabBow, leftHandTransform.position, leftHandTransform.rotation, leftHandTransform);

            } else {
                //this would be the javelin
                currentRightHandWeapon = Instantiate(prefabJavelin, rightHandTransform.position, rightHandTransform.rotation, rightHandTransform);
                currentRightHandWeapon.transform.localRotation = Quaternion.Euler(45, 0, 0);
            }

            holdingWeapon = true;
        }
    }


    /// need to destroy the current weapon before instantiating a new one
    void DestroyCurrentWeapon() {

        if (currentRightHandWeapon != null) 
        {
            Destroy(currentRightHandWeapon);
            currentRightHandWeapon = null;
        }

        if (currentLeftHandWeapon != null) 
        {
            Destroy(currentLeftHandWeapon);
            currentLeftHandWeapon = null;
        }

    }
    
}



