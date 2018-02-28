using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteDirection
{
    Left,
    Up,
    Down,
    Right
}

public enum Perfectness
{
    Bad,
    Good,
    Perfect
}

public class Note : MonoBehaviour {
    
    public NoteDirection direction;
    public GameObject fireworkPrefab;

    private GameObject note;
    private bool canMove = false;
    private Rigidbody rigidbody;
    private GameManager gameManager;

    public Note() { }

    public void InitializeNote(NoteDirection direction, GameObject notePrefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        this.direction = direction;
        this.note = notePrefab;
    }

    // Use this for initialization
    void Start ()
    {
        fireworkPrefab = Resources.Load<GameObject>("Prefabs/Firework");
  
        gameManager = FindObjectOfType<GameManager>();
        Move();
    }
	
	// Update is called once per frame
	void Update ()
    {
		if(canMove)
        {
            transform.Translate(-Vector3.forward * Example.speed);
        }
    }

    public void Move()
    {
        canMove = true;
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name.Contains("Good Collider"))
        {
            if (CheckClicked())
            {
                gameManager.AddCombo();
                GameObject firework = Instantiate(fireworkPrefab, transform.position, Quaternion.identity, transform.parent);
                Destroy(firework, 1f);
                Destroy(gameObject);

                gameManager.SetPerfectness(Perfectness.Good);
                gameManager.Heal();
            }
        }
        else if (other.gameObject.name.Contains("Button"))
        {
            if (CheckClicked())
            {
                gameManager.AddCombo();
                GameObject firework = Instantiate(fireworkPrefab, transform.localPosition, Quaternion.identity, transform.parent);
                Destroy(firework, 1f);
                Destroy(gameObject);

                gameManager.SetPerfectness(Perfectness.Perfect);
                gameManager.Heal();
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("Button"))
        {
            Destroy(gameObject);
            gameManager.ResetCombo();

            gameManager.SetPerfectness(Perfectness.Bad);
            gameManager.Damage();
        }
    }

    private bool CheckClicked()
    {
        bool clicked = false;
        switch (direction)
        {
            case NoteDirection.Left:
                if (Input.GetKey(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    clicked = true;
                }
                break;
            case NoteDirection.Up:
                if (Input.GetKey(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    clicked = true;
                }
                break;
            case NoteDirection.Down:
                if (Input.GetKey(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    clicked = true;
                }
                break;
            case NoteDirection.Right:
                if (Input.GetKey(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    clicked = true;
                }
                break;
        }
        return clicked;
    }
}
