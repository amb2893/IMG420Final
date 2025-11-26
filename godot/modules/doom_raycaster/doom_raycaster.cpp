#include "doom_raycaster.h"
#include "core/os/keyboard.h"
#include "core/math/math_funcs.h"

DoomRaycaster::DoomRaycaster(){
    render_image.instantiate();
    render_texture.instantiate();
}

DoomRaycaster::~DoomRaycaster(){}

void DoomRaycaster::_bind_methods(){
    ClassDB::bind_method(D_METHOD("set_map", "map", "width", "height"), &DoomRaycaster::set_map);
    ClassDB::bind_method(D_METHOD("set_player_position", "position"), &DoomRaycaster::set_player_position);
    ClassDB::bind_method(D_METHOD("get_player_position"), &DoomRaycaster::get_player_position);
    ClassDB::bind_method(D_METHOD("set_player_angle", "angle"), &DoomRaycaster::set_player_angle);
    ClassDB::bind_method(D_METHOD("get_player_angle"), &DoomRaycaster::get_player_angle);
    ClassDB::bind_method(D_METHOD("set_screen_size", "width", "height"), &DoomRaycaster::set_screen_size);
    ClassDB::bind_method(D_METHOD("set_fov", "fov"), &DoomRaycaster::set_fov);
    ClassDB::bind_method(D_METHOD("set_render_distance", "distance"), &DoomRaycaster::set_render_distance);
    ClassDB::bind_method(D_METHOD("set_wall_color", "color"), &DoomRaycaster::set_wall_color);
    ClassDB::bind_method(D_METHOD("set_floor_color", "color"), &DoomRaycaster::set_floor_color);
    ClassDB::bind_method(D_METHOD("set_ceiling_color", "color"), &DoomRaycaster::set_ceiling_color);
    ClassDB::bind_method(D_METHOD("set_wall_texture", "texture"), &DoomRaycaster::set_wall_texture);
    ClassDB::bind_method(D_METHOD("set_floor_texture", "texture"), &DoomRaycaster::set_floor_texture);
    ClassDB::bind_method(D_METHOD("set_ceiling_texture", "texture"), &DoomRaycaster::set_ceiling_texture);
    ClassDB::bind_method(D_METHOD("set_key_texture", "texture"), &DoomRaycaster::set_key_texture);
    ClassDB::bind_method(D_METHOD("clear_wall_texture"), &DoomRaycaster::clear_wall_texture);
    ClassDB::bind_method(D_METHOD("clear_floor_texture"), &DoomRaycaster::clear_floor_texture);
    ClassDB::bind_method(D_METHOD("clear_ceiling_texture"), &DoomRaycaster::clear_ceiling_texture);
    ClassDB::bind_method(D_METHOD("clear_key_texture"), &DoomRaycaster::clear_key_texture);
    ClassDB::bind_method(D_METHOD("set_move_speed", "speed"), &DoomRaycaster::set_move_speed);
    ClassDB::bind_method(D_METHOD("set_rotation_speed", "speed"), &DoomRaycaster::set_rotation_speed);
    
    ADD_SIGNAL(MethodInfo("key_collected"));
}

void DoomRaycaster::_notification(int p_what) {
    switch (p_what) {
        case NOTIFICATION_READY: {
            // Initialize render image
            render_image->initialize_data(screen_width, screen_height, false, Image::FORMAT_RGB8);
            render_texture->set_image(render_image);
            set_process(true);
            print_line("DoomRaycaster: Ready - Image size: " + itos(screen_width) + "x" + itos(screen_height));
        } break;
        
        case NOTIFICATION_PROCESS: {
            double delta = get_process_delta_time();
            Input *input = Input::get_singleton();
            
            // Rotation
            if (input->is_key_pressed(Key::LEFT) || input->is_key_pressed(Key::A)){
                player_angle -= rotation_speed * delta;
            }
            if (input->is_key_pressed(Key::RIGHT) || input->is_key_pressed(Key::D)){
                player_angle += rotation_speed * delta;
            }
            
            // Movement
            Vector2 move_dir(Math::cos(player_angle), Math::sin(player_angle));
            Vector2 strafe_dir(-Math::sin(player_angle), Math::cos(player_angle));
            
            Vector2 new_pos = player_pos;
            
            if (input->is_key_pressed(Key::UP) || input->is_key_pressed(Key::W)){
                new_pos += move_dir * move_speed * delta;
            }
            if (input->is_key_pressed(Key::DOWN) || input->is_key_pressed(Key::S)){
                new_pos -= move_dir * move_speed * delta;
            }
            if (input->is_key_pressed(Key::Q)){
                new_pos -= strafe_dir * move_speed * delta;
            }
            if (input->is_key_pressed(Key::E)){
                new_pos += strafe_dir * move_speed * delta;
            }
            
            // Collision detection
            int map_x = (int)new_pos.x;
            int map_y = (int)new_pos.y;
            if (get_map_value(map_x, map_y) == 0){
                player_pos = new_pos;
            }
            
            // Key collection
            map_x = (int)player_pos.x;
            map_y = (int)player_pos.y;
            if (get_map_value(map_x, map_y) == 2){
                Vector2 key_pos(map_x, map_y);
                bool already_collected = false;
                
                for(int i = 0; i < collected_keys.size(); i++){
                    if(collected_keys[i].distance_to(key_pos) < 0.1f){
                        already_collected = true;
                        break;
                    }
                }
                
                if(!already_collected){
                    collected_keys.push_back(key_pos);
                    emit_signal("key_collected");
                    print_line("DoomRaycaster: Key collected at (" + itos(map_x) + ", " + itos(map_y) + ")");
                }
            }
            
            raycast_and_render();
            queue_redraw();
        } break;
        
        case NOTIFICATION_DRAW: {
            if (render_texture.is_valid()){
                draw_texture(render_texture, Vector2(0, 0));
            }
        } break;
    }
}

Color DoomRaycaster::sample_texture(Ref<Image> texture, float u, float v){
    if(!texture.is_valid() || texture->is_empty()){
        return Color(1, 1, 1);
    }
    
    // Wrap UV coordinates
    u = u - Math::floor(u);
    v = v - Math::floor(v);
    
    int tex_width = texture->get_width();
    int tex_height = texture->get_height();
    
    int tex_x = (int)(u * tex_width) % tex_width;
    int tex_y = (int)(v * tex_height) % tex_height;
    
    if(tex_x < 0) tex_x += tex_width;
    if(tex_y < 0) tex_y += tex_height;
    
    return texture->get_pixel(tex_x, tex_y);
}

void DoomRaycaster::render_billboard(int screen_x, float distance, Vector2 billboard_pos, Ref<Image> texture){
    if(!texture.is_valid() || texture->is_empty()){
        return;
    }
    
    if(distance < 0.1f) return;
    
    // Calculate billboard height
    float billboard_height = screen_height / distance;
    int draw_start_y = MAX(0, (screen_height - billboard_height) / 2);
    int draw_end_y = MIN(screen_height - 1, (screen_height + billboard_height) / 2);
    
    // Billboard width (same as height for square billboards)
    float billboard_width = billboard_height;
    
    // Calculate screen space position
    Vector2 to_billboard = billboard_pos - player_pos;
    float angle_to_billboard = Math::atan2(to_billboard.y, to_billboard.x);
    float angle_diff = angle_to_billboard - player_angle;
    
    // Normalize angle
    while(angle_diff > Math_PI) angle_diff -= Math_TAU;
    while(angle_diff < -Math_PI) angle_diff += Math_TAU;
    
    float fov_rad = Math::deg_to_rad(fov);
    float screen_x_f = (angle_diff / fov_rad + 0.5f) * screen_width;
    int billboard_screen_x = (int)screen_x_f;
    
    // Calculate billboard width in screen space
    int half_width = (int)(billboard_width / 2);
    int start_x = billboard_screen_x - half_width;
    int end_x = billboard_screen_x + half_width;
    
    // Draw the billboard
    for(int x = start_x; x <= end_x; x++){
        if(x < 0 || x >= screen_width) continue;
        
        // Calculate texture U coordinate
        float u = (float)(x - start_x) / (float)(end_x - start_x);
        
        for(int y = draw_start_y; y <= draw_end_y; y++){
            // Calculate texture V coordinate
            float v = (float)(y - draw_start_y) / (float)(draw_end_y - draw_start_y);
            
            Color pixel = sample_texture(texture, u, v);
            
            // Simple alpha test (assuming black is transparent, or check alpha channel)
            if(pixel.a > 0.5f){
                // Apply distance fog
                float fog = 1.0f - MIN(distance / render_distance, 1.0f) * 0.6f;
                pixel = pixel * fog;
                render_image->set_pixel(x, y, pixel);
            }
        }
    }
}

void DoomRaycaster::raycast_and_render() {
    if (map_data.size() == 0 || !render_image.is_valid()){
        return;
    }
    
    float fov_rad = Math::deg_to_rad(fov);
    float angle_step = fov_rad / screen_width;
    
    // Store depth buffer for billboard rendering
    Vector<float> depth_buffer;
    depth_buffer.resize(screen_width);
    
    for (int x = 0; x < screen_width; x++){
        float ray_angle = player_angle - fov_rad / 2.0f + x * angle_step;
        Vector2 ray_dir(Math::cos(ray_angle), Math::sin(ray_angle));
        
        // DDA algorithm
        bool hit = false;
        int side = 0;
        
        Vector2 ray_pos = player_pos;
        
        // Avoid division by zero
        Vector2 delta_dist(
            (ray_dir.x == 0) ? 1e30 : Math::abs(1.0f / ray_dir.x),
            (ray_dir.y == 0) ? 1e30 : Math::abs(1.0f / ray_dir.y)
        );
        
        Vector2 step;
        Vector2 side_dist;
        
        int map_x = (int)ray_pos.x;
        int map_y = (int)ray_pos.y;
        
        if (ray_dir.x < 0){
            step.x = -1;
            side_dist.x = (ray_pos.x - map_x) * delta_dist.x;
        }
        else{
            step.x = 1;
            side_dist.x = (map_x + 1.0f - ray_pos.x) * delta_dist.x;
        }
        
        if (ray_dir.y < 0){
            step.y = -1;
            side_dist.y = (ray_pos.y - map_y) * delta_dist.y;
        } 
        else{
            step.y = 1;
            side_dist.y = (map_y + 1.0f - ray_pos.y) * delta_dist.y;
        }
        
        // Raycasting loop with max iterations to prevent infinite loops
        int max_iterations = 100;
        int iterations = 0;
        
        while(!hit && iterations < max_iterations){
            if(side_dist.x < side_dist.y){
                side_dist.x += delta_dist.x;
                map_x += step.x;
                side = 0;
            } 
            else{
                side_dist.y += delta_dist.y;
                map_y += step.y;
                side = 1;
            }
            
            if(get_map_value(map_x, map_y) == 1){
                hit = true;
            }
            iterations++;
        }
        
        float dist = 0.1f;
        
        if(hit){
            // Calculate distance (fix fisheye effect)
            if (side == 0){
                dist = (map_x - ray_pos.x + (1 - step.x) / 2) / ray_dir.x;
            }
            else{
                dist = (map_y - ray_pos.y + (1 - step.y) / 2) / ray_dir.y;
            }
            
            // Prevent division by zero or negative distances
            if (dist < 0.1f) dist = 0.1f;
            
            // Calculate wall height
            int wall_height = (int)(screen_height / dist);
            int draw_start = MAX(0, (screen_height - wall_height) / 2);
            int draw_end = MIN(screen_height - 1, (screen_height + wall_height) / 2);
            
            // Calculate texture coordinate
            float wall_x;
            if(side == 0){
                wall_x = ray_pos.y + dist * ray_dir.y;
            } else {
                wall_x = ray_pos.x + dist * ray_dir.x;
            }
            wall_x -= Math::floor(wall_x);
            
            // Draw column
            for (int y = 0; y < screen_height; y++){
                Color pixel_color;
                
                if(y < draw_start){
                    // Ceiling
                    if(ceiling_texture.is_valid() && !ceiling_texture->is_empty()){
                        float ceiling_dist = screen_height / (2.0f * y - screen_height);
                        float weight = ceiling_dist / dist;
                        float current_floor_x = weight * (map_x - ray_pos.x + (1.0f - step.x) / 2.0f) + ray_pos.x;
                        float current_floor_y = weight * (map_y - ray_pos.y + (1.0f - step.y) / 2.0f) + ray_pos.y;
                        pixel_color = sample_texture(ceiling_texture, current_floor_x, current_floor_y);
                    } else {
                        pixel_color = ceiling_color;
                    }
                }
                else if(y >= draw_start && y <= draw_end){
                    // Wall
                    if(wall_texture.is_valid() && !wall_texture->is_empty()){
                        float tex_y = (float)(y - draw_start) / (float)wall_height;
                        pixel_color = sample_texture(wall_texture, wall_x, tex_y);
                        
                        // Shade based on side
                        if(side == 1){
                            pixel_color = pixel_color * 0.7f;
                        }
                    } else {
                        pixel_color = wall_color;
                        if(side == 1){
                            pixel_color = pixel_color * 0.7f;
                        }
                    }
                    
                    // Distance fog
                    float fog = 1.0f - MIN(dist / render_distance, 1.0f) * 0.6f;
                    pixel_color = pixel_color * fog;
                } 
                else{
                    // Floor
                    if(floor_texture.is_valid() && !floor_texture->is_empty()){
                        float floor_dist = screen_height / (2.0f * y - screen_height);
                        float weight = floor_dist / dist;
                        float current_floor_x = weight * (map_x - ray_pos.x + (1.0f - step.x) / 2.0f) + ray_pos.x;
                        float current_floor_y = weight * (map_y - ray_pos.y + (1.0f - step.y) / 2.0f) + ray_pos.y;
                        pixel_color = sample_texture(floor_texture, current_floor_x, current_floor_y);
                    } else {
                        pixel_color = floor_color;
                    }
                }
                
                render_image->set_pixel(x, y, pixel_color);
            }
        } 
        else{
            // No wall hit, draw ceiling and floor
            for(int y = 0; y < screen_height; y++){
                Color pixel_color;
                
                if(y < screen_height / 2){
                    // Ceiling
                    if(ceiling_texture.is_valid() && !ceiling_texture->is_empty()){
                        float ceiling_dist = screen_height / (2.0f * y - screen_height);
                        if(ceiling_dist > 0){
                            float current_x = player_pos.x + ray_dir.x * ceiling_dist;
                            float current_y = player_pos.y + ray_dir.y * ceiling_dist;
                            pixel_color = sample_texture(ceiling_texture, current_x, current_y);
                        } else {
                            pixel_color = ceiling_color;
                        }
                    } else {
                        pixel_color = ceiling_color;
                    }
                } else {
                    // Floor
                    if(floor_texture.is_valid() && !floor_texture->is_empty()){
                        float floor_dist = screen_height / (2.0f * y - screen_height);
                        float current_x = player_pos.x + ray_dir.x * floor_dist;
                        float current_y = player_pos.y + ray_dir.y * floor_dist;
                        pixel_color = sample_texture(floor_texture, current_x, current_y);
                    } else {
                        pixel_color = floor_color;
                    }
                }
                
                render_image->set_pixel(x, y, pixel_color);
            }
            dist = render_distance; // Max distance for depth buffer
        }
        
        depth_buffer.write[x] = dist;
    }
    
    // Render keys as billboards
    if(key_texture.is_valid() && !key_texture->is_empty()){
        for(int y = 0; y < map_height; y++){
            for(int x = 0; x < map_width; x++){
                if(get_map_value(x, y) == 2){
                    Vector2 key_pos(x + 0.5f, y + 0.5f);
                    
                    // Check if already collected
                    bool collected = false;
                    for(int i = 0; i < collected_keys.size(); i++){
                        if(collected_keys[i].distance_to(Vector2(x, y)) < 0.1f){
                            collected = true;
                            break;
                        }
                    }
                    
                    if(!collected){
                        Vector2 to_key = key_pos - player_pos;
                        float distance = to_key.length();
                        
                        if(distance < render_distance){
                            render_billboard(0, distance, key_pos, key_texture);
                        }
                    }
                }
            }
        }
    }
    
    render_texture->update(render_image);
}

int DoomRaycaster::get_map_value(int x, int y){
    if(x < 0 || y < 0 || x >= map_width || y >= map_height){
        return 1; // Out of bounds = wall
    }
    return map_data[y * map_width + x];
}

void DoomRaycaster::set_map(const Array &p_map, int p_width, int p_height){
    map_width = p_width;
    map_height = p_height;
    map_data.clear();
    collected_keys.clear();
    
    for(int i = 0; i < p_map.size(); i++){
        map_data.push_back(p_map[i]);
    }
    
    print_line("DoomRaycaster: Map set - " + itos(map_width) + "x" + itos(map_height) + " = " + itos(map_data.size()) + " cells");
}

void DoomRaycaster::set_player_position(Vector2 p_pos){
    player_pos = p_pos;
    print_line("DoomRaycaster: Player position set to (" + rtos(p_pos.x) + ", " + rtos(p_pos.y) + ")");
}

Vector2 DoomRaycaster::get_player_position() const{
    return player_pos;
}

void DoomRaycaster::set_player_angle(float p_angle){
    player_angle = p_angle;
}

float DoomRaycaster::get_player_angle() const{
    return player_angle;
}

void DoomRaycaster::set_screen_size(int p_width, int p_height){
    screen_width = p_width;
    screen_height = p_height;
    if (render_image.is_valid()) {
        render_image->initialize_data(screen_width, screen_height, false, Image::FORMAT_RGB8);
        render_texture->set_image(render_image);
    }
}

void DoomRaycaster::set_fov(float p_fov){
    fov = p_fov;
}

void DoomRaycaster::set_render_distance(float p_distance){
    render_distance = p_distance;
}

void DoomRaycaster::set_wall_color(Color p_color){
    wall_color = p_color;
}

void DoomRaycaster::set_floor_color(Color p_color){
    floor_color = p_color;
}

void DoomRaycaster::set_ceiling_color(Color p_color){
    ceiling_color = p_color;
}

void DoomRaycaster::set_wall_texture(Ref<Image> p_texture){
    wall_texture = p_texture;
    if(wall_texture.is_valid()){
        print_line("DoomRaycaster: Wall texture set - " + itos(wall_texture->get_width()) + "x" + itos(wall_texture->get_height()));
    }
}

void DoomRaycaster::set_floor_texture(Ref<Image> p_texture){
    floor_texture = p_texture;
    if(floor_texture.is_valid()){
        print_line("DoomRaycaster: Floor texture set - " + itos(floor_texture->get_width()) + "x" + itos(floor_texture->get_height()));
    }
}

void DoomRaycaster::set_ceiling_texture(Ref<Image> p_texture){
    ceiling_texture = p_texture;
    if(ceiling_texture.is_valid()){
        print_line("DoomRaycaster: Ceiling texture set - " + itos(ceiling_texture->get_width()) + "x" + itos(ceiling_texture->get_height()));
    }
}

void DoomRaycaster::set_key_texture(Ref<Image> p_texture){
    key_texture = p_texture;
    if(key_texture.is_valid()){
        print_line("DoomRaycaster: Key texture set - " + itos(key_texture->get_width()) + "x" + itos(key_texture->get_height()));
    }
}

void DoomRaycaster::clear_wall_texture(){
    wall_texture.unref();
    print_line("DoomRaycaster: Wall texture cleared");
}

void DoomRaycaster::clear_floor_texture(){
    floor_texture.unref();
    print_line("DoomRaycaster: Floor texture cleared");
}

void DoomRaycaster::clear_ceiling_texture(){
    ceiling_texture.unref();
    print_line("DoomRaycaster: Ceiling texture cleared");
}

void DoomRaycaster::clear_key_texture(){
    key_texture.unref();
    print_line("DoomRaycaster: Key texture cleared");
}

void DoomRaycaster::set_move_speed(float p_speed){
    move_speed = p_speed;
}

void DoomRaycaster::set_rotation_speed(float p_speed){
    rotation_speed = p_speed;
}