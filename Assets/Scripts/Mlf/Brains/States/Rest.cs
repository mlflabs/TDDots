using Unity.Entities;
using UnityEngine;

namespace Mlf.Brains.States
{

    public struct RestDefaultSystemTag : IComponentData { }

    public struct RestScore : IComponentData
    {
        public float Value;
        public float GETValue() => Value;
    }

    public struct RestState : IComponentData
    {
        public float Value;
        public bool SkipNextStateSelection;
    }

    public struct RestStateCurrent : IComponentData { }

    public struct RestData : IComponentData
    {
        public float RestLps;
        public float ValueThreshold;
        //public float lastCheck; delay to check every sec, or every so many frames
    }

    [UpdateBefore(typeof(StateSelectionSystem))]
    class RestManagementSystem : SystemBase
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

            Entities
                .WithName("RestManagementSystem")
                .WithAll<RestDefaultSystemTag>()
                .ForEach((ref RestState state, in RestData data) =>
            {
                //calculate current hunger


                state.Value += data.RestLps * deltaTime;
            }).ScheduleParallel();


            var job2 = Entities
               .WithName("RestScoreSystem")
               .ForEach((Entity entity,
                         int entityInQueryIndex,
                         ref RestScore score,
                         ref RestState state,
                         ref CurrentBrainState currentState,
                         in StateCompleteTag completeTag,
                         in RestData data) =>
               {


                   if (completeTag.Progress == StateCompleteProgress.removingOldStateCurrentTag)
                   {
                       Debug.Log("Rest, removeing old tag");
                       if (currentState.State == BrainStates.Rest)
                           ecb.RemoveComponent<RestStateCurrent>(entityInQueryIndex, entity);

                   }
                   else if (completeTag.Progress == StateCompleteProgress.loadingNewState)
                   {
                       if (currentState.State == BrainStates.Rest)
                       {
                           Debug.Log("Rest Hunger State");
                           ecb.AddComponent<RestStateCurrent>(entityInQueryIndex, entity);

                       }

                   }
                   else if (completeTag.Progress == StateCompleteProgress.choosingNewState)
                   {
                       if (state.SkipNextStateSelection)
                       {
                           state.SkipNextStateSelection = false;
                           score.Value = 0;
                       }
                       else if (state.Value < data.ValueThreshold)//if not hungry just return 0
                       {
                           score.Value = 0;
                       }
                       else
                       {
                           score.Value = ScoreUtils.CalculateDefaultScore(state.Value);
                           if (score.Value > currentState.Score)
                           {
                               currentState.Score = score.Value;
                               currentState.State = BrainStates.Rest;
                           }
                       }

                   }

               }).Schedule(Dependency);
            Dependency = job2;
            _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);



        }
    }

    class RestAction : SystemBase
    {
        protected EndSimulationEntityCommandBufferSystem MEndSimulationEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            MEndSimulationEcbSystem = World
                .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }


        protected override void OnUpdate()
        {
            //Here we will have several steps to complete, find food source, go to it
            //eat it
            var entityCommandBuffer =
                MEndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

            Dependency = Entities
                .ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    // do your magic
                    //entityCommandBuffer.AddComponent<MyReadWriteExampleComponent>(entityInQueryIndex, entity);
                })
                .ScheduleParallel(Dependency);

            MEndSimulationEcbSystem.AddJobHandleForProducer(Dependency);

            //when finished add ActionCompleteTag
        }
    }
}
