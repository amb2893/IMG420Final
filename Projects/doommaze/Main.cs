using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node
{
	private const int WIDTH = 10;
	private const int HEIGHT = 10;

	private int[,] maze = new int[WIDTH, HEIGHT];
	private Random rnd = new Random();
	public int[,] Maze => maze;

	private int keysCollected = 0;
	private const int KEYS_TO_WIN = 3;
	
	private Node2D raycaster;

	public override void _Ready()
	{
		GenerateMaze();
		PlaceKeys(KEYS_TO_WIN);
		PrintMaze("MAZE:");
		
		// Check if DoomRaycaster class exists
		if (!ClassDB.ClassExists("DoomRaycaster"))
		{
			GD.PrintErr("DoomRaycaster class not found!");
			return;
		}

		// Instantiate DoomRaycaster
		raycaster = (Node2D)ClassDB.Instantiate(new StringName("DoomRaycaster"));
		if (raycaster == null)
		{
			GD.PrintErr("Failed to instantiate DoomRaycaster!");
			return;
		}

		AddChild(raycaster);

		// Convert 2D maze to 1D array for Godot
		Godot.Collections.Array mazeArray = new Godot.Collections.Array();
		for(int y = 0; y < HEIGHT; y++)
		{
			for(int x = 0; x < WIDTH; x++)
			{
				mazeArray.Add(maze[x, y]);
			}
		}
		
		// Set up the raycaster (only non-default values)
		raycaster.Call("set_map", mazeArray, WIDTH, HEIGHT);
		
		// Optional: Override defaults if needed
		// raycaster.Call("set_screen_size", 800, 600); // Already default
		// raycaster.Call("set_fov", 60.0f); // Already default
		// raycaster.Call("set_player_position", new Vector2(1.5f, 1.5f)); // Already default
		// raycaster.Call("set_render_distance", 25.0f);
		// raycaster.Call("set_move_speed", 4.0f);
		// raycaster.Call("set_rotation_speed", 2.5f);
		
		// Connect to key_collected signal
		raycaster.Connect("key_collected", new Callable(this, nameof(OnKeyCollected)));
		
		// Optional: Load and set textures
		LoadTextures();
		
		GD.Print($"Game started! Collect {KEYS_TO_WIN} keys to win!");
	}

	private void OnKeyCollected()
	{
		keysCollected++;
		GD.Print($"Key collected! {keysCollected}/{KEYS_TO_WIN}");
		
		if(keysCollected >= KEYS_TO_WIN)
		{
			OnGameWon();
		}
	}

	private void OnGameWon()
	{
		GD.Print("=================================");
		GD.Print("CONGRATULATIONS! YOU WIN!");
		GD.Print($"All {KEYS_TO_WIN} keys collected!");
		GD.Print("=================================");
		
		// Optional: Add game over logic here
		// GetTree().Quit();
		// GetTree().ChangeSceneToFile("res://victory_screen.tscn");
	}

	private void LoadTextures()
	{
		// Example: Load textures from files
		// Replace these paths with your actual texture files
		
		// Wall texture
		if(ResourceLoader.Exists("res://textures/wall.jpg"))
		{
			var wallTexture = Image.LoadFromFile("res://textures/wall.jpg");
			raycaster.Call("set_wall_texture", wallTexture);
		}
		
		// Floor texture
		//if(ResourceLoader.Exists("res://textures/floor.jpg"))
		//{
			//var floorTexture = Image.LoadFromFile("res://textures/floor.jpg");
			//raycaster.Call("set_floor_texture", floorTexture);
		//}
		
		// Ceiling texture
		//if(ResourceLoader.Exists("res://textures/skybox.png"))
		//{
			//var ceilingTexture = Image.LoadFromFile("res://textures/skybox.png");
			//raycaster.Call("set_ceiling_texture", ceilingTexture);
		//}
		
		// Key texture (billboard)
		//if(ResourceLoader.Exists("res://textures/key.png"))
		//{
			//var keyTexture = Image.LoadFromFile("res://textures/key.png");
			//raycaster.Call("set_key_texture", keyTexture);
		//}
		
		// If textures don't exist, colors will be used as fallback
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
	// BFS PATHFINDING (Optional - kept for reference)
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
	// PRINT KEY PATHS (Optional)
	// =============================================================
	private void PrintKeyPaths()
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
