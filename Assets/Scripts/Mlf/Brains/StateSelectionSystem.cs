using Mlf.Npc;
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
        public BrainStates State; // current action to perform
        public float Score;
        public bool Finished;
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

        public StateCompleteProgress Progress;
    }


    public struct NpcData : IComponentData
    {
        public int UserId;
    }




    class StateSelectionSystem : SystemBase
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
            var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

            var job1 = Entities
               .WithName("BrainStateSeleciton")
               .ForEach((Entity entity,
                         int entityInQueryIndex,
                         ref StateCompleteTag completeTag,
                         in CurrentBrainState currentState) =>
               {
                   Debug.Log($"Loading State: {completeTag.Progress}");

                   //this runs after all the state systems, so just keep moving progress
                   // previous state finished, its current tag removed, initiate state seleciton
                   if (completeTag.Progress == StateCompleteProgress.removingOldStateCurrentTag)
                   {
                       Debug.Log($"00000 22-- {currentState.State}");
                       //old state tag removed, so no current state, initiate scorers
                       completeTag.Progress = StateCompleteProgress.choosingNewState;
                   }
                   else if (completeTag.Progress == StateCompleteProgress.choosingNewState)
                   {
                       Debug.Log($"1111111 22-- {currentState.State}");
                       //state systems have inputed score, now let the highest load tag
                       completeTag.Progress = StateCompleteProgress.loadingNewState;
                   }
                   else if (completeTag.Progress == StateCompleteProgress.loadingNewState)
                   {
                       Debug.Log($"22222 33-- {currentState.State}");
                       //new state loaded, remove currentStateTag, done with this system
                       completeTag.Progress = StateCompleteProgress.remvingStateCompleteTag;
                   }
                   else if (completeTag.Progress == StateCompleteProgress.remvingStateCompleteTag)
                   {
                       Debug.Log($"33333 44-- {currentState.State}");
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
                   if (currentState.Finished)
                   {
                       Debug.Log("Startng new state, first remove prvious state tag");
                       ecb.AddComponent<StateCompleteTag>(entityInQueryIndex, entity,
                           new StateCompleteTag { Progress = StateCompleteProgress.removingOldStateCurrentTag });
                       currentState.Finished = false;
                       currentState.Score = 0;
                       Debug.Log($"Removed State");
                       //currentState.state = BrainStates.NoState;
                   }
               }).Schedule(job1);


            Dependency = finalDep2;

            _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
        }




    }
}



