using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridTile : MonoBehaviour
{
    [SerializeField] private GameObject highlight;
    [SerializeField] private GridBuilding building;
    [SerializeField] private UIManager uimanager;
    [SerializeField] private GameObject wisp0;
    [SerializeField] private GameObject wisp1;
    [SerializeField] private GameObject wisp2;
    [SerializeField] private GameObject wisp3;
    [SerializeField] private Material decayMaterial;
    [SerializeField] private Sprite decay1, decay2, decay3;

    private GridManager gmanager;
    private Player player;
    private GridBuilding assginedBuilding;
    private ParticleSystem ps;
    private Material origMat;
    private Sprite origSprite;
    private AudioSource decaySFX;

    public LayerMask mask;
    public LayerMask forceMask;

    public bool hasBuilding = false;
    public int totalTicks;
    public bool infected = false;
    private bool decaying = false;
    private int progress = 0;

    private bool scoreboardRun = false;

    // Start is called before the first frame update
    void Start()
    {
        // This stuff is bad, might try to find better way but the ui one wasnt
        // working when I was trying other methods.
        ps = GetComponent<ParticleSystem>();
        uimanager = FindObjectOfType<UIManager>();
        decaySFX = GetComponent<AudioSource>();
        player = GameObject.Find("Player").GetComponent<Player>();
        gmanager = GameObject.Find("GridManager").GetComponent<GridManager>();
        origMat = this.GetComponent<SpriteRenderer>().material;
        origSprite = this.GetComponent<SpriteRenderer>().sprite;
    }

    void Update()
    {
        if(gmanager.curePlaced){
            if(infected && decaying){
                setPlatform(true);
                this.GetComponent<SpriteRenderer>().material = origMat;
                var psColor = ps.main;
                psColor.startColor = new Color(0.34f, 0.29f, 0.81f);
                ps.Play();
            }
            StopAllCoroutines();

            if(!scoreboardRun){
                //scoreboard stuff and timer
                Timer.StopTimer();
                if(player.returnPlayerName().Length > 0){
                    ScoreboardManager.addScore(player.returnPlayerName(), Timer.GetEndTime());
                    ScoreboardManager.saveScores();
                }
                scoreboardRun = true;
            }
        }

        if(player.playerFell){
            StopAllCoroutines();    
        }

        if(infected && !decaying){
            decaying = true;
            // Normal = 1
            // Medium = .8
            // Hard = .6
            StartCoroutine(decay(1));
        }

        if(assginedBuilding != null){
            if(assginedBuilding.complete && hasBuilding){
                Destroy(this.assginedBuilding.gameObject);
                this.hasBuilding = false;
            }
        }

        // Also move this to timer function maybe? Shouldn't be here.
        // Add second infection when time reaches 1 minute.
        if(Timer.GetCurrentTime() == 60){
            if(GameObject.FindWithTag("Platform") != null && !decaying){
                GridTile oldestTile = GameObject.FindWithTag("Platform").GetComponent<GridTile>();
                oldestTile.infected = true;
            }
        }

    }

    // ////////////////////////
    // Game functions 

    IEnumerator decay(float multi){
        progress = 0;
        this.gameObject.tag = "Decaying";

        if(!gmanager.secondInf){
            StartCoroutine(startSecondInf());
            gmanager.secondInf = true;
        }

        // Destroys buildings if there is one present at start of decay.
        if(hasBuilding){
            assginedBuilding.complete = true;
            Destroy(assginedBuilding.gameObject);
            hasBuilding = false;
        }

        // Ticks down until platform has finished decaying
        while(progress < totalTicks && !gmanager.curePlaced){
            progress++;
            ps.Play();

            if(progress <= 2){
                this.GetComponent<SpriteRenderer>().sprite = decay1;
            } else if(progress <= 4){
                this.GetComponent<SpriteRenderer>().sprite = decay2;
            } else if(progress < totalTicks){
                this.GetComponent<SpriteRenderer>().sprite = decay3;
            }
            yield return new WaitForSeconds(2*multi);
            genWisp(Random.Range(0,3), wisp0);
            genWisp(Random.Range(-1,2), wisp1);
            genWisp(Random.Range(-2,3), wisp2);
        }
        if(progress == totalTicks){

            decaySFX.Play();
            this.gameObject.tag = "Decayed";
            this.GetComponent<SpriteRenderer>().material = decayMaterial;
            origMat.color = new Color(origMat.color.r, origMat.color.g, origMat.color.b, 0f);

            int numAdj = getAdjacent().Count; 
            if(numAdj > 1){
                // If there's more than one adjacent tile, pick a random one and set it to infected.
                getAdjacent()[Random.Range(0, numAdj)].gameObject.GetComponent<GridTile>().infected = true;
            } 
            else if(numAdj == 1){
                // If only one, just set it to infected.
                getAdjacent()[0].gameObject.GetComponent<GridTile>().infected = true;
            } 
            else {
                // Infects the oldest platform if none are adjacent.
                if(GameObject.FindWithTag("Platform") != null){
                    GridTile oldestTile = GameObject.FindWithTag("Platform").GetComponent<GridTile>();
                    oldestTile.infected = true;
                }
                // If there are no tiles to infect, games probably over.
                else{
                    Debug.Log("Game over");
                }
            }
            
        }
    }


    public void genWisp(int numWiSpawned, GameObject type){
        float angleVariation = 20f;
        Vector3 hitPos;
        Vector3 currentPos = this.transform.position;

        if(player.gameObject != null){
            hitPos = player.transform.position;

            // Spawn the wisp(s) and apply force
            for(int i = 0; i < numWiSpawned; i++){

                // Instantiate and get the rigidbody
                var wispSpawn = Instantiate(type, currentPos, Quaternion.identity);
                wispSpawn.transform.parent = GameObject.Find("WISPS").transform;
                Rigidbody2D wispBody = wispSpawn.GetComponent<Rigidbody2D>();
                wispBody.velocity = Vector3.zero;

                // Find distance between player and current tile, then add random multiplier.
                float distance = Vector3.Distance(player.transform.position, currentPos);
                distance *= Random.Range(1.1f, 1.6f);

                // Adds variety to the angle the wisps are sent.
                Vector2 toPlayer = (hitPos * distance).normalized; 
                float randomVariation = Random.Range(-angleVariation, angleVariation);

                // Send the wisp in direction of the player with a slight variety to the angle.
                wispBody.AddForce(Quaternion.Euler(0f, 0f, randomVariation) * toPlayer, ForceMode2D.Impulse);
            }
        }
    }



    void OnMouseEnter(){
        if(this.CompareTag("Platform") || this.CompareTag("GhostPlatform")){
            highlight.SetActive(true);
        }
        else if(this.CompareTag("Decayed") && player.wisp2Count >= 5){
            highlight.SetActive(true);
        }
    }
    void OnMouseExit(){
        highlight.SetActive(false);
    }


    void OnMouseDown(){
        // All this is doing is creating / reverting platforms. 
        // Do some other stuff when decided.

        // Ghost platforms > Real platforms
        if(this.CompareTag("GhostPlatform"))
        {
            if(player.wisp0Count >= 5){
                player.wisp0Count -= 5;
                setPlatform(false);
            }
            else{
                Debug.Log("Not enough wisps!");
            }
        }

        // Decayed platforms > Real platforms
        else if(this.CompareTag("Decayed") && uimanager.currentBuilding.Equals("Reclaimer"))
        {             
            if(player.wisp2Count >= 5){
                StartCoroutine(instantiateBuilding().buildReclaimer(this, player));
            }
            else{
                Debug.Log("Not enough wisps!");
            }
        }

        // For building spawns.
        else if(this.CompareTag("Platform") && !decaying && !hasBuilding)
        {
            // Check which building is currently present on UI.
            switch(uimanager.currentBuilding){
                case "Drill":
                    // If they have the resources to build, build corresponding building.
                    if(player.wisp2Count >= 5){
                        StartCoroutine(instantiateBuilding().buildDrill(this, player));
                    }
                break;
                case "Recycler":
                    if(player.wisp2Count >= 5){
                        StartCoroutine(instantiateBuilding().buildRecycler(this, player));
                    }
                break;
                case "The Cure":
                    if(player.wisp3Count >= 3){
                        StartCoroutine(instantiateBuilding().cureAll(gmanager, this, player));
                    }
                break;
            }
        }
    }


    // //////////////////////////////
    // Helpers


    List<GridTile> getAdjacent(){
        
        List<GridTile> adjacentTiles = new List<GridTile>();

        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        foreach(Vector2 dir in gmanager.directions){

            // Create raycasts
            RaycastHit2D hit = Physics2D.Raycast(this.transform.position, dir, 1f, mask);

            if(hit.collider != null && hit.collider.CompareTag("Platform")){

                // If raycast hits another tile add it to the list.
                GridTile tile = hit.collider.gameObject.GetComponent<GridTile>();
                adjacentTiles.Add(tile);
            }
        }
        this.gameObject.layer = LayerMask.NameToLayer("Ground");

        return adjacentTiles;
    }

    public void setPlatform(bool decayPlat){
        // Resets platforms / places them.
        if(decayPlat){
            this.infected = false;
            this.decaying = false;
        }
        this.gameObject.tag = "Platform";
        this.gameObject.layer = LayerMask.NameToLayer("Ground");
        this.GetComponent<SpriteRenderer>().sprite = origSprite;
        this.GetComponent<SpriteRenderer>().material = origMat;
        this.GetComponent<Renderer>().material.color = new Color(0.525f,1f,0.647f);
        this.hasBuilding = false;
        gmanager.genGhostPlatforms(this);
    }

    public GridBuilding instantiateBuilding(){
        hasBuilding = true;
        assginedBuilding = Instantiate(building, this.transform.position, Quaternion.identity);
        assginedBuilding.transform.parent = GameObject.Find("BUILDINGS").transform;
        assginedBuilding.active = true;
        return assginedBuilding;
    }

    public IEnumerator startSecondInf(){
        yield return new WaitForSeconds(60);
        GridTile oldest = GameObject.FindWithTag("Platform").GetComponent<GridTile>();
        if (oldest != null) {
            oldest.infected = true;
        } 
    }

    public void gameoverWin(){
        
    }
}
