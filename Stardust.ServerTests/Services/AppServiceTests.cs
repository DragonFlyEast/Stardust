﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stardust.Data;
using Stardust.Server.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stardust.Server.Services.Tests
{
    [TestClass()]
    public class AppServiceTests
    {
        [TestMethod()]
        public void AuthorizeTest()
        {
            var app = App.FindByName("test");
            if (app != null) app.Delete();

            var service = new AppService();

            // 没有自动注册
            var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() => service.Authorize("test", "xxx", false));
            Assert.IsNotNull(ex);

            // 启用
            app = App.FindByName("test");
            app.Enable = true;
            app.Update();

            // 自动注册
            var rs = service.Authorize("test", "xxx", true);
            Assert.IsNotNull(rs);

            Assert.IsNotNull(app);
            Assert.AreEqual(app.ID, rs.ID);

            // 再次验证
            var rs2 = service.Authorize("test", "xxx", false);
            Assert.IsNotNull(rs2);
            Assert.AreEqual(app.ID, rs.ID);

            // 错误验证
            Assert.ThrowsException<InvalidOperationException>(() => service.Authorize("test", "yyy", true));
        }

        [TestMethod()]
        public void IssueTokenTest()
        {
            var app = new App { Name = "test" };

            var set = Setting.Current;
            var service = new AppService();

            var model = service.IssueToken(app, set);
            Assert.IsNotNull(model);

            Assert.AreEqual(3, model.AccessToken.Split('.').Length);
            Assert.AreEqual(3, model.RefreshToken.Split('.').Length);
            Assert.AreEqual(set.TokenExpire, model.ExpireIn);
            Assert.AreEqual("JWT", model.TokenType);
        }

        [TestMethod()]
        public void DecodeTokenTest()
        {
            var app = App.FindByName("test");
            if (app == null)
            {
                app = new App { Name = "test", Enable = true };
                app.Insert();
            }

            var set = Setting.Current;
            var service = new AppService();

            var model = service.IssueToken(app, set);
            Assert.IsNotNull(model);

            // 马上解码
            var app2 = service.DecodeToken(model.AccessToken, set);
            Assert.IsNotNull(app2);

            app2 = service.DecodeToken(model.RefreshToken, set);
            Assert.IsNotNull(app2);
        }
    }
}