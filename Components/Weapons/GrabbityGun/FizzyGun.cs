using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace UltraFunGuns;

[WeaponAbility("Physic Manipulate", "Manipulate a physics object", 0, RichTextColors.aqua)]
[WeaponAbility("Physic Lock", "Lock a physics object in place", 1, RichTextColors.aqua)]
[FunGun("FizzyGun", "PHYSICS GUN", 3, true, WeaponIconColor.Blue)]
public class FizzyGun : UltraFunGunBase
{
    public float forceOnRb = 150.0f;
    public float forwardBias = 5.0f;
    public float groupForce = 2.0f;
    public float groupMaxRange = 10.0f;

    
    public bool setVelo = false;

    public float maxRange = 10.0f;
    public float minAngle = 0.60f; //45o angle
    public bool IsHoldingObject => (heldObject != null);

    private Transform objectHolder;

    private Collider heldObject;
    private Transform heldObjectParent;
    private bool objectWasKinematic;
    private bool waitForPrimaryFireReleased;

    public override void OnAwakeFinished()
    {
        objectHolder = new GameObject("PhysicsObjectHolder").transform;
        objectHolder.parent = transform;
    }

    public override void GetInput()
    {

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
        if (IsHoldingObject)
        {
            UpdateHolderPosition();
        }
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

        if(Physics.Raycast(mainCam.position, mainCam.forward, out RaycastHit hit))
        {
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
        if(objectHolder != null)
        {
            return;
        }

        Vector3 camPos = mainCam.position;
        Vector3 camDirection = mainCam.forward;

        Vector3 newWorldPos = camPos + camDirection * lastDistFromCamera;

        objectHolder.transform.position = newWorldPos;

        camPos.y = newWorldPos.y;

        Vector3 directionToCamera = camPos - newWorldPos;
        Quaternion newRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);

        objectHolder.transform.rotation = newRotation;
    }

    private void BeginManipulation(Collider col)
    {
        if (col == null)
        {
            return;
        }

        HydraLogger.Log($"Beginning manipulation on object {col.name}", DebugChannel.Warning);

        if (col.attachedRigidbody != null)
        {
            objectWasKinematic = col.attachedRigidbody.isKinematic;
            col.attachedRigidbody.isKinematic = true;
        }
        else
        {
            objectWasKinematic = false;
        }

        heldObject = col;
        if(heldObject.transform.parent != null)
        {
            heldObjectParent = heldObject.transform.parent;
        }
        heldObject.transform.parent = objectHolder;
    }

    private void EndManipulation()
    {
        if (heldObject != null)
        {
            HydraLogger.Log($"Ending manipulation on object {heldObject.name}", DebugChannel.Warning);

            heldObject.transform.parent = heldObjectParent;
            heldObject = null;
            heldObjectParent = null;
        }
    }

    private void PhysicsLockObject()
    {
        if (IsHoldingObject) 
        {
            if(heldObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
            {
                rigidbody.isKinematic = true;
                EndManipulation();
            }
        }
    }

    public void OnDisable()
    {
        EndManipulation();
    }
}
