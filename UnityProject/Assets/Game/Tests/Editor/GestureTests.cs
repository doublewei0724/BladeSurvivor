using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace BladeSurvivor.Tests {
public class GestureTests {
    GestureResolver resolver;List<GestureKind> events;
    [SetUp] public void Setup(){resolver=new GestureResolver();events=new List<GestureKind>();resolver.OnGesture=(kind,a,b)=>events.Add(kind);}
    [Test] public void SingleTapWaitsForDoubleTapWindow(){resolver.Begin(Vector2.zero,0,false);resolver.End(Vector2.zero,.05f);resolver.Tick(Vector2.zero,.2f);Assert.IsEmpty(events);resolver.Tick(Vector2.zero,.3f);CollectionAssert.AreEqual(new[]{GestureKind.Tap},events);}
    [Test] public void DoubleTapProducesOnlyDodge(){resolver.Begin(Vector2.zero,0,false);resolver.End(Vector2.zero,.03f);resolver.Begin(Vector2.one,.12f,false);resolver.End(Vector2.one,.15f);resolver.Tick(Vector2.one,.5f);CollectionAssert.AreEqual(new[]{GestureKind.Dodge},events);}
    [Test] public void SwipeDoesNotAlsoTap(){resolver.Begin(Vector2.zero,0,false);resolver.End(new Vector2(100,0),.2f);resolver.Tick(Vector2.zero,1);CollectionAssert.AreEqual(new[]{GestureKind.Swipe},events);}
    [Test] public void HoldRequiresReleaseAndDoesNotAutoFire(){resolver.Begin(Vector2.zero,0,false);resolver.Tick(Vector2.zero,.5f);resolver.Tick(Vector2.zero,4);Assert.AreEqual(1,events.FindAll(e=>e==GestureKind.ChargeStart).Count);Assert.IsFalse(events.Contains(GestureKind.ChargeRelease));resolver.End(Vector2.zero,4.1f);Assert.AreEqual(GestureKind.ChargeRelease,events[events.Count-1]);}
    [Test] public void HoldingWhileDraggingAimsInsteadOfSwiping(){resolver.Begin(Vector2.zero,0,false);resolver.Tick(new Vector2(120,0),.6f);resolver.Tick(new Vector2(0,150),1);resolver.End(new Vector2(-150,0),1.1f);Assert.Contains(GestureKind.ChargeStart,events);Assert.Contains(GestureKind.ChargeAim,events);Assert.Contains(GestureKind.ChargeRelease,events);Assert.IsFalse(events.Contains(GestureKind.Swipe));}
    [Test] public void ForwardRectangleChecksLengthWidthAndOrientation(){Assert.IsTrue(HitDetector.InForwardBox(Vector3.zero,Vector3.right,new Vector3(4.9f,0,1.2f),0,5,2.5f));Assert.IsFalse(HitDetector.InForwardBox(Vector3.zero,Vector3.right,new Vector3(5.1f,0,0),0,5,2.5f));Assert.IsFalse(HitDetector.InForwardBox(Vector3.zero,Vector3.right,new Vector3(2,0,1.3f),0,5,2.5f));Assert.IsFalse(HitDetector.InForwardBox(Vector3.zero,Vector3.right,new Vector3(-1,0,0),0,5,2.5f));}
    [Test] public void UIPressNeverCreatesGameplayGesture(){resolver.Begin(Vector2.zero,0,true);resolver.Tick(Vector2.zero,1);resolver.End(new Vector2(100,0),1.1f);resolver.Tick(Vector2.zero,2);Assert.IsEmpty(events);}
    [Test] public void CancelClearsPendingTap(){resolver.Begin(Vector2.zero,0,false);resolver.End(Vector2.zero,.05f);resolver.Cancel();resolver.Tick(Vector2.zero,1);CollectionAssert.AreEqual(new[]{GestureKind.Cancel},events);}
}
}
