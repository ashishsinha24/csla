//-----------------------------------------------------------------------
// <copyright file="RollBackTests.cs" company="Marimer LLC">
//     Copyright (c) Marimer LLC. All rights reserved.
//     Website: https://cslanet.com
// </copyright>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using Csla.TestHelpers;

#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
#endif

namespace Csla.Test.RollBack
{
  [TestClass]
  public class RollBackTests
  {
    private static TestDIContext _testDIContext;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
      _testDIContext = TestDIContextFactory.CreateDefaultContext();
    }

    [TestMethod]
    [TestCategory("SkipWhenLiveUnitTesting")]
    public void NoFail()
    {
      IDataPortal<RollbackRoot> dataPortal = _testDIContext.CreateDataPortal<RollbackRoot>();

      TestResults.Reinitialise();
      RollbackRoot root = Csla.Test.RollBack.RollbackRoot.NewRoot(dataPortal);

      root.BeginEdit();
      root.Data = "saved";
      Assert.AreEqual("saved", root.Data, "data is 'saved'");
      Assert.AreEqual(false, root.Fail, "fail is false");
      Assert.AreEqual(true, root.IsDirty, "isdirty is true");
      Assert.AreEqual(true, root.IsValid, "isvalid is true");
      Assert.AreEqual(true, root.IsNew, "isnew is true");

      TestResults.Reinitialise();
      RollbackRoot tmp = (RollbackRoot)(root.Clone());
      root.ApplyEdit();
      root = root.Save();

      Assert.IsNotNull(root, "obj is not null");
      Assert.AreEqual("Inserted", TestResults.GetResult("Root"), "obj was inserted");
      Assert.AreEqual("saved", root.Data, "data is 'saved'");
      Assert.AreEqual(false, root.IsNew, "is new is false");
      Assert.AreEqual(false, root.IsDeleted, "isdeleted is false");
      Assert.AreEqual(false, root.IsDirty, "isdirty is false");
    }

    [TestMethod]
    [TestCategory("SkipWhenLiveUnitTesting")]
    public void YesFail()
    {
      IDataPortal<RollbackRoot> dataPortal = _testDIContext.CreateDataPortal<RollbackRoot>();

      TestResults.Reinitialise();
      RollbackRoot root = Csla.Test.RollBack.RollbackRoot.NewRoot(dataPortal);

      root.BeginEdit();
      root.Data = "saved";
      root.Fail = true;
      Assert.AreEqual("saved", root.Data, "data is 'saved'");
      Assert.AreEqual(true, root.Fail, "fail is true");
      Assert.AreEqual(true, root.IsDirty, "isdirty is true");
      Assert.AreEqual(true, root.IsValid, "isvalid is true");
      Assert.AreEqual(true, root.IsNew, "isnew is true");

      TestResults.Reinitialise();
      RollbackRoot tmp = (RollbackRoot)(root.Clone());
      try
      {
        root.ApplyEdit();
        root = (RollbackRoot)(root.Save());
        Assert.Fail("exception didn't occur");
      }
      catch
      {
        root = tmp;
      }

      Assert.IsNotNull(root, "obj is not null");
      Assert.AreEqual("Inserted", TestResults.GetResult("Root"), "obj was inserted");
      Assert.AreEqual("saved", root.Data, "data is 'saved'");
      Assert.AreEqual(true, root.IsNew, "isnew is true");
      Assert.AreEqual(false, root.IsDeleted, "isdeleted is false");
      Assert.AreEqual(true, root.IsDirty, "isdirty is true");
    }

    [TestMethod]
    [TestCategory("SkipWhenLiveUnitTesting")]
    public void YesFailCancel()
    {
      IDataPortal<RollbackRoot> dataPortal = _testDIContext.CreateDataPortal<RollbackRoot>();

      TestResults.Reinitialise();
      RollbackRoot root = Csla.Test.RollBack.RollbackRoot.NewRoot(dataPortal);
      Assert.AreEqual(true, root.IsDirty, "isdirty is true");
      Assert.AreEqual("<new>", root.Data, "data is '<new>'");

      root.BeginEdit();
      root.Data = "saved";
      root.Fail = true;
      Assert.AreEqual("saved", root.Data, "data is 'saved'");
      Assert.AreEqual(true, root.Fail, "fail is true");
      Assert.AreEqual(true, root.IsDirty, "isdirty is true");
      Assert.AreEqual(true, root.IsValid, "isvalid is true");
      Assert.AreEqual(true, root.IsNew, "isnew is true");

      TestResults.Reinitialise();
      RollbackRoot tmp = (RollbackRoot)(root.Clone());
      try
      {
        root.ApplyEdit();
        root = (RollbackRoot)(root.Save());
        Assert.Fail("exception didn't occur");
      }
      catch
      {
        root = tmp;
        root.CancelEdit();
      }

      Assert.IsNotNull(root, "obj is not null");
      Assert.AreEqual("Inserted", TestResults.GetResult("Root"), "obj was inserted");
      Assert.AreEqual("<new>", root.Data, "data is '<new>'");
      Assert.AreEqual(true, root.IsNew, "isnew is true");
      Assert.AreEqual(false, root.IsDeleted, "isdeleted is false");
      Assert.AreEqual(true, root.IsDirty, "isdirty is true");
    }

    [TestMethod]
    public void EditParentEntity()
    {
      IDataPortal<DataBinding.ParentEntity> dataPortal = _testDIContext.CreateDataPortal<DataBinding.ParentEntity>();

      Csla.Test.DataBinding.ParentEntity p = Csla.Test.DataBinding.ParentEntity.NewParentEntity(dataPortal);
      p.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(p_PropertyChanged);

      p.BeginEdit();
      p.Data = "something";
      p.BeginEdit();
      p.CancelEdit();
      p.CancelEdit();

      Assert.AreEqual(string.Empty, p.Data);

      p.BeginEdit();
      p.BeginEdit();
      p.Data = "data";
      p.ApplyEdit();
      p.CancelEdit();

      Assert.AreEqual(string.Empty, p.Data);

      p.Data = "data";
      p.BeginEdit();
      p.Data += " more data";
      p.ApplyEdit();
      p.CancelEdit();

      Assert.AreEqual("data more data", p.Data);
    }

    public void p_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      Console.WriteLine(e.PropertyName + " has been changed");
    }
  }
}