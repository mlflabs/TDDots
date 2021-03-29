using Mlf.Brains.States;
using UnityEngine;

namespace Mlf.Brains.Authoring.NPCFriend
{
    [CreateAssetMenu(fileName = "NpcPlantEater", menuName = "Mlf/Brains/NpcPlantEater")]
    public class NPCPlantEaterSO : ScriptableObject
    {
        public int userId;// = System.Guid.NewGuid();
        public HungerSO hunger;
        //public PlaySO play;
        public RestSO rest;
        public WanderSO wander;
        public ThirstSO thirst;
    }


}
