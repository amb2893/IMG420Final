#include "register_types.h"
#include "doom_raycaster.h"
#include "core/object/class_db.h"

void initialize_doom_raycaster_module(ModuleInitializationLevel p_level){
    if(p_level != MODULE_INITIALIZATION_LEVEL_SCENE){
        return;
    }
    ClassDB::register_class<DoomRaycaster>();
}

void uninitialize_doom_raycaster_module(ModuleInitializationLevel p_level){
    if(p_level != MODULE_INITIALIZATION_LEVEL_SCENE){
        return;
    }
}