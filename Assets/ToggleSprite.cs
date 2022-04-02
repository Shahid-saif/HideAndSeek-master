using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSprite : MonoBehaviour
{
    [SerializeField] GameObject toggleSprite;


    public void ToggleToggle()
    {
        toggleSprite.gameObject.SetActive(!toggleSprite.gameObject.active);
    }
}
