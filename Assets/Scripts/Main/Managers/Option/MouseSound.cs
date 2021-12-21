using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class MouseSound : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            SoundManager.Instance.SFXSource.PlayOneShot(SoundManager.Instance.Click, 1.0f);
        }
    }

}
