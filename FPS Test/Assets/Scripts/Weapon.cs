using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	private Animator anim;
	private AudioSource _AudioSource;

	public float range = 125f;
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
    public AudioClip magOut;
    public AudioClip magIn;
    public AudioClip boltBack;
    public AudioClip boltForward;
    public AudioClip ironOut;
    public AudioClip ironIn;

    public float fireRate = 0.1f;
	public float damage = 20f;
	private bool reloadEmpty;
    private bool reloadNorm;

	float fireTimer;
	private bool isReloading;
    private bool isInspecting;
    private bool shootInput;
	private Vector3 originalPosition;
	public Vector3 aimPosition;
	public float aodSpeed = 8f;

	public float spreadFactor = 0.1f;

	void Start ()
	{
		
		anim = GetComponent<Animator>();
		_AudioSource = GetComponent<AudioSource>();

		currentBullets = bulletsPerMag;
		originalPosition = transform.localPosition;
	}
	
	void Update ()
	{
		
		switch(shootingMode)
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
            Reload();
            DoReload();
        }

        if (fireTimer < fireRate)
			fireTimer += Time.deltaTime;

		AimDownSights();

		Inspect();
	}

	void FixedUpdate()
	{
		AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

		isReloading = info.IsName("Reload");
	}

    private void AimDownSights()
    {
        if (Input.GetButton("Fire2") && !isReloading)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * aodSpeed);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * aodSpeed);
        }
    }

    private void Inspect()
    {
        if (Input.GetButtonDown("Fire3") && !isReloading)
        {
            anim.CrossFadeInFixedTime("Inspect", 0.01f);
        }
    }

    private void Fire()
	{
		if (fireTimer < fireRate || currentBullets <= 0 || isReloading)
			return;

		RaycastHit hit;
		
		Vector3 shootDirection = shootPoint.transform.forward;
		shootDirection.x += Random.Range(-spreadFactor, spreadFactor);
		shootDirection.y += Random.Range(-spreadFactor, spreadFactor);

		if (Physics.Raycast(shootPoint.position, shootDirection, out hit, range))
		{
			Debug.Log(hit.transform.name + " Was Hit!");

			GameObject hitParticlesEffect = Instantiate(hitParticles, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
			hitParticlesEffect.transform.SetParent(hit.transform);
			GameObject bulletHole = Instantiate(bulletImpact, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
			bulletHole.transform.SetParent(hit.transform);

			Destroy(hitParticlesEffect, 1f);
			Destroy(bulletHole, 2f);

			if (hit.transform.GetComponent<HealthController>())
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

        int bulletsToLoad = bulletsPerMag - currentBullets;
        int bulletsToDeduct = (bulletsLeft >= bulletsToLoad) ? bulletsToLoad : bulletsLeft;

        bulletsLeft -= bulletsToDeduct;
        currentBullets += bulletsToDeduct;
    }

    private void DoReload()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        anim.CrossFadeInFixedTime("Reload", 0.01f);
    }

    private void PlayShootSound()
    {
        _AudioSource.PlayOneShot(shootSound);
    }
}
