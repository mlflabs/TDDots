using Unity.Entities;
using UnityEngine;

namespace Mlf.Brains
{
    public enum BrainStates : byte
    {
        Play,
        Rest,
        Eat,
        Drink,
        Wander,
        NoState, //deciding on next state
    }



    public struct CurrentBrainState : IComponentData
    {
        public BrainStates state; // current action to perform
        public float score;
        public bool finished;
    }


    public enum StateCompleteProgress : byte
    {
        choosingNewState,
        loadingNewState,
        newStateLoaded,
        remvingStateCompleteTag,
        removingOldStateCurrentTag
    }
    struct StateCompleteTag : IComponentData
    {

        public StateCompleteProgress progress;
    }


    class StateSelectionSystem : SystemBase
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
            var ecb = endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

            var job1 = Entities
               .WithName("BrainStateSeleciton")
               .ForEach((Entity entity,
                         int entityInQueryIndex,
                         ref StateCompleteTag completeTag,
                         in CurrentBrainState currentState) =>
               {
                   Debug.Log($"Loading State: {completeTag.progress}");

                   //this runs after all the state systems, so just keep moving progress
                   // previous state finished, its current tag removed, initiate state seleciton
                   if (completeTag.progress == StateCompleteProgress.removingOldStateCurrentTag)
                   {
                       Debug.Log($"00000 -- {currentState.state}");
                       //old state tag removed, so no current state, initiate scorers
                       completeTag.progress = StateCompleteProgress.choosingNewState;
                   }
                   else if (completeTag.progress == StateCompleteProgress.choosingNewState)
                   {
                       Debug.Log($"1111111 -- {currentState.state}");
                       //state systems have inputed score, now let the highest load tag
                       completeTag.progress = StateCompleteProgress.loadingNewState;
                   }
                   else if (completeTag.progress == StateCompleteProgress.loadingNewState)
                   {
                       Debug.Log($"22222 -- {currentState.state}");
                       //new state loaded, remove currentStateTag, done with this system
                       completeTag.progress = StateCompleteProgress.remvingStateCompleteTag;
                   }
                   else if (completeTag.progress == StateCompleteProgress.remvingStateCompleteTag)
                   {
                       Debug.Log($"33333 -- {currentState.state}");
                       //state selection finished, remove this tag
                       ecb.RemoveComponent<StateCompleteTag>(entityInQueryIndex, entity);
                   }


               }).ScheduleParallel(Dependency);



            var finalDep2 = Entities
               .WithName("BrainCheckStateProgressStatus")
               .ForEach((Entity entity,
                        int entityInQueryIndex,
                        ref CurrentBrainState currentState) =>
               {
                   if (currentState.finished)
                   {
                       Debug.Log("Startng new state, first remove prvious state tag");
                       ecb.AddComponent<StateCompleteTag>(entityInQueryIndex, entity,
                           new StateCompleteTag { progress = StateCompleteProgress.removingOldStateCurrentTag });
                       currentState.finished = false;
                       currentState.score = 0;
                       Debug.Log($"Removed State");
                       //currentState.state = BrainStates.NoState;
                   }
               }).Schedule(job1);


            Dependency = finalDep2;

            endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
        }




    }
}



