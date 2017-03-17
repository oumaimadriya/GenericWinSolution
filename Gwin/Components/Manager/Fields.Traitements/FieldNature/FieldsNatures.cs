﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Gwin.FieldsTraitements.Enumerations
{
    /// <summary>
    /// Field Type suported by Gwin Application
    /// 
    /// </summary>
    public enum FieldsNatures
    {
        // Must add Test into  DetermineFieldNature() methode in ConfigProperty

        Default,
        String,
        StringWithDataSource,
        LocalizedString,
        DateTime,
        Enumeration,
        Int32,
        Int64,
        ManyToMany_Creation,
        ManyToMany_Selection,
        ManyToOne
    }
}