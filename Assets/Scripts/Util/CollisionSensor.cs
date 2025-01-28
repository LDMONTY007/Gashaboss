using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CollisionSensor : MonoBehaviour
{
    public float angle = 30;
    public float height = 1.0f;
    public bool debugTargets;
    public bool debug;
    public Color sensorColor = Color.blue;
    public List<GameObject> Objects
    {
        get
        {
            objects.RemoveAll(obj => !obj);
            return objects;
        }
    }
    private List<GameObject> objects = new List<GameObject>();

    public SphereCollider triggerCollider;

    // Start is called before the first frame update
    void Start()
    {
        triggerCollider = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    

    public List<GameObject> ScanForObjects()
    {
        //get the objects in our 
        //arc.
        List<GameObject> _objects = new List<GameObject>();
        foreach (Transform t in transforms)
        {
            _objects.Add(t.gameObject);
        }
        
        return _objects;
    }


    //draws a wedge that spans from -angle to angle
    public void DrawWedge(float angle, Vector3 forward, Vector3 up, Vector3 startPos)
    {
        for (int i = 0; i < angle * 2; i++)
        {
            //we do i - angle because we start at the negative side of the angle and extend to the positive
            // so we go from -angle to +angle
            Vector3 offsetVector = Quaternion.AngleAxis(i - angle, up) * (forward.normalized * triggerCollider.radius * 2);
            //Vector3 oppositeOffsetVector = Quaternion.AngleAxis(angle, up) * (forward.normalized * distance);


            Gizmos.color = sensorColor;
            //draw line from the arc origin to the edge of the radius at the desired angle.
            Gizmos.DrawLine(startPos, startPos + offsetVector);
            Gizmos.color = Color.green;
            //draw a line to show the height and how it extends in both directions.
            Gizmos.DrawLine(startPos + offsetVector, startPos + offsetVector + transform.up.normalized * height * 0.5f);
            Gizmos.DrawLine(startPos + offsetVector, startPos + offsetVector - transform.up.normalized * height * 0.5f);
            //Gizmos.DrawLine(startPos, startPos + oppositeOffsetVector);
        }


        
    }

    private void OnDrawGizmos()
    {
        if (debug)
        {
            //draw the wedge arc for where we're allowed to 
            //consider the object within our attack arc.
            DrawWedge(angle, transform.forward, transform.up, transform.position);
        }

        if (debugTargets)
        {
            Gizmos.color = Color.green;
            foreach (var obj in objects)
            {
                Gizmos.DrawSphere(obj.transform.position, 0.5f);
            }

            //Draw transforms that are within the sphere trigger.
            if (transforms.Count > 0)
            {
                foreach (Transform t in transforms)
                {
                    //if the transform is null, then continue instead.
                    if (t == null) continue;
                    Gizmos.DrawSphere(t.position, 0.5f);
                    Gizmos.color = Color.blue;
                    //draw the bounds width in the up direcion of our collision sensor.
                    Gizmos.DrawLine(t.position, t.position + transform.up.normalized * GetWidthAlongDirection(t.GetComponent<Collider>().bounds, transform.up) / 2);
                    Gizmos.DrawLine(t.position, t.position - transform.up.normalized * GetWidthAlongDirection(t.GetComponent<Collider>().bounds, transform.up) / 2);
                }
            }
        }

    }

    List<Transform> transforms = new List<Transform>();


    //old point in arc method.
    public bool pointInArc(Vector3 point)
    {
        

        if (Mathf.Abs(Vector3.SignedAngle(transform.forward, (point - transform.position).normalized, transform.up)) <= angle)
        {
            Debug.LogWarning(Mathf.Abs(Vector3.SignedAngle(transform.forward, (point - transform.position).normalized, transform.up)));
            return true;
        }
        return false;
    }

    bool IsPointWithinArc(Vector3 point, Vector3 arcCenter, Vector3 forwardTransform, Vector3 upTransform, float arcAngle, float heightRange)

    {

        //Calculate plane normal, this is the plane that our arc lies on.

        Vector3 planeNormal = Vector3.Cross(upTransform, forwardTransform).normalized;



        //Project point onto the plane, this makes it easy to check
        //if it's within our target angle or not.
        Vector3 projectedPoint = point - Vector3.Dot(point - arcCenter, planeNormal) * planeNormal;



        //Calculate angle between projected point and center
        Vector3 directionVector = projectedPoint - arcCenter;
        float angle = Vector3.Angle(directionVector, forwardTransform);


        //print out the height distance from the arc's center to the point
        //we are checking for the arc to see if it's within our height range. 
        Debug.Log(Vector3.Dot(arcCenter - point, upTransform));

        //Check if angle is within arc range and height is within height range
        //for the height range, we're just projecting the point onto the arc's up
        //transform so we can get a float value that represents the distance from 
        //the projected point to the actual point to check if it is within
        //the height range. 
        return (angle <= arcAngle / 2) && (Mathf.Abs(Vector3.Dot(arcCenter - point, upTransform)) <= heightRange);

    }

    
    //gets the width of the bounds in a given direction.
    //this is used for calculating if bounds are within
    //our collision sensor.
    public float GetWidthAlongDirection(Bounds bounds, Vector3 direction)

    {

        //start at infinities
        var maxProjection = Mathf.NegativeInfinity;

        var minProjection = Mathf.Infinity;



        //Check each corner of the bounds
        //it was trial and error to figure
        //out how to get the coordinates of all
        //the corners.
        foreach (var corner in new Vector3[] { bounds.min, bounds.max, new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), new Vector3(bounds.max.x, bounds.min.y, bounds.max.z) })

        {
            //project the corner onto the direction vector,
            //thus turning it into a float
            var projection = Vector3.Dot(corner, direction);

            //set max projection, gives us the edge of the object on
            //one side in our direction
            maxProjection = Mathf.Max(maxProjection, projection);

            //set min projection, gives us the other edge of the object
            //on the other side in our direction
            minProjection = Mathf.Min(minProjection, projection);

        }


        //return the difference between the max and min projection,
        //which will be the distance from one edge of the bounds to the other
        //in our specified direction.
        return maxProjection - minProjection;

    }

    private void OnTriggerEnter(Collider other)
    {
        //we add the GetWidthAlongDirection calculation here so
        //that the point we check also takes into account the width
        //of the collider in the up direction of the collision sensor.
        if (IsPointWithinArc(other.transform.position , transform.position, transform.forward, transform.up, angle, height + GetWidthAlongDirection(other.bounds, transform.up) / 2))
        {
            //add the transform to our list.
            transforms.Add(other.transform);
        }

    }

    private void OnTriggerStay(Collider other)
    {
        //If the point is within the arc angle and height
        if (IsPointWithinArc(other.transform.position, transform.position, transform.forward, transform.up, angle, height + GetWidthAlongDirection(other.bounds, transform.up) / 2))
        {
            //if this transform isn't in the transforms
            //yet, then add it.
            if (!transforms.Contains(other.transform))
            {
                transforms.Add(other.transform);
            }
        }
        //if the angle is too great,
        //remove them from the list.
        else
        {
            //remove the transform from
            //the list as it is no longer
            //within the arc.
            transforms.Remove(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //remove the transform from
        //the list as it has left the sphere.
        transforms.Remove(other.transform);
    }
}
