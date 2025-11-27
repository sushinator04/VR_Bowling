using System;
using System.Collections;
using System.Threading.Tasks;
using Dev.ComradeVanti.WaitForAnim;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Random = UnityEngine.Random;

namespace NPC
{
    public class BotHandler : MonoBehaviour
    {
        [SerializeField] private GameObject botPrefab;

        [SerializeField] private float walkSpeed;
        [SerializeField] private float rotationSpeedWhenWalking;
        [SerializeField] private float walkAccelerationTime;

        [SerializeField] private float throwBallWalkSpeed;
        [SerializeField] private float throwBallAcceleration;

        [SerializeField] private float distanceToBallReturnSystem;
        [SerializeField] private float maxHandDistBallPickup;
        [SerializeField] private float maxTimeUntilNewTarget;

        [SerializeField] private float walkTargetErrorThres;

        [SerializeField] private float ikSpeed;

        [SerializeField] private SeatingConfiguration seatingConfiguration;
    
        [SerializeField]private bool[] seatReserved;

        private AlleyHandler alleyHandler;
        private BallReturnChecker ballReturnChecker;
        private BotVariables[] botVariables;


        private GameObject[] bots;
        private int alleyIndex;

        private Animator[] animators;

        private Transform[] seatingTrans;

        [SerializeField]private bool[] initialBotInSeat;

        private BotVariables.BotLevel botLevel;


        private void Awake()
        {
            alleyHandler = GetComponent<AlleyHandler>();
            ballReturnChecker = alleyHandler.GetBallReturnChecker();

            seatingTrans = seatingConfiguration.seatingPos;
            alleyIndex = GetAlleyIndex();

            seatReserved = new[] {false, false, false};
        }


        private IEnumerator Initialize(int botCount, BotVariables.BotLevel level)
        {
            //StopAllCoroutines();

            if (bots != null)
            {
                foreach (var bot in bots)
                {
                    Destroy(bot);
                }
            }

            animators = new Animator[botCount];
            bots = new GameObject[botCount];
            botVariables = new BotVariables[botCount];

            initialBotInSeat = new bool[botCount];

            for (int i = 0; i < botCount; i++)
            {
                bots[i] = Instantiate(botPrefab, seatingConfiguration.walkBackStartPos.position, Quaternion.identity);
                botVariables[i] = bots[i].GetComponent<BotVariables>();
                botVariables[i].level = level;
                botVariables[i].pinRespawn = alleyHandler.GetPinRespawn();
                animators[i] = bots[i].GetComponent<Animator>();
                StartCoroutine(GetInSeatQueue(i));
                yield return new WaitForSeconds(5f);
            }
        
            yield return new WaitUntil(InitialAllBotsInSeat);
        }

        private bool InitialAllBotsInSeat()
        {
            foreach (bool completed in initialBotInSeat)
            {
                if (!completed) return false;
            }

            return true;
        }

        private int GetAlleyIndex()
        {
            int result = -1;

            String name = transform.name;
            int start = name.IndexOf("(") + 1;
            int end = name.IndexOf(")");

            int.TryParse(name.Substring(start, end - start), out result);

            if (result == -1)
            {
                Debug.LogError("Parsing Failed!");
            }

            return result;
        }


        public IEnumerator StartBots(int botCount, BotVariables.BotLevel level)
        {
            yield return StartCoroutine(Initialize(botCount, level));
       
            for (int i = 0; i < 10; i++) // 10 rounds
            {
                // foreach player
                for (int botIndex = 0; botIndex < bots.Length; botIndex++)
                {
                    yield return new WaitUntil(() => alleyHandler.currentPlayer == alleyHandler.realPlayers + botIndex);

                    // walk to start throw
                    yield return StartCoroutine(MoveNPC(botIndex, seatingConfiguration.startThrowPos.position, walkTargetErrorThres, walkSpeed, walkAccelerationTime, true, false));

                    while (alleyHandler.currentPlayer == alleyHandler.realPlayers + botIndex)
                    {
                        // Get Random Ball (Move to ball -> pickup (cycle until done))
                        yield return StartCoroutine(PickupBall(botIndex));

                        // Walk to start throw pos
                        yield return StartCoroutine(MoveNPC(botIndex, seatingConfiguration.startThrowPos.position, walkTargetErrorThres, walkSpeed, walkAccelerationTime));
                        bots[botIndex].transform.LookAt(transform);

                        yield return new WaitForSeconds(Random.Range(100, 200) / 100f);
                    
                        // Throw ball
                        // Logic random throw direction
                        StartCoroutine(MoveNPC(botIndex, seatingConfiguration.endThrowPos.position, walkTargetErrorThres, throwBallWalkSpeed, throwBallAcceleration, false, true, false));
                        yield return StartCoroutine(MakeTriggerAnimation(botIndex, "ThrowBall"));
                    
                        yield return new WaitForSeconds(8f);
                    }
                
                    // Get back in seat
                    StartCoroutine(GetInSeatQueue(botIndex));
                }
            }
        }

        private IEnumerator PickupBall(int npcIndex)
        {
            Task<Transform> ballReadyTask;

            // Ball pickup event variables
            Transform target = ballReturnChecker.GetBallTargetIfAvailable();
            if (target == null)
            {
                // Wait until ball becomes available
                ballReadyTask = alleyHandler.GetBallReturnChecker().WaitForBallReady();
                yield return new WaitUntil(() => ballReadyTask.IsCompleted);
                target = ballReadyTask.Result;
            }

            // Walk To Ball
            int alleyFac = (alleyIndex % 2 == 0) ? 1 : -1;
            Vector3 targetPos = new Vector3(target.position.x, 0, target.position.z + alleyFac * distanceToBallReturnSystem);
            yield return StartCoroutine(MoveNPC(npcIndex, targetPos, walkTargetErrorThres, walkSpeed, walkAccelerationTime));

            // IK variables
            //botVariables[npcIndex].targetHandIK.parent = null; // don't move target during animation
            botVariables[npcIndex].targetHandIK.position = target.position;
            ChainIKConstraint handIK = botVariables[npcIndex].handIKConstraint;
            Transform rightMiddleFingerBone = botVariables[npcIndex].rightMiddleFingerBone;

            float time = 0f;
            float velocity = 0f; // internal for smooth ik transformation
            handIK.weight = 0f;

            // Rotate towards target ball
            Vector3 lookAtPos = new Vector3(target.position.x, bots[npcIndex].transform.position.y, target.position.z);
            bots[npcIndex].transform.LookAt(lookAtPos);

            // Start pickup animation and check if was able to catch ball
            StartCoroutine(MakeTriggerAnimation(npcIndex, "BallPickup"));
            while (!hasReachedTarget(rightMiddleFingerBone.position, target.position, maxHandDistBallPickup))
            {
                time += Time.deltaTime;
                //Debug.Log(time);

                if (time > maxTimeUntilNewTarget)
                {
                    yield return new WaitForAnimationToFinish(animators[npcIndex], "BallPickup");

                    // Needed too long for pickup -> new target
                    target = ballReturnChecker.GetBallTargetIfAvailable();
                    if (target == null)
                    {
                        // Wait until ball becomes available
                        ballReadyTask = alleyHandler.GetBallReturnChecker().WaitForBallReady();
                        yield return new WaitUntil(() => ballReadyTask.IsCompleted);
                        target = ballReadyTask.Result;
                    }

                    // Walk To target
                    alleyFac = (alleyIndex % 2 == 0) ? 1 : -1;
                    targetPos = new Vector3(target.position.x, 0, target.position.z + alleyFac * distanceToBallReturnSystem);

                    yield return StartCoroutine(MoveNPC(npcIndex, targetPos, walkTargetErrorThres, walkSpeed, walkAccelerationTime, true, true));

                    // Rotate towards target
                    lookAtPos = new Vector3(target.position.x, bots[npcIndex].transform.position.y, target.position.z);
                    bots[npcIndex].transform.LookAt(lookAtPos);
                    botVariables[npcIndex].targetHandIK.position = target.position;
                    StartCoroutine(MakeTriggerAnimation(npcIndex, "BallPickup"));
                    time = 0;
                    handIK.weight = 0;
                }

                botVariables[npcIndex].targetHandIK.position = target.position;
                // Smoothly increase IK weight
                handIK.weight = Mathf.SmoothDamp(handIK.weight, 1f, ref velocity, ikSpeed);
                yield return new WaitForEndOfFrame();
            }

            // Was able to catch ball
            target.GetComponent<Rigidbody>().isKinematic = true;

            // Disable Collider of target to not interfere with other balls when picking up
            target.GetComponent<SphereCollider>().isTrigger = true;

            // Parent the ball to the NPC's hand
            target.parent = botVariables[npcIndex].rightIndexFinger;

            // Position the ball correctly in the hand
            target.transform.SetLocalPositionAndRotation(new Vector3(0.0481f, -0.0613f, 0.1215f), Quaternion.Euler(229.624f, -8.1000006f, 13.48801f));

            // Adjust IK weight smoothly to 0
            for (int i = 0; i < 100; i++)
            {
                handIK.weight = 1 - (i / 100f);
                yield return new WaitForEndOfFrame();
            }

            botVariables[npcIndex].targetHandIK.position = botVariables[npcIndex].rightIndexFinger.position;

            yield return new WaitForAnimationToFinish(animators[npcIndex], "BallPickup");
            animators[npcIndex].SetBool("isHoldingBall", true);
        }
    

        IEnumerator GetInSeatQueue(int npcIndex)
        {
            // walk back to start sofa
            yield return StartCoroutine(MoveNPC(npcIndex, seatingConfiguration.walkBackStartPos.position, walkTargetErrorThres, walkSpeed, walkAccelerationTime, true, false));
            yield return StartCoroutine(MoveNPC(npcIndex, seatingConfiguration.walkBackEndPos.position, walkTargetErrorThres, walkSpeed, walkAccelerationTime, true, true));

            // Siting Queue -> Sit down and wait until free space

            // Wait for SEAT 0
            while (seatReserved[0])
            {
                yield return new WaitUntil(() => seatReserved[0] == false);
            };
            if (alleyHandler.botCount > 2)
            {
                initialBotInSeat[2] = true;
            }
            seatReserved[0] = true;
       
        

            if (seatReserved[1])
            {
                yield return StartCoroutine(MoveNPC(npcIndex, seatingTrans[0].position, walkTargetErrorThres, walkSpeed, walkAccelerationTime, setWalkingToFalse: true));

            }
            else
            {
                yield return StartCoroutine(MoveNPC(npcIndex, seatingTrans[0].position, walkTargetErrorThres, walkSpeed, walkAccelerationTime, setWalkingToFalse: false));

            }

            // Wait for SEAT 1
            if (seatReserved[1])
            {
                // sit down until next space free
                bots[npcIndex].transform.position = seatingTrans[0].position;
                bots[npcIndex].transform.rotation = seatingTrans[0].rotation;
                yield return StartCoroutine(MakeTriggerAnimation(npcIndex, "StandToSit"));
                while (seatReserved[1])
                {
                    yield return new WaitUntil(() => seatReserved[1] == false);
                }
                seatReserved[1] = true;
                yield return StartCoroutine(MakeTriggerAnimation(npcIndex, "SitToStand"));
            }
            seatReserved[1] = true;
            if (alleyHandler.botCount > 2)
            {
                initialBotInSeat[2] = false;
            }
            if (alleyHandler.botCount > 1)
            {
                initialBotInSeat[1] = true;
            }
            seatReserved[0] = false;
       
       
        
            if (seatReserved[2])
            {
                yield return StartCoroutine(MoveNPC(npcIndex, seatingTrans[1].position, walkTargetErrorThres, walkSpeed, walkAccelerationTime, setWalkingToFalse: true));
            }
            else
            {
                yield return StartCoroutine(MoveNPC(npcIndex, seatingTrans[1].position, walkTargetErrorThres, walkSpeed, walkAccelerationTime, setWalkingToFalse: false));
            }

        
            // SEAT 2
            if (seatReserved[2])
            {
                // sit down until next space free
                bots[npcIndex].transform.position = seatingTrans[1].position;
                bots[npcIndex].transform.rotation = seatingTrans[1].rotation;
                yield return StartCoroutine(MakeTriggerAnimation(npcIndex, "StandToSit"));
                while (seatReserved[2])
                {
                    yield return new WaitUntil(() => seatReserved[2] == false);
                }
                seatReserved[2] = true;
                yield return StartCoroutine(MakeTriggerAnimation(npcIndex, "SitToStand"));
            }
            seatReserved[2] = true;
            if (alleyHandler.botCount > 1)
            {
                initialBotInSeat[1] = false;
            }
            seatReserved[1] = false;
       


            if (alleyHandler.currentPlayer != alleyHandler.realPlayers + npcIndex || !InitialAllBotsInSeat())
            {
                yield return StartCoroutine(MoveNPC(npcIndex, seatingTrans[2].position, walkTargetErrorThres, walkSpeed, walkAccelerationTime, setWalkingToFalse: true));

            }
            else
            {
                yield return StartCoroutine(MoveNPC(npcIndex, seatingTrans[2].position, walkTargetErrorThres, walkSpeed, walkAccelerationTime, setWalkingToFalse: false));
            }

        
            // Wait until player turn
            if (alleyHandler.currentPlayer != alleyHandler.realPlayers + npcIndex || !InitialAllBotsInSeat())
            {
                // sit down until next space free
                bots[npcIndex].transform.position = seatingTrans[2].position;
                bots[npcIndex].transform.rotation = seatingTrans[2].rotation;
                yield return StartCoroutine(MakeTriggerAnimation(npcIndex, "StandToSit"));
                while (alleyHandler.currentPlayer != alleyHandler.realPlayers + npcIndex)
                {
                    yield return new WaitUntil(() => alleyHandler.currentPlayer == alleyHandler.realPlayers + npcIndex);
                }
                yield return StartCoroutine(MakeTriggerAnimation(npcIndex, "SitToStand"));
            }
            if (alleyHandler.botCount > 0)
            {
                initialBotInSeat[0] = true;
            }
            seatReserved[2] = false;
        }


        IEnumerator MakeTriggerAnimation(int npcIndex, string stateName)
        {
            string triggerName = "is" + stateName;

            animators[npcIndex].SetTrigger(triggerName);

            yield return new WaitForAnimationToStart(animators[npcIndex], stateName);
            yield return new WaitForAnimationToFinish(animators[npcIndex], stateName);
        }


        IEnumerator MoveNPC(
            int npcIndex,
            Vector3 targetPosition,
            float maxErrDist,
            float speed,
            float accelerationTime,
            bool makeWalkAnimation = true,
            bool setWalkingToFalse = true, // to ensure clean speed transitions when changing walk target
            bool rotateNpc = true)
        {
            float currentSpeed = 0f;
            if (animators[npcIndex].GetBool("isWalking"))
            {
                currentSpeed = animators[npcIndex].GetFloat("currentWalkSpeed");
            }

            float smoothTime = accelerationTime;
            float velocity = 0f; // Used internally by SmoothDamp

            if (makeWalkAnimation)
            {
                animators[npcIndex].SetBool("isWalking", true);
            }

            while (!hasReachedTarget(bots[npcIndex].transform.position, targetPosition, maxErrDist))
            {
                // Gradually increase the current speed to look more realistic
                currentSpeed = Mathf.SmoothDamp(currentSpeed, speed, ref velocity, smoothTime);
                animators[npcIndex].SetFloat("currentWalkSpeed", currentSpeed);

                if (rotateNpc)
                {
                    // Smoothly rotate towards the target position
                    Vector3 directionToTarget = (targetPosition - bots[npcIndex].transform.position).normalized;
                    directionToTarget.y = 0; // dont rotate up or down
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    bots[npcIndex].transform.rotation = Quaternion.RotateTowards(bots[npcIndex].transform.rotation, targetRotation, rotationSpeedWhenWalking * Time.deltaTime);
                }

                bots[npcIndex].transform.position = Vector3.MoveTowards(bots[npcIndex].transform.position, targetPosition, currentSpeed * Time.deltaTime);
            
                yield return new WaitForEndOfFrame();
            }

            if (setWalkingToFalse)
            {
                animators[npcIndex].SetBool("isWalking", false);
                animators[npcIndex].SetFloat("currentWalkSpeed", 0f);
            }
        }


        private bool hasReachedTarget(Vector3 thisPos, Vector3 targetPos, float maxError)
        {
            return (thisPos - targetPos).magnitude <= maxError;
        }
    }
}