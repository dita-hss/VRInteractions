using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;



public class PlayerShoot : MonoBehaviour
{
    public GameObject BulletTemplate;
    public InputActionReference trigger;

    public InputActionReference reloadButton;
    private int currentAmmo;
    public int maxAmmo = 20;
    public float fireRate = 0.1f;
    private Coroutine shootingCoroutine;



    private XRBaseController rightHandController;
    public float hapticIntensity = 0.5f; // 0.0 to 1.0 only
    public float hapticDuration = 0.1f;
    
    void Start(){
        //need this for the haptic feedback to work - idk how to do it without casting --- Problem: cant find the correct game object
        rightHandController = GameObject.Find("RightHand Controller").GetComponent<ActionBasedController>();

        currentAmmo = maxAmmo;
        // this line adds the Shoot method to the trigger action
        trigger.action.started += StartShooting;
        trigger.action.canceled += StopShooting;
        reloadButton.action.performed += Reload;
    }

    // Shoots a bullet
    void Shoot(){
        // this line creates a new bullet object and adds it into the world
        //spawn the bullet a bit in front of the player

        if (currentAmmo > 0) {

            Quaternion rotationOffset = Quaternion.Euler(0, 90, 0);
            Quaternion newRotation = transform.rotation * rotationOffset;

            GameObject bullet = Instantiate(BulletTemplate, transform.position, newRotation);
            //bullet.transform.rotation = Quaternion.LookRotation(transform.forward);


            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            var vc = rb.velocity;
            vc.z = 0;
            vc.y = 0;
            rb.velocity = bullet.transform.forward * 40;
            Destroy(bullet, 0.9f);

            currentAmmo--;
            //////Debug.Log("Current Ammo: " + currentAmmo);


            ////sometimes i get errors when trying to access the rightHandController idk why
            if (rightHandController != null){
                rightHandController.SendHapticImpulse(hapticIntensity, hapticDuration);
            }

            
        }
    }

    void Reload(InputAction.CallbackContext __){
        currentAmmo = maxAmmo;
        //Debug.Log("reloading: " + currentAmmo);

        ////sometimes i get errors when trying to access the rightHandController idk why
        if (rightHandController != null){
            rightHandController.SendHapticImpulse(1, 0.2f);
        }
    }

    void StartShooting(InputAction.CallbackContext __){
        /// here we start the shooting coroutine
        if (shootingCoroutine == null) {
            shootingCoroutine = StartCoroutine(ShootContinuously());
            //OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.RTouch);

        }
    }

    void StopShooting(InputAction.CallbackContext __){
        /// here we stop the shooting coroutine
        if (shootingCoroutine != null){
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
            //OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);

        }
    }


    /// https://docs.unity3d.com/ScriptReference/MonoBehaviour.StartCoroutine.html
    /// here we create a coroutine in order to be able to shoot continuously
    IEnumerator ShootContinuously(){
        while (currentAmmo > 0){
            Shoot();
            yield return new WaitForSeconds(fireRate);
        }
    }

    private void OnDestroy(){
        ///when the object is destroyed we need to unsubscribe from the input actions to prevent errors when pressing the triggers when the object is destroyed
        if (shootingCoroutine != null) {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }

        if (trigger != null){
            trigger.action.started -= StartShooting;
        }
        if (trigger != null){
            trigger.action.canceled -= StopShooting;
        }
        if (reloadButton != null){ 
            reloadButton.action.performed -= Reload;
        }
    }

}