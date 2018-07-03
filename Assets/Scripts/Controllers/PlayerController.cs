using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Spine;
using UnityEngine.UI;
using Spine.Unity;

using Dragonling.Concepts;
using Dragonling.Utility;

namespace Dragonling.Controllers {

    public class PlayerController : AnimatedEntity {
        private float _Acceleration = 200F;
        private float _JumpStrength = 100F;
        private float _FlightDragDiff = 2F;

        private FireBreathController FireBreath;
        private Rigidbody2D Body;

        public bool Moving;
        public bool Airborne;
        public bool Flying;

        private DebugUI DebugUI;

        new private static class Anim {
            public const string UNNAMED = "???";
            public const string SHOWREEL = "___showreel";
            public const string ATTACK_BASIC_STATIC = "attack_basic ( static )";
            public const string ATTACK_BASIC_MOVING = "attack_basic ( combo )";
            public const string ATTACK_STRONG_STATIC = "attack_strong ( static )";
            public const string ATTACK_STRONG_MOVING = "attack_strong ( combo )";
            public const string IDLE_NORMAL = "**idle_standard";
            public const string IDLE_BATTLE = "**idle_battle";
            public const string INTERACT_LOOT = "interact_loot";
            public const string MOVE_JUMPING = "move_jumping";
            public const string MOVE_RUNNING_END_LONG = "move_running_end  ( > 1x move_running_loop )";
            public const string MOVE_RUNNING_END_SHORT = "move_running_end ( <= 1x move_running_loop )";
            public const string MOVE_RUNNING_LOOP = "move_running_loop";
            public const string MOVE_RUNNING_START = "move_running_start";
            public const string TAKING_OFF = "**taking off";
        }

        void Start() {
            Init();
        }

        private void Init() {
            DebugUI = FindObjectOfType<Canvas>().GetComponent<DebugUI>();

            SkeletonAnimation = GetComponent<SkeletonAnimation>();
            AnimationState = SkeletonAnimation.AnimationState;
            Body = GetComponent<Rigidbody2D>();
            FireBreath = GetComponentInChildren<FireBreathController>();

            SetAnim(AnimTrack.Movement, Anim.IDLE_NORMAL, true);
        }

        void Update() {
            HandleInput();
        }

        private void FixedUpdate() {
            ResolveMovement();
        }

        private void HandleInput() {
            if (Input.anyKey) {
                if (Input.GetKeyDown(KeyCode.W)) {
                    if (!Flying && !Airborne && !AnimationLock(AnimTrack.Movement))
                        Jump();
                }
                if (Input.GetKeyDown(KeyCode.A)) {
                    Stop(false);
                    MoveBackward();

                } else if (Input.GetKeyDown(KeyCode.D)) {
                    Stop(false);
                    MoveForward();
                } else if (Input.GetKeyDown(KeyCode.S)) {
                    if (!Flying && !Airborne && !AnimationLock(AnimTrack.Movement))
                        Duck();
                }
                if (Input.GetKeyDown(KeyCode.Space)) {
                    if (!Flying && !Airborne)
                        AttackBasic();
                }
                if (Input.GetKeyDown(KeyCode.LeftAlt)) {
                    if (!Flying && !Airborne)
                        AttackStrong();
                }
                if (Input.GetKeyDown(KeyCode.F)) {
                    if (Flying) {
                        Land();
                    } else {
                        Fly();
                    }
                }
            } else {
                if (!AnimationLock(AnimTrack.Movement) && !Flying && !Airborne) {
                    Stop(true);
                }
            }
        }

        private void ResolveMovement() {
            Moving = Body.velocity.x != 0F;
            Airborne = Body.velocity.y != 0F;

            if (Input.anyKey) {
                if (Input.GetKey(KeyCode.W)) {

                }
                if (Input.GetKey(KeyCode.A)) {
                    Body.AddForce(new Vector2(-_Acceleration, 0), ForceMode2D.Force);
                } else if (Input.GetKey(KeyCode.D)) {
                    Body.AddForce(new Vector2(_Acceleration, 0), ForceMode2D.Force);
                }
            }
            if (Input.GetKey(KeyCode.S)) {

            }
        }

        private void Flip(bool flipX) {
            SkeletonAnimation.Skeleton.FlipX = flipX;
        }
        public bool Flipped() {
            return SkeletonAnimation.Skeleton.FlipX;
        }

        private bool AnimationLock(AnimTrack track) {
            if (GetCurrentAnim(track) == null)
                return false;
            switch (GetCurrentAnim(track).Name) {
                case Anim.TAKING_OFF:
                case Anim.ATTACK_STRONG_STATIC:
                case Anim.ATTACK_STRONG_MOVING:
                case Anim.MOVE_JUMPING:
                case Anim.INTERACT_LOOT:
                    DebugUI.Flash_AnimBlock();
                    return true;
                default:
                    return false;
            }
        }

        private bool MovementLock() {
            switch (GetCurrentAnim(AnimTrack.Attack).Name) {
                case Anim.ATTACK_STRONG_STATIC:
                case Anim.TAKING_OFF:
                    return true;
                default:
                    return false;
            }
        }

        private void Fly() {
            Body.gravityScale = 0;
            Body.AddForce(new Vector2(0, _JumpStrength * 2), ForceMode2D.Impulse);
            Body.drag -= _FlightDragDiff;
            SetAnim(AnimTrack.Movement, Anim.TAKING_OFF, true);
            Flying = true;
        }

        private void Land() {
            Body.gravityScale = 1;
            Body.AddForce(new Vector2(0, _JumpStrength * -3), ForceMode2D.Impulse);
            Body.AddForce(new Vector2(0, -_JumpStrength), ForceMode2D.Force);
            Body.drag += _FlightDragDiff;
            SetAnim(AnimTrack.Movement, Anim.IDLE_NORMAL, true);
            Flying = false;
        }

        private void AttackBasic() {
            if (Moving) {
                SetAnim(AnimTrack.Attack, Anim.ATTACK_BASIC_MOVING, false);
            } else {
                SetAnim(AnimTrack.Attack, Anim.ATTACK_BASIC_STATIC, false);
            }
            AddAnimEmpty(AnimTrack.Attack, 0);
        }

        private void AttackStrong() {
            FireBreath.SetStartDelay(Moving ? 0.1F : 0.3F);
            FireBreath.SetSize(Moving ? 0.75F : 1);
            FireBreath.SetShapeScale(Moving ? new Vector3(1, 0.2F, 1) : new Vector3(1, 0.5F, 1));
            SetAnim(AnimTrack.Attack, Moving ? Anim.ATTACK_STRONG_MOVING : Anim.ATTACK_STRONG_STATIC, false);
            AddAnimEmpty(AnimTrack.Attack, 0);
            FireBreath.Activate();
        }

        private void MoveForward() {
            Flip(false);
            Move();
        }

        private void MoveBackward() {
            Flip(true);
            Move();
        }

        private void Move() {
            if (!Flying && !Airborne && !AnimationLock(AnimTrack.Movement)) {
                if (!Moving) {
                    ClearAnim(AnimTrack.Movement);
                    SetAnim(AnimTrack.Movement, Anim.MOVE_RUNNING_START, false);
                    AddAnim(AnimTrack.Movement, Anim.MOVE_RUNNING_LOOP, true);
                } else {
                    ClearAnim(AnimTrack.Movement);
                    SetAnim(AnimTrack.Movement, Anim.MOVE_RUNNING_LOOP, true);
                }
            }
        }

        private void Stop(bool goIdle) {
            if (!Airborne) {
                if (goIdle) {
                    if (GetCurrentAnim(AnimTrack.Movement).Name != Anim.IDLE_NORMAL
                         && GetCurrentAnim(AnimTrack.Movement).Name != Anim.MOVE_RUNNING_END_SHORT
                         && GetCurrentAnim(AnimTrack.Movement).Name != Anim.MOVE_RUNNING_END_LONG) {
                        DebugUI.Flash_IdleStart();
                        if (!Moving) {
                            SetAnim(AnimTrack.Movement, Anim.IDLE_NORMAL, true);
                        } else {
                            if (CurrectTrackEntry(AnimTrack.Movement).TrackTime < GetCurrentAnim(AnimTrack.Movement).Duration) {
                                SetAnim(AnimTrack.Movement, Anim.MOVE_RUNNING_END_SHORT, false);
                            } else {
                                SetAnim(AnimTrack.Movement, Anim.MOVE_RUNNING_END_LONG, false);
                            }
                            AddAnim(AnimTrack.Movement, Anim.IDLE_NORMAL, true);
                        }
                    }
                } else {
                    DebugUI.Flash_IdleStop();
                    ClearAnim(AnimTrack.Movement);
                }
            }
        }

        private void Jump() {
            Body.AddForce(new Vector2(0, _JumpStrength), ForceMode2D.Impulse);
            SetAnim(AnimTrack.Movement, Anim.MOVE_JUMPING, false);
            Airborne = true;
            CurrectTrackEntry(AnimTrack.Movement).End += delegate {
                Airborne = false;
            };
            AddAnim(AnimTrack.Movement, Moving ? Anim.MOVE_RUNNING_LOOP : Anim.IDLE_NORMAL, true);
        }

        private void Duck() {
            SetAnim(AnimTrack.Movement, Anim.INTERACT_LOOT, false);
            AddAnim(AnimTrack.Movement, Moving ? Anim.MOVE_RUNNING_LOOP : Anim.IDLE_NORMAL, true);
        }
    }

}