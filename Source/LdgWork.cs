﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart;

public abstract class LdgWork<V> : WebWork where V : LdgVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainTable(HtmlBuilder h, IList<Ldg> arr)

    {
        h.TABLE_();
        DateTime last = default;
        foreach (var o in arr)
        {
            if (o.dt != last)
            {
                h.TR_().TD_("uk-padding-tiny-left uk-label", colspan: 4).T(o.dt, time: 0)._TD()._TR();
            }

            h.TR_();
            h.TD(o.name);
            h.TD(o.trans);
            h.TD(o.qty);
            h.TD(o.amt, true, true);
            h._TR();

            last = o.dt;
        }

        h._TABLE();
    }
}

[AdmlyAuthorize(User.ROL_FIN)]
[Ui("市场业务报表")]
public class AdmlyBuyLdgWork : LdgWork<AdmlyBuyLdgVarWork>
{
    [Ui("市场业务", status: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Ldg.Empty).T(" FROM buyldgs_typ ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
        var arr = dc.Query<Ldg>(p => p.Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE(arr, o =>
            {
                h.TD_().T(o.dt, 3, 0)._TD();
                h.TD(Buy.Typs[(short)o.acct]);
                h.TD_("uk-text-right").T(o.trans)._TD();
                h.TD_("uk-text-right").CNY(o.amt)._TD();
            }, thead: () =>
            {
                h.TH("日期", css: "uk-width-medium");
                h.TH("类型");
                h.TH("笔数");
                h.TH("金额");
            });
            h.PAGINATION(arr?.Length == 30);
        }, false, 60);
    }
}

[AdmlyAuthorize(User.ROL_FIN)]
[Ui("供应业务报表")]
public class AdmlyPurLdgWork : LdgWork<AdmlyPurLdgVarWork>
{
    [Ui("供应业务", status: 1), Tool(Anchor)]
    public void @default(WebContext wc, int page)
    {
        wc.GivePage(200, h => { h.TOOLBAR(); });
    }
}

[Ui("销售报表")]
public class RtllyBuyLdgWork : LdgWork<RtllyBuyLdgVarWork>
{
    [Ui("按商品", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Ldg.Empty).T(" FROM buyldgs_itemid WHERE orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
        var arr = await dc.QueryAsync<Ldg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无汇总记录");
                return;
            }

            MainTable(h, arr);

            h.PAGINATION(arr.Length == 30);
        }, false, 60);
    }

    [Ui("按业务类型", status: 2), Tool(Anchor)]
    public async Task typ(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Ldg.Empty).T(" FROM buyldgs_typ WHERE orgid = @1 ORDER BY dt DESC LIMIT 30 OFFSET 30 * @2");
        var arr = await dc.QueryAsync<Ldg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无汇总记录");
                return;
            }

            MainTable(h, arr);

            h.PAGINATION(arr.Length == 30);
        }, false, 60);
    }
}

[Ui("销售报表")]
public class SuplyPurLdgWork : LdgWork<SuplyPurLdgVarWork>
{
    [Ui("按产品批次", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT * FROM purldgs_lotid WHERE orgid = @1 ORDER BY dt DESC, acct LIMIT 30 OFFSET 30 * @2");
        var arr = await dc.QueryAsync<Ldg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无汇总记录");
                return;
            }

            MainTable(h, arr);

            h.PAGINATION(arr.Length == 30);
        }, false, 60);
    }

    [Ui("按类型", status: 2), Tool(Anchor)]
    public async Task typ(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT * FROM purldgs_typ WHERE orgid = @1 ORDER BY dt DESC, acct LIMIT 30 OFFSET 30 * @2");
        var arr = await dc.QueryAsync<Ldg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("暂无汇总记录");
                return;
            }

            h.TABLE(arr, o =>
            {
                h.TD_().T(o.dt, 3, 0)._TD();
                h.TD(Buy.Typs[(short)o.acct]);
                h.TD_("uk-text-right").T(o.trans)._TD();
                h.TD_("uk-text-right").CNY(o.amt)._TD();
            }, thead: () =>
            {
                h.TH("日期", css: "uk-width-medium");
                h.TH("类型");
                h.TH("笔数");
                h.TH("金额");
            });
            h.PAGINATION(arr.Length == 30);
        }, false, 60);
    }
}

[OrglyAuthorize(Org.TYP_CTR)]
[Ui("品控仓报表")]
public class CtrlyPurLdgWork : LdgWork<LdgVarWork>
{
    public void @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();
            h.ALERT("暂无生成报表");
        }, false, 60);
    }
}