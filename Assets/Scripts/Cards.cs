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
using FishNet.Connection;
using FishNet.Object;
using FishNet.Example.ColliderRollbacks;
using FishNet.Example.Scened;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using NUnit.Framework;

public class Cards : NetworkBehaviour
{
    public Card[] board = new Card[9];
    public GameObject[] boardObj = new GameObject[9];
    public bool isPlacing = false;
    public bool isChoosing = false;
    private bool player1;
    public int players = 0;
    public float[] cardx; // = new float[] { -0.727f, 1.06f, 2.847f, 4.634f }
    public float cardz; //-4.71751f
    public float baseCardRot; //-4.71751
    public float cardZHover; //-0.41751f
    System.Random rnd = new System.Random();

    int UILayer;
    public class Card
    {
        public string name;
        public int[] placement;
        public bool own;
        public int playEffectID;
        public int passiveID;
        public int target;


        public Card()
        {
            this.name = "Turtle";
            this.placement = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            this.own = true;
            this.playEffectID = -1;
            this.passiveID = 0;
            this.target = -1;
        }

        public Card(string n, int[] pl, int pE, int p)
        {
            this.name = n;
            this.placement = pl;
            this.own = true;
            this.playEffectID = pE;
            this.passiveID = p;
            this.target = -1;
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
    Card[] oppDeck;
    Card[] handCards = new Card[3];
    GameObject[] handObjects = new GameObject[3];
    String phase = "action";
    private Camera playerCamera;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            return;
        }
        GlobalVariables playerScript = GameObject.Find("Globals").GetComponent<GlobalVariables>();
        players = playerScript.players.Value;
        if (players == 1)
        {
            playerCamera = GameObject.Find("P1_Camera").GetComponent<Camera>();
            Debug.Log(playerCamera.name + " Camera");
            GameObject P2_Camera = GameObject.Find("P2_Camera");
            P2_Camera.SetActive(false);
        }
        else if (players == 2)
        {
            //GetComponent<Cards>().enabled = false;
            playerCamera = GameObject.Find("P2_Camera").GetComponent<Camera>();
            Debug.Log(playerCamera.name + " Camera");
            GameObject P1_Camera = GameObject.Find("P1_Camera");
            P1_Camera.SetActive(false);
        }
        Debug.Log(players);
        playerScript.AddPlayer(playerScript, 1);
    }
    void Start()
    {
        deck = new Card[Turtles.Length];
        deck[0] = new Card("Turtle", new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, -1, 0);
        deck[1] = new Card("Spy Turtle", new int[] { 1, 3, 5, 7, 9 }, 0, 1);
        deck[2] = new Card("Defender Turtle", new int[] { 1, 3, 7, 9 }, -1, 2);
        deck[3] = new Card("Magic Turtle", new int[] { 2, 4, 6, 8 }, 1, 3);
        deck[4] = new Card("Attacker Turtle", new int[] { 2, 4, 6, 8 }, -1, 4);
        deck[5] = new Card("Business Turtle", new int[] { 1, 2, 3, 4, 6, 7, 8, 9 }, -1, 5);
        deck[6] = new Card("Evil Turtle", new int[] { 1, 3, 5, 7, 9 }, -1, 6);
        deck[7] = new Card("War Turtle", new int[] { 1, 3, 7, 9 }, 2, 7);

        oppDeck = new Card[3];
        oppDeck[0] = new Card("Evil Turtle", new int[] { 1, 3, 5, 7, 9 }, -1, 6);
        oppDeck[1] = new Card("Evil Turtle", new int[] { 1, 3, 5, 7, 9 }, -1, 6);
        oppDeck[2] = new Card("Evil Turtle", new int[] { 1, 3, 5, 7, 9 }, -1, 6);
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

        handObjects[0] = Instantiate(Turtles[handCards[0].getPassiveID()], new Vector3(cardx[0], 0.9855669f, cardz), Quaternion.identity);
        handObjects[0].transform.Rotate(0,baseCardRot,0);
        handObjects[0].layer = LayerMask.NameToLayer("Hand");
        ServerManager.Spawn(handObjects[0]);

        handObjects[1] = Instantiate(Turtles[handCards[1].getPassiveID()], new Vector3(cardx[1], 0.9855669f, cardz), Quaternion.identity);
        handObjects[1].transform.Rotate(0,baseCardRot,0);
        handObjects[1].layer = LayerMask.NameToLayer("Hand");
        ServerManager.Spawn(handObjects[1]);

        handObjects[2] = Instantiate(Turtles[handCards[2].getPassiveID()], new Vector3(cardx[2], 0.9855669f, cardz), Quaternion.identity);
        handObjects[2].transform.Rotate(0,baseCardRot,0);
        handObjects[2].layer = LayerMask.NameToLayer("Hand");
        ServerManager.Spawn(handObjects[2]);
        
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
            highlight.GetComponent<MeshRenderer>().material = yes;
        }
    }
    void noHighlights()
    {
        foreach (GameObject highlight in highlights)
        {
            highlight.GetComponent<MeshRenderer>().material = no;
        }
    }
    bool effectActive = false;

    // Update is called once per frame
    void Update()
    {
        if (!base.IsOwner)
        {
            return;
        }
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        if (!effectActive && !isChoosing)
        {
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
                                    break;
                                }
                                else
                                {
                                    handObjects[i].transform.position = new Vector3(handObjects[i].transform.position.x, 1.18f, -4.71751f + cardZHover); //-0.41751
                                }
                            }
                            else
                            {
                                handObjects[i].transform.position = new Vector3(handObjects[i].transform.position.x, 0.9855669f, cardz);
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
                            i.transform.position = new Vector3(i.transform.position.x, 0.9855669f, cardz);
                        }
                    }
                }

            }

            if (phase == "draw")
            {
                bool isBusiness = false;
                foreach (Card c in board) //Business Turtle
                {
                    if (c != null && c.getPassiveID() == 5 && c.getOwn())
                    {
                        isBusiness = true;
                        Array.Resize(ref handCards, 4);
                        Array.Resize(ref handObjects, 4);
                    }
                }
                if (!isBusiness && handCards.Length > 3)
                {
                    List<Card> newHandCards = new List<Card>();
                    List<GameObject> newHandObjects = new List<GameObject>();
                    for (int j = 0; j < handCards.Length; j++)
                    {
                        if (handCards[j] != null)
                        {
                            newHandCards.Add(handCards[j]);
                            newHandObjects.Add(handObjects[j]);
                        }
                    }
                    for (int k = 0; k < newHandCards.Count && k < 3; k++)
                    {
                        handCards[k] = newHandCards[k];
                        handObjects[k] = newHandObjects[k];
                    }
                    Array.Resize(ref handCards, 3);
                    Array.Resize(ref handObjects, 3);
                }


                int ownCount = 0;
                int oppCount = 0;
                foreach (Card c in board) //Evil Turtle
                {
                    if (c == null)
                    {
                        continue;
                    }
                    else if (c.getOwn())
                    {
                        ownCount++;
                    }
                    else
                    {
                        oppCount++;
                    }
                }
                foreach (Card c in board)
                {
                    if (c != null && c.getPassiveID() == 6 && !c.getOwn())
                    {
                        if (handCards.Length > 2)
                        {
                            if (handCards[2] == null && handCards[3] == null)
                            {
                                Array.Resize(ref handCards, 2);
                                Array.Resize(ref handObjects, 2);
                            }
                            else
                            {
                                isChoosing = true;
                                StartCoroutine(choosing());
                                break;
                            }
                        }
                    }
                }

                if (deck.Length == 0 || !handCards.Contains(null)) //Draw
                {
                    phase = "opponent";
                }
                else if (Physics.Raycast(ray, out raycastHit) && raycastHit.collider.gameObject == deckObject && handCards.Contains(null) && Input.GetMouseButtonDown(0))
                {
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
                            handObjects[i] = Instantiate(Turtles[drawnCard.getPassiveID()], new Vector3(cardx[i], 0.9855669f, cardz), Quaternion.identity);
                            handObjects[i].transform.Rotate(0,baseCardRot,0);
                            handObjects[i].layer = LayerMask.NameToLayer("Hand");
                            ServerManager.Spawn(handObjects[i]);
                            break;
                        }
                    }
                    if (!handCards.Contains(null))
                    {
                        phase = "opponent";
                    }
                }
            }

            if (phase == "opponent")
            {
                phase = "action";
            }
        }
    }
    IEnumerator placing(Card currentCard, GameObject cardPrefab, int handIndex)
    {
        cardPrefab = Instantiate(Turtles[currentCard.getPassiveID()], new Vector3(2.706f, 0, 1.689f), Quaternion.identity);
        ServerManager.Spawn(cardPrefab);

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
            if (currentCard.getPassiveID() == 4) //Attacker Turtle
            {
                bool canPlace = true;
                if (board[selectedSquare - 1] != null)
                {
                    if (selectedSquare == 2)
                    {
                        if (board[0] != null && board[0].getPassiveID() == 2)
                        {
                            canPlace = false;
                        }
                        if (board[2] != null && board[2].getPassiveID() == 2)
                        {
                            canPlace = false;
                        }
                    }
                    if (selectedSquare == 4)
                    {
                        if (board[0] != null && board[0].getPassiveID() == 2)
                        {
                            canPlace = false;
                        }
                        if (board[6] != null && board[6].getPassiveID() == 2)
                        {
                            canPlace = false;
                        }
                    }
                    if (selectedSquare == 6)
                    {
                        if (board[2] != null && board[2].getPassiveID() == 2)
                        {
                            canPlace = false;
                        }
                        if (board[8] != null && board[8].getPassiveID() == 2)
                        {
                            canPlace = false;
                        }
                    }
                    else if (selectedSquare == 8)
                    {
                        if (board[6] != null && board[6].getPassiveID() == 2)
                        {
                            canPlace = false;
                        }
                        if (board[8] != null && board[8].getPassiveID() == 2)
                        {
                            canPlace = false;
                        }
                    }
                }
                else
                {
                    canPlace = true;
                }
                int[] atkPlace = new int[] { 2, 4, 6, 8 };
                if (canPlace && atkPlace.Contains(selectedSquare))
                {
                    yesHighlights();
                    if (Input.GetMouseButtonDown(0))
                    {
                        isPlacing = false;
                        board[selectedSquare - 1] = currentCard;
                        Destroy(boardObj[selectedSquare - 1]);
                        ServerManager.Despawn(boardObj[selectedSquare - 1]);
                        boardObj[selectedSquare - 1] = cardPrefab;
                        handCards[handIndex] = null;
                        Destroy(handObjects[handIndex]);
                        ServerManager.Despawn(handObjects[handIndex]);
                        handObjects[handIndex] = null;
                        StartCoroutine(effect(currentCard, cardPrefab));
                        phase = "draw";
                    }
                }
                else
                {
                    noHighlights();
                }
            }
            else if (currentCard.getplacement().Contains(selectedSquare) && board[selectedSquare - 1] == null) //Every Other Turtle
            {
                yesHighlights();
                if (Input.GetMouseButtonDown(0))
                {
                    isPlacing = false;
                    board[selectedSquare - 1] = currentCard;
                    boardObj[selectedSquare - 1] = cardPrefab;
                    handCards[handIndex] = null;
                    Destroy(handObjects[handIndex]);
                    ServerManager.Despawn(handObjects[handIndex]);
                    StartCoroutine(effect(currentCard, cardPrefab));
                    phase = "draw";
                }
            }
            else
            {
                noHighlights();
            }
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

            if (Input.GetMouseButtonDown(1))
            {
                isPlacing = false;
                disableHighlights();
                Destroy(cardPrefab);
                ServerManager.Despawn(cardPrefab);
                yield break;
            }
        }
        yield return new WaitForSeconds(0.001f);
        cardPrefab.GetComponent<Rigidbody>().useGravity = true;
        cardPrefab.GetComponent<Rigidbody>().isKinematic = false;
        cardPrefab.layer = LayerMask.NameToLayer("Board");

        disableHighlights();
    }


    IEnumerator effect(Card currentCard, GameObject cardPrefab)
    {
        effectActive = true;
        while (effectActive)
        {
            if (currentCard.getPlayEffectID() == -1)
            {
                phase = "draw";
                effectActive = false;
            }
            if (currentCard.getPlayEffectID() == 0)
            {
                phase = "draw";
                effectActive = false;
            }
            else if (currentCard.getPlayEffectID() == 1 && !currentCard.getOwn())
            {
                phase = "draw";
                effectActive = false;
            }
            else if (currentCard.getPlayEffectID() > 0 && currentCard.getOwn())
            {
                bool chosing = true;
                float initialX = Input.mousePosition.x;
                float initialY = Input.mousePosition.y;
                int selectedSquare = 0;
                while (chosing)
                {
                    if (Input.mousePosition.x + 50 < initialX)
                    {
                        if (Input.mousePosition.y - 50 > initialY)
                        {
                            //Pos 1
                            disableHighlights();
                            highlights[0].SetActive(true);
                            selectedSquare = 1;
                        }
                        else if (Input.mousePosition.y + 50 < initialY)
                        {
                            //Pos 7
                            disableHighlights();
                            highlights[6].SetActive(true);
                            selectedSquare = 7;
                        }
                        else
                        {
                            //Pos 4
                            disableHighlights();
                            highlights[3].SetActive(true);
                            selectedSquare = 4;
                        }
                    }
                    else if (Input.mousePosition.x - 50 > initialX)
                    {
                        if (Input.mousePosition.y - 50 > initialY)
                        {
                            //Pos 3
                            disableHighlights();
                            highlights[2].SetActive(true);
                            selectedSquare = 3;
                        }
                        else if (Input.mousePosition.y + 50 < initialY)
                        {
                            //Pos 9
                            disableHighlights();
                            highlights[8].SetActive(true);
                            selectedSquare = 9;
                        }
                        else
                        {
                            //Pos 6
                            disableHighlights();
                            highlights[5].SetActive(true);
                            selectedSquare = 6;
                        }
                    }
                    else
                    {
                        if (Input.mousePosition.y - 50 > initialY)
                        {
                            //Pos 2
                            disableHighlights();
                            highlights[1].SetActive(true);
                            selectedSquare = 2;
                        }
                        else if (Input.mousePosition.y + 50 < initialY)
                        {
                            //Pos 8
                            disableHighlights();
                            highlights[7].SetActive(true);
                            selectedSquare = 8;
                        }
                        else
                        {
                            //Pos 5
                            disableHighlights();
                            highlights[4].SetActive(true);
                            selectedSquare = 5;
                        }
                    }
                    yield return new WaitForSeconds(0.000001f);

                    //Wizard
                    if (currentCard.getPlayEffectID() == 1)
                    {
                        if (board[selectedSquare - 1] != null && board[selectedSquare - 1].getOwn() == false)
                        {
                            yesHighlights();
                            if (Input.GetMouseButtonDown(0))
                            {
                                chosing = false;
                                disableHighlights();
                                board[selectedSquare - 1].own = true;
                                currentCard.target = selectedSquare - 1;
                                StartCoroutine(effect(board[selectedSquare - 1], boardObj[selectedSquare - 1]));
                                boardObj[selectedSquare - 1].transform.Rotate(0, 180f, 0);
                                boardObj[selectedSquare - 1].transform.position += new Vector3(2.0824069f, 0.02110998f, 3.381845f);
                                while (effectActive)
                                {
                                    yield return new WaitForSeconds(0.00000001f);
                                }
                                StartCoroutine(giveBack(board[selectedSquare - 1]));
                            }
                        }
                        else
                        {
                            noHighlights();
                        }

                    }
                    //War Turtle
                    if (currentCard.getPlayEffectID() == 2)
                    {
                        if (board[selectedSquare - 1] != null)
                        {
                            yesHighlights();
                            if (Input.GetMouseButtonDown(0))
                            {
                                chosing = false;
                                disableHighlights();
                                StartCoroutine(giveBack(board[selectedSquare - 1]));
                                Destroy(boardObj[selectedSquare - 1]);
                                ServerManager.Despawn(boardObj[selectedSquare - 1]);
                                boardObj[selectedSquare - 1] = cardPrefab;
                                boardObj[selectedSquare - 1] = null;
                                board[selectedSquare - 1] = null;
                                effectActive = false;
                            }
                        }
                        else
                        {
                            noHighlights();
                        }

                    }
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

                    if (Input.GetMouseButtonDown(1))
                    {
                        chosing = false;
                        effectActive = false;
                        disableHighlights();
                    }
                }
            }
            else
            {
                effectActive = false;
            }
        }
        yield return new WaitForSeconds(0.001f);
    }

    //wizard give turtle back
    IEnumerator giveBack(Card card)
    {
        if (card.target != -1)
        {
            if (board[card.target].getOwn() == true && card.getOwn() == true)
            {
                board[card.target].own = false;
                boardObj[card.target].transform.Rotate(0, 180f, 0);
                boardObj[card.target].transform.position -= new Vector3(2.0824069f, 0.02110998f, 3.381845f);
                card.target = -1;
            }
            else if (board[card.target].getOwn() == false && card.getOwn() == false)
            {
                board[card.target].own = true;
                boardObj[card.target].transform.Rotate(0, 180f, 0);
                boardObj[card.target].transform.position += new Vector3(2.0824069f, 0.02110998f, 3.381845f);
                card.target = -1;
            }
        }
        yield return new WaitForSeconds(0.001f);
    }

    IEnumerator choosing()
    {
        yield return new WaitForSeconds(0.00001f);
        while (isChoosing)
        {
            yield return new WaitForSeconds(0.00001f);
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
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
                                Destroy(handObjects[i]);
                                ServerManager.Despawn(handObjects[i]);
                                handCards[i] = null;
                                handObjects[i] = null;
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
            int cards = 0;
            foreach (Card c in handCards)
            {
                if (c != null)
                {
                    cards++;
                }
            }
            if (cards == 2)
            {
                List<Card> newHandCards = new List<Card>();
                List<GameObject> newHandObjects = new List<GameObject>();
                for (int j = 0; j < handCards.Length; j++)
                {
                    if (handCards[j] != null)
                    {
                        newHandCards.Add(handCards[j]);
                        newHandObjects.Add(handObjects[j]);
                    }
                }
                for (int k = 0; k < newHandCards.Count && k < 2; k++)
                {
                    handCards[k] = newHandCards[k];
                    handObjects[k] = newHandObjects[k];
                    handObjects[k].transform.position = new Vector3(cardx[k], 0.9855669f, -4.71751f);
                }
                Array.Resize(ref handObjects, 2);
                Array.Resize(ref handCards, 2);
                isChoosing = false;
            }

        }
    }
}