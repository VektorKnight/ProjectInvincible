using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    [Header("Direct Camera Object")]
    public Camera MainCamera;

    [Header("Camera Config")]
    public float maxHeight = 200;
    public float minHeight = 10;
    public float Dampening = 0.5f;
    public float moveSpeed = 1;
    public float zoomSpeed = 20;
    public float rotationSpeed = 1;
    
    [Header("Camera Angle")]
    public AnimationCurve CameraAngleFromHeight;

    //Private Fields
    private Vector3 targetDestination;
    private Vector3 targetRotation;
    private Vector3 mousePosCurrent;
    private Vector3 mousePosPrevious;


    public float ScrollWheel { get { return Input.mouseScrollDelta.y / 10; } }
    // Use this for initialization
    void Start()
    {
        targetDestination=transform.position;
        targetRotation = transform.eulerAngles;
      //  MainCamera.transform.Rotate(45, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
       
        float ratio = (transform.position.y +20) / maxHeight;
       // targetRotation = new Vector3((CameraAngleFromHeight.Evaluate(ratio)*45)+45, transform.eulerAngles.y, transform.eulerAngles.z);

        //MMB Camera control
        if (Input.GetMouseButtonDown(2))
            mousePosPrevious = Input.mousePosition;
        if (Input.GetMouseButton(2))
        {
            //before frame, update position
            mousePosCurrent = Input.mousePosition;
            //calculate difference
            Vector3 change = mousePosPrevious - mousePosCurrent;
            //apply trahsform
            targetDestination += (new Vector3(change.x, 0, change.y) * moveSpeed);
            //after frame, update position
            mousePosPrevious = Input.mousePosition;
           // targetDestination = transform.position;

        }

        //WASD Camera control
       /* float boost = 2;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        { boost = 4; }
        if (Input.GetKey(KeyCode.W))
        { transform.Translate(Vector3.forward * moveSpeed * boost,Space.World); targetDestination = transform.position;
        }
        if (Input.GetKey(KeyCode.A))
        { transform.Translate(Vector3.left * moveSpeed * boost, Space.World); targetDestination = transform.position;
        }
        if (Input.GetKey(KeyCode.S))
        { transform.Translate(Vector3.back * moveSpeed * boost, Space.World); targetDestination = transform.position;
        }
        if (Input.GetKey(KeyCode.D))
        { transform.Translate(Vector3.right * moveSpeed * boost, Space.World); targetDestination = transform.position;
        }
        */
        //zoom control
        RaycastHit hit = new RaycastHit();
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
       
        Vector3 difference = hit.point - transform.position;
        difference.Normalize();
        float distance = Vector3.Distance(transform.position, hit.point);
        if (ScrollWheel != 0) {
            Physics.Raycast(ray, out hit);
        }
       

        //reset target for zoom        
        if (ScrollWheel > 0 && transform.position.y > minHeight)
        { targetDestination = Vector3.MoveTowards(transform.position, hit.point, zoomSpeed); }
        if (ScrollWheel < 0 && transform.position.y < maxHeight)
        { targetDestination = Vector3.MoveTowards(transform.position, hit.point, -zoomSpeed); }
        targetDestination.y = Mathf.Clamp(targetDestination.y, minHeight, maxHeight);
        transform.position = Vector3.Lerp(transform.position, targetDestination, Dampening/2);
        transform.eulerAngles = targetRotation;

        //rotation
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, -rotationSpeed, 0, Space.Self);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, rotationSpeed, 0, Space.Self);
        }
    }
}
