using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dragonling.Controllers {

    public class CameraController : MonoBehaviour {
        private readonly float _MoveTime = 0.8F;

        private Camera Controller;
        private Rigidbody2D PivotBody;

        public bool IsMoving;

        private float MovementStart;
        private Vector3 MovementPosEnd;

        public void Init(Rigidbody2D pivot) {
            Controller = GetComponent<Camera>();
            PivotBody = pivot;
            IsMoving = false;
        }

        private void FixedUpdate() {
            UpdatePosition();
        }

        private void UpdatePosition() {
            Vector3 absPosition = PivotBody.transform.position;
            Vector3 relPosition = new Vector3(0, 0, 0);

            if (IsMoving) {
                float movementProg = Time.fixedTime / (MovementStart + _MoveTime);
                if (movementProg < 1) {
                    relPosition = Vector3.Lerp(absPosition, MovementPosEnd, movementProg);
                }
            }

            Controller.transform.position = absPosition + relPosition;
            //Debug.Log(Controller.transform.position);
        }

        public void MoveTo(Vector3 position) {
            MovementStart = Time.fixedTime;
            MovementPosEnd = position;
        }
    }

}