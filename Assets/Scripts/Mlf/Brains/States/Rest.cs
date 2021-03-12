using Unity.Entities;
using UnityEngine;

namespace Mlf.Brains.States
{

    public struct RestDefaultSystemTag : IComponentData { }

    public struct RestScore : IComponentData
    {
        public float value;
        public float getValue() => value;
    }

    public struct RestState : IComponentData
    {
        public float value;
        public bool skipNextStateSelection;
    }

    public struct RestStateCurrent : IComponentData { }

    public struct RestData : IComponentData
    {
        public float restLPS;
        public float valueThreshold;
        //public float lastCheck; delay to check every sec, or every so many frames
    }

    [UpdateBefore(typeof(StateSelectionSystem))]
    class RestManagementSystem : SystemBase
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
                .WithName("RestManagementSystem")
                .WithAll<RestDefaultSystemTag>()
                .ForEach((ref RestState state, in RestData data) =>
            {
                //calculate current hunger


                state.value += data.restLPS * deltaTime;
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


                   if (completeTag.progress == StateCompleteProgress.removingOldStateCurrentTag)
                   {
                       Debug.Log("Rest, removeing old tag");
                       if (currentState.state == BrainStates.Rest)
                           ecb.RemoveComponent<RestStateCurrent>(entityInQueryIndex, entity);

                   }
                   else if (completeTag.progress == StateCompleteProgress.loadingNewState)
                   {
                       if (currentState.state == BrainStates.Rest)
                       {
                           Debug.Log("Rest Hunger State");
                           ecb.AddComponent<RestStateCurrent>(entityInQueryIndex, entity);

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
                               currentState.state = BrainStates.Rest;
                           }
                       }

                   }

               }).Schedule(Dependency);
            Dependency = job2;
            endSimulationEcbSystem.AddJobHandleForProducer(Dependency);



        }
    }

    class RestAction : SystemBase
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
