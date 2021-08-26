using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShifterGenerator : MonoBehaviour
{
    public GameObject startPiece;
    public List<GameObject> rooms;
    public GameObject player;
    public int copyDistance;
    public Material prototypeMaterial;
    int currentRooms = 0;
    public  int complexity = 200;

    void Awake() {
    }

    void Start()
    {
        List<GameObject> allRooms = new List<GameObject>();
        GameObject primaryStructure = new GameObject("STRUCTURE");

        // Create start room.
        GameObject start = Instantiate(startPiece, new Vector3(0,-2,0), Quaternion.identity);
        // int startRoomIndex = Random.Range(0, startRooms.Count);
        // GameObject start = Instantiate(startRooms[startRoomIndex], new Vector3(0,0,0), Quaternion.identity);
        allRooms.Add(start);
        start.transform.parent = primaryStructure.transform;

        // Get all connections leaving start room.
        List<GameObject> connections = start.GetComponent<RoomConnections>().connections;

        // Use the queue for breadth first creation.
        Queue<GameObject> allExits = new Queue<GameObject>();
        foreach(GameObject connection in connections) {
            allExits.Enqueue(connection);
        }

        // int currentCreationAttempts = 0;
        while(currentRooms <= complexity) {
            NewRoom:

            Debug.Log("creating new room: " + currentRooms);

            // Get the exit of the room we built previously, if any exits exist in queue.
            if(allExits.Count == 0) {
                Debug.Log("no exits to dequeue. ending");
                break;
            }
            GameObject exit = allExits.Dequeue();
            Vector3 exitPosition = exit.GetComponent<Connection>().connectionPoint.position;
            Vector3 exitNormal = exit.GetComponent<Connection>().exitPlane.transform.up;

            // We will track which room indexes we have already tried to build.
            List<int> alreadyAttemptedRoomIndexes = new List<int>();

            CreateRoom:

            // Instantiate a random room.
            GameObject roomToBuild = chooseRoomToBuild(alreadyAttemptedRoomIndexes);
            GameObject room = Instantiate(roomToBuild, new Vector3(0,0,0), Quaternion.identity);

            // Store the list of connection indexes we have tried to use as an entrance.
            List<int> alreadyAttemptedEntranceIndexes = new List<int>();

            ChooseEntrance:
            
            // Ensure that room position is at origin before we move it into position.
            // It may have moved from origin if we have previously tried a different connection as entrance.
            room.transform.position = new Vector3(0,0,0);
            room.transform.rotation = Quaternion.identity;

            List<GameObject> roomConnections = room.GetComponent<RoomConnections>().connections;
            int entranceConnectionIndex = chooseEntranceIndexToUse(alreadyAttemptedEntranceIndexes, roomConnections);
            Debug.Log("trying to use as entrance connection index: " + entranceConnectionIndex);
            GameObject entranceConnection = roomConnections[entranceConnectionIndex];

            // Translate room into position.
            Vector3 entrancePosition = entranceConnection.GetComponent<Connection>().connectionPoint.position;
            room.transform.Translate(exitPosition - entrancePosition, Space.Self);

            // Rotate the room by rotating the angle between previous exit normal and new entrance normal.
            Vector3 newEntrancePosition = room.GetComponent<RoomConnections>().connections[entranceConnectionIndex].GetComponent<Connection>().connectionPoint.position;
            Vector3 newEntranceNormal = room.GetComponent<RoomConnections>().connections[entranceConnectionIndex].GetComponent<Connection>().entrancePlane.transform.up;
            float rotation = Vector3.SignedAngle(newEntranceNormal, exitNormal, Vector3.up);
            room.transform.RotateAround(newEntrancePosition, Vector3.up, rotation);

            // Check if new room intersects any previous rooms.
            Debug.Log("number of rooms added: " + allRooms.Count);
            foreach(GameObject prevRoom in allRooms) {

                // Check all bounding boxes for collisions;
                foreach(Renderer roomBoundingBox in room.GetComponent<RoomConnections>().boundingBoxes) {
                    Bounds roomBounds = roomBoundingBox.bounds;
                   
                    foreach(Renderer prevRoomBoundingBox in prevRoom.GetComponent<RoomConnections>().boundingBoxes) {
                        Bounds prevRoomBounds = prevRoomBoundingBox.bounds;

                        if(roomBounds.Intersects(prevRoomBounds)) {
                            // Draw the colliding bounding boxes for debugging.
                            Debug.Log("intersection detected with: " + prevRoom.ToString());
                            
                            // If we have another connection on this room to try as the entrance, try to use that.
                            if(alreadyAttemptedEntranceIndexes.Count != roomConnections.Count) {
                                Debug.Log("choosing new entrance to try");
                                goto ChooseEntrance;
                            } else {
                                Destroy(room);
                                if(alreadyAttemptedRoomIndexes.Count != rooms.Count) {
                                    Debug.Log("trying to add another room, current count: " + currentRooms);
                                    // If we have other rooms we could add, try them.
                                    goto CreateRoom;
                                } else {
                                    // Otherwise, create a new room.
                                    Debug.Log("no more connections to try, adding a new room new position, current count: " + currentRooms);
                                    goto NewRoom;
                                }
                                
                            }
                        }
                    }
                }
            }
            
            // Room was successfully created and does not intersect.
            currentRooms++;

            Debug.Log("adding room to allrooms"); 
            allRooms.Add(room);

            // Make the created room a child of the main parent object.
            room.transform.parent = primaryStructure.transform;

            // Add all non-entrance connections into the exit queue.
            for(int i=0; i < roomConnections.Count; i++) {
                if(i != entranceConnectionIndex) {
                    allExits.Enqueue(roomConnections[i]);
                }
            }
        }

        // Finally, disable all the bounding boxes we used for construction.
        foreach(GameObject builtRoom in allRooms) {
            foreach(Renderer boundingBox in builtRoom.GetComponent<RoomConnections>().boundingBoxes) {
                boundingBox.enabled = false;
            }
        }

        Debug.Log(allRooms.Count + " rooms created");
    }

    GameObject chooseRoomToBuild(List<int> alreadyAttemptedRoomIndexes) {
        // Construct the list of rooms we can build, omitting rooms we have already attempted.
        List<GameObject> buildableRooms = new List<GameObject>();
        for(int i=0; i<rooms.Count; i++) {
            if(!alreadyAttemptedRoomIndexes.Contains(i)) {
                buildableRooms.Add(rooms[i]);
            }
        }

        int roomIndex = Random.Range(0, buildableRooms.Count);
        GameObject roomToBuild = buildableRooms[roomIndex];
        Debug.Log("attempting to build room: " + roomToBuild.ToString());

        // Add the room index we will try to build to the list of attempted room indexes.
        alreadyAttemptedRoomIndexes.Add(roomIndex);

        return roomToBuild;
    }

    int chooseEntranceIndexToUse(List<int> alreadyAttemptedEntranceIndexes, List<GameObject> roomConnections) {
        // Add all indexes to the list of possible indexes.
        List<int> possibleIndexes = new List<int>();
        for(int i=0; i < roomConnections.Count; i++) {
            possibleIndexes.Add(i);
        }

        // Remove indexes that we have already tried.
        foreach(int alreadyAttemptedIndex in alreadyAttemptedEntranceIndexes) {
            possibleIndexes.Remove(alreadyAttemptedIndex);
        }

        // Choose a random one from the remaining options.
        int entranceConnectionIndex = possibleIndexes[Random.Range(0, possibleIndexes.Count)];

        alreadyAttemptedEntranceIndexes.Add(entranceConnectionIndex);

        return entranceConnectionIndex;
    }

    void combineMeshes(GameObject parent) {
        MeshRenderer[] meshRenderers = parent.GetComponentsInChildren<MeshRenderer>();
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();

        List<bool> meshesActive = new List<bool>();
        foreach(MeshRenderer mr in meshRenderers) {
            if(mr.enabled) {
                meshesActive.Add(true);
            } else {
                meshesActive.Add(false);
            }
        }
        
        // CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        List<CombineInstance> combine = new List<CombineInstance>();

        for(int i=0; i < meshFilters.Length; i++)
        {
            /*
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            // Disable the original meshes.
            meshFilters[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
            */

            if(meshesActive[i]) {
                 CombineInstance combined = new CombineInstance();
                combined.mesh = meshFilters[i].sharedMesh;
                combined.transform = meshFilters[i].transform.localToWorldMatrix;
                combine.Add(combined);
                meshFilters[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        // Add the mesh renderer and assign material.
        parent.AddComponent<MeshRenderer>().material = prototypeMaterial;

        // Combine the meshes into a single mesh.
        Mesh finalMesh = new Mesh();
        parent.AddComponent<MeshFilter>();
        // Increase the number of vertices the mesh can handle. https://forum.unity.com/threads/combine-meshes-creates-bad-geometry.487011/
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 
        parent.transform.GetComponent<MeshFilter>().mesh = finalMesh;
        // parent.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        parent.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine.ToArray());
    }
}

