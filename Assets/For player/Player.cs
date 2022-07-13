using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
public class Player : MonoBehaviour
{
	private static List<Waypoint> Waypoints = new List<Waypoint>();

	private NavMeshAgent agent;
	private Animator animator;

	private int index; //* Индекс, отслеживающий текущий waypoint в массиве 
	private int CurrentIndexAttackPoint;

	private bool followingOnWaypoints;
	private const float angleSpeed = 14; //* Скорость разворота игрока вокруг своей оси

	private bool LookAtPoint;
	private Vector3 DirectionToPoint;
	private Vector3 PositionOfClickedPoint;

	[SerializeField]
	private Weapon weapon;
	private bool shooting;
	private bool Restarting;

	//* Для использования Raycast требуется камера, просто кэшируем ее, чтобы каждый раз не вызывать в коде Camera.main
	private Camera cachedCamera;
	private Animator zoomController;
	private void Start()
	{
		cachedCamera = Camera.main;
		zoomController = cachedCamera.gameObject.GetComponent<Animator>();
		animator = GetComponent<Animator>();
		agent = GetComponent<NavMeshAgent>();

		//! Из-за особенностей игрового движка индексу назначаем не 0, а индекс самого последнего элемента,
		//! т.к. новые добавленные точки (waypoints) хранятся в самом начале списка, т.е. последний созданный waypoint на карте имеет индекс 0, т.е. он первый
		index = Waypoints.Count - 1;
		CurrentIndexAttackPoint = index;

		animator.SetTrigger("Idle"); //* Назначаем состояние игроку в начале игры
	}
	private void Update()
	{
		//* Если игра "возобновляется", то отключаем управление игроком
		if (!Restarting)
		{
			if (!followingOnWaypoints) //* Если персонаж не следует по вейпоинтам, то передаем управление в руки игроку
			{

#if UNITY_EDITOR
				if (Input.GetMouseButtonDown(0))
					GetTouchPositionAndAttack(Input.mousePosition); //* Если находимся в редакторе - получаем позицию мыши
#endif

#if UNITY_IOS || UNITY_ANDROID
		if (Input.touchCount > 0)
			GetTouchPositionAndAttack(Input.GetTouch(0).position); //* В противном случае - получаем позицию пальца на экране
#endif
				//* Переменная LookAtPoint требуется для того, чтобы игрок мог некоторое время развернуться и смотреть на точку, при условии 
				//* если игрок не перемещается по вейпоинтам
				if (LookAtPoint && !followingOnWaypoints)
				{
					transform.forward = Vector3.Lerp(transform.forward, DirectionToPoint, angleSpeed * Time.fixedDeltaTime);
				}

				//* Если в данном вейпоинте все враги были уничтожены и мы не дошли до конечного вейпоинта, то изменяем индекс и заставляем игрока перемещаться к следующему вейпоинту
				//* В противном случае, возобновляем игру
				if (Waypoints[CurrentIndexAttackPoint].AreEnemiesDestroyed())
				{
					if (index >= 0)
						StartMove();
					else
						StartCoroutine(RestartGame());
				}
			}

			//* Этот фрагмент кода отвечает за перемещение игрока по вейпоинтам
			if (Waypoints.Count > 0 && followingOnWaypoints && index >= 0)
			{
				//* Если игрок еще не дошел до вейпоинта, то включаем NavMesh
				//! Используем вместо Vector3.Distance другую операцию по определению расстояния
				if ((Waypoints[index].transform.position - transform.position).sqrMagnitude > 0.0169f)
				{
					CurrentIndexAttackPoint = index;
					agent.SetDestination(Waypoints[index].transform.position);
				}
				else //* В противном случае, изменяем состояние игрока
				{
					index--;
					StayOnAttackPosition();
				}
			}
		}
	}

	private IEnumerator RestartGame()
	{
		Restarting = true;
		yield return new WaitForSeconds(1.5f);

		//! Данный фрагмент кода необходим, так как статичные поля хранят значения на протяжении всей игры даже после изменения сцен
		//! В избежании ошибок, очищаем данные коллекции
		PoolOfBullets.ClearPool();
		Waypoints.Clear();

		SceneManager.LoadScene(0);
	}

	//* Добавляем вейпоинт для того, чтобы игрок мог двигаться по маршруту
	public static void AddWaypoint(Waypoint waypoint)
	=> Waypoints.Add(waypoint);

	private void StayOnAttackPosition()
	{
		followingOnWaypoints = false;
		animator.SetTrigger("Idle");
	}

	//* При запуске игры (то есть после тапа по экрану) или после окончания стрельбы заставляем игрока изменить состояние и двигаться по вейпоинтам
	public void StartMove()
	{
		followingOnWaypoints = true;
		animator.SetTrigger("Walking");
		zoomController.SetBool("Zoom", false);
	}

	//* В этом коде даем игроку выстрелить не сразу, а спустя время
	//* => Это требуется для того, чтобы игрок сначала смог развернуться в сторону точку и потом уже выстрелить
	//* => Игрок разворачивается не сразу, это выглядит намного реалистичнее
	IEnumerator LookingAtPointAndShoot()
	{
		LookAtPoint = true;
		yield return new WaitForSeconds(0.3f);

		//* Стреляем из ружья
		weapon.Shoot(PositionOfClickedPoint);
		LookAtPoint = false;
	}

	private void GetTouchPositionAndAttack(Vector3 touchPosition)
	{
		//* Для того, чтобы многократно не вызывать корутин по развороту и стрельбе игрока
		if (!LookAtPoint)
			StartCoroutine(LookingAtPointAndShoot());

		Ray ray = cachedCamera.ScreenPointToRay(touchPosition); //* Испускаем луч из нажатой позиции
		RaycastHit hitData;

		//* Если луч коснулся какого-либо коллайдера, то получаем позицию в данной точке столкновения
		if (Physics.Raycast(ray, out hitData, 1000))
		{
			PositionOfClickedPoint = hitData.point; //* Получаем позицию в данной точке столкновения

			//* Данный код требуется для того, чтобы игрок развернулся в сторону тапа
			//* => т.к. мы используем синюю стрелку игрока (transform.forward), то чтобы разворот игрока в одной плоскости (чтобы синяя стрелка вращалась как-бы в двумерной плоскости),
			//* => то мы используем transform.position.y, чтобы вращение происходило только вокруг своей оси и в сторону оьъекта
			DirectionToPoint = (new Vector3(PositionOfClickedPoint.x, transform.position.y, PositionOfClickedPoint.z) - transform.position).normalized;
		}
		animator.SetTrigger("Shoot");
		zoomController.SetBool("Zoom", true);
	}

}
