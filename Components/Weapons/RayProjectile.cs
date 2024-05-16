using System;
using UnityEngine;

public class RayProjectile : MonoBehaviour
{
    public float StepDistance;
    public Action<RaycastHit, RayProjectile> OnHit;
    public LayerMask Hitmask;

    private void Update()
    {
        MoveProjectile();
    }

    private void MoveProjectile()
    {
        Vector3 step = transform.forward * StepDistance * Time.deltaTime;
        if (!CheckStep(step, out RaycastHit hit))
        {
            transform.position += step;
            return;
        }

        OnHit.Invoke(hit, this);
    }

    private bool CheckStep(Vector3 travel, out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, travel.normalized, out hit, travel.magnitude, Hitmask);
    }
}
