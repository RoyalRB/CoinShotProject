using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    Transform cameraTransform;

    //It's up here for debug reasons (I would keep it native to the function if I was working with particle effects instead :P)
    List<GameObject> coinShotOrder;

    Color[] debugColors = {
        new Color(255, 255, 255), //White
        new Color(255, 0, 0), //Red
        new Color(0, 255, 0), //Green
        new Color(0, 0, 255) //Blue
    };

    bool drawDebugRays;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = transform.parent.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 100.0f))
            {
                //Checking if the item being shot is a coin
                if (hit.transform.CompareTag("Coin"))
                {
                    coinShotOrder = new List<GameObject>(hit.transform.GetComponent<Coin>().CoinShot());

                    //Check if there are no enemies to hit (null value), if there are enemies, however, draw the rays like normal
                    if (coinShotOrder[coinShotOrder.Count - 1] == null) 
                    {
                        Debug.Log("No rays are being drawn, as there are no enemies provided");
                    }
                    else
                    {
                        drawDebugRays = true;
                    }
                }
            }
        }

        if (drawDebugRays)
        {
            DrawRaysBetweenCoins();
        }
    }

    //For debugging, so you can see the path travelled throught the coins to the enemy
    void DrawRaysBetweenCoins()
    {
        for (int i = 0; i < coinShotOrder.Count - 1; i++)
        {
            Debug.DrawRay(coinShotOrder[i].transform.position, coinShotOrder[i + 1].transform.position - coinShotOrder[i].transform.position, debugColors[i]);
        }

        if(coinShotOrder.Count > 2)
        {
            int coinCount = coinShotOrder.Count;
            Debug.DrawRay(coinShotOrder[coinCount - 2].transform.position, coinShotOrder[coinCount - 1].transform.position - coinShotOrder[coinCount - 2].transform.position, debugColors[coinCount - 2]);
        }
    }
}
