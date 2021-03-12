using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mlf.MyTime
{

    public class MyTimeUI : MonoBehaviour
    {

        public TextMeshProUGUI TMPTime;
        public TextMeshProUGUI TMPHour;
        public TextMeshProUGUI TMPDay;
        public TextMeshProUGUI TMPMonth;
        public TextMeshProUGUI TMPYear;




        // Start is called before the first frame update
        void Start()
        {
            TimeSystem.OnHourChanged += onHourChanged;
            TimeSystem.OnDayChanged += onDayChanged;
            TimeSystem.OnMonthChanged += onMonthChanged;
            TimeSystem.OnYearChanged += onYearChanged;
        }

        private void onYearChanged(int value)
        {
            if (TMPYear == null) return;
            TMPYear.SetText($"Year: {value}");
        }

        private void onMonthChanged(int value)
        {
            if (TMPMonth == null) return;
            TMPMonth.SetText($"Month: {value}");
        }

        private void onDayChanged(int value)
        {
            if (TMPDay == null) return;
            TMPDay.SetText($"Day: {value}");
        }

        private void onHourChanged(int value)
        {
            if (TMPHour == null) return;
            TMPHour.SetText($"Hour: {value}");
        }

        // Update is called once per frame
        void Update()
        {
            if (TMPTime == null) return;
            TMPTime.SetText($"Time: {TimeSystem.Second}");
        }
    }

}
