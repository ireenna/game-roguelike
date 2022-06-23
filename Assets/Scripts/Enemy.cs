using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
public class Enemy : MovingObject
{
	public int playerDamage;
	public AudioClip enemyAttack1;
	public AudioClip enemyAttack2;
	public GameManager gameManager;

	private BoardManager boardManager;
	private Animator animator;
	//private Transform transform;
	private Player target;
	private bool skipMove;
    public int life;
    public int x;
    public int y;

	protected override void Start()
	{
		GameManager.instance.AddEnemyToList(this);

		animator = GetComponent<Animator>();
        target = MovingObject.FindObjectOfType<Player>();
        boardManager = MonoBehaviour.FindObjectOfType<BoardManager>();
		gameManager = MonoBehaviour.FindObjectOfType<GameManager>();
		life = 5;
        x = (int)transform.position.x;
        y = (int)transform.position.y;
        base.Start();
	}

	protected override void AttemptMove <T> (int xDir, int yDir)
	{
		if(gameObject != null)
        {
			base.AttemptMove<Player>(xDir, yDir);
			RaycastHit2D hit;

			if (Move(xDir, yDir, out hit))
			{
				x = x + xDir;
				y = y + yDir;
				//var isInside = new Vector3(x, y, 0);
				//if (boardManager.meal.Contains(isInside))
				//{
				//	boardManager.meal.Remove(isInside);
				//}
				//SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
			}
			//x = x + xDir;
			//y = y + yDir;

			//skipMove = true;
        }
		
	}

	protected override void OnCantMove <T> (T component)
	{
		animator.SetTrigger("enemyAttack");

		SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);
		
		target.LoseFood(playerDamage);
        
		life -= 1;
		if(life <= 0)
        {
			return;
        }
	}

	public void MoveEnemy()
	{
		if (gameObject != null)
		{
			int xDir = 0;
			int yDir = 0;

			var list = boardManager.walls;
			var grid = boardManager.grid;
            int isTrue = Random.Range(0, 2);
            if (Convert.ToBoolean(isTrue))
            {

			//if (Mathf.Abs(x - target.x) < 10 || Mathf.Abs(y - target.y) < 10)
			//{
				if ((x,y) != (target.x,target.y))
				{
					var BFSpath = GetBFSPath((x, y), (target.x, target.y), grid);
					if (BFSpath.Count() > 1)
					{
						var BFSstep = BFSpath[BFSpath.Count() - 2];
						xDir = -x + BFSstep.Item1;
						yDir = -y + BFSstep.Item2;

						if (!list.Contains(new Vector3((float)BFSstep.Item1, (float)BFSstep.Item2, 0f)))
						{
							AttemptMove<Player>(xDir, yDir);
						}
					}
					else
					{
						xDir = 0;
						yDir = 0;

					var BFSstep = BFSpath[BFSpath.Count() - 1];
					xDir = -x + BFSstep.Item1;
					yDir = -y + BFSstep.Item2;
					if (!list.Contains(new Vector3((float)BFSstep.Item1, (float)BFSstep.Item2, 0f)))
					{
						AttemptMove<Player>(xDir, yDir);
					}
					//if (Mathf.Abs(target.x - x) <= 0)
					//{
					//	yDir = (target.y > y) ? 1 : -1;
					//}
					//else
					//{
					//	xDir = (target.x > x) ? 1 : -1;
					//}

					AttemptMove<Player>(xDir, yDir);
					}
				}
				else
				{
					AttemptMove<Player>(xDir, yDir);

				//if (Mathf.Abs(target.x - x) <=0)
				//{
				//	yDir = (target.y > y) ? 1 : -1;
				//}
				//else
				//{
				//	xDir = (target.x > x) ? 1 : -1;
				//}
				//               if (gameObject != null)
				//               {
				//	AttemptMove<Player>(xDir, yDir);
				//               }
			}
            }
			//}
        }
	}
	
	public List<(int,int)> GetBFSPath((int,int) start, (int,int) target, int[,] grid)
    {
		Queue<(int, int)> queue = new Queue<(int, int)>();
		queue.Enqueue(start);
		List<Node> path = new List<Node>();
		List<(int, int)> visited = new List<(int, int)>();
		(int,int)[] directions = { (0,-1), (1,0), (0,1), (-1,0) };

		while(queue.Count != 0)
        {
			var current = queue.Dequeue();
			visited.Add(current);
			if (current.Item1 == target.Item1 && current.Item2 == target.Item2)
            {
				break;
            }
            else
            {
                foreach((int,int) direction in directions)
                {
					var step = (current.Item1 + direction.Item1, current.Item2 + direction.Item2);
					if(step.Item1 >= 0 && step.Item2 >= 0 && step.Item1<boardManager.columns && step.Item2<boardManager.rows){
						if (grid[step.Item1, step.Item2] != 1)
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
				if(step.Next == target)
                {
					target = step.Current;
					shortest.Add(step.Current);
                }
            }
        }

		return shortest;
	}
}
public class Node
{
    public (int,int) Current { get; set; }
    public (int,int) Next { get; set; }
}
