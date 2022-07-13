using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	//* Ссылка на префаб нашей пули
	//! Используем конкретно Bullet вместо Gameobject для того, чтобы постоянно не использовать GetComponent для взаимодействия со скриптом
	[SerializeField]
	private Bullet Bullet;
	[SerializeField]
	private Transform Firepoint;

	private AudioSource fireSound;
	private void Start()
	=> fireSound = GetComponent<AudioSource>();

	//* Вызываем функцию стрельбы и указываем точку полета для нашей пули
	public void Shoot(Vector3 PositionToFly)
	{
		fireSound.Play();

		Bullet bullet;
		//* Если в пуле объектов уже есть пули - просто берем нужный объект из пула
		if (PoolOfBullets.HasPoolAnyBullets())
		{
			bullet = PoolOfBullets.GetBulletFromPool();
		}
		else //* Если нет - создаем новый объект
		{
			bullet = Instantiate(this.Bullet);
		}

		//* Инициализируем нашу пулю
		bullet.transform.position = Firepoint.transform.position;
		bullet.transform.forward = (PositionToFly - bullet.transform.position).normalized;
		bullet.gameObject.SetActive(true);
	}
}
