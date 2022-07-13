using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolOfBullets : MonoBehaviour
{
	//* Стек идеально подходит для нашего пула, для того, чтобы хранить пули
	private static Stack<Bullet> bullets = new Stack<Bullet>();

	//* Здесь мы добавляем "уничтоженные" пули в наш пул
	public static void AddObjectToPool(Bullet bullet)
  => bullets.Push(bullet);

	//* Здесь мы берем готовую пулю из нашего пула
	public static Bullet GetBulletFromPool()
	=> bullets.Pop();

	//* Здесь мы смотрим, есть ли в пуле пули или нет
	public static bool HasPoolAnyBullets()
	=> bullets.Count > 0;

	//* Очищаем пул, если мы перезапускаем игру и т.д.
	public static void ClearPool()
	=> bullets.Clear();
}
