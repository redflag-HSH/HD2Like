using UnityEngine;

public class Ledging : MonoBehaviour
{
    public Movement Mevement;
    public Transform orientation;
    public Rigidbody rb;

    [Header("LedgeGrabbing")]
    public float movingToLedgeSpeed;
    public float LedgeGrabMAxDistance;

    public float minTimeOnledge;
    private float timeOnLedge;

    public bool holding;
    [Header("LedgeJump")]
    public float LedgeJumpingForce;
    [Header("LedgeDetection")]
    public float ledgeDetectionLength;
    public float ledgeDetectionSphereRadius;
    public LayerMask whatIsLedge;
    [Header("Exiting")]
    public bool exitingLedge;
    public float exitLedgeTime;
    private float exitLedgeTimer;

    Transform lastLedge;
    Transform currentLedge;

    RaycastHit LedgeHit;

    public bool anyInputOccured;
    public bool LedgeJumpHappened;
    private void Update()
    {
        LedgeDetection();
        SubStateMachine();
    }

    void SubStateMachine()
    {

        if (holding)
        {
            FreezeRigidOnLedge();
            timeOnLedge += Time.deltaTime;

            if (timeOnLedge > minTimeOnledge && anyInputOccured)
                ExitLedge();
            if (LedgeJumpHappened)
                LedgeJump();

        }
        else if (exitingLedge)
        {
            if (exitLedgeTimer > 0)
                exitLedgeTime -= Time.deltaTime;
            else
                exitingLedge = false;
        }
    }

    private void LedgeDetection()
    {
        bool ledgeDetected = Physics.SphereCast(transform.position, ledgeDetectionSphereRadius, orientation.transform.forward, out LedgeHit, ledgeDetectionLength, whatIsLedge);
        Debug.Log("ledge Detected : " + ledgeDetected);
        if (!ledgeDetected)
            return;
        float distanceToLedge = Vector3.Distance(transform.position, LedgeHit.transform.position);

        if (LedgeHit.transform == lastLedge)
            return;

        if (distanceToLedge < LedgeGrabMAxDistance && !holding)
            EnterLedgeHold();
    }
    private void LedgeJump()
    {
        ExitLedge();
        Invoke(nameof(DelayedLedgeJump), .05f);
    }
    public void DelayedLedgeJump()
    {
        LedgeJumpHappened = false;

        Vector3 forceToAdd =/* orientation.forward * LedgeJumpingForce +*/ orientation.up * LedgeJumpingForce;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }

    private void EnterLedgeHold()
    {
        Debug.Log("LedgeHoldEnter");
        holding = true;

        Mevement.unlimited = true;
        Mevement.restricted = true;

        currentLedge = LedgeHit.transform;
        lastLedge = LedgeHit.transform;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;


        //orientation이 ledge를 쳐다보게
        //destination - origin
        Vector3 lookatt = (currentLedge.transform.position - orientation.position).normalized;
        orientation.forward = new Vector3(0, lookatt.y, 0);
    }
    private void FreezeRigidOnLedge()
    {
        rb.useGravity = false;
        Vector3 directionToLedge = currentLedge.position - transform.position;
        float distanceToLedge = Vector3.Distance(transform.position, currentLedge.position);
        if (distanceToLedge > 1f)
        {
            if (rb.linearVelocity.magnitude < movingToLedgeSpeed)
                rb.AddForce(directionToLedge.normalized * movingToLedgeSpeed * 1000f * Time.deltaTime);
        }
        else
        {
            if (!Mevement.freeze)
                Mevement.freeze = true;
            if (Mevement.unlimited)
                Mevement.unlimited = false;
        }

        if (distanceToLedge > LedgeGrabMAxDistance)
            ExitLedge();
    }
    private void ExitLedge()
    {
        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        holding = false;
        timeOnLedge = 0f;
        Mevement.restricted = false;
        Mevement.freeze = false;

        rb.useGravity = true;
        Invoke(nameof(REsetLastLedge), 1f);
    }

    private void REsetLastLedge()
    {
        lastLedge = null;
    }
}
