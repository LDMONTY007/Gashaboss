using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CollisionSensor : MonoBehaviour
{
    public float distance = 10;
    public float angle = 30;
    public float height = 1.0f;
    public float heightOffset = 0f;
    public Color meshColor = Color.red;
    public int scanFrequency = 30;
    public LayerMask layers;
    public LayerMask occlusionLayers;
    public bool debugTargets;
    public bool debug;
    public List<GameObject> Objects
    {
        get
        {
            objects.RemoveAll(obj => !obj);
            return objects;
        }
    }
    private List<GameObject> objects = new List<GameObject>();

    Collider[] colliders = new Collider[50];
    Mesh mesh;
    int count;
    float scanInterval;
    float scanTimer;

    // Start is called before the first frame update
    void Start()
    {
        scanInterval = 1f / scanFrequency;
    }

    // Update is called once per frame
    void Update()
    {

        //Old code for collision sensor stuff.
        //it used to run all the time but we only
        //run it once when attacking.
/*        scanTimer -= Time.deltaTime;
        if (scanTimer < 0)
        {
            scanTimer += scanInterval;
            Scan();
        }*/
    }

    private void Scan()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, layers, QueryTriggerInteraction.Collide);

        objects.Clear();

        

        for (int i = 0; i < count; i++)
        {
            GameObject obj = colliders[i].gameObject;
            if (IsInSight(obj, colliders[i]))
            {
                objects.Add(obj);
            }
        }
    }

    public List<GameObject> ScanForObjects()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, layers, QueryTriggerInteraction.Collide);

        objects.Clear();



        for (int i = 0; i < count; i++)
        {
            GameObject obj = colliders[i].gameObject;
            if (IsInSight(obj, colliders[i]))
            {
                objects.Add(obj);
            }
        }

        return objects;
    }

    public bool IsInSight(GameObject obj, Collider col)
    {


        //include height offset in origin.
        Vector3 origin = transform.position - new Vector3(0f, heightOffset, 0f);
        Vector3 dest = obj.transform.position;
        Vector3 direction = dest - origin;

        //we use col.bounds.size.y / 2 
        //here so that we get the full shape of the collider's vertical
        //bounds in worldspace to adhere to our method of checking collisions.
        //this makes it so we don't just check where the origin of the object
        //is that is within our sensor, we instead check the full size of our object.
        if (direction.y < 0 - col.bounds.size.y / 2 || direction.y > height + col.bounds.size.y / 2)
        {
            //draw a ray to show when we aren't intersecting,
            //where the direction is facing.
            //Debug.DrawRay(transform.position, direction.normalized * Vector3.Distance(origin, dest), Color.red);
            return false;
        }

        direction.y = 0;
        float deltaAngle = Vector3.Angle(direction, transform.forward);
        if (deltaAngle > angle)
        {
            return false;
        }

        origin.y += height / 2;
        dest.y = origin.y;
        if (Physics.Linecast(origin, dest, occlusionLayers))
        {
            return false;
        }

        return true;
    }

    Mesh CreateWedgeMesh()
    {
        Mesh mesh = new Mesh();

        int segments = 10;
        int numTriangles = (segments * 4) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 bottomCenter = (Vector3.zero) - new Vector3(0f, heightOffset, 0f);
        Vector3 bottomLeft = (Quaternion.Euler(0, -angle, 0) * Vector3.forward * distance) - new Vector3(0f, heightOffset, 0f);
        Vector3 bottomRight = (Quaternion.Euler(0, angle, 0) * Vector3.forward * distance) - new Vector3(0f, heightOffset, 0f);

        Vector3 topCenter = bottomCenter + Vector3.up * height;
        Vector3 topRight = bottomRight + Vector3.up * height;
        Vector3 topLeft = bottomLeft + Vector3.up * height;

        int vert = 0;

        //Left side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomLeft;
        vertices[vert++] = topLeft;

        vertices[vert++] = topLeft;
        vertices[vert++] = topCenter;
        vertices[vert++] = bottomCenter;

        //Right side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = topCenter;
        vertices[vert++] = topRight;

        vertices[vert++] = topRight;
        vertices[vert++] = bottomRight;
        vertices[vert++] = bottomCenter;

        float currentAngle = -angle;
        float deltaAngle = (angle * 2) / segments;
        for (int i = 0; i < segments; i++)
        {

            bottomLeft = (Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance) - new Vector3(0f, heightOffset, 0f);
            bottomRight = (Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance) - new Vector3(0f, heightOffset, 0f);


            topRight = bottomRight + Vector3.up * height;
            topLeft = bottomLeft + Vector3.up * height;

            //Far side
            vertices[vert++] = bottomLeft;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;

            vertices[vert++] = topRight;
            vertices[vert++] = topLeft;
            vertices[vert++] = bottomLeft;

            //Top side

            vertices[vert++] = topCenter;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;


            //Bottom side
            //The reason we reverse the order here is because the normals need to face outward from the center of the mesh.
            vertices[vert++] = bottomCenter;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomLeft;

            currentAngle += deltaAngle;
        }


        for (int i = 0; i < numVertices; i++)
        {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void OnValidate()
    {
        scanInterval = 1f / scanFrequency;
        mesh = CreateWedgeMesh(); //if we change something in the inspector recalculate this mesh.
    }

    private void OnDrawGizmos()
    {
        if (debug)
        {
            if (mesh)
            {
                Gizmos.color = meshColor;
                Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
            }

        }

        if (debugTargets)
        {
            Gizmos.color = Color.green;
            foreach (var obj in objects)
            {
                Gizmos.DrawSphere(obj.transform.position, 0.5f);
            }
        }

    }

    public int Filter(GameObject[] buffer, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        int count = 0;
        foreach (var obj in Objects)
        {
            if (obj.layer == layer)
            {
                buffer[count++] = obj;
            }

            if (buffer.Length == count)
            {
                break; //buffer is full
            }
        }

        return count;
    }
}
