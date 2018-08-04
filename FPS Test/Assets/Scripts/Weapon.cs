using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    private Animator anim;
    private AudioSource _AudioSource;

    public float range = 100f;
    public int bulletsPerMag = 30;
    public int bulletsLeft = 200;

    public int currentBullets;

    public enum ShootMode { Auto, Semi }
    public ShootMode shootingMode;

    public Transform shootPoint;
    public GameObject hitParticles;
    public GameObject bulletImpact;

    public ParticleSystem muzzleFlash;
    public AudioClip shootSound;

    public float fireRate = 0.125f;

    float fireTimer;
    public float damage = 15f;

    private bool isReloading;
    private bool isEmptyReloading;
    private bool isInspecting;
    private bool shootInput;

    private void Start()
    {
        anim = GetComponent<Animator>();
        _AudioSource = GetComponent<AudioSource>();

        currentBullets = bulletsPerMag;
    }

    private void Update()
    {

        switch (shootingMode)
        {
            case ShootMode.Auto:
                shootInput = Input.GetButton("Fire1");
            break;

            case ShootMode.Semi:
                shootInput = Input.GetButtonDown("Fire1");
            break;
        }

        if (shootInput)
        {
            if (currentBullets > 0)
                Fire();

            else if (bulletsLeft > 0)
                DoReload();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentBullets < bulletsPerMag && bulletsLeft > 0)
                DoReload();
        }

        if (fireTimer < fireRate)
            fireTimer += Time.deltaTime;
    }

    void FixedUpdate()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        isReloading = info.IsName("Reload");
        isEmptyReloading = info.IsName("Reload_Empty");
        isInspecting = info.IsName("Inspect");
    }

    private void Fire()
    {
        if (fireTimer < fireRate || currentBullets <=0 || isReloading || isEmptyReloading)
            return;

        RaycastHit hit;

        if(Physics.Raycast(shootPoint.position, shootPoint.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name + " found!");
            GameObject hitParticlesEffect = Instantiate(hitParticles, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            hitParticlesEffect.transform.SetParent(hit.transform);
            GameObject bulletHole = Instantiate(bulletImpact, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
            bulletHole.transform.SetParent(hit.transform);

            Destroy(hitParticlesEffect, 1f);
            Destroy(bulletHole, 2f);

            if(hit.transform.GetComponent<HealthController>())
            {
                hit.transform.GetComponent<HealthController>().ApplyDamage(damage);
            }
        }

        anim.CrossFadeInFixedTime("Fire", 0.01f);
        muzzleFlash.Play();
        PlayShootSound();

        currentBullets--;
        fireTimer = 0.0f;
    }

    public void Reload()
    {
        if (bulletsLeft <= 0) return;

        int bulletsToload = bulletsPerMag - currentBullets;
        int bulletsToDeduct = (bulletsLeft >= bulletsToload) ? bulletsToload : bulletsLeft;

        bulletsLeft -= bulletsToDeduct;
        currentBullets += bulletsToDeduct;
    }

    private void DoReload()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        if (isReloading) return;
        if (isEmptyReloading) return;

        if (currentBullets > 0)
        {
            anim.CrossFadeInFixedTime("Reload", 0.01f);
        }
        else if(currentBullets <= 0)
        {
            anim.CrossFadeInFixedTime("Reload_Empty", 0.01f);
        }
    }

    private void Inspect()
    {
        if (isInspecting) return;

        if(Input.GetButtonDown("Fire3"))
        {
            anim.CrossFadeInFixedTime("Inspect", 0.01f);
        }
    }

    private void PlayShootSound()
    {
        _AudioSource.PlayOneShot(shootSound);
    }
}
