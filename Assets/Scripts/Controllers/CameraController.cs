using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private Camera Controller;
    private Rigidbody2D PivotBody;
    private readonly Vector3 Offset = new Vector3(4, 4, -10);

    void Start () {

    }

    public void Init(Rigidbody2D pivot) { 
        Controller = GetComponent<Camera>();
        PivotBody = pivot;
    }

    void Update () {
		
	}

    private void FixedUpdate() {
        UpdatePosition();
    }

    private void UpdatePosition() {
        Controller.transform.position = PivotBody.transform.position + Offset;
    }
}
