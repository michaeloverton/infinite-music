using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverController : MonoBehaviour
{
    public float maxHoverAmount = 200f;
    public float minHoverAmount = 30f;
    public float speedMultiplier = 0.5f;
    private List<GameObject> hoverables = new List<GameObject>();
    private List<float> topHeights = new List<float>();
    private List<float> bottomHeights = new List<float>();
    private List<float> hoverAmounts = new List<float>();
    private List<bool> movingUp = new List<bool>();
    private bool hoverStuff = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        if(hoverStuff) {
            for(int i=0; i<hoverables.Count; i++) {
                GameObject hoverable = hoverables[i];

                // If we're past the hover amount, reverse the direction.
                if(hoverable.transform.position.y > topHeights[i]) {
                    movingUp[i] = false;
                }
                if(hoverable.transform.position.y < bottomHeights[i]) {
                    movingUp[i] = true;
                }

                hoverable.transform.Translate(new Vector3(0, hoverAmounts[i] * speedMultiplier * Time.fixedDeltaTime * (movingUp[i] ? 1 : -1), 0));
            }
        }
    }

    public void addHoverable(GameObject hoverable) {
        hoverables.Add(hoverable);
        float originalHeight = hoverable.transform.position.y;
        float hoverAmount = Random.Range(minHoverAmount, maxHoverAmount);
        hoverAmounts.Add(hoverAmount);
        topHeights.Add(originalHeight + hoverAmount);
        bottomHeights.Add(originalHeight - hoverAmount);
        if(Random.Range(0, 100) > 50) {
            movingUp.Add(true);
        } else {
            movingUp.Add(false);
        }
    }

    public void startHovering() {
        hoverStuff = true;
    }
}
