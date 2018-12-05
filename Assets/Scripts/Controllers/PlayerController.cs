﻿using System.Collections;
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
        private StateManager stateManager;

        private readonly float _Acceleration = 200F;
        private readonly float _JumpStrength = 100F;
        private readonly float _FlightDragDiff = 2F;
        private readonly Vector3 _CameraOffset = new Vector3(4, 4, -10);

        private Vector3 CameraOffset;

        private FireBreathController FireBreath;
        private Rigidbody2D Body;
        private PolygonCollider2D Collider;
        private CameraController Camera;

        public bool IsMoving;
        public bool IsAirborne;
        public bool IsFlying;
        public bool IsFlipped;

        private Dictionary<string, AudioClip> AudioClips;
        private AudioSource AudioEmitter;

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

            InitializeComponents();

            Camera = GetComponentInChildren<CameraController>();
            Camera.Init(Body);

            InitializeAudioEmitter();

            Flip(false);
            SetAnim(AnimTrack.Movement, Anim.IDLE_NORMAL, true);

            inputBuffer = new List<KeyPress>();

            InitializeStateManager();
        }

        private void InitializeStateManager() {
            stateManager = new StateManager(AnimationState);

            stateManager.SetIdleAnimation(Anim.IDLE_NORMAL);

            stateManager.RegisterState(
                Anim.MOVE_RUNNING_LOOP,
                Anim.MOVE_RUNNING_END_SHORT,
                (int)AnimTrack.Movement,
                new string[] { Anim.MOVE_JUMPING },
                KeyCode.D,
                KeyMode.Hold,
                AnimationMode.Looping
                );
            stateManager.RegisterState(
                Anim.MOVE_JUMPING,
                "",
                (int)AnimTrack.Movement,
                new string[] { },
                KeyCode.W,
                KeyMode.OneShot,
                AnimationMode.Once
                );
        }

        private void InitializeComponents() {
            SkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            AnimationState = SkeletonAnimation.AnimationState;
            Body = GetComponentInChildren<Rigidbody2D>();
            Collider = GetComponentInChildren<PolygonCollider2D>();
            FireBreath = GetComponentInChildren<FireBreathController>();
        }

        private void InitializeAudioEmitter() {
            AudioEmitter = GetComponentInChildren<AudioSource>();
            AudioClips = new Dictionary<string, AudioClip>();
            //FIXME: solchen scheiß in ne art ressource-helper auslagern:
            AudioClips.Add("step", Resources.Load<AudioClip>("Sounds/dragonling/step"));
            AudioClips.Add("stop_long", Resources.Load<AudioClip>("Sounds/dragonling/stop_long"));
            AudioClips.Add("stop_short", Resources.Load<AudioClip>("Sounds/dragonling/stop_short"));
        }

        void Update() {
            //RegisterInput();
            //ProcessInputBuffer();
            stateManager.ProcessStates();
        }

        private void FixedUpdate() {
            //ResolveMovement();
            //UpdateColiderPosition();
            //if (IsMoving)
            //    Camera.MoveTo(CameraOffset + Body.transform.position);
        }

        private void UpdateColiderPosition() {
            Bone bone = SkeletonAnimation.Skeleton.FindBone("body");
            Vector2 colliderOffset = new Vector2(0, bone.GetSkeletonSpacePosition().y - Collider.bounds.size.y);
            Collider.offset = colliderOffset;
        }

        private List<KeyPress> inputBuffer;
        private KeyCode currentKeyDown;
        private readonly KeyCode[] validInput = new KeyCode[] {
            KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space, KeyCode.LeftAlt, KeyCode.F
            };
        struct KeyPress {
            public bool Down;
            public KeyCode Key;
        }
        private void RegisterInput() {
            if (inputBuffer.Count == 0) {
                inputBuffer.Add(new KeyPress { Key = KeyCode.None, Down = true });
            }
            if (inputBuffer.Count > 2) {
                inputBuffer.RemoveRange(2, 1);
            }
            KeyPress first;

            if (Input.anyKey) {
                if (Input.GetKeyUp(currentKeyDown)) {
                    first = inputBuffer.First();
                    inputBuffer.Remove(first);
                    currentKeyDown = first.Key;
                }

                foreach (KeyCode key in validInput) {
                    if (Input.GetKey(key)) {
                        var press = new KeyPress { Key = key };

                        if (Input.GetKeyDown(key)) {
                            press.Down = true;
                        } else {
                            press.Down = false;
                        }

                        this.inputBuffer.Insert(0, press);
                    }
                };
            }
            first = inputBuffer.First();
            inputBuffer.Remove(first);
            currentKeyDown = first.Key;
        }


        /*


            BAU EINFACH ALLES AUF FEST DEFINIERTE STATES UM
            KOMBINATIONEN AUS UNTER-ZUSTÄNDEN
            ODER SO EIN MIST
    */

        private void ProcessInputBuffer() {
            switch(currentKeyDown) {
                case (KeyCode.W):
                    if (!IsFlying && !IsAirborne)
                        Jump();
                    break;
                case (KeyCode.A):
                    MoveBackward();
                    break;
                case (KeyCode.D):
                    MoveForward();
                    break;
                case (KeyCode.S):
                    if (!IsFlying && !IsAirborne)
                        Duck();
                    break;
                case (KeyCode.Space):
                    if (!IsFlying && !IsAirborne)
                        AttackBasic();
                    break;
                case (KeyCode.LeftAlt):
                    if (!IsFlying && !IsAirborne)
                        AttackStrong();
                    break;
                case (KeyCode.F):
                    if (IsFlying) {
                        Land();
                    } else {
                        Fly();
                    }
                    break;
                default:
                    if (GetCurrentAnim(AnimTrack.Movement).Name != Anim.MOVE_JUMPING && !IsFlying && !IsAirborne) {
                        Stop(true);
                    }
                    break;
            }
        }

        private void ResolveMovement() {
            IsMoving = Body.velocity.x != 0F;
            IsAirborne = Body.velocity.y != 0F;

            if (currentKeyDown == KeyCode.A) {
                Body.AddForce(new Vector2(-_Acceleration, 0), ForceMode2D.Force);
            } else if (currentKeyDown == KeyCode.D) {
                Body.AddForce(new Vector2(_Acceleration, 0), ForceMode2D.Force);
            }
        }

        private void Flip(bool flipX) {
            Collider.transform.rotation = Quaternion.LookRotation(flipX ? Vector3.back : Vector3.forward, Vector3.up);
            IsFlipped = flipX;
            CameraOffset = _CameraOffset;
            if (flipX)
                CameraOffset.Scale(new Vector3(-1, 1, 1));
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
            IsFlying = true;
        }

        private void Land() {
            Body.gravityScale = 1;
            Body.AddForce(new Vector2(0, _JumpStrength * -3), ForceMode2D.Impulse);
            Body.AddForce(new Vector2(0, -_JumpStrength), ForceMode2D.Force);
            Body.drag += _FlightDragDiff;
            SetAnim(AnimTrack.Movement, Anim.IDLE_NORMAL, true);
            IsFlying = false;
        }

        private void AttackBasic() {
            SetAnim(AnimTrack.Attack, IsMoving ? Anim.ATTACK_BASIC_MOVING : Anim.ATTACK_BASIC_STATIC, false);
            AddAnimEmpty(AnimTrack.Attack, 0);
        }

        private void AttackStrong() {
            FireBreath.SetStartDelay(IsMoving ? 0.1F : 0.3F);
            FireBreath.SetSize(IsMoving ? 0.75F : 1);
            FireBreath.SetShapeScale(IsMoving ? new Vector3(1, 0.2F, 1) : new Vector3(1, 0.5F, 1));
            SetAnim(AnimTrack.Attack, IsMoving ? Anim.ATTACK_STRONG_MOVING : Anim.ATTACK_STRONG_STATIC, false);
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
            if (!IsFlying && !IsAirborne && !AnimationLock(AnimTrack.Movement)) {
                if (!IsMoving) {
                    ClearAnim(AnimTrack.Movement);
                    SetAnim(AnimTrack.Movement, Anim.MOVE_RUNNING_START, false);
                    AddAnim(AnimTrack.Movement, Anim.MOVE_RUNNING_LOOP, true);
                }
            }
            AudioEmitter.loop = true;
            AudioEmitter.clip = AudioClips["step"];
            AudioEmitter.Play();
        }

        private void Stop(bool goIdle) {
            AudioEmitter.loop = false;
            if (!IsAirborne) {
                if (goIdle) {
                    if (GetCurrentAnim(AnimTrack.Movement).Name != Anim.IDLE_NORMAL
                         && GetCurrentAnim(AnimTrack.Movement).Name != Anim.MOVE_RUNNING_END_SHORT
                         && GetCurrentAnim(AnimTrack.Movement).Name != Anim.MOVE_RUNNING_END_LONG) {
                        DebugUI.Flash_IdleStart();
                        if (!IsMoving) {
                            SetAnim(AnimTrack.Movement, Anim.IDLE_NORMAL, true);
                        } else {
                            if (CurrectTrackEntry(AnimTrack.Movement).TrackTime < GetCurrentAnim(AnimTrack.Movement).Duration) {
                                SetAnim(AnimTrack.Movement, Anim.MOVE_RUNNING_END_SHORT, false);
                                AudioEmitter.clip = AudioClips["stop_short"];
                            } else {
                                SetAnim(AnimTrack.Movement, Anim.MOVE_RUNNING_END_LONG, false);
                                AudioEmitter.clip = AudioClips["stop_long"];
                            }
                            AddAnim(AnimTrack.Movement, Anim.IDLE_NORMAL, true);
                            AudioEmitter.Play();
                        }
                    }
                } else {
                    DebugUI.Flash_IdleStop();
                    ClearAnim(AnimTrack.Movement);
                }
            }


        }

        private void Jump() {
            //Body.AddForce(new Vector2(0, _JumpStrength), ForceMode2D.Impulse);
            SetAnim(AnimTrack.Movement, Anim.MOVE_JUMPING, false);
            IsAirborne = true;
            CurrectTrackEntry(AnimTrack.Movement).Complete += delegate {
                IsAirborne = false;
                SetAnim(AnimTrack.Movement, IsMoving ? Anim.MOVE_RUNNING_LOOP : Anim.IDLE_NORMAL, true);
                AudioEmitter.loop = true;
                AudioEmitter.clip = AudioClips["step"];
                AudioEmitter.Play();
            };
            AudioEmitter.Stop();
        }

        private void Duck() {
            SetAnim(AnimTrack.Movement, Anim.INTERACT_LOOT, false);
            AddAnim(AnimTrack.Movement, IsMoving ? Anim.MOVE_RUNNING_LOOP : Anim.IDLE_NORMAL, true);
        }
    }

}