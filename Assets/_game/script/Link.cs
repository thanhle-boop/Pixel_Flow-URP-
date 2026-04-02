using UnityEngine;

public class Link : MonoBehaviour
{
    public GameObject link1;
    public GameObject link2;
    public Material mesh1;
    public Material mesh2;
    public Material hiddenMesh;
    private PigComponent pigObject1;
    private PigComponent pigObject2;
    private Vector3 startPos;
    private Vector3 endPos;
    private string color1;
    private string color2;

    void OnEnable()
    {
        EventManager.OnPigIsOnTopNoMoreHidden += ResetColorNoMoreHidden;
        EventManager.OnClearLinked += DestroyLink;
    }
    void OnDisable()
    {
        EventManager.OnPigIsOnTopNoMoreHidden -= ResetColorNoMoreHidden;
        EventManager.OnClearLinked -= DestroyLink;
    }

    void DestroyLink(PigComponent pig)
    {
        if (pig == pigObject1 || pig == pigObject2)
        {
            Destroy(gameObject);
        }
    }
    void ResetColorNoMoreHidden(PigComponent pig)
    {
        if (pig == pigObject1)
        {
            var material1 = link1.GetComponent<Renderer>();
            material1.material = mesh1;
            material1.material.color = ColorGameConfig.instance.GetColorByName(color1);

        }
        else if (pig == pigObject2)
        {
            var material2 = link2.GetComponent<Renderer>();
            material2.material = mesh2;
            material2.material.color = ColorGameConfig.instance.GetColorByName(color2);
        }
    }

    void Update()
    {
        if (pigObject1 == null || pigObject2 == null)
        {
            return;
        }

        startPos = pigObject1.transform.position;
        endPos = pigObject2.transform.position;

        transform.position = Vector3.Lerp(startPos, endPos, 0.5f) + new Vector3(0, 0.5f, 0);
        Vector3 dir = endPos - startPos;
        Vector3 huongZ = Vector3.Cross(Vector3.up, dir).normalized;
        if (huongZ == Vector3.zero)
        {
            huongZ = Vector3.Cross(Vector3.forward, dir).normalized;
        }
        transform.rotation = Quaternion.LookRotation(huongZ, dir);
        float distance = dir.magnitude;

        transform.localScale = new Vector3(transform.localScale.x, distance * 0.5f, transform.localScale.z);
    }

    public void SetColor(string color1, string color2, PigComponent pig1, PigComponent pig2)
    {
        pigObject1 = pig1;
        var material1 = link1.GetComponent<Renderer>();
        this.color1 = color1;
        this.color2 = color2;

        if (pigObject1.isHidden)
        {
            material1.material = hiddenMesh;
        }
        else
        {
            material1.material = mesh1;
            material1.material.color = ColorGameConfig.instance.GetColorByName(color1);
        }


        pigObject2 = pig2;
        var material2 = link2.GetComponent<Renderer>();
        if (pigObject2.isHidden)
        {
            material2.material = hiddenMesh;
        }
        else
        {
            material2.material = mesh2;
            material2.material.color = ColorGameConfig.instance.GetColorByName(color2);
        }
    }
}
