﻿﻿using System.Threading.Tasks;
using CoChain.Web;

namespace Revital
{
    public abstract class ClearVarWork : WebWork
    {
    }

    public class AdmlySupplyClearVarWork : ClearVarWork
    {
    }

    public class AdmlyBuyClearVarWork : ClearVarWork
    {
    }

    public class OrglyClearVarWork : ClearVarWork
    {
        [Ui("￥", "微信领款"), Tool(Modal.ButtonShow)]
        public async Task rcv(WebContext wc, int dt)
        {
            int orderid = wc[0];
            if (wc.IsGet)
            {
            }
            else // POST
            {
                wc.GivePane(200); // close
            }
        }
    }
}