using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{

    [SerializeField] private int width;
    [SerializeField] private GridTile tile;

    private List<GridTile> outsideTiles = new List<GridTile>();
    private int numToOutside;
    private int edgeTile;
    private GridTile infected;
    private GridTile zero;
    public AudioSource audioSource;

    public bool curePlaced;
    public bool secondInf;
    private bool music;

    public GameObject player;
    public GameObject resultWindow;
    public TextMeshProUGUI resultText;

    public Vector2[] directions = {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };

    
    // Start is called before the first frame update
    void Start()
    {
        music = false;
        secondInf = false;
        curePlaced = false;
        //These make it so that the center tile is (0,0).
        //Number of tiles from center to edge.
        numToOutside = width/2;
        //The left-most tile's x coord. Right-most is numToOutside.
        edgeTile = numToOutside*-1;
        Generate();                
    }

    void Update()
    {
        if(infected.infected && !music){
            audioSource = GetComponent<AudioSource>();
            audioSource.Play();

            music = true;
        } else if(curePlaced){
            resultText.text = "You Win";
            resultWindow.SetActive(true);
            audioSource.Stop();
        }

    }

    private void Generate(){

        int rndx;
        int rndy;
        do{
            rndx = Random.Range(-1, 2);
            rndy = Random.Range(-1, 2);
        } while (rndx == 0 && rndy == 0); 
        Debug.Log($"({rndx},{rndy})");

        player.SetActive(true);


        // Navigating x and y. (0,0) is the center.
        for(int x = edgeTile; x < numToOutside+1; x++){
            for(int y = edgeTile; y < numToOutside+1; y++){

                // Instantiates and spawns the tiles.
                var spawned = Instantiate(tile, new Vector2(x, y), Quaternion.identity);

                // Gives them their names and places them under the empty object PLATFORMS.
                spawned.name = $"Platform({x},{y})";
                spawned.transform.parent = GameObject.Find("PLATFORMS").transform;

                // If the current tile is equal to the one selected randomly, it becomes infected.
                if(x == rndx && y == rndy){
                    infected = spawned;
                }
                // Stores 0,0 tile.
                if(x == 0 && y == 0){
                    zero = spawned;
                }

                outsideTiles.Add(spawned);
            }
        }
        StartCoroutine(zero.instantiateBuilding().magicTile(this, infected, outsideTiles));
    }



    public void genGhostPlatforms(GridTile pos) {

        // Create a list that holds all positions from selected tile.
        List<Vector2> allPos = new List<Vector2>();
        foreach(Vector2 dir in directions){
            Vector2 newPos = (Vector2)pos.transform.position + dir;
        
            // Check if the neighboring position is empty.
            if (!IsPositionOccupied(newPos) && withinRange(newPos)){
                allPos.Add(newPos);
            }
        }

        // Create a ghost tiles in empty positions.
        foreach(Vector2 dir in allPos){
            var spawned = Instantiate(tile, (Vector3)dir, Quaternion.identity);

            // Casted so names don't have a million decimal places..
            int x = (int)dir.x;
            int y = (int)dir.y;

            spawned.tag = "GhostPlatform";
            spawned.gameObject.layer = LayerMask.NameToLayer("Default");
            spawned.GetComponent<SpriteRenderer>().material.color = new Color(0, 0, 0, 0);
            spawned.name = $"Platform({x},{y})";
            spawned.transform.parent = GameObject.Find("PLATFORMS").transform;
        }
    }

    // Checks selected tile to see if something is already there.
    private bool IsPositionOccupied(Vector2 position) {

        Collider2D[] colliders = Physics2D.OverlapPointAll(position);

        foreach (Collider2D collider in colliders){
            if (collider.CompareTag("Platform") || collider.CompareTag("Decayed") || collider.CompareTag("Decaying")){
                return true;
            }
        }
        return false;
    }

    // Used for checking that tiles are placed in the grid range.
    private bool withinRange(Vector2 pos){
        if(Math.Abs(pos.x) <= 9 && Math.Abs(pos.y) <= 4){
            return true;
        }
        return false;
    }
}
