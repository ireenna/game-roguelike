using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MovingObject
{
	public int wallDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1f;
	public Text foodText;
	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;
	public HealthBar healthBar;
	private Transform player;
	private BoardManager boardManager;
	public int x;
	public int y;


	private Animator animator;
	public int food;

	protected override void Start()
	{
		animator = GetComponent<Animator>();
        food = GameManager.instance.playerFoodPoints;
		player = GameObject.FindGameObjectWithTag("Player").transform;
		boardManager = MonoBehaviour.FindObjectOfType<BoardManager>();

		foodText.text = "Food: " + food;
		x = 0;
		y = 0;

		base.Start();
	}

	void Update()
	{
		if (!GameManager.instance.playersTurn) {
			return;
		}

		//var w = boardManager.walls;
  //      var p = (transform.position.x, transform.position.y);

        //var posMinimax = Minimax();
        //if (posMinimax != null)
        //{
        //    AttemptMove<Wall>((int)(-x + posMinimax.Value.Item1), (int)(-y + posMinimax.Value.Item2));
        //}

        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal != 0)
        {
            vertical = 0;
        }

        if (horizontal != 0 || vertical != 0)
        {
            AttemptMove<Wall>(horizontal, vertical);
        }
    }
		
	public void LoseFood (int loss)
	{
		animator.SetTrigger("playerHit");

		food -= loss;
		healthBar.SetHealth(food);

		foodText.text = "-" + loss + " Food: " + food;

		CheckIfGameOver();
	}

	protected override void AttemptMove <T> (int xDir, int yDir)
	{
        //food--;
        //foodText.text = "Food: " + food;

        base.AttemptMove<T>(xDir, yDir);


        RaycastHit2D hit;

		if (Move(xDir, yDir, out hit)) {
			x = x + xDir;
			y = y + yDir;
			var isInside = new Vector3(x, y, 0);
			if (boardManager.meal.Contains(isInside)){
				boardManager.meal.Remove(isInside);
			}
			SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }
  //      else
  //      {
		//	base.AttemptMove<T>(xDir, yDir);
		//}

		CheckIfGameOver();

		GameManager.instance.playersTurn = false;
	}

	protected override void OnCantMove <T> (T component)
	{
		//Wall hitWall = component as Wall;
		//hitWall.DamageWall(wallDamage);
		//animator.SetTrigger("playerChop");
	}

	private void OnTriggerEnter2D (Collider2D other)
	{
		if (other.CompareTag("Exit")) {
			Invoke("Restart", restartLevelDelay);
			enabled = false;
		}
		else if (other.CompareTag("Food")) {
			food += pointsPerFood;
			foodText.text = "+" + pointsPerFood + " Food: " + food;
			healthBar.SetHealth(food);


			SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

			other.gameObject.SetActive(false);	
		}
		else if (other.CompareTag("Soda")) {
			food += pointsPerSoda;
			healthBar.SetHealth(food);

			foodText.text = "+" + pointsPerSoda + " Food: " + food;

			SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

			other.gameObject.SetActive(false);
		}
	}

	private void Restart()
	{
		SceneManager.LoadScene(0);
	}

	private void OnDisable()
	{
		GameManager.instance.playerFoodPoints = food;
	}

	private void CheckIfGameOver()
	{
		if (food <= 0) {
			SoundManager.instance.PlaySingle(gameOverSound);
			SoundManager.instance.musicSource.Stop();

			GameManager.instance.GameOver();
		}
	}

	public List<(int, int)> PossibleDirs()
	{
		(int, int)[] dirs = { (0,1), (1,0), (0,-1), (-1,0)};
		List<(int, int)> possibleDirs = new List<(int,int)>();
		var walls = boardManager.walls;

		foreach (var dir in dirs)
        {
			var a = (x + dir.Item1);
			var b = (y + dir.Item2);
			if (!walls.Contains(new Vector3(a, b, 0)))
            {
				if(a>=0 && a<15 && b>=0 && b < 15)
                {
					possibleDirs.Add(dir);
                }
			}
        }

		return possibleDirs;
    }
	public List<(int, int)> PossibleEnemyPos()
	{
		(int, int)[] dirs = { (0, 1), (1, 0), (0, -1), (-1, 0) };
		List<(int, int)> possibleDirs = new List<(int, int)>();
		var walls = boardManager.walls;
		var enemies = GameManager.instance.enemies.Where(x=>x.life>0);
        foreach (var enemy in enemies)
        {
			foreach (var dir in dirs)
			{
				var a = (float)(enemy.x + dir.Item1);
				var b = (float)(enemy.y + dir.Item2);
				if (!walls.Contains(new Vector3(a, b, 0f)))
				{
					if (a >= 0 && a < 15 && b >= 0 && b < 15)
					{
						possibleDirs.Add(((int)a, (int)b));
					}
				}
			}
        }

		

		return possibleDirs;
	}

	public (int,int)? Minimax()
    {
		//boardManager.meal =new List<Vector3>();
		List<(int, int)> possibleDirs = PossibleDirs();
		List<(int, int)> possiblePos = possibleDirs.Select(a => (a.Item1 + x, a.Item2 + y)).ToList();
		List<(int, int)> possibleEnemiesPos = PossibleEnemyPos();

		float oldVal = 1000f;
		Vector3 nearestFood = new Vector3();
		var listOfGoodHavka = boardManager.meal.Where(x => !boardManager.walls.Contains(new Vector3(x.x, x.y)));
        foreach (var havka in listOfGoodHavka)
        {
			var newVal = GetBFSPath((x,y), ((int)havka.x,(int)havka.y), boardManager.grid, "search for havka").Count();
			nearestFood = havka;
			if (newVal <= oldVal){
				oldVal = newVal;
				nearestFood = havka;
            }
        }

		MyNode mainNode_max = new MyNode(null, null, true, 0);
        foreach (var item in possiblePos)
        {
			mainNode_max.Children.Add(new MyNode(mainNode_max, item, false, 0));
        }
		
		if (possibleEnemiesPos.Count() != 0)
		{
			foreach (var secondNode_min in mainNode_max.Children)
			{
				foreach (var enemyMoves in possibleEnemiesPos)
				{
					if(boardManager.walls.Contains(new Vector3(nearestFood.x, nearestFood.y, 0)))
                    {
						throw new Exception("Walls");
                    }
					var distToGhost = GetBFSPath(secondNode_min.Pos_xy.Value, enemyMoves, boardManager.grid, "enemy").Count();

					if (boardManager.meal.Count() != 0)
					{
						var distToFood = GetBFSPath(secondNode_min.Pos_xy.Value, ((int)nearestFood.x, (int)nearestFood.y), boardManager.grid, "Havka").Count();
						if (food > 50)
						{
							if (distToGhost <= 0.1)
							{

								secondNode_min.Children.Add(new MyNode(secondNode_min, enemyMoves, true, -1000));
							}
							else
							{
								secondNode_min.Children.Add(new MyNode(secondNode_min, enemyMoves, true, (-distToFood)));
							}
                        }
                        else
                        {
							if(distToFood> distToGhost/2)
                            {
								secondNode_min.Children.Add(new MyNode(secondNode_min, enemyMoves, true, -distToGhost));
							}
							else
                            {
								secondNode_min.Children.Add(new MyNode(secondNode_min, enemyMoves, true, -distToFood));
							}
						}
                    }
                    else
                    {
						var distToExit = GetBFSPath(secondNode_min.Pos_xy.Value, (14,14), boardManager.grid, "Exit").Count();
						if (distToExit <= 1)
						{

							secondNode_min.Children.Add(new MyNode(secondNode_min, enemyMoves, true, 1000));
						}
						else if (distToGhost <= 1)
                        {
							secondNode_min.Children.Add(new MyNode(secondNode_min, enemyMoves, true, -1000));
						}
						else
						{
							secondNode_min.Children.Add(new MyNode(secondNode_min, enemyMoves, true,  distToExit - distToGhost));
						}
					}
				}
			}
        }
        else
        {
			if (food < 100 && boardManager.meal.Count() !=0)
			{
				if(!boardManager.walls.Contains(new Vector3(14,14)))
					boardManager.walls.Add(new Vector3(14, 14));
				var BFSpath = GetBFSPath((x, y), ((int)nearestFood.x, (int)nearestFood.y), boardManager.grid, "Konets");
				if (BFSpath.Count() > 1)
				{
					var BFSstep = BFSpath[BFSpath.Count() - 2];

					if (!boardManager.walls.Contains(new Vector3((float)BFSstep.Item1, (float)BFSstep.Item2, 0f)))
					{
						return (BFSstep.Item1, BFSstep.Item2);
					}
				}
            }
            else
            {
				if (boardManager.walls.Contains(new Vector3(14, 14)))
					boardManager.walls.Remove(new Vector3(14, 14));

				var BFSpath = GetBFSPath((x, y), (14, 14), boardManager.grid, "Konets");
				if (BFSpath.Count() > 1)
				{
					var BFSstep = BFSpath[BFSpath.Count() - 2];

					if (!boardManager.walls.Contains(new Vector3((float)BFSstep.Item1, (float)BFSstep.Item2, 0f)))
					{
						return (BFSstep.Item1, BFSstep.Item2);
					}
				}
				else
				{
					var BFSstep = BFSpath[BFSpath.Count() - 1];

					if (!boardManager.walls.Contains(new Vector3((float)BFSstep.Item1, (float)BFSstep.Item2, 0f)))
					{
						return (14, 14);
					}
				}
			}
		}
        foreach (var secondNode_min in mainNode_max.Children)
        {
			var oldValue = 1000;
            foreach (var lowlvlMAX in secondNode_min.Children)
            {
				if(lowlvlMAX.Value < oldValue)
                {
					oldVal = lowlvlMAX.Value;
					secondNode_min.Value = oldVal;
                }
            }
        }
		float finishNode = -1000f;
		(int,int)? finishCords = null;
        foreach (var secondNode_min in mainNode_max.Children)
        {
			if(secondNode_min.Value > finishNode)
            {
				finishNode = secondNode_min.Value;
				finishCords = secondNode_min.Pos_xy.Value;
            }
        }
		return finishCords;
	}
	public List<(int, int)> GetBFSPath((int, int) start, (int, int) target, int[,] grid, string item)
	{
		Queue<(int, int)> queue = new Queue<(int, int)>();
		queue.Enqueue(start);
		List<Node> path = new List<Node>();
		List<(int, int)> visited = new List<(int, int)>();
		(int, int)[] directions = { (0, -1), (1, 0), (0, 1), (-1, 0) };

		while (queue.Count != 0)
		{
			var current = queue.Dequeue();
			visited.Add(current);
			if (current.Item1 == target.Item1 && current.Item2 == target.Item2)
			{
				break;
			}
			else
			{
				foreach ((int, int) direction in directions)
				{
					var step = (current.Item1 + direction.Item1, current.Item2 + direction.Item2);
					if (!boardManager.walls.Contains(new Vector3(step.Item1, step.Item2,0)))
					{
						if (step.Item1 >= 0 && step.Item1 < 15 && step.Item2 >= 0 && step.Item2 < 15)
						{
							var tempList = visited.Where(x => x.Item1 == step.Item1 && x.Item2 == step.Item2).ToList();
							bool isVisited = Convert.ToBoolean(tempList.Count());
							if (!isVisited)
							{
								queue.Enqueue(step);
								path.Add(new Node { Current = current, Next = step });
							}
						}
					}
				}
			}
		}
		List<(int, int)> shortest = new List<(int, int)>();
		shortest.Add(target);
		while (target != start)
		{
			foreach (var step in path)
			{
				if (step.Next == target)
				{
					target = step.Current;
					shortest.Add(step.Current);
				}
			}
		}

		return shortest;
	}

	public float ManhattanHeuristic((int,int) a, (int,int) b)
    {
		return Mathf.Abs(a.Item1 - b.Item1) + Mathf.Abs(a.Item2 - b.Item2);
    }
	public float EuclidHeuristic((int, int) a, (int, int) b)
	{
		return (float)(Math.Sqrt(Math.Pow((a.Item1 - b.Item1), 2) + Math.Pow((a.Item2 - b.Item2), 2)));
	}


}
#nullable enable
public class MyNode{
	public int Heuristic { get; set; } = 0;
	public MyNode? Parent { get; set; }
	public (int, int)? Pos_xy { get; set; }
	public int Cost { get; set; } = 0;
	public int Cost_all { get; set; } = 0;
	public List<MyNode>? Children { get; set; } = new List<MyNode>();
	public bool IsMax { get; set; }
	public float Value { get; set; }

	public MyNode(MyNode? parent = null, (int,int)? position = null, bool isMax = false, float val = 0)
    {
		Parent = parent;
		Pos_xy = position;
		IsMax = isMax;
		Value = val;
    }
}
