using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Completed
{
	public static class ListShufflerExtensionMethods
	{
		private static System.Random rng = new System.Random();

		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
	//Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
	public class Enemy : MovingObject
	{
		/// <summary>
		/// ////
		
		/// </summary>
		public int playerDamage; 							//The amount of food points to subtract from the player when attacking.
		public AudioClip attackSound1;						//First of two audio clips to play when attacking the player.
		public AudioClip attackSound2;                      //Second of two audio clips to play when attacking the player.
        public int m_HealthMax;
        public bool isDead;
		private Animator animator;							//Variable of type Animator to store a reference to the enemy's Animator component.
		private Transform target;                           //Transform to attempt to move toward each turn.
		private int m_HealthCurrent;
        private Image[] healthBars;
        private Image greenHealthBar;
        //Start overrides the virtual Start function of the base class.
    
        protected override void Start ()
		{
			//Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
			//This allows the GameManager to issue movement commands.
			GameManager.instance.AddEnemyToList (this);
			m_HealthCurrent = m_HealthMax;
            healthBars = GetComponentsInChildren<Image>();
            foreach(Image i in healthBars)
            {
                if (i.gameObject.name== "GreenBar")
                {
                    greenHealthBar = i;
                }
            }
			//Get and store a reference to the attached Animator component.
			animator = GetComponent<Animator> ();
           
			//Find the Player GameObject using it's tag and store a reference to its transform component.
			target = GameObject.FindGameObjectWithTag ("Player").transform;
            isDead = false;
            //Call the start function of our base class MovingObject.
            base.Start ();
		}
		
		
		////Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
		////See comments in MovingObject for more on how base AttemptMove function works.
		//protected override Transform AttemptMove <T> (int xDir, int yDir)
		//{
		//	//Call the AttemptMove function from MovingObject.
		//	return base.AttemptMove<T>(xDir, yDir);
			
		//}
		
		
		//MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
		public void MoveEnemy ()
		{
			//Declare variables for X and Y axis move directions, these range from -1 to 1.
			//These values allow us to choose between the cardinal directions: up, down, left and right.
			int xDir = 0;
			int yDir = 0;

			if (Mathf.Abs(target.position.x - transform.position.x) < Mathf.Abs(target.position.y - transform.position.y) )
				yDir = target.position.y > transform.position.y ? 1 : -1;
			else
				xDir = target.position.x > transform.position.x ? 1 : -1;

			Vector2 player_dir = new Vector2(xDir, yDir);
			List<Vector2> attack_positions = new List<Vector2>( );
			List<Vector2> temp = new List<Vector2>(4);

			//attack_positions.Add(player_dir);
			temp.Add(new Vector2( -1, 0));
			temp.Add(new Vector2(1, 0));
			temp.Add(new Vector2(0,-1));
			temp.Add(new Vector2(0, 1));
			temp.Shuffle();

			int index = 0;

			for (int x = 0; x < 4; x++)
				if (temp[x] == player_dir)
				{
					index = x;
					break;
				}
			for (int x = 0; x < 4; x++)
				attack_positions.Add( temp[(index++) % 4] );


			for (int x = 0; x < 4; x++)
			{
				Transform thit_transform = AttemptMove<Player>((int)attack_positions[x].x , (int)attack_positions[x].y);
				if (thit_transform == null)
					break;

				Wall hitComponent = thit_transform.GetComponent<Wall>();
				if (hitComponent == null)
					return;
			}
			
		}


        //OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
        //and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
        protected override void OnCantMove<T>(T component)
        {
            ////Declare hitPlayer and set it to equal the encountered component.
			Player hitPlayer = component as Player;
			if (component.gameObject.tag == "Player")
			{

				hitPlayer.TakeDamage(playerDamage);
                animator.SetTrigger("enemyAttack");
            }

            ////Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
            //hitPlayer.LoseFood(playerDamage);

            ////Set the attack trigger of animator to trigger Enemy attack animation.
            //animator.SetTrigger("enemyAttack");

            ////Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
            //SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
        }
        public void DamageEnemy(int dmg)
        {            
            m_HealthCurrent-=dmg;
            greenHealthBar.fillAmount = (float)m_HealthCurrent / m_HealthMax;
            // greenHealthBar.fillAmount = health; To do link enemy health and the fill amount of the green bar;
            if (m_HealthCurrent <= 0)
            {
                isDead = true;
              
                
            }
        }
	}
}
