using Unity.Entities;
using UnityEngine;

namespace Mlf.Brains.States
{
    public struct PlayDefaultSystemTag : IComponentData { }

    public struct PlayScore : IComponentData
    {
        public float value;
    }

    public struct PlayState : IComponentData
    {
        public float value;
        public bool skipNextStateSelection;
    }

    public struct PlayStateCurrent : IComponentData { }

    public struct PlayData : IComponentData
    {
        public float playLPS;
        public float valueThreshold;
        //public float lastCheck; delay to check every sec, or every so many frames
    }





    //Systems
    [UpdateBefore(typeof(StateSelectionSystem))]
    class PlayManagementSystem : SystemBase
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


            Entities
                .WithName("PlayManagementSystem")
                .WithAll<PlayDefaultSystemTag>()
                .ForEach((ref PlayState state, in PlayData data) =>
            {
                //calculate current hunger


                state.value += data.playLPS * deltaTime;
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


                   if (completeTag.progress == StateCompleteProgress.removingOldStateCurrentTag)
                   {
                       Debug.Log("Play, removeing old tag");
                       if (currentState.state == BrainStates.Play)
                           ecb.RemoveComponent<PlayStateCurrent>(entityInQueryIndex, entity);

                   }
                   else if (completeTag.progress == StateCompleteProgress.loadingNewState)
                   {
                       if (currentState.state == BrainStates.Play)
                       {
                           Debug.Log("Loading Play State");
                           ecb.AddComponent<PlayStateCurrent>(entityInQueryIndex, entity);

                       }

                   }
                   else if (completeTag.progress == StateCompleteProgress.choosingNewState)
                   {
                       if (state.skipNextStateSelection)
                       {
                           state.skipNextStateSelection = false;
                           score.value = 0;
                       }
                       else if (state.value < data.valueThreshold)//if not hungry just return 0
                       {
                           score.value = 0;
                       }
                       else
                       {
                           score.value = ScoreUtils.calculateDefaultScore(state.value);
                           if (score.value > currentState.score)
                           {
                               currentState.score = score.value;
                               currentState.state = BrainStates.Play;
                           }
                       }

                   }



               }).Schedule(Dependency);
            Dependency = job2;
            endSimulationEcbSystem.AddJobHandleForProducer(Dependency);

        }
    }

    class PlayAction : SystemBase
    {
        protected EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_EndSimulationEcbSystem = World
                .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }


        protected override void OnUpdate()
        {
            //Here we will have several steps to complete, find food source, go to it
            //eat it
            var entityCommandBuffer =
                m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

            Dependency = Entities
                .ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    // do your magic
                    //entityCommandBuffer.AddComponent<MyReadWriteExampleComponent>(entityInQueryIndex, entity);
                })
                .ScheduleParallel(Dependency);

            m_EndSimulationEcbSystem.AddJobHandleForProducer(Dependency);

            //when finished add ActionCompleteTag
        }
    }
}