using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.

namespace Completed
{
	//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
	public class Player : MovingObject
	{
		public float restartLevelDelay = 1f;        //Delay time in seconds to restart level.
        public int collectedPrizes;
        public int pointsPerFood = 10;				//Number of points to add to player food points when picking up a food object.
		public int pointsPerSoda = 20;				//Number of points to add to player food points when picking up a soda object.
		public int wallDamage = 1;                  //How much damage a player does to a wall when chopping it.
        public int enemyDamage = 1;                 //How much damge a player does to a enemy.
		public int m_HealthMax = 0;
		public int m_AleMax = 0;
		public Text m_AleText;                      //UI Text to display current player food total.
		public Text m_HealthText;                   //UI Text to display current player food total.
		public AudioClip moveSound1;				//1 of 2 Audio clips to play when player moves.
		public AudioClip moveSound2;				//2 of 2 Audio clips to play when player moves.
		public AudioClip eatSound1;					//1 of 2 Audio clips to play when player collects a food object.
		public AudioClip eatSound2;					//2 of 2 Audio clips to play when player collects a food object.
		public AudioClip drinkSound1;				//1 of 2 Audio clips to play when player collects a soda object.
		public AudioClip drinkSound2;				//2 of 2 Audio clips to play when player collects a soda object.
		public AudioClip gameOverSound;				//Audio clip to play when player dies.
		
		private Animator animator;					//Used to store a reference to the Player's animator component.
		private int m_AleCurrent = 0;      
		private int m_HealthCurrent = 0;
		private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
		
		
		//Start overrides the Start function of MovingObject
		protected override void Start ()
		{
			//Call the Start function of the MovingObject base class.
			base.Start ();
            collectedPrizes = 0;
            //Get a component reference to the Player's animator component
            animator = GetComponent<Animator>();

			//Get the current food point total stored in GameManager.instance between levels.
			m_AleCurrent = m_AleMax;
			ChangeAle( 0 );
			m_HealthCurrent = m_HealthMax;
			SyncHealthText();
		}
		
		
		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{
			//When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
			//GameManager.instance.playerFoodPoints = food;
		}		

        public void DestroyAfterAnimFinish()
        {
            Destroy(this.gameObject);
        }

        private void Update ()
		{
			//If it's not the player's turn, exit the function.
			if(!GameManager.instance.playersTurn) return;
			
			int horizontal = 0;  	//Used to store the horizontal move direction.
			int vertical = 0;		//Used to store the vertical move direction.
			
			//Check if we are running either in the Unity editor or in a standalone build.
			#if UNITY_STANDALONE || UNITY_WEBPLAYER
			
			//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
			
			//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
			vertical = (int) (Input.GetAxisRaw ("Vertical"));
			
			//Check if moving horizontally, if so set vertical to zero.
			if(horizontal != 0)
			{
				vertical = 0;
			}
			//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
			#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			
			//Check if Input has registered more than zero touches
			if (Input.touchCount > 0)
			{
				//Store the first touch detected.
				Touch myTouch = Input.touches[0];
				
				//Check if the phase of that touch equals Began
				if (myTouch.phase == TouchPhase.Began)
				{
					//If so, set touchOrigin to the position of that touch
					touchOrigin = myTouch.position;
				}
				
				//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
				else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
				{
					//Set touchEnd to equal the position of this touch
					Vector2 touchEnd = myTouch.position;
					
					//Calculate the difference between the beginning and end of the touch on the x axis.
					float x = touchEnd.x - touchOrigin.x;
					
					//Calculate the difference between the beginning and end of the touch on the y axis.
					float y = touchEnd.y - touchOrigin.y;
					
					//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
					touchOrigin.x = -1;
					
					//Check if the difference along the x axis is greater than the difference along the y axis.
					if (Mathf.Abs(x) > Mathf.Abs(y))
						//If x is greater than zero, set horizontal to 1, otherwise set it to -1
						horizontal = x > 0 ? 1 : -1;
					else
						//If y is greater than zero, set horizontal to 1, otherwise set it to -1
						vertical = y > 0 ? 1 : -1;
				}
			}
			
			#endif //End of mobile platform dependendent compilation section started above with #elif
			//Check if we have a non-zero value for horizontal or vertical
			if(horizontal != 0 || vertical != 0)
			{
				//Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
				//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
				AttemptMove<Wall> (horizontal, vertical);
            }
		}

        //AttemptMove overrides the AttemptMove function in the base class MovingObject
        //AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
        protected override void PlayMoveAnimation()
        {
            if (m_Direction == DIRECTION.Right)
            {
                animator.SetTrigger("isMovingRight");
            }
            if (m_Direction == DIRECTION.Left)
            {
                animator.SetTrigger("isMovingleft");
            }
        }
        protected override Transform AttemptMove <T> (int xDir, int yDir)
		{
			//Every time player moves, subtract from food points total.
			ChangeAle(-1);
            
			//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
			Transform hit_transform = base.AttemptMove <T> (xDir, yDir);

			if (hit_transform != null && m_AleCurrent >0)
			{
				
				//play hit animation and sound
				Wall hitWall = hit_transform.GetComponent<Wall>();
                if (hitWall != null && hitWall.dmgSprite != null)
                {
                    hitWall.DamageWall(wallDamage);
                    if (m_Direction == m_prev_direction && m_Direction == DIRECTION.Left)
                    {
                        animator.SetTrigger("chop_left");
                    }
                    if (m_Direction == m_prev_direction && m_Direction == DIRECTION.Right)
                    {
                        animator.SetTrigger("playerChop");
                    }
                    if (m_Direction == DIRECTION.Left && m_prev_direction == DIRECTION.Right)
                    {
                        animator.SetTrigger("chop_right_left");
                    }
                    if (m_Direction == DIRECTION.Right && m_prev_direction == DIRECTION.Left)
                    {
                        animator.SetTrigger("chop_left_right");
                    }

                }
				Enemy hitEnemy = hit_transform.GetComponent<Enemy>();
                if (hitEnemy != null)
                {
                    hitEnemy.DamageEnemy(enemyDamage);
                    if (m_Direction == m_prev_direction)
                    {
                        animator.SetTrigger("playerChop");
                    }
                    if (m_Direction == DIRECTION.Left && m_prev_direction == DIRECTION.Right)
                    {
                        animator.SetTrigger("chop_right_left");
                    }
                    if (m_Direction == DIRECTION.Right && m_prev_direction == DIRECTION.Left)
                    {
                        animator.SetTrigger("chop_left_right");
                    }
                }
            }

			//Hit allows us to reference the result of the Linecast done in Move.
				RaycastHit2D hit;
			
			//If Move returns true, meaning Player was able to move into an empty space.
			if (Move (xDir, yDir, out hit)) 
			{
				//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
				SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
			}
			
			//Since the player has moved and lost food points, check if the game has ended.
			CheckIfGameOver ();
			
			//Set the playersTurn boolean of GameManager to false now that players turn is over.
			GameManager.instance.playersTurn = false;
			return null;
		}
		
		
		//OnCantMove overrides the abstract function OnCantMove in MovingObject.
		//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
		protected override void OnCantMove <T> (T component)
		{
		}


        //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
        private void OnTriggerEnter2D(Collider2D other)
        {
            //Check if the tag of the trigger collided with is Exit.
            if (other.tag == "Exit")
            {
                //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
                Invoke("Restart", restartLevelDelay);

                //Disable the player object since level is over.
                enabled = false;
            }

            //Check if the tag of the trigger collided with is Food.
            else if (other.tag == "Food")
            {
				ChangeHealth(pointsPerFood);
				

                //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                //Disable the food object the player collided with.
                other.gameObject.SetActive(false);
            }
            else if(other.tag == "Shit")
            {
                ChangeAle(-pointsPerFood);
                //TO Do add eat shit SFX
                other.gameObject.SetActive(false);
            }

            //Check if the tag of the trigger collided with is Soda.
            else if (other.tag == "Soda")
            {
				ChangeAle(pointsPerSoda);

                //Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

                //Disable the soda object the player collided with.
                other.gameObject.SetActive(false);
            }
           else if(other.tag == "Prize")
            {
                collectedPrizes++;
                other.gameObject.SetActive(false);
                if (collectedPrizes == 3)
                {
                    //ToDo enable ezxit portal/boat thing 
                }
            }
        }

      
        //Restart reloads the scene when called.
        private void Restart ()
		{
			//Load the last scene loaded, in this case Main, the only scene in the game.
			Application.LoadLevel (Application.loadedLevel);
		}
		
		
		//LoseFood is called when an enemy attacks the player.
		//It takes a parameter loss which specifies how many points to lose.
		public void TakeDamage(int dmg)
		{
			//Set the trigger for the player animator to transition to the playerHit animation.
			animator.SetTrigger ("playerHit");
			m_HealthCurrent -= dmg;
			SyncHealthText();
		}

		private void SyncHealthText()
		{
			m_HealthText.text = "Health:" + m_HealthCurrent;
		}

		//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
		private void CheckIfGameOver ()
		{
			//Check if food point total is less than or equal to zero.
			if (m_HealthCurrent <= 0) 
			{
				//Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
				SoundManager.instance.PlaySingle (gameOverSound);
				
				//Stop the background music.
				SoundManager.instance.musicSource.Stop();
				
				//Call the GameOver function of GameManager.
				GameManager.instance.GameOver();
			}
		}

		private void ChangeAle( int change )
		{
			m_AleCurrent += change;
			m_AleText.text = "Ale:" + m_AleCurrent; 
		}
        private void ChangeHealth(int change)
        {
            m_HealthCurrent += change;
            m_HealthText.text = "Health:" + m_HealthCurrent;
            if (m_HealthCurrent > m_HealthMax)
                m_HealthCurrent = m_HealthMax;
        }
	}
}

