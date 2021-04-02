using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Mlf.MyTime
{

    public class MyTimeUI : MonoBehaviour
    {

        [FormerlySerializedAs("TMPTime")] public TextMeshProUGUI tmpTime;
        [FormerlySerializedAs("TMPHour")] public TextMeshProUGUI tmpHour;
        [FormerlySerializedAs("TMPDay")] public TextMeshProUGUI tmpDay;
        [FormerlySerializedAs("TMPMonth")] public TextMeshProUGUI tmpMonth;
        [FormerlySerializedAs("TMPYear")] public TextMeshProUGUI tmpYear;




        // Start is called before the first frame update
        void Start()
        {
            TimeSystem.OnHourChanged += ONHourChanged;
            TimeSystem.OnDayChanged += ONDayChanged;
            TimeSystem.OnMonthChanged += ONMonthChanged;
            TimeSystem.OnYearChanged += ONYearChanged;
        }

        private void ONYearChanged(int value)
        {
            if (tmpYear == null) return;
            tmpYear.SetText($"Year: {value}");
        }

        private void ONMonthChanged(int value)
        {
            if (tmpMonth == null) return;
            tmpMonth.SetText($"Month: {value}");
        }

        private void ONDayChanged(int value)
        {
            if (tmpDay == null) return;
            tmpDay.SetText($"Day: {value}");
        }

        private void ONHourChanged(int value)
        {
            if (tmpHour == null) return;
            tmpHour.SetText($"Hour: {value}");
        }

        // Update is called once per frame
        void Update()
        {
            if (tmpTime == null) return;
            tmpTime.SetText($"Time: {TimeSystem.Second}");
        }
    }

}
