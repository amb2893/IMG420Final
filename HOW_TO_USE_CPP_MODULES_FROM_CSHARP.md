# How to Use Custom C++ Modules from C# in Godot

This guide explains how to create and use custom C++ modules in Godot that are accessible from C# scripts.

## Overview

When you build Godot with mono support, any C++ classes you register using the ClassDB system automatically get C# bindings generated. This means you can use your C++ modules directly from C# code without writing any glue code.

## Step 1: Create Your C++ Module

Your C++ module needs to follow Godot's module structure. Here's what you need:

### Module Structure
```
godot/modules/your_module_name/
├── config.py
├── SCsub
├── register_types.h
├── register_types.cpp
├── your_class.h
└── your_class.cpp
```

### 1.1 Create `config.py`
```python
def can_build(env, platform):   
    return True

def configure(env):
    pass
```

### 1.2 Create Your C++ Class Header (`doom_raycaster.h` example)
```cpp
#ifndef DOOM_RAYCASTER_H
#define DOOM_RAYCASTER_H

#include "core/object/ref_counted.h"
#include "scene/2d/node_2d.h"

class DoomRaycaster : public Node2D {
    GDCLASS(DoomRaycaster, Node2D);

private:
    // Your private members
    Vector2 player_pos = Vector2(1.5, 1.5);
    float player_angle = 0.0f;

protected:
    // REQUIRED: This method binds your C++ methods to Godot
    static void _bind_methods();

public:
    DoomRaycaster();
    ~DoomRaycaster();
    
    // Your public methods
    void set_player_position(Vector2 p_pos);
    Vector2 get_player_position() const;
    void set_player_angle(float p_angle);
    float get_player_angle() const;
};

#endif
```

**Key Points:**
- Inherit from a Godot class (Node2D, RefCounted, Node, etc.)
- Use `GDCLASS(YourClass, ParentClass)` macro
- Declare `static void _bind_methods();` in protected section

### 1.3 Create Your C++ Class Implementation (`doom_raycaster.cpp` example)
```cpp
#include "doom_raycaster.h"

DoomRaycaster::DoomRaycaster() {
    // Constructor
}

DoomRaycaster::~DoomRaycaster() {
    // Destructor
}

// REQUIRED: Bind all methods you want to expose to GDScript/C#
void DoomRaycaster::_bind_methods() {
    ClassDB::bind_method(D_METHOD("set_player_position", "position"), 
                         &DoomRaycaster::set_player_position);
    ClassDB::bind_method(D_METHOD("get_player_position"), 
                         &DoomRaycaster::get_player_position);
    ClassDB::bind_method(D_METHOD("set_player_angle", "angle"), 
                         &DoomRaycaster::set_player_angle);
    ClassDB::bind_method(D_METHOD("get_player_angle"), 
                         &DoomRaycaster::get_player_angle);
}

void DoomRaycaster::set_player_position(Vector2 p_pos) {
    player_pos = p_pos;
}

Vector2 DoomRaycaster::get_player_position() const {
    return player_pos;
}

void DoomRaycaster::set_player_angle(float p_angle) {
    player_angle = p_angle;
}

float DoomRaycaster::get_player_angle() const {
    return player_angle;
}
```

**Key Points:**
- Implement `_bind_methods()` and bind all methods you want to expose
- Use `D_METHOD("method_name", "param1", "param2")` for method registration
- The first string in D_METHOD is the name as it appears in GDScript/C#

### 1.4 Create `register_types.h`
```cpp
#ifndef DOOM_RAYCASTER_MODULE_H
#define DOOM_RAYCASTER_MODULE_H

#include "modules/register_module_types.h"

void initialize_doom_raycaster_module(ModuleInitializationLevel p_level);
void uninitialize_doom_raycaster_module(ModuleInitializationLevel p_level);

#endif
```

### 1.5 Create `register_types.cpp`
```cpp
#include "register_types.h"
#include "doom_raycaster.h"
#include "core/object/class_db.h"

void initialize_doom_raycaster_module(ModuleInitializationLevel p_level) {
    if (p_level != MODULE_INITIALIZATION_LEVEL_SCENE) {
        return;
    }
    ClassDB::register_class<DoomRaycaster>();
}

void uninitialize_doom_raycaster_module(ModuleInitializationLevel p_level) {
    if (p_level != MODULE_INITIALIZATION_LEVEL_SCENE) {
        return;
    }
    // Cleanup if needed
}
```

**Key Points:**
- Function names MUST follow pattern: `initialize_<module_name>_module` and `uninitialize_<module_name>_module`
- The `<module_name>` part must match your module folder name exactly
- Use `ClassDB::register_class<YourClass>();` to register your class

## Step 2: Build Godot with Mono Support

Build Godot with your module and mono support enabled:

```powershell
# Navigate to godot source directory
cd C:\Users\Larry\Godot\GodotSourceForModule\godot

# Build for Windows with mono support (example)
scons platform=windows module_mono_enabled=yes
```

**Important Build Flags:**
- `module_mono_enabled=yes` - Enables C# support
- `target=editor` - Builds the editor (default)
- `dev_build=yes` - Development build with better error messages

## Step 3: Using Your C++ Module from C#

Once Godot is built with mono support, your C++ classes are automatically available in C#!

### 3.1 Create a C# Script in Your Godot Project

```csharp
using Godot;
using System;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        // Create an instance of your C++ class
        DoomRaycaster raycaster = new DoomRaycaster();
        
        // Set up the map
        var map = new Godot.Collections.Array
        {
            1, 1, 1, 1, 1,
            1, 0, 0, 0, 1,
            1, 0, 1, 0, 1,
            1, 0, 0, 0, 1,
            1, 1, 1, 1, 1
        };
        raycaster.SetMap(map, 5, 5);
        
        // Set player position and angle
        raycaster.SetPlayerPosition(new Vector2(2.5f, 2.5f));
        raycaster.SetPlayerAngle(0.0f);
        
        // Set rendering parameters
        raycaster.SetScreenSize(800, 600);
        raycaster.SetFov(60.0f);
        raycaster.SetRenderDistance(20.0f);
        
        // Set colors
        raycaster.SetWallColor(new Color(0.8f, 0.2f, 0.2f));
        raycaster.SetFloorColor(new Color(0.2f, 0.2f, 0.2f));
        raycaster.SetCeilingColor(new Color(0.1f, 0.1f, 0.3f));
        
        // Set movement speeds
        raycaster.SetMoveSpeed(3.0f);
        raycaster.SetRotationSpeed(2.0f);
        
        // Add to scene tree
        AddChild(raycaster);
        
        GD.Print("DoomRaycaster initialized!");
        GD.Print($"Player position: {raycaster.GetPlayerPosition()}");
        GD.Print($"Player angle: {raycaster.GetPlayerAngle()}");
    }
}
```

### 3.2 Method Name Conversion

C++ method names are automatically converted to C# naming conventions:
- `set_player_position` → `SetPlayerPosition`
- `get_player_angle` → `GetPlayerAngle`
- Snake_case becomes PascalCase

### 3.3 Type Conversions

Common Godot types work seamlessly between C++ and C#:
- `Vector2`, `Vector3` - Direct mapping
- `Color` - Direct mapping
- `Array` (C++) → `Godot.Collections.Array` (C#)
- `int`, `float`, `bool` - Direct mapping
- `String` → `string`

## Step 4: Verification

To verify your module is working:

1. **Check if the class is available:**
   - In the Godot editor, go to "Create New Node"
   - Search for your class name (e.g., "DoomRaycaster")
   - If it appears, the C++ module is registered correctly

2. **Test from C#:**
   - Create a simple C# script that instantiates your class
   - Add print statements to verify methods are working
   - Run the project

3. **Check for errors:**
   - Look at the Godot console output
   - Check for any binding or compilation errors

## Common Issues and Solutions

### Issue 1: Class not found in C#
**Solution:** Make sure you rebuilt Godot after adding your module. The C# bindings are generated at build time.

### Issue 2: Methods not available
**Solution:** Check that you called `ClassDB::bind_method()` for each method in `_bind_methods()`.

### Issue 3: Module not loading
**Solution:** Verify that your `register_types.cpp` function names match your module folder name exactly.

### Issue 4: Wrong initialization level
**Solution:** Most game classes should use `MODULE_INITIALIZATION_LEVEL_SCENE`. Core classes use `MODULE_INITIALIZATION_LEVEL_CORE`.

## Example: Summator Module

Here's a simpler example that's easier to understand:

### C++ Header (summator.h)
```cpp
#ifndef SUMMATOR_H
#define SUMMATOR_H

#include "core/object/ref_counted.h"

class Summator : public RefCounted {
    GDCLASS(Summator, RefCounted);
    
    int count;

protected:
    static void _bind_methods();

public:
    void add(int p_value);
    void reset();
    int get_total() const;
    
    Summator();
};

#endif
```

### C++ Implementation (summator.cpp)
```cpp
#include "summator.h"

void Summator::add(int p_value) {
    count += p_value;
}

void Summator::reset() {
    count = 0;
}

int Summator::get_total() const {
    return count;
}

void Summator::_bind_methods() {
    ClassDB::bind_method(D_METHOD("add", "value"), &Summator::add);
    ClassDB::bind_method(D_METHOD("reset"), &Summator::reset);
    ClassDB::bind_method(D_METHOD("get_total"), &Summator::get_total);
}

Summator::Summator() {
    count = 0;
}
```

### C# Usage
```csharp
using Godot;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        Summator s = new Summator();
        s.Add(10);
        s.Add(20);
        s.Add(30);
        GD.Print(s.GetTotal()); // Prints: 60
    }
}
```

## Quick Reference Checklist

When creating a new C++ module for C# use:

- [ ] Create module folder in `godot/modules/`
- [ ] Add `config.py` with `can_build()` and `configure()`
- [ ] Inherit from a Godot class (Node, RefCounted, etc.)
- [ ] Use `GDCLASS(YourClass, ParentClass)` macro
- [ ] Declare `static void _bind_methods();` in protected section
- [ ] Implement `_bind_methods()` and bind all public methods
- [ ] Create `register_types.h` and `register_types.cpp`
- [ ] Use correct naming: `initialize_<module_name>_module`
- [ ] Call `ClassDB::register_class<YourClass>();`
- [ ] Build Godot with `module_mono_enabled=yes`
- [ ] Test in C# by instantiating your class

## Additional Resources

- Godot Documentation: https://docs.godotengine.org/en/stable/contributing/development/core_and_modules/custom_modules_in_cpp.html
- ClassDB Reference: Check Godot source code in `core/object/class_db.h`
- Example Modules: Look at existing modules in `godot/modules/` folder

---

**Remember:** Every time you change your C++ code, you need to rebuild Godot for the changes to appear in C#!
