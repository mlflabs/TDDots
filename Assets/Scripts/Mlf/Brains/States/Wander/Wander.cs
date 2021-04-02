using Unity.Entities;
using UnityEngine;

namespace Mlf.Brains.States
{

    //just make the default system run State system
    public struct WanderDefaultSystemTag : IComponentData { }

    public struct WanderScore : IComponentData
    {
        public float Value;
        public float GETValue() => Value;
    }

    public struct WanderState : IComponentData
    {
        public float DefaultNeed;
    }


    [UpdateBefore(typeof(StateSelectionSystem))]
    class WanderManagementSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _endSimulationEcbSystem =
              World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {

            float deltaTime = Time.DeltaTime;
            var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
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


                   if (completeTag.Progress == StateCompleteProgress.removingOldStateCurrentTag)
                   {
                       Debug.Log("Hunger, removeing old tag");
                       if (currentState.State == BrainStates.Wander)
                           ecb.RemoveComponent<WanderStateCurrent>(entityInQueryIndex, entity);

                   }
                   else if (completeTag.Progress == StateCompleteProgress.loadingNewState)
                   {
                       if (currentState.State == BrainStates.Wander)
                       {
                           Debug.Log("Loading Wander State");
                           ecb.AddComponent<WanderStateCurrent>(entityInQueryIndex, entity);
                       }


                   }
                   else if (completeTag.Progress == StateCompleteProgress.choosingNewState)
                   {

                       score.Value = ScoreUtils.CalculateDefaultScore(state.DefaultNeed);
                       if (score.Value > currentState.Score)
                       {
                           currentState.Score = score.Value;
                           currentState.State = BrainStates.Wander;
                           Debug.Log($"Score Wander::: {currentState.Score}, {currentState.State}");
                       }

                       
                   }



               }).Schedule(Dependency);
            Dependency = job2;
            _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
