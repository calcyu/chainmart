﻿using System.Threading.Tasks;
using SkyChain.Web;

namespace Revital
{
    public class OrglyReportWork : WebWork
    {
    }

    [UserAuthorize(Org.TYP_MRT, 1)]
    [Ui("市场运营报告")]
    public class MrtlyReportWork : WebWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }

    [Ui("产源运营报告")]
    public class SrclyReportWork : WebWork
    {
        public async Task @default(WebContext wc)
        {
        }
    }
}