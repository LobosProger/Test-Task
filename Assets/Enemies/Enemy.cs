using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
	public int Health = 5;
	private int maxHealth;

	private Rigidbody[] rigidbodies;
	private Waypoint waypoint;
	private Animator animator;

	//* Для того, чтобы не отображать UI после убийства врага - прикрепляем ссылку на канвас
	[SerializeField]
	private Canvas canvas;
	//* Здесь прикрепляем линию на здоровье
	[SerializeField]
	private Image healthLine;

	private void Awake()
	{
		maxHealth = Health;
		animator = GetComponent<Animator>();
		rigidbodies = GetComponentsInChildren<Rigidbody>();
		//* Изменяем состояние рэгдолла (Rigidbody) для того, чтобы враг не "упал"
		ChangeStateOfRagdoll();
	}

	IEnumerator Death()
	{
		//* Отключаем Idle
		animator.enabled = false;
		//* Включаем рэгдоллы
		ChangeStateOfRagdoll();
		yield return new WaitForSeconds(1.5f);
		//* После прохождения некоторого времени отключаем рэгдоллы, чтобы не затрачивать ресурсы процессора на физику
		ChangeStateOfRagdoll();
	}

	private void ChangeStateOfRagdoll()
	{
		foreach (Rigidbody every in rigidbodies)
			every.isKinematic = !every.isKinematic;
	}

	public void Damage(int damage)
	{
		//* Если здоровья много - наносим урон
		if (Health > 0)
		{
			Health -= damage;
			//* Обновляем показатель здоровья
			healthLine.fillAmount = ((float)Health / (float)maxHealth);
			//* Если здоровья мало - выключаем канвас, удаляем из HashSet нашего врага и запускаем рэгдоллы
			if (Health <= 0)
			{
				canvas.enabled = false;
				waypoint.EnemyDestroy(this);
				StartCoroutine(Death());
			}
		}
	}

	public void SetWaypoint(Waypoint waypoint)
	=> this.waypoint = waypoint;
}
