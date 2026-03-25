using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperCat : MonoBehaviour
{
    private Vector3 initPosition;

    private float speed = 20f;
    private SuperCatState currentState = SuperCatState.Idel;
    private WavyLineRenderer wavyLineRenderer;

    public List<GameObject> targetBlocks = new List<GameObject>();
    public Color color;

    private void OnEnable()
    {
        wavyLineRenderer = GetComponentInChildren<WavyLineRenderer>();
        initPosition = transform.position;
    }

    public void AddAllTarget(List<GameObject> blocks)
    {
        targetBlocks = new List<GameObject>(blocks);
        currentState = SuperCatState.Shoot;
        wavyLineRenderer.SetColor(color);
        wavyLineRenderer.InitializeLineRenderer(0.15f, 0.15f);
        StartCoroutine(ShootingRoutine());
    }

    private IEnumerator ShootingRoutine()
    {
        wavyLineRenderer.UpdateStartPoint(transform.position);

        for (int i = 0; i < targetBlocks.Count; i++)
        {
            if (targetBlocks[i] == null) continue;

            wavyLineRenderer.UpdateStartPoint(transform.position);
            wavyLineRenderer.AddTarget(targetBlocks[i]);

            yield return new WaitUntil(() => !wavyLineRenderer.IsProcessing);
        }

        targetBlocks.Clear();
        currentState = SuperCatState.Rotate;
    }

    void Update()
    {
        if (currentState == SuperCatState.Rotate)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(90f, 0, 0), Time.deltaTime * 10f);
            if (Quaternion.Angle(transform.rotation, Quaternion.Euler(90f, 0, 0)) < 0.1f)
            {
                // transform.rotation = Quaternion.Euler(90f, 0, 0);
                currentState = SuperCatState.Move;
            }
        }
        if (currentState == SuperCatState.Move)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);
            Debug.Log("Moving super cat, current position: " + (initPosition + new Vector3(0, 0, 10)));
            if (Vector3.Distance(initPosition, transform.position) >= 10f)
            {
                currentState = SuperCatState.Idel;
                transform.position = initPosition;
                gameObject.SetActive(false);
            }
        }
    }

    public enum SuperCatState
    {
        Idel,
        Move,
        Shoot,
        Rotate
    }
}
