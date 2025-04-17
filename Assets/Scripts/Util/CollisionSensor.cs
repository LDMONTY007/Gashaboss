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


        for (int i = 0; i < colliders.Count; i++)
        {
            //if the collider is now null,
            //remove it from the list and skip
            //adding it to the objects list.
            if (colliders[i] == null)
            {
                colliders.Remove(colliders[i]);
                continue;
            }
            //if our parent somehow gets in this list,
            //skip them.
            if (transform.IsChildOf(colliders[i].transform))
            {
                colliders.Remove(colliders[i]);
                continue;
            }

            //we add the GetWidthAlongDirection calculation here so
            //that the point we check also takes into account the width
            //of the collider in the up direction of the collision sensor.
            if (IsPointWithinArc(colliders[i].transform.position, transform.position, transform.forward, transform.up, angle, height + GetWidthAlongDirection(colliders[i].bounds, transform.up) / 2))
            {
                //add the collider to our list
                //because it is within collision sensor.
                _objects.Add(colliders[i].gameObject);
            }

            
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
            if (colliders.Count > 0)
            {
                foreach (Collider c in colliders)
                {
                    //if the collider is null, then continue instead.
                    if (c == null) continue;
                    Gizmos.DrawSphere(c.transform.position, 0.5f);
                    Gizmos.color = Color.blue;
                    //draw the bounds width in the up direcion of our collision sensor.
                    Gizmos.DrawLine(c.transform.position, c.transform.position + transform.up.normalized * GetWidthAlongDirection(c.transform.GetComponent<Collider>().bounds, transform.up) / 2);
                    Gizmos.DrawLine(c.transform.position, c.transform.position - transform.up.normalized * GetWidthAlongDirection(c.transform.GetComponent<Collider>().bounds, transform.up) / 2);
                }
            }
        }

    }

    List<Collider> colliders = new List<Collider>();

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
        //Debug.Log(Vector3.Dot(arcCenter - point, upTransform));

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
        //Do not add our own parent to a collision list.
        //We want to ignore our parents in these checks.
        if (transform.IsChildOf(other.transform))
        {
            return;
        }

        //add the collider to our list.
        colliders.Add(other);

    }

    private void OnTriggerStay(Collider other)
    {
        //if this transform isn't in the transforms
        //yet, then add it.
        if (!colliders.Contains(other))
        {
            colliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //remove the collider from
        //the list as it has left the sphere.
        colliders.Remove(other);
    }
}
