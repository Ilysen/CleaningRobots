using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts
{
	[Serializable]
	public class Ceres_CleaningRobots_AICleanSpills : IScribedPart
	{
		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade) || ID == GetInventoryActionsAlwaysEvent.ID || ID == InventoryActionEvent.ID || ID == GetDebugInternalsEvent.ID || ID == AIBoredEvent.ID;
		}

		public override bool HandleEvent(AIBoredEvent E)
		{
			if (CheckForSpills())
				return false;
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
		{
			if (RespectsAuthorityOf(E.Actor))
			{
				E.AddAction("Configure", "configure", "Configure", Key: 'c');
				if (CanDeactivate)
					E.AddAction("Deactivate", "deactivate", "Deactivate", Key: 'a');
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (E.Command == "Configure")
			{
			Configure:
				int choice = Popup.PickOption($"{ParentObject.The} {ParentObject.DisplayName} hums.", Options: new List<string>() { $"Configure cleaning threshold (currently {CleaningThreshold.Things("dram")} or lower)", $"Set to clean {(ImpureOnly ? "all liquids (currently impure only)" : "impure liquids only (currently all liquids)")}" }, AllowEscape: true);
				switch (choice)
				{
					case 0:
						int? newThreshold = Popup.AskNumber("Enter a new threshold.", Start: CleaningThreshold, Max: 64);
						if (newThreshold != null)
							CleaningThreshold = (int)newThreshold;
						goto Configure;
					case 1:
						ImpureOnly = !ImpureOnly;
						goto Configure;
				}
			}
			else if (E.Command == "Deactivate" && CanDeactivate)
			{
				if (Popup.ShowYesNoCancel($"Really deactivate {ParentObject.GetDisplayName()}?") == DialogResult.Yes)
				{
					XDidYToZ(E.Actor, "deactivate", ParentObject);
					IInventory dropInv = ParentObject.GetDropInventory();
					if (dropInv != null)
					{
						List<GameObject> objectsToDrop = ParentObject.Inventory.Objects.Where(x => x.IsReal).ToList();
						if (objectsToDrop.Count() > 0)
						{
							XDidY(ParentObject, "relinquish", $"{ParentObject.its} inventory onto the ground");
							for (int i = 0; i < objectsToDrop.Count(); i++)
							{
								GameObject go = objectsToDrop[i];
								go.RemoveFromContext(E);
								dropInv.AddObjectToInventory(go, ParentObject, true, ParentEvent: E);
								DroppedEvent.Send(ParentObject, go);
							}
						}
					}
					ParentObject.ReplaceWith(GameObject.CreateUnmodified("Ceres_CleaningRobots_DormantCleaner"));
					E.RequestInterfaceExit();
				}
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetDebugInternalsEvent E)
		{
			if (ParentObject.HasGoal(nameof(Ceres_CleaningRobots_CleanSpillGoal)))
				E.AddEntry(this, "Spill cleaning", "Cleaning a spill");
			else
				E.AddEntry(this, "Spill cleaning", "Idle");
			if (_lastSpill != null)
				E.AddEntry(this, "Last spill location", $"{_lastSpill.CurrentCell.X}, {_lastSpill.CurrentCell.Y} in {_lastSpill.CurrentZone.ZoneID}");
			else
				E.AddEntry(this, "Last spill location", "Has not cleaned a spill");
			return base.HandleEvent(E);
		}

		public override bool WantTurnTick() => true;

		public override void TurnTick(long TimeTick, int Amount) => CheckForSpills();

		/// <summary>
		/// Core AI loop. Searches the current active zone for the closest liquid pool, and if any are found, assigns a task to clean it up.
		/// </summary>
		private bool CheckForSpills()
		{
			if (ParentObject.CurrentZone == null) // Fix errors on init
				return false;
			if (!ParentObject.FireEvent("CanAIDoIndependentBehavior") || ParentObject.IsPlayerControlled())
				return false;
			if (ParentObject.IsBusy())
			{
				ParentObject.Brain.Think("I'm too busy to be cleaning.");
				return false;
			}
			var spills = ParentObject.CurrentZone.GetObjectsWithPart(nameof(LiquidVolume)).Where(x => ShouldClean(x.LiquidVolume));
			if (spills.IsNullOrEmpty())
			{
				ParentObject.Brain.Think("There's no spills to clean in this zone.");
				return false;
			}
			GameObject _targetSpill = null;
			int distance = int.MaxValue;
			foreach (var spill in spills)
			{
				if (ParentObject.canPathTo(spill.CurrentCell))
				{
					if (ParentObject.DistanceTo(spill) < distance)
					{
						_targetSpill = spill;
						distance = ParentObject.DistanceTo(spill);
					}
				}
			}
			if (_targetSpill == null)
			{
				ParentObject.Brain.Think("I couldn't find a spill to clean in this zone.");
				return false;
			}
			_lastSpill = _targetSpill;
			ParentObject.Brain.DidX("whirr", "studiously");
			ParentObject.Brain.PushGoal(new Ceres_CleaningRobots_CleanSpillGoal(_targetSpill));
			ParentObject.Brain.Think("I've acquired a spill to clean up.");
			return true;
		}

		/// <summary>
		/// Cleaners will attempt to mop up any spills that are no more than this many drams in volume.
		/// </summary>
		public int CleaningThreshold = 20;

		/// <summary>
		/// If true, only impure liquids will be cleaned up. Pure liquids will be ignored, regardless of type or volume.
		/// </summary>
		public bool ImpureOnly = true;

		/// <summary>
		/// If false, this cleaner cannot manually be deactivated, even if it respects the player's authority.
		/// </summary>
		public bool CanDeactivate = true;

		/// <summary>
		/// Determines whether the cleaner respects the authority of the provided <see cref="GameObject"/>.
		/// An object with no respected authority cannot deactivate or configure the cleaner.
		/// </summary>
		public bool RespectsAuthorityOf(GameObject go)
		{
			// we're doing our own thing, thank you very much
			if (ParentObject.HasPart<SocialRoles>() || ParentObject.HasIntProperty("Villager"))
				return false;
			// heck no, you're a jerk
			if (ParentObject.Brain.IsHostileTowards(go))
				return false;
			// we've got other stuff to do
			if (ParentObject.PartyLeader != null && !ParentObject.IsPlayerLed())
				return false;
			// well one must imagine Sisyphus happy we suppose
			return true;
		}

		/// <summary>
		/// Checks whether or not a given <see cref="LiquidVolume"/> should be cleaned.
		/// In addition to logic checking for <see cref="CleaningThreshold"/> and <see cref="ImpureOnly"/>, only non-sealed open volumes can be cleaned.
		/// </summary>
		private bool ShouldClean(LiquidVolume lv) => lv.IsOpenVolume() && !lv.EffectivelySealed() && lv.Volume <= CleaningThreshold && (!ImpureOnly || !lv.IsPure());

		/// <summary>
		/// A cached value referring to the last spill we were cleaning up. This is currently only used for debugging.
		/// </summary>
		private GameObject _lastSpill;
	}
}
