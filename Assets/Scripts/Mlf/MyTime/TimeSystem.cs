using BovineLabs.Event.Systems;
using Mlf.Map2d;
using Mlf.Tiles2d;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Mlf.MyTime
{

    public struct MyTimeTag : IComponentData {}

    enum TimePeriods: byte
    {
        second, minute, hour, day, month, year
    }

    //[UpdateBefore(typeof(SimulationSystemGroup))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class TimeSystem : SystemBase
    {

        
        public static float TimeSpeed = 1;//multiply the delta time

        public static float TimeInHour = 2;//run time;
        public static int HoursInDay = 2;
        public static int DaysInMonth = 2;
        public static int MonthsInYear = 2;


        public static float DeltaTime;
        public static float ElapsedTime;
        public static float Second;
        public static int Hour;
        public static int Day;
        public static int Month;
        public static int Year;

        public static Entity TimeEntity;


        //EVENTS
        public static event System.Action<int> OnHourChanged;
        public static event System.Action<int> OnDayChanged;
        public static event System.Action<int> OnMonthChanged;
        public static event System.Action<int> OnYearChanged;


        protected override void OnCreate()
        {
            base.OnCreate();

            EntityManager.RemoveComponent(TimeEntity, typeof(MyTimeTag));

            EntityManager.DestroyEntity(TimeEntity);
            TimeEntity = EntityManager.CreateEntity(typeof(MyTimeTag));
            
        }


        protected override void OnUpdate()
        {

            DeltaTime = Time.DeltaTime * TimeSpeed;
            ElapsedTime += DeltaTime;

            Second += DeltaTime;
            if(Second >= TimeInHour)
            {
                Hour += 1;
                Second -= TimeInHour;
                if(Hour < HoursInDay)
                    OnHourChanged?.Invoke(Hour);
            }
            if(Hour >= HoursInDay)
            {
                Day += 1;
                Hour -= HoursInDay;
                OnHourChanged?.Invoke(Hour);
                
                if(Day < DaysInMonth)
                    OnDayChanged?.Invoke(Day);
            }
            if (Day >= DaysInMonth)
            {
                Month += 1;
                Day -= DaysInMonth;
                OnDayChanged?.Invoke(Day);

                if(Month < MonthsInYear)
                    OnMonthChanged?.Invoke(Month);
            }
            if(Month >= MonthsInYear)
            {
                Year += 1;
                Month -= MonthsInYear;
                OnMonthChanged?.Invoke(Month);
                OnYearChanged?.Invoke(Year);
            }

            Entities
                .WithName("MyTimeManager")
                .ForEach((Entity entity,
                         ref MyTimeTag timeData) =>
                {
                    //timeData.ElapsedTime = ElapsedTime;
                    //just have a default timetag to force update.
                }).ScheduleParallel();


        }






    }
}
