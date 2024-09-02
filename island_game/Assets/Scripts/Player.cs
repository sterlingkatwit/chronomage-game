using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector2 = UnityEngine.Vector2;

public class Player : MonoBehaviour
{
    private List<GameObject> tilesInContact = new List<GameObject>(); // List to store tiles in contact with the player
    private GridManager gmanager;

    private Rigidbody2D rb;

    public int wisp0Count, wisp1Count, wisp2Count, wisp3Count;
    private TextMeshProUGUI wisp0Text, wisp1Text, wisp2Text, wisp3Text;
    public float moveSpeed = 5f;
    private bool floating = false;
    public bool wallCollide = false;
    public bool playerFell;

    private Animator myAnimator;

    public GameObject resultWindow;
    public TextMeshProUGUI resultText;
    private string playerName;
    public DataTransfer dataTransfer;




    private void Start()
    {
        resultWindow.SetActive(false);



        playerFell = false;
        gmanager = GameObject.Find("GridManager").GetComponent<GridManager>();
        wisp0Text = GameObject.Find("Wisp0Text").GetComponent<TextMeshProUGUI>();
        wisp1Text = GameObject.Find("Wisp1Text").GetComponent<TextMeshProUGUI>();
        wisp2Text = GameObject.Find("Wisp2Text").GetComponent<TextMeshProUGUI>();
        wisp3Text = GameObject.Find("Wisp3Text").GetComponent<TextMeshProUGUI>();

        rb = GetComponent<Rigidbody2D>();

        myAnimator = GetComponent<Animator>();

        myAnimator.SetBool("isDead", false);
        myAnimator.SetBool("isWalking", false);
        myAnimator.SetBool("isJumping", false);
        myAnimator.SetBool("isCasting", false);
        playerName = dataTransfer.playerName;

    }

    private void Update()
    {
        if(resultWindow.activeInHierarchy && Input.anyKey)
        {
            SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);

        }

        // General movement stuff
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(moveX, moveY);
        movement.Normalize();

        if (movement.magnitude > 0) // Check if there's any movement input
        {
            myAnimator.SetBool("isWalking", true); // Player is moving, set isWalking to true
                                                   // Flip sprite based on movement direction
            if (moveX < 0) // Moving left
            {
                GetComponent<SpriteRenderer>().flipX = true; // Flip sprite horizontally
            }
            else if (moveX > 0) // Moving right
            {
                GetComponent<SpriteRenderer>().flipX = false; // Flip sprite back to normal
            }
        }
        else
        {
            myAnimator.SetBool("isWalking", false); // No movement input, set isWalking to false
        }

        if (wallCollide){
            wallCollisions(Vector2.right, ref movement);
            wallCollisions(Vector2.left, ref movement);
            wallCollisions(Vector2.up, ref movement);
            wallCollisions(Vector2.down, ref movement);
        }
    
        rb.velocity = movement*moveSpeed;

        //Nicks - version before merge V1 with main
        // if(tilesInContact.Count <= 0 && !floating){
        //     myAnimator.SetBool("isDead", true);
        //     resultText.text = "You Died";
        //     resultWindow.SetActive(true);
        //     Debug.Log("You fell!");
        if(tilesInContact.Count <= 0 && !floating && !playerFell){
            gameoverFall();
            playerFell = true;
        }


        // Coroutine to float using space.
        if (Input.GetKeyDown(KeyCode.Space) && !floating && wisp1Count > 0){
            myAnimator.SetBool("isJumping", true);
            StartCoroutine(wisp1Drain());
        }

        // Checks if player is on platforms.


        updateWispCounter();
    }

    public string returnPlayerName(){
        return this.playerName;
    }

    // ////////////////////////////
    // CHECK IF YOU'RE ON PLATFORMS
    // 

    // Adds tile to list if in contact.
    private void OnTriggerEnter2D(Collider2D col){
        if(col.CompareTag("Platform") || col.CompareTag("Decaying")){
            tilesInContact.Add(col.gameObject);
        }
        if (col.CompareTag("Wall")){
            wallCollide = true;
        }
    }

    // Removes if no longer in contact.
    private void OnTriggerExit2D(Collider2D col){
        if(col.CompareTag("Platform") || col.CompareTag("Decaying")){
            tilesInContact.Remove(col.gameObject);
        }
        if (col.CompareTag("Wall")){
            wallCollide = false;
        }
    }

    private void wallCollisions(Vector2 wallDirection, ref Vector2 movement){
        float dotProduct = Vector2.Dot(movement.normalized, wallDirection);
        if (dotProduct > 0)
        {
            Vector2 parallelComponent = dotProduct * wallDirection;
            float distanceToWall = Vector2.Dot(wallDirection, transform.position);

            if (distanceToWall < 0.05f){
                parallelComponent *= distanceToWall / 100f;
            }
            movement -= parallelComponent;
        }
    }

    // 
    ////////////////////////////////
    public void gameoverFall(){
        Debug.Log("You fell!");
        gmanager.audioSource.Stop();
        myAnimator.SetBool("isDead", true);
        resultText.text = "You Died";
        resultWindow.SetActive(true);

    }

    private void updateWispCounter(){
        wisp0Text.text = "x" + wisp0Count;
        wisp1Text.text = "x" + wisp1Count;
        wisp2Text.text = "x" + wisp2Count;
        wisp3Text.text = "x" + wisp3Count;
    }


    // Drains the Wisp1 count when space is held. Allows player to float across gaps.
    IEnumerator wisp1Drain(){
        myAnimator.SetBool("isJumping", true);
        floating = true;
        Color origColor = this.GetComponent<SpriteRenderer>().material.color;

        // Check that space is being held and wisps are available.
        while(Input.GetKey(KeyCode.Space) && wisp0Count > 0){

            // Change transparency of player and remove Wisp1s every second.
            this.GetComponent<SpriteRenderer>().material.color = new Color(origColor.r, origColor.g, origColor.b, 0.3f);
            wisp1Count--;
            yield return new WaitForSeconds(0.5f);
        }

        // Set floating to false so you can fall, revert transparency.
        floating = false;
        myAnimator.speed = 1;
        myAnimator.SetBool("isJumping", false);
        this.GetComponent<SpriteRenderer>().material.color = origColor;
    }

    IEnumerator pauseJump()
    {
        myAnimator.speed = 0;
        yield return new WaitForSeconds(0.1f);
        if (!Input.GetKey(KeyCode.Space))
        {
            myAnimator.speed = 1;
        }
    }


}
