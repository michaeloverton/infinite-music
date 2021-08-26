using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Worms() {
        SceneManager.LoadScene("Worms");
    }

    public void NextTune() {
        StructureComplexity structureComplexity = GameObject.FindObjectOfType<StructureComplexity>();
        if(structureComplexity == null) {
            return;
        }

        structureComplexity.nextTune();
    }
}
