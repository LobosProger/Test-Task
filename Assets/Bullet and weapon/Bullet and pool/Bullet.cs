using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	//* Здесь можем присвоить скорость нашей пули
	[SerializeField]
	private float Speed;

	private Rigidbody rb;

	private void OnEnable()
	{
		if (rb is null)
			rb = GetComponent<Rigidbody>();

		rb.velocity = transform.forward * Speed;

		//* "Уничтожаем" нашу пулю, если она просто летит в бездну
		Invoke(nameof(DestroyBullet), 3f);
	}

	private void OnTriggerEnter(Collider other)
	{
		//* Если мы попали во врага - наносим ему урон
		if (other.CompareTag("Enemy"))
			other.GetComponent<Enemy>().Damage(1);

		DestroyBullet();
	}

	private void DestroyBullet()
	{
		//* Здесь мы не уничтожаем нашу пулю, а добавляем ее в пул объектов, предварительно деактивировав
		PoolOfBullets.AddObjectToPool(this);
		gameObject.SetActive(false);
	}
}
