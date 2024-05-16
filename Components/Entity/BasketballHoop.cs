using Configgy;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns.Components.Entity
{
    public class BasketballHoop : MonoBehaviour
    {
        [UFGAsset("ThrowableBasketball")] public static GameObject basketballPrefab { get; private set; }
        [UFGAsset("MegaDunk")] private static AudioClip megaDunkAudio;
        [UFGAsset("BasketballScoreFX")] private static GameObject basketballScoreFX;
        [UFGAsset("CardboardTubeHit1")] private static AudioClip ballSpawnSound;

        [Configgable("Ballin", "Basketball Hoop In Sandbox")]
        public static ConfigToggle EnableBasketballHoop = new ConfigToggle(true);

        [SerializeField] private Transform hoop;
        [SerializeField] private Rigidbody[] netRbs;
        [SerializeField] private Text scoreText, highScoreText;

        private bool ballInHoop;
        private List<Rigidbody> rbsInHoop = new List<Rigidbody>();

        private List<GameObject> spawnedBasketballs = new List<GameObject>();
        private int maxBalls = 69;

        private int currentScore;
        private int highScore => Data.SaveInfo.Data.basketballHighScore;
        public int CurrentScore
        {
            get
            {
                return currentScore;
            }

            private set
            {
                currentScore = value;
                UpdateScore();
            }
        }

        private void UpdateScore()
        {
            if(currentScore > highScore)
            {
                if(highScore <= 99 && currentScore >= 100)
                {
                    //Play YIPEE Sound.
                    HudMessageReceiver.Instance.SendHudMessage($"You have unlocked <color=orange>BasketBall Skin</color> for the weapon <color=orange>ULTRABALLER</color> use <color=orange>{UFGInput.SecretButton.Value}</color> while using it to toggle it.");
                }
                Data.SaveInfo.Data.basketballHighScore = currentScore;
            }

            highScoreText.text = (highScore > 99999999) ? "ERROR" : highScore.ToString("000");
            scoreText.text = (currentScore > 99999999) ? "ERROR" : currentScore.ToString("000");
        }

        private void Start()
        {
            EnableBasketballHoop.OnValueChanged += transform.root.gameObject.SetActive;
            UpdateScore();
            transform.root.gameObject.SetActive(EnableBasketballHoop.Value);
        }


        public Vector3 GetHoopPos()
        {
            return hoop.position;
        }

        public void OnTriggerEnter(Collider col)
        { 
            HydraLogger.Log($"TRIGGERED {col.gameObject.name} ENTER");
            if (col.attachedRigidbody == null)
                return;
                
            MoveNet(col.attachedRigidbody.velocity.magnitude * 18.0f);

            if (col.attachedRigidbody != null)
            {
                if (rbsInHoop.Contains(col.attachedRigidbody))
                    return;
            }

            Vector3 dirToCollider = col.gameObject.transform.position - GetHoopPos();

            float dot = Vector3.Dot(dirToCollider.normalized, Vector3.up); //Make sure ball is above hoop, not below.

            bool isBall = IsBall(col, dot>0);

            bool inHoop = rbsInHoop.Contains(col.attachedRigidbody);

            HydraLogger.Log($"isball: {isBall} inhoop: {ballInHoop} dot: {dot > 0}");

            if ( isBall && !inHoop && dot > 0)
            {
                rbsInHoop.Add(col.attachedRigidbody);
                HydraLogger.Log($"In da hoop {col.gameObject.name}");
                return;
            }

            HydraLogger.Log($"Failed check {col.gameObject.name}");
        }

        float lastThrowDistance = 0;

        private Vector3 lastBallVelo;

        public void OnTriggerExit(Collider col)
        {

            HydraLogger.Log($"TRIGGERED {col.gameObject.name} EXIT");

            if (col.attachedRigidbody == null)
                return;
            
            MoveNet(col.attachedRigidbody.velocity.magnitude * 18.0f);

            bool inHoop = rbsInHoop.Contains(col.attachedRigidbody);

            if (!inHoop)
                return;

            Vector3 dirToCollider = col.gameObject.transform.position - GetHoopPos();

            float dot = Vector3.Dot(dirToCollider.normalized, Vector3.down); //Make sure ball is below hoop, not above.

            bool isBall = IsBall(col, dot<0);

            HydraLogger.Log($"isball: {isBall} inhoop: {inHoop} dot: {dot > 0}");

            if (isBall  && dot > 0)
            {
                rbsInHoop.Remove(col.attachedRigidbody);
                HydraLogger.Log($"Out da {col.gameObject.name}");
                Score();
                return;
            }

            HydraLogger.Log($"Failed check {col.gameObject.name}");
        }

        private bool IsBall(Collider col, bool aboveHoop)
        {
            if (col.gameObject.TryFindComponent<RealisticBasketball>(out RealisticBasketball basketBall))
            {
                lastThrowDistance = basketBall.LastThrowDist;
                lastBallVelo = basketBall.Rigidbody.velocity;
                if(UnityEngine.Random.value > 0.5f && aboveHoop)
                    basketBall.Rigidbody.velocity = (GetHoopPos() - (basketBall.Rigidbody.centerOfMass+basketBall.transform.position));
                return true;
            }

            if (col.gameObject.TryFindComponent<ThrownDodgeball>(out ThrownDodgeball dodgeBall))
            {
                lastBallVelo = dodgeBall.rb.velocity;
                return true;
            }

            return false;
        }

        //Destroys parentless balls.
        public void AnnihilateBalls()
        {
            for (int i = 0; i < spawnedBasketballs.Count; i++)
            {
                if (spawnedBasketballs[i] == null)
                    continue;

                if (spawnedBasketballs[i].transform.parent != null)
                    continue;

                Destroy(spawnedBasketballs[i]);
            }

            spawnedBasketballs = spawnedBasketballs.Where(x => x != null).ToList();
        }

        public void SpawnBall()
        {
            Vector3 spawnPos = hoop.position+(hoop.transform.forward*10.0f);
            ballSpawnSound.PlayAudioClip();
            Instantiate(Prefabs.SmackFX, spawnPos, Quaternion.identity);
            GameObject newBall = Instantiate(basketballPrefab, spawnPos, Quaternion.identity);
            spawnedBasketballs.Add(newBall);
            spawnedBasketballs = spawnedBasketballs.Where(x => x != null).ToList();
            if(spawnedBasketballs.Count > maxBalls)
            {
                if (spawnedBasketballs[0].transform.parent != null) //If its being held, move it to the back of the deletion line.
                {
                    GameObject heldBall = spawnedBasketballs[0];
                    spawnedBasketballs.Remove(heldBall);
                    spawnedBasketballs.Add(heldBall);
                }
                Destroy(spawnedBasketballs[0]);
            }
        }
        
        public void SlamDunk()
        {
            lastBallVelo = Vector3.zero;
            lastThrowDistance =25f*3;
            megaDunkAudio.PlayAudioClip();
            Score();
        }

        private void Score()
        {
            HydraLogger.Log("SCORED");
            Instantiate(basketballScoreFX, GetHoopPos(), Quaternion.identity);
            if(lastBallVelo.magnitude > 100.0f)
            {
                Instantiate(Prefabs.UK_Explosion.Asset, GetHoopPos(), Quaternion.identity);
                megaDunkAudio.PlayAudioClip();
                MoveNet(500.0f);
                CurrentScore += 10;
                return;
            }

            CurrentScore += CalcScore(lastThrowDistance);
        }

        private int CalcScore(float distance)
        {
            int score = 1;

            float remainder = distance % 25f;
            float adjusted = distance - remainder;
            int unitsAway = Mathf.FloorToInt(adjusted / 25f);

            score += (unitsAway > 0) ? (2*unitsAway) : 1;
            return score;
        }
        
        private void MoveNet(float intensity)
        {
            foreach (Rigidbody rb in netRbs)
            {
                if(rb == null)
                    continue;
                Vector3 torque = UnityEngine.Random.onUnitSphere * intensity;
                torque.y = 0.0f;
                rb.AddTorque(torque, ForceMode.Impulse);
            }
        }

        private void OnDestroy()
        {
            EnableBasketballHoop.OnValueChanged -= transform.root.gameObject.SetActive;
        }
        
    }
}
