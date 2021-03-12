using BovineLabs.Event.Jobs;
using BovineLabs.Event.Systems;
using Mlf.Grid2d;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.Map2d
{


    public struct AskForItemOwnershipEvent
    {
        public int mapId;
        public int indexPos;
        public int userId;
        public Entity entity;
    }

    [System.Serializable]
    public struct MapComponentShared : ISharedComponentData
    {
        public int mapId;
    }


    public struct MapItemComponent : IComponentData
    {
        public Type type;
        public int posIndex;
        public int itemId;
    }

    public struct MapPlantItemComponent : IComponentData
    {

    }





    public struct MapItemData
    {
        public int mapid;
        public int2 gridSize;
        public float3 cellSize;
        //public NativeArray<CellItem> items;


        //public bool dirty;
        //public BlobAssetReference<PlantItemsBlob> plantsRef;





        public int getIndex(in int2 pos)
        {
            return pos.x + (pos.y * gridSize.x);
        }

    }








    //[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    public class MapItemManagerSystem : SystemBase
    {
        //pointers
        public static World world;
        private EventSystem eventSystem;
        EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;

        //item references
        public static NativeHashMap<int, ItemReference> ItemReferences;
        public static NativeHashMap<int, Entity> EntityItemDict;

        //main map
        public static int mainMapId;

        //public static MapItemData MainMapItemData;
        public static NativeHashMap<int, GridItem> MainMapPlantItems;
        public static NativeHashMap<int, GridItem> MainMapBuildingItems;


        protected override void OnCreate()
        {
            base.OnCreate();
            world = World;
            endSimulationEcbSystem =
              World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            eventSystem = World.GetOrCreateSystem<EventSystem>();
        }

        protected override void OnUpdate()
        {

            //var ecb = endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

            //var mainMapPlantItems = new NativeHashMap<int, GridItem>();


            Dependency = new EventJob
            {
                mainMapId = mainMapId,
                mainMapPlantItems = MainMapPlantItems

            }.Schedule<EventJob, AskForItemOwnershipEvent>(eventSystem);
            endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //References
            if (ItemReferences.IsCreated) ItemReferences.Dispose();

            //Entity Dic
            if (EntityItemDict.IsCreated) EntityItemDict.Dispose();

            //Items
            if (MainMapPlantItems.IsCreated) MainMapPlantItems.Dispose();

        }



        [BurstCompile]
        private struct EventJob : IJobEvent<AskForItemOwnershipEvent>
        {
            public int mainMapId;
            //public NativeQueue<int>.ParallelWriter Counter;
            public NativeHashMap<int, GridItem> mainMapPlantItems;


            public void Execute(AskForItemOwnershipEvent e)
            {
                Debug.Log($"===============Reading onwership requests from: {e.userId}, {e.indexPos} ");
                //get the item
                if (e.mapId == mainMapId)
                {

                }
                if (mainMapPlantItems[e.indexPos].currentOwner == 0)
                {
                    GridItem item = mainMapPlantItems[e.indexPos];
                    item.currentOwner = e.userId;
                    mainMapPlantItems[e.indexPos] = item;
                }
                //is it free

                //

            }
        }













        public static void LoadPlantItems(MapDataSO map)
        {

            /*
			Debug.Log("Loading Map ITEMS to ECS: " + map.id);

			//--------------- Items references
			if (!ItemReferences.IsCreated)
			{
				using (var blobAssetStore = new BlobAssetStore())
				{
					var conversionSettings = GameObjectConversionSettings.FromWorld(
						World.DefaultGameObjectInjectionWorld, blobAssetStore);

					


					ItemReferences = new NativeHashMap<int, ItemReference>(map.mapItemReferenceList.items.Length, Allocator.Persistent);
					EntityItemDict = new NativeHashMap<int, Entity>(map.mapItemReferenceList.items.Length, Allocator.Persistent);
					ItemReference r;
					MapItem mi;
					for (int i = 0; i < map.mapItemReferenceList.items.Length; i++)
					{
						mi = map.mapItemReferenceList.items[i];


						EntityItemDict[mi.id] = GameObjectConversionUtility.ConvertGameObjectHierarchy(mi.prefab, conversionSettings);

						r = new ItemReference
						{

							id = mi.id,
							//name = mi.name,
							//description = mi.description,
							harvestQuality = mi.harvestQuality,
							type = mi.type,
							//workTypes = mi.workTypes,
							//toolTypes = mi.toolTypes,
							maxQuantity = mi.maxQuantity
						};
						r.setWorkTypes(mi.workTypes);
						ItemReferences.Add(mi.id, r);
					}
				}
			}



			//------------------ Map Data
			var data = new MapItemData
			{
				mapid = map.id,
				gridSize = new int2(map.MapGenerationSO.width,
									map.MapGenerationSO.height),
				cellSize = new float3(map.MapGenerationSO.voxelSize,
									  map.MapGenerationSO.voxelHeight,
									  map.MapGenerationSO.voxelSize),
			};

			if(mapType == MapType.main)
				MainMapItemData = data;


			//------------------Map Items
			GridItem g;
			ItemReference ir;
			int index;
			//bool success;
			var PItems = new NativeHashMap<int, GridItem>(50, Allocator.Persistent);
			//var FItems = new NativeHashMap<int, int>(50, Allocator.Persistent);
			//var OItems = new NativeHashMap<int, int>(50, Allocator.Persistent);

			Entity e;
			float3 cellSize = new float3(map.MapGenerationSO.voxelSize,
										 map.MapGenerationSO.voxelHeight,
										 map.MapGenerationSO.voxelSize);

			
				for (int i = 0; i < map.mapItems.Count; i++)
				{
					g = map.mapItems[i];
					ir = ItemReferences[g.typeId];

				Debug.Log($"Loading item:: {ir.type}");

					if (ir.type == Type.Plant)
					{
						index= data.getIndex(in g.pos);
						PItems[index] = g;
						//place the item on map
						e =  world.EntityManager.Instantiate(EntityItemDict[g.typeId]);
						world.EntityManager.AddSharedComponentData(e, new MapComponentShared { mapId=0 });
						world.EntityManager.AddComponentData(e, new MapItemComponent
						{
							itemId = g.typeId,
						    posIndex = index
						});
					world.EntityManager.AddComponentData(e, new SelectableData
					{
						selected = false,
						type = SelectableType.plant
					});
						world.EntityManager.AddComponentData(e, new MapPlantItemComponent { });
						world.EntityManager.SetName(e, $"Plant: {g.pos}");
						world.EntityManager.AddComponentData(e, new Translation
						{
							//Value = Brains.Grid.UtilsGrid.GetCellWorldCoordinates(
								//map.Grid[map.GetIndex(g.pos)].pos,
							//	cellSize)
						//});
					}
				
			}

			if(mapType == MapType.main)
            {
				if (MainMapPlantItems.IsCreated) MainMapPlantItems.Dispose();
				MainMapPlantItems = PItems;
            }				


			*/
        }
    }
}
