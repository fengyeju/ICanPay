﻿using ICanPay.Core;
using Microsoft.AspNetCore.Mvc;

namespace ICanPay.Demo.Controllers
{
    public class QueryController : Controller
    {
        private readonly IGateways gateways;

        public QueryController(IGateways gateways)
        {
            this.gateways = gateways;
        }

        public IActionResult Index(string id)
        {
            var notify = (Alipay.Notify)QueryAlipayOrder(id);

            return Json(notify);
        }

        /// <summary>
        /// 查询支付宝的订单
        /// </summary>
        private Alipay.Notify QueryAlipayOrder(string id)
        {
            var gateway = gateways.Get<Alipay.AlipayGateway>();

            return (Alipay.Notify)gateway.Query(new Alipay.Auxiliary
            {
                OutTradeNo = id
            });
        }

        /// <summary>
        /// 查询微信的订单
        /// </summary>
        private Wechatpay.Notify QueryWechatpayOrder(string id)
        {
            var gateway = gateways.Get<Wechatpay.WechatpayGataway>();

            return (Wechatpay.Notify)gateway.Query(new Wechatpay.Auxiliary
            {
                OutTradeNo = id
            });
        }
    }
}
