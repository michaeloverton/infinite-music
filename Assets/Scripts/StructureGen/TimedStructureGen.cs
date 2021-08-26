using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedStructureGen : MonoBehaviour
{
    // public GameObject player;
    public List<GameObject> startRooms;
    public List<GameObject> rooms;
    public int maxRooms;
    public bool debugMode;
    int currentRooms = 0;
    List<GameObject> generatedRooms;
    float elapsed = 0f;

    void Start()
    {
        generateStructure();
    }

    void generateStructure() {
        List<GameObject> allRooms = new List<GameObject>();

        // Create start room.
        int startRoomIndex = Random.Range(0, startRooms.Count);
        GameObject start = Instantiate(startRooms[startRoomIndex], new Vector3(0,0,0), Quaternion.identity);
        allRooms.Add(start);

        // Get all connections leaving start room.
        List<GameObject> connections = start.GetComponent<RoomConnections>().connections;

        // Use the queue for breadth first creation.
        Queue<GameObject> allExits = new Queue<GameObject>();
        foreach(GameObject connection in connections) {
            allExits.Enqueue(connection);
        }

        // int currentCreationAttempts = 0;
        while(currentRooms <= maxRooms) {
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

        generatedRooms = allRooms;
    }

    void Update() {
        elapsed += Time.deltaTime;
        if (elapsed >= 5f) {
            elapsed = elapsed % 1f;
            
            // Destroy previously created rooms.
            foreach(GameObject room in generatedRooms) {
                Destroy(room);
            }

            // Reset current rooms count.
            currentRooms = 0;

            // Generate new structure.
            generateStructure();
        }
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

