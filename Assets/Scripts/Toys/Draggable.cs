using UnityEngine;
using System.Collections;


//LD Didn't code this entirely, here's where he got it:
//https://web.archive.org/web/20200708013820/http://wiki.unity3d.com/index.php/Drag&Throw
//LD Just modified it a bit, he put his names on his comments.
[RequireComponent(typeof(Rigidbody))]
public class Draggable : MonoBehaviour
{

    int normalCollisionCount = 1;
    float spring = 50.0f;
    float damper = 5.0f;
    float drag = 10.0f;
    float angularDrag = 5.0f;
    float distance = 0.2f;
    float throwForce = 500;
    float throwRange = 1000;
    bool attachToCenterOfMass = false;

    private SpringJoint springJoint;

    bool is_dragging = false;

    GameObject dragger;

    void Update()
    {

        // Make sure the user pressed the mouse down
        if (!Input.GetMouseButtonDown(0))
            return;



        var mainCamera = FindCamera();

        // We need to actually hit an object
        RaycastHit hit;
        //LD Montello
        //Modified this to include the 
        //Layermask for ignoring raycasts
        //so that my ball shooter doesn't
        //block the raycast.
        if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 100f, ~LayerMask.GetMask("Ignore Raycast", "Player")))
            return;


        //LD Montello
        //if the hit object is not equal to ourselves,
        //then we need to stop here.
        if (hit.collider.gameObject != gameObject)
        {
            return;
        }

        // We need to hit a rigidbody that is not kinematic
        if (!hit.rigidbody || hit.rigidbody.isKinematic)
            return;





        if (!springJoint)
        {
            if (dragger == null)
            {
                dragger = new GameObject("Rigidbody dragger");
                var body = dragger.AddComponent(typeof(Rigidbody)) as Rigidbody;
                springJoint = dragger.AddComponent(typeof(SpringJoint)) as SpringJoint;
                body.isKinematic = true;
            }
        }

        springJoint.transform.position = hit.point;
        if (attachToCenterOfMass)
        {
            var anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position;
            anchor = springJoint.transform.InverseTransformPoint(anchor);
            springJoint.anchor = anchor;
        }
        else
        {
            springJoint.anchor = Vector3.zero;
        }



        springJoint.spring = spring;
        springJoint.damper = damper;
        springJoint.maxDistance = distance;
        springJoint.connectedBody = hit.rigidbody;




        if (is_dragging == false)
        {
            StartCoroutine(DragObject(hit.distance));
        }


    }

    public IEnumerator DragObject(float distance)
    {
        is_dragging = true;

        float oldDrag = springJoint.connectedBody.linearDamping;
        float oldAngularDrag = springJoint.connectedBody.angularDamping;
        springJoint.connectedBody.linearDamping = drag;
        springJoint.connectedBody.angularDamping = angularDrag;
        var mainCamera = FindCamera();
        while (Input.GetMouseButton(0))
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            springJoint.transform.position = ray.GetPoint(distance);

            //LD Montello
            //I think they did this to:
            //Wait until next frame before
            //checking if we should throw the object.
            yield return null;

            //LD Montello, 
            //Added a check that the connected body isn't null.
            if (Input.GetMouseButton(1) && springJoint.connectedBody != null)
            {
                springJoint.connectedBody.AddExplosionForce(throwForce, mainCamera.transform.position, throwRange);
                springJoint.connectedBody.linearDamping = oldDrag;
                springJoint.connectedBody.angularDamping = oldAngularDrag;
                springJoint.connectedBody = null;
            }
        }
        if (springJoint.connectedBody)
        {
            springJoint.connectedBody.linearDamping = oldDrag;
            springJoint.connectedBody.angularDamping = oldAngularDrag;
            springJoint.connectedBody = null;
        }


        //LD Montello
        //only say we aren't dragging
        //when the coroutine is going to end.
        is_dragging = false;
        //coroutine_count--;
    }

    Camera FindCamera()
    {
        if (GetComponent<Camera>())
            return GetComponent<Camera>();
        else
            return Camera.main;
    }
}