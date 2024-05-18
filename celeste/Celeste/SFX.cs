using System.Collections.Generic;
using System.Reflection;

namespace Celeste
{
	public static class SFX
	{
		public const string NONE = "null";

		public const string music_levelselect = "event:/music/menu/level_select";

		public const string music_credits = "event:/music/menu/credits";

		public const string music_complete_area = "event:/music/menu/complete_area";

		public const string music_complete_summit = "event:/music/menu/complete_summit";

		public const string music_complete_bside = "event:/music/menu/complete_bside";

		public const string music_prologue_intro_vignette = "event:/game/00_prologue/intro_vignette";

		public const string music_prologue_beginning = "event:/music/lvl0/intro";

		public const string music_prologue_collapse = "event:/music/lvl0/bridge";

		public const string music_prologue_title_ping = "event:/music/lvl0/title_ping";

		public const string music_city = "event:/music/lvl1/main";

		public const string music_city_theo = "event:/music/lvl1/theo";

		public const string music_oldsite_beginning = "event:/music/lvl2/beginning";

		public const string music_oldsite_mirror = "event:/music/lvl2/mirror";

		public const string music_oldsite_dreamblock_sting_pt1 = "event:/music/lvl2/dreamblock_sting_pt1";

		public const string music_oldsite_dreamblock_sting_pt2 = "event:/music/lvl2/dreamblock_sting_pt2";

		public const string music_oldsite_evil_maddy = "event:/music/lvl2/evil_madeline";

		public const string music_oldsite_chase = "event:/music/lvl2/chase";

		public const string music_oldsite_payphone_loop = "event:/music/lvl2/phone_loop";

		public const string music_oldsite_payphone_end = "event:/music/lvl2/phone_end";

		public const string music_oldsite_awake = "event:/music/lvl2/awake";

		public const string music_resort_intro = "event:/music/lvl3/intro";

		public const string music_resort_explore = "event:/music/lvl3/explore";

		public const string music_resort_clean = "event:/music/lvl3/clean";

		public const string music_resort_clean_extended = "event:/music/lvl3/clean_extended";

		public const string music_resort_oshiro_theme = "event:/music/lvl3/oshiro_theme";

		public const string music_resort_oshiro_chase = "event:/music/lvl3/oshiro_chase";

		public const string music_cliffside_main = "event:/music/lvl4/main";

		public const string music_cliffside_heavywinds = "event:/music/lvl4/heavy_winds";

		public const string music_cliffside_panicattack = "event:/music/lvl4/minigame";

		public const string music_temple_normal = "event:/music/lvl5/normal";

		public const string music_temple_middle = "event:/music/lvl5/middle_temple";

		public const string music_temple_mirror = "event:/music/lvl5/mirror";

		public const string music_temple_mirrorcutscene = "event:/music/lvl5/mirror_cutscene";

		public const string music_reflection_maddietheo = "event:/music/lvl6/madeline_and_theo";

		public const string music_reflection_starjump = "event:/music/lvl6/starjump";

		public const string music_reflection_fall = "event:/music/lvl6/the_fall";

		public const string music_reflection_fight = "event:/music/lvl6/badeline_fight";

		public const string music_reflection_fight_glitch = "event:/music/lvl6/badeline_glitch";

		public const string music_reflection_fight_finish = "event:/music/lvl6/badeline_acoustic";

		public const string music_reflection_main = "event:/music/lvl6/main";

		public const string music_reflection_secretroom = "event:/music/lvl6/secret_room";

		public const string music_summit_main = "event:/music/lvl7/main";

		public const string music_summit_finalascent = "event:/music/lvl7/final_ascent";

		public const string music_epilogue_main = "event:/music/lvl8/main";

		public const string music_core_main = "event:/music/lvl9/main";

		public const string music_farewell_part01 = "event:/new_content/music/lvl10/part01";

		public const string music_farewell_part02 = "event:/new_content/music/lvl10/part02";

		public const string music_farewell_part03 = "event:/new_content/music/lvl10/part03";

		public const string music_farewell_intermission_heartgroove = "event:/new_content/music/lvl10/intermission_heartgroove";

		public const string music_farewell_intermission_powerpoint = "event:/new_content/music/lvl10/intermission_powerpoint";

		public const string music_farewell_reconciliation = "event:/new_content/music/lvl10/reconciliation";

		public const string music_farewell_cassette = "event:/new_content/music/lvl10/cassette_rooms";

		public const string music_farewell_final_run = "event:/new_content/music/lvl10/final_run";

		public const string music_farewell_end_cinematic = "event:/new_content/music/lvl10/cinematic/end";

		public const string music_farewell_end_cinematic_intro = "event:/new_content/music/lvl10/cinematic/end_intro";

		public const string music_farewell_firstbirdcrash_cinematic = "event:/new_content/music/lvl10/cinematic/bird_crash_first";

		public const string music_farewell_secondbirdcrash_cinematic = "event:/new_content/music/lvl10/cinematic/bird_crash_second";

		public const string music_farewell_granny = "event:/new_content/music/lvl10/granny_farewell";

		public const string music_farewell_golden_room = "event:/new_content/music/lvl10/golden_room";

		public const string music_pico8_title = "event:/classic/pico8_mus_00";

		public const string music_pico8_area1 = "event:/classic/pico8_mus_01";

		public const string music_pico8_area2 = "event:/classic/pico8_mus_02";

		public const string music_pico8_area3 = "event:/classic/pico8_mus_03";

		public const string music_pico8_wind = "event:/classic/sfx61";

		public const string music_pico8_end = "event:/classic/sfx62";

		public const string music_pico8_boot = "event:/classic/pico8_boot";

		public const string music_rmx_01_forsaken_city = "event:/music/remix/01_forsaken_city";

		public const string music_rmx_02_old_site = "event:/music/remix/02_old_site";

		public const string music_rmx_03_resort = "event:/music/remix/03_resort";

		public const string music_rmx_04_cliffside = "event:/music/remix/04_cliffside";

		public const string music_rmx_05_mirror_temple = "event:/music/remix/05_mirror_temple";

		public const string music_rmx_06_reflection = "event:/music/remix/06_reflection";

		public const string music_rmx_07_summit = "event:/music/remix/07_summit";

		public const string music_rmx_09_core = "event:/music/remix/09_core";

		public const string cas_01_forsaken_city = "event:/music/cassette/01_forsaken_city";

		public const string cas_02_old_site = "event:/music/cassette/02_old_site";

		public const string cas_03_resort = "event:/music/cassette/03_resort";

		public const string cas_04_cliffside = "event:/music/cassette/04_cliffside";

		public const string cas_05_mirror_temple = "event:/music/cassette/05_mirror_temple";

		public const string cas_06_reflection = "event:/music/cassette/06_reflection";

		public const string cas_07_summit = "event:/music/cassette/07_summit";

		public const string cas_08_core = "event:/music/cassette/09_core";

		public const string dialog_prefix = "event:/char/dialogue/";

		public const string dialog_phone_static_ex = "event:/char/dialogue/sfx_support/phone_static_ex";

		public const string dialog_phone_static_mom = "event:/char/dialogue/sfx_support/phone_static_mom";

		public const string char_mad_footstep = "event:/char/madeline/footstep";

		public const string char_mad_land = "event:/char/madeline/landing";

		public const string char_mad_jump = "event:/char/madeline/jump";

		public const string char_mad_jump_assisted = "event:/char/madeline/jump_assisted";

		public const string char_mad_jump_wall_right = "event:/char/madeline/jump_wall_right";

		public const string char_mad_jump_wall_left = "event:/char/madeline/jump_wall_left";

		public const string char_mad_jump_climb_left = "event:/char/madeline/jump_climb_left";

		public const string char_mad_jump_climb_right = "event:/char/madeline/jump_climb_right";

		public const string char_mad_jump_super = "event:/char/madeline/jump_super";

		public const string char_mad_jump_superwall = "event:/char/madeline/jump_superwall";

		public const string char_mad_jump_superslide = "event:/char/madeline/jump_superslide";

		public const string char_mad_jump_special = "event:/char/madeline/jump_special";

		public const string char_mad_jump_dreamblock = "event:/char/madeline/jump_dreamblock";

		public const string char_mad_grab = "event:/char/madeline/grab";

		public const string char_mad_grab_letgo = "event:/char/madeline/grab_letgo";

		public const string char_mad_handhold = "event:/char/madeline/handhold";

		public const string char_mad_wallslide = "event:/char/madeline/wallslide";

		public const string char_mad_duck = "event:/char/madeline/duck";

		public const string char_mad_stand = "event:/char/madeline/stand";

		public const string char_mad_climb_loop = "event:/char/madeline/climb_loop";

		public const string char_mad_climb_ledge = "event:/char/madeline/climb_ledge";

		public const string char_mad_dash_red_left = "event:/char/madeline/dash_red_left";

		public const string char_mad_dash_red_right = "event:/char/madeline/dash_red_right";

		public const string char_mad_dash_pink_left = "event:/char/madeline/dash_pink_left";

		public const string char_mad_dash_pink_right = "event:/char/madeline/dash_pink_right";

		public const string char_mad_core_haircharged = "event:/char/madeline/core_hair_charged";

		public const string char_mad_predeath = "event:/char/madeline/predeath";

		public const string char_mad_death = "event:/char/madeline/death";

		public const string char_mad_death_golden = "event:/new_content/char/madeline/death_golden";

		public const string char_mad_revive = "event:/char/madeline/revive";

		public const string char_mad_campfire_sit = "event:/char/madeline/campfire_sit";

		public const string char_mad_campfire_stand = "event:/char/madeline/campfire_stand";

		public const string char_mad_dreamblock_enter = "event:/char/madeline/dreamblock_enter";

		public const string char_mad_dreamblock_travel = "event:/char/madeline/dreamblock_travel";

		public const string char_mad_dreamblock_exit = "event:/char/madeline/dreamblock_exit";

		public const string char_mad_mirrortemple_landing = "event:/char/madeline/mirrortemple_big_landing";

		public const string char_mad_crystaltheo_lift = "event:/char/madeline/crystaltheo_lift";

		public const string char_mad_crystaltheo_throw = "event:/char/madeline/crystaltheo_throw";

		public const string char_mad_theo_collapse = "event:/char/madeline/theo_collapse";

		public const string char_mad_summit_flytonext = "event:/char/madeline/summit_flytonext";

		public const string char_mad_summit_areastart = "event:/char/madeline/summit_areastart";

		public const string char_mad_summit_sit = "event:/char/madeline/summit_sit";

		public const string char_mad_idle_scratch = "event:/char/madeline/idle_scratch";

		public const string char_mad_idle_sneeze = "event:/char/madeline/idle_sneeze";

		public const string char_mad_idle_crackknuckles = "event:/char/madeline/idle_crackknuckles";

		public const string char_mad_backpack_drop = "event:/char/madeline/backpack_drop";

		public const string char_mad_screenentry_lowgrav = "event:/new_content/char/madeline/screenentry_lowgrav";

		public const string char_mad_screenentry_stubborn = "event:/new_content/char/madeline/screenentry_stubborn";

		public const string char_mad_screenentry_golden = "event:/new_content/char/madeline/screenentry_golden";

		public const string char_mad_screenentry_gran = "event:/new_content/char/madeline/screenentry_gran";

		public const string char_mad_screenentry_gran_landing = "event:/new_content/char/madeline/screenentry_gran_landing";

		public const string char_mad_glider_drop = "event:/new_content/char/madeline/glider_drop";

		public const string char_mad_bounce_boost = "event:/new_content/char/madeline/bounce_boost";

		public const string char_mad_water_in = "event:/char/madeline/water_in";

		public const string char_mad_water_out = "event:/char/madeline/water_out";

		public const string char_mad_water_dash_in = "event:/char/madeline/water_dash_in";

		public const string char_mad_water_dash_out = "event:/char/madeline/water_dash_out";

		public const string char_mad_water_dash_gen = "event:/char/madeline/water_dash_gen";

		public const string char_mad_water_move_shallow = "event:/char/madeline/water_move_shallow";

		public const string char_mad_water_move_general = "event:/char/madeline/water_move_general";

		public const string char_mad_energy_out_loop = "event:/char/madeline/energy_out_loop";

		public const string char_mad_energy_recharged = "event:/char/madeline/energy_recharged";

		public const string char_mad_hiccup_standing = "event:/new_content/char/madeline/hiccup_standing";

		public const string char_mad_hiccup_ducking = "event:/new_content/char/madeline/hiccup_ducking";

		public const string char_theo_yolofist = "event:/char/theo/yolo_fist";

		public const string char_theo_phonetaps_loop = "event:/char/theo/phone_taps_loop";

		public const string char_theo_resort_vent_tug = "event:/char/theo/resort_vent_tug";

		public const string char_theo_resort_vent_grab = "event:/char/theo/resort_vent_grab";

		public const string char_theo_resort_vent_rip = "event:/char/theo/resort_vent_rip";

		public const string char_theo_resort_vent_tumble = "event:/char/theo/resort_vent_tumble";

		public const string char_theo_resort_standtocrawl = "event:/char/theo/resort_standtocrawl";

		public const string char_theo_resort_crawl = "event:/char/theo/resort_crawl";

		public const string char_theo_resort_ceilingvent_shake = "event:/char/theo/resort_ceilingvent_shake";

		public const string char_theo_resort_ceilingvent_popoff = "event:/char/theo/resort_ceilingvent_popoff";

		public const string char_theo_resort_ceilingvent_hey = "event:/char/theo/resort_ceilingvent_hey";

		public const string char_theo_resort_ceilingvent_seeya = "event:/char/theo/resort_ceilingvent_seeya";

		public const string char_bad_appear = "event:/char/badeline/appear";

		public const string char_bad_disappear = "event:/char/badeline/disappear";

		public const string char_bad_level_entry = "event:/char/badeline/level_entry";

		public const string char_bad_footstep = "event:/char/badeline/footstep";

		public const string char_bad_land = "event:/char/badeline/landing";

		public const string char_bad_grab = "event:/char/badeline/grab";

		public const string char_bad_handhold = "event:/char/badeline/handhold";

		public const string char_bad_wallslide = "event:/char/badeline/wallslide";

		public const string char_bad_grab_letgo = "event:/char/badeline/grab_letgo";

		public const string char_bad_jump = "event:/char/badeline/jump";

		public const string char_bad_jump_assisted = "event:/char/badeline/jump_assisted";

		public const string char_bad_jump_wall_left = "event:/char/badeline/jump_wall_left";

		public const string char_bad_jump_wall_right = "event:/char/badeline/jump_wall_right";

		public const string char_bad_jump_climb_left = "event:/char/badeline/jump_climb_left";

		public const string char_bad_jump_climb_right = "event:/char/badeline/jump_climb_right";

		public const string char_bad_duck = "event:/char/badeline/duck";

		public const string char_bad_stand = "event:/char/badeline/stand";

		public const string char_bad_climb_ledge = "event:/char/badeline/climb_ledge";

		public const string char_bad_dash_red_left = "event:/char/badeline/dash_red_left";

		public const string char_bad_dash_red_right = "event:/char/badeline/dash_red_right";

		public const string char_bad_jump_dreamblock = "event:/char/badeline/jump_dreamblock";

		public const string char_bad_dreamblock_enter = "event:/char/badeline/dreamblock_enter";

		public const string char_bad_dreamblock_travel = "event:/char/badeline/dreamblock_travel";

		public const string char_bad_dreamblock_exit = "event:/char/badeline/dreamblock_exit";

		public const string char_bad_jump_super = "event:/char/badeline/jump_super";

		public const string char_bad_jump_superwall = "event:/char/badeline/jump_superwall";

		public const string char_bad_jump_superslide = "event:/char/badeline/jump_superslide";

		public const string char_bad_jump_special = "event:/char/badeline/jump_special";

		public const string char_bad_temple_move_first = "event:/char/badeline/temple_move_first";

		public const string char_bad_temple_move_chats = "event:/char/badeline/temple_move_chats";

		public const string char_bad_boss_prefightgetup = "event:/char/badeline/boss_prefight_getup";

		public const string char_bad_boss_hug = "event:/char/badeline/boss_hug";

		public const string char_bad_boss_idle_air = "event:/char/badeline/boss_idle_air";

		public const string char_bad_boss_idle_ground_loop = "event:/char/badeline/boss_idle_ground";

		public const string char_bad_boss_bullet = "event:/char/badeline/boss_bullet";

		public const string char_bad_boss_laser_charge = "event:/char/badeline/boss_laser_charge";

		public const string char_bad_boss_laser_fire = "event:/char/badeline/boss_laser_fire";

		public const string char_bad_booster_begin = "event:/char/badeline/booster_begin";

		public const string char_bad_booster_throw = "event:/char/badeline/booster_throw";

		public const string char_bad_booster_relocate = "event:/char/badeline/booster_relocate";

		public const string char_bad_booster_reappear = "event:/char/badeline/booster_reappear";

		public const string char_bad_booster_final = "event:/char/badeline/booster_final";

		public const string char_bad_maddy_join = "event:/char/badeline/maddy_join";

		public const string char_bad_maddy_split = "event:/char/badeline/maddy_split";

		public const string char_bad_maddy_join_quick = "event:/new_content/char/badeline/maddy_join_quick";

		public const string char_bad_booster_first_appear = "event:/new_content/char/badeline/booster_first_appear";

		public const string char_bad_booster_relocate_slow = "event:/new_content/char/badeline/booster_relocate_slow";

		public const string char_bad_booster_finalfinal_part1 = "event:/new_content/char/badeline/booster_finalfinal_part1";

		public const string char_bad_booster_finalfinal_part2 = "event:/new_content/char/badeline/booster_finalfinal_part2";

		public const string char_bad_birdscene_float = "event:/new_content/char/badeline/birdcrash_scene_float";

		public const string char_gran_laugh_firstphrase = "event:/char/granny/laugh_firstphrase";

		public const string char_gran_laugh_oneha = "event:/char/granny/laugh_oneha";

		public const string char_gran_cane_tap = "event:/char/granny/cane_tap";

		public const string char_gran_dissipate = "event:/new_content/char/granny/dissipate";

		public const string char_gran_cane_tap_ending = "event:/new_content/char/granny/cane_tap_ending";

		public const string char_oshiro_chat_turn_right = "event:/char/oshiro/chat_turn_right";

		public const string char_oshiro_chat_turn_left = "event:/char/oshiro/chat_turn_left";

		public const string char_oshiro_chat_collapse = "event:/char/oshiro/chat_collapse";

		public const string char_oshiro_chat_getup = "event:/char/oshiro/chat_get_up";

		public const string char_oshiro_boss_transform_begin = "event:/char/oshiro/boss_transform_begin";

		public const string char_oshiro_boss_transform_burst = "event:/char/oshiro/boss_transform_burst";

		public const string char_oshiro_boss_enterscreen = "event:/char/oshiro/boss_enter_screen";

		public const string char_oshiro_boss_reform = "event:/char/oshiro/boss_reform";

		public const string char_oshiro_boss_precharge = "event:/char/oshiro/boss_precharge";

		public const string char_oshiro_boss_charge = "event:/char/oshiro/boss_charge";

		public const string char_oshiro_boss_slam_first = "event:/char/oshiro/boss_slam_first";

		public const string char_oshiro_boss_slam_final = "event:/char/oshiro/boss_slam_final";

		public const string char_oshiro_move_01_0xa_exit = "event:/char/oshiro/move_01_0xa_exit";

		public const string char_oshiro_move_02_03a_exit = "event:/char/oshiro/move_02_03a_exit";

		public const string char_oshiro_move_03_08a_exit = "event:/char/oshiro/move_03_08a_exit";

		public const string char_oshiro_move_04_pace_right = "event:/char/oshiro/move_04_pace_right";

		public const string char_oshiro_move_04_pace_left = "event:/char/oshiro/move_04_pace_left";

		public const string char_oshiro_move_05_09b_exit = "event:/char/oshiro/move_05_09b_exit";

		public const string char_oshiro_move_06_04d_exit = "event:/char/oshiro/move_06_04d_exit";

		public const string char_oshiro_move_07_roof00_enter = "event:/char/oshiro/move_07_roof00_enter";

		public const string char_oshiro_move_08_roof07_exit = "event:/char/oshiro/move_08_roof07_exit";

		public const string char_tutghost_appear = "event:/new_content/char/tutorial_ghost/appear";

		public const string char_tutghost_disappear = "event:/new_content/char/tutorial_ghost/disappear";

		public const string char_tutghost_jump = "event:/new_content/char/tutorial_ghost/jump";

		public const string char_tutghost_jump_super = "event:/new_content/char/tutorial_ghost/jump_super";

		public const string char_tutghost_dash_left = "event:/new_content/char/tutorial_ghost/dash_red_left";

		public const string char_tutghost_dash_right = "event:/new_content/char/tutorial_ghost/dash_red_right";

		public const string char_tutghost_dreamblock_sequence = "event:/new_content/char/tutorial_ghost/dreamblock_sequence";

		public const string char_tutghost_land = "event:/new_content/char/tutorial_ghost/land";

		public const string char_tutghost_grab = "event:/new_content/char/tutorial_ghost/grab";

		public const string char_tutghost_footstep = "event:/new_content/char/tutorial_ghost/footstep";

		public const string char_tutghost_handhold = "event:/new_content/char/tutorial_ghost/handhold";

		public const string game_gen_diamond_touch = "event:/game/general/diamond_touch";

		public const string game_gen_diamond_return = "event:/game/general/diamond_return";

		public const string game_gen_strawberry_touch = "event:/game/general/strawberry_touch";

		public const string game_gen_strawberry_blue_touch = "event:/game/general/strawberry_blue_touch";

		public const string game_gen_strawberry_pulse = "event:/game/general/strawberry_pulse";

		public const string game_gen_strawberry_blue_pulse = "event:/game/general/strawberry_blue_pulse";

		public const string game_gen_strawberry_get = "event:/game/general/strawberry_get";

		public const string game_gen_strawberry_flyaway = "event:/game/general/strawberry_flyaway";

		public const string game_gen_strawberry_laugh = "event:/game/general/strawberry_laugh";

		public const string game_gen_strawberry_wingflap = "event:/game/general/strawberry_wingflap";

		public const string game_gen_seed_pulse = "event:/game/general/seed_pulse";

		public const string game_gen_seed_touch = "event:/game/general/seed_touch";

		public const string game_gen_seed_poof = "event:/game/general/seed_poof";

		public const string game_gen_seed_reappear = "event:/game/general/seed_reappear";

		public const string game_gen_seed_complete_main = "event:/game/general/seed_complete_main";

		public const string game_gen_seed_complete_berry = "event:/game/general/seed_complete_berry";

		public const string game_gen_key_get = "event:/game/general/key_get";

		public const string game_gen_lookout_use = "event:/game/general/lookout_use";

		public const string game_gen_lookout_move = "event:/game/general/lookout_move";

		public const string game_gen_touchswitch_any = "event:/game/general/touchswitch_any";

		public const string game_gen_touchswitch_last_oneshot = "event:/game/general/touchswitch_last_oneshot";

		public const string game_gen_touchswitch_last_cutoff = "event:/game/general/touchswitch_last_cutoff";

		public const string game_gen_touchswitch_gate_open = "event:/game/general/touchswitch_gate_open";

		public const string game_gen_touchswitch_gate_finish = "event:/game/general/touchswitch_gate_finish";

		public const string game_gen_crystalheart_bounce = "event:/game/general/crystalheart_bounce";

		public const string game_gen_crystalheart_pulse = "event:/game/general/crystalheart_pulse";

		public const string game_gen_crystalheart_red_get = "event:/game/general/crystalheart_red_get";

		public const string game_gen_crystalheart_blue_get = "event:/game/general/crystalheart_blue_get";

		public const string game_gen_crystalheart_gold_get = "event:/game/general/crystalheart_gold_get";

		public const string game_gen_thing_booped = "event:/game/general/thing_booped";

		public const string game_gen_cassette_bubblereturn = "event:/game/general/cassette_bubblereturn";

		public const string game_gen_cassette_get = "event:/game/general/cassette_get";

		public const string game_gen_cassette_preview = "event:/game/general/cassette_preview";

		public const string game_gen_spring = "event:/game/general/spring";

		public const string game_gen_platform_disintegrate = "event:/game/general/platform_disintegrate";

		public const string game_gen_platform_unit_return = "event:/game/general/platform_return";

		public const string game_gen_secret_revealed = "event:/game/general/secret_revealed";

		public const string game_gen_passageclosedbehind = "event:/game/general/passage_closed_behind";

		public const string game_gen_bird_squawk = "event:/game/general/bird_squawk";

		public const string game_gen_bird_startle = "event:/game/general/bird_startle";

		public const string game_gen_bird_in = "event:/game/general/bird_in";

		public const string game_gen_bird_land_dirt = "event:/game/general/bird_land_dirt";

		public const string game_gen_bird_peck = "event:/game/general/bird_peck";

		public const string game_gen_birdbaby_hop = "event:/game/general/birdbaby_hop";

		public const string game_gen_birdbaby_flyaway = "event:/game/general/birdbaby_flyaway";

		public const string game_gen_birdbaby_tweet_loop = "event:/game/general/birdbaby_tweet_loop";

		public const string game_gen_wallbreak_dirt = "event:/game/general/wall_break_dirt";

		public const string game_gen_wallbreak_stone = "event:/game/general/wall_break_stone";

		public const string game_gen_wallbreak_ice = "event:/game/general/wall_break_ice";

		public const string game_gen_wallbreak_wood = "event:/game/general/wall_break_wood";

		public const string game_gen_fallblock_shake = "event:/game/general/fallblock_shake";

		public const string game_gen_fallblock_impact = "event:/game/general/fallblock_impact";

		public const string game_gen_debris_stone = "event:/game/general/debris_stone";

		public const string game_gen_debris_wood = "event:/game/general/debris_wood";

		public const string game_gen_debris_dirt = "event:/game/general/debris_dirt";

		public const string game_gen_spotlight_intro = "event:/game/general/spotlight_intro";

		public const string game_gen_spotlight_outro = "event:/game/general/spotlight_outro";

		public const string game_gen_cassetteblock_switch_1 = "event:/game/general/cassette_block_switch_1";

		public const string game_gen_cassetteblock_switch_2 = "event:/game/general/cassette_block_switch_2";

		public const string game_assist_screenbottom = "event:/game/general/assist_screenbottom";

		public const string game_assist_dreamblockbounce = "event:/game/general/assist_dreamblockbounce";

		public const string game_assist_nonsolidblock_in = "event:/game/general/assist_nonsolid_in";

		public const string game_assist_nonsolidblock_out = "event:/game/general/assist_nonsolid_out";

		public const string game_assist_dash_aim = "event:/game/general/assist_dash_aim";

		public const string game_00_car_down = "event:/game/00_prologue/car_down";

		public const string game_00_car_up = "event:/game/00_prologue/car_up";

		public const string game_00_fallingblock_prologue_shake = "event:/game/00_prologue/fallblock_first_shake";

		public const string game_00_fallingblock_prologue_impact = "event:/game/00_prologue/fallblock_first_impact";

		public const string game_00_bridge_rumble_loop = "event:/game/00_prologue/bridge_rumble_loop";

		public const string game_00_bridge_supportbreak = "event:/game/00_prologue/bridge_support_break";

		public const string game_01_zipmover = "event:/game/01_forsaken_city/zip_mover";

		public const string game_01_fallingblock_ice_shake = "event:/game/01_forsaken_city/fallblock_ice_shake";

		public const string game_01_fallingblock_ice_impact = "event:/game/01_forsaken_city/fallblock_ice_impact";

		public const string game_01_birdbros_fly_loop = "event:/game/01_forsaken_city/birdbros_fly_loop";

		public const string game_01_birdbros_thrust = "event:/game/01_forsaken_city/birdbros_thrust";

		public const string game_01_birdbros_finish = "event:/game/01_forsaken_city/birdbros_finish";

		public const string game_01_console_static_short = "event:/game/01_forsaken_city/console_static_short";

		public const string game_01_console_static_long = "event:/game/01_forsaken_city/console_static_long";

		public const string game_01_console_static_loop = "event:/game/01_forsaken_city/console_static_loop";

		public const string game_01_console_white = "event:/game/01_forsaken_city/console_white";

		public const string game_01_console_purple = "event:/game/01_forsaken_city/console_purple";

		public const string game_01_console_yellow = "event:/game/01_forsaken_city/console_yellow";

		public const string game_01_console_red = "event:/game/01_forsaken_city/console_red";

		public const string game_01_console_blue = "event:/game/01_forsaken_city/console_blue";

		public const string game_02_sequence_mirror = "event:/game/02_old_site/sequence_mirror";

		public const string game_02_sequence_badeline_intro = "event:/game/02_old_site/sequence_badeline_intro";

		public const string game_02_sequence_phone_pickup = "event:/game/02_old_site/sequence_phone_pickup";

		public const string game_02_sequence_phone_transform = "event:/game/02_old_site/sequence_phone_transform";

		public const string game_02_sequence_phone_ring_loop = "event:/game/02_old_site/sequence_phone_ring_loop";

		public const string game_02_sequence_phone_ringtone_loop = "event:/game/02_old_site/sequence_phone_ringtone_loop";

		public const string game_02_theoselfie_foley = "event:/game/02_old_site/theoselfie_foley";

		public const string game_02_theoselfie_photo_in = "event:/game/02_old_site/theoselfie_photo_in";

		public const string game_02_theoselfie_photo_out = "event:/game/02_old_site/theoselfie_photo_out";

		public const string game_02_theoselfie_photo_filter = "event:/game/02_old_site/theoselfie_photo_filter";

		public const string game_02_lantern_hit = "event:/game/02_old_site/lantern_hit";

		public const string game_03_door_wood_open = "event:/game/03_resort/door_wood_open";

		public const string game_03_door_wood_close = "event:/game/03_resort/door_wood_close";

		public const string game_03_fallingblock_wood_shake = "event:/game/03_resort/fallblock_wood_shake";

		public const string game_03_fallingblock_wood_impact = "event:/game/03_resort/fallblock_wood_impact";

		public const string game_03_fallingblock_wood_distantimpact = "event:/game/03_resort/fallblock_wooddistant_impact";

		public const string game_03_door_metal_open = "event:/game/03_resort/door_metal_open";

		public const string game_03_door_metal_close = "event:/game/03_resort/door_metal_close";

		public const string game_03_door_trapdoor_fromtop = "event:/game/03_resort/trapdoor_fromtop";

		public const string game_03_door_trapdoor_frombottom = "event:/game/03_resort/trapdoor_frombottom";

		public const string game_03_key_unlock = "event:/game/03_resort/key_unlock";

		public const string game_03_forcefield_vanish = "event:/game/03_resort/forcefield_vanish";

		public const string game_03_forcefield_bump = "event:/game/03_resort/forcefield_bump";

		public const string game_03_platform_horiz_left = "event:/game/03_resort/platform_horiz_left";

		public const string game_03_platform_horiz_right = "event:/game/03_resort/platform_horiz_right";

		public const string game_03_lantern_bump = "event:/game/03_resort/lantern_bump";

		public const string game_03_deskbell_again = "event:/game/03_resort/deskbell_again";

		public const string game_03_clutterswitch_return = "event:/game/03_resort/clutterswitch_return";

		public const string game_03_clutterswitch_squish = "event:/game/03_resort/clutterswitch_squish";

		public const string game_03_clutterswitch_press_books = "event:/game/03_resort/clutterswitch_books";

		public const string game_03_clutterswitch_press_boxes = "event:/game/03_resort/clutterswitch_boxes";

		public const string game_03_clutterswitch_press_linens = "event:/game/03_resort/clutterswitch_linens";

		public const string game_03_clutterswitch_press_finish = "event:/game/03_resort/clutterswitch_finish";

		public const string game_03_fluff_tendril_emerge = "event:/game/03_resort/fluff_tendril_emerge";

		public const string game_03_fluff_tendril_recede = "event:/game/03_resort/fluff_tendril_recede";

		public const string game_03_fluff_tendril_touch = "event:/game/03_resort/fluff_tendril_touch";

		public const string game_03_platform_vert_start = "event:/game/03_resort/platform_vert_start";

		public const string game_03_platform_vert_down_loop = "event:/game/03_resort/platform_vert_down_loop";

		public const string game_03_platform_vert_up_loop = "event:/game/03_resort/platform_vert_up_loop";

		public const string game_03_platform_vert_end = "event:/game/03_resort/platform_vert_end";

		public const string game_03_memo_in = "event:/game/03_resort/memo_in";

		public const string game_03_memo_out = "event:/game/03_resort/memo_out";

		public const string game_03_suite_badintro = "event:/game/03_resort/suite_bad_intro";

		public const string game_03_suite_badmirrorbreak = "event:/game/03_resort/suite_bad_mirrorbreak";

		public const string game_03_suite_badmovestageleft = "event:/game/03_resort/suite_bad_movestageleft";

		public const string game_03_suite_badceilingbreak = "event:/game/03_resort/suite_bad_ceilingbreak";

		public const string game_03_suite_badexittop = "event:/game/03_resort/suite_bad_exittop";

		public const string game_03_suite_badmoveroof = "event:/game/03_resort/suite_bad_moveroof";

		public const string game_03_sequence_oshiro_intro = "event:/game/03_resort/sequence_oshiro_intro";

		public const string game_03_sequence_oshirofluff_pt1 = "event:/game/03_resort/sequence_oshirofluff_pt1";

		public const string game_03_sequence_oshirofluff_pt2 = "event:/game/03_resort/sequence_oshirofluff_pt2";

		public const string game_04_arrowblock_activate = "event:/game/04_cliffside/arrowblock_activate";

		public const string game_04_arrowblock_move_loop = "event:/game/04_cliffside/arrowblock_move";

		public const string game_04_arrowblock_side_depress = "event:/game/04_cliffside/arrowblock_side_depress";

		public const string game_04_arrowblock_side_release = "event:/game/04_cliffside/arrowblock_side_release";

		public const string game_04_arrowblock_break = "event:/game/04_cliffside/arrowblock_break";

		public const string game_04_arrowblock_debris = "event:/game/04_cliffside/arrowblock_debris";

		public const string game_04_arrowblock_reform_begin = "event:/game/04_cliffside/arrowblock_reform_begin";

		public const string game_04_arrowblock_reappear = "event:/game/04_cliffside/arrowblock_reappear";

		public const string game_04_greenbooster_enter = "event:/game/04_cliffside/greenbooster_enter";

		public const string game_04_greenbooster_dash = "event:/game/04_cliffside/greenbooster_dash";

		public const string game_04_greenbooster_end = "event:/game/04_cliffside/greenbooster_end";

		public const string game_04_greenbooster_reappear = "event:/game/04_cliffside/greenbooster_reappear";

		public const string game_04_cloud_blue_boost = "event:/game/04_cliffside/cloud_blue_boost";

		public const string game_04_cloud_pink_boost = "event:/game/04_cliffside/cloud_pink_boost";

		public const string game_04_cloud_pink_reappear = "event:/game/04_cliffside/cloud_pink_reappear";

		public const string game_04_snowball_spawn = "event:/game/04_cliffside/snowball_spawn";

		public const string game_04_snowball_impact = "event:/game/04_cliffside/snowball_impact";

		public const string game_04_stone_blockade = "event:/game/04_cliffside/stone_blockade";

		public const string game_04_whiteblock_fallthru = "event:/game/04_cliffside/whiteblock_fallthru";

		public const string game_04_gondola_theo_fall = "event:/game/04_cliffside/gondola_theo_fall";

		public const string game_04_gondola_theo_recover = "event:/game/04_cliffside/gondola_theo_recover";

		public const string game_04_gondola_theo_lever_start = "event:/game/04_cliffside/gondola_theo_lever_start";

		public const string game_04_gondola_cliffmechanism_start = "event:/game/04_cliffside/gondola_cliffmechanism_start";

		public const string game_04_gondola_movement_loop = "event:/game/04_cliffside/gondola_movement_loop";

		public const string game_04_gondola_theoselfie_halt = "event:/game/04_cliffside/gondola_theoselfie_halt";

		public const string game_04_gondola_halted_loop = "event:/game/04_cliffside/gondola_halted_loop";

		public const string game_04_gondola_theo_lever_fail = "event:/game/04_cliffside/gondola_theo_lever_fail";

		public const string game_04_gondola_scaryhair_01 = "event:/game/04_cliffside/gondola_scaryhair_01";

		public const string game_04_gondola_scaryhair_02 = "event:/game/04_cliffside/gondola_scaryhair_02";

		public const string game_04_gondola_scaryhair_03 = "event:/game/04_cliffside/gondola_scaryhair_03";

		public const string game_04_gondola_restart = "event:/game/04_cliffside/gondola_restart";

		public const string game_04_gondola_finish = "event:/game/04_cliffside/gondola_finish";

		public const string game_05_key_unlock_light = "event:/game/05_mirror_temple/key_unlock_light";

		public const string game_05_key_unlock_dark = "event:/game/05_mirror_temple/key_unlock_dark";

		public const string game_05_redbooster_enter = "event:/game/05_mirror_temple/redbooster_enter";

		public const string game_05_redbooster_dash = "event:/game/05_mirror_temple/redbooster_dash";

		public const string game_05_redbooster_move_loop = "event:/game/05_mirror_temple/redbooster_move";

		public const string game_05_redbooster_end = "event:/game/05_mirror_temple/redbooster_end";

		public const string game_05_redbooster_reappear = "event:/game/05_mirror_temple/redbooster_reappear";

		public const string game_05_torch_activate = "event:/game/05_mirror_temple/torch_activate";

		public const string game_05_bladespinner_spin = "event:/game/05_mirror_temple/bladespinner_spin";

		public const string game_05_room_lightlevel_down = "event:/game/05_mirror_temple/room_lightlevel_down";

		public const string game_05_room_lightlevel_up = "event:/game/05_mirror_temple/room_lightlevel_up";

		public const string game_05_swapblock_move = "event:/game/05_mirror_temple/swapblock_move";

		public const string game_05_swapblock_move_end = "event:/game/05_mirror_temple/swapblock_move_end";

		public const string game_05_swapblock_return = "event:/game/05_mirror_temple/swapblock_return";

		public const string game_05_swapblock_return_end = "event:/game/05_mirror_temple/swapblock_return_end";

		public const string game_05_gatebutton_depress = "event:/game/05_mirror_temple/button_depress";

		public const string game_05_gatebutton_return = "event:/game/05_mirror_temple/button_return";

		public const string game_05_gatebutton_activate = "event:/game/05_mirror_temple/button_activate";

		public const string game_05_gate_main_open = "event:/game/05_mirror_temple/gate_main_open";

		public const string game_05_gate_main_close = "event:/game/05_mirror_temple/gate_main_close";

		public const string game_05_gate_theo_open = "event:/game/05_mirror_temple/gate_theo_open";

		public const string game_05_gate_theo_close = "event:/game/05_mirror_temple/gate_theo_close";

		public const string game_05_sequence_mainmirror_torch_1 = "event:/game/05_mirror_temple/mainmirror_torch_lit_1";

		public const string game_05_sequence_mainmirror_torch_2 = "event:/game/05_mirror_temple/mainmirror_torch_lit_2";

		public const string game_05_sequence_mainmirror_torch_loop = "event:/game/05_mirror_temple/mainmirror_torch_loop";

		public const string game_05_sequence_mainmirror_reveal = "event:/game/05_mirror_temple/mainmirror_reveal";

		public const string game_05_seeker_playercontrolstart = "event:/game/05_mirror_temple/seeker_playercontrolstart";

		public const string game_05_seeker_statuebreak = "event:/game/05_mirror_temple/seeker_statue_break";

		public const string game_05_seeker_aggro = "event:/game/05_mirror_temple/seeker_aggro";

		public const string game_05_seeker_dash = "event:/game/05_mirror_temple/seeker_dash";

		public const string game_05_seeker_dash_turn = "event:/game/05_mirror_temple/seeker_dash_turn";

		public const string game_05_seeker_impact_normal = "event:/game/05_mirror_temple/seeker_hit_normal";

		public const string game_05_seeker_impact_lightwall = "event:/game/05_mirror_temple/seeker_hit_lightwall";

		public const string game_05_seeker_booped = "event:/game/05_mirror_temple/seeker_booped";

		public const string game_05_seeker_death = "event:/game/05_mirror_temple/seeker_death";

		public const string game_05_seeker_revive = "event:/game/05_mirror_temple/seeker_revive";

		public const string game_05_crackedwall_vanish = "event:/game/05_mirror_temple/crackedwall_vanish";

		public const string game_05_crystaltheo_breakfree = "event:/game/05_mirror_temple/crystaltheo_break_free";

		public const string game_05_crystaltheo_impact_ground = "event:/game/05_mirror_temple/crystaltheo_hit_ground";

		public const string game_05_crystaltheo_impact_side = "event:/game/05_mirror_temple/crystaltheo_hit_side";

		public const string game_05_eyewall_idle_eyemove = "event:/game/05_mirror_temple/eyebro_eyemove";

		public const string game_05_eyewall_pulse = "event:/game/05_mirror_temple/eye_pulse";

		public const string game_05_eyewall_destroy = "event:/game/05_mirror_temple/eyewall_destroy";

		public const string game_05_eyewall_bounce = "event:/game/05_mirror_temple/eyewall_bounce";

		public const string game_06_fallingblock_boss_shake = "event:/game/06_reflection/fallblock_boss_shake";

		public const string game_06_fallingblock_boss_impact = "event:/game/06_reflection/fallblock_boss_impact";

		public const string game_06_crushblock_activate = "event:/game/06_reflection/crushblock_activate";

		public const string game_06_crushblock_move_loop = "event:/game/06_reflection/crushblock_move_loop";

		public const string game_06_crushblock_move_loop_covert = "event:/game/06_reflection/crushblock_move_loop_covert";

		public const string game_06_crushblock_impact = "event:/game/06_reflection/crushblock_impact";

		public const string game_06_crushblock_return_loop = "event:/game/06_reflection/crushblock_return_loop";

		public const string game_06_crushblock_rest = "event:/game/06_reflection/crushblock_rest";

		public const string game_06_crushblock_rest_waypoint = "event:/game/06_reflection/crushblock_rest_waypoint";

		public const string game_06_badeline_freakout_1 = "event:/game/06_reflection/badeline_freakout_1";

		public const string game_06_badeline_freakout_2 = "event:/game/06_reflection/badeline_freakout_2";

		public const string game_06_badeline_freakout_3 = "event:/game/06_reflection/badeline_freakout_3";

		public const string game_06_badeline_freakout_4 = "event:/game/06_reflection/badeline_freakout_4";

		public const string game_06_badeline_freakout_5 = "event:/game/06_reflection/badeline_freakout_5";

		public const string game_06_badeline_featherslice = "event:/game/06_reflection/badeline_feather_slice";

		public const string game_06_badelinepull_whooshdown = "event:/game/06_reflection/badeline_pull_whooshdown";

		public const string game_06_badelinepull_impact = "event:/game/06_reflection/badeline_pull_impact";

		public const string game_06_badelinepull_rumble_loop = "event:/game/06_reflection/badeline_pull_rumble_loop";

		public const string game_06_badelinepull_cliffbreak = "event:/game/06_reflection/badeline_pull_cliffbreak";

		public const string game_06_fall_spike_smash = "event:/game/06_reflection/fall_spike_smash";

		public const string game_06_supersecret_torch_1 = "event:/game/06_reflection/supersecret_torch_1";

		public const string game_06_supersecret_torch_2 = "event:/game/06_reflection/supersecret_torch_2";

		public const string game_06_supersecret_torch_3 = "event:/game/06_reflection/supersecret_torch_3";

		public const string game_06_supersecret_torch_4 = "event:/game/06_reflection/supersecret_torch_4";

		public const string game_06_supersecret_dashflavour = "event:/game/06_reflection/supersecret_dashflavour";

		public const string game_06_supersecret_heartappear = "event:/game/06_reflection/supersecret_heartappear";

		public const string game_06_pinballbumper_hit = "event:/game/06_reflection/pinballbumper_hit";

		public const string game_06_pinballbumper_reset = "event:/game/06_reflection/pinballbumper_reset";

		public const string game_06_feather_get = "event:/game/06_reflection/feather_get";

		public const string game_06_feather_renew = "event:/game/06_reflection/feather_renew";

		public const string game_06_feather_reappear = "event:/game/06_reflection/feather_reappear";

		public const string game_06_feather_state_loop = "event:/game/06_reflection/feather_state_loop";

		public const string game_06_feather_state_warning = "event:/game/06_reflection/feather_state_warning";

		public const string game_06_feather_state_end = "event:/game/06_reflection/feather_state_end";

		public const string game_06_feather_state_bump = "event:/game/06_reflection/feather_state_bump";

		public const string game_06_feather_bubble_get = "event:/game/06_reflection/feather_bubble_get";

		public const string game_06_feather_bubble_renew = "event:/game/06_reflection/feather_bubble_renew";

		public const string game_06_feather_bubble_bounce = "event:/game/06_reflection/feather_bubble_bounce";

		public const string game_06_scaryhair_move = "event:/game/06_reflection/scaryhair_move";

		public const string game_06_scaryhair_whoosh = "event:/game/06_reflection/scaryhair_whoosh";

		public const string game_06_boss_spikes_burst = "event:/game/06_reflection/boss_spikes_burst";

		public const string game_06_hug_image_1 = "event:/game/06_reflection/hug_image_1";

		public const string game_06_hug_image_2 = "event:/game/06_reflection/hug_image_2";

		public const string game_06_hug_image_3 = "event:/game/06_reflection/hug_image_3";

		public const string game_06_hug_badelineglow = "event:/game/06_reflection/hug_badeline_glow";

		public const string game_06_hug_levelup_text_in = "event:/game/06_reflection/hug_levelup_text_in";

		public const string game_06_hug_levelup_text_out = "event:/game/06_reflection/hug_levelup_text_out";

		public const string game_07_altitudecount = "event:/game/07_summit/altitude_count";

		public const string game_07_checkpointconfetti = "event:/game/07_summit/checkpoint_confetti";

		public const string game_07_gem_get = "event:/game/07_summit/gem_get";

		public const string game_07_gem_unlock_1 = "event:/game/07_summit/gem_unlock_1";

		public const string game_07_gem_unlock_2 = "event:/game/07_summit/gem_unlock_2";

		public const string game_07_gem_unlock_3 = "event:/game/07_summit/gem_unlock_3";

		public const string game_07_gem_unlock_4 = "event:/game/07_summit/gem_unlock_4";

		public const string game_07_gem_unlock_5 = "event:/game/07_summit/gem_unlock_5";

		public const string game_07_gem_unlock_6 = "event:/game/07_summit/gem_unlock_6";

		public const string game_07_gem_unlock_complete = "event:/game/07_summit/gem_unlock_complete";

		public const string game_09_frontdoor_heartfill = "event:/game/09_core/frontdoor_heartfill";

		public const string game_09_frontdoor_unlock = "event:/game/09_core/frontdoor_unlock";

		public const string game_09_conveyor_activate = "event:/game/09_core/conveyor_activate";

		public const string game_09_hotpinball_activate = "event:/game/09_core/hotpinball_activate";

		public const string game_09_bounceblock_touch = "event:/game/09_core/bounceblock_touch";

		public const string game_09_bounceblock_break = "event:/game/09_core/bounceblock_break";

		public const string game_09_bounceblock_reappear = "event:/game/09_core/bounceblock_reappear";

		public const string game_09_iceblock_touch = "event:/game/09_core/iceblock_touch";

		public const string game_09_iceblock_reappear = "event:/game/09_core/iceblock_reappear";

		public const string game_09_switch_to_cold = "event:/game/09_core/switch_to_cold";

		public const string game_09_switch_to_hot = "event:/game/09_core/switch_to_hot";

		public const string game_09_switch_shutdown = "event:/game/09_core/switch_dies";

		public const string game_09_iceball_break = "event:/game/09_core/iceball_break";

		public const string game_09_pinballbumper_hit = "event:/game/09_core/pinballbumper_hit";

		public const string game_09_risingthreat_loop = "event:/game/09_core/rising_threat";

		public const string game_10_lightning_strike = "event:/new_content/game/10_farewell/lightning_strike";

		public const string game_10_heart_door = "event:/new_content/game/10_farewell/heart_door";

		public const string game_10_bird_camera_pan_up = "event:/new_content/game/10_farewell/bird_camera_pan_up";

		public const string game_10_bird_fly_uptonext = "event:/new_content/game/10_farewell/bird_fly_uptonext";

		public const string game_10_bird_startle = "event:/new_content/game/10_farewell/bird_startle";

		public const string game_10_bird_relocate = "event:/new_content/game/10_farewell/bird_relocate";

		public const string game_10_bird_wingflap = "event:/new_content/game/10_farewell/bird_wingflap";

		public const string game_10_bird_flyuproll = "event:/new_content/game/10_farewell/bird_flyuproll";

		public const string game_10_bird_throw = "event:/new_content/game/10_farewell/bird_throw";

		public const string game_10_bird_flappyscene_entry = "event:/new_content/game/10_farewell/bird_flappyscene_entry";

		public const string game_10_bird_flappyscene = "event:/new_content/game/10_farewell/bird_flappyscene";

		public const string game_10_bird_crashscene_start = "event:/new_content/game/10_farewell/bird_crashscene_start";

		public const string game_10_bird_crashscene_twitch_1 = "event:/new_content/game/10_farewell/bird_crashscene_twitch_1";

		public const string game_10_bird_crashscene_twitch_2 = "event:/new_content/game/10_farewell/bird_crashscene_twitch_2";

		public const string game_10_bird_crashscene_twitch_3 = "event:/new_content/game/10_farewell/bird_crashscene_twitch_3";

		public const string game_10_bird_crashscene_recover = "event:/new_content/game/10_farewell/bird_crashscene_recover";

		public const string game_10_bird_crashscene_relocate = "event:/new_content/game/10_farewell/bird_crashscene_relocate";

		public const string game_10_bird_crashscene_leave = "event:/new_content/game/10_farewell/bird_crashscene_leave";

		public const string game_10_pinkdiamond_touch = "event:/new_content/game/10_farewell/pinkdiamond_touch";

		public const string game_10_pinkdiamond_return = "event:/new_content/game/10_farewell/pinkdiamond_return";

		public const string game_10_puffer_shrink = "event:/new_content/game/10_farewell/puffer_shrink";

		public const string game_10_puffer_expand = "event:/new_content/game/10_farewell/puffer_expand";

		public const string game_10_puffer_boop = "event:/new_content/game/10_farewell/puffer_boop";

		public const string game_10_puffer_splode = "event:/new_content/game/10_farewell/puffer_splode";

		public const string game_10_puffer_reform = "event:/new_content/game/10_farewell/puffer_reform";

		public const string game_10_puffer_return = "event:/new_content/game/10_farewell/puffer_return";

		public const string game_10_quake_onset = "event:/new_content/game/10_farewell/quake_onset";

		public const string game_10_quake_rockbreak = "event:/new_content/game/10_farewell/quake_rockbreak";

		public const string game_10_locked_door_appear_1 = "event:/new_content/game/10_farewell/locked_door_appear_1";

		public const string game_10_locked_door_appear_2 = "event:/new_content/game/10_farewell/locked_door_appear_2";

		public const string game_10_locked_door_appear_3 = "event:/new_content/game/10_farewell/locked_door_appear_3";

		public const string game_10_locked_door_appear_4 = "event:/new_content/game/10_farewell/locked_door_appear_4";

		public const string game_10_locked_door_appear_5 = "event:/new_content/game/10_farewell/locked_door_appear_5";

		public const string game_10_key_unlock_1 = "event:/new_content/game/10_farewell/key_unlock_1";

		public const string game_10_key_unlock_2 = "event:/new_content/game/10_farewell/key_unlock_2";

		public const string game_10_key_unlock_3 = "event:/new_content/game/10_farewell/key_unlock_3";

		public const string game_10_key_unlock_4 = "event:/new_content/game/10_farewell/key_unlock_4";

		public const string game_10_key_unlock_5 = "event:/new_content/game/10_farewell/key_unlock_5";

		public const string game_10_glider_platform_dissipate = "event:/new_content/game/10_farewell/glider_platform_dissipate";

		public const string game_10_glider_wallbounce_left = "event:/new_content/game/10_farewell/glider_wallbounce_left";

		public const string game_10_glider_wallbounce_right = "event:/new_content/game/10_farewell/glider_wallbounce_right";

		public const string game_10_glider_land = "event:/new_content/game/10_farewell/glider_land";

		public const string game_10_glider_engage = "event:/new_content/game/10_farewell/glider_engage";

		public const string game_10_glider_movement = "event:/new_content/game/10_farewell/glider_movement";

		public const string game_10_glider_emancipate = "event:/new_content/game/10_farewell/glider_emancipate";

		public const string game_10_fusebox_hit_1 = "event:/new_content/game/10_farewell/fusebox_hit_1";

		public const string game_10_fusebox_hit_2 = "event:/new_content/game/10_farewell/fusebox_hit_2";

		public const string game_10_fakeheart_get = "event:/new_content/game/10_farewell/fakeheart_get";

		public const string game_10_fakeheart_pulse = "event:/new_content/game/10_farewell/fakeheart_pulse";

		public const string game_10_fakeheart_bounce = "event:/new_content/game/10_farewell/fakeheart_bounce";

		public const string game_10_glitch_short = "event:/new_content/game/10_farewell/glitch_short";

		public const string game_10_glitch_medium = "event:/new_content/game/10_farewell/glitch_medium";

		public const string game_10_glitch_long = "event:/new_content/game/10_farewell/glitch_long";

		public const string game_10_zip_mover = "event:/new_content/game/10_farewell/zip_mover";

		public const string game_10_pico8_flag = "event:/new_content/game/10_farewell/pico8_flag";

		public const string game_10_strawberry_gold_detach = "event:/new_content/game/10_farewell/strawberry_gold_detach";

		public const string game_10_cafe_computer_on = "event:/new_content/game/10_farewell/cafe_computer_on";

		public const string game_10_cafe_computer_startupsfx = "event:/new_content/game/10_farewell/cafe_computer_startupsfx";

		public const string game_10_cafe_computer_off = "event:/new_content/game/10_farewell/cafe_computer_off";

		public const string game_10_ppt_mouseclick = "event:/new_content/game/10_farewell/ppt_mouseclick";

		public const string game_10_ppt_doubleclick = "event:/new_content/game/10_farewell/ppt_doubleclick";

		public const string game_10_ppt_cube_transition = "event:/new_content/game/10_farewell/ppt_cube_transition";

		public const string game_10_ppt_dissolve_transition = "event:/new_content/game/10_farewell/ppt_dissolve_transition";

		public const string game_10_ppt_happy_wavedashing = "event:/new_content/game/10_farewell/ppt_happy_wavedashing";

		public const string game_10_ppt_impossible = "event:/new_content/game/10_farewell/ppt_impossible";

		public const string game_10_ppt_its_easy = "event:/new_content/game/10_farewell/ppt_its_easy";

		public const string game_10_ppt_spinning_transition = "event:/new_content/game/10_farewell/ppt_spinning_transition";

		public const string game_10_ppt_wavedash_whoosh = "event:/new_content/game/10_farewell/ppt_wavedash_whoosh";

		public const string game_10_endscene_dial_theo = "event:/new_content/game/10_farewell/endscene_dial_theo";

		public const string game_10_endscene_attachment_notify = "event:/new_content/game/10_farewell/endscene_attachment_notify";

		public const string game_10_endscene_attachment_click = "event:/new_content/game/10_farewell/endscene_attachment_click";

		public const string game_10_endscene_photozoom = "event:/new_content/game/10_farewell/endscene_photo_zoom";

		public const string game_10_endscene_final_input = "event:/new_content/game/10_farewell/endscene_final_input";

		public const string game_10_timeline_bubble_to_remembered = "event:/new_content/timeline_bubble_to_remembered";

		public const string env_amb_worldmap = "event:/env/amb/worldmap";

		public const string env_amb_00_main = "event:/env/amb/00_prologue";

		public const string env_amb_01_main = "event:/env/amb/01_main";

		public const string env_amb_02_dream = "event:/env/amb/02_dream";

		public const string env_amb_02_awake = "event:/env/amb/02_awake";

		public const string env_amb_03_exterior = "event:/env/amb/03_exterior";

		public const string env_amb_03_interior = "event:/env/amb/03_interior";

		public const string env_amb_03_pico8_closeup = "event:/env/amb/03_pico8_closeup";

		public const string env_amb_04_main = "event:/env/amb/04_main";

		public const string env_amb_05_interior_main = "event:/env/amb/05_interior_main";

		public const string env_amb_05_interior_dark = "event:/env/amb/05_interior_dark";

		public const string env_amb_05_mirror_sequence = "event:/env/amb/05_mirror_sequence";

		public const string env_amb_06_lake = "event:/env/amb/06_lake";

		public const string env_amb_06_main = "event:/env/amb/06_main";

		public const string env_amb_06_prehug = "event:/env/amb/06_prehug";

		public const string env_amb_09_main = "event:/env/amb/09_main";

		public const string env_amb_10_rain = "event:/new_content/env/10_rain";

		public const string env_amb_10_electricity = "event:/new_content/env/10_electricity";

		public const string env_amb_10_endscene = "event:/new_content/env/10_endscene";

		public const string env_amb_10_rushingvoid = "event:/new_content/env/10_rushingvoid";

		public const string env_amb_10_space_underwater = "event:/new_content/env/10_space_underwater";

		public const string env_amb_10_voidspiral = "event:/new_content/env/10_voidspiral";

		public const string env_amb_10_grannyclouds = "event:/new_content/env/10_grannyclouds";

		public const string env_state_underwater = "event:/env/state/underwater";

		public const string env_loc_campfire_start = "event:/env/local/campfire_start";

		public const string env_loc_campfire_loop = "event:/env/local/campfire_loop";

		public const string env_loc_waterfall_big_main = "event:/env/local/waterfall_big_main";

		public const string env_loc_waterfall_big_in = "event:/env/local/waterfall_big_in";

		public const string env_loc_waterfall_small_main = "event:/env/local/waterfall_small_main";

		public const string env_loc_waterfall_small_in_deep = "event:/env/local/waterfall_small_in_deep";

		public const string env_loc_waterfall_small_in_shallow = "event:/env/local/waterfall_small_in_shallow";

		public const string env_loc_02_lamp = "event:/env/local/02_old_site/phone_lamp";

		public const string env_loc_03_pico8machine_loop = "event:/env/local/03_resort/pico8_machine";

		public const string env_loc_03_brokenwindow_large_loop = "event:/env/local/03_resort/broken_window_large";

		public const string env_loc_03_brokenwindow_small_loop = "event:/env/local/03_resort/broken_window_small";

		public const string env_loc_07_flag_flap = "event:/env/local/07_summit/flag_flap";

		public const string env_loc_09_conveyer_idle = "event:/env/local/09_core/conveyor_idle";

		public const string env_loc_09_lavagate_idle = "event:/env/local/09_core/lavagate_idle";

		public const string env_loc_09_fireball_idle = "event:/env/local/09_core/fireballs_idle";

		public const string env_loc_10_cafe_computer = "event:/new_content/env/local/cafe_computer";

		public const string env_loc_10_cafe_sign = "event:/new_content/env/local/cafe_sign";

		public const string env_loc_10_tutorial_static_left = "event:/new_content/env/local/tutorial_static_left";

		public const string env_loc_10_tutorial_static_right = "event:/new_content/env/local/tutorial_static_right";

		public const string env_loc_10_kevinpc = "event:/new_content/env/local/kevinpc";

		public const string state_cafe_computer_active = "event:/state/cafe_computer_active";

		public const string ui_main_postcard_ch1_in = "event:/ui/main/postcard_ch1_in";

		public const string ui_main_postcard_ch1_out = "event:/ui/main/postcard_ch1_out";

		public const string ui_main_postcard_ch2_in = "event:/ui/main/postcard_ch2_in";

		public const string ui_main_postcard_ch2_out = "event:/ui/main/postcard_ch2_out";

		public const string ui_main_postcard_ch3_in = "event:/ui/main/postcard_ch3_in";

		public const string ui_main_postcard_ch3_out = "event:/ui/main/postcard_ch3_out";

		public const string ui_main_postcard_ch4_in = "event:/ui/main/postcard_ch4_in";

		public const string ui_main_postcard_ch4_out = "event:/ui/main/postcard_ch4_out";

		public const string ui_main_postcard_ch5_in = "event:/ui/main/postcard_ch5_in";

		public const string ui_main_postcard_ch5_out = "event:/ui/main/postcard_ch5_out";

		public const string ui_main_postcard_ch6_in = "event:/ui/main/postcard_ch6_in";

		public const string ui_main_postcard_ch6_out = "event:/ui/main/postcard_ch6_out";

		public const string ui_main_postcard_csides_in = "event:/ui/main/postcard_csides_in";

		public const string ui_main_postcard_csides_out = "event:/ui/main/postcard_csides_out";

		public const string ui_main_postcard_variants_in = "event:/new_content/ui/postcard_variants_in";

		public const string ui_main_postcard_variants_out = "event:/new_content/ui/postcard_variants_out";

		public const string ui_main_title_firstinput = "event:/ui/main/title_firstinput";

		public const string ui_main_roll_down = "event:/ui/main/rollover_down";

		public const string ui_main_roll_up = "event:/ui/main/rollover_up";

		public const string ui_main_button_select = "event:/ui/main/button_select";

		public const string ui_main_button_back = "event:/ui/main/button_back";

		public const string ui_main_button_climb = "event:/ui/main/button_climb";

		public const string ui_main_button_toggle_on = "event:/ui/main/button_toggle_on";

		public const string ui_main_button_toggle_off = "event:/ui/main/button_toggle_off";

		public const string ui_main_button_invalid = "event:/ui/main/button_invalid";

		public const string ui_main_button_lowkey = "event:/ui/main/button_lowkey";

		public const string ui_main_whoosh_large_in = "event:/ui/main/whoosh_large_in";

		public const string ui_main_whoosh_large_out = "event:/ui/main/whoosh_large_out";

		public const string ui_main_whoosh_list_in = "event:/ui/main/whoosh_list_in";

		public const string ui_main_whoosh_list_out = "event:/ui/main/whoosh_list_out";

		public const string ui_main_whoosh_savefile_in = "event:/ui/main/whoosh_savefile_in";

		public const string ui_main_whoosh_savefile_out = "event:/ui/main/whoosh_savefile_out";

		public const string ui_main_savefile_roll_down = "event:/ui/main/savefile_rollover_down";

		public const string ui_main_savefile_roll_up = "event:/ui/main/savefile_rollover_up";

		public const string ui_main_savefile_roll_first = "event:/ui/main/savefile_rollover_first";

		public const string ui_main_savefile_rename_start = "event:/ui/main/savefile_rename_start";

		public const string ui_main_savefile_delete = "event:/ui/main/savefile_delete";

		public const string ui_main_savefile_begin = "event:/ui/main/savefile_begin";

		public const string ui_main_message_confirm = "event:/ui/main/message_confirm";

		public const string ui_main_rename_entry_roll = "event:/ui/main/rename_entry_rollover";

		public const string ui_main_rename_entry_char = "event:/ui/main/rename_entry_char";

		public const string ui_main_rename_entry_space = "event:/ui/main/rename_entry_space";

		public const string ui_main_rename_entry_backspace = "event:/ui/main/rename_entry_backspace";

		public const string ui_main_rename_entry_accept = "event:/ui/main/rename_entry_accept";

		public const string ui_main_rename_entry_accept_locked = "event:/new_content/ui/rename_entry_accept_locked";

		public const string ui_main_assist_button_info = "event:/ui/main/assist_button_info";

		public const string ui_main_assist_info_whistle = "event:/ui/main/assist_info_whistle";

		public const string ui_main_assist_button_yes = "event:/ui/main/assist_button_yes";

		public const string ui_main_assist_button_no = "event:/ui/main/assist_button_no";

		public const string ui_main_bside_intro_text = "event:/ui/main/bside_intro_text";

		public const string ui_game_pause = "event:/ui/game/pause";

		public const string ui_game_unpause = "event:/ui/game/unpause";

		public const string ui_game_tutorialnote_flip_back = "event:/ui/game/tutorial_note_flip_back";

		public const string ui_game_tutorialnote_flip_front = "event:/ui/game/tutorial_note_flip_front";

		public const string ui_game_hotspot_main_in = "event:/ui/game/hotspot_main_in";

		public const string ui_game_hotspot_main_out = "event:/ui/game/hotspot_main_out";

		public const string ui_game_hotspot_note_in = "event:/ui/game/hotspot_note_in";

		public const string ui_game_hotspot_note_out = "event:/ui/game/hotspot_note_out";

		public const string ui_game_general_text_loop = "event:/ui/game/general_text_loop";

		public const string ui_game_memorial_text_in = "event:/ui/game/memorial_text_in";

		public const string ui_game_memorial_text_loop = "event:/ui/game/memorial_text_loop";

		public const string ui_game_memorial_text_out = "event:/ui/game/memorial_text_out";

		public const string ui_game_memorialdream_text_in = "event:/ui/game/memorial_dream_text_in";

		public const string ui_game_memorialdream_text_loop = "event:/ui/game/memorial_dream_text_loop";

		public const string ui_game_memorialdream_text_out = "event:/ui/game/memorial_dream_text_out";

		public const string ui_game_memorialdream_loop = "event:/ui/game/memorial_dream_loop";

		public const string ui_game_lookout_on = "event:/ui/game/lookout_on";

		public const string ui_game_lookout_off = "event:/ui/game/lookout_off";

		public const string ui_game_chatoptions_roll_up = "event:/ui/game/chatoptions_roll_up";

		public const string ui_game_chatoptions_roll_down = "event:/ui/game/chatoptions_roll_down";

		public const string ui_game_chatoptions_appear = "event:/ui/game/chatoptions_appear";

		public const string ui_game_chatoptions_select = "event:/ui/game/chatoptions_select";

		public const string ui_game_textbox_madeline_in = "event:/ui/game/textbox_madeline_in";

		public const string ui_game_textbox_madeline_out = "event:/ui/game/textbox_madeline_out";

		public const string ui_game_textbox_other_in = "event:/ui/game/textbox_other_in";

		public const string ui_game_textbox_other_out = "event:/ui/game/textbox_other_out";

		public const string ui_game_textadvance_madeline = "event:/ui/game/textadvance_madeline";

		public const string ui_game_textadvance_other = "event:/ui/game/textadvance_other";

		public const string ui_game_increment_strawberry = "event:/ui/game/increment_strawberry";

		public const string ui_game_increment_dashcount = "event:/ui/game/increment_dashcount";

		public const string ui_world_icon_roll_right = "event:/ui/world_map/icon/roll_right";

		public const string ui_world_icon_roll_left = "event:/ui/world_map/icon/roll_left";

		public const string ui_world_icon_select = "event:/ui/world_map/icon/select";

		public const string ui_world_icon_flip_right = "event:/ui/world_map/icon/flip_right";

		public const string ui_world_icon_flip_left = "event:/ui/world_map/icon/flip_left";

		public const string ui_world_icon_assistskip = "event:/ui/world_map/icon/assist_skip";

		public const string ui_world_whoosh_400ms_forward = "event:/ui/world_map/whoosh/400ms_forward";

		public const string ui_world_whoosh_400ms_back = "event:/ui/world_map/whoosh/400ms_back";

		public const string ui_world_whoosh_600ms_forward = "event:/ui/world_map/whoosh/600ms_forward";

		public const string ui_world_whoosh_600ms_back = "event:/ui/world_map/whoosh/600ms_back";

		public const string ui_world_whoosh_700ms_forward = "event:/ui/world_map/whoosh/700ms_forward";

		public const string ui_world_whoosh_700ms_back = "event:/ui/world_map/whoosh/700ms_back";

		public const string ui_world_whoosh_1000ms_forward = "event:/ui/world_map/whoosh/1000ms_forward";

		public const string ui_world_whoosh_1000ms_back = "event:/ui/world_map/whoosh/1000ms_back";

		public const string ui_world_whoosh_900ms_forward = "event:/ui/world_map/whoosh/900ms_forward";

		public const string ui_world_whoosh_900ms_back = "event:/ui/world_map/whoosh/900ms_back";

		public const string ui_world_chapter_back = "event:/ui/world_map/chapter/back";

		public const string ui_world_chapter_pane_expand = "event:/ui/world_map/chapter/pane_expand";

		public const string ui_world_chapter_pane_contract = "event:/ui/world_map/chapter/pane_contract";

		public const string ui_world_chapter_tab_roll_right = "event:/ui/world_map/chapter/tab_roll_right";

		public const string ui_world_chapter_tab_roll_left = "event:/ui/world_map/chapter/tab_roll_left";

		public const string ui_world_chapter_level_select = "event:/ui/world_map/chapter/level_select";

		public const string ui_world_chapter_checkpoint_back = "event:/ui/world_map/chapter/checkpoint_back";

		public const string ui_world_chapter_checkpoint_photo_add = "event:/ui/world_map/chapter/checkpoint_photo_add";

		public const string ui_world_chapter_checkpoint_photo_remove = "event:/ui/world_map/chapter/checkpoint_photo_remove";

		public const string ui_world_chapter_checkpoint_start = "event:/ui/world_map/chapter/checkpoint_start";

		public const string ui_world_journal_select = "event:/ui/world_map/journal/select";

		public const string ui_world_journal_back = "event:/ui/world_map/journal/back";

		public const string ui_world_journal_page_cover_forward = "event:/ui/world_map/journal/page_cover_forward";

		public const string ui_world_journal_page_cover_back = "event:/ui/world_map/journal/page_cover_back";

		public const string ui_world_journal_page_main_forward = "event:/ui/world_map/journal/page_main_forward";

		public const string ui_world_journal_page_main_back = "event:/ui/world_map/journal/page_main_back";

		public const string ui_world_journal_heart_roll = "event:/ui/world_map/journal/heart_roll";

		public const string ui_world_journal_heart_grab = "event:/ui/world_map/journal/heart_grab";

		public const string ui_world_journal_heart_release = "event:/ui/world_map/journal/heart_release";

		public const string ui_world_journal_heart_shift_up = "event:/ui/world_map/journal/heart_shift_up";

		public const string ui_world_journal_heart_shift_down = "event:/ui/world_map/journal/heart_shift_down";

		public const string ui_postgame_crystalheart = "event:/ui/postgame/crystal_heart";

		public const string ui_postgame_strawberry_count = "event:/ui/postgame/strawberry_count";

		public const string ui_postgame_goldberry_count = "event:/ui/postgame/goldberry_count";

		public const string ui_postgame_strawberry_total = "event:/ui/postgame/strawberry_total";

		public const string ui_postgame_strawberry_total_all = "event:/ui/postgame/strawberry_total_all";

		public const string ui_postgame_death_appear = "event:/ui/postgame/death_appear";

		public const string ui_postgame_death_count = "event:/ui/postgame/death_count";

		public const string ui_postgame_death_final = "event:/ui/postgame/death_final";

		public const string ui_postgame_unlock_newchapter = "event:/ui/postgame/unlock_newchapter";

		public const string ui_postgame_unlock_newchapter_icon = "event:/ui/postgame/unlock_newchapter_icon";

		public const string ui_postgame_unlock_bside = "event:/ui/postgame/unlock_bside";

		public const string ui_postgame_skip_all = "event:/new_content/ui/skip_all";

		private static Dictionary<string, string> byHandle = new Dictionary<string, string>();

		public static Dictionary<string, string> MadelineToBadelineSound = new Dictionary<string, string>();

		public static void Initialize()
		{
			FieldInfo[] fields = typeof(SFX).GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo field in fields)
			{
				if (field.FieldType == typeof(string))
				{
					string value = field.GetValue(null).ToString();
					byHandle.Add(field.Name, value);
					if (value.StartsWith("event:/char/madeline/"))
					{
						MadelineToBadelineSound.Add(value, value.Replace("madeline", "badeline"));
					}
				}
			}
			MadelineToBadelineSound.Add("event:/game/general/assist_screenbottom", "event:/game/general/assist_screenbottom");
		}

		public static string EventnameByHandle(string handle)
		{
			string eventName = "";
			byHandle.TryGetValue(handle, out eventName);
			return eventName;
		}
	}
}
