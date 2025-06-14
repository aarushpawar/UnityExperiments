using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public bool menuEnabled = false;

    GameObject nameField;
    GameObject sensitivitySlider;
    GameObject sensitivityShower;

    public string username = "Unnamed User";
    public float sensitivity = 0.5f;
    
    // Start is called before the first frame update
    void Start()
    {
        username = "User #" + Random.Range(0, 100);
        nameField = transform.GetChild(0).Find("Username").gameObject;
        sensitivitySlider = transform.GetChild(0).Find("Sensitivity").gameObject;
        sensitivityShower = sensitivitySlider.transform.Find("SensitivityShow").gameObject;

        nameField.GetComponent<TMP_InputField>().text = username;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            menuEnabled = !menuEnabled;
        }

        sensitivity = sensitivitySlider.GetComponent<Slider>().value;
        sensitivityShower.GetComponent<TextMeshProUGUI>().text = "" + Mathf.Round(sensitivity * 1000f) / 1000f;

        transform.GetChild(0).gameObject.SetActive(menuEnabled);
    }

    public void ConfirmNameChange() {
        username = nameField.GetComponent<TMP_InputField>().text;
    }

    public void Resume() {
        menuEnabled = false;
    }

    public void Quit() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        
        Application.Quit();
    }
}
