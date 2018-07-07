using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Dragonling.Controllers {

    public class FireBreathController : MonoBehaviour {
        private PlayerController Player;
        private ParticleSystem ParticleSystem;

        private ParticleSystem.MainModule Main;
        private ParticleSystem.ShapeModule Shape;

        public bool IsActive;

        void Start() {
            Init();
        }

        private void Init() {
            ParticleSystem = GetComponentInParent<ParticleSystem>();
            Player = GetComponentInParent<PlayerController>();
            Main = ParticleSystem.main;
            Shape = ParticleSystem.shape;
        }

        void Update() {

        }

        private void FixedUpdate() {
            if (ParticleSystem.isPlaying)
                UpdatePosition();
        }

        private void UpdatePosition() {
            Shape.rotation = new Vector3(0, Player.Flipped ? -130 : 130, 0);
            Player.SkeletonAnimation.Skeleton.UpdateWorldTransform();
            Bone bone = Player.SkeletonAnimation.Skeleton.FindBone("head_a");
            Shape.position = new Vector3(bone.WorldX / 2, bone.WorldY / 2 - 0.2F, 0);
        }

        public void Activate() {
            ParticleSystem.Play();
        }

        public void SetStartDelay(float delay) {
            Main.startDelay = delay;
        }

        public void SetSize(float size) {
            Main.startSize = size;
        }

        public void SetShapeScale(Vector3 scale) {
            Shape.scale = scale;
        }
    }

}
