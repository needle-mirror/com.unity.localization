using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Tests
{
    public class CallbackArrayTests
    {
        delegate void MyCallback(int val);

        CallbackArray<MyCallback> m_CallbackArray;

        [SetUp]
        public void Setup()
        {
            m_CallbackArray = new CallbackArray<MyCallback>();
        }

        int m_Callback1Value;
        int m_Callback2Value;
        int m_Callback3Value;
        int m_CallCount;

        void Callback1(int i)
        {
            m_Callback1Value = i;
            m_CallCount++;
        }

        void Callback2(int i)
        {
            m_Callback2Value = i;
            m_CallCount++;
        }

        void Callback3(int i)
        {
            m_Callback3Value = i;
            m_CallCount++;
        }

        [Test]
        public void AddValidCallback_UpdatesLengthAndDelegates()
        {
            var cb1 = new MyCallback(Callback1);
            var cb2 = new MyCallback(Callback2);
            var cb3 = new MyCallback(Callback3);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(0));

            m_CallbackArray.Add(cb1);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(1));
            Assert.AreEqual(1, m_CallbackArray.Length);
            Assert.AreEqual(cb1, m_CallbackArray.SingleDelegate);

            m_CallbackArray.Add(cb2);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(2));
            Assert.Contains(cb1, m_CallbackArray.MultiDelegates);
            Assert.Contains(cb2, m_CallbackArray.MultiDelegates);

            m_CallbackArray.Add(Callback3);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(3));
            Assert.Contains(cb1, m_CallbackArray.MultiDelegates);
            Assert.Contains(cb2, m_CallbackArray.MultiDelegates);
            Assert.Contains(cb3, m_CallbackArray.MultiDelegates);
            Assert.Null(m_CallbackArray.SingleDelegate);
        }

        [Test]
        public void Add_WhileInvoking_UpdatesLengthAndDelegates()
        {
            m_CallbackArray.Add(Callback1);
            m_CallbackArray.Add(Callback2);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(2));

            m_CallbackArray.LockForChanges();
            m_CallbackArray.Add(Callback3);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(2), "Expected length to not change while locked for changes.");
            m_CallbackArray.UnlockForChanges();
            Assert.That(m_CallbackArray, Has.Length.EqualTo(3));
        }

        [Test]
        public void Add_NUll_IsIgnored()
        {
            m_CallbackArray.Add(null);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(0));
        }

        [Test]
        public void Add_UsesCustomCapacityIncrement()
        {
            const int capacityIncrment = 12;

            Assert.Null(m_CallbackArray.MultiDelegates);

            m_CallbackArray.Add(Callback1, capacityIncrment);
            m_CallbackArray.Add(Callback2, capacityIncrment);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(2));
            Assert.That(m_CallbackArray.MultiDelegates, Has.Length.EqualTo(capacityIncrment));
        }

        [Test]
        public void RemoveValidCallback_UpdatesLength()
        {
            var cb1 = new MyCallback(Callback1);
            var cb2 = new MyCallback(Callback2);
            var cb3 = new MyCallback(Callback3);

            m_CallbackArray.Add(cb1);
            m_CallbackArray.Add(cb2);
            m_CallbackArray.Add(cb3);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(3));
            Assert.Contains(cb1, m_CallbackArray.MultiDelegates);
            Assert.Contains(cb2, m_CallbackArray.MultiDelegates);
            Assert.Contains(cb3, m_CallbackArray.MultiDelegates);
            Assert.Null(m_CallbackArray.SingleDelegate);

            m_CallbackArray.RemoveByMovingTail(cb2);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(2));
            Assert.Contains(cb1, m_CallbackArray.MultiDelegates);
            Assert.Contains(cb3, m_CallbackArray.MultiDelegates);
            Assert.Null(m_CallbackArray.SingleDelegate);

            m_CallbackArray.RemoveByMovingTail(cb1);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(1));
            Assert.AreEqual(cb3, m_CallbackArray.SingleDelegate);

            m_CallbackArray.RemoveByMovingTail(cb3);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(0));
        }

        [Test]
        public void Remove_WhileInvoking_UpdatesLengthAfterInvokeIsComplete()
        {
            m_CallbackArray.Add(Callback1);
            m_CallbackArray.Add(Callback2);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(2));

            m_CallbackArray.LockForChanges();
            m_CallbackArray.RemoveByMovingTail(Callback2);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(2), "Expected length to not change while locked for changes.");
            m_CallbackArray.UnlockForChanges();
            Assert.That(m_CallbackArray, Has.Length.EqualTo(1));
        }

        [Test]
        public void Remove_WithDifferentCallbackInstance_RemovesMatchingCallback()
        {
            var cb1 = new MyCallback(Callback1);
            var cb2 = new MyCallback(Callback1);

            m_CallbackArray.Add(cb1);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(1));

            m_CallbackArray.RemoveByMovingTail(cb2);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(0));
        }

        [Test]
        public void Remove_NUll_IsIgnored()
        {
            m_CallbackArray.Add(Callback1);
            m_CallbackArray.RemoveByMovingTail(null);
            Assert.That(m_CallbackArray, Has.Length.EqualTo(1));
        }
    }
}
