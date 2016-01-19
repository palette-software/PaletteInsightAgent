using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalMon.Output
{


    public class DelayedAction
    {
        private DateTimeOffset lastOccurance;
        private DateTimeOffset nextOccurance;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The recommended intervals between triggers. The actual interval depends
        /// on when Tick() gets called
        /// </summary>
        private TimeSpan WaitUntil;

        private Action<DateTimeOffset, DateTimeOffset> WhatToDo;

        public DelayedAction(TimeSpan intervals, Action<DateTimeOffset,DateTimeOffset> callback)
        {
            lastOccurance = DateTimeOffset.MinValue;
            nextOccurance = DateTimeOffset.MinValue;

            WaitUntil = intervals;
            WhatToDo = callback;
        }


        /// <summary>
        /// Call this any time this action may be triggered
        /// </summary>
        public void Tick()
        {
            TriggerIfNeeded();
        }


        #region private
        private bool TriggerIfNeeded()
        {
            var needsTrigger = DateTimeOffset.UtcNow >= nextOccurance;
            if (!needsTrigger) return false;

            DoTrigger();
            return true;
        }

        private void DoTrigger()
        {
            if (lastOccurance != DateTimeOffset.MinValue)
            {
                try
                {
                    WhatToDo(lastOccurance, nextOccurance);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error during delayed action run. {0}", e.ToString());
                }
            }

            lastOccurance = DateTimeOffset.UtcNow;
            nextOccurance = lastOccurance.Add(WaitUntil);
        }
        #endregion
    }
}
