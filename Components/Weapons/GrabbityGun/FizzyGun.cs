using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltraFunGuns;

[WeaponAbility("Manipulate", "Manipulate a physics object <color=orange>Fire 1</color>", 0, RichTextColors.aqua)]
[WeaponAbility("Lock", "Lock a physics object in place <color=orange>Fire 2</color>", 1, RichTextColors.aqua)]
[WeaponAbility("Rotat-e", "Rotate held object <color=orange>secret</color>", 2, RichTextColors.aqua)]
[UFGWeapon("FizzyGun", "Fizix Applicator (Experimental)", 3, true, WeaponIconColor.Blue, false)]
public class FizzyGun : UltraFunGunBase
{
    public float forceOnRb = 150.0f;
    public float forwardBias = 5.0f;
    public float groupForce = 2.0f;
    public float groupMaxRange = 10.0f;

    public float rotateSpeed = 15.0f;

    public float stopRange = 0.2f;
    public float manipulateSpeed = 400.0f;
    
    public bool setVelo = false;

    public float maxRange = 10.0f;
    public float minAngle = 0.60f; //45o angle
    public bool IsHoldingObject => (heldObjectRigidbody != null);

    private Transform objectHolder;

    private Rigidbody heldObjectRigidbody;
    private Transform heldObjectParent;
    private bool waitForPrimaryFireReleased;

    private Dictionary<Rigidbody, RigidbodySettings> oldKinematicStates = new Dictionary<Rigidbody, RigidbodySettings>();

    private RigidbodySettings manipulatedSettings = new RigidbodySettings(RigidbodyConstraints.FreezeRotation, RigidbodyInterpolation.None, false);

    private bool checking, hittingSomething, rotating;

    private Vector3 holderPositionLastFrame;

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

        if (UFGInput.SecretButton.IsPressed() && IsHoldingObject)
        {
            RotateManipulation();
        }else if(rotating)
        {
            rotating = false;
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


        Vector3 rigidbodyToHolder = (ObjectHolder.position - (heldObjectRigidbody.centerOfMass+heldObjectRigidbody.position));

        float modifier = manipulateSpeed;

        if(rigidbodyToHolder.magnitude < manipulateSpeed)
        {
            modifier = rigidbodyToHolder.magnitude;
        }

        heldObjectRigidbody.velocity = rigidbodyToHolder.normalized * modifier;

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

    Quaternion lookRot = Quaternion.identity;

    private void RotateManipulation()
    {
        if(!rotating)
        {
            lookRot = CameraController.Instance.transform.rotation;
            rotating = true;
        }

        HydraUtils.SetPlayerRotation(lookRot);

        Vector2 lookInput = InputManager.Instance.InputSource.Look.ReadValue<Vector2>();

        Vector3 axis = new Vector3(lookInput.y,-lookInput.x, 0.0f);

        axis = CameraController.Instance.transform.rotation * axis;

        //heldObjectRigidbody.Rotate(axis, lookInput.magnitude * Time.deltaTime * rotateSpeed);
        heldObjectRigidbody.transform.RotateAround(ObjectHolder.position, axis, lookInput.magnitude * Time.deltaTime * rotateSpeed);
    }

    private bool CheckForCollider(out Collider col)
    {
        col = null;
        hittingSomething = false;

        RaycastHit[] hits = Physics.SphereCastAll(mainCam.position, 0.015f, mainCam.forward, LayerMask.GetMask("Environment", "Outdoors", "Outdoors Non-solid", "Default", "Limb", "Gib", "Armor", "Projectile"));

        if(hits.Length <= 0)
        {
            return false;
        }

        hits = hits.OrderBy(x => x.distance).ToArray();

        hits = hits.Where(x => x.collider.attachedRigidbody != null && x.collider.gameObject.name != "CameraCollisionChecker").ToArray();

        hits = hits.Where(x => !x.collider.attachedRigidbody.gameObject.name.Contains("Player")).ToArray();

        if (hits.Length > 0)
        {
            UltraFunGuns.Log.LogWarning(hits[0].collider.gameObject.name);
            hittingSomething = true;
            lastDistFromCamera = hits[0].distance;
            UpdateHolderPosition();
            BeginManipulation(hits[0].collider);
            return true;

        }

        /*
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
        */
        return false;
    }

    private float lastDistFromCamera = 0.0f;

    //Updates the position of the object holder relative to the camera
    private void UpdateHolderPosition()
    {

        Vector3 camPos = mainCam.position;
        Vector3 camDirection = mainCam.forward;

        Vector3 newWorldPos = camPos + camDirection * lastDistFromCamera;

        if(IsHoldingObject)
        {
            holderPositionLastFrame = heldObjectRigidbody.position+heldObjectRigidbody.centerOfMass;
        }
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

        UltraFunGuns.Log.LogWarning($"Beginning manipulation on object {heldObjectRigidbody.name}");


        if(!oldKinematicStates.ContainsKey(heldObjectRigidbody))
        {
            oldKinematicStates.Add(heldObjectRigidbody, new RigidbodySettings(heldObjectRigidbody));
            manipulatedSettings.ApplySettings(heldObjectRigidbody);
        }else
        {
            oldKinematicStates[heldObjectRigidbody].ApplySettings(heldObjectRigidbody);
            oldKinematicStates.Remove(heldObjectRigidbody);
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
     
        UltraFunGuns.Log.LogWarning($"Ending manipulation on object {heldObjectRigidbody.name}");

        heldObjectRigidbody.transform.parent = heldObjectParent;
        heldObjectParent = null;
        if(!heldObjectRigidbody.isKinematic)
        {
            heldObjectRigidbody.velocity = ObjectHolder.transform.position - holderPositionLastFrame;
        }
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

public struct RigidbodySettings
{
    public RigidbodyConstraints constraints;
    public RigidbodyInterpolation interpolation;
    public bool isKinematic;

    public RigidbodySettings(Rigidbody rigidbody)
    {
        SetSettings(rigidbody);
    }

    public RigidbodySettings(RigidbodyConstraints constraints, RigidbodyInterpolation interpolation, bool isKinematic)
    {
        this.constraints = constraints;
        this.interpolation = interpolation;
        this.isKinematic = isKinematic;
    }

    public void ApplySettings(Rigidbody rigidbody)
    {
        rigidbody.isKinematic = isKinematic;
        rigidbody.interpolation = interpolation;
        rigidbody.constraints = constraints;
    }

    public void SetSettings(Rigidbody rigidbody)
    {
        constraints = rigidbody.constraints;
        interpolation = rigidbody.interpolation;
        isKinematic = rigidbody.isKinematic;
    }
}
