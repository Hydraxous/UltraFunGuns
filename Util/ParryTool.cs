using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace UltraFunGuns
{
    public static class ParryTool
    {
        //This is copied from the game with minor tweaks
        //TODO fix this
        public static void ParryProjectile(Projectile proj, Vector3 parryDirection)
        {
            proj.hittingPlayer = false;
            proj.friendly = true;
            proj.speed *= 2f;
            proj.homingType = HomingType.None;
            proj.explosionEffect = Prefabs.UK_Explosion.Asset;
            proj.precheckForCollisions = true;

            Rigidbody component = proj.GetComponent<Rigidbody>();
            if (proj.playerBullet)
            {
                proj.boosted = true;
                proj.GetComponent<SphereCollider>().radius *= 4f;
                proj.damage = 0f;
                if (component)
                {
                    component.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                }
                Color color = new Color(1f, 0.35f, 0f);
                MeshRenderer meshRenderer;
                if (proj.TryGetComponent<MeshRenderer>(out meshRenderer) && meshRenderer.material && meshRenderer.material.HasProperty("_Color"))
                {
                    meshRenderer.material.SetColor("_Color", color);
                }
                TrailRenderer trailRenderer;
                if (proj.TryGetComponent<TrailRenderer>(out trailRenderer))
                {
                    Gradient gradient = new Gradient();
                    gradient.SetKeys(new GradientColorKey[]
                    {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
                    }, new GradientAlphaKey[]
                    {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
                    });
                    trailRenderer.colorGradient = gradient;
                }
                Light light;
                if (proj.TryGetComponent<Light>(out light))
                {
                    light.color = color;
                }
            }
            if (component)
            {
                component.constraints = RigidbodyConstraints.FreezeRotation;
            }
            else
            {
                MonoSingleton<TimeController>.Instance.ParryFlash();
            }
            if (proj.explosive)
            {
                proj.explosive = false;
            }
            Rigidbody component2 = proj.GetComponent<Rigidbody>();
            if (component2 && component2.useGravity)
            {
                component2.useGravity = false;
            }
            proj.transform.forward = parryDirection;
            if (proj.speed == 0f)
            {
                component2.velocity = parryDirection * 250f;
            }
            else if (proj.speed < 100f)
            {
                proj.speed = 100f;
            }
            if (proj.spreaded)
            {
                ProjectileSpread componentInParent = proj.GetComponentInParent<ProjectileSpread>();
                if (componentInParent != null)
                {
                    componentInParent.ParriedProjectile();
                }
            }
            proj.transform.SetParent(null, true);
        }

        //TODO fix this
        public static void ParryCannonball(Cannonball cannonball, Vector3 direction)
        {
            cannonball.transform.forward = direction;
            cannonball.Launch();
        }

        //TODO fix this
        public static bool TryParryCannonball(Collider collider, Vector3 direction, out Cannonball cannonball)
        {
            cannonball = null;

            Transform target = (collider.attachedRigidbody != null) ? collider.attachedRigidbody.transform : collider.transform;

            if (target.TryGetComponent<ParryHelper>(out ParryHelper  parryHelper))
            {
                target = parryHelper.target;
            }

            if (target.TryGetComponent<Cannonball>(out cannonball) && cannonball.launchable)
            {
                ParryCannonball(cannonball, direction);
                return true;
            }

            return false;

        }

        //TODO fix this
        public static bool TryParryProjectile(Collider collider, Vector3 direction, out Projectile foundProjectile)
        {
            foundProjectile = null;
            Transform target = (collider.attachedRigidbody != null) ? collider.attachedRigidbody.transform : collider.transform;

            if (target.TryGetComponent<ParryHelper>(out ParryHelper parryHelper))
            {
                target = parryHelper.target;
            }

            if (target.TryGetComponent<Projectile>(out Projectile  projectile))
            {
                foundProjectile = projectile;
                ParryProjectile(projectile, direction);
                return true;
            }

            return false;
        }
    }
}
