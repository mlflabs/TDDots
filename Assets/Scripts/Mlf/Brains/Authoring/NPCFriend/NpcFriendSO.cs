using Mlf.Brains.States;
using UnityEngine;

namespace Mlf.Brains.Authoring.NPCFriend
{
    [CreateAssetMenu(fileName = "NpcFriend", menuName = "Mlf/Brains/NpcFriends")]
    public class NpcFriendSO : ScriptableObject
    {
        public int userId;
        public HungerSO hunger;
        public PlaySO play;
        public RestSO rest;
        public WanderSO wander;
        public ThirstSO thirst;
    }


}
