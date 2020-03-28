using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using static PickableItem;

public class PlayerController : MonoBehaviour
{
    public Transform movePoint;
    public float moveSpeed = 5f;
    public LayerMask coliderMask;
    public TMPro.TextMeshProUGUI noteText;
    public TMPro.TextMeshProUGUI keysText;
    public WebSocketController websocket;
    public Tilemap fogOfWar;
    public TileBase fogTile;
    private HashSet<Vector3Int> PositionsWithoutFog = new HashSet<Vector3Int>();
    public int keys = 0;
    public int health = 3;
    public GameObject healthObjects;

    public Dictionary<Item, int> Inventory = new Dictionary<Item, int>();
    public GameObject inventoryObjects;
    public GameObject enemiesObjects;

    void Start()
    {
        movePoint.parent = null;
        Move();
        foreach(var item in Enum.GetValues(typeof(Item))) {
            Inventory.Add((Item)item, 0);
        }
    }

    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, movePoint.position) <= .05f)
        {
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(horizontal) == 1f)
            {
                var to = new Vector3(horizontal, 0f, 0f);
                transform.localScale = new Vector3(1f * horizontal, 1f, 1f);
                var collider = Physics2D.OverlapCircle(movePoint.position + to, 0.2f, coliderMask);
                if (!collider)
                {
                    movePoint.position += to;
                    Move();
                } else if(collider.gameObject.name.StartsWith("Door") && keys > 0)
                {
                    keys--;
                    keysText.SetText(keys.ToString());

                    movePoint.position += to;
                    Move();
                    Destroy(collider.gameObject);

                }

            } else if (Mathf.Abs(vertical) == 1f)
            {
                var to = new Vector3(0f, vertical, 0f);

                var collider = Physics2D.OverlapCircle(movePoint.position + to, 0.2f, coliderMask);

                if (!collider)
                {
                    movePoint.position += to;
                    Move();
                }
                else if (collider.gameObject.name.StartsWith("Door") && keys > 0)
                {
                    keys--;
                    keysText.SetText(keys.ToString());
                    movePoint.position += to;
                    Move();
                    Destroy(collider.gameObject);
                }
            }
        }

    }

    private void Move()
    {
        var px = Mathf.FloorToInt(movePoint.position.x);
        var py = Mathf.FloorToInt(movePoint.position.y);
        for (var x = -1; x < 2; x++)
        {
            for (var y = -1; y < 2; y++)
            {
                PositionsWithoutFog.Add(new Vector3Int(px + x, py + y, 0));
                fogOfWar.SetTile(new Vector3Int(px + x, py + y, 0), null);
            }
        }

        for (var x = -11; x < 11; x++)
        {
            for (var y = -9; y < 9; y++)
            {
                var pos = new Vector3Int(px + x, py + y, 0);
                if (!PositionsWithoutFog.Contains(pos))
                {
                    fogOfWar.SetTile(pos, fogTile);
                }
            }
        }

        TickAll();
    }

    public void OnNoteAddClicked()
    {
        Debug.Log("sending note");
        websocket.AddNote("Trap ahead!" + UnityEngine.Random.Range(0, 5), Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), 0, false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var note = other.GetComponent<NoteController>();
        if (note != null)
        {
            noteText.SetText(note.note.Text);
            Debug.Log("Trigger Enter " + other.tag);

        }

        Debug.Log(other.gameObject.name);
        if(other.gameObject.name == "Key")
        {
            Destroy(other.gameObject);
            keys++;
            keysText.SetText(keys.ToString());
        }

        var pickable = other.gameObject.GetComponent<PickableItem>();
        if (pickable)
        {
            AddItem(pickable.Type);
            Destroy(pickable.gameObject);
        }

        var trap = other.gameObject.GetComponent<Trap>();
        if (trap)
        {
            trap.Activate(this);
        }

        var ladder = other.gameObject.GetComponent<Ladder>();
        if(ladder)
        {
            ladder.Teleport(this);
        }
    }

    public void TickAll()
    {
        for(int i = 0; i < enemiesObjects.transform.childCount; i++)
        {
            var child = enemiesObjects.transform.GetChild(i).GetComponent<EnemyController>();
            if(child)
            {
                child.Tick();
            }
            
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("take damage" + damage);
        health -= damage;
        if(health <= 0)
        {
            // DED
            websocket.AddNote("", Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), 0, true);
        }
        for (int i = 0; i < 3; i++)
        {
            var obj = healthObjects.transform.GetChild(i).GetComponent<Animator>();
            obj.SetBool("Active", i + 1 <= health);
        }
    }

    public void AddItem(Item item)
    {
        Inventory[item]++;
        for(int i = 0; i < Inventory.Count; i++)
        {
            inventoryObjects.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().SetText(Inventory[(Item)i].ToString());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var note = other.GetComponent<NoteController>();
        if (note != null)
        {
            noteText.SetText("");
            Debug.Log("Trigger Exit " + other.tag);
        }

        var ladder = other.gameObject.GetComponent<Ladder>();
        if (ladder)
        {
            ladder.waiting = false;
        }
    }
}
