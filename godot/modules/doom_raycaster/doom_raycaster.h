#ifndef DOOM_RAYCASTER_H
#define DOOM_RAYCASTER_H

#include "core/object/ref_counted.h"
#include "scene/2d/node_2d.h"
#include "core/io/image.h"
#include "scene/resources/image_texture.h"

class DoomRaycaster : public Node2D{
    GDCLASS(DoomRaycaster, Node2D);

    private:
        // Map data
        Vector<int> map_data;
        int map_width = 0;
        int map_height = 0;
        
        // Player position and rotation
        Vector2 player_pos = Vector2(1.5, 1.5);
        float player_angle = 0.0f;
        
        // Rendering settings
        int screen_width = 800;
        int screen_height = 600;
        float fov = 60.0f;
        float render_distance = 20.0f;
        
        // Colors (used as fallback if no texture)
        Color wall_color = Color(0.7, 0.7, 0.7);
        Color floor_color = Color(0.3, 0.3, 0.3);
        Color ceiling_color = Color(0.1, 0.1, 0.1);
        
        // Textures
        Ref<Image> wall_texture;
        Ref<Image> floor_texture;
        Ref<Image> ceiling_texture;
        Ref<Image> key_texture;
        
        // Movement
        float move_speed = 3.0f;
        float rotation_speed = 2.0f;
        
        // Key tracking
        Vector<Vector2> collected_keys;
        
        Ref<Image> render_image;
        Ref<ImageTexture> render_texture;
        
        void raycast_and_render();
        int get_map_value(int x, int y);
        Color sample_texture(Ref<Image> texture, float u, float v);
        void render_billboard(int screen_x, float distance, Vector2 billboard_pos, Ref<Image> texture);

    protected:
        static void _bind_methods();
        void _notification(int p_what);

    public:
        DoomRaycaster();
        ~DoomRaycaster();
        
        // Map setup
        void set_map(const Array &p_map, int p_width, int p_height);
        
        // Player control
        void set_player_position(Vector2 p_pos);
        Vector2 get_player_position() const;
        void set_player_angle(float p_angle);
        float get_player_angle() const;
        
        // Rendering settings
        void set_screen_size(int p_width, int p_height);
        void set_fov(float p_fov);
        void set_render_distance(float p_distance);
        
        // Colors (fallback when no texture)
        void set_wall_color(Color p_color);
        void set_floor_color(Color p_color);
        void set_ceiling_color(Color p_color);
        
        // Textures
        void set_wall_texture(Ref<Image> p_texture);
        void set_floor_texture(Ref<Image> p_texture);
        void set_ceiling_texture(Ref<Image> p_texture);
        void set_key_texture(Ref<Image> p_texture);
        
        // Clear textures (revert to colors)
        void clear_wall_texture();
        void clear_floor_texture();
        void clear_ceiling_texture();
        void clear_key_texture();
        
        // Movement settings
        void set_move_speed(float p_speed);
        void set_rotation_speed(float p_speed);
};

#endif // DOOM_RAYCASTER_H