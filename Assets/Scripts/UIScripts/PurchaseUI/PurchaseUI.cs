using UnityEngine;
using TMPro;
using System.Collections;

//Where I got some of the code for this:
//https://discussions.unity.com/t/fixed-change-color-of-individual-characters-in-textmeshpro-text-ui/880934/4
public class PurchaseUI : MonoBehaviour
{

/*    public string purchaseName = string.Empty;

    public int cost = 250;

    public TextMeshPro[] letters = null;

    private string stringToDisplay = string.Empty;*/

    public float letterSpacing = 1f;

    public float fontSize = 36f;

    private TMP_Text m_TextComponent;

    private Coroutine _currentCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_TextComponent = GetComponent<TMP_Text>();

        _currentCoroutine = StartCoroutine(Wobble2());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float speedMultiplier = 10f;

    IEnumerator Wobble()
    {

        m_TextComponent.ForceMeshUpdate();
        Mesh mesh = m_TextComponent.mesh;
        Vector3[] vertices = mesh.vertices;
        while (true)
        {
            for (int i = 0; i < m_TextComponent.textInfo.characterCount; i++)
            {
                TMP_CharacterInfo c = m_TextComponent.textInfo.characterInfo[i];
                int index = c.vertexIndex;
                Vector3 offset = Wobble((Time.time + i) / speedMultiplier);
                Debug.Log(vertices[index].y);
                vertices[index] += offset;
                vertices[index + 1] += offset;
                vertices[index + 2] += offset;
                vertices[index + 3] += offset;
            }
            mesh.vertices = vertices;
            //Are we on a canvas renderer?
            if (m_TextComponent.canvasRenderer != null)
            {
                //set mesh
                m_TextComponent.canvasRenderer.SetMesh(mesh);
            }
            //No, we are a normal mesh renderer.
            else
            {
                //push new vertices.
                m_TextComponent.SetVertices(mesh.vertices);
            }


           
           
            yield return new WaitForSeconds(0.025f);
        }
    }

    public Vector3 Wobble(float t)
    {
        return new Vector3(0, Mathf.Sin(Time.time * speedMultiplier), 0);
    }

    [Space(10)]
    [Header("Move")]
    public Vector2 startOffset = -Vector2.up * 1;
    public Vector2 endOffset = Vector2.up * 1;

    //more reference for code:
    //https://github.com/LeiQiaoZhi/Easy-Text-Effects-for-Unity/blob/main/Runtime/Effects/TextEffectInstance.cs#L94

    IEnumerator Wobble2()
    {
        startTime = Time.time;

        m_TextComponent.ForceMeshUpdate();
 
        while (true)
        {
            TMP_TextInfo textInfo = m_TextComponent.textInfo;
            //Loop through every character.
            for (int i = 0; i < m_TextComponent.textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                
                //if it isn't visible, skip it.
                if (!charInfo.isVisible) continue;


                int materialIndex = charInfo.materialReferenceIndex;
                
                
                
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
                
                
/*                foreach(Vector3 v1 in  vertices)
                {
                    Debug.Log(v1);
                }*/

                //loop through each vertice in the character mesh
                for (int j = 0; j < 4; j++)
                {
                    int v = charInfo.vertexIndex + j;
                    Vector3 offset = Interpolate(startOffset, endOffset, i);
                    Debug.Log(vertices[v]);
                    vertices[v] += offset;
                }



            }

            // apply changes and update mesh
            for (var i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];

                meshInfo.mesh.colors32 = meshInfo.colors32;
                meshInfo.mesh.vertices = meshInfo.vertices;

                m_TextComponent.UpdateGeometry(meshInfo.mesh, i);
            }


            yield return new WaitForSeconds(0.025f);
        }
    }

    float timePerChar = 0.5f;

    public Vector2 Interpolate(Vector2 start, Vector2 end, int charIndex)
    {
        float time = GetCharTime(charIndex);

        return Vector2.Lerp(start, end, Mathf.Clamp01(time / timePerChar));
    }

    float timeBetweenChars = 0.05f;

    float startTime = 0f;

    public float GetCharTime(int charIndex)
    {
        float time = Time.time;

        float charStartTime = startTime + timeBetweenChars * charIndex;

        return time - charStartTime;
    }
}
