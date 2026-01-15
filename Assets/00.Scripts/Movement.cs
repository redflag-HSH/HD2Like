using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    public float moveSpeed;

    public float groundDrag;

    public float playerHeight;
    public LayerMask whatisGround;
    bool grounded;

    public Transform orientation;


    Vector3 moveDirection;

    Rigidbody rb;

    TPSActions _actions;
    TPSActions.TpsDefalutActions _tpsActions;
    Vector2 _input;
    public InputActionReference refe;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        _actions = new TPSActions();
        _tpsActions = _actions.tpsDefalut;
    }
    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * .5f + .2f, whatisGround);

        _input = refe.action.ReadValue<Vector2>();
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer(_input);
    }


    private void MovePlayer(Vector2 input)
    {

        moveDirection = orientation.forward * input.y + orientation.right * input.x;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }
}
