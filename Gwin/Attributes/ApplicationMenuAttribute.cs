﻿using System;

namespace App.Gwin.Attributes
{
    /// <summary>
    /// Displaying the object in the application menu
    /// </summary>
    public class MenuAttribute : BaseAttribute
    {
        /// <summary>
        /// Parent MenuItem Name
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// The item name
        /// </summary>
        public string Title { get; set; }
    }
}