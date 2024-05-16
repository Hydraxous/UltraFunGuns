using UltraFunGuns;
using UnityEngine;

public class RayBasedProjectile : MonoBehaviour, ICleanable
{
    public float projectileSpeed;
    public float projectileSize;
    public float imortalTime = 0.001f;
    public bool affectedByGravity;
    public float gravityMultiplier = 1.0f;
    public bool ricochet;
    public bool hitTriggers;
    public LayerMask hitMask;
    public float damage;
    public float critMultiplier;
    public bool tryForExplode;

    public GameObject spawnAtImpact;

    public GameObject sourceWeapon;

    public string[] eventDataTags;

    private float imortalTimeRemaining;
    private bool dieNextFrame;
    private float distanceToSurface;

    public delegate void OnProjectileDeathHandler(ProjectileDeathEvent projectileDeathEvent);
    public OnProjectileDeathHandler OnProjectileDeath;

    private ProjectileDeathEvent deathParamters = new ProjectileDeathEvent();

    private void Start()
    {
        imortalTimeRemaining = imortalTime;
    }

    private RaycastHit hit;

    void Update()
    {
        if(dieNextFrame)
        {
            Impact();
        }

        dieNextFrame = CheckImpact();

        imortalTimeRemaining -= Time.deltaTime;

        transform.position += GetNextPositionStep();
    }

    Vector3 GetNextPositionStep()
    {
        Vector3 nextPositionLocal = Vector3.zero;
        Vector3 currentDirection = transform.forward;
        Vector3 currentPosition = transform.position;

        if (affectedByGravity)
        {
            nextPositionLocal += Physics.gravity * gravityMultiplier * Time.deltaTime;
        }

        bool hittingArmor = false;

        if (hit.collider != null)
            hittingArmor = hit.collider.tag == "Armor";

        if((ricochet || hittingArmor) && dieNextFrame)
        {
            float travelDistance = projectileSpeed * Time.deltaTime;

            while(travelDistance > 0.0f && Physics.Raycast(currentPosition, currentDirection, out RaycastHit surfaceHit, travelDistance, hitMask))
            {
                SurfaceHitInformation surfaceHitInformation = new SurfaceHitInformation()
                {
                    hitCollider = surfaceHit.collider,
                    hittingObject = gameObject,
                    normal = surfaceHit.normal,
                    position = surfaceHit.point,
                    velocity = currentDirection*projectileSpeed
                };

                travelDistance -= surfaceHit.distance;
                currentPosition = surfaceHit.point;
                currentDirection = Vector3.Reflect(currentDirection, surfaceHit.normal);

                HitCollider(hit.collider);
            }
            if (travelDistance > 0.0f)
            {
                currentPosition += currentDirection * travelDistance;
            }
            transform.forward = currentDirection;
            nextPositionLocal += currentPosition - transform.position;
            dieNextFrame = false;
        }else
        {
            nextPositionLocal += currentDirection * ((dieNextFrame) ? (distanceToSurface) : projectileSpeed * Time.deltaTime);

        }

        return nextPositionLocal;
    }

    bool CheckImpact()
    {
        Vector3 rayDirection = transform.forward * projectileSpeed;

        if(affectedByGravity)
            rayDirection += Physics.gravity * gravityMultiplier;

        float range = rayDirection.magnitude * Time.deltaTime;

        if(projectileSize > 0.0f)
        {
            if (!Physics.SphereCast(transform.position, projectileSize, rayDirection.normalized, out hit, range, hitMask))
                return false;
        }
        else
        {
            if (!Physics.Raycast(transform.position, rayDirection.normalized, out hit, range, hitMask))
                return false;
        }

        if (!hitTriggers && hit.collider.isTrigger)
            return false;

        deathParamters.normal = hit.normal;
        deathParamters.velocity = transform.forward * projectileSpeed;
        if (imortalTimeRemaining < 0.0f)
        {
            distanceToSurface = (hit.point - transform.position).magnitude;
            return true;
        }

        return false;
    }

    private void Impact()
    {
        SurfaceHitInformation surfaceHitInformation = new SurfaceHitInformation()
        {
            hitCollider = hit.collider,
            normal = hit.normal,
            position = hit.point,
            hittingObject = gameObject,
            velocity = transform.forward * projectileSpeed
        };

        if (!HitCollider(hit.collider))
        {
            transform.forward = Vector3.Reflect(transform.forward, hit.normal);
            return;
        }

        if(spawnAtImpact != null && hit.collider != null)
        {
            GameObject spawnedAtImpact = Instantiate(spawnAtImpact, hit.point+(hit.normal*0.01f), Quaternion.LookRotation(hit.normal,Vector3.up));
        }

        Destroy(gameObject);
    }

    private bool HitCollider(Collider col)
    {
        if (col == null)
            return false;

        if(col.IsColliderEnemy(out EnemyIdentifier eid))
        {
            eid?.DeliverDamage(eid.gameObject, transform.forward*projectileSpeed, hit.point, damage, tryForExplode,critMultiplier,sourceWeapon);
        }

        if(col.TryFindComponent<IUFGInteractionReceiver>(out IUFGInteractionReceiver uFGInteraction))
        {
            UFGInteractionEventData eventData = new UFGInteractionEventData();
            eventData.power = damage;
            eventData.direction = transform.forward;
            eventData.interactorPosition = transform.position;
            eventData.tags = (eventDataTags == null) ? new string[0] : eventDataTags;
            uFGInteraction.Interact(eventData);
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * projectileSpeed * 0.0166666667f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, projectileSize);
    }

    private void OnDestroy()
    {
        deathParamters.direction = transform.forward;
        deathParamters.position = transform.position;
        OnProjectileDeath?.Invoke(deathParamters);
    }

    public void Cleanup()
    {
        Destroy(gameObject);
    }
}

public struct SurfaceHitInformation
{
    public Vector3 position;
    public Vector3 normal;
    public Vector3 velocity;
    public Collider hitCollider;
    public GameObject hittingObject;
}

public struct ProjectileDeathEvent
{
    public Vector3 position;
    public Vector3 direction;
    public Vector3 normal;
    public Vector3 velocity;
}
