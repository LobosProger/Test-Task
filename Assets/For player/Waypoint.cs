using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	//* В список добавляем врагов, которые игрок должен уничтожить
	[SerializeField]
	private List<Enemy> enemies = new List<Enemy>();
	//! HashSet требуется для того, чтобы осуществить следующую оптимизацию:
	//! вместо проверки здоровье каждого врага для возобновления движения по вейпоинтам, мы удаляем из списка убитых врагов и затем просто проверяем размер коллекции
	//! т.е. если в коллекции не осталось элементов, то можно считать, что враги уничтожены и можно двигаться по вейпоинтам
	//! HashSet позволяет удалять, добавлять элементы за O(1)
	//! Если бы мы проверяли здоровье каждого врага, то это заняло бы по времени O(n)
	public HashSet<Enemy> enemiesSet = new HashSet<Enemy>();

	private void Awake()
	{
		//* Добавляем вейпоинт для того, чтобы игрок мог двигаться
		Player.AddWaypoint(this);
		enemiesSet = new HashSet<Enemy>(enemies);

		//* Чтобы враг мог сказать вейпоинту о том, что он "мертв", мы добавляем ссылку на вейпоинт
		foreach (Enemy every in enemiesSet)
			every.SetWaypoint(this);
	}

	//* Если из коллекции не осталось ни одного врага, то они все уничтожены
	public bool AreEnemiesDestroyed()
	=> enemiesSet.Count == 0;

	//* Враг вызывает данный код, когда он погибает, т.е. когда его игрок убивает
	public void EnemyDestroy(Enemy enemy)
	=> enemiesSet.Remove(enemy);
}
