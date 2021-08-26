using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureGenNonVR : MonoBehaviour
{
    public GameObject startPiece;
    public List<GameObject> startRooms;
    public List<GameObject> rooms;
    public GameObject player;
    public int copies;
    public int copyDistance;
    public bool debugMode;
    public Material prototypeMaterial;
    int currentRooms = 0;
    private int complexity;

    void Awake() {
        // Set the complexity, which has not been destroyed.
        StructureComplexity structureComplexity = GameObject.FindObjectOfType<StructureComplexity>();
        complexity = structureComplexity.getComplexity();
        Debug.Log("COMPLEXITY: " + complexity);
    }

    void Start()
    {
        List<GameObject> allRooms = new List<GameObject>();
        GameObject primaryStructure = new GameObject("STRUCTURE");

        // Create start room.
        GameObject start = Instantiate(startPiece, new Vector3(0,0,0), Quaternion.identity);
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
                            drawDebugRenderer(roomBoundingBox);
                            drawDebugRenderer(prevRoomBoundingBox);
                            
                            // If we have another connection on this room to try as the entrance, try to use that.
                            if(alreadyAttemptedEntranceIndexes.Count != roomConnections.Count) {
                                Debug.Log("choosing new entrance to try");
                                createDebugCube(roomBounds.center);
                                goto ChooseEntrance;
                            } else {
                                Destroy(room);
                                if(alreadyAttemptedRoomIndexes.Count != rooms.Count) {
                                    Debug.Log("trying to add another room, current count: " + currentRooms);
                                    // If we have other rooms we could add, try them.
                                    goto CreateRoom;
                                } else {
                                    createDebugSphere(roomBounds.center);
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

        primaryStructure.isStatic = true;

        // Combine all meshes into a single mesh for optimization.
        combineMeshes(primaryStructure);

        // Create eerie copies of the primary structure.
        for(int i=1; i <= copies; i++) {
            GameObject one = Instantiate(primaryStructure, new Vector3(copyDistance*i,0,0), Quaternion.Euler(0, Random.Range(0,360), 0));
            one.isStatic = true;
            GameObject two = Instantiate(primaryStructure, new Vector3(0,0,copyDistance*i), Quaternion.Euler(0, Random.Range(0,360), 0));
            two.isStatic = true;
            GameObject three = Instantiate(primaryStructure, new Vector3(-1*copyDistance*i,0,0), Quaternion.Euler(0, Random.Range(0,360), 0));
            three.isStatic = true;
            GameObject four = Instantiate(primaryStructure, new Vector3(0,0,-1*copyDistance*i), Quaternion.Euler(0, Random.Range(0,360), 0));
            four.isStatic = true;
        }

        // Instantiate the player object.
        // Instantiate(player, new Vector3(0, 3, 0), Quaternion.Euler(0, 0, 0));

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
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for(int i=0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            // Disable the original meshes.
            meshFilters[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
        }

        // Add the mesh renderer and assign material.
        parent.AddComponent<MeshRenderer>().material = prototypeMaterial;

        // Combine the meshes into a single mesh.
        Mesh finalMesh = new Mesh();
        parent.AddComponent<MeshFilter>();
        // Increase the number of vertices the mesh can handle. https://forum.unity.com/threads/combine-meshes-creates-bad-geometry.487011/
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 
        parent.transform.GetComponent<MeshFilter>().mesh = finalMesh;
        parent.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
    }

    void createDebugSphere(Vector3 pos) {
        if(debugMode) {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = pos;
            sphere.transform.localScale = new Vector3(2,2,2);
        }
    }

    void createDebugCube(Vector3 pos) {
        if(debugMode) {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = pos;
            cube.transform.localScale = new Vector3(2,2,2);
        }
    }

    void drawDebugRenderer(Renderer rend) {
        if(debugMode) {
            Instantiate(rend, rend.transform.position, rend.transform.rotation);
        }
    }
}

