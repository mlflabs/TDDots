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

        public static float timeInHour = 2;//run time;
        public static int hoursInDay = 2;
        public static int daysInMonth = 2;
        public static int monthsInYear = 2;


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
            if(Second >= timeInHour)
            {
                Hour += 1;
                Second -= timeInHour;
                if(Hour < hoursInDay)
                    OnHourChanged?.Invoke(Hour);
            }
            if(Hour >= hoursInDay)
            {
                Day += 1;
                Hour -= hoursInDay;
                OnHourChanged?.Invoke(Hour);
                
                if(Day < daysInMonth)
                    OnDayChanged?.Invoke(Day);
            }
            if (Day >= daysInMonth)
            {
                Month += 1;
                Day -= daysInMonth;
                OnDayChanged?.Invoke(Day);

                if(Month < monthsInYear)
                    OnMonthChanged?.Invoke(Month);
            }
            if(Month >= monthsInYear)
            {
                Year += 1;
                Month -= monthsInYear;
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
