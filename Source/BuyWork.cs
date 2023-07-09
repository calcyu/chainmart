﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;
using static ChainSmart.OrgNoticePack;

namespace ChainSmart;

public abstract class BuyWork<V> : WebWork where V : BuyVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }
}

[Ui("网售订单")]
public class RtllyBuyWork : BuyWork<RtllyBuyVarWork>
{
    static void MainGrid(HtmlBuilder h, IList<Buy> lst)
    {
        h.MAINGRID(lst, o =>
        {
            h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, tip: o.uname, css: "uk-card-body uk-flex");

            // the first detail
            var items = o.items;

            if (items == null || items.Length == 0)
            {
                h.PIC("/void.webp", css: "uk-width-1-5");
            }
            else if (items.Length == 1)
            {
                var bi = items[0]; // buyitem
                if (bi.lotid > 0)
                {
                    h.PIC(MainApp.WwwUrl, "/lot/", bi.lotid, "/icon", css: "uk-width-1-5");
                }
                else
                    h.PIC(MainApp.WwwUrl, "/item/", bi.itemid, "/icon", css: "uk-width-1-5");
            }
            else
            {
                h.PIC("/solid.webp", css: "uk-width-1-5");
            }

            h.ASIDE_();
            h.HEADER_().H4(o.uname).SPAN_("uk-badge").T(o.created, time: 0)._SPAN().SP().PICK(o.Key)._HEADER();
            h.Q_("uk-width-expand");
            for (int i = 0; i < o.items?.Length; i++)
            {
                var it = o.items[i];
                if (i > 0) h.T('；');
                h.T(it.name).SP().T(it.qty).T(it.unit);
            }
            h._Q();
            h.FOOTER_().SPAN(string.IsNullOrEmpty(o.ucom) ? "非派送区" : o.ucom, "uk-width-expand").SPAN(o.utel, "uk-width-1-3 uk-output").SPAN_("uk-width-1-3 uk-flex-right").CNY(o.pay)._SPAN()._FOOTER();
            h._ASIDE();

            h._A();
        });
    }


    [OrgSpy(BUY_CREATED)]
    [Ui("网售订单", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND typ = 1 AND status = 1 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);
            if (arr == null)
            {
                h.ALERT("尚无网售订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已集合待派发", icon: "chevron-double-right", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND typ = 1 AND status = 2 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);
            if (arr == null)
            {
                h.ALERT("尚无已集合待派发的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [OrgSpy(BUY_OKED)]
    [Ui(tip: "已派发", icon: "arrow-right", status: 4), Tool(Anchor)]
    public async Task oked(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND typ = 1 AND status = 4 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);
            if (arr == null)
            {
                h.ALERT("尚无已派发订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已撤销", icon: "trash", status: 0), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND typ = 1 AND status = 0 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);
            if (arr == null)
            {
                h.ALERT("尚无已撤销订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui("集合", icon: "chevron-double-right", status: 1), Tool(ButtonPickOpen)]
    public async Task adapt(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        int[] key;
        bool print;

        if (wc.IsGet)
        {
            key = wc.Query[nameof(key)];

            wc.GivePane(200, h =>
            {
                h.FORM_();
                h.ALERT("备好的货集合到统一派送区");
                foreach (var k in key) h.HIDDEN(nameof(key), k);
                h.CHECKBOX(null, nameof(print), true, tip: "是否打印");
                h.BOTTOM_BUTTON("确认", nameof(adapt), post: true);
                h._FORM();
            });
        }
        else
        {
            var f = await wc.ReadAsync<Form>();
            key = f[nameof(key)];
            print = f[nameof(print)];

            using var dc = NewDbContext();
            dc.Sql("UPDATE buys SET adapted = @1, adapter = @2, status = 2 WHERE rtlid = @3 AND id ")._IN_(key).T(" AND status = 1");
            await dc.ExecuteAsync(p =>
            {
                p.Set(DateTime.Now).Set(prin.name).Set(org.id);
                p.SetForIn(key);
            });

            wc.GivePane(200);
        }
    }
}

[OrglyAuthorize(Org.TYP_MKT)]
[Ui("网售统一发货")]
public class MktlyBuyWork : BuyWork<MktlyBuyVarWork>
{
    [Ui("待发货订单", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        // group by commuity 
        dc.Sql($"SELECT ucom, count(id) FROM buys WHERE mktid = @1 AND typ = {Buy.TYP_PLAT} AND status = {Entity.STU_ADAPTED} GROUP BY ucom");
        await dc.QueryAsync(p => p.Set(mkt.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            while (dc.Next())
            {
                dc.Let(out string ucom);
                dc.Let(out int count);

                if (string.IsNullOrEmpty(ucom)) ucom = "非派送区";

                h.TR_();
                h.TD(ucom);
                h.TD(count);
                h._TR();
            }
            h._TABLE();
        });
    }

    [Ui(tip: "个人发货任务", icon: "user", status: 2), Tool(Anchor)]
    public async Task wait(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ucom, count(id) FROM buys WHERE mktid = @1 AND typ = 1 AND status BETWEEN 1 AND 2 GROUP BY ucom");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(mkt.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null) return;

            h.MAIN_(grid: true);

            int last = 0; // uid

            foreach (var o in arr)
            {
                if (o.uid != last)
                {
                    h.FORM_("uk-card uk-card-default");
                    h.HEADER_("uk-card-header").T(o.uname).SP().T(o.utel).SP().T(o.uaddr)._HEADER();
                    h.UL_("uk-card-body");
                    // h.TR_().TD_("uk-label uk-padding-tiny-left", colspan: 6).T(spr.name)._TD()._TR();
                }

                h.LI_().T(o.name)._LI();

                last = o.uid;
            }

            h._UL();
            h._FORM();

            h._MAIN();
        });
    }

    [Ui(tip: "社区查询", icon: "search"), Tool(AnchorPrompt)]
    public async Task search(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        bool inner = wc.Query[nameof(inner)];
        string com;
        if (inner)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("选择社区");
                h.LI_().SELECT_SPEC(nameof(com), org.specs)._LI();
                h._FIELDSUL()._FORM();
            });
        }
        else // OUTER
        {
            com = wc.Query[nameof(com)];

            using var dc = NewDbContext();
            dc.Sql("SELECT ucom, oked, first(oker), count(id) FROM buys WHERE mktid = @1 AND typ = 1 AND status = 4 AND ucom = @2 GROUP BY oked DESC");

            var arr = await dc.QueryAsync<User>(p => p.Set(org.id).Set(com));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", ToolAttribute.MOD_OPEN, false, css: "uk-card-body uk-flex");

                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/user/").T(o.id).T("/icon")._PIC();
                    }
                    else
                        h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();
                    h.HEADER_().H4(o.name).SPAN("")._HEADER();
                    h.Q(o.tel, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left")._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            }, false, 30);
        }
    }
}