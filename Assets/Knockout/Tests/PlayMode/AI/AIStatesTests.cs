using NUnit.Framework;
using UnityEngine;
using Knockout.AI;
using Knockout.AI.States;

namespace Knockout.Tests.PlayMode.AI
{
    /// <summary>
    /// Play mode tests for AI behavioral states and decision logic.
    /// </summary>
    public class AIStatesTests
    {
        [Test]
        public void ObserveState_TransitionsToApproach_WhenPlayerTooFar()
        {
            // Arrange
            var observeState = new ObserveState();
            var context = new AIContext
            {
                DistanceToPlayer = 5.0f,
                OwnHealthPercentage = 100f,
                PlayerHealthPercentage = 100f,
                PlayerIsAttacking = false,
                TimeSinceLastStateChange = 0.5f
            };

            observeState.Enter(context);

            // Act
            var nextState = observeState.Update(context);

            // Assert
            Assert.IsNotNull(nextState);
            Assert.IsInstanceOf<ApproachState>(nextState);
        }

        [Test]
        public void ObserveState_TransitionsToRetreat_WhenPlayerTooClose()
        {
            // Arrange
            var observeState = new ObserveState();
            var context = new AIContext
            {
                DistanceToPlayer = 1.0f,
                OwnHealthPercentage = 100f,
                PlayerHealthPercentage = 100f,
                PlayerIsAttacking = false,
                TimeSinceLastStateChange = 0.5f
            };

            observeState.Enter(context);

            // Act
            var nextState = observeState.Update(context);

            // Assert
            Assert.IsNotNull(nextState);
            Assert.IsInstanceOf<RetreatState>(nextState);
        }

        [Test]
        public void ApproachState_TransitionsToObserve_WhenInOptimalRange()
        {
            // Arrange
            var approachState = new ApproachState();
            var context = new AIContext
            {
                DistanceToPlayer = 2.5f,
                OwnHealthPercentage = 100f,
                PlayerHealthPercentage = 100f,
                PlayerIsAttacking = false,
                TimeSinceLastStateChange = 0.5f
            };

            approachState.Enter(context);

            // Act
            var nextState = approachState.Update(context);

            // Assert
            Assert.IsNotNull(nextState);
            Assert.IsInstanceOf<ObserveState>(nextState);
        }

        [Test]
        public void RetreatState_TransitionsToObserve_WhenAtSafeDistance()
        {
            // Arrange
            var retreatState = new RetreatState();
            var context = new AIContext
            {
                DistanceToPlayer = 4.0f,
                OwnHealthPercentage = 100f,
                PlayerHealthPercentage = 100f,
                PlayerIsAttacking = false,
                TimeSinceLastStateChange = 0.5f
            };

            retreatState.Enter(context);

            // Act
            var nextState = retreatState.Update(context);

            // Assert
            Assert.IsNotNull(nextState);
            Assert.IsInstanceOf<ObserveState>(nextState);
        }

        [Test]
        public void AttackState_ChoosesUppercut_WhenCloseRange()
        {
            // Arrange
            var attackState = new AttackState();
            var context = new AIContext
            {
                DistanceToPlayer = 1.0f,
                OwnHealthPercentage = 100f,
                PlayerHealthPercentage = 100f,
                PlayerIsAttacking = false
            };

            // Act
            int selectedAttack = attackState.ChooseAttack(context);

            // Assert
            Assert.AreEqual(AttackState.ATTACK_UPPERCUT, selectedAttack);
        }

        [Test]
        public void DefendState_TransitionsToObserve_AfterMinimumDuration()
        {
            // Arrange
            var defendState = new DefendState();
            var context = new AIContext
            {
                DistanceToPlayer = 2.5f,
                OwnHealthPercentage = 100f,
                PlayerHealthPercentage = 100f,
                PlayerIsAttacking = false,
                TimeSinceLastStateChange = 1.0f // After minimum block duration
            };

            defendState.Enter(context);

            // Act
            var nextState = defendState.Update(context);

            // Assert
            Assert.IsNotNull(nextState);
            Assert.IsInstanceOf<ObserveState>(nextState);
        }

        [Test]
        public void ObserveState_TransitionsToDefend_WhenPlayerAttacking()
        {
            // Arrange
            var observeState = new ObserveState();
            var context = new AIContext
            {
                DistanceToPlayer = 2.5f,
                OwnHealthPercentage = 100f,
                PlayerHealthPercentage = 100f,
                PlayerIsAttacking = true,
                TimeSinceLastStateChange = 0.5f
            };

            observeState.Enter(context);

            // Act
            var nextState = observeState.Update(context);

            // Assert
            Assert.IsNotNull(nextState);
            Assert.IsInstanceOf<DefendState>(nextState);
        }
    }
}
