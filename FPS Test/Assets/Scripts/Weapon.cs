using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    private Animator anim;
    private AudioSource _AudioSource;

    [Header("Properties")]
    public float range = 100f;
    public int bulletsPerMag = 30;
    public int bulletsLeft = 200;
    public float damage = 15f;
    public int currentBullets;
    public float fireRate = 0.125f;
    public enum ShootMode { Auto, Semi }
    public ShootMode shootingMode;
    public float spreadFactor = 0.5f;

    [Header("UI")]
    public Text ammoText;

    [Header("Setup")]
    public Transform shootPoint;
    public GameObject hitParticles;
    public GameObject bulletImpact;
    public ParticleSystem muzzleFlash;

    [Header ("Sound Effects")]
    public AudioClip shootSound;

    float fireTimer;
    private bool isReloading;
    private bool isEmptyReloading;
    private bool isInspecting;
    private bool isFiring;
    private bool shootInput;
    private Vector3 originalPosition;

    [Header("ADS")]
    public Vector3 aimPosition;
    public float aodSpeed = 5;

    private void Start()
    {
        anim = GetComponent<Animator>();
        _AudioSource = GetComponent<AudioSource>();

        currentBullets = bulletsPerMag;
        originalPosition = transform.localPosition;

        UpdateAmmoText();
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

        if (Input.GetButtonDown("Fire3"))
        {
            Inspect();
        }

        if (fireTimer < fireRate)
            fireTimer += Time.deltaTime;

        AimDownSights();
    }

    void FixedUpdate()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        isReloading = info.IsName("Reload");
        isEmptyReloading = info.IsName("Reload_Empty");
        isInspecting = info.IsName("Inspect");
        isFiring = info.IsName("Fire");
    }

    private void AimDownSights()
    {
        if (Input.GetButton("Fire2") && !isReloading && !isEmptyReloading && !isInspecting)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * aodSpeed);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * aodSpeed);
        }
    }

    private void Fire()
    {
        if (fireTimer < fireRate || currentBullets <=0 || isReloading || isEmptyReloading)
            return;

        RaycastHit hit;

        Vector3 shootDirection = shootPoint.transform.forward;
        shootDirection.x += Random.Range(-spreadFactor, spreadFactor);
        shootDirection.y += Random.Range(-spreadFactor, spreadFactor);

        if (Physics.Raycast(shootPoint.position, shootDirection, out hit, range))
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

        UpdateAmmoText();

        fireTimer = 0.0f;
    }

    public void Reload()
    {
        if (bulletsLeft <= 0) return;

        int bulletsToload = bulletsPerMag - currentBullets;
        int bulletsToDeduct = (bulletsLeft >= bulletsToload) ? bulletsToload : bulletsLeft;

        bulletsLeft -= bulletsToDeduct;
        currentBullets += bulletsToDeduct;

        UpdateAmmoText();
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
        if (Input.GetButtonDown("Fire3") && !isReloading && !isEmptyReloading && !isFiring && !isInspecting)
        {
            anim.CrossFadeInFixedTime("Inspect", 0.01f);
        }
    }

    private void PlayShootSound()
    {
        _AudioSource.PlayOneShot(shootSound);
    }

    private void UpdateAmmoText()
    {
        ammoText.text = currentBullets + " / " + bulletsLeft;
    }
}
