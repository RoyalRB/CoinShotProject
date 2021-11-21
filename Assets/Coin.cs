using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] LayerMask blocksCoinPathLayer; //In this case, everything that's on the "environment" layer should be considered an obstacle to the coin path

    //When the coin is shot, calculate the path the bullet will take along the coins. The coins will look for their nearest neighbor and will check if anything is blocking the path, and then adjust accordingly.
    public List<GameObject> CoinShot()
    {
        GameObject[] coinsInHierarchy = GameObject.FindGameObjectsWithTag("Coin"); //Note: There should be no other coins apart from the ones thrown by the player. So any coins in the hierarchy would've been thrown by the player
        List<GameObject> dispersedCoins = new List<GameObject>(coinsInHierarchy);
        List<GameObject> returnCoinOrderList = new List<GameObject>();

        Vector3 referencePosition = transform.position;
        float minDistance = 0f;

        if (coinsInHierarchy.Length > 2) //If the coin is shot, that means that there's always at least one coin in the hierarchy, so if there's more, then the rest of the code will apply
        {
            //Removing the shot coin (this one) so that it won't be used in the distance comparison
            for (int i = 0; i < dispersedCoins.Count; i++)
            {
                if (dispersedCoins[i] == gameObject)
                {
                    dispersedCoins.RemoveAt(i);
                }
            }

            returnCoinOrderList.Add(gameObject);

            //Comparing the distances between the coins. The result should be a list containing the order that the coins will be linked in
            while (dispersedCoins.Count != 1)
            {
                minDistance = (referencePosition - dispersedCoins[0].transform.position).sqrMagnitude;
                int minIndex = 0;
                int pathsBlocked = 0;

                for (int j = 0; j < dispersedCoins.Count; j++)
                {
                    bool coinPathBlocked = Physics.Linecast(referencePosition, dispersedCoins[j].transform.position, blocksCoinPathLayer);

                    if (coinPathBlocked) //If there's an obstacle blocking the path
                    {
                        pathsBlocked++;

                        if(pathsBlocked == dispersedCoins.Count)
                        {
                            returnCoinOrderList = new List<GameObject>(ScanForEnemy(returnCoinOrderList)); //Add the final enemy to the list as the last step the coin rays take
                            return returnCoinOrderList;
                        }
                    }
                    else //If the path towards the next coin is free -> calculate the distance
                    {
                        float checkedDistance = (referencePosition - dispersedCoins[j].transform.position).sqrMagnitude;

                        if (checkedDistance < minDistance)
                        {
                            minDistance = checkedDistance;
                            minIndex = j;
                        }
                    }
                }

                //If all of the paths have been checked for a coin
                Debug.Log(minIndex + ", " + minDistance + ", " + dispersedCoins[minIndex].name);
                referencePosition = dispersedCoins[minIndex].transform.position;
                returnCoinOrderList.Add(dispersedCoins[minIndex]);
                dispersedCoins.RemoveAt(minIndex);
            }

            //If all the coins have been checked for potential paths
            Debug.Log("Finished with " + dispersedCoins[0].name);
            returnCoinOrderList.Add(dispersedCoins[0]);
            returnCoinOrderList = new List<GameObject>(ScanForEnemy(returnCoinOrderList)); //Add the final enemy to the list as the last step the coin rays take
            return returnCoinOrderList;
        }
        else //If there's only one coin in the hierarchy -> skip the distance comparison
        {
            List<GameObject> returnSelfAndEnemy = new List<GameObject>();
            returnSelfAndEnemy.Add(gameObject);
            returnSelfAndEnemy = new List<GameObject>(ScanForEnemy(returnSelfAndEnemy));
            return returnSelfAndEnemy;
        }
    }

    //Look if the last coin in the sequence can hit any of the nearby enemies, if not, roll back to a previous coin and check from there. If there are none, return null
    List<GameObject> ScanForEnemy(List<GameObject> coinOrder)
    {
        int coinCheckIndex = coinOrder.Count - 1;
        Vector3 coinPosition = coinOrder[coinCheckIndex].transform.position; //The last coin's position
        Collider[] enemiesNearby = Physics.OverlapSphere(coinPosition, 50f, enemyLayer); //Check for an enemy gameobject within a 50 unit radius
        List<GameObject> listWithEnemy = new List<GameObject>(coinOrder);

        if (enemiesNearby.Length > 0) //If there are enemies nearby
        {
            GameObject enemyToReturn = enemiesNearby[0].gameObject;

            int pathsBlocked = 0;
            bool coinPathBlocked = false;

            if (enemiesNearby.Length > 1) //If there's more than one enemy nearby
            {
                float minDistance = (enemiesNearby[0].transform.position - coinPosition).sqrMagnitude;

                for (int i = 0; i < enemiesNearby.Length; i++)
                {
                    coinPathBlocked = Physics.Linecast(coinPosition, enemiesNearby[i].transform.position, blocksCoinPathLayer);

                    if (coinPathBlocked) //If there is an obstacle blocking the path towards an enemy
                    {
                        pathsBlocked++;

                        if(pathsBlocked == enemiesNearby.Length) //If the checked coin has no possible paths to any of the enemies
                        {
                            if(coinCheckIndex != 0) //If it isn't the first coin that's being checked for possible paths
                            {
                                //Reset the loop and roll back to the previous coin to check if any paths are possible to an enemy
                                coinCheckIndex -= 1;
                                coinPosition = coinOrder[coinCheckIndex].transform.position;
                                pathsBlocked = 0;
                                i = -1;
                                listWithEnemy.RemoveAt(listWithEnemy.Count - 1);
                                continue;
                            }
                            else //If there's no possible way to get a path towards any enemy
                            {
                                Debug.Log("There are no ways to get a clear shot on the enemy, so the coin will shoot out into space");
                                return null;
                            }
                        }
                    }
                    else //If nothing is blocking the path towards an enemy -> distance check
                    {
                        float checkedDistance = (enemiesNearby[i].transform.position - coinPosition).sqrMagnitude;

                        if (checkedDistance < minDistance)
                        {
                            minDistance = checkedDistance;
                            enemyToReturn = enemiesNearby[i].gameObject;
                        }
                    }
                }
            }
            else //If there's only one enemy nearby
            {
                //Look for every coin if there is a path towards the only enemy nearby
                for (int i = coinOrder.Count - 1; i > -1; i--)
                {
                    coinPathBlocked = Physics.Linecast(coinOrder[i].transform.position, enemiesNearby[0].transform.position, blocksCoinPathLayer);

                    if (coinPathBlocked)
                    {
                        listWithEnemy.RemoveAt(i);

                        if(i == 0)
                        {
                            listWithEnemy.Add(null); //The null value will prevent the rays from being drawn. This line is to prevent logerrors from an empty list.
                            Debug.Log("There are no ways to get a clear shot on the enemy, so the coin will shoot out into space");
                            return listWithEnemy;
                        }

                        continue;
                    }
                    else
                    {
                        enemyToReturn = enemiesNearby[0].transform.gameObject;
                        break;
                    }
                }
            }

            Debug.Log(enemyToReturn.name);
            listWithEnemy.Add(enemyToReturn);
            return listWithEnemy;
        }
        else //If there are no enemies near
        {
            Debug.Log("There are no enemies nearby, so the coin will shoot out into space");
            listWithEnemy.Add(null); //The null value will prevent the rays from being drawn. This line is to prevent logerrors from an empty list.
            return listWithEnemy;
        }

    }
}
