using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Xml;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics;
using UnityEngine.UIElements;

public class Cards : MonoBehaviour
{
    Card[] board = new Card[9];
    bool isPlacing = false;
    System.Random rnd = new System.Random();

    int UILayer;
    public class Card
    {
        public string name;
        public int[] placement;
        public bool own;
        public int playEffectID;
        public int passiveID;


        public Card()
        {
            this.name = "Turtle";
            this.placement = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            this.own = true;
            this.playEffectID = -1;
            this.passiveID = 0;
        }

        public Card(string n, int[] pl, int pE, int p)
        {
            this.name = n;
            this.placement = pl;
            this.own = true;
            this.playEffectID = pE;
            this.passiveID = p;
        }
        public String getName()
        {
            return this.name;
        }
        public int[] getplacement()
        {
            return this.placement;
        }
        public bool getOwn()
        {
            return this.own;
        }
        public int getPlayEffectID()
        {
            return this.playEffectID;
        }
        public int getPassiveID()
        {
            return this.passiveID;
        }
    }
    Card[] deck;
    Card[] oppDeck = new Card[3];
    Card[] handCards = new Card[3];
    GameObject[] handObjects = new GameObject[3];
    float[] cardx = new float[] { -0.727f, 1.06f, 2.847f };
    String phase = "action";

    void Start()
    {
        deck = new Card[Turtles.Length];
        deck[0] = new Card("Turtle", new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, -1, 0);
        deck[1] = new Card("Spy Turtle", new int[] { 1, 3, 5, 7, 9 }, 0, 1);
        deck[2] = new Card("Defender Turtle", new int[] { 1, 3, 7, 9 }, -1, 2);
        deck[3] = new Card("Magic Turtle", new int[] { 2, 4, 6, 8 }, 1, 3);

        oppDeck = new Card[Turtles.Length];
        oppDeck[0] = new Card("Turtle", new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, -1, 0);
        oppDeck[1] = new Card("Spy Turtle", new int[] { 1, 3, 5, 7, 9 }, 0, 1);
        oppDeck[2] = new Card("Defender Turtle", new int[] { 1, 3, 7, 9 }, -1, 2);
        oppDeck[3] = new Card("Magic Turtle", new int[] { 2, 4, 6, 8 }, 1, 3);
        foreach (Card i in oppDeck)
        {
            i.own = false;
        }

        for (int i = 0; i < handCards.Length; i++)
        {
            int randDraw = rnd.Next(0, deck.Length);
            handCards[i] = deck[randDraw];
            deck[randDraw] = null;
            for (int j = 0; j < deck.Length; j++)
            {
                if (deck[j] == null)
                {
                    deck[j] = deck[deck.Length - 1];
                    Array.Resize(ref deck, deck.Length - 1);
                    break;
                }
            }
        }

        handObjects[0] = Instantiate(Turtles[handCards[0].getPassiveID()], new Vector3(cardx[0], 0.9855669f, -4.71751f), Quaternion.identity);
        handObjects[0].transform.Rotate(-24.838f, 0, 0);
        handObjects[0].layer = LayerMask.NameToLayer("Hand");

        handObjects[1] = Instantiate(Turtles[handCards[1].getPassiveID()], new Vector3(cardx[1], 0.9855669f, -4.71751f), Quaternion.identity);
        handObjects[1].transform.Rotate(-24.838f, 0, 0);
        handObjects[1].layer = LayerMask.NameToLayer("Hand");

        handObjects[2] = Instantiate(Turtles[handCards[2].getPassiveID()], new Vector3(cardx[2], 0.9855669f, -4.71751f), Quaternion.identity);
        handObjects[2].transform.Rotate(-24.838f, 0, 0);
        handObjects[2].layer = LayerMask.NameToLayer("Hand");
    }
    Ray ray;
    RaycastHit hit;
    public bool IsPointerOverUIElement()
    {
        return Physics.Raycast(ray, out hit);
    }

    public GameObject[] Turtles;
    GameObject cardPrefab;

    public GameObject[] highlights;

    public Material yes;
    public Material no;
    public GameObject deckObject;

    void disableHighlights()
    {
        foreach (GameObject highlight in highlights)
        {
            highlight.SetActive(false);
        }
    }
    void yesHighlights()
    {
        foreach (GameObject highlight in highlights)
        {
            highlight.GetComponent<MeshRenderer> ().material = yes;
        }
    }
    void noHighlights()
    {
        foreach (GameObject highlight in highlights)
        {
            highlight.GetComponent<MeshRenderer> ().material = no;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        if (isPlacing == false && phase == "action")
        {
            if (Physics.Raycast(ray, out raycastHit))
            {
                for (int i = 0; i < handObjects.Length; i++)
                {
                    if (handObjects[i] != null)
                    {
                        if (raycastHit.collider.gameObject == handObjects[i])
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                StartCoroutine(placing(handCards[i], handObjects[i], i));
                                isPlacing = true;
                            }
                            else
                            {
                                handObjects[i].transform.position = new Vector3(handObjects[i].transform.position.x, 1.18f, -4.3f);
                            }
                        }
                        else
                        {
                            handObjects[i].transform.position = new Vector3(handObjects[i].transform.position.x, 0.9855669f, -4.71751f);
                        }
                    }
                }
            }
            else
            {
                foreach (GameObject i in handObjects)
                {
                    if (i != null)
                    {
                        i.transform.position = new Vector3(i.transform.position.x, 0.9855669f, -4.71751f);
                    }
                }
            }

        }

        if (phase == "draw")
        {
            if (deck.Length == 0)
            {
                phase = "opponent";
            }
            else if (Physics.Raycast(ray, out raycastHit) && raycastHit.collider.gameObject == deckObject && handCards.Contains(null) && Input.GetMouseButtonDown(0))
            {
                Debug.Log("Drawing a card");
                int randDraw = rnd.Next(0, deck.Length);
                Card drawnCard = deck[randDraw];
                deck[randDraw] = null;
                for (int i = 0; i < deck.Length; i++)
                {
                    if (deck[i] == null)
                    {
                        deck[i] = deck[deck.Length - 1];
                        Array.Resize(ref deck, deck.Length - 1);
                        break;
                    }
                }
                for (int i = 0; i < handCards.Length; i++)
                {
                    if (handCards[i] == null)
                    {
                        handCards[i] = drawnCard;
                        handObjects[i] = Instantiate(Turtles[drawnCard.getPassiveID()], new Vector3(cardx[i], 0.9855669f, -4.71751f), Quaternion.identity);
                        handObjects[i].transform.Rotate(-24.838f, 0, 0);
                        handObjects[i].layer = LayerMask.NameToLayer("Hand");
                        break;
                    }
                }
                if (!handCards.Contains(null))
                {
                    phase = "opponent";
                }
            }
        }

        if (phase == "opponent" )
        {
            Card drawnCard = null;
            int randDraw = rnd.Next(0, oppDeck.Length);
            drawnCard = oppDeck[randDraw];
            oppDeck[randDraw] = null;
            for (int i = 0; i < oppDeck.Length; i++)
            {
                if (oppDeck[i] == null)
                {
                    oppDeck[i] = oppDeck[oppDeck.Length - 1];
                    Array.Resize(ref oppDeck, oppDeck.Length - 1);
                    break;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (board[i] == null && drawnCard.getplacement().Contains(i + 1))
                {
                    board[i] = drawnCard;
                    float cardX = 2.706f;
                    float cardZ = 1.689f;
                    if (i == 0 || i == 3 || i == 6)
                    {
                        cardX = 0.92f;
                    }
                    if (i == 2 || i == 5 || i == 8)
                    {
                        cardX = 4.465f;
                    }
                    if (i == 0 || i == 1 || i == 2)
                    {
                        cardZ = 4.502f;
                    }
                    if (i == 6 || i == 7 || i == 8)
                    {
                        cardZ = -1.118f;
                    }
                    GameObject cardPrefab = Instantiate(Turtles[drawnCard.getPassiveID()], new Vector3(cardX, 0, cardZ), Quaternion.identity);
                    cardPrefab.GetComponent<Rigidbody>().useGravity = true;
                    cardPrefab.layer = LayerMask.NameToLayer("Board");
                    cardPrefab.transform.Rotate(0, 180f, 0);
                    break;
                }
            }
            phase = "action";
        }
    }
    IEnumerator placing(Card currentCard, GameObject cardPrefab, int handIndex)
    {
        Debug.Log("Card selected: " + currentCard.getName()); //debug
        cardPrefab = Instantiate(Turtles[currentCard.getPassiveID()], new Vector3(2.706f, 0, 1.689f), Quaternion.identity);

        cardPrefab.GetComponent<Rigidbody>().useGravity = false;
        float initialX = Input.mousePosition.x;
        float initialY = Input.mousePosition.y;
        float cardX = 2.706f;
        float cardY = 0;
        float cardZ = 1.689f;
        yield return new WaitForSeconds(0.001f);
        int selectedSquare = 0;

        while (isPlacing)
        {
            if (Input.mousePosition.x + 50 < initialX)
            {
                cardX = 0.92f;
                if (Input.mousePosition.y - 50 > initialY)
                {
                    //Pos 1
                    disableHighlights();
                    highlights[0].SetActive(true);
                    cardZ = 4.502f;
                    selectedSquare = 1;
                }
                else if (Input.mousePosition.y + 50 < initialY)
                {
                    //Pos 7
                    disableHighlights();
                    highlights[6].SetActive(true);
                    cardZ = -1.118f;
                    selectedSquare = 7;
                }
                else
                {
                    //Pos 4
                    disableHighlights();
                    highlights[3].SetActive(true);
                    cardZ = 1.689f;
                    selectedSquare = 4;
                }
            }
            else if (Input.mousePosition.x - 50 > initialX)
            {
                cardX = 4.465f;
                if (Input.mousePosition.y - 50 > initialY)
                {
                    //Pos 3
                    disableHighlights();
                    highlights[2].SetActive(true);
                    cardZ = 4.502f;
                    selectedSquare = 3;
                }
                else if (Input.mousePosition.y + 50 < initialY)
                {
                    //Pos 9
                    disableHighlights();
                    highlights[8].SetActive(true);
                    cardZ = -1.118f;
                    selectedSquare = 9;
                }
                else
                {
                    //Pos 6
                    disableHighlights();
                    highlights[5].SetActive(true);
                    cardZ = 1.689f;
                    selectedSquare = 6;
                }
            }
            else
            {
                cardX = 2.706f;
                if (Input.mousePosition.y - 50 > initialY)
                {
                    //Pos 2
                    disableHighlights();
                    highlights[1].SetActive(true);
                    cardZ = 4.502f;
                    selectedSquare = 2;
                }
                else if (Input.mousePosition.y + 50 < initialY)
                {
                    //Pos 8
                    disableHighlights();
                    highlights[7].SetActive(true);
                    cardZ = -1.118f;
                    selectedSquare = 8;
                }
                else
                {
                    //Pos 5
                    disableHighlights();
                    highlights[4].SetActive(true);
                    cardZ = 1.689f;
                    selectedSquare = 5;
                }
            }
            if (currentCard.getplacement().Contains(selectedSquare) && board[selectedSquare - 1] == null)
            {
                yesHighlights();
                if (Input.GetMouseButtonDown(0))
                {
                    isPlacing = false;
                    board[selectedSquare - 1] = currentCard;
                    handCards[handIndex] = null;
                    Destroy(handObjects[handIndex]);

                    phase = "draw";
                }
            }
            else
            {
                noHighlights();
            }
            Debug.Log(initialX+" "+initialY);
            if (initialX - 200 > Input.mousePosition.x)
            {
                initialX -= 60;
            }
            if (initialX + 200 < Input.mousePosition.x)
            {
                initialX += 60;
            }
            if (initialY - 200 > Input.mousePosition.y)
            {
                initialY -= 60;
            }
            if (initialY + 200 < Input.mousePosition.y)
            {
                initialY += 60;
            }
            yield return new WaitForSeconds(0.000001f);
            cardPrefab.transform.position = new Vector3(cardX, cardY, cardZ);
        }
        yield return new WaitForSeconds(0.001f);
        cardPrefab.GetComponent<Rigidbody>().useGravity = true;
        cardPrefab.GetComponent<Rigidbody>().isKinematic = false;
        cardPrefab.layer = LayerMask.NameToLayer("Board");
        Debug.Log(currentCard.getName() + " placed");
        disableHighlights();
    }
}
