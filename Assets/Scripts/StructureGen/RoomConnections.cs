using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomConnections : MonoBehaviour
{
    public bool isStartRoom = false;
    public List<GameObject> connections;
    public Transform playerStart;
    // We use a Renderer instead of Collider because Colliders seemed to be translating incorrectly.
    public List<Renderer> boundingBoxes;
    // Do not set in editor. This is set by room generation algorithm.
    // parentRoom is the room that this room was spawned from.
    GameObject parentRoom;
    // public GameObject childRoom;
    // public GameObject roomGen;

    public void setParentRoom(GameObject room) {
        parentRoom = room;
    }

    public GameObject getParentRoom() {
        return parentRoom;
    }

    public void destroyParentRoom() {
        Destroy(parentRoom);
    }

    // THIS IS A TEST!!!
    void OnTriggerEnter(Collider other) {
        // Debug.Log("parent room position: " + roomObject.GetComponent<RoomConnections>().parentRoom.transform.position);
        if(getParentRoom() != null) {
            destroyParentRoom();
        }
    }


}
