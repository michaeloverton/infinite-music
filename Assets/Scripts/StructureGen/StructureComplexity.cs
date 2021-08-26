using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructureComplexity: MonoBehaviour
{
    private float complexity;
    private float copies;
    private int tune;
    public Slider complexitySlider;
    public TextMeshProUGUI complexityText;
    public Slider copiesSlider;
    public TextMeshProUGUI copiesText;
    public List<AudioSource> ambientTunes;

    void Awake() {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
	{
		//Adds a listener to the complexity slider and invokes a method when the value changes.
		complexitySlider.onValueChanged.AddListener (delegate {ComplexityValueChangeCheck ();});
        complexity = complexitySlider.value;
        complexityText.SetText(complexitySlider.value.ToString());

        //Adds a listener to the copies slider and invokes a method when the value changes.
		copiesSlider.onValueChanged.AddListener (delegate {CopiesValueChangeCheck ();});
        copies = copiesSlider.value;
        copiesText.SetText(copiesSlider.value.ToString());

        // Select tune.
        float tuneIndex = Random.Range(0,3);
        if(tuneIndex < 1) {
            tune = 0;
            ambientTunes[0].Play();
        } else if(tuneIndex >= 1 && tuneIndex < 2) {
            tune = 1;
            ambientTunes[1].Play();
        } else {
            tune = 2;
            ambientTunes[2].Play();
        }
	}

    public  void ComplexityValueChangeCheck() {
        complexity = complexitySlider.value;
        complexityText.SetText(complexitySlider.value.ToString());
    }

    public  void CopiesValueChangeCheck() {
        copies = copiesSlider.value;
        copiesText.SetText(copiesSlider.value.ToString());
    }

    public int getComplexity() {
        return (int)complexity;
    }

    public int getCopies() {
        return (int)copies;
    }

    public void nextTune() {
        ambientTunes[tune].Stop();

        tune = (tune + 1) % ambientTunes.Count;
        ambientTunes[tune].Play();
    }
}