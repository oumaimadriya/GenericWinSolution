﻿using App.Gwin.Attributes;
using App.Gwin.Entities.Authentication;
using App.Gwin.Entities.Autorizations;
using App.Gwin.Entities.MultiLanguage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Gwin.Entities.Application
{
    [DisplayEntity(Localizable =true, DisplayMember = nameof(Code),isMaleName =true)]
    [ManagementForm(FormTitle ="MenuManager")]
    [Menu(Group ="Admin")]
    public class MenuItemApplication : BaseEntity
    {
        // Name & Description
        [DisplayProperty(isInGlossary =true)]
        [EntryForm(Ordre = 2)]
        [Filter]
        [DataGrid]
        public string Code { set; get; }

        [DisplayProperty(isInGlossary = true)]
        [EntryForm(Ordre = 3, MultiLine = true)]
        [DataGrid]
        public LocalizedString Description { set; get; }

        [DisplayProperty(isInGlossary = true)]
        [EntryForm(Ordre = 2)]
        [Filter]
        [DataGrid]
        public LocalizedString Title { set; get; }

        [DisplayProperty(isInGlossary = true)]
        [EntryForm(Ordre = 2)]
        [DataGrid]
        [Relationship(Relation = RelationshipAttribute.Relations.ManyToMany_Selection,EditMode= RelationshipAttribute.EditingModes.Selection_With_Check_Box)]
        public virtual List<Role> Roles { set; get; }
    }
}
