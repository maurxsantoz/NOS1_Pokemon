using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class UseTooltipScript : MonoBehaviour
{
    public string Message;
    void OnMouseEnter()
    {
        TooltipScript._instance.ShowTooltip(Message);
    }
    void OnMouseExit()
    {
        TooltipScript._instance.HideTooltip();
    }
}
