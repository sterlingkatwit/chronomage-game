using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wisp : MonoBehaviour
{

    Player player;
    Rigidbody2D rb;
    public int wispNum;
    public float slowAmount;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();

        Destroy(this.gameObject, 20);
    }

    // Update is called once per frame
    void Update()
    {
        SloWisp(slowAmount);
    }


    void SloWisp(float amount){
        Vector3 slowForce = -rb.velocity * amount;

        rb.AddForce(slowForce);
    }


    private void OnTriggerEnter2D(Collider2D col){
        if(col.CompareTag("Player")){
            switch(wispNum){
                case 0:
                    player.wisp0Count++;
                break;
                case 1:
                    player.wisp1Count++;
                break;
                case 2:
                    player.wisp2Count++;
                break;
                case 3:
                    player.wisp3Count++;
                break;
            }
            Destroy(this.gameObject);
        }
    }

}
