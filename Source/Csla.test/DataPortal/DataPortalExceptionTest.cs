﻿using System;
using Csla;
using Csla.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Csla.Test.DataPortal
{
  [TestClass]
  public class DataPortalExceptionTests
  {
    private static TestDIContext _testDIContext;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
      _testDIContext = TestDIContextFactory.CreateDefaultContext();
    }

    [TestMethod]
    [TestCategory("SkipWhenLiveUnitTesting")]
    public void ChildInnerExceptionFlowsFromDataPortal()
    {
      IDataPortal<EditableRoot1> dataPortal = _testDIContext.CreateDataPortal<EditableRoot1>();

      try
      {
        var bo = dataPortal.Create();

        bo.Save();
      }
      catch (DataPortalException e)
      {
        Assert.IsInstanceOfType(e.BusinessException, typeof(InvalidOperationException));
      }
    }
  }

  [Serializable]
  public class EditableRoot1 : BusinessBase<EditableRoot1>
  {
    public static readonly PropertyInfo<EditableChild1> ChildProperty =
        RegisterProperty<EditableChild1>(c => c.Child);

    public EditableChild1 Child
    {
      get { return GetProperty(ChildProperty); }
      private set { LoadProperty(ChildProperty, value); }
    }

    [RunLocal]
    [Create]
	protected void DataPortal_Create([Inject] IChildDataPortal<EditableChild1> childDataPortal)
    {
      using (BypassPropertyChecks)
      {
        LoadProperty(ChildProperty, childDataPortal.CreateChild());
      }
      BusinessRules.CheckRules();
    }

    [Transactional(TransactionalTypes.TransactionScope)]
    [Insert]
    protected void DataPortal_Insert()
    {
      using (BypassPropertyChecks)
      {
        FieldManager.UpdateChildren(this);
      }
    }
  }

  [Serializable]
  public class EditableChild1 : BusinessBase<EditableChild1>
  {
    #region Data Access

    [CreateChild]
    protected override void Child_Create()
    {
      using (BypassPropertyChecks)
      {
      }
      base.Child_Create();
    }

    [InsertChild]
    private void Child_Insert(object parent)
    {
      using (BypassPropertyChecks)
      {
        throw new InvalidOperationException("Insert not allowed");
      }
    }

    #endregion
  }
}