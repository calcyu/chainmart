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
[Help("按照几个阶段，对所有的网售订单进行管理")]
public class RtllyBuyWork : BuyWork<RtllyBuyVarWork>
{
    static void MainGrid(HtmlBuilder h, IList<Buy> lst, bool pick = false)
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
            h.HEADER_().H4(o.uname).SPAN_("uk-badge").T(o.created, time: 0).SP();
            if (pick)
            {
                h.PICK(o.Key);
            }
            else
            {
                h.T(Buy.Statuses[o.status]);
            }
            h._SPAN()._HEADER();
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


    static readonly string[] ExcludeActions = { nameof(adapted), nameof(adapt) };

    [OrgSpy(BUY_CREATED)]
    [Ui("网售订单", "新收的网售订单", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND status = 1 AND typ = 1 ORDER BY created DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id, exclude: org.IsService ? ExcludeActions : null);

            if (arr == null)
            {
                h.ALERT("尚无新网售订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui(tip: "已集合", icon: "chevron-double-right", status: 2), Tool(Anchor)]
    public async Task adapted(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND status = 2 AND typ = 1 ORDER BY adapted DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id, exclude: org.IsService ? ExcludeActions : null);

            if (arr == null)
            {
                h.ALERT("尚无已集合的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [OrgSpy(BUY_OKED)]
    [Ui(tip: "已派发", icon: "arrow-right", status: 4), Tool(Anchor)]
    public async Task after(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND status >= 4 AND typ = 1 ORDER BY oked DESC LIMIT 20 OFFSET 20 * @2");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id, exclude: org.IsService ? ExcludeActions : null);

            if (arr == null)
            {
                h.ALERT("尚无已派发的订单");
                return;
            }

            MainGrid(h, arr);

            h.PAGINATION(arr.Length == 20);
        }, false, 6);
    }


    [Ui(tip: "已撤销", icon: "trash", status: 8), Tool(Anchor)]
    public async Task @void(WebContext wc)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE rtlid = @1 AND status = 0 AND typ = 1 ORDER BY id DESC");
        var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR(twin: org.id);
            if (arr == null)
            {
                h.ALERT("尚无已撤销的订单");
                return;
            }

            MainGrid(h, arr);
        }, false, 6);
    }

    [Ui("集合", icon: "chevron-double-right", status: 1), Tool(ButtonPickShow)]
    public async Task adapt(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;
        int[] key;
        bool print = false;

        if (wc.IsGet)
        {
            key = wc.Query[nameof(key)];

            wc.GivePane(200, h =>
            {
                h.SECTION_("uk-card uk-card-primary");
                h.H2("送存到集合区", css: "uk-card-header");
                h.DIV_("uk-card-body").T("将备好的货贴上派送标签或小票，送存到集合区，等待统一派送")._DIV();
                h._SECTION();

                h.FORM_("uk-card uk-card-primary uk-margin-top");
                foreach (var k in key)
                {
                    h.HIDDEN(nameof(key), k);
                }
                h.DIV_("uk-card-body").CHECKBOX(null, nameof(print), true, tip: "使用共享机打印小票", disabled: !print)._DIV();
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
[Ui("网售统一派发")]
public class MktlyBuyWork : BuyWork<MktlyBuyVarWork>
{
    internal void MainGrid(HtmlBuilder h, IList<Buy> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.UL_("uk-card-body uk-list uk-list-divider");
            h.LI_().H4(o.utel).SPAN_("uk-badge").T(o.created, time: 0).SP().T(Buy.Statuses[o.status])._SPAN()._LI();

            foreach (var it in o.items)
            {
                h.LI_();

                h.SPAN_("uk-width-expand").T(it.name);
                if (it.unitw > 0)
                {
                    h.SP().SMALL_().T(it.unitw).T(it.unit)._SMALL();
                }
                h._SPAN();

                h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.RealPrice).SP().SUB(it.unit)._SPAN();
                h.SPAN_("uk-width-tiny uk-flex-right").T(it.qty).SP().T(it.unit)._SPAN();
                h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.SubTotal)._SPAN();
                h._LI();
            }
            h._LI();

            h._UL();
        });
    }

    [Ui("网售统一派发", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ucom, count(CASE WHEN status = 1 THEN 1 END), count(CASE WHEN status = 2 THEN 2 END) FROM buys WHERE mktid = @1 AND typ = 1 AND (status = 1 OR status = 2) GROUP BY ucom");
        await dc.QueryAsync(p => p.Set(mkt.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("社区").TH("收单", css: "uk-width-tiny").TH("集合", css: "uk-width-tiny")._THEAD();

            while (dc.Next())
            {
                dc.Let(out string ucom);
                dc.Let(out int created);
                dc.Let(out int adapted);

                string ucomlabel = string.IsNullOrEmpty(ucom) ? "非派送区" : ucom;

                h.TR_();
                h.TD_().ADIALOG_(string.IsNullOrEmpty(ucom) ? "_" : ucom, "/com", mode: ToolAttribute.MOD_OPEN, false, tip: ucomlabel, css: "uk-link uk-button-link").T(ucomlabel)._A()._TD();
                h.TD_(css: "uk-text-center");
                if (created > 0)
                {
                    h.T(created);
                }
                h._TD();
                h.TD_(css: "uk-text-center");
                if (adapted > 0)
                {
                    h.T(adapted);
                }
                h._TD();
                h._TR();
            }

            h._TABLE();
        });
    }

    [Ui(tip: "按商户", icon: "grid", status: 2), Tool(Anchor)]
    public async Task orgs(WebContext wc)
    {
        var mkt = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql($"SELECT rtlid, first(name), first(tip), count(CASE WHEN status = 1 THEN 1 END), count(CASE WHEN status = 2 THEN 2 END) FROM buys WHERE mktid = @1 AND typ = 1 AND (status = 1 OR status = 2) GROUP BY rtlid");
        await dc.QueryAsync(p => p.Set(mkt.id));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            h.TABLE_();
            h.THEAD_().TH("商户").TH("收单", css: "uk-width-tiny").TH("集合", css: "uk-width-tiny")._THEAD();
            while (dc.Next())
            {
                dc.Let(out int rtlid);
                dc.Let(out string name);
                dc.Let(out string tip);
                dc.Let(out int created);
                dc.Let(out int adapted);

                h.TR_();
                h.TD_().T(name);
                if (!string.IsNullOrEmpty(tip))
                {
                    h.T('（').T(tip).T('）');
                }
                h._TD();
                h.TD(created, right: null);
                h.TD(adapted, right: null);
                h._TR();
            }
            h._TABLE();
        }, false, 6);
    }

    [Ui(tip: "已统一派送", icon: "arrow-right", status: 4), Tool(AnchorPrompt)]
    public async Task oked(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        string com;

        bool inner = wc.Query[nameof(inner)];
        if (inner)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_(css: "uk-card uk-card-primary uk-card-body");

                var specs = org?.specs;
                for (int i = 0; i < specs?.Count; i++)
                {
                    var spec = specs.EntryAt(i);
                    var v = spec.Value;
                    if (v.IsObject)
                    {
                        h.FIELDSUL_(spec.Key, css: "uk-list uk-list-divider");

                        var sub = (JObj)v;
                        for (int k = 0; k < sub.Count; k++)
                        {
                            var e = sub.EntryAt(k);

                            h.LI_().RADIO(nameof(com), e.Key, e.Key)._LI();
                        }

                        h._FIELDSUL();
                    }
                }
                h._FORM();
            });
        }
        else // OUTER
        {
            com = wc.Query[nameof(com)];

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE mktid = @1 AND status = 4 AND (typ = 1 AND adapter IS NOT NULL) AND ucom = @2 ORDER BY oked DESC LIMIT 20 OFFSET 20 * @3");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id).Set(com).Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();

                if (arr == null)
                {
                    h.ALERT("没有找到记录");
                    return;
                }

                h.MAINGRID(arr, o =>
                {
                    h.UL_("uk-card-body uk-list uk-list-divider");
                    h.LI_().H4(o.name).SPAN_("uk-badge").T(o.oked, date: 2, time: 2).SP().T(Buy.Statuses[o.status])._SPAN()._LI();

                    foreach (var it in o.items)
                    {
                        h.LI_();

                        h.SPAN_("uk-width-expand").T(it.name);
                        if (it.unitw > 0)
                        {
                            h.SP().SMALL_().T(Unit.Weights[it.unitw])._SMALL();
                        }

                        h._SPAN();

                        h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.RealPrice)._SPAN();
                        h.SPAN_("uk-width-tiny uk-flex-right").T(it.qty).SP().T(it.unit)._SPAN();
                        h.SPAN_("uk-width-1-5 uk-flex-right").CNY(it.SubTotal)._SPAN();
                        h._LI();
                    }
                    h._LI();

                    h.LI_();
                    h.SPAN_("uk-width-expand").SMALL_().T(o.ucom).T(o.uaddr)._SMALL()._SPAN();
                    if (o.fee > 0)
                    {
                        h.SMALL_().T("派送到楼下 +").T(o.fee)._SMALL();
                    }
                    h.SPAN_("uk-width-1-5 uk-flex-right").CNY(o.topay)._SPAN();
                    h._LI();

                    h._UL();
                });

                h.PAGINATION(arr.Length == 20);
            }, false, 6);
        }
    }
}