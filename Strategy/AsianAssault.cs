﻿#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Indicator;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Strategy;
#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// Enter the description of your strategy here
    /// </summary>
    [Description("Enter the description of your strategy here")]
    public class AsianAssault : BaseForexStrategy
    {
        #region Variables


        private int _timeStartHour = 2;
        private int _timeStartMinute = 0;
        private int _timeStopHour = 4;
        private int _timeStopMinute = 59;


        private int _maxTicksToTarget = 10;

        private PriorDayOHLC _indiPrior;

        private IOrder _latestSubmittedOrder;
        #endregion

        protected override void MyInitialize()
        {
          //  TraceOrders = true;

            _exitType = ExitType.IntialToBreakevenToTrailing;

            _indiPrior = PriorDayOHLC();
            _indiPrior.ShowClose = false;
            _indiPrior.ShowOpen = false;
            Add(_indiPrior);

      //      SetTrailStop(CalculationMode.Ticks, 15);
            //SetStopLoss(CalculationMode.Ticks, 20);
            //SetProfitTarget(CalculationMode.Ticks, 30);

        }

        protected override void SetupIndicatorProperties()
        {

        }

 
        protected override void MyOnBarUpdate()
        {
    
            if (_latestSubmittedOrder != null)
            {
                if (Time[0].Hour > _timeStopHour)
                {
                    CancelOrder(_latestSubmittedOrder);
                    _latestSubmittedOrder = null;
                }
            }
             
        }


        protected override void OnOrderUpdate(IOrder order)
        {
            if (_latestSubmittedOrder != null && _latestSubmittedOrder == order)
            {
                Print(order.ToString());
                if (order.OrderState == OrderState.Filled)
                    _latestSubmittedOrder = null;
            }
        }


        private List<string> _orderDates = new List<string>();
       

        protected override void LookForTrade()
        {

            if (!EntryOk()) return;
            if (_latestSubmittedOrder != null) return;

            double priorHigh = _indiPrior.PriorHigh[0];
            double priorLow = _indiPrior.PriorLow[0];

            if (priorLow == 0 || priorHigh == 0) return;


            // go long?
            if (Close[0] > priorLow && Close[0] <= priorLow + (TickSize * _maxTicksToTarget) && Falling(Close))
            {
                
                 _latestSubmittedOrder = EnterLongLimit(0, true, DefaultQuantity, priorLow, "long");
                _tradeState = TradeState.InitialStop;
           //      SetStopLoss("", CalculationMode.Price, priorLow - TickSize * _mmInitialSL, true);
                 _lossLevel = priorLow - TickSize*_mmInitialSL;
                _orderDates.Add(Time[0].ToShortDateString());
                
            }

            // go short?
            if (Close[0] < priorHigh && Close[0] >= priorHigh - (TickSize * _maxTicksToTarget) && Rising(Close))
            {

                
                _latestSubmittedOrder = EnterShortLimit(0, true, DefaultQuantity, priorHigh, "short");
                _tradeState = TradeState.InitialStop;
           //     SetStopLoss("", CalculationMode.Price, priorHigh + TickSize * _mmInitialSL, true);
                _lossLevel = priorHigh + TickSize*_mmInitialSL;
                _orderDates.Add(Time[0].ToShortDateString());
            
            }




        }



        protected override void MyManagePosition()
        {

        }

        private bool EntryOk()
        {
            if (Time[0].Day == 25  && Time[0].Hour == 1 && Time[0].Minute == 30)
            {
                var s = "";
            }

            if ((Time[0].Hour < TimeStartHour) )
            {
                return false;
            }

            if ((Time[0].Hour >= TimeStopHour))
            {
                return false;
            }
            if (_orderDates.Contains(Time[0].ToShortDateString()))
            {
                return false;

            }


            //if (_orderDates.Count >0)
            //{
            //    return false;

            //}



            return true;
        }

        #region parameters

        [Description("")]
        [GridCategory("Parameters")]
        public int TimeStartHour
        {
            get { return _timeStartHour; }
            set { _timeStartHour = Math.Max(1, value); }
        }
        [Description("")]
        [GridCategory("Parameters")]
        public int TimeStopHour
        {
            get { return _timeStopHour; }
            set { _timeStopHour = Math.Max(1, value); }
        }
        #endregion
    }
}

