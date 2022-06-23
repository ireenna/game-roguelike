using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    public int columns = 15;
	public int rows = 15;
	public Count foodCount = new Count(5, 8);
	public Count wallCount = new Count(1, 2);
	public GameObject exit;
	public GameObject[] enemyTiles;
	public GameObject[] floorTiles;
	public GameObject[] foodTiles;
	public GameObject[] outerWallTiles;
	public GameObject[] wallTiles;
	public List<Vector3> walls;
	public int[,] grid;
	public List<Vector3> meal;

	private Transform boardHolder;
	private List<Vector3> gridPositions = new List<Vector3>();

    public BoardManager()
    {

    }

    public BoardManager(int count)
    {
        columns = count;
    }

	void InitializeList()
	{
		gridPositions.Clear();

		for (int x = 1; x < columns - 1; x++)
		{
			for (int y = 1; y < rows - 1; y++)
			{
				gridPositions.Add(new Vector3(x, y, 0f));
			}
		}
	}

	void BoardSetup()
	{
		boardHolder = new GameObject("Board").transform;

		for (int x = -1; x < columns + 1; x++)
		{
			for (int y = -1; y < rows + 1; y++)
			{
				GameObject toInstantiate;

				if (x == -1 || x == columns || y == -1 || y == rows)
				{
					toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
				}
				else
				{
					toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
				}

				GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

				instance.transform.SetParent(boardHolder);
			}
		}
	}

	public int[,] VectorToGrid(List<Vector3> vectList)
	{
		int[,] grid = new int[columns, rows];
		foreach (Vector3 vect in vectList)
		{
			grid[Convert.ToInt32(vect.x), Convert.ToInt32(vect.y)] = 1;
		}
		return grid;
	}

	Vector3 RandomPosition()
	{
		int randomIndex = Random.Range(0, gridPositions.Count);
		Vector3 randomPosition = gridPositions[randomIndex];
		gridPositions.RemoveAt(randomIndex);

		return randomPosition;
	}

	void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
	{
		int objectCount = Random.Range(minimum, maximum + 1);
		for (int i = 0; i < objectCount; i++)
		{
			Vector3 randomPosition = RandomPosition();
			while (walls.Contains(randomPosition))
			{
				randomPosition = RandomPosition();
				
			}
			meal.Add(randomPosition);
			GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

			Instantiate(tileChoice, randomPosition, Quaternion.identity);
		}
	}

	void LayoutWallWithoutRandom(GameObject[] tileArray, int minimum, int maximum)
	{

		walls = new List<Vector3>();
		int[,] Maze = MazeBuilder.GenerateMaze(columns + 2, rows + 2);
		Maze[14, 14] = Maze[13, 13] = Maze[13, 14] = Maze[14, 13] = 0;
		for (int i = 0; i < columns; i++)
		{
			for (int j = 0; j < rows; j++)
			{
				bool isWall;
				isWall = Maze[i + 1, j + 1] == 1;
				if (isWall)
				{
					Vector3 vector3 = new Vector3(i, j);
					GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
					walls.Add(vector3);
					Instantiate(tileChoice, vector3, Quaternion.identity);
				}

			}
		}
		grid = VectorToGrid(walls);

	}
    void LayoutWallWithRandom(GameObject[] tileArray, int minimum, int maximum)
    {

        walls = new List<Vector3>();
		var countOfWalls = Random.Range(40,50);
        for (int i = 0; i < countOfWalls; i++)
        {
                    Vector3 vector3 = new Vector3(Random.Range(2, columns), Random.Range(2, rows));
                    var badVectors = new List<Vector3>()
                    {
                        new Vector3(columns - 1, rows - 1), new Vector3(columns - 2, rows - 2),
                        new Vector3(columns - 1, rows - 2), new Vector3(columns - 2, rows - 1),
                        new Vector3(columns - 1, rows - 3), new Vector3(columns - 3, rows - 1),
                        new Vector3(columns - 2, rows - 3), new Vector3(columns - 3, rows - 2),
                        new Vector3(columns - 3, rows - 3), new Vector3(0, rows - 1),
                        new Vector3(0, rows), new Vector3(1, rows), new Vector3(2, rows), new Vector3(1, rows-1),
                        new Vector3(1, rows+1), new Vector3(1, rows+1)
					};
                    while (walls.Contains(vector3)||meal.Contains(vector3)||badVectors.Contains(vector3))
                    {
				        vector3 = new Vector3(Random.Range(2, columns), Random.Range(2, rows));
			        }
                    GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
                    walls.Add(vector3);
                    Instantiate(tileChoice, vector3, Quaternion.identity);
                    
        }
        grid = VectorToGrid(walls);

    }

	public void SetupScene(int level)
	{
		BoardSetup();
		InitializeList();
        LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
		int enemyCount = (int)Mathf.Log(level, 2f);

		if (enemyCount == 0)
		{
			enemyCount = 1;
		}
		LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
        LayoutWallWithRandom(wallTiles, wallCount.minimum, wallCount.maximum);
        


		Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
	}
    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }
}
