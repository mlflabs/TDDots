using Mlf.Brains.States;
using UnityEngine;

namespace Mlf.Brains.Authoring.NPCFriend
{
    [CreateAssetMenu(fileName = "NpcPlantEater", menuName = "Mlf/Brains/NpcPlantEater")]
    public class NpcPlantEaterSo : ScriptableObject
    {
        public int userId;// = System.Guid.NewGuid();
        public HungerSo hunger;
        //public PlaySO play;
        public RestSo rest;
        public WanderSo wander;
        public ThirstSo thirst;
    }


}
