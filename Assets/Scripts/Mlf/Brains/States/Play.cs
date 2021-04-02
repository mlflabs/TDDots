using Unity.Entities;
using UnityEngine;

namespace Mlf.Brains.States
{
    public struct PlayDefaultSystemTag : IComponentData { }

    public struct PlayScore : IComponentData
    {
        public float Value;
    }

    public struct PlayState : IComponentData
    {
        public float Value;
        public bool SkipNextStateSelection;
    }

    public struct PlayStateCurrent : IComponentData { }

    public struct PlayData : IComponentData
    {
        public float PlayLps;
        public float ValueThreshold;
        //public float lastCheck; delay to check every sec, or every so many frames
    }





    //Systems
    [UpdateBefore(typeof(StateSelectionSystem))]
    class PlayManagementSystem : SystemBase
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
                .WithName("PlayManagementSystem")
                .WithAll<PlayDefaultSystemTag>()
                .ForEach((ref PlayState state, in PlayData data) =>
            {
                //calculate current hunger


                state.Value += data.PlayLps * deltaTime;
            }).ScheduleParallel();


            var job2 = Entities
               .WithName("HungerScoreSystem")
               .ForEach((Entity entity,
                         int entityInQueryIndex,
                         ref PlayScore score,
                         ref PlayState state,
                         ref CurrentBrainState currentState,
                         in StateCompleteTag completeTag,
                         in PlayData data) =>
               {


                   if (completeTag.Progress == StateCompleteProgress.removingOldStateCurrentTag)
                   {
                       Debug.Log("Play, removeing old tag");
                       if (currentState.State == BrainStates.Play)
                           ecb.RemoveComponent<PlayStateCurrent>(entityInQueryIndex, entity);

                   }
                   else if (completeTag.Progress == StateCompleteProgress.loadingNewState)
                   {
                       if (currentState.State == BrainStates.Play)
                       {
                           Debug.Log("Loading Play State");
                           ecb.AddComponent<PlayStateCurrent>(entityInQueryIndex, entity);

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
                               currentState.State = BrainStates.Play;
                           }
                       }

                   }



               }).Schedule(Dependency);
            Dependency = job2;
            _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);

        }
    }

    class PlayAction : SystemBase
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