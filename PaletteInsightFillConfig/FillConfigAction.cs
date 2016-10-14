using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.Windows.Forms;

namespace PaletteInsightFillConfig
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult FillConfig(Session session)
        {
            session.Log("Begin FillConfig");
            var InsightServerUrl = session["WIXUI_INSIGHTSERVERURL"];

            MessageBox.Show("Insight Server URL: ", InsightServerUrl);
            return ActionResult.Success;
        }
    }
}
