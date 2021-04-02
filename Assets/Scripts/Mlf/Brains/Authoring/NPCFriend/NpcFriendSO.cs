using Mlf.Brains.States;
using UnityEngine;

namespace Mlf.Brains.Authoring.NPCFriend
{
    [CreateAssetMenu(fileName = "NpcFriend", menuName = "Mlf/Brains/NpcFriends")]
    public class NpcFriendSo : ScriptableObject
    {
        public int userId;
        public HungerSo hunger;
        public PlaySo play;
        public RestSo rest;
        public WanderSo wander;
        public ThirstSo thirst;
    }


}
