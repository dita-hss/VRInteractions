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
    private Transform parentTransform;

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
        Debug.Log(Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, teleportMask));
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, teleportMask)) {

            // The object we hit will be in the collider property of our hit object.
            // We can get the name of that object by accessing it's Game Object then the name property
            Debug.Log("here" + hit.collider.gameObject.name);
            
            // Don't forget to attach the player origin in the editor!
            //playerOrigin.transform.position = hit.collider.gameObject.transform.position;
            GameObject newObject = Instantiate(hit.collider.gameObject, hit.point, Quaternion.identity, parentTransform);
            Debug.Log("object " + newObject + " " + newObject.name);
            //newObject.transform.SetParent(parentTransform, false);

        }

    }
}

