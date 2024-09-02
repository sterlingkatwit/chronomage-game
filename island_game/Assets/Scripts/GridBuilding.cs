using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

public class GridBuilding : MonoBehaviour
{

    [SerializeField] private GameObject wisp0;
    [SerializeField] private GameObject wisp1;
    [SerializeField] private GameObject wisp2;
    [SerializeField] private GameObject wisp3;
    [SerializeField] private GameObject magicTileShader;
    [SerializeField] private Sprite magicTileSprite;
    private BoxCollider2D c2d;
    public bool complete = false;
    public bool active = false;
    public bool inContact = false;

    private GameObject[] resultWindow;

    // Start is called before the first frame update
    void Start()
    {
        c2d = GetComponent<BoxCollider2D>();
        resultWindow = GameObject.FindGameObjectsWithTag("resultWin");
        // gmanager = GameObject.Find("GridManager").GetComponent<GridManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }


    // Didn't intend to put player through here but maybe it's better.
    public IEnumerator buildDrill(GridTile pos, Player player){
        this.GetComponent<Renderer>().material.color = new Color(0.75f, 0.8f, 0.85f);
        int progress = 0;
        int totalTicks = 20;

        //myAnimator.SetBool("isCasting ", true);
        player.wisp2Count -= 5;

        // Generates wisps for 60 seconds, 15 total ticks. Lower odds than decaying platforms.
        while(progress < totalTicks && active){
            yield return new WaitForSeconds(3);
            progress++;
            pos.genWisp(Random.Range(-3,3), wisp0);
            pos.genWisp(Random.Range(-3,2), wisp1);
            pos.genWisp(Random.Range(-4,3), wisp2);
        }
        complete = true;
        yield break;
    }


    // If player is in contact with the recycler, trade 1 wisp1 for
    // either 1 wisp0 or 1 wisp2.
    public IEnumerator buildRecycler(GridTile pos, Player player){
        int progress = 0;
        this.GetComponent<Renderer>().material.color = new Color(0.56f, 0.93f, 0.56f);

        player.wisp2Count -= 5;


        while(true){
            if(Input.GetKey(KeyCode.F)){
                if(active && inContact){
                    player.wisp1Count--;
                    if(Random.Range(0,2) == 0){
                        pos.genWisp(1, wisp0);
                    } else {
                        pos.genWisp(1, wisp2);
                    }
                }
                yield return new WaitForSeconds(0.75f);
            }
            if(progress == 60){
                this.complete = true;
                break;
            }
            progress++;
            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator buildReclaimer(GridTile pos, Player player){
        this.GetComponent<Renderer>().material.color = Color.blue;//new Color(0.56f, 0.93f, 0.56f); 
        int progress = 0;

        player.wisp2Count -= 5;
        Debug.Log("Reclaimer placed. Ticks=" + pos.totalTicks);

        while(progress < pos.totalTicks){
            progress++;
            Debug.Log("Reclaim progress: " + progress + "/" + pos.totalTicks);
            yield return new WaitForSeconds(2);
        }
        if(progress == pos.totalTicks){
            pos.genWisp(1, wisp3);
            pos.setPlatform(true);
            pos.hasBuilding = false;
            Destroy(this.gameObject);
        }
        yield break;
    }

    public IEnumerator magicTile(GridManager man, GridTile inf, List<GridTile> outside){
        var spawned = Instantiate(magicTileShader, this.transform.position, Quaternion.identity);
        spawned.transform.localScale = new Vector2(1.1f, 1.1f);
        this.GetComponent<Renderer>().material.color = Color.gray;//new Color(0.56f, 0.93f, 0.56f); 
        this.GetComponent<SpriteRenderer>().sprite = magicTileSprite;
        this.transform.localScale = new Vector2(0.7f, 0.8f);

        while(!complete){
            if(Input.GetKey(KeyCode.F)){
                complete = true;
                Timer.StartTimer();
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }

        // Creates infected and initiates ghost tiles.
        // Add timer start.
        inf.infected = true;
        foreach(GridTile makeGhosts in outside){
            man.genGhostPlatforms(makeGhosts);
        }
        Destroy(spawned.gameObject);
        yield break;
    }

    public IEnumerator cureAll(GridManager gmanager, GridTile pos, Player player){
        this.GetComponent<Renderer>().material.color = Color.red;//new Color(0.56f, 0.93f, 0.56f); 
        player.wisp3Count -= 3;
        gmanager.curePlaced = true;

        yield return new WaitForSeconds(3);

        //showResult(true); maybe make a winManager?? and it calls this function?

        complete = true;
    }

    //bad and not working T
    private void showResult(bool win)
    {
        if(win)
        {
            if(resultWindow[0].name == "ResultWindow")
            {
                resultWindow[0].SetActive(true);
                //resultWindow[1].
            }
            else
            {
                resultWindow[1].SetActive(true);
                //resultWindow[1].
            }
        }
    }



    private void OnTriggerEnter2D(Collider2D col){
        if(col.gameObject.CompareTag("Player")){
            this.inContact = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col){
        if(col.gameObject.CompareTag("Player")){
            this.inContact = false;
        }
    }
}
