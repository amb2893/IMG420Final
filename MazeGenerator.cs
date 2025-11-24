using Godot;
using System;
using System.Collections.Generic;

public partial class MazeGenerator : Node
{
	private const int WIDTH = 40;
	private const int HEIGHT = 40;

	private int[,] maze = new int[WIDTH, HEIGHT];
	private Random rnd = new Random();
	public int[,] Maze => maze;

	public override void _Ready()
	{
		GenerateMaze();
		PlaceKeys(3);
		PrintMaze("MAZE:");
		//PrintKeyPaths();
	}

	// =============================================================
	// MAZE GENERATION (Recursive Backtracking)
	// =============================================================
	private void GenerateMaze()
	{
		// Fill with walls
		for (int x = 0; x < WIDTH; x++)
			for (int y = 0; y < HEIGHT; y++)
				maze[x, y] = 1;

		// Start carving
		Carve(1, 1);
	}

	private void Carve(int x, int y)
	{
		maze[x, y] = 0;

		var dirs = new List<Vector2I>
		{
			new Vector2I(1, 0),
			new Vector2I(-1, 0),
			new Vector2I(0, 1),
			new Vector2I(0, -1)
		};

		// Shuffle directions
		for (int i = 0; i < dirs.Count; i++)
		{
			int idx = rnd.Next(dirs.Count);
			(dirs[i], dirs[idx]) = (dirs[idx], dirs[i]);
		}

		foreach (var d in dirs)
		{
			int nx = x + d.X * 2;
			int ny = y + d.Y * 2;

			if (IsInside(nx, ny) && maze[nx, ny] == 1)
			{
				maze[x + d.X, y + d.Y] = 0;
				Carve(nx, ny);
			}
		}
	}

	private bool IsInside(int x, int y)
	{
		return x > 0 && y > 0 && x < WIDTH - 1 && y < HEIGHT - 1;
	}

	// =============================================================
	// KEY PLACEMENT
	// =============================================================
	private void PlaceKeys(int count)
	{
		int placed = 0;

		while (placed < count)
		{
			int x = rnd.Next(1, WIDTH - 1);
			int y = rnd.Next(1, HEIGHT - 1);

			if (maze[x, y] == 0)
			{
				maze[x, y] = 2;
				placed++;
			}
		}
	}

	// =============================================================
	// BFS PATHFINDING
	// =============================================================
	private List<Vector2I> BFS(Vector2I start, Vector2I goal)
	{
		Queue<Vector2I> queue = new Queue<Vector2I>();
		queue.Enqueue(start);

		Dictionary<Vector2I, Vector2I> parent = new Dictionary<Vector2I, Vector2I>();
		HashSet<Vector2I> visited = new HashSet<Vector2I>();
		visited.Add(start);

		Vector2I[] dirs =
		{
			new Vector2I(1,0),
			new Vector2I(-1,0),
			new Vector2I(0,1),
			new Vector2I(0,-1)
		};

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();

			if (current == goal)
				break;

			foreach (var d in dirs)
			{
				Vector2I next = current + d;

				if (!IsInside(next.X, next.Y)) continue;
				if (visited.Contains(next)) continue;
				if (maze[next.X, next.Y] == 1) continue; // wall

				visited.Add(next);
				parent[next] = current;
				queue.Enqueue(next);
			}
		}

		// If no path, return empty list
		if (!parent.ContainsKey(goal))
			return new List<Vector2I>();

		// Reconstruct path
		List<Vector2I> path = new List<Vector2I>();
		Vector2I p = goal;

		while (parent.ContainsKey(p))
		{
			path.Add(p);
			p = parent[p];
		}

		path.Add(start);
		path.Reverse();
		return path;
	}

	// =============================================================
	// PRINT KEY PATHS ONLY
	// =============================================================
	/* private void PrintKeyPaths()
	{
		Vector2I start = new Vector2I(1, 1);

		List<Vector2I> keys = new List<Vector2I>();

		// Find all keys on maze
		for (int x = 0; x < WIDTH; x++)
			for (int y = 0; y < HEIGHT; y++)
				if (maze[x, y] == 2)
					keys.Add(new Vector2I(x, y));

		GD.Print("\nKEY PATHS:");

		foreach (var key in keys)
		{
			GD.Print($"Path to key at {key}:");

			var path = BFS(start, key);

			if (path.Count == 0)
			{
				GD.Print("  NO PATH FOUND!\n");
				continue;
			}

			foreach (var step in path)
				GD.Print($"  {step}");

			GD.Print(""); // blank line
		}
	}
*/
	// =============================================================
	// PRINT MAZE
	// =============================================================
	private void PrintMaze(string title)
	{
		GD.Print(title);
		for (int y = 0; y < HEIGHT; y++)
		{
			string row = "";
			for (int x = 0; x < WIDTH; x++)
				row += maze[x, y].ToString();
			GD.Print(row);
		}
	}
}
