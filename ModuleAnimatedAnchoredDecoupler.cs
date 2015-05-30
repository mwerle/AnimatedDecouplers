using System;
using System.Linq;
using KSP;
using UnityEngine;

namespace AnimatedDecouplers
{
	public class ModuleAnimatedAnchoredDecoupler : ModuleAnchoredDecoupler, IScalarModule
	{
		[KSPField]
		public string animationName;
		
		protected Animation anim;
		
		[KSPField(isPersistant = true)]
		public bool animationComplete = false;
		
		public ModuleAnimatedAnchoredDecoupler ():
		base()
		{
		}

		public override void OnAwake()
		{
			this.OnMovingEvent = new EventData<float, float>("ModuleAnimatedAnchoredDecoupler.OnMovingEvent");
			this.OnStoppedEvent = new EventData<float>("ModuleAnimatedAnchoredDecoupler.OnStoppedEvent");
		}
		
		public override void OnStart (StartState state)
		{
			GameEvents.onStageSeparation.Add (checkForDecoupling);
			base.OnStart (state);
			Debug.Log ("ModuleAnimatedAnchoredDecoupler.OnStart(), isDecoupled = " + isDecoupled.ToString ());
			if (animationName != "")
			{
				anim = part.FindModelAnimators(animationName).FirstOrDefault ();
				if ((object)anim == null)
				{
					Debug.Log ("ModuleAnimatedAnchoredDecoupler: Animations not found");
				}
				else
				{
					Debug.Log ("ModuleAnimatedAnchoredDecoupler.OnStart() - Animation found named " + animationName);
					if (this.animationComplete || this.isDecoupled)
					// If Decoupled or animation already played then set animation to end.
					{
						this.anim[animationName].normalizedTime = 1f;
					}
				}
			}
		}

		public void checkForDecoupling(EventReport separationData)
		{
			if (separationData.eventType == FlightEvents.STAGESEPARATION && separationData.origin == this.part)
			{
				// PROBABLY got called because we decoupled, but no way to know because ModuleAnchoredDecoupler doesn't SET isDecoupled until after the event fires. 
				if (animationName != "" && (!this.animationComplete || !this.anim.IsPlaying (animationName)))
				{
					OnMoving.Fire (0f, 1f);
					this.anim.Play (animationName);
					this.animationComplete = true;
					Debug.Log ("ModuleAnimatedAnchoredDecoupler.onStageSeparation() triggered animation " + this.animationName);
				}
				this.isDecoupled = true;
				this.OnStop.Fire (1f);
			}
		}

		//
		// Properties
		//
		private EventData <float, float> OnMovingEvent;
		private EventData <float> OnStoppedEvent;
		
		public bool CanMove
		{
			get
			{
				//return part.ShieldedFromAirstream;
				return true;
			}
		}
		
		public float GetScalar
		{
			get
			{
				if (isDecoupled)
					return 1f;
				else
					return 0f;
			}
		}
		
		public EventData<float, float> OnMoving
		{
			get
			{
				return OnMovingEvent;
			}
		}
		
		public EventData<float> OnStop
		{
			get
			{
				return OnStoppedEvent;
			}
		}
		
		//
		// Methods
		//
		public bool IsMoving ()
		{
			return false;
		}
		
		public void SetScalar (float t)
		{
		}
		
		public void SetUIRead (bool state)
		{
		}
		
		public void SetUIWrite (bool state)
		{
		}
	}
}
