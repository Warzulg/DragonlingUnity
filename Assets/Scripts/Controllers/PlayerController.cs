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
        private PolygonCollider2D Collider;
        private CameraController Camera;

        public bool Moving;
        public bool Airborne;
        public bool Flying;
        public bool Flipped;

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

            SkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            AnimationState = SkeletonAnimation.AnimationState;
            Body = GetComponentInChildren<Rigidbody2D>();
            Collider = GetComponentInChildren<PolygonCollider2D>();
            FireBreath = GetComponentInChildren<FireBreathController>();

            Camera = GetComponentInChildren<CameraController>();
            Camera.Init(Body);

            AudioEmitter = GetComponentInChildren<AudioSource>();
            AudioClips = new Dictionary<string, AudioClip>();
            //FIXME: solchen scheiß in ne art ressource-helper auslagern:
            AudioClips.Add("step", Resources.Load<AudioClip>("Sounds/dragonling/step"));
            AudioClips.Add("stop_long", Resources.Load<AudioClip>("Sounds/dragonling/stop_long"));
            AudioClips.Add("stop_short", Resources.Load<AudioClip>("Sounds/dragonling/stop_short"));

            Flip(false);
            SetAnim(AnimTrack.Movement, Anim.IDLE_NORMAL, true);
        }

        void Update() {
            HandleInput();
        }

        private void FixedUpdate() {
            ResolveMovement();
            UpdateColiderPosition();
        }

        private void UpdateColiderPosition() {
            Bone bone = SkeletonAnimation.Skeleton.FindBone("body");
            Vector2 colliderOffset = new Vector2(0, bone.GetSkeletonSpacePosition().y - Collider.bounds.size.y);
            Collider.offset = colliderOffset;
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
                if (Input.GetKey(KeyCode.A)) {
                    Body.AddForce(new Vector2(-_Acceleration, 0), ForceMode2D.Force);
                } else if (Input.GetKey(KeyCode.D)) {
                    Body.AddForce(new Vector2(_Acceleration, 0), ForceMode2D.Force);
                }
            }
        }

        private void Flip(bool flipX) {
            Collider.transform.rotation = Quaternion.LookRotation(flipX ? Vector3.back : Vector3.forward, Vector3.up);
            Flipped = flipX;
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
            SetAnim(AnimTrack.Attack, Moving ? Anim.ATTACK_BASIC_MOVING : Anim.ATTACK_BASIC_STATIC, false);
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
            AudioEmitter.loop = true;
            AudioEmitter.clip = AudioClips["step"];
            AudioEmitter.Play();
        }

        private void Stop(bool goIdle) {
            AudioEmitter.loop = false;
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
            Airborne = true;
            CurrectTrackEntry(AnimTrack.Movement).Complete += delegate {
                Airborne = false;
                SetAnim(AnimTrack.Movement, Moving ? Anim.MOVE_RUNNING_LOOP : Anim.IDLE_NORMAL, true);
                AudioEmitter.loop = true;
                AudioEmitter.clip = AudioClips["step"];
                AudioEmitter.Play();
            };
            AudioEmitter.Stop();
        }

        private void Duck() {
            SetAnim(AnimTrack.Movement, Anim.INTERACT_LOOT, false);
            AddAnim(AnimTrack.Movement, Moving ? Anim.MOVE_RUNNING_LOOP : Anim.IDLE_NORMAL, true);
        }
    }

}