using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LetraPalabra : MonoBehaviour
{

    public TMP_Text textContainer;
    public int index = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void SetLetter(string letter)
    {
        textContainer.text = letter;
    }
}
