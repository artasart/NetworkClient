using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using MEC;
using Protocol;
using UnityEngine;

namespace FrameWork.Network
{
	public class NetworkAnimator : NetworkComponent
	{
		#region Members

		Animator animator;

		#endregion



		#region Initialize

		protected override void OnDestroy()
		{
			base.OnDestroy();

			Timing.KillCoroutines(handle_update);
		}

		protected override void Awake()
		{
			base.Awake();

			animator = GetComponent<Animator>();
		}

		protected override void Start()
		{
			base.Start();
		}

		private void Update()
		{
			if (isMine) return;
		}

		#endregion




		//private void S_BASE_ANIMATION(PacketSession _session, IMessage _packet)
		//{
		//	var packet = _packet as S_SET_ANIMATION;

		//	if (packet.ObjectId != objectId) return;

		//	packetAnimation = packet.Animation.Decompress().TrimEnd();
		//	animations = packet.Animation.Decompress().TrimEnd().Split(' ');

		//	isRecieved = true;
		//}
	}
}