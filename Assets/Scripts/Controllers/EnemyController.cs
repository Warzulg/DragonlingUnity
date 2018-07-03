using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dragonling.Controllers {

    public class EnemyController : MonoBehaviour {

        private Collider2D Collider;

        void Start() {
            Init();
        }

        private void Init() {
            Collider = GetComponent<Collider2D>();
        }

        void Update() {

        }

        private void FixedUpdate() {
            CheckCollision();
        }

        private void CheckCollision() {
            if (GetComponentsInParent<Collider2D>().Where(c => c.IsTouching(Collider)).Count() > 0)
                Debug.Log("HIT");
        }
    }

}