using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Each entity that is affected when the fireButton is pressed will check if fireButton is true each update.
 * However, each enemy does not check this individually. Instead, the EnemyManager checks this for all of them.
 * The only other entities that check this are the PlayerController and the GroundController.
 * Anyone who checks the fireButton will also have a reference to the PlayerController,
 * because the PlayerController has all the information needed to process an attack.
 * This information includes the movement direction, and other constants that affect the strength of the attack.
 */
public class InputController : MonoBehaviour {
    public static Vector3 leftStick;
    public static bool fireButton;

    private static List<OnFireEventHandler> listeners;

    // Use this for initialization
    void Start() {
        leftStick = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update() {
        // leftStick handling
        leftStick.Set(Input.GetAxis("Horizontal Stick"), 0, Input.GetAxis("Vertical Stick"));
        if (leftStick.magnitude < 0.17f) {
            leftStick.Set(0, 0, 0);
        }
        if (leftStick.magnitude > 1) {
            leftStick.Normalize();
        }
        if (Input.GetAxis("Horizontal Key") != 0 || Input.GetAxis("Vertical Key") != 0) {
            leftStick.Set(Input.GetAxis("Horizontal Key"), 0, Input.GetAxis("Vertical Key"));
            leftStick.Normalize();    
        }
        
        // fireButton handling
        if (Input.GetButton("Fire")) {
            if (!fireButton) {
                fireButton = true;
                if (!object.ReferenceEquals(listeners, null)) {    
                    foreach (OnFireEventHandler listener in listeners) {
                        listener.OnFireEvent();
                    }
                }
            }
        } else {
            fireButton = false;
        }
    }

    public static void AddOnFireEventHandler(OnFireEventHandler listener) {
        if (object.ReferenceEquals(listeners, null)) {
            listeners = new List<OnFireEventHandler>();
        }

        listeners.Add(listener);
    }
}

public interface OnFireEventHandler {
    void OnFireEvent();
}
