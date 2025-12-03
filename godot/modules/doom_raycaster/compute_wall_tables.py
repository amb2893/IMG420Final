import math

SCREEN_WIDTH = 1152
FOV = 60.0  # match your DoomRaycaster fov

tan_half = math.tan(math.radians(FOV) / 2)

print(f"static const int RENDER_WIDTH = {SCREEN_WIDTH};\n")

print("static const float RAY_COS[RENDER_WIDTH] = {")
for x in range(SCREEN_WIDTH):
    screen_space = (2.0 * x / SCREEN_WIDTH) - 1.0
    angle = math.atan(screen_space * tan_half)
    print(f"{math.cos(angle):.9f}f,", end="")
print("};\n")

print("static const float RAY_SIN[RENDER_WIDTH] = {")
for x in range(SCREEN_WIDTH):
    screen_space = (2.0 * x / SCREEN_WIDTH) - 1.0
    angle = math.atan(screen_space * tan_half)
    print(f"{math.sin(angle):.9f}f,", end="")
print("};")

print("static const float RAY_ANGLE[RENDER_WIDTH] = {")
for x in range(SCREEN_WIDTH):
    screen_space = (2.0 * x / SCREEN_WIDTH) - 1.0
    angle = math.atan(screen_space * tan_half)
    print(f"{angle:.9f}f,", end="")
print("};")

