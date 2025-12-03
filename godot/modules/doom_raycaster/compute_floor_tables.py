import math

SCREEN_WIDTH = 1152
SCREEN_HEIGHT = 648
FOV = 60.0

# ---- Precompute constants ----
fov_rad = math.radians(FOV)
tan_half = math.tan(fov_rad / 2.0)
view_plane_dist = (SCREEN_WIDTH * 0.5) / tan_half

# ---- A) RowDistance table ----
print("static const float ROW_DIST[SCREEN_HEIGHT] = {")
for y in range(SCREEN_HEIGHT):
    p = y - (SCREEN_HEIGHT / 2)
    if p == 0:
        print("0.0f,", end="")
    else:
        row_dist = view_plane_dist / p
        print(f"{row_dist:.9f}f,", end="")
print("};\n")

# ---- B) Floor ray direction for each column ----
# Compute edge directions
dir_left = (-tan_half, 1.0)
dir_right = (tan_half, 1.0)

# Normalize both
len_l = math.sqrt(dir_left[0]**2 + dir_left[1]**2)
len_r = math.sqrt(dir_right[0]**2 + dir_right[1]**2)
dir_left = (dir_left[0]/len_l, dir_left[1]/len_l)
dir_right = (dir_right[0]/len_r, dir_right[1]/len_r)

print("static const float FLOOR_RAY_X[SCREEN_WIDTH] = {")
for x in range(SCREEN_WIDTH):
    t = x / SCREEN_WIDTH
    fx = dir_left[0] + (dir_right[0] - dir_left[0]) * t
    fy = dir_left[1] + (dir_right[1] - dir_left[1]) * t
    length = math.sqrt(fx*fx + fy*fy)
    print(f"{fx/length:.9f}f,", end="")
print("};\n")

print("static const float FLOOR_RAY_Y[SCREEN_WIDTH] = {")
for x in range(SCREEN_WIDTH):
    t = x / SCREEN_WIDTH
    fx = dir_left[0] + (dir_right[0] - dir_left[0]) * t
    fy = dir_left[1] + (dir_right[1] - dir_left[1]) * t
    length = math.sqrt(fx*fx + fy*fy)
    print(f"{fy/length:.9f}f,", end="")
print("};")
