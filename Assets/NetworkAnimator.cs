using Framework.Network;
using MEC;
using Protocol;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace FrameWork.Network
{
    public class NetworkAnimator : NetworkComponent
    {
        #region Members

        private readonly float interval = 0.05f;
        private readonly int totalStep = 3;
        private Animator animator;
        private readonly Queue<S_SET_ANIMATION> queue = new();
        private CoroutineHandle SetAnimationHandler;
        private Stopwatch stopwatch;

        #endregion

        #region Initialize

        protected void Awake()
        {
            stopwatch = new Stopwatch();

            animator = GetComponentInChildren<Animator>();
        }

        protected void Start()
        {
            if (isMine)
            {
                SetAnimationHandler = Timing.RunCoroutine(Co_Update());
            }
            else
            {
                GameClientManager.Instance.mainConnection.AddHandler(S_SET_ANIMATION);
            }
        }

        private IEnumerator<float> Co_Update()
        {
            string prev = string.Empty;

            while (true)
            {
                string current = GetParameters();

                //if (!Equals(current, prev) || animator.GetFloat(Define.MOVEMENT) >= Define.THRESHOLD_MOVEMENT)
                if (!Equals(current, prev))
                {
                    C_SET_ANIMATION();
                }

                prev = current.ToString();

                yield return Timing.WaitForSeconds(interval);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (isMine)
            {
                _ = Timing.KillCoroutines(SetAnimationHandler);
            }
            else
            {
                GameClientManager.Instance.mainConnection.RemoveHandler(S_SET_ANIMATION);
            }
        }

        #endregion

        #region Core Methods

        private void C_SET_ANIMATION()
        {
            C_SET_ANIMATION packet = new()
            {
                GameObjectId = objectId
            };

            AnimationParameter movement = new()
            {
                FloatParam = animator.GetFloat(Define.MOVEMENT)
            };
            packet.Params.Add(Define.MOVEMENT, movement);

            AnimationParameter jump = new()
            {
                IntParam = animator.GetInteger(Define.JUMP)
            };
            packet.Params.Add(Define.JUMP, jump);

            AnimationParameter sit = new()
            {
                BoolParam = animator.GetBool(Define.SIT)
            };
            packet.Params.Add(Define.SIT, sit);

            GameClientManager.Instance.mainConnection.Send(PacketManager.MakeSendBuffer(packet));
        }

        private CoroutineHandle updateAnimation;

        private void S_SET_ANIMATION( S_SET_ANIMATION _packet )
        {
            if (_packet.GameObjectId != objectId)
            {
                return;
            }

            print("set animation arrived : " + connection.calcuatedServerTime);

            if (updateAnimation.IsRunning)
            {
                _ = Timing.KillCoroutines(updateAnimation);
            }

            updateAnimation = Timing.RunCoroutine(Co_SET_ANIMATION(_packet), "Co_SET_ANIMATION");
        }

        private IEnumerator<float> Co_SET_ANIMATION( S_SET_ANIMATION target )
        {
            stopwatch.Reset();
            stopwatch.Start();

            animator.SetInteger(Define.JUMP, target.Params[Define.JUMP].IntParam);
            animator.SetBool(Define.SIT, target.Params[Define.SIT].BoolParam);

            for (int currentStep = 1; currentStep <= totalStep; currentStep++)
            {
                foreach (KeyValuePair<string, AnimationParameter> item in target.Params)
                {
                    switch (item.Key)
                    {
                        case Define.MOVEMENT:
                            {
                                animator.SetFloat(Define.MOVEMENT, Mathf.Lerp(animator.GetFloat(Define.MOVEMENT), item.Value.FloatParam, (float)currentStep / totalStep));
                                break;
                            }
                    }
                }

                yield return Timing.WaitForSeconds((float)(interval * currentStep / totalStep) - (float)stopwatch.Elapsed.TotalSeconds);
            }

            stopwatch.Stop();
        }

        #endregion


        #region Basic Methods

        private string GetParameters()
        {
            StringBuilder builder = new();

            _ = builder.Append(animator.GetFloat(Define.MOVEMENT).ToString("N4"))
                   .Append(animator.GetInteger(Define.JUMP))
                   .Append(animator.GetBool(Define.SIT));

            return builder.ToString();
        }

        #endregion
    }
}