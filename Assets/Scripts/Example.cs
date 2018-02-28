using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Example : MonoBehaviour
{

    public GameObject barPrefab, spectrumHolder, noteSpawner, noteHolder;
    public GameObject leftNote, upNote, downNote, rightNote;
    public GameObject leftButton, upButton, downButton, rightButton;
    public GameObject[] spawners = new GameObject[4];
    public static float speed = 0.3f;
    public AudioSource audioSource;

    private GameObject fireworkPrefab;
    private float[] spectrums;

    private int sampleSize = 12;
    private int[] numberList = new int[] { -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6 };
    private float[] xPosList = new float[] { -3f, -1f, 1f, 3f };
    private int beatCounter = 0;

    private GameObject[] objectList;
    private GameManager gameManager;
    private Scrollbar scrollbar;
    private bool gameEnded = false;

    private float[] buttonYPos;
    private Color[] buttonColors;

    private void Awake()
    {
        fireworkPrefab = Resources.Load<GameObject>("Prefabs/Firework");
    }

    void Start()
    {
        //Select the instance of AudioProcessor and pass a reference
        //to this object
        AudioProcessor processor = FindObjectOfType<AudioProcessor>();
        processor.onBeat.AddListener(onOnbeatDetected);
        processor.onSpectrum.AddListener(onSpectrum);

        gameManager = FindObjectOfType<GameManager>();
        gameManager.GameEnd.AddListener(StopGame);
        gameManager.GameStart.AddListener(RestartGame);

        objectList = new GameObject[sampleSize];
        foreach (int i in numberList)
        {
            GameObject g = Instantiate(barPrefab);
            g.transform.localPosition = new Vector3(i, 1, 0);
            g.transform.localRotation = spectrumHolder.transform.localRotation;
            g.transform.parent = spectrumHolder.transform;
            objectList[i + 5] = g;
        }

        // buttonYPos = new float[4] { leftButton.transform.position.y, upButton.transform.position.y, downButton.transform.position.y, rightButton.transform.position.y };
        buttonYPos = new float[4] { 2.6f, 2.6f, 2.6f, 2.6f };
        buttonColors = new Color[4] { leftButton.GetComponent<MeshRenderer>().material.color, upButton.GetComponent<MeshRenderer>().material.color, downButton.GetComponent<MeshRenderer>().material.color, rightButton.GetComponent<MeshRenderer>().material.color };

        scrollbar = FindObjectOfType<Scrollbar>();
        scrollbar.onValueChanged.AddListener(OnScollbarValueChanged);

        Camera.main.gameObject.GetComponent<AudioSource>().clip = audioSource.clip;
        Camera.main.gameObject.GetComponent<AudioSource>().Play();
        StartCoroutine(PlayMusicInDelay((1f / speed) * 0.7f, audioSource));
    }

    private void Update()
    {
        scrollbar.value = audioSource.time / audioSource.clip.length;

        CheckButtonPress();

        if(gameEnded && Input.GetKeyDown(KeyCode.Space))
        {
            gameManager.GameStart.Invoke();
        }
    }

    //this event will be called every time a beat is detected.
    //Change the threshold parameter in the inspector
    //to adjust the sensitivity
    void onOnbeatDetected()
    {
        if (noteSpawner.activeInHierarchy)
        {
            //Reset Beat Counter
            if (beatCounter > 11)
            {
                beatCounter = 0;
            }
            Instantiate(fireworkPrefab, new Vector3(beatCounter - 4f, 0f, 0f), Quaternion.identity, spectrumHolder.transform);
            beatCounter++;

            if(UnityEngine.Random.Range(0,101) % 2 == 0)
            {
                int index = UnityEngine.Random.Range(0, 4);
                NoteDirection randomDirection = (NoteDirection)index;

                Vector3 pos = spawners[index].transform.position;
                Quaternion quat = spawners[index].transform.rotation;
                GameObject randomNote = Instantiate(RandomNote(randomDirection), pos, quat, noteHolder.transform);
                Note note = randomNote.GetComponent<Note>();
                note.InitializeNote(randomDirection, randomNote, pos, quat, noteHolder.transform);
            }
        }

    }

    //This event will be called every frame while music is playing
    void onSpectrum(float[] spectrum)
    {
        //The spectrum is logarithmically averaged
        //to 12 bands
        spectrums = spectrum;

        if(!gameEnded)
        {
            for (int i = 0; i < spectrum.Length; i++)
            {
                Vector3 start = new Vector3(i, 0, 0);
                Vector3 end = new Vector3(i, spectrum[i] * 100f, 0);
                Debug.DrawLine(start, end, Color.blue);

                //Visualization
                Vector3 scale = new Vector3(0.5f, spectrums[i] * 100f, 0.5f);
                objectList[i].transform.localScale = scale;
                Vector3 pos = objectList[i].transform.localPosition;
                pos.y = 0;
                pos.y += objectList[i].transform.localScale.y / 2f;
                objectList[i].transform.localPosition = pos;
            }
        }
        else
        {
            for (int i = 0; i < spectrum.Length; i++)
            {
                //Visualization
                Vector3 scale = objectList[i].transform.localScale;
                Vector3 noYScale = new Vector3(scale.x, 0f, scale.z);
                objectList[i].transform.localScale = Vector3.Lerp(objectList[i].transform.localScale, noYScale, 0.1f);
               
                Vector3 pos = objectList[i].transform.localPosition;
                pos.y = 0;
                pos.y -= objectList[i].transform.localScale.y / 2f;
                objectList[i].transform.localPosition = pos;
            }
        }
    }

    void OnScollbarValueChanged(float currentValue)
    {
        audioSource.time = Mathf.Min(currentValue * audioSource.clip.length, audioSource.clip.length - 0.1f);
    }

    GameObject noteToReturn;

    float RandomNoteXPos(NoteDirection dir)
    {
        float noteXPos = 0f;

        switch (dir)
        {
            case NoteDirection.Left:
                noteXPos = xPosList[0];
                break;
            case NoteDirection.Up:
                noteXPos = xPosList[1];
                break;
            case NoteDirection.Down:
                noteXPos = xPosList[2];
                break;
            case NoteDirection.Right:
                noteXPos = xPosList[3];
                break;
        }

        return noteXPos;
    }

    GameObject RandomNote(NoteDirection dir)
    {

        switch (dir)
        {
            case NoteDirection.Left:
                noteToReturn = leftNote;
                break;
            case NoteDirection.Up:
                noteToReturn = upNote;
                break;
            case NoteDirection.Down:
                noteToReturn = downNote;
                break;
            case NoteDirection.Right:
                noteToReturn = rightNote;
                break;
            default:
                noteToReturn = upNote;
                break;
        }

        return noteToReturn;
    }

    void CheckButtonPress()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            leftButton.GetComponent<MeshRenderer>().material.color = Color.gray;
        }
        else
        {
            leftButton.GetComponent<MeshRenderer>().material.color = buttonColors[0];
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            upButton.GetComponent<MeshRenderer>().material.color = Color.gray;
        }
        else
        {
            upButton.GetComponent<MeshRenderer>().material.color = buttonColors[1];
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            downButton.GetComponent<MeshRenderer>().material.color = Color.gray;
        }
        else
        {
            downButton.GetComponent<MeshRenderer>().material.color = buttonColors[2];
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            rightButton.GetComponent<MeshRenderer>().material.color = Color.gray;
        }
        else
        {
            rightButton.GetComponent<MeshRenderer>().material.color = buttonColors[3];
        }

    }

    IEnumerator StopMusicInDelay(float waitTime, AudioSource aS)
    {
        yield return new WaitForSeconds(waitTime);
        aS.Stop();
    }

    IEnumerator PlayMusicInDelay(float waitTime, AudioSource aS)
    {
        yield return new WaitForSeconds(waitTime);
        aS.Play();
    }

    public void RestartGame()
    {
        noteSpawner.SetActive(true);
        StartCoroutine(PlayMusicInDelay((1f / speed) * 0.7f, audioSource));
        Camera.main.GetComponent<AudioSource>().Play();
        gameEnded = false;
    }

    public void StopGame()
    {
        noteSpawner.SetActive(false);
        
        Note[] allNote = FindObjectsOfType<Note>();
        foreach (Note note in allNote)
        {
            Destroy(note.gameObject);
        }

        gameEnded = true;
        audioSource.Stop();
        StartCoroutine(StopMusicInDelay(1f, Camera.main.GetComponent<AudioSource>()));
    }
}

