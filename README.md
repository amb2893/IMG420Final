# IMG420Final

# Escape  
## Team Members  
- **Anthony Birk**  
- **Larry Griffith**  
- **Colton Leighton**

## Project Overview  
Escape is a 2.5D retro-style first-person maze exploration game inspired by the visual and movement style of classic shooters such as DOOM (1993). The player navigates a procedurally generated maze, collects three hidden keys, and escapes before becoming lost or disoriented. Each playthrough presents a new layout, supporting replayability and consistent challenge.

The project incorporates a custom C++ Godot module that manages maze generation, data structures, and environment construction. This module integrates with Godot Mono and communicates seamlessly with C# scripts, enabling efficient 2.5D rendering and flexible level creation.

## Module Functionality  
The Maze module operates in two primary phases:

### 1. Matrix Generation  
Generates a `Vector<Vector<int>>` representing the maze layout. Each numerical cell corresponds to a defined element such as empty space, wall, key, or exit.

### 2. Environment Construction  
The matrix is passed to the environment builder, which iterates through the grid and constructs the playable 2.5D world using raycast-style rendering. The system leverages Godot’s node structure, allowing easy replacement of walls, floors, keys, and exits through standard scene resources.

### Supporting Architecture  
- **MazeConfig**: Stores parameters for maze generation.  
- **MazeMatrix**: Holds the grid of integer cell values.  
- **MazeGenerator**: Abstract algorithm interface selected through a factory.  
- **MazeEnvironmentBuilder**: Converts the matrix into a 2.5D environment.  
- **MazeManager (Singleton)**: Exposes simple generation and construction methods to C# and GDScript.

## Features  
- Procedurally generated mazes with unique layouts  
- 2.5D raycast-style rendering inspired by early first-person games  
- Collectible key system with a minimal HUD  
- Configurable maze parameters through the native module  
- Integration with Godot’s node and scene system  
- Modular architecture supporting rapid iteration


## Installation and Build Instructions  

### Requirements  
- Godot 4 with Mono support  
- Compatible C++ build environment (GDExtension)  
- Git  

### Building the Native Module  
1. Clone the repository.  
2. Build the custom C++ module using Godot’s native extension tools.  
3. Place the compiled library in the project's native extensions directory.  
4. Open the project using Godot with Mono enabled.  
5. Allow the C# project to build so scripts can call into the module.



## Running the Game Prototype  
1. Open the project in Godot.  
2. Verify that the native extension loads correctly.  
3. Press **Play** to launch the prototype.  
4. A new maze will be generated at runtime, starting the player in first-person view.

## Controls and Gameplay Instructions  
**Movement:** Arrow Keys  or WASD

**Goal:**  
- Explore the maze  
- Collect all three keys  
- Find and escape through the exit  

The minimal HUD keeps the player's view unobstructed, supporting navigation and spatial reasoning.

## Known Issues  
- Slight perspective distortion may occur in narrow spaces.  
- Very large maze sizes may cause performance reduction depending on hardware.

## Future Improvements  
- Enemy character with AI behavior  
- Additional visual themes and environmental styles  
- Improved sound design and ambience  
- Optional minimap or navigational assists  
- Difficulty scaling based on maze size and layout

## Credits and Acknowledgments  
- NAU’s IMG 420 course and Professor Ashish Amresh  
- Official Godot Documentation  
- Visual and structural inspiration from DOOM (1993)

## Demo Video  

