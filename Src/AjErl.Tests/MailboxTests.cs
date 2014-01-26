﻿namespace AjErl.Tests
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading;

    [TestClass]
    public class MailboxTests
    {
        [TestMethod]
        public void AddAndTakeMessage()
        {
            Mailbox box = new Mailbox();

            box.Add(1);

            Assert.AreEqual(1, box.Take());
        }

        [TestMethod]
        public void AddAndTakeTwoMessages()
        {
            Mailbox box = new Mailbox();

            box.Add(1);
            box.Add(2);

            Assert.AreEqual(1, box.Take());
            Assert.AreEqual(2, box.Take());
        }

        [TestMethod]
        public void TakeADelayedAddedMessage()
        {
            Mailbox box = new Mailbox();

            ThreadStart ts = new ThreadStart(() => { Thread.Sleep(100); box.Add(1); });
            Thread th = new Thread(ts);
            th.Start();

            Assert.AreEqual(1, box.Take());
        }
    }
}