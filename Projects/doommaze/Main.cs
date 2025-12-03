using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node
{
	private const int WIDTH = 15;
	private const int HEIGHT = 15;

	private int[,] maze = new int[WIDTH, HEIGHT];
	private Random rnd = new Random();
	public int[,] Maze => maze;

	private int keysCollected = 0;
	private const int KEYS_TO_WIN = 3;

	private Node2D raycaster;

	// ============================
	// UI EXPORTS
	// ============================
	[Export] private Control StartPanel { get; set; }
	[Export] private Control WinPanel { get; set; }
	[Export] private Button StartButton { get; set; }
	[Export] private Button RestartButton { get; set; }
	[Export] private Button QuitButton { get; set; }
	[Export] private AudioStreamPlayer MusicPlayer { get; set; }

	// ============================
	// READY
	// ============================
	public override void _Ready()
	{
		// Connect buttons
		if (StartButton != null)
			StartButton.Pressed += OnStartButtonPressed;

		if (RestartButton != null)
			RestartButton.Pressed += OnRestartButtonPressed;

		if (QuitButton != null)
			QuitButton.Pressed += OnQuitButtonPressed;

		// Show start screen, hide win screen
		StartPanel?.Show();
		WinPanel?.Hide();

		GD.Print("Welcome! Press Start to begin.");
	}

	// ============================
	// START GAME
	// ============================
	private void StartGame()
	{
		keysCollected = 0;

		GenerateMaze();
		PlaceKeys(KEYS_TO_WIN);
		PrintMaze("MAZE:");

		if (!ClassDB.ClassExists("DoomRaycaster"))
		{
			GD.PrintErr("DoomRaycaster class not found!");
			return;
		}

		raycaster = (Node2D)ClassDB.Instantiate(new StringName("DoomRaycaster"));
		if (raycaster == null)
		{
			GD.PrintErr("Failed to instantiate DoomRaycaster!");
			return;
		}

		AddChild(raycaster);

		// Convert maze to 1D Godot array
		Godot.Collections.Array mazeArray = new Godot.Collections.Array();
		for (int y = 0; y < HEIGHT; y++)
			for (int x = 0; x < WIDTH; x++)
				mazeArray.Add(maze[x, y]);

		raycaster.Call("set_map", mazeArray, WIDTH, HEIGHT);

		// Connect signal
		raycaster.Connect("key_collected", new Callable(this, nameof(OnKeyCollected)));

		LoadTextures();

		GD.Print($"Game started! Collect {KEYS_TO_WIN} keys to win!");
	}

	// ============================
	// UI CALLBACKS
	// ============================
	private void OnStartButtonPressed()
	{
		StartPanel?.Hide();
		StartGame();
		
		// Start playing music on loop
		if (MusicPlayer != null)
		{
			GD.Print("=== MUSIC DEBUG ===");
			GD.Print($"MusicPlayer exists: {MusicPlayer != null}");
			GD.Print($"Stream loaded: {MusicPlayer.Stream != null}");
			GD.Print($"Stream type: {MusicPlayer.Stream?.GetType().Name}");
			GD.Print($"Volume dB: {MusicPlayer.VolumeDb}");
			GD.Print($"Bus: {MusicPlayer.Bus}");
			
			MusicPlayer.Play();
			GD.Print($"Playing: {MusicPlayer.Playing}");
			GD.Print($"Stream paused: {MusicPlayer.StreamPaused}");
			GD.Print("===================");
		}
		else
		{
			GD.PrintErr("MusicPlayer is NULL!");
		}
	}

	private void OnRestartButtonPressed()
	{
		GD.Print("Restarting game...");
		GetTree().ReloadCurrentScene();
	}

	private void OnQuitButtonPressed()
	{
		GD.Print("Quitting game...");
		GetTree().Quit();
	}

	// ============================
	// WIN LOGIC
	// ============================
	private void OnKeyCollected()
	{
		keysCollected++;
		GD.Print($"Key collected! {keysCollected}/{KEYS_TO_WIN}");

		if (keysCollected >= KEYS_TO_WIN)
			OnGameWon();
	}

	private void OnGameWon()
	{
		GD.Print("=================================");
		GD.Print("CONGRATULATIONS! YOU WIN!");
		GD.Print("=================================");

		// Stop music when game is won
		if (MusicPlayer != null)
		{
			MusicPlayer.Stop();
		}

		// Disable raycaster input and rendering
		if (raycaster != null)
		{
			raycaster.SetProcess(false);
			raycaster.SetProcessInput(false);
		}

		WinPanel?.Show();
	}

	// ============================
	// LOAD TEXTURES
	// ============================
	private void LoadTextures()
	{
		if (ResourceLoader.Exists("res://textures/wall.jpg"))
		{
			var wallTexture = Image.LoadFromFile("res://textures/wall.jpg");
			raycaster.Call("set_wall_texture", wallTexture);
		}

		if (ResourceLoader.Exists("res://textures/floor.jpg"))
		{
			var floorTexture = Image.LoadFromFile("res://textures/floor.jpg");
			raycaster.Call("set_floor_texture", floorTexture);
		}

		if (ResourceLoader.Exists("res://textures/skybox.png"))
		{
			var ceilingTexture = Image.LoadFromFile("res://textures/skybox.png");
			raycaster.Call("set_ceiling_texture", ceilingTexture);
		}

		if (ResourceLoader.Exists("res://textures/key.png"))
		{
			var keyTexture = Image.LoadFromFile("res://textures/key.png");
			raycaster.Call("set_key_texture", keyTexture);
		}
	}

	// ============================
	// MAZE GENERATION
	// ============================
	private void GenerateMaze()
	{
		for (int x = 0; x < WIDTH; x++)
			for (int y = 0; y < HEIGHT; y++)
				maze[x, y] = 1;

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

	// ============================
	// KEY PLACEMENT
	// ============================
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

	// ============================
	// BFS (kept for reference)
	// ============================
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
				if (maze[next.X, next.Y] == 1) continue;

				visited.Add(next);
				parent[next] = current;
				queue.Enqueue(next);
			}
		}

		if (!parent.ContainsKey(goal))
			return new List<Vector2I>();

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

	// ============================
	// PRINT MAZE
	// ============================
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
