using Unity.Entities;
using UnityEngine;

namespace Mlf.Brains.States
{

    //just make the default system run State system
    public struct WanderDefaultSystemTag : IComponentData { }

    public struct WanderScore : IComponentData
    {
        public float value;
        public float getValue() => value;
    }

    public struct WanderState : IComponentData
    {
        public float defaultNeed;
    }


    [UpdateBefore(typeof(StateSelectionSystem))]
    class WanderManagementSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            endSimulationEcbSystem =
              World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {

            float deltaTime = Time.DeltaTime;
            var ecb = endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
            /*
            Entities
               .WithName("WanderScoreSystem")
               .WithAll<StateCompleteTag>()
               .ForEach((ref WanderScore score, 
                         ref 
                         in WanderState state) =>
               {
                   //calculate current hunger
                   Debug.Log("Updated WanderScore");
                   score.value = state.defaultNeed;
               }).ScheduleParallel();
            */



            var job2 = Entities
               .WithName("WanderScoreSystem")
               .ForEach((Entity entity,
                         int entityInQueryIndex,
                         ref WanderScore score,
                         ref WanderState state,
                         ref CurrentBrainState currentState,
                         in StateCompleteTag completeTag,
                         in WanderData data) =>
               {


                   if (completeTag.progress == StateCompleteProgress.removingOldStateCurrentTag)
                   {
                       Debug.Log("Hunger, removeing old tag");
                       if (currentState.state == BrainStates.Wander)
                           ecb.RemoveComponent<WanderStateCurrent>(entityInQueryIndex, entity);

                   }
                   else if (completeTag.progress == StateCompleteProgress.loadingNewState)
                   {
                       if (currentState.state == BrainStates.Wander)
                       {
                           Debug.Log("Loading Wander State");
                           ecb.AddComponent<WanderStateCurrent>(entityInQueryIndex, entity);
                       }


                   }
                   else if (completeTag.progress == StateCompleteProgress.choosingNewState)
                   {

                       score.value = ScoreUtils.calculateDefaultScore(state.defaultNeed);
                       if (score.value > currentState.score)
                       {
                           currentState.score = score.value;
                           currentState.state = BrainStates.Wander;
                       }

                       Debug.Log($"Score Wander::: {currentState.score}, {currentState.state}");
                   }



               }).Schedule(Dependency);
            Dependency = job2;
            endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
