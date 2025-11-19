using NUnit.Framework;
using Knockout.Combat;
using Knockout.Combat.States;

namespace Knockout.Tests.EditMode.Combat
{
    /// <summary>
    /// Edit mode tests for combat state machine and state transitions.
    /// </summary>
    public class CombatStateMachineTests
    {
        [Test]
        public void IdleState_CanTransitionTo_AttackingState()
        {
            var idleState = new IdleState();
            var attackingState = new AttackingState();

            Assert.IsTrue(idleState.CanTransitionTo(attackingState));
        }

        [Test]
        public void IdleState_CanTransitionTo_BlockingState()
        {
            var idleState = new IdleState();
            var blockingState = new BlockingState();

            Assert.IsTrue(idleState.CanTransitionTo(blockingState));
        }

        [Test]
        public void IdleState_CannotTransitionTo_IdleState()
        {
            var idleState = new IdleState();
            var anotherIdleState = new IdleState();

            Assert.IsFalse(idleState.CanTransitionTo(anotherIdleState));
        }

        [Test]
        public void AttackingState_CannotTransitionTo_BlockingState()
        {
            var attackingState = new AttackingState();
            var blockingState = new BlockingState();

            Assert.IsFalse(attackingState.CanTransitionTo(blockingState));
        }

        [Test]
        public void AttackingState_CannotTransitionTo_AnotherAttackingState()
        {
            var attackingState = new AttackingState();
            var anotherAttackingState = new AttackingState();

            Assert.IsFalse(attackingState.CanTransitionTo(anotherAttackingState));
        }

        [Test]
        public void AttackingState_CanTransitionTo_IdleState()
        {
            var attackingState = new AttackingState();
            var idleState = new IdleState();

            Assert.IsTrue(attackingState.CanTransitionTo(idleState));
        }

        [Test]
        public void AttackingState_CanTransitionTo_HitStunnedState()
        {
            var attackingState = new AttackingState();
            var hitStunnedState = new HitStunnedState();

            Assert.IsTrue(attackingState.CanTransitionTo(hitStunnedState));
        }

        [Test]
        public void BlockingState_CannotTransitionTo_AttackingState()
        {
            var blockingState = new BlockingState();
            var attackingState = new AttackingState();

            Assert.IsFalse(blockingState.CanTransitionTo(attackingState));
        }

        [Test]
        public void BlockingState_CanTransitionTo_IdleState()
        {
            var blockingState = new BlockingState();
            var idleState = new IdleState();

            Assert.IsTrue(blockingState.CanTransitionTo(idleState));
        }

        [Test]
        public void HitStunnedState_CannotTransitionTo_AttackingState()
        {
            var hitStunnedState = new HitStunnedState();
            var attackingState = new AttackingState();

            Assert.IsFalse(hitStunnedState.CanTransitionTo(attackingState));
        }

        [Test]
        public void HitStunnedState_CanTransitionTo_IdleState()
        {
            var hitStunnedState = new HitStunnedState();
            var idleState = new IdleState();

            Assert.IsTrue(hitStunnedState.CanTransitionTo(idleState));
        }

        [Test]
        public void KnockedDownState_CannotTransitionTo_AttackingState()
        {
            var knockedDownState = new KnockedDownState();
            var attackingState = new AttackingState();

            Assert.IsFalse(knockedDownState.CanTransitionTo(attackingState));
        }

        [Test]
        public void KnockedDownState_CanTransitionTo_KnockedOutState()
        {
            var knockedDownState = new KnockedDownState();
            var knockedOutState = new KnockedOutState();

            Assert.IsTrue(knockedDownState.CanTransitionTo(knockedOutState));
        }

        [Test]
        public void KnockedOutState_CannotTransitionTo_AnyState()
        {
            var knockedOutState = new KnockedOutState();
            var idleState = new IdleState();
            var attackingState = new AttackingState();
            var blockingState = new BlockingState();

            Assert.IsFalse(knockedOutState.CanTransitionTo(idleState));
            Assert.IsFalse(knockedOutState.CanTransitionTo(attackingState));
            Assert.IsFalse(knockedOutState.CanTransitionTo(blockingState));
        }

        [Test]
        public void AllStates_CanTransitionTo_KnockedOutState()
        {
            var knockedOutState = new KnockedOutState();
            var states = new CombatState[]
            {
                new IdleState(),
                new AttackingState(),
                new BlockingState(),
                new HitStunnedState(),
                new KnockedDownState()
            };

            foreach (var state in states)
            {
                Assert.IsTrue(state.CanTransitionTo(knockedOutState),
                    $"{state.StateName} should be able to transition to KnockedOutState");
            }
        }

        #region ExhaustedState Tests

        [Test]
        public void IdleState_CanTransitionTo_ExhaustedState()
        {
            var idleState = new IdleState();
            var exhaustedState = new ExhaustedState();

            Assert.IsTrue(idleState.CanTransitionTo(exhaustedState),
                "Should be able to transition from Idle to Exhausted when stamina depletes");
        }

        [Test]
        public void ExhaustedState_CannotTransitionTo_AttackingState()
        {
            var exhaustedState = new ExhaustedState();
            var attackingState = new AttackingState();

            Assert.IsFalse(exhaustedState.CanTransitionTo(attackingState),
                "Cannot attack while exhausted");
        }

        [Test]
        public void ExhaustedState_CanTransitionTo_BlockingState()
        {
            var exhaustedState = new ExhaustedState();
            var blockingState = new BlockingState();

            Assert.IsTrue(exhaustedState.CanTransitionTo(blockingState),
                "Defensive actions allowed while exhausted");
        }

        [Test]
        public void ExhaustedState_CanTransitionTo_HitStunnedState()
        {
            var exhaustedState = new ExhaustedState();
            var hitStunnedState = new HitStunnedState();

            Assert.IsTrue(exhaustedState.CanTransitionTo(hitStunnedState),
                "Can be hit while exhausted");
        }

        [Test]
        public void AttackingState_CanTransitionTo_ExhaustedState()
        {
            var attackingState = new AttackingState();
            var exhaustedState = new ExhaustedState();

            Assert.IsTrue(attackingState.CanTransitionTo(exhaustedState),
                "Can become exhausted during attack");
        }

        #endregion
    }
}
