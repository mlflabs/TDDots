using Mlf.Map2d;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Mlf.Npc
{






    //[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    public class NpcManagerSystem : SystemBase
    {
       
        public static NativeHashMap<int, Entity> MainMapNpcs;
        public static NativeHashMap<int, Entity> SecondaryMapNpcs;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            MainMapNpcs = new NativeHashMap<int, Entity>(100, Allocator.Persistent);
            SecondaryMapNpcs = new NativeHashMap<int, Entity>(1, Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (MainMapNpcs.IsCreated) MainMapNpcs.Dispose();
            
            if (SecondaryMapNpcs.IsCreated) SecondaryMapNpcs.Dispose();
        }



        public static int AddNpc(Entity e, int id, MapType map)
        {
            if(id == 0)
            {
                id = getUniqueId();
            }

            if(map == MapType.main)
            {
                MainMapNpcs[id] = e;
            }
            else if(map == MapType.secondary)
            {
                SecondaryMapNpcs[id] = e;
            }
            else
            {
                Debug.LogError("Map type not recognized::: " + map);
            }

            return id;
        }

        private static int getUniqueId()
        {
            bool unique = false;
            int r = 0;
            while (!unique)
            {
                r = UnityEngine.Random.Range(1, int.MaxValue);

                unique = true;
                if (MainMapNpcs.ContainsKey(r)) unique = false;
                if (SecondaryMapNpcs.ContainsKey(r)) unique = false;
            }

            return r;
        }


    }
}
