﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Gwin.FieldsTraitements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Gwin.FieldsTraitements.Params;
using GenericWinForm.Demo.Entities;
using System.Reflection;
using System.Drawing;
using App.Shared.AttributesManager;
using App.Gwin.Attributes;
using System.Windows.Forms;
using App.Gwin.Application.BAL;
using App.Gwin.Entities;
using App.Gwin.Fields;
using GenericWinForm.Demo.BAL;
using App.Gwin.Fields.Traitements.Params;
using App.Gwin.EntityManagement;

namespace App.Gwin.FieldsTraitements.Tests
{
    [TestClass()]
    public class FieldTraitementTests
    {
        ConfigEntity configEntity = null;
        EntityMiniConfig Entity = null;
        IBaseBLO entityMiniConfigBLO = null;
        [TestInitialize]
        public void GwinAppStart()
        {
            GwinApp.Start(typeof(ModelContext), typeof(BaseBLO<>), new Application.Presentation.MainForm.FormApplication(), null);
            configEntity = ConfigEntity.CreateConfigEntity(typeof(EntityMiniConfig));
            Entity = new EntityMiniConfig();
            entityMiniConfigBLO = BaseEntityBLO<BaseEntity>.CreateBLO_Instance(typeof(EntityMiniConfig), typeof(BaseBLO<>));

        }

        [TestMethod()]
        public void CreateField_In_EntryFormTest()
        {
            BaseEntryForm baseEntryForm = new BaseEntryForm(entityMiniConfigBLO, Entity, new Dictionary<string, object>(), true);
            baseEntryForm.BaseEntryForm_Load(baseEntryForm, null);
            // Load EntityMiniConfigBLO dynamicly
            Assert.AreEqual(typeof(EntityMiniConfigBLO), entityMiniConfigBLO.GetType());
        }

        [TestMethod()]
        public void WriteEntity_To_EntryFormTest()
        {
            BaseEntryForm baseEntryForm = new BaseEntryForm(entityMiniConfigBLO, Entity, new Dictionary<string, object>(), true);
            baseEntryForm.BaseEntryForm_Load(baseEntryForm, null);
            baseEntryForm.WriteEntityToField();
        }

        [TestMethod()]
        public void CreateField_In_Filter()
        {
            BaseFilterControl filter = new BaseFilterControl(entityMiniConfigBLO);
        }

        [TestMethod()]
        public void GetFieldValue_From_FilterTest()
        {
            BaseFilterControl filter = new BaseFilterControl(entityMiniConfigBLO);
            filter.GetFilterValues();
        }
    }
}