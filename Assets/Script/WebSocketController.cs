using NativeWebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class Note
{
    public string Text;
    public int X;
    public int Y;
    public int DungeonId;
    public bool IsDead;
}

[Serializable]
public class Packet
{
    public string id;
    public Note[] notes;
}

[Serializable]
public class SendNotePacket : Note
{
    public string id = "STORE_NOTE";
}
public class WebSocketController : MonoBehaviour
{
    private WebSocket websocket;
    public GameObject noteObject;
    public GameObject deadObject;
    public GameObject notes;

    // Start is called before the first frame update
    async void Start()
    {
        websocket = new WebSocket("ws://undrfined.me:" + 0x1337);

        Debug.Log("Connecting to websocket");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            websocket.SendText("{\"id\": \"GET_ALL_NOTES\"}");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            Debug.Log("OnMessage!");
            Debug.Log(bytes);

            // getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            var pk = JsonUtility.FromJson<Packet>(message);
            foreach(var note in pk.notes)
            {
                var gm = Instantiate(note.IsDead ? deadObject : noteObject, new Vector3(0.5f + (float)note.X, 0.5f + (float)note.Y, 0f), Quaternion.identity, notes.transform);
                gm.GetComponent<NoteController>().note = note;
            }
            // Debug.Log("OnMessage! " + message);
        };

        await websocket.Connect();

    }

    public string CurrentSecret = "";

    public void FixedUpdate()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            if(CurrentSecret == "")
                CurrentSecret += "P";
            else CurrentSecret = "";
        } else if (Input.GetKeyDown(KeyCode.I))
        {
            if (CurrentSecret == "P")
                CurrentSecret += "I";
            else CurrentSecret = "";
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (CurrentSecret == "PI")
                CurrentSecret += "D";
            else CurrentSecret = "";
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            if (CurrentSecret == "PID")
                CurrentSecret += "O";
            else CurrentSecret = "";
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (CurrentSecret == "PIDO")
            {

                websocket.SendText("{\"id\": \"CLEAR\"}");
                for (var i = 0; i < notes.transform.childCount; i++)
                {
                    Destroy(notes.transform.GetChild(i).gameObject);
                }
                CurrentSecret = "";

            }
            else CurrentSecret = "";
        }
    }

    public void AddNote(string text, int x, int y, int dungeonId, bool isDead)
    {
        var note = new SendNotePacket
        {
            Text = text,
            X = x,
            Y = y,
            DungeonId = dungeonId,
            IsDead = isDead
        };
        Debug.Log("Sending note" + note.Text);
        websocket.SendText(JsonUtility.ToJson(note));

        var gm = Instantiate(isDead ? deadObject : noteObject, new Vector3(0.5f + (float)note.X, 0.5f + (float)note.Y, 0f), Quaternion.identity);
        gm.GetComponent<NoteController>().note = note;
    }
}
