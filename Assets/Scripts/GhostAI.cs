using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*****************************************************************************
 * IMPORTANT NOTES - PLEASE READ
 * 
 * This is where all the code needed for the Ghost AI goes. There should not
 * be any other place in the code that needs your attention.
 * 
 * There are several sets of variables set up below for you to use. Some of
 * those settings will do much to determine how the ghost behaves. You don't
 * have to use this if you have some other approach in mind. Other variables
 * are simply things you will find useful, and I am declaring them for you
 * so you don't have to.
 * 
 * If you need to add additional logic for a specific ghost, you can use the
 * variable ghostID, which is set to 1, 2, 3, or 4 depending on the ghost.
 * 
 * Similarly, set ghostID=ORIGINAL when the ghosts are doing the "original" 
 * PacMan ghost behavior, and to CUSTOM for the new behavior that you supply. 
 * Use ghostID and ghostMode in the Update() method to control all this.
 * 
 * You could if you wanted to, create four separate ghost AI modules, one per
 * ghost, instead. If so, call them something like BlinkyAI, PinkyAI, etc.,
 * and bind them to the correct ghost prefabs.
 * 
 * Finally there are a couple of utility routines at the end.
 * 
 * Please note that this implementation of PacMan is not entirely bug-free.
 * For example, player does not get a new screenful of dots once all the
 * current dots are consumed. There are also some issues with the sound 
 * effects. By all means, fix these bugs if you like.
 * 
 *****************************************************************************/

public class GhostAI : MonoBehaviour {

    const int BLINKY = 1;   // These are used to set ghostID, to facilitate testing.
    const int PINKY = 2;
    const int INKY = 3;
    const int CLYDE = 4;
    public int ghostID;     // This variable is set to the particular ghost in the prefabs,

    const int ORIGINAL = 1; // These are used to set ghostMode, needed for the assignment.
    const int CUSTOM = 2;
    public int ghostMode;   // ORIGINAL for "original" ghost AI; CUSTOM for your unique new AI

    Movement move;
    private Vector3 startPos;
    private bool[] dirs = new bool[4];
	private bool[] prevDirs = new bool[4];

	public float releaseTime = 0f;          // This could be a tunable number
	private float releaseTimeReset = 0f;
	public float waitTime = 3f;             // This could be a tunable number
    private const float ogWaitTime = .1f;
	public int range = 0;                   // This could be a tunable number

    public bool dead = false;               // state variables
	public bool fleeing = false;

	//Default: base value of likelihood of choice for each path
	public float Dflt = 1f;

	//Available: Zero or one based on whether a path is available
	int A = 0;

	//Value: negative 1 or 1 based on direction of pacman
	int V = 1;

	//Fleeing: negative if fleeing
	int F = 1;

	//Priority: calculated preference based on distance of target in one direction weighted by the distance in others (dist/total)
	float P = 0f;

    // Variables to hold distance calcs
	float distX = 0f;
	float distY = 0f;
	float total = 0f;

    // Percent chance of each coice. order is: up < right < 0 < down < left for random choice
    // These could be tunable numbers. You may or may not find this useful.
    public float[] directions = new float[4];
    
	//remember previous choice and make inverse illegal!
	private int[] prevChoices = new int[4]{1,1,1,1};

    // This will be PacMan when chasing, or Gate, when leaving the Pit
	public GameObject target;
	GameObject gate;
	GameObject pacMan;

	public bool chooseDirection = true;
	public int[] choices ;
	public float choice;
    /* */
    public bool canChase = false;
    public bool isLeaving = false;
    public char[] currentDirections;
    public char[] previousDirections;
    char backwardsDirection;
    int moveIndex;
    Vector2 currentTile;
    Vector2 targetTile;
    List<Vector2> pacManPath;
    int count;
    bool firstMove;
   
	public enum State{
		waiting,
		entering,
		leaving,
		active,
		fleeing,
        scatter         // Optional - This is for more advanced ghost AI behavior
	}

	public State _state = State.waiting;

    // Use this for initialization
    private void Awake()
    {
        startPos = this.gameObject.transform.position;
    }

    void Start () {
		move = GetComponent<Movement> ();
		gate = GameObject.Find("Gate(Clone)");
		pacMan = GameObject.Find("PacMan(Clone)") ? GameObject.Find("PacMan(Clone)") : GameObject.Find("PacMan 1(Clone)");
		releaseTimeReset = releaseTime;
        currentDirections = new char[4] { 'u', 'r', 'd', 'l' };
        previousDirections = new char[4] { 'd', 'l', 'u', 'r' };
        backwardsDirection = '_';
        pacManPath = new List<Vector2>();
        currentTile = transform.position;
        count = 0;
	}

	public void restart(){
		releaseTime = releaseTimeReset;
		transform.position = startPos;
		_state = State.waiting;
	}
	
    /// <summary>
    /// This is where most of the work will be done. A switch/case statement is probably 
    /// the first thing to test for. There can be additional tests for specific ghosts,
    /// controlled by the GhostID variable. But much of the variations in ghost behavior
    /// could be controlled by changing values of some of the above variables, like
    /// 
    /// </summary>
	void Update () {
        if(ghostID == CLYDE || ghostID == INKY) {
            return;
        }
      //  pacManPath.Add(pacMan.transform.position);
        switch (_state) {
		case(State.waiting):

            // below is some sample code showing how you deal with animations, etc.
			move._dir = Movement.Direction.still;
			if (releaseTime <= 0f) {
				chooseDirection = true;
				gameObject.GetComponent<Animator>().SetBool("Dead", false);
				gameObject.GetComponent<Animator>().SetBool("Running", false);
				gameObject.GetComponent<Animator>().SetInteger ("Direction", 0);
				gameObject.GetComponent<Movement> ().MSpeed = 5f;

				_state = State.leaving;

                // etc.
			}
			gameObject.GetComponent<Animator>().SetBool("Dead", false);
			gameObject.GetComponent<Animator>().SetBool("Running", false);
			gameObject.GetComponent<Animator>().SetInteger ("Direction", 0);
			gameObject.GetComponent<Movement> ().MSpeed = 5f;
			releaseTime -= Time.deltaTime;
            // etc.
			break;


		case(State.leaving):
                if (ghostID == BLINKY) {
                    this.target = pacMan; // make the target pacMan 
                    _state = State.active;
                }
                                                            //if(ghostID == PINKY) 

                if (ghostID == PINKY) {
                    Debug.Log("heres pinkys pos " + transform.position);
                    Debug.Log("heres the gate pos " + gate.transform.position);
                    transform.position = new Vector2(transform.position.x + num2vec(0).x * move.MSpeed * Time.deltaTime, transform.position.y + num2vec(0).y * move.MSpeed * Time.deltaTime);
                    if(transform.position.y > gate.transform.position.y) {
                        _state = State.active;
                    }
                }
                
			break;

		case(State.active):
            if (dead) {
                    canChase = false;
                   
                    // etc.
                // most of your AI code will be placed here!
            }
                if (this.fleeing) {
                    _state = State.fleeing;
                } else {

                    // etc.
                    if (ghostID == BLINKY) {
                        
                             Chase();
                              canChase = true; // start the chase 
                        }
                       
                    }
                    if (ghostID == PINKY) {
                        // Chase();
                        // canChase = true;
                    }
                
             
			break;

		case State.entering:

                
                /*
                // Leaving this code in here for you.
			move._dir = Movement.Direction.still;

			if (transform.position.x < 13.48f || transform.position.x > 13.52) {
				//print ("GOING LEFT OR RIGHT");
				transform.position = Vector3.Lerp (transform.position, new Vector3 (13.5f, transform.position.y, transform.position.z), 3f * Time.deltaTime);
			} else if (transform.position.y > -13.99f || transform.position.y < -14.01f) {
				gameObject.GetComponent<Animator>().SetInteger ("Direction", 2);
				transform.position = Vector3.Lerp (transform.position, new Vector3 (transform.position.x, -14f, transform.position.z), 3f * Time.deltaTime);
			} else {
				fleeing = false;
				dead = false;
				gameObject.GetComponent<Animator>().SetBool("Running", true);
				_state = State.waiting;
			}
            */

            break;
            case State.fleeing:

                this.target = gate;
               // Chase();

                break;
		}

     
    

    }
    /*
    private void FixedUpdate() {
        
 
    }
    */

    // Utility routines

    Vector2 num2vec(int n){
        switch (n)
        {
            case 0:
                return new Vector2(0, 1); // up
            case 1:
    			return new Vector2(1, 0); // right
		    case 2:
			    return new Vector2(0, -1); // down
            case 3:
			    return new Vector2(-1, 0); // left
            default:    // should never happen
                return new Vector2(0, 0);
        }
	}

	bool compareDirections(bool[] n, bool[] p){
		for(int i = 0; i < n.Length; i++){
			if (n [i] != p [i]) {
				return false;
			}
		}
		return true;
	}
    
    // function to make ghost chase pacMan 
    void Chase() {
        
        if (ghostID == PINKY && _state != State.entering) { // for pinky, always have to get tile two head of pacMan
            this.target = twoAhead();
        }
        Vector2 nextTile = Vector2.zero;
        if(backwardsDirection != '_') {
            int ind = System.Array.IndexOf(previousDirections, backwardsDirection);
          //  Debug.Log("can't go back this way " + prevDir + " and we are considering this " + currentDirections[ind]);
            nextTile = new Vector2(transform.position.x + num2vec(ind).x, transform.position.y + num2vec(ind).y);
        }
        
         // list of possible directions ghost can move 
        Vector2[] possibleDirections = new Vector2[4];
        // holds the minimum distance any step on the map will make between the ghost and pacMan
        float leastDistance = 10000f;
        // holds the direction that stepping in would result in being closer to pacMan
        Vector2 leastDist = Vector2.zero;
        // index of the opposite direction in our list of directions
        int leastIndex = -1;
        for (int i = 0; i < 4; i++) {
            // if you can move that way and you wouldnt be going backwards, add to list of possible directions 
            if (move.checkDirectionClear(num2vec(i)) && backwardsDirection != currentDirections[i]) {
                possibleDirections[i] = new Vector2(num2vec(i).x, num2vec(i).y);
            }
        }
        float[] allDistances = new float[4];
        int count = 0; // to count the total amount of possible moves 
        // loop through the list of possible moves and get the move with the smallest distance to pacMan
        for (int i = 0; i < possibleDirections.Length; i++) {
            if (possibleDirections[i] != Vector2.zero) {
                // get the position of the tile 
                Vector2 dir = new Vector2(transform.position.x + possibleDirections[i].x, transform.position.y + possibleDirections[i].y);
                count += 1;
                // get the distance from pacMan
                float distance = getDistance(dir, target.transform.position);
                allDistances[i] = distance;
                // reset the smallest distance if this is the closest tile to PacMan
                if (distance < leastDistance) {
                    // if going this way would be the same distance as going up ...
                    leastDistance = distance;
                    leastDist = new Vector2(num2vec(i).x, num2vec(i).y);
                    leastIndex = i;

                }
            }
        }
        
       // Debug.Log("heres the time" + time);
        // if we can move that way (sanity check) move that way 
        if (move.checkDirectionClear(leastDist)) {
            // this is temporary .. thought waiting a bit before reassigning previous direction would make the ghost stop moving backwards, but
            //not really working 
            //  Debug.Log("can't go this way " + prevDir + " and going this way " + currentDirections[leastIndex]);
            
            backwardsDirection = previousDirections[leastIndex];
            Vector2 newDir = new Vector2((int)transform.position.x + (int)leastDist.x, (int)transform.position.y + leastDist.y);
           // if (ghostID == BLINKY) Debug.Log(" new direction " + newDir);
            moveIndex = leastIndex;
            move.move(leastDist);
            /*
            while (transform.position.x != newDir.x && transform.position.y != newDir.y) {
                move.move(leastDist);
            }
            */
            

        }
              
       
    }
    

    /* function to return a tile two ahead of pacMan's current direction of movement */
    public GameObject twoAhead() {
        GameObject newTarget = new GameObject();
        if(pacMan.GetComponent<Movement>()._dir == Movement.Direction.up) { // going up 
            newTarget.transform.position = new Vector2(pacMan.transform.position.x, pacMan.transform.position.y + 2);
        }else if(pacMan.GetComponent<Movement>()._dir == Movement.Direction.right) { // going right
            newTarget.transform.position = new Vector2(pacMan.transform.position.x + 2, pacMan.transform.position.y);
        } else if(pacMan.GetComponent<Movement>()._dir == Movement.Direction.down) { // going down
            newTarget.transform.position = new Vector2(pacMan.transform.position.x, pacMan.transform.position.y - 2);
        } else if(pacMan.GetComponent<Movement>()._dir == Movement.Direction.left) { // going left
            newTarget.transform.position = new Vector2(pacMan.transform.position.x - 2, pacMan.transform.position.y);
        }
        return newTarget;
    }
    // not used currently ... tried to do this to get ghost to stop moving backwards
    IEnumerator WaitABit() {
        yield return new WaitForSeconds(1.5f);
    }
    // gets the distance between a ghost position (can be tile ahead or ghosts current position)and the target 
    float getDistance(Vector2 ghostPos, Vector2 targetPos) {

        float dx = ghostPos.x - targetPos.x;
        float dy = ghostPos.y - targetPos.y;
        float distance = Mathf.Sqrt(dx * dx + dy * dy);
        return distance;
    }
    void ReachTile() {

    }

}
