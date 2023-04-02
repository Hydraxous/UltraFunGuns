using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace UltraFunGuns;

[WeaponAbility("Physic Manipulate", "Manipulate a physics object", 0, RichTextColors.aqua)]
[WeaponAbility("Physic Lock", "Lock a physics object in place", 1, RichTextColors.aqua)]
[UFGWeapon("FizzyGun", "Fizix Gun", 3, true, WeaponIconColor.Blue)]
public class FizzyGun : UltraFunGunBase
{
    public float forceOnRb = 150.0f;
    public float forwardBias = 5.0f;
    public float groupForce = 2.0f;
    public float groupMaxRange = 10.0f;

    public float stopRange = 0.2f;
    
    public bool setVelo = false;

    public float maxRange = 10.0f;
    public float minAngle = 0.60f; //45o angle
    public bool IsHoldingObject => (heldObjectRigidbody != null);

    private Transform objectHolder;

    private Rigidbody heldObjectRigidbody;
    private Transform heldObjectParent;
    private bool waitForPrimaryFireReleased;

    private Dictionary<Rigidbody, bool> oldKinematicStates = new Dictionary<Rigidbody, bool>();

    private bool checking, hittingSomething;

    private Transform ObjectHolder
    {
        get
        {
            if(objectHolder == null)
            {
                objectHolder = new GameObject("PhysicsObjectHolder").transform;
            }
            return objectHolder;
        }
    }

    public override void GetInput()
    {
        if (IsHoldingObject)
        {
            UpdateHolderPosition();
        }

        if (InputManager.Instance.InputSource.Fire1.IsPressed)
        {
            FireBeam();
        }else
        {
            waitForPrimaryFireReleased = false;

            if(IsHoldingObject)
            {
                EndManipulation();
            }
        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && IsHoldingObject)
        {
            PhysicsLockObject();
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            if (UltraFunGuns.DebugMode)
            {
                setVelo = !setVelo;
            }
        }
    }


    void FixedUpdate()
    {
        if (!IsHoldingObject)
            return;

        float dist = Vector3.Distance(heldObjectRigidbody.transform.position+heldObjectRigidbody.centerOfMass,ObjectHolder.position);
        heldObjectRigidbody.velocity = (dist < stopRange) ? Vector3.zero : (ObjectHolder.position - heldObjectRigidbody.position)*3.0f;
    }

    private void FireBeam()
    {
        if(IsHoldingObject || waitForPrimaryFireReleased)
        {
            return;
        }

        if(CheckForCollider(out Collider col))
        {
            BeginManipulation(col);
        }
    }

    private bool CheckForCollider(out Collider col)
    {
        col = null;
        hittingSomething = false;

        if (Physics.Raycast(mainCam.position, mainCam.forward, out RaycastHit hit, LayerMask.GetMask("Environment", "Outdoors", "Outdoors Non-solid","Default", "Limb", "Gib", "Armor", "Projectile")))
        {
            hittingSomething = true;
            if(hit.collider != null)
            {
                lastDistFromCamera = hit.distance;
                UpdateHolderPosition();
                BeginManipulation(hit.collider);
                return true;
            }
        }

        return false;
    }

    private float lastDistFromCamera = 0.0f;

    //Updates the position of the object holder relative to the camera
    private void UpdateHolderPosition()
    {

        Vector3 camPos = mainCam.position;
        Vector3 camDirection = mainCam.forward;

        Vector3 newWorldPos = camPos + camDirection * lastDistFromCamera;

        ObjectHolder.position = newWorldPos;

        camPos.y = newWorldPos.y;

        Vector3 directionToCamera = camPos - newWorldPos;
        Quaternion newRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);

        ObjectHolder.rotation = newRotation;
    }

    private void BeginManipulation(Collider col)
    {
        if (col == null)
            return;
        
        //Dont grab objects with no arby
        if (col.attachedRigidbody == null)
            return;

        heldObjectRigidbody = col.attachedRigidbody;

        HydraLogger.Log($"Beginning manipulation on object {heldObjectRigidbody.name}", DebugChannel.Warning);


        if(!oldKinematicStates.ContainsKey(heldObjectRigidbody))
        {
            oldKinematicStates.Add(heldObjectRigidbody, heldObjectRigidbody.isKinematic);
        }else
        {
            heldObjectRigidbody.isKinematic = oldKinematicStates[heldObjectRigidbody];
        }

        if (heldObjectRigidbody.transform.parent != null)
        {
            heldObjectParent = heldObjectRigidbody.transform.parent;
        }
        heldObjectRigidbody.transform.parent = ObjectHolder;
    }

    private void EndManipulation()
    {
        if (!IsHoldingObject)
            return;
     
        HydraLogger.Log($"Ending manipulation on object {heldObjectRigidbody.name}", DebugChannel.Warning);

        heldObjectRigidbody.transform.parent = heldObjectParent;
        heldObjectParent = null;
        heldObjectRigidbody = null;

    }

    private void PhysicsLockObject()
    {
        if (!IsHoldingObject)
            return;

        waitForPrimaryFireReleased = true;
        heldObjectRigidbody.isKinematic = true;
        EndManipulation();
    }

    public void OnDisable()
    {
        EndManipulation();
    }

    public override string GetDebuggingText()
    {
        string debug = base.GetDebuggingText();
        debug += $"CHECKING: {InputManager.Instance.InputSource.Fire1.IsPressed}\n";
        debug += $"ISHOLD: {IsHoldingObject}\n";
        debug += $"HITTING: {hittingSomething}\n";
        if (IsHoldingObject)
        {
            debug += $"HELD: {heldObjectRigidbody.name}\n";
            debug += $"VELO: {heldObjectRigidbody.velocity}\n";
            debug += $"SPEED: {heldObjectRigidbody.velocity.magnitude}\n";
            debug += $"KINEMATIC: {heldObjectRigidbody.isKinematic}\n";
        }
        return debug;
    }
}
