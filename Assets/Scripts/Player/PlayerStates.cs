using UnityEngine;

public enum playerStates
{
    Idle, Run, Slide, Falling, Rising, Hurt, WallSlide, Attack, AerialAttack, Throw, Crouch,
    IdleWep, RisingWep, FallingWep, RunWep, SlideWep, HurtWep, WallSlideWep, CrouchWep,
    Melee1, Melee1_Recovery, Melee2, Melee2_Recovery, Melee3, Melee3_Recovery,
    Dash, DashWep, RangedAttack, CrouchAttack, Punch1, Punch2, Death, DeathWep, Climb, 
    CrouchRangedAttack, Reload,
    // Legacy/alternative names for compatibility
    Knife1, Knife2, Knife3, Knife1_Recovery, Knife2_Recovery, Knife3_Recovery,
    Gun1, AttackRecovery
}

