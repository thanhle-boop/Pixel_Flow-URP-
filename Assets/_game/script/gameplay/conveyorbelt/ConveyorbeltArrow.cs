using System.Collections.Generic;
using UnityEngine;

public partial class ConveyorbeltArrow : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] float speedAnim = 1;

    bool isPause = false;   
    float valueCore = 0;

    private void Update()
    {
        if(isPause) return;

        valueCore -= speedAnim * Time.deltaTime;
        lineRenderer.material.SetFloat("_TextureCoreModify_X", valueCore);
    }

    public void SetPauseAnimConveyorbelt(bool val)
    {
        isPause = val;
    }
}
