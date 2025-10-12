using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;
    void Start()
    {
        ActivateTab(0); //inventory always active first.
    }

    // Update is called once per frame
    public void ActivateTab(int tabNumber){
        for(int i = 0; i < pages.Length; i++){
            pages[i].SetActive(false);
            tabImages[i].color = Color.grey;
        }
        pages[tabNumber].SetActive(true);
        tabImages[tabNumber].color = Color.white;
    }
}
