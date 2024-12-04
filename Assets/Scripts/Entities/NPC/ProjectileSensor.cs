using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSensor : MonoBehaviour
{

    public CircleCollider2D myCollider;

    private AISensor parentSensor;


    private Action<Projectile> OnProjectileDetected;

    public void Setup(AISensor parentSensor, Action<Projectile> onProjectileDetected) {
        this.parentSensor = parentSensor;
        this.OnProjectileDetected = onProjectileDetected;
    }




    private void OnTriggerEnter2D(Collider2D other) {
        Projectile detectedProjectile = IsDetectionProjectile(other);

        if(detectedProjectile != null) {
            OnProjectileDetected?.Invoke(detectedProjectile);
        }
    }



    private Projectile IsDetectionProjectile(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile") == false)
            return null;

        Projectile projectile = other.GetComponent<Projectile>();

        return projectile;
    }
}
