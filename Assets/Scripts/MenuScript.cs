using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    bool menuEnabled = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape)) {
            menuEnabled = !menuEnabled;

        }

        transform.GetChild(0).gameObject.SetActive(menuEnabled);
    }

    public void ConfirmNameChange() {

    }

    public void Resume() {

    }

    public void Quit() {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
        
        Application.Quit();
    }
}
